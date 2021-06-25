using SupportApi.Controllers;
using SupportApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using SupportApi.Collaboration.Models;
using SupportApi.Connection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SupportApi.Collaboration
{

    [ProtoContract]
    public class SharedDocument
    {

        private Dictionary<string, SharedAccessMode> _sharedUsers;
        private Dictionary<string, byte[]> _attachments = new Dictionary<string, byte[]>();

        #region ** constructors

        /// <summary>
        /// シリアライザで使用するコンストラクタ
        /// </summary>
        public SharedDocument()
        {

        }

        /// <summary>
        /// 新しいドキュメントが共有されたときに使用されるコンストラクタ
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="ownerUserName"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        public SharedDocument(string documentId, string ownerUserName, string fileName, Stream stream, ModificationsState modificationsState = null)
        {
            DocumentId = documentId;
            OwnerUserName = ownerUserName;
            FileName = fileName;
            Data = ReadAllBytes(stream);
            if (modificationsState == null) 
                modificationsState = new ModificationsState();
            ModificationsState = modificationsState;
            ApplyTouchedAnnotations();
        }

        #endregion


        #region ** properties

        [ProtoMember(1)]
        public string DocumentId { get; }

        [ProtoMember(2)]
        public string OwnerUserName { get; }

        [ProtoMember(3)]
        public string FileName { get; private set; }

        /// <summary>
        /// ドキュメントにアクセスできるユーザ
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<string, SharedAccessMode> SharedUsers
        {
            get
            {
                if (_sharedUsers == null)
                {
                    _sharedUsers = new Dictionary<string, SharedAccessMode>();
                }
                return _sharedUsers;
            }
            set
            {
                _sharedUsers = value;
            }
        }

        [ProtoMember(5)]
        public DateTime LastAccess { get; set; } = DateTime.Now;

        // データは個別に保存
        public byte[] Data;

        // ModificationsStateは個別に保存
        public ModificationsState ModificationsState { 
            get {
                return _modificationsState;
            }
            set
            {
                _modificationsState = value;
            }
        }

        /// <summary>
        /// ファイルの添付
        /// </summary>
        [ProtoMember(6)]
        public Dictionary<string, byte[]> Attachments
        {
            get
            {
                if (_attachments == null)
                {
                    _attachments = new Dictionary<string, byte[]>();
                }
                return _attachments;
            }
            set
            {
                _attachments = value;
            }
        }

        // 未定：UndoListを保存する？
        public List<string> UndoList = new List<string>();
        public int UndoIndex = 0;
        private ModificationsState _modificationsState;

        #endregion

        public UserSharedDocument GetUserSharedDocument(UserAccess userAccess)
        {            
            var enumerator = GcPdfViewerController.DocumentLoaders.GetEnumerator();
            if (string.IsNullOrEmpty(FileName))
            {
                string fileName = "Unknown";
                while (enumerator.MoveNext())
                {
                    var docLoader = enumerator.Current.Value;
                    if (docLoader.DocumentId.Equals(DocumentId))
                    {
                        if (docLoader.Info != null)
                        {
                            fileName = docLoader.Info.documentOptions.fileName;
                        }
                    }
                    FileName = fileName;
                }
            }
            return new UserSharedDocument(DocumentId, userAccess, OwnerUserName, FileName);
        }

        public async Task ChangeUserAccessModeAsync(string userName, string callerUserName, SharedAccessMode newAccessMode)
        {
            if (OwnerUserName.Equals(userName) && newAccessMode != SharedAccessMode.ViewAndEdit)
            {

                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.YouCannotRestrictAccessModeForOwner);
            }
            else
            {
                var callerUserAccessMode = GetUserAccess(callerUserName);
                if (callerUserAccessMode.AccessMode != SharedAccessMode.ViewAndEdit)
                {
                    throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.YouCannotRestrictAccessMode);
                }
                lock (SharedUsers)
                {
                    SharedUsers[userName] = newAccessMode;
                }
                await SharedDocumentsStorage.Instance().OnDocumentAccessModeChanged(this);
            }
        }

        public async Task RemoveUserAccessModeAsync(string userName, string callerUser)
        {
            if (OwnerUserName.Equals(userName) && GetUserAccessList().Count > 1)
            {
                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.YouCannotUnshareForOwnerRemoveOthersFirst);
            }
            var callerUserAccessMode = GetUserAccess(callerUser);
            if (callerUserAccessMode.AccessMode != SharedAccessMode.ViewAndEdit)
            {
                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.YouCannotRemoveUserAccess);
            }
            try
            {
                lock (SharedUsers)
                {
                    if (SharedUsers.ContainsKey(userName))
                        SharedUsers.Remove(userName);
                }
                await SharedDocumentsStorage.Instance().OnDocumentAccessModeChanged(this);
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.CannotRemoveUserAccessModeFormat, ex.Message));
            }
        }

        public UserAccess GetUserAccess(string userName)
        {
            lock (SharedUsers)
            {
                var enumerator = SharedUsers.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key.Equals(userName))
                        return new UserAccess(userName, enumerator.Current.Value);
                }
                if (userName.Equals(OwnerUserName))
                    return new UserAccess(userName, SharedAccessMode.ViewAndEdit);
                return new UserAccess(userName, SharedAccessMode.AccessDenied);
            }
        }

        public List<UserAccess> GetUserAccessList()
        {
            lock (SharedUsers)
            {
                var list = new List<UserAccess>();
                var enumerator = SharedUsers.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    list.Add(new UserAccess() { UserName = enumerator.Current.Key, AccessMode = enumerator.Current.Value });
                }
                return list;
            }
        }

        #region ** Modifications
        

        public async Task<ModificationsState> OnModificationReceived(string callerUserName, ModificationType modificationType, JObject dictData)
        {
            
            var isReset = false;
            switch (modificationType)
            {
                case ModificationType.Structure:
                    OnStructureChanged((dictData.GetValue("data") as JObject).ToObject<StructureChanges>(), callerUserName);
                    SaveUndoState();
                    break;
                case ModificationType.AddAnnotation:
                    OnAddAnnotation((dictData.GetValue("data") as JObject).ToObject<AnnotationInfo>(), callerUserName);
                    SaveUndoState();
                    break;
                case ModificationType.UpdateAnnotation:
                    OnUpdateAnnotation((dictData.GetValue("data") as JObject).ToObject<AnnotationInfo>(), callerUserName);
                    SaveUndoState();
                    break;
                case ModificationType.RemoveAnnotation:
                    OnRemoveAnnotation((dictData.GetValue("data") as JObject).ToObject<RemovedAnnotationInfo>(), callerUserName);
                    SaveUndoState();
                    break;
                case ModificationType.Undo:
                    OnUndo();
                    break;
                case ModificationType.Redo:
                    OnRedo();
                    break;
                case ModificationType.ResetUndo:
                    //OnResetUndo();
                    isReset = true;
                    break;
                case ModificationType.Reset:
                    //OnReset();
                    isReset = true;
                    break;

            }
            if (ModificationsState != null)
            {
                ModificationsState.lastChangeType = modificationType;
            }
            if (!isReset)
            {
                await SharedDocumentsStorage.Instance().OnDocumentChanged(this);
            }
            return ModificationsState;
        }



        private void OnUndo()
        {
            if (UndoIndex <= 0)
            {
                ModificationsState = new ModificationsState();
                UndoIndex = 0;
            }
            else
            {
                UndoIndex--;
                ModificationsState = RestoreFromUndoState(UndoIndex);
            }
        }

        private void OnRedo()
        {
            if (UndoIndex < UndoList.Count - 1)
            {                
                ModificationsState = RestoreFromUndoState(UndoIndex);
                UndoIndex++;
            }

        }

        private void OnResetUndo()
        {
            UndoIndex = 0;
            UndoList = new List<string>();
        }

        private void OnReset()
        {
            ModificationsState = new ModificationsState();
            OnResetUndo();
        }

        ModificationsState RestoreFromUndoState(int undoIndex)
        {
            try
            {
                string undoState = UndoList[undoIndex];
                ModificationsState modificationsState = JsonConvert.DeserializeObject<ModificationsState>(undoState);
                modificationsState.undoIndex = undoIndex;
                modificationsState.undoCount = UndoList.Count;
                return modificationsState;
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UnableToLoadUndoStateFormat, ex.Message));
            }
        }

        void SaveUndoState()
        {
            try
            {
                lock (UndoList)
                {                    
                    string undoState = JsonConvert.SerializeObject(ModificationsState);
                    UndoList.Insert(UndoIndex, undoState);
                    UndoIndex++;
                    ModificationsState.undoIndex = UndoIndex;
                    ModificationsState.undoCount = UndoList.Count;
                }
            }
            catch (Exception ex)
            {
                new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UnableToSaveUndoStateFormat, ex.Message));
            }
        }

        private void OnStructureChanged(StructureChanges structure, string callerUserName)
        {

            try
            {


                ModificationsState.structure = structure;
                ModificationsState.version++;
                ApplyTouchedAnnotations();
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.CannotChangeDocumentStructureFormat, ex.Message));
            }
        }

        private bool ApplyTouchedAnnotations()
        {
            bool isChanged = false;
            if (ModificationsState != null)
            {
                var structure = ModificationsState.structure;
                if (structure != null)
                {
                    var touchedAnnotations = structure.touchedAnnotations;
                    if (touchedAnnotations != null)
                    {
                        foreach (var touchedAnnot in touchedAnnotations)
                        {
                            var annot = ModificationsState.newAnnotations.Where(a => a.annotation["id"] == touchedAnnot.annotationId).FirstOrDefault();
                            if(annot == null)
                            {
                                annot = ModificationsState.updatedAnnotations.Where(a => a.annotation["id"] == touchedAnnot.annotationId).FirstOrDefault();
                            }
#if DEBUG
                            Debug.Assert(annot != null, "Cannot find touched annotation.");
#endif

                            if (annot != null)
                            {
                                if (annot.pageIndex != touchedAnnot.pageIndex)
                                {
                                    annot.pageIndex = touchedAnnot.pageIndex;
                                    isChanged = true;
                                }
                                if (touchedAnnot.pageIndex == -9/*removed flag*/)
                                {
                                    ModificationsState.updatedAnnotations.Remove(annot);
                                    if (!ModificationsState.newAnnotations.Remove(annot))
                                    {
                                        ModificationsState.removedAnnotations.Add(new RemovedAnnotationInfo() { annotationId = annot.annotation["id"], pageIndex = annot.pageIndex });
                                    }
                                    isChanged = true;
                                }
                            }
                            structure.touchedAnnotations = null;
                        }
                    }
                }
                
            }
            return isChanged;
        }

        private void OnAddAnnotation(AnnotationInfo annotationInfo, string callerUserName)
        {
            try
            {
                Dictionary<string, dynamic> annotation = annotationInfo.annotation;
                MarkAnnotationChanged(annotation, callerUserName);
                ModificationsState.newAnnotations.Add(annotationInfo);
                ModificationsState.version++;
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UnableToAddAnnotationFormat, ex.Message));
            }
        }

        private void OnUpdateAnnotation(AnnotationInfo annotationInfo, string callerUserName)
        {
            try
            {
                Dictionary<string, dynamic> annotation = annotationInfo.annotation;
                MarkAnnotationChanged(annotation, callerUserName);
                string annotationId = GetAnnotationId(annotation);
                var newAnnotation = ModificationsState.newAnnotations.Where(a => a.annotation["id"] == annotationId).FirstOrDefault();
                if (newAnnotation != null)
                {
                    var index = ModificationsState.newAnnotations.IndexOf(newAnnotation);
                    ModificationsState.newAnnotations.RemoveAt(index);
                    index = Math.Min(index, ModificationsState.newAnnotations.Count);
                    ModificationsState.newAnnotations.Insert(index, annotationInfo);
                }
                else
                {
                    var updatedAnnotation = ModificationsState.updatedAnnotations.Where(a => a.annotation["id"] == annotationId).FirstOrDefault();
                    if(updatedAnnotation != null)
                    {
                        var index = ModificationsState.updatedAnnotations.IndexOf(updatedAnnotation);
                        ModificationsState.updatedAnnotations.RemoveAt(index);
                        index = Math.Min(index, ModificationsState.updatedAnnotations.Count);
                        ModificationsState.updatedAnnotations.Insert(index, annotationInfo);
                    }
                    else
                    {
                        ModificationsState.updatedAnnotations.Add(annotationInfo);
                    }
                    
                }
                ModificationsState.version++;
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UnableToUpdateAnnotationFormat, ex.Message));
            }
        }

        private void OnRemoveAnnotation(RemovedAnnotationInfo removedAnnotationInfo, string callerUserName)
        {
            try
            {
                var newAnnotation = ModificationsState.newAnnotations.Where(a => a.annotation["id"] == removedAnnotationInfo.annotationId).FirstOrDefault();
                if (newAnnotation != null)
                {
                    ModificationsState.newAnnotations.Remove(newAnnotation);
                }
                else
                {
                    var updatedAnnotation = ModificationsState.updatedAnnotations.Where(a => a.annotation["id"] == removedAnnotationInfo.annotationId).FirstOrDefault();
                    if (updatedAnnotation != null)
                    {
                        ModificationsState.updatedAnnotations.Remove(updatedAnnotation);
                    }
                    ModificationsState.removedAnnotations.Add(removedAnnotationInfo);
                }
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(Controllers.GcPdfViewerController.Settings.ErrorMessages.UnableToRemoveAnnotationFormat, ex.Message));
            }
        }
        
        public void SetAttachedFile(string fileId, byte[] resultBytes)
        {
            lock (_attachments)
            {
                if (_attachments.ContainsKey(fileId))
                {
                    _attachments.Remove(fileId);
                }
                _attachments.Add(fileId, resultBytes);
            }
            //   await SharedDocumentsStorage.Instance().OnDocumentChanged(this);
        }

        public byte[] GetAttachedFile(string fileId)
        {
            lock (_attachments)
            {
                if (_attachments.ContainsKey(fileId))
                {
                    if(_attachments.TryGetValue(fileId, out var file))
                    {
                        return file;
                    }
                }
            }
            return null;

        }

        private void MarkAnnotationChanged(Dictionary<string, dynamic> annotation, string callerUserName)
        {
            Dictionary<string, long> changes = new Dictionary<string, long>();
            changes.Add(callerUserName, DateTime.Now.ToFileTimeUtc());
            annotation["sharedChanges"] = changes;
            if(!annotation.TryGetValue("id", out _))
                annotation["id"] = string.Empty;
        }

        /// <summary>
        /// 注釈ID。新しい注釈の場合、一時的なIDである可能性があります
        /// </summary>
        public string GetAnnotationId(Dictionary<string, dynamic> annotationData)
        {
            if (!annotationData.ContainsKey("id"))
                return string.Empty;
            return annotationData["id"];
        }

        #endregion

        public static byte[] ReadAllBytes(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }



        public async Task<StartSharedModeResponse> StartSharedMode(ClientConnection clientConnection)
        {            
            await LoadDataFromStorage();
            string clientId = clientConnection.ClientId;
            var loader = GcPdfViewerController.GetDocumentLoader(clientId);
            if (loader == null)
            {
                string userName = clientConnection.UserName;
                var userAccess = GetUserAccess(userName);
                GcPdfViewerController.CreateDocumentLoader(clientId, userAccess.AccessMode, new MemoryStream(Data));
            }
            return new StartSharedModeResponse(ModificationsState, GetUserAccess(clientConnection.UserName), GetUserAccessList());
        }

        public StopSharedModeResponse StopSharedMode(ClientConnection clientConnection)
        {
            string clientId = clientConnection.ClientId;
            var loader = GcPdfViewerController.GetDocumentLoader(clientId);
            if (loader != null)
            {
                loader.DocumentId = string.Empty;
            }
            return new StopSharedModeResponse();
        }

        public async Task LoadDataFromStorage(bool force = false)
        {
            var storage = SharedDocumentsStorage.Instance();
            if (force || Data == null || ModificationsState == null)
            {
                Data = await storage.LoadDocumentData(DocumentId);
                ModificationsState = await storage.LoadDocumentModifications(DocumentId);
            }
        }

        public void DisposeCache()
        {
            Data = null;
            ModificationsState = null;
        }

    }

    public enum SharedAccessMode
    {
        Loading = -1,
        AccessDenied = 0,
        ViewOnly = 1,
        ViewAndEdit = 2
    }
}

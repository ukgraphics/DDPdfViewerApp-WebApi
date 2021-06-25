using ProtoBuf;
using SupportApi.Collaboration.Models;
using SupportApi.Collaboration.Storages;
using SupportApi.Connection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportApi.Collaboration
{
    public class SharedDocumentsStorage
    {

        #region ** fields

        private static SharedDocumentsStorage _instance;
        /// <summary>
        /// 共有ドキュメント（キー：document id, value）
        /// </summary>
        private ConcurrentDictionary<string, SharedDocument> _sharedDocuments;

        private ICollaborationStorage _collaborationStorage;

        /// <summary>
        /// ドキュメントのライフタイム期間
        /// </summary>
        public double LifeTimeHours { get; set; } = 8;

        public void SetStorage(ICollaborationStorage collaborationStorage)
        {
            _ = InitializeCllaborationStorageAsync(collaborationStorage);
        }

        #endregion

        #region ** constructor

        private SharedDocumentsStorage()
        {
            _sharedDocuments = new ConcurrentDictionary<string, SharedDocument>();
        }

        #endregion

        #region ** singleton

        public static SharedDocumentsStorage Instance()
        {
            if (_instance == null) _instance = new SharedDocumentsStorage();
            return _instance;
        }

        #endregion

        #region ** methods

        public async Task<SharedDocument> ShareDocument(string documentId, string callerUserName, string fileName, Stream stream, ModificationsState modificationsState = null)
        {
            SharedDocument sharedDocument = Get(documentId);
            if (sharedDocument != null)
            {
                throw new ClientConnectionException(string.Format(Controllers.GcPdfViewerController.Settings.ErrorMessages.DocumentAlreadySharedFormat, documentId));
            }

            sharedDocument = new SharedDocument(documentId, callerUserName, fileName, stream, modificationsState);
            try
            {

                if (!_sharedDocuments.TryAdd(documentId, sharedDocument))
                {
                    throw new ClientConnectionException(Controllers.GcPdfViewerController.Settings.ErrorMessages.CannotAddSharedDocumentToCollectionUnknown);
                }
            }
            catch (Exception ex)
            {
                throw new ClientConnectionException(string.Format(Controllers.GcPdfViewerController.Settings.ErrorMessages.CannotAddSharedDocumentToCollectionFormat, ex.Message));
            }

            await sharedDocument.ChangeUserAccessModeAsync(callerUserName, callerUserName, SharedAccessMode.ViewAndEdit);
            await Instance().OnDocumentShared(sharedDocument);
            return sharedDocument;
        }

        public async Task UnshareDocumentAsync(string documentId, string userName, string callerUserName)
        {
            var sharedDocument = Get(documentId);
            await sharedDocument.RemoveUserAccessModeAsync(userName, callerUserName);
            if (sharedDocument.GetUserAccessList().Count == 0)
            {
                if (_sharedDocuments.ContainsKey(documentId))
                {
                    _ = _sharedDocuments.TryRemove(documentId, out _);
                }
            }
        }

        public async Task RemoveDocument(string documentId)
        {
            if (_sharedDocuments.ContainsKey(documentId))
            {
                _ = _sharedDocuments.TryRemove(documentId, out _);
                _ = SaveDocumentData(documentId, null);
                await OnDocumentListChanged();
            }
        }

        /// <summary>
        /// 共有ドキュメントをドキュメントIDで返します
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public SharedDocument Get(string documentId)
        {
            if(_sharedDocuments.TryGetValue(documentId, out var doc))
            {
                return doc;
            }
            return null;
        }

        public List<UserAccess> GetUserAccessList(string documentId)
        {
            var document = Get(documentId);
            if(document != null)
            {
                return document.GetUserAccessList();
            }
            return new List<UserAccess>();
        }

        internal List<UserSharedDocument> GetSharedDocumentsList(string userName)
        {
            var list = new List<UserSharedDocument>();
            if (string.IsNullOrEmpty(userName))
                return list;
            var enumerator = _sharedDocuments.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var sharedDocument = enumerator.Current.Value;

                var userAccess = sharedDocument.GetUserAccess(userName);
                if(userAccess != null && userAccess.AccessMode != SharedAccessMode.AccessDenied)
                {
                    list.Add(sharedDocument.GetUserSharedDocument(userAccess));
                }
            }
            return list;
        }

        #endregion

        #region ** Collaboration storage CRUD operations

        public async Task OnDocumentListChanged()
        {
            await SaveDocumentList();
        }

        public async Task OnDocumentShared(SharedDocument sharedDocument)
        {
            sharedDocument.LastAccess = DateTime.Now;
            await SaveDocumentList();
            await SaveDocumentData(sharedDocument.DocumentId, sharedDocument.Data);
            await SaveDocumentModifications(sharedDocument.DocumentId, sharedDocument.ModificationsState);
        }

        public async Task OnDocumentAccessModeChanged(SharedDocument sharedDocument)
        {
            sharedDocument.LastAccess = DateTime.Now;
            await SaveDocumentList();
        }

        public async Task OnDocumentChanged(SharedDocument sharedDocument)
        {
            if (sharedDocument.Data != null)
            {
                sharedDocument.LastAccess = DateTime.Now;
                await SaveDocumentModifications(sharedDocument.DocumentId, sharedDocument.ModificationsState);
            }
            await CleanupOldDocuments();
        }

        public async Task SaveDocumentList()
        {
            if (_collaborationStorage != null)
            {
                KeyValuePair<string, SharedDocument>[] sharedDocumentsArray = _sharedDocuments.ToArray();
                var memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, sharedDocumentsArray);
                await _collaborationStorage.WriteData("documents-list.bin", memoryStream.ToArray());
            }
        }

        public async Task LoadDocumentList()
        {
            if (_collaborationStorage != null)
            {
                var bytesData = await _collaborationStorage.ReadData("documents-list.bin");
                if (bytesData != null)
                {
                    KeyValuePair<string, SharedDocument>[] sharedDocumentsArray = Serializer.Deserialize<KeyValuePair<string, SharedDocument>[]>(new MemoryStream(bytesData));
                    _sharedDocuments.Clear();
                    if (sharedDocumentsArray != null)
                    {
                        foreach (var pair in sharedDocumentsArray)
                            _sharedDocuments.TryAdd(pair.Key, pair.Value);
                    }
                }
            }
        }

        public async Task<byte[]> LoadDocumentData(string documentId)
        {
            if (_collaborationStorage != null)
            {
                return await _collaborationStorage.ReadData($"doc_{documentId}.pdf");
            }
            else
            {
                return null;
            }
        }


        public async Task SaveDocumentData(string documentId, byte[] data)
        {
            if (_collaborationStorage != null)
            {
                await _collaborationStorage.WriteData($"doc_{documentId}.pdf", data);
            }

        }

        public async Task<ModificationsState> LoadDocumentModifications(string documentId)
        {
            ModificationsState modificationsState = null;
            if (_collaborationStorage != null)
            {
                byte[] bytesData = await _collaborationStorage.ReadData($"doc_{documentId}.mod");
                modificationsState = Serializer.Deserialize<ModificationsState>(new MemoryStream(bytesData));
            }
            if (modificationsState == null)
                return new ModificationsState();
            return modificationsState;
        }

        public async Task SaveDocumentModifications(string documentId, ModificationsState modificationsState)
        {
            if (_collaborationStorage != null)
            {
                var memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, modificationsState);
                await _collaborationStorage.WriteData($"doc_{documentId}.mod", memoryStream.ToArray());
            }
        }



        private async Task InitializeCllaborationStorageAsync(ICollaborationStorage collaborationStorage)
        {
            _collaborationStorage = collaborationStorage;
            if (_collaborationStorage != null)
            {
                await LoadDocumentList();
            }
            else
            {

            }
        }

        public async Task CleanupOldDocuments()
        {
            double lifeTimeHours = LifeTimeHours;

            KeyValuePair<string, SharedDocument>[] sharedDocumentsArray = _sharedDocuments.ToArray();
            foreach (var pair in sharedDocumentsArray)
            {
                var doc = pair.Value;
                
                var hasDocumentConnections = ClientConnection.GetByDocumentId(doc.DocumentId).Count > 0;
                if (hasDocumentConnections)
                {
                    doc.LastAccess = DateTime.Now;
                    continue;
                }
                if (_collaborationStorage != null)
                {
                    // ドキュメントのデータが保存されている場合にのみ
                    // 未使用のドキュメントのメモリをクリーンアップ
                    doc.DisposeCache();
                }
                if (lifeTimeHours > 0)
                {                    
                    var compareDate = doc.LastAccess.AddHours(lifeTimeHours);
                    if (compareDate < DateTime.Now)
                    {
                        // ストレージからドキュメントを削除
                        await RemoveDocument(doc.DocumentId);
                    }
                }
            }

        }

        #endregion

    }
}

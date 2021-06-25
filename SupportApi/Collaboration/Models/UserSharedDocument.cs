using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SupportApi.Collaboration.Models
{
    /// <summary>
    /// 共有ドキュメントに関する情報。このモデルはクライアントビューワに渡されます
    /// </summary>
    [Serializable]
    public class UserSharedDocument : ISerializable, IComparable
    {

        #region ** constructor

        public UserSharedDocument(string documentId, UserAccess userAccess, string ownerUserName, string fileName)
        {
            DocumentId = documentId;
            UserName = userAccess.UserName;
            AccessMode = userAccess.AccessMode;
            OwnerUserName = ownerUserName;
            FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName;
        }

        #endregion

        #region ** properties

        /// <summary>
        /// ドキュメントへのアクセスを要求したユーザ（<see cref="UserName"/>）のアクセスモード 
        /// </summary>
        public SharedAccessMode AccessMode { get; set; }

        /// <summary>
        /// 共有ドキュメントのID
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// 共有ドキュメントのファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 最初にドキュメントを共有したユーザ
        /// </summary>
        public string OwnerUserName { get; set; }

        /// <summary>
        /// ドキュメントへのアクセスを要求したユーザ
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region ** ISerializable interface implementation

        protected UserSharedDocument(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");

            int intAccessMode = serializationInfo.GetInt32("accessMode");
            AccessMode = (SharedAccessMode)intAccessMode;
            DocumentId = serializationInfo.GetString("documentId");
            FileName = serializationInfo.GetString("fileName");
            OwnerUserName = serializationInfo.GetString("ownerUserName");
            UserName = serializationInfo.GetString("userName");
        }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");            
            serializationInfo.AddValue("accessMode", (int)AccessMode);
            serializationInfo.AddValue("documentId", DocumentId);
            serializationInfo.AddValue("fileName", FileName);
            serializationInfo.AddValue("ownerUserName", OwnerUserName);
            serializationInfo.AddValue("userName", UserName);
        }

        #endregion

        #region ** IComparable interface implementation

        public int CompareTo(object obj)
        {
            var userSharedDocument = obj as UserSharedDocument;
            if (userSharedDocument == null)
                return -1;
            var result = DocumentId.CompareTo(userSharedDocument.DocumentId);
            if (result == 0)
                result = OwnerUserName.CompareTo(userSharedDocument.OwnerUserName);
            if (result == 0)
                result = AccessMode.CompareTo(userSharedDocument.AccessMode);
            if (result == 0)
                result = UserName.CompareTo(userSharedDocument.UserName);
            if (result == 0)
                result = FileName.CompareTo(userSharedDocument.FileName);
            return result;

        }

        #endregion
    }
}

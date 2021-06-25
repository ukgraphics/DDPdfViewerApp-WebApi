using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace SupportApi.Collaboration.Models
{

    /// <summary>
    /// <see cref="UserName"/> プロパティによって提供される、ユーザのアクセスモードに関する情報
    /// </summary>
    [Serializable]
    public class UserAccess : ISerializable, IComparable
    {

        #region ** constructor

        /// <summary>
        /// デフォルトのUserAccessクラスのコンストラクタ
        /// </summary>
        public UserAccess()
        {
        }

        /// <summary>
        /// ユーザ名とアクセスモードを引数とするコンストラクタ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessMode"></param>
        public UserAccess(string userName, SharedAccessMode accessMode)
        {
            UserName = userName;
            AccessMode = accessMode;
        }

        #endregion

        #region ** properties

        /// <summary>
        /// ユーザ名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 共有ドキュメントのアクセスタイプ
        /// </summary>
        public SharedAccessMode AccessMode { get; set; }

        #endregion

        #region ** ISerializable interface implementation

        protected UserAccess(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");
            UserName = serializationInfo.GetString("userName");
            int intAccessMode = serializationInfo.GetInt32("accessMode");
            AccessMode = (SharedAccessMode)intAccessMode;
        }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo == null)
                throw new ArgumentNullException("serializationInfo");

            serializationInfo.AddValue("userName", UserName);
            serializationInfo.AddValue("accessMode", (int)AccessMode);
        }

        #endregion

        #region ** IComparable interface implementation

        public int CompareTo(object obj)
        {
            var userAccess = obj as UserAccess;
            if (userAccess == null)
                return -1;
            var result = UserName.CompareTo(userAccess.UserName);
            if(result == 0)
            {
                return AccessMode.CompareTo(userAccess.AccessMode);
            }
            else
            {
                return result;
            }
        }

        #endregion
    }
 
}

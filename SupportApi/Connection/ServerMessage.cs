using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Connection
{

    /// <summary>
    /// サーバーメッセージ
    /// </summary>
    public class ServerMessage : Message
    {

        public ServerMessage() : base()
        {

        }

        public ServerMessage(string correlationId) : base(correlationId)
        {

        }

        /// <summary>
        /// メッセージタイプ
        /// </summary>
        public int /* サーバのメッセージタイプ */ type;
        
        public static readonly string EMPTY_CORRELATION_ID = "no-id";
    }

    /// <summary>
    /// サーバのメッセージタイプ
    /// </summary>
    public enum ServerMessageType
    {
        /// <summary>
        /// ユーザーに情報メッセージを表示
        /// </summary>
        Information = 10,

        /// <summary>
        /// ユーザーにエラーメッセージを表示
        /// </summary>
        Error = 11,

        /// <summary>
        /// 共有ドキュメントの変更をプッシュ
        /// </summary>
        Modifications = 20,

        /// <summary>
        /// このメッセージは、変更時に共有ドキュメントの一覧を送信するために使用されます
        /// </summary>
        SharedDocumentsListChanged = 45,

        // リクエストメッセージへの返信

        /// <summary>
        /// UserAccessList のレスポンス
        /// </summary>
        UserAccessListResponse = 100,

        /// <summary>
        /// SharedDocumentsList のレスポンス
        /// </summary>
        SharedDocumentsListResponse = 101,

        /// <summary>
        /// AllUsersList のレスポンス
        /// </summary>
        AllUsersListResponse = 102,

        /// <summary>
        /// OpenSharedDocument のレスポンス
        /// </summary>
        OpenSharedDocumentResponse = 103,

        /// <summary>
        /// StartSharedMode のレスポンス
        /// </summary>
        StartSharedModeResponse = 104,

        /// <summary>
        /// StopSharedMode のレスポンス
        /// </summary>
        StopSharedModeResponse = 105
    }

}

using SupportApi.Controllers;
using SupportApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SupportApi.Connection
{
    public class ClientConnection : IDisposable
    {
        
        #region ** fields

        private static List<ClientConnection> _allConnections { get; set; } = new List<ClientConnection>();
        private bool _disposed;

        #endregion

        #region ** constructor

        public ClientConnection(string connectionId, string clientId, string userName)
        {
            ConnectionId = connectionId;
            ClientId = clientId;
            UserName = userName;
        }

        #endregion

        #region ** properties

        public string ClientId { get; set; }
        public string ConnectionId { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 接続に関連付けられたドキュメント識別子
        /// ドキュメントが開かれていない場合はnullまたは空にできます
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;

        public GcPdfDocumentLoader DocumentLoader
        {
            get
            {
                if (GcPdfViewerController.DocumentLoaders.TryGetValue(ClientId, out GcPdfDocumentLoader loader))
                {
                    return loader;
                }
                else
                {
                    throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.DocumentLoaderNotFoundFatal);
                }
            }
        }

        #endregion

        #region ** factory methods

        /// <summary>
        /// クライアント接続を作成して登録
        /// </summary>
        /// <param name="connectionId">SignalRのconnectionId</param>
        /// <param name="clientId">クライアントビューワのID</param>
        /// <param name="userName">所有者のユーザー名</param>
        /// <returns></returns>
        public static ClientConnection CreateConnection(string connectionId, string clientId, string userName)
        {
            if (string.IsNullOrEmpty(connectionId))
                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.CreateConnectionMissingConnectionIdInternal);
            if (string.IsNullOrEmpty(clientId))
                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.CreateConnectionMissingClientIdInternal);
            lock (_allConnections)
            {
                var connection = _allConnections.Where(i => i.ClientId == clientId).FirstOrDefault();
                if (connection != null)
                {
                    connection.ConnectionId = connectionId;
                    connection.UserName = userName;
                }
                else
                {
                    connection = new ClientConnection(connectionId, clientId, userName);
                    _allConnections.Add(connection);
                }
                return connection;
            }
        }

        /// <summary>
        /// クライアント接続の登録を解除
        /// </summary>
        /// <param name="connectionId">SignalRのconnectionId</param>
        public static void DisposeConnection(string connectionId)
        {
            lock (_allConnections)
            {
                var connection = _allConnections.Where(i => i.ConnectionId == connectionId).FirstOrDefault();
                if (connection != null)
                {
                    _allConnections.Remove(connection);
                    connection.Dispose();                    
                }
            }
        }

        internal static void DisposeConnection(ClientConnection clientConnection)
        {
            if (clientConnection != null)
            {
                DisposeConnection(clientConnection.ConnectionId);
            }
        }

        #endregion

        #region ** methods

        /// <summary>
        /// 登録されているクライアントの接続を取得
        /// </summary>
        /// <param name="connectionId">SignalRのconnectionId</param>
        /// <returns></returns>
        public static ClientConnection Get(string connectionId)
        {
            lock (_allConnections)
            {
                return _allConnections.Where(i => i.ConnectionId == connectionId).FirstOrDefault();
            }
        }

        public static List<ClientConnection> GetByUserName(string userName)
        {
            lock (_allConnections)
            {
                return _allConnections.Where(i => i.UserName == userName).ToList();
            }
        }

        public static ClientConnection GetByClientId(string clientId)
        {
            lock (_allConnections)
            {
                return _allConnections.Where(i => i.ClientId == clientId).FirstOrDefault();
            }
        }

        public static List<ClientConnection> GetByDocumentId(string documentId)
        {
            lock (_allConnections)
            {
                return _allConnections.Where(i => i.DocumentId == documentId).ToList();
            }
        }

        #endregion

        #region ** IDisposable interface implementation

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                // 管理された状態（管理されたオブジェクト）を破棄
            }
            _disposed = true;
        }

        public void Dispose()
        {
            // 管理されていないリソースを廃棄
            Dispose(true);
            // 最終化を抑制
            GC.SuppressFinalize(this);
        }



        #endregion
    }
}
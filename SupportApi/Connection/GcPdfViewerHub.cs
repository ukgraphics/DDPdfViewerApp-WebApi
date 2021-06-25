using SupportApi.Models;
#if WEB_FORMS
using Owin;
using Microsoft.AspNet.SignalR;
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SupportApi.Collaboration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SupportApi.Controllers;
using System.Collections;
using System.Linq;
using SupportApi.Collaboration.Models;
using System.Diagnostics;
using System.IO;

namespace SupportApi.Connection
{


    public class GcPdfViewerHub : Hub
    {

        const int MAX_MESSAGE_SIZE = 268435456/*256MB*/;

        #region ** startup Configure methods


#if WEB_FORMS
        /// <summary>
        /// ASP.NETやOWINの Startup クラスからGcPdfViewerのトランスポート層を登録するには、このメソッドを使用します
        /// </summary>
        /// <example>
        /// [assembly: OwinStartup(typeof(GcPdfViewerSupportApiDemo.Startup))]
        /// namespace GcPdfViewerSupportApiDemo
        /// {
        ///     public class Startup
        ///     {
        ///         public void Configuration(IAppBuilder app)
        ///         {
        ///             SupportApi.Connection.GcPdfViewerHub.Configure(app);
        ///         }
        ///     }
        /// }

        /// </example>
        /// <param name="app"></param>
        public static void Configure(IAppBuilder app)
        {
            AppDomain.CurrentDomain.Load(typeof(GcPdfViewerHub).Assembly.FullName);
            app.MapSignalR();
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = MAX_MESSAGE_SIZE;
        }
#else
        public static void Configure(IApplicationBuilder app)
        {            
            string hubUrl = "/signalr";
            app.UseSignalR(routes =>
            {
                routes.MapHub<GcPdfViewerHub>(hubUrl, opts =>
                {
                    opts.TransportMaxBufferSize = MAX_MESSAGE_SIZE;
                    opts.ApplicationMaxBufferSize = MAX_MESSAGE_SIZE;
                    //opts.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                });
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }
#endif
        #endregion

        #region ** fields


        #endregion

        #region ** public methods


        /// <summary>
        /// 接続しているすべてのクライアントにプッシュメッセージを送信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToAll(ServerMessage message)
        {

#if WEB_FORMS
            await Clients.All.send(JsonConvert.SerializeObject(message));
#else
            await Clients.All.SendAsync("send", JsonConvert.SerializeObject(message));
#endif
        }

        /// <summary>
        /// 現在の呼び出しをトリガしたクライアント以外のすべてのクライアントにメッセージを送信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToOthers(ServerMessage message)
        {
#if WEB_FORMS
            await Clients.Others.send(JsonConvert.SerializeObject(message));
#else
            await Clients.Others.SendAsync("send", JsonConvert.SerializeObject(message));
#endif
        }

        /// <summary>
        /// 現在の呼び出しをトリガしたクライアントにメッセージを送信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToSelf(ServerMessage message)
        {
#if WEB_FORMS
            await Clients.Caller.send(JsonConvert.SerializeObject(message));
#else
            await Clients.Caller.SendAsync("send", JsonConvert.SerializeObject(message));
#endif
        }

        /// <summary>
        /// 特定のクライアントにメッセージを送信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendToClient(string connectionId, ServerMessage message)
        {
#if WEB_FORMS
            await Clients.Client(connectionId).send(JsonConvert.SerializeObject(message));
#else
            await Clients.Client(connectionId).SendAsync("send", JsonConvert.SerializeObject(message));
#endif
        }

        /// <summary>
        /// 引数"documentId"で指定されたドキュメントを共有しているすべてのユーザにメッセージを送信
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="serverMessage"></param>
        /// <param name="exceptCurrentUser"></param>
        /// <returns></returns>
        public async Task SendToDocumentUsers(string documentId, ServerMessage serverMessage, 
                                                                            bool exceptCurrentUser = true)
        {
            SharedDocument sharedDocument = SharedDocumentsStorage.Instance().Get(documentId);
            Debug.Assert(sharedDocument != null, "sharedDocument should not be null here.");
            if (sharedDocument != null)
            {
                var userAccessList = sharedDocument.GetUserAccessList();
                var userAccessListEnumerator = userAccessList.GetEnumerator();
                while (userAccessListEnumerator.MoveNext())
                {
                    var cur = userAccessListEnumerator.Current;
                    if (cur.AccessMode != SharedAccessMode.AccessDenied)
                    {
                        var clientConnections = ClientConnection.GetByUserName(cur.UserName);
                        foreach(var clientConnection in clientConnections)
                        {
                            if (exceptCurrentUser && clientConnection.ConnectionId == Context.ConnectionId)
                                continue;
                            if (documentId.Equals(clientConnection.DocumentId))
                            {
                                await SendToClient(clientConnection.ConnectionId, serverMessage);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 現在の接続のユーザー名を返します
        /// </summary>
        /// <returns></returns>
        public string GetCallerUserName(ClientMessage message)
        {
            ClientConnection clientConnection = GetCurrentClientConnection(message);
            if(clientConnection == null)
            {
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UnknownConnectionIdFormat, Context.ConnectionId));
            }
            return clientConnection.UserName;
        }

        #endregion

        protected virtual async Task OnClientMessageAsync(ClientMessage message)
        {
            if(message == null)
            {
                await SendToSelf(new ServerMessage(ServerMessage.EMPTY_CORRELATION_ID) { 
                    type = (int)ServerMessageType.Error, 
                    data = GcPdfViewerController.Settings.ErrorMessages.EmptyClientMessage });
                return;
            }
            if (message.type == ClientMessageOrRequestType.Start)
            {
                var startDictData = message.data as JObject;
                try
                {
                    ClientConnection.CreateConnection(Context.ConnectionId,
                                                      startDictData.GetValue("clientId").Value<string>(),
                                                      startDictData.GetValue("userName").Value<string>());
                }
                catch (ClientConnectionException startEx)
                {
                    await SendToSelf(new ServerMessage(message.correlationId) { 
                        type = (int)ServerMessageType.Error, 
                        data = string.Format(GcPdfViewerController.Settings.ErrorMessages.StartErrorFormat, startEx.Message) });
                }
                return;
            }
            try
            {
                var clientConnection = GetCurrentClientConnection(message);
                string documentId = string.Empty;
                switch (message.type)
                {
                    case ClientMessageOrRequestType.Reconnect:
                        GetCurrentClientConnection(message); // これにより、現在の接続が再バインドされます
                        break;
                    case ClientMessageOrRequestType.Stop:
                        var stopDictData = message.data as JObject;
                        string clientId = stopDictData.GetValue("clientId").Value<string>();
                        try
                        {

                            ClientConnection.DisposeConnection(Context.ConnectionId);
                            clientConnection = ClientConnection.GetByClientId(clientId);
                            if (clientConnection != null)
                            {
                                ClientConnection.DisposeConnection(clientConnection);
                            }
                        }
                        catch (ClientConnectionException stopEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.StopErrorFormat, stopEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.ShareDocument:
                        try
                        {
                            var shareDictData = message.data as JObject;
                            if (!shareDictData.ContainsKey("documentId"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingDocumentId);
                            if (!shareDictData.ContainsKey("userName"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingUserName);
                            if (!shareDictData.ContainsKey("accessMode"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingAccessMode);
                            if (!shareDictData.ContainsKey("modificationsState"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingModificationsState);
                            documentId = shareDictData.GetValue("documentId").Value<string>();
                            string userName = shareDictData.GetValue("userName").Value<string>();
                            SharedAccessMode accessMode = (SharedAccessMode)shareDictData.GetValue("accessMode").Value<int>();



                            ModificationsState modificationsState = shareDictData.GetValue("modificationsState").ToObject<ModificationsState>();

                            //ModificationsState modificationsState = shareDictData.GetValue("modificationsState").Value<ModificationsState>();

                            if (clientConnection == null)
                            {
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.PersistentConnectionNotFound);
                            }
                            var sharedDocumentsStorage = SharedDocumentsStorage.Instance();
                            if (accessMode != SharedAccessMode.AccessDenied)
                            {
                                var sharedDocument = sharedDocumentsStorage.Get(documentId);
                                if (sharedDocument == null)
                                {
                                    var currentDocLoader = GcPdfViewerController.DocumentLoaders[clientConnection.ClientId];
                                    var documentModifications = currentDocLoader.DocumentModifications;
                                    if (modificationsState == null && documentModifications != null)
                                    {
                                        modificationsState = documentModifications.annotationsData;
                                    }
                                    sharedDocument = await sharedDocumentsStorage.ShareDocument(documentId, GetCallerUserName(message),
                                        currentDocLoader.FileName,
                                        currentDocLoader.Stream,
                                        modificationsState);
                                }
                                await sharedDocument.ChangeUserAccessModeAsync(userName, GetCallerUserName(message), accessMode);
                            }
                            else
                            {
                                await sharedDocumentsStorage.UnshareDocumentAsync(documentId, userName, GetCallerUserName(message));
                            }
                            await OnUserSharedDocumentsChanged(userName);
                            var callerUserName1 = GetCallerUserName(message);
                            if (!callerUserName1.Equals(userName))
                            {
                                await OnUserSharedDocumentsChanged(callerUserName1);
                            }
                        }
                        catch (ClientConnectionException shareDocumentEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.ShareDocumentErrorFormat, shareDocumentEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.OpenSharedDocument:
                        try
                        {
                            var openSharedDictData = message.data as JObject;
                            if (!openSharedDictData.ContainsKey("documentId"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingDocumentId);
                            documentId = openSharedDictData.GetValue("documentId").Value<string>();
                            //if (!openSharedDictData.ContainsKey("documentOptions")) throw new ClientConnectionException("A required parameter 'documentOptions' was missing.");                            
                            //DocumentOptions documentOptions = openSharedDictData.GetValue("documentOptions").Value<DocumentOptions>();
                            if (clientConnection == null)
                            {
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.PersistentConnectionNotFound);
                            }
                            var sharedDocument = SharedDocumentsStorage.Instance().Get(documentId);
                            if (sharedDocument == null)
                            {
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.SharedDocumentNotExists);
                            }
                            await sharedDocument.LoadDataFromStorage();
                            GcPdfViewerController.DisposeDocumentLoader(clientConnection.ClientId);
                            string userName = clientConnection.UserName;
                            var userAccess = sharedDocument.GetUserAccess(userName);
                            if (userAccess == null || userAccess.AccessMode == SharedAccessMode.AccessDenied)
                            {
                                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.UserNotSharingDocumentFormat, sharedDocument.OwnerUserName));
                            }
                            var documentLoader = GcPdfViewerController.CreateDocumentLoader(clientConnection.ClientId, userAccess.AccessMode, new MemoryStream(sharedDocument.Data), documentId);
                            var openDocumentInfo = documentLoader.Info;
                            await SendToSelf(new ServerMessage(message.correlationId) { type = (int)ServerMessageType.OpenSharedDocumentResponse, data = openDocumentInfo });

                        }
                        catch (ClientConnectionException openSharedDocumentEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.OpenSharedDocumentErrorFormat, openSharedDocumentEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.StartSharedMode:
                        await OnStartSharedMode(message.correlationId, message);
                        break;
                    case ClientMessageOrRequestType.StopSharedMode:
                        await OnStopSharedMode(message.correlationId, message);
                        break;
                    case ClientMessageOrRequestType.UnshareDocument:
                        try
                        {
                            var unshareDictData = message.data as JObject;
                            if (!unshareDictData.ContainsKey("documentId"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingDocumentId);
                            if (!unshareDictData.ContainsKey("userName"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingUserName);
                            documentId = unshareDictData.GetValue("documentId").Value<string>();
                            string userName = unshareDictData.GetValue("userName").Value<string>();
                            await SharedDocumentsStorage.Instance().UnshareDocumentAsync(documentId, userName, GetCallerUserName(message));
                            await OnUserSharedDocumentsChanged(userName);
                        }
                        catch (ClientConnectionException unshareDocumentEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.UnshareDocumentErrorFormat, unshareDocumentEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.Modification:
                        try
                        {
                            if (clientConnection == null)
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.PersistentConnectionNotFound);
                            documentId = clientConnection.DocumentId;
                            if (string.IsNullOrEmpty(documentId))
                            {
                                documentId = clientConnection.DocumentLoader.DocumentId;
                            }
                            JObject dictData = message.data as JObject;
                            ModificationType modificationType = (ModificationType)dictData.GetValue("type").Value<int>();
                            var sharedDocument = SharedDocumentsStorage.Instance().Get(documentId);
                            if (sharedDocument == null)
                                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.SharedDocumentNotFoundFormat, documentId));
                            var userAccess = sharedDocument.GetUserAccess(GetCallerUserName(message));
                            if (userAccess.AccessMode != SharedAccessMode.ViewAndEdit)
                            {
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.DocumentChangesAreNotAllowed);
                            }
                            ModificationsState modificationsState = await sharedDocument.OnModificationReceived(GetCallerUserName(message), modificationType, dictData);
                            if (modificationsState != null)
                            {
                                await SendToDocumentUsers(documentId, new ServerMessage(ServerMessage.EMPTY_CORRELATION_ID)
                                {
                                    type = (int)ServerMessageType.Modifications,
                                    data = modificationsState
                                });
                            }
                        }
                        catch (ClientConnectionException modificationEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.ModificationErrorFormat, modificationEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.SharedDocumentsList:
                        try
                        {
                            var sharedDocumentsListData = message.data as JObject;
                            if (!sharedDocumentsListData.ContainsKey("userName"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingUserName);
                            string userName = sharedDocumentsListData.GetValue("userName").Value<string>();
                            List<UserSharedDocument> sharedDocumentsList = SharedDocumentsStorage.Instance().GetSharedDocumentsList(userName);
                            await SendToSelf(new ServerMessage(message.correlationId) { type = (int)ServerMessageType.SharedDocumentsListResponse, data = sharedDocumentsList.ToArray() });

                        }
                        catch (ClientConnectionException sharedDocumentsListEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.SharedDocumentsListErrorFormat, sharedDocumentsListEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.UserAccessList:
                        try
                        {
                            var userAccessListData = message.data as JObject;
                            if (!userAccessListData.ContainsKey("documentId"))
                                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.MissingDocumentId);
                            documentId = userAccessListData.GetValue("documentId").Value<string>();
                            List<UserAccess> userAccessList = SharedDocumentsStorage.Instance().GetUserAccessList(documentId);
                            await SendToSelf(new ServerMessage(message.correlationId) { type = (int)ServerMessageType.UserAccessListResponse, data = userAccessList.ToArray() });

                        }
                        catch (ClientConnectionException userAccessListEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.UserAccessListErrorFormat, userAccessListEx.Message)
                            });
                        }
                        break;
                    case ClientMessageOrRequestType.AllUsersList:
                        try
                        {
                            List<string> allUsersList = GcPdfViewerController.Settings.AvailableUsers;
                            if (allUsersList == null)
                                allUsersList = new List<string>();
                            await SendToSelf(new ServerMessage(message.correlationId) { type = (int)ServerMessageType.AllUsersListResponse, data = allUsersList.ToArray() });

                        }
                        catch (ClientConnectionException allUsersListEx)
                        {
                            await SendToSelf(new ServerMessage(message.correlationId)
                            {
                                type = (int)ServerMessageType.Error,
                                data = string.Format(GcPdfViewerController.Settings.ErrorMessages.AllUsersListErrorFormat, allUsersListEx.Message)
                            });
                        }
                        break;
                }

            }

#if DEBUG
            catch (OutOfMemoryException ex)
            {
                var errorMessage = string.Format(GcPdfViewerController.Settings.ErrorMessages.OutOfMemoryExceptionDebugFormat, ex.Message, ex.StackTrace);
#else
            catch (OutOfMemoryException)
            {
                var errorMessage = GcPdfViewerController.Settings.ErrorMessages.OutOfMemoryExceptionInternal;
#endif
            await SendToSelf(new ServerMessage(message.correlationId)
                {
                    type = (int)ServerMessageType.Error,
                    data = errorMessage
                });
            }

#if DEBUG
            catch (NullReferenceException ex)
            {
                var errorMessage = string.Format(GcPdfViewerController.Settings.ErrorMessages.NullReferenceExceptionDebugFormat, ex.Message, ex.StackTrace);
#else
            catch (NullReferenceException)
            {
                var errorMessage = GcPdfViewerController.Settings.ErrorMessages.NullReferenceExceptionInternal;
#endif                
                await SendToSelf(new ServerMessage(message.correlationId)
                {
                    type = (int)ServerMessageType.Error,
                    data = errorMessage
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                var errorMessage = string.Format(GcPdfViewerController.Settings.ErrorMessages.ServerErrorDebugFormat, ex.Message, ex.StackTrace);
#else
                var errorMessage = string.Format(GcPdfViewerController.Settings.ErrorMessages.ServerErrorFormat, ex.Message);
#endif
                await SendToSelf(new ServerMessage(message.correlationId)
                {
                    type = (int)ServerMessageType.Error,
                    data = errorMessage
                });
            }
            CollectGarbageConnections();
        }



        private async Task OnStartSharedMode(string correlationId, ClientMessage message)
        {
            var clientConnection = GetCurrentClientConnection(message);
            if (clientConnection == null)
                throw new ClientConnectionException(GcPdfViewerController.Settings.ErrorMessages.PersistentConnectionNotFound);
            string documentId = clientConnection.DocumentLoader.DocumentId;
            var sharedDocument = SharedDocumentsStorage.Instance().Get(documentId);
            if (sharedDocument == null)
                throw new ClientConnectionException(string.Format(GcPdfViewerController.Settings.ErrorMessages.SharedDocumentNotFoundFormat, documentId));
            clientConnection.DocumentId = documentId;
            StartSharedModeResponse response = await sharedDocument.StartSharedMode(clientConnection);
            await SendToSelf(new ServerMessage(correlationId) { type = (int)ServerMessageType.StartSharedModeResponse, data = response });
        }

        private async Task OnStopSharedMode(string correlationId, ClientMessage message)
        {
            var clientConnection = GetCurrentClientConnection(message);
            if (clientConnection == null)
                return;
            string documentId = clientConnection.DocumentLoader.DocumentId;            
            clientConnection.DocumentId = string.Empty;
            var sharedDocument = SharedDocumentsStorage.Instance().Get(documentId);
            if (sharedDocument != null)
            {
                StopSharedModeResponse response = sharedDocument.StopSharedMode(clientConnection);
                await SendToSelf(new ServerMessage(correlationId) { type = (int)ServerMessageType.StopSharedModeResponse, data = response });
            }
            else
            {
                await SendToSelf(new ServerMessage(correlationId) { type = (int)ServerMessageType.StopSharedModeResponse, data = new StopSharedModeResponse() });
            }
        }

        /// <summary>
        /// パラメータ <paramref name="userName"/> で指定されたユーザの共有ドキュメントリストが変更された場合に呼び出されるメソッド
        /// </summary>
        /// <param name="userName"></param>
        protected virtual async Task OnUserSharedDocumentsChanged(string userName)
        {
            List<UserSharedDocument> sharedDocumentsList = SharedDocumentsStorage.Instance().GetSharedDocumentsList(userName);
            List<ClientConnection> clientConnections = ClientConnection.GetByUserName(userName);
            foreach (var clientConnection in clientConnections)
            {
                await SendToClient(clientConnection.ConnectionId, new ServerMessage(ServerMessage.EMPTY_CORRELATION_ID) { type = (int)ServerMessageType.SharedDocumentsListChanged, data = sharedDocumentsList.ToArray() });
            }
        }

        #region ** private and internal implementation


        /// <summary>
        /// 受信したクライアントメッセージ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Task Send(string json)
        {
            //var connectionId = Context.ConnectionId;var caller = Context.User;
            return OnClientMessageAsync(JsonConvert.DeserializeObject<ClientMessage>(json));
        }

#if WEB_FORMS
        public override async Task OnConnected()
        {
            OnClientConnected(Context.ConnectionId);
            await base.OnConnected();
        }
#else
        /// <summary>
        /// 接続したクライアント
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            OnClientConnected(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
#endif
#if WEB_FORMS
        public override async Task OnDisconnected(bool stopCalled)
        {
            OnClientDisconnected(Context.ConnectionId);            
            await base.OnDisconnected(stopCalled);
        }
#else
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            OnClientDisconnected(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
#endif

        public ClientConnection GetCurrentClientConnection(ClientMessage message)
        {
            ClientConnection con = ClientConnection.Get(Context.ConnectionId);
            if(message != null)
            {
                var clientId = message.clientId;
                var conByClient = ClientConnection.GetByClientId(clientId);
                if(conByClient != null && (con == null || !conByClient.ConnectionId.Equals(Context.ConnectionId)))
                {
                    OnClientReconnected(conByClient.ConnectionId);
                    conByClient.ConnectionId = Context.ConnectionId;
                    con = conByClient;
                }
            }
            Debug.Assert(con != null, "Connection not found");
            return con;
        }

        protected void OnClientConnected(string connectionId)
        {
            lock (_disconnectedClients)
            {
                if (_disconnectedClients.ContainsKey(connectionId))
                {
                    _disconnectedClients.Remove(connectionId);
                }
            }
        }

        protected void OnClientReconnected(string connectionId)
        {
            lock (_disconnectedClients)
            {
                if (_disconnectedClients.ContainsKey(connectionId))
                {
                    _disconnectedClients.Remove(connectionId);
                }
            }
        }

        protected void OnClientDisconnected(string connectionId)
        {
            lock (_disconnectedClients)
            {
                _disconnectedClients[connectionId] = DateTime.Now;
            }
        }

        private void CollectGarbageConnections()
        {
            lock (_disconnectedClients)
            {
                var waitForReconnectTime = GcPdfViewerController.Settings.Collaboration.WaitForReconnectTime;
                if (waitForReconnectTime == null)
                    waitForReconnectTime = new TimeSpan(0, 30, 0);
                var connectionsToDispose = _disconnectedClients.Where(kv => (DateTime.Now - kv.Value) > waitForReconnectTime).ToList();
                foreach(var id in connectionsToDispose)
                {
                    ClientConnection.DisposeConnection(id.Key);
                    _disconnectedClients.Remove(id.Key);
                }
            }
        }

        private static Dictionary<string, DateTime> _disconnectedClients = new Dictionary<string, DateTime>();


        #endregion


    }
}

using System;
using System.IO;
using System.Linq;
using SupportApi.Utils;
using SupportApi.Models;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using SupportApi.Connection;
using SupportApi.Collaboration;
using System.Net;
using System.Web;
using GrapeCity.Documents.Pdf;


#if WEB_FORMS
using System.Web.Http;
using System.Web.Http.Controllers;
#else
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
#endif

namespace SupportApi.Controllers
{
    /// <summary>
    /// GcPdfViewer Support API コントローラ
    /// </summary>
    /// <example>
    /// ASP.NET CoreのApplicationPartsを使用してコントローラを設定
    /// <code>
    ///    public void ConfigureServices(IServiceCollection services)
    ///    {
    ///      services.AddMvc().ConfigureApplicationPartManager(apm => 
    ///         apm.ApplicationParts.Add(new AssemblyPart(typeof(GcPdfViewerController).GetTypeInfo().Assembly)));
    ///      ...
    /// </code>
    /// </example>
    [Route("SupportApi/GcPdfViewer")]
#if WEB_FORMS
    public class GcPdfViewerController : ApiController
#else
    [ApiController]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(268435456/*256MB*/)] 
    [RequestFormLimits(MultipartBodyLengthLimit = 268435456/*256MB*/, KeyLengthLimit = 268435456, ValueLengthLimit = 268435456, ValueCountLimit = 268435456)]
    public class GcPdfViewerController : ControllerBase
#endif
    {
        //public GcPdfViewerController()
        //{
        //    GcPdfDocument.SetLicenseKey("XXXXX");
        //}

        #region ** fields

        protected static ConcurrentDictionary<string, GcPdfDocumentLoader> _docLoaders = new ConcurrentDictionary<string, GcPdfDocumentLoader>();
        protected static ConcurrentDictionary<string, DocumentOptions> _docOptions = new ConcurrentDictionary<string, DocumentOptions>();
        protected static ConcurrentDictionary<string, DateTime> _docLoaderLastPingTime = new ConcurrentDictionary<string, DateTime>();
        protected static string _lastError;

        #endregion

        #region ** Event handlers

        /// <summary>
        /// このメソッドは、クライアントにて開いているドキュメントに変更が適用された場合に呼び出されます
        /// </summary>
        /// <param name="documentLoader"></param>
        public virtual void OnDocumentModified(GcPdfDocumentLoader documentLoader)
        {

        }

        #endregion

        #region ** properties

        /// <summary>
        /// GcPdfDocumentLoader クラスのインスタンス（キー：client id, value）
        /// </summary>
        public static ConcurrentDictionary<string, GcPdfDocumentLoader> DocumentLoaders
        {
            get
            {
                return _docLoaders;
            }
        }

        /// <summary>
        /// SupportAPIのグローバル設定
        /// </summary>
        public static SupportApiSettings Settings { get; } = new SupportApiSettings();

        #endregion

        #region ** Web API methods

        /// <summary>
        /// SupportAPIのバージョンを返します
        /// </summary>
        /// <returns>SupportAPIのバージョン</returns>
        /// <example>
        /// http://localhost:50016/SupportApi/GcPdfViewer/Version
        /// http://localhost:50016/api/pdf-viewer/version
        /// </example>
#if WEB_FORMS
        [Route("Version")]
        [HttpGet()]
#else
        [HttpGet("Version")]
#endif
        public virtual string Version()
        {
            try
            {
                VerifyTokenInternal(nameof(Version));
                return $"SupportApi v{(typeof(GcPdfViewerController)).Assembly.GetName().Version.ToString(3)}/GcPdf v{typeof(GrapeCity.Documents.Pdf.GcPdfDocument).Assembly.GetName().Version.ToString(4)}";
            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }
        }

        /// <summary>
        /// ドキュメントのオプションを設定
        /// </summary>
        /// <returns></returns>
#if WEB_FORMS
        [HttpPost()]
        [Route("SetOptions")]
        public virtual async Task<object> SetOptions()
        {
            string clientID = GetLastParameter();
#else
        [Route("SetOptions/{clientID}")]
        public virtual async Task<object> SetOptions(string clientID)
        {
#endif

            using (var ms = await ReadBodyAsync())
            {
                DocumentOptions documentOptions = null;
                try
                {
                    VerifyTokenInternal(nameof(SetOptions));
                    var bytes = ms.ToArray();
                    string json = System.Text.Encoding.UTF8.GetString(bytes);
                    documentOptions = JsonConvert.DeserializeObject<DocumentOptions>(json);
                    if (documentOptions == null || string.IsNullOrEmpty(documentOptions.clientId))
                    {
                        throw new Exception(GcPdfViewerController.Settings.ErrorMessages.CantParseDocumentOptions);
                    }
                    string clientId = documentOptions.clientId;
                    if (_docOptions.ContainsKey(clientId))
                        _docOptions.TryRemove(documentOptions.clientId, out var val);
                    _docOptions.TryAdd(documentOptions.clientId, documentOptions);
                    return _PrepareStringAnswer("ok");
                }
                catch (Exception ex)
                {
                    return _PrepareStringAnswer($"Error: {ex.Message}");
                }                
            }
            
        }

        /*
        /// <summary>
        /// 外部URLからPDFドキュメントをダウンロードします
        /// 外部URLからPDFを開きたい場合はこの方法を使用します
        /// </summary>
        /// <example>
        ///  // 外部URLサンプルからPDFを開く:
        ///  viewer.options.friendlyFileName = "test.pdf";
        ///  viewer.open("SupportApi/GcPdfViewer/GetFileFromUrl/<%=Convert.ToBase64String(Encoding.UTF8.GetBytes("http://localhost:22138/Documents/test.pdf"))%>/file_name");
        /// </example>
        /// <param name="url">URL</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns>ダウンロードされたファイル</returns>

#if WEB_FORMS
        [HttpGet()]
        [Route("GetFileFromUrl")]
        public virtual object GetFileFromUrl()
        {
            string fileName = GetLastParameter();
            string url = GetLastParameter(2);
#else
        [HttpGet("GetFileFromUrl/{url}/{fileName}")]
        public virtual object GetFileFromUrl(string url, string fileName)
        {
#endif
            try
            {
                fileName = System.Web.HttpUtility.UrlDecode(fileName);
                url = System.Web.HttpUtility.UrlDecode(url);
                try {
                    url = Encoding.UTF8.GetString(Convert.FromBase64String(url));
                } catch (Exception) {  }
                
                try {
                    fileName = Encoding.UTF8.GetString(Convert.FromBase64String(fileName));
                } catch (Exception) {  }
                if (!fileName.EndsWith(".pdf"))
                    fileName = $"{fileName}.pdf";

                if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
                {
#if WEB_FORMS
                    if (!url.StartsWith("/"))
                        url = $"/{url}";
                    url = $"{Request.RequestUri.GetLeftPart(UriPartial.Authority)}{url}";
#else
                url = $"{Request.Scheme}://{(url.StartsWith(Request.Host.Value) ? "" : Request.Host.Value)}/{(Request.PathBase.HasValue ? Request.PathBase.Value : "")}{url}";
#endif
                }
                using (var client = new WebClientWithTimeout())
                {
                    var bytes = client.DownloadData(url);
                    var data = new MemoryStream(bytes);
                    return _PrepareFileAttachmentAnswer(data, fileName);
                }
                
            }
            catch (Exception ex)
            {
                SetLastError(ex.Message);
                throw;
            }
        }
        */

        /// <summary>
        /// バイナリデータを使用してPDFドキュメントを開きます
        /// </summary>
        /// <returns>開いたドキュメントに関する情報</returns>

#if WEB_FORMS
        [HttpPost()]
        [Route("OpenBinary")]
        public virtual async Task<object> OpenBinary()
        {
          string clientId = GetLastParameter();
#else
        [Route("OpenBinary/{clientId}")]
        public virtual async Task<object> OpenBinary(string clientId)
        {
#endif
            VerifyTokenInternal(nameof(OpenBinary));
            MemoryStream ms = await ReadBodyAsync();
            SharedAccessMode sharedAccessMode = SharedAccessMode.ViewAndEdit;
            string knownDocumentId = null;
            var connection = ClientConnection.GetByClientId(clientId);
            if(connection != null)
                knownDocumentId = connection.DocumentId;
            var loader = CreateDocumentLoader(clientId, sharedAccessMode, ms, knownDocumentId);
            return _PrepareJsonAnswer(loader.Info);
        }

        /// <summary>
        /// 署名情報をドキュメントに関連付けます
        /// 保存すると、指定された情報を使ってドキュメントに署名されます
        /// </summary>
        /// <param name="docId">ドキュメントID</param>
        /// <param name="signatureInfo">署名情報</param>
        /// <returns>ドキュメントに関する情報</returns>
        [Route("Sign")]
        public virtual async Task<OpenDocumentInfo> Sign(string clientID, [FromBody] SignatureInfo signatureInfo)
        {
            VerifyTokenInternal(nameof(Sign));
            var docLoader = GetDocumentLoader(clientID);
            _ = await ReadBodyAsync();
            docLoader.Sign(signatureInfo);
            return docLoader.Info;
        }

        /// <summary>
        /// 署名フィールドを検証
        /// 署名が適用された後にドキュメントが修正されていない場合、
        /// 署名は有効とみなされます
        /// </summary>
        /// <param name="clientID">クライアント識別子</param>
        /// <param name="fieldName">署名フィールド名</param>
        /// <returns>検証結果に応じて"true "または "false "</returns>
#if WEB_FORMS
        [Route("VerifySignature")]
        [HttpPost]
        public virtual bool VerifySignature()
        {
            string clientID = GetLastParameter(2);
            string fieldName = GetLastParameter(1);
#else
        [Route("VerifySignature/{clientID}/{fieldName}")]
        public virtual bool VerifySignature(string clientID, string fieldName)
        {
#endif
            VerifyTokenInternal(nameof(VerifySignature));
            fieldName = System.Web.HttpUtility.UrlDecode(fieldName);
            var docLoader = GetDocumentLoader(clientID);
            if (docLoader.VerifySignature(fieldName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 開いているドキュメントを閉じて、ドキュメントに関連するサーバーリソースを解放します
        /// </summary>
        /// <param name="clientID">クライアント識別子</param>
        /// <returns>"ok"</returns>
#if WEB_FORMS
        [Route("Close")]
        [HttpPost()]
        public virtual async Task<object> Close()
        {
            string clientID = GetLastParameter();

#else
        [Route("Close/{clientID?}")]
        public virtual async Task<object> Close(string clientID)
        {
#endif
            using (var ms = await ReadBodyAsync())
            {
                if (string.IsNullOrEmpty(clientID))
                {
                    var bytes = ms.ToArray();
                    clientID = System.Text.Encoding.UTF8.GetString(bytes);
                }
                if (!string.IsNullOrEmpty(clientID))
                {
                    DisposeDocumentLoader(clientID);
                    return _PrepareStringAnswer("ok");
                }
                return _PrepareStringAnswer("not found");
            }
        }

        /// <summary>
        /// クライアントビューワからの ping を受け付けます
        /// このメソッドは、指定された <paramref name="docId"/> のドキュメントが
        /// サーバ上で開いている場合に "ok" を返します
        /// </summary>
        /// <param name="docId">ドキュメントID（任意）</param>
        /// <returns>"ok" または "not-found"</returns>

#if WEB_FORMS
        [HttpPost()]
        [Route("Ping")]
        public virtual string Ping()
        {
            string docId = GetLastParameter();
#else
        [Route("Ping/{docId?}")]
        public virtual string Ping(string docId) 
        {
#endif
            VerifyTokenInternal(nameof(Ping));
            if (!string.IsNullOrEmpty(docId) && _docLoaderLastPingTime.ContainsKey(docId))
            {
                _docLoaderLastPingTime[docId] = DateTime.Now;
                _DisposeInactiveDocuments();
                return "ok";
            }
            _DisposeInactiveDocuments();
            return "not-found";
        }

        /// <summary>
        /// 開いているドキュメントに指定した変更を適用します
        /// 適用する変更はリクエストの本文にて渡されます
        /// <para>
        /// 指定されたIDを持つドキュメントがサーバ上で見つからなかった場合、
        /// 文字列「Code-N5001」を含むエラーメッセージを返します
        /// クライアントビューワがその文字列を受け取ると、再度ドキュメントを開いて
        /// リクエストを繰り返します
        /// </para>
        /// </summary>
        /// <param name="clientID">クライアント識別子</param>
        /// <returns>変更が適用された場合は "ok"、そうでない場合はエラーメッセージ</returns>
#if WEB_FORMS
        [HttpPost()]
        [Route("Modify")]
        public virtual async Task<object> Modify()
        {
            string clientID = GetLastParameter();
#else
        [Route("Modify/{clientID}")]
        public virtual async Task<object> Modify(string clientID)
        {
#endif
            VerifyTokenInternal(nameof(Modify));
            using (var ms = await ReadBodyAsync())
            {
                DocumentModifications modifications = null;
                try
                {
                    var bytes = ms.ToArray();
                    string json = System.Text.Encoding.UTF8.GetString(bytes);
                    modifications = JsonConvert.DeserializeObject<DocumentModifications>(json);
                    if (modifications == null)
                    {
                        throw new Exception(GcPdfViewerController.Settings.ErrorMessages.CantParseDocumenModifications);
                    }                    
                }
                catch (Exception ex)
                {
                    return _PrepareStringAnswer($"Error: {ex.Message}");
                }
                try
                {
                    var docLoader = GetDocumentLoader(clientID);
                    docLoader.ApplyDocumentModifications(modifications);
                    OnDocumentModified(docLoader);
                    return _PrepareStringAnswer("ok");
                }
                catch(NotLicensedException ex)
                {
                    return _PrepareStringAnswer(ex.Message);
                }
                catch(DocumentLoaderNotFoundException ex)
                {
                    // ドキュメントがサーバー上で開いていません
                    // エラーテキストの "Code-N5001" は、ドキュメントを再度開く必要があることを
                    // クライアントに伝えます。
                    return _PrepareStringAnswer($"Error: Code-N5001 {ex.Message}");
                }
                catch (Exception ex)
                {
                    return _PrepareStringAnswer(ex.Message);
                }
            }
        }

        /// <summary>
        /// ドキュメントローダーがサーバー上に存在することを確認してください
        /// サーバー再起動後にドキュメントローダーが存在しない可能性があります
        /// </summary>
        /// <returns></returns>
#if WEB_FORMS
        [HttpPost()]
        [Route("CheckDocumentLoader")]
#else
        [Route("CheckDocumentLoader")]
#endif
        public virtual object CheckDocumentLoader()
        {
            try
            {
                VerifyTokenInternal(nameof(CheckDocumentLoader));
                var clientId = GetQueryValue("clientId");
                GetDocumentLoader(clientId);
                return _PrepareStringAnswer("ok");
            }
            catch (DocumentLoaderNotFoundException ex)
            {
                return _PrepareStringAnswer($"Code-N5001: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError(ex.Message);
                return _PrepareStringAnswer($"Error: {ex.Message}");
            }
        }

#if WEB_FORMS
        [HttpGet()]
        [Route("DownloadFile")]
#else
        [HttpGet("DownloadFile")]
#endif
        public virtual async Task<object> DownloadFile()
        {
            VerifyTokenInternal(nameof(DownloadFile));
            string clientId = GetQueryValue("clientId");
            string fileId = GetQueryValue("fileId");
            int packageIndex = int.Parse(GetQueryValue("packageIndex"));
            int packagesCount = int.Parse(GetQueryValue("packagesCount"));
            string contentType = GetQueryValue("contentType");
            using (var ms = await ReadBodyAsync())
            {
                try
                {
                    var docLoader = GetDocumentLoader(clientId);
                    var sharedDoc = SharedDocumentsStorage.Instance().Get(docLoader.DocumentId);
                    if (sharedDoc != null)
                    {
                        byte[] resultBytes = sharedDoc.GetAttachedFile(fileId);                        
                        return _PrepareFileAttachmentAnswer(new MemoryStream(resultBytes), fileId);
                    }                    
                    return null;
                }
                catch (DocumentLoaderNotFoundException ex)
                {
                    // このケースは想定されていません 
                    // ドキュメントローダはすでに CheckDocumentLoader メソッドでチェックされているはずです
                    SetLastError(ex.Message);
                    return _PrepareStringAnswer($"Code-N5001: {ex.Message}");
                }
                catch (Exception ex)
                {
                    SetLastError(ex.Message);
                    return _PrepareStringAnswer($"Error: {ex.Message}");
                }
            }
        }


#if WEB_FORMS
        [HttpPost()]
        [Route("UploadFile")]
#else
        [HttpPost("UploadFile")]
#endif
        public virtual async Task<object> UploadFile()
        {
            VerifyTokenInternal(nameof(UploadFile));
            string clientId = GetQueryValue("clientId");
            string fileId = GetQueryValue("fileId");
            int packageIndex = int.Parse(GetQueryValue("packageIndex"));
            int packagesCount = int.Parse(GetQueryValue("packagesCount"));
            string contentType = GetQueryValue("contentType");
            using (var ms = await ReadBodyAsync())
            {
                try
                {
                    var docLoader = GetDocumentLoader(clientId);
                    var bytes = ms.ToArray();
                    byte[] resultBytes;
                    if (contentType == "base64") {
                        string base64Content = System.Text.Encoding.UTF8.GetString(bytes);
                        resultBytes = Convert.FromBase64String(base64Content);
                    }
                    else
                    {
                        resultBytes = bytes;
                    }
                    if (docLoader.AttachedFiles.ContainsKey(fileId))
                    {
                        if(!docLoader.AttachedFiles.TryRemove(fileId, out _))
                        {
                            return _PrepareStringAnswer($"Cannot remove old file data from concurrent dictionary.");
                        }
                    }
                    if(!docLoader.AttachedFiles.TryAdd(fileId, resultBytes))
                    {
                        return _PrepareStringAnswer($"Cannot add file data to concurrent dictionary.");
                    }
                    var sharedDoc = SharedDocumentsStorage.Instance().Get(docLoader.DocumentId);
                    if(sharedDoc != null)
                    {
                        sharedDoc.SetAttachedFile(fileId, resultBytes);
                    }
                    return _PrepareStringAnswer("ok");
                }
                catch (DocumentLoaderNotFoundException ex)
                {
                    // このケースは想定されていません 
                    // ドキュメントローダはすでに CheckDocumentLoader メソッドでチェックされているはずです
                    SetLastError(ex.Message);
                    return _PrepareStringAnswer($"Code-N5001: {ex.Message}");
                }
                catch (Exception ex)
                {
                    SetLastError(ex.Message);
                    return _PrepareStringAnswer($"Error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ドキュメントに加えられたすべての変更を破棄します
        /// </summary>
        /// <param name="clientID">一意のクライアント識別子</param>
        /// <returns>"ok"</returns>
#if WEB_FORMS
        [HttpPost()]
        [Route("Reset")]
        public virtual object Reset()
        {
            string clientID = GetLastParameter();
#else
        [Route("Reset/{clientID}")]
        public virtual object Reset(string clientID)
        {
#endif
            VerifyTokenInternal(nameof(Reset));
            var docLoader = GetDocumentLoader(clientID);
            docLoader.Reset();
            return _PrepareStringAnswer("ok");
        }

        /// <summary>
        /// 変更を適用し、PDFドキュメントをファイルとしてダウンロードします
        /// </summary>
        /// <param name="clientID">一意のクライアント識別子</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns></returns>

#if WEB_FORMS
        [Route("Download")]
        [HttpGet]
        public virtual object Download()
        {
            string clientId = GetLastParameter(2);
            string fileName = GetLastParameter(1);
#else
        [HttpGet("Download/{clientID}/{fileName}")]
        public virtual object Download(string clientId, string fileName)
        {
#endif
            VerifyTokenInternal(nameof(Download));
            var docLoader = GetDocumentLoader(clientId);
            var content = new MemoryStream();
            try
            {
                docLoader.Save(content);
            }
            catch(Exception ex)
            {
                SetLastError(ex.Message);
            }
            fileName = string.IsNullOrEmpty(fileName) ? $"{docLoader.ClientId}.pdf" : fileName;
            if (fileName.EndsWith("-pdf"))
            {
                fileName = $"{fileName.Substring(0, fileName.Length - "-pdf".Length)}.pdf";
            }
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", string.Empty);
            return _PrepareFileAttachmentAnswer(content, fileName);
        }

#if WEB_FORMS
        [Route("DownloadUnmodified")]
        [HttpGet]
        public virtual object DownloadUnmodified()
        {
            string clientId = GetLastParameter(2);
            string fileName = GetLastParameter(1);
#else
        [HttpGet("DownloadUnmodified/{clientId}/{fileName}")]
        public virtual object DownloadUnmodified(string clientId, string fileName)
        {
#endif
            VerifyTokenInternal(nameof(DownloadUnmodified));
            MemoryStream content;
            /*
            ClientConnection clientConnection = ClientConnection.GetByClientId(clientId);
            if(clientConnection != null)
            {

            }*/
            var docLoader = GetDocumentLoader(clientId);
            content = new MemoryStream();
            docLoader.SaveWithoutModifications(content);
            fileName = string.IsNullOrEmpty(fileName) ? $"{docLoader.ClientId}.pdf" : fileName;
            if (fileName.EndsWith("-pdf"))
            {
                fileName = $"{fileName.Substring(0, fileName.Length - "-pdf".Length)}.pdf";
            }
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", string.Empty);
            return _PrepareFileAttachmentAnswer(content, fileName);
        }

        /// <summary>
        /// このメソッドは、エラーが発生した直後にクライアントから呼び出されます
        /// </summary>
        /// <returns></returns>
#if WEB_FORMS
        [Route("GetLastError")]
        [HttpGet]
#else
        [HttpGet("GetLastError")]
#endif
        public object GetLastError()
        {
            string s = _lastError;
            _lastError = string.Empty;            
            return _PrepareStringAnswer(string.IsNullOrEmpty(s) ? "" : s);
        }

        #endregion

        #region ** Protected


        protected object _PrepareFileAttachmentAnswer(Stream content, string fileName)
        {
#if WEB_FORMS
            return new HttpResponseMessage() { Content = new CustomFileAttachmentContent(content, fileName) };
#else
            /* 添付ファイルのファイル名がIE11で動作していることを確認してください
             * コンテンツ処理値には互換モードのIE11を使用します */
            string contentDispositionValues = $"attachment;filename=\"{fileName}\"";   
            Response.Headers.Add("content-disposition", contentDispositionValues);
            Response.Headers.ContentLength = content.Length;
            return new FileStreamResult(content, "application/octet-stream");
#endif
        }

        protected object _PrepareStringAnswer(string s)
        {
#if WEB_FORMS
            return new HttpResponseMessage() { Content = new CustomStringContent(s) };
#else
            return s;
#endif
        }

        protected object _PrepareJsonAnswer(object o)
        {
#if WEB_FORMS
            return new HttpResponseMessage() { Content = new CustomJsonContent(o)};
#else
            return o;
#endif
        }

#if WEB_FORMS
        protected string GetLastParameter(int shift = 1)
        {
            var localPathArr = Request.RequestUri.LocalPath.Split('/');
            string last = localPathArr[localPathArr.Length - shift];
            return last;
        }
#endif

        protected async Task<MemoryStream> ReadBodyAsync()
        {
            try
            {
                var ms = new MemoryStream();
#if WEB_FORMS
                await ControllerContext.Request.Content.CopyToAsync(ms);
#else
            await Request.Body.CopyToAsync(ms);
#endif
                return ms;
            } 
            catch (Exception ex)
            {
                SetLastError("Document is too large. " + ex.Message);
                throw;
            }
        }

        public static void SetLastError(string s)
        {
            _lastError = s;
        }


        public static GcPdfDocumentLoader CreateDocumentLoader(string clientId, SharedAccessMode accessMode, Stream data, string knownDocumentId = null)
        {
            DocumentOptions docOptions;
            if (!_docOptions.TryGetValue(clientId, out docOptions))
            {
                docOptions = new DocumentOptions();
            }
            var loader = new GcPdfDocumentLoader(Settings, docOptions, accessMode);            
            _docLoaders[loader.ClientId] = loader;
            _docLoaderLastPingTime[loader.ClientId] = DateTime.Now;
            if (!string.IsNullOrEmpty(knownDocumentId))
            {
                loader.DocumentId = knownDocumentId;
            }
            loader.Open(data, knownDocumentId);
            return loader;
        }

        public static void DisposeDocumentLoader(string clientID)
        {
            if (clientID == null)
                return;
            if (_docLoaderLastPingTime.ContainsKey(clientID))
            {
                _docLoaderLastPingTime.TryRemove(clientID, out _);
            }
            var loader = _docLoaders.ContainsKey(clientID) ? _docLoaders[clientID] : null;
            if (loader == null)
            {
                return;
            }
            loader.Dispose();
            _docLoaders.TryRemove(clientID, out _);
        }

        private void _DisposeInactiveDocuments()
        {
            var dt = DateTime.Now.AddMinutes(-10);
            var inactiveClientIDs = _docLoaderLastPingTime.Where(kv => kv.Value < dt).Select(kv => kv.Key).ToArray();
            foreach (var clientId in inactiveClientIDs)
            {
                DisposeDocumentLoader(clientId);
            }
        }



        public static GcPdfDocumentLoader GetDocumentLoader(string clientID)
        {
            var loader = _docLoaders.ContainsKey(clientID) ? _docLoaders[clientID] : null;
            if (loader == null)
            {
                throw new DocumentLoaderNotFoundException(string.Format("Document loader for client with id {0} not found.", clientID));
            }
            return loader;
        }

        #endregion

        #region ** private

        protected string GetQueryValue(string key, bool secured = false)
        {
            string result = string.Empty;
#if WEB_FORMS
            var val = result = ControllerContext.Request.GetQueryNameValuePairs().Where(q=>q.Key == key).FirstOrDefault().Value;
            if (val != null)
                result = val;
#else
            var query = HttpContext.Request.Query;
            if (query.ContainsKey(key) &&
                query.TryGetValue(key, out var resultArr) && resultArr.Count > 0)
                result = resultArr[0];
#endif
            if (!string.IsNullOrEmpty(result))
            {
                result = System.Web.HttpUtility.UrlDecode(result);
            }
            if (secured) { }
            return result;
        }

        private void VerifyTokenInternal(string actionName)
        {
#if WEB_FORMS
            HttpControllerContext controllerContext = ControllerContext;
#else
            ControllerContext controllerContext = ControllerContext;
#endif
            string token = GetQueryValue("token", true);
            if (!string.IsNullOrEmpty(token))
            {
                var matches = Regex.Matches(token, @"value=""(.*?)""", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    Match m = matches[0];
                    if (m.Groups.Count > 1)
                        token = m.Groups[1].Value;
                }
#if WEB_FORMS
                if (!controllerContext.Request.Headers.Contains("RequestVerificationToken"))
                    controllerContext.Request.Headers.Add("RequestVerificationToken", token);
#else
                 if (!controllerContext.HttpContext.Request.Headers.ContainsKey("RequestVerificationToken"))
                    controllerContext.HttpContext.Request.Headers.Add("RequestVerificationToken", new StringValues(token));           
#endif
            }
            var e = new VerifyTokenEventArgs(controllerContext, token, actionName);
            Settings.OnVerifyToken(e);
            if (e.Reject)
            {
                throw new HttpListenerException(401, GcPdfViewerController.Settings.ErrorMessages.Unauthorized);
            }
        }

#endregion
    }
}

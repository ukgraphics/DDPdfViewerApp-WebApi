using GrapeCity.Documents.Pdf;
using SupportApi.Collaboration;

namespace SupportApi.Models
{

    /// <summary>
    /// PDFドキュメントに関する情報
    /// このクラスのインスタンスは、ドキュメントが開かれたときに適切なクライアントビューワに渡されます
    /// </summary>
    public class OpenDocumentInfo
    {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="fileName"></param>
        /// <param name="accessMode"></param>
        /// <param name="documentInfo"></param>
        /// <param name="pagesCount"></param>
        /// <param name="clientId"></param>
        /// <param name="defaultViewPortSize"></param>
        /// <param name="docOptions"></param>
        public OpenDocumentInfo(string documentId, string fileName, SharedAccessMode accessMode, DocumentInfo documentInfo, int pagesCount, string clientId, System.Drawing.SizeF defaultViewPortSize, DocumentOptions docOptions)
        {
            this.documentId = documentId;
            this.fileName = fileName;
            this.accessMode = accessMode;
            this.clientId = clientId;
            info = new DocumentInfoWrapper(documentInfo);
            this.pagesCount = pagesCount;
            this.defaultViewPortSize = new { w = defaultViewPortSize.Width, h = defaultViewPortSize.Height };
            documentOptions = docOptions;
        }


        /// <summary>
        /// 一意なクライアント(ビューワアプリケーション)のセッション識別子
        /// </summary>
        public string clientId { get; set; }

        /// <summary>
        /// ユーザーのアクセスモード
        /// </summary>
        public SharedAccessMode accessMode { get; set; }

        /// <summary>
        /// 一意のドキュメント識別子。クライアント間でドキュメントを共有するために使用されます
        /// </summary>
        public string documentId { get; set; }

        public string fileName { get; set; }

        /// <summary>
        /// クライアントで指定されたフレンドリーファイル名（GcPdfViewerのfriendlyFileNameオプションを参照）
        /// </summary>
        public string friendlyFileName { get; set; }

        /// <summary>
        /// 総ページ数
        /// </summary>
        public int pagesCount { get; set; }

        /// <summary>
        /// デフォルトのページビューのポートサイズ
        /// </summary>
        public dynamic defaultViewPortSize { get; set; }

        /// <summary>
        /// クライアントビューワから渡されるドキュメントオプション
        /// </summary>
        public DocumentOptions documentOptions { get; set; }

        /// <summary>
        /// ドキュメントに関する情報（作成者、タイトル、キーワードなど）
        /// </summary>
        public DocumentInfoWrapper info { get; set; }

    }

    public class DocumentInfoWrapper
    {

        public string keywords { get; set; }

        public string author { get; set; }

        public string creator { get; set; }

        public string subject { get; set; }

        public string title { get; set; }

        public string producer { get; set; }

        public string creationDate { get; set; }

        public string modifyDate { get; set; }

        public DocumentInfoWrapper(DocumentInfo documentInfo)
        {
            if (documentInfo != null)
            {
                keywords = documentInfo.Keywords;
                author = documentInfo.Author;
                creator = documentInfo.Creator;
                subject = documentInfo.Subject;
                title = documentInfo.Title;
                producer = documentInfo.Producer;
                // 注意：PdfDateTime.DateTimeValueは、日付が正しくない場合に例外になることがあるので、
                // 代わりに文字列の値を使用しています。
                creationDate = documentInfo.CreationDate.HasValue ? documentInfo.CreationDate.Value.ToString() : "";
                modifyDate = documentInfo.ModifyDate.HasValue ? documentInfo.ModifyDate.Value.ToString() : "";
            }
        }
    }
}

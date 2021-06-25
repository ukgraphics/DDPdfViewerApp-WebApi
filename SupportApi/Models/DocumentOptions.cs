namespace SupportApi.Models
{
    /// <summary>
    /// ビューワから渡されたクライアント側のオプションとプロパティ
    /// </summary>
    public class DocumentOptions
    {

        /// <summary>
        /// 一意のビューワ識別子（必須）
        /// </summary>
        public string clientId;

        /// <summary>
        /// ドキュメントを開くときに使用するパスワード（任意）
        /// </summary>
        public string password;

        /// <summary>
        /// ドキュメントを開くために使用したURL
        /// </summary>
        public string fileUrl;

        /// <summary>
        /// ドキュメントを開く際に使用したファイル名
        /// </summary>
        public string fileName;

        /// <summary>
        /// クライアントで指定されたフレンドリーファイル名のオプション（GcPdfViewerのfriendlyFileNameオプションを参照）
        /// </summary>
        public string friendlyFileName;

        /**
        * ビューワに関連付けられた任意のデータ
        * このデータは、ドキュメント保存時にサーバーに送信されます
        **/
        public object userData;

        /// <summary>
        /// 現在のユーザー名。この値は、注釈のステータス設定や返信時のユーザ名として使用されます
        /// </summary>
        public string userName;



    }
}

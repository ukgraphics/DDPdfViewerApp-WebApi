using System;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Pdf.AcroForms;

namespace SupportApi.Models
{

    /// <summary>
    /// ドキュメントの署名についての情報
    /// </summary>
    public class SignatureInfo
    {
        /// <summary>
        /// 署名者から提供された情報を取得または設定し、
        /// 受信者が署名者に連絡して署名を確認できるようにします (例：電話番号など)
        /// </summary>
        public string ContactInfo { get; internal set; }

        /// <summary>
        /// 署名のCPUホスト名または物理的な場所を取得または設定
        /// <para>
        /// デフォルトでは、このプロパティは <see cref="Environment.MachineName"/> で初期化されます
        /// </para>
        /// </summary>
        public string Location { get; internal set; }

        /// <summary>
        /// ドキュメントに署名している個人または権限者の名前を取得または設定
        /// <para>
        /// 注: この値は、署名者の証明書など、署名から名前を抽出できない場合にのみ使用されます
        /// </para>
        /// <para>
        /// デフォルトでは、このプロパティは <see cref="Environment.UserName"/> で初期化されます
        /// </para>
        /// </summary>
        public string SignerName { get; internal set; }

        /// <summary>
        /// "同意する"などの署名の理由を取得または設定
        /// </summary>
        public string Reason { get; internal set; }

        /// <summary>
        /// 署名のダイジェストアルゴリズムを取得または設定
        /// <para>
        /// 注: <see cref="SignatureFormat"/> が <see cref="Pdf.SignatureFormat.PKCS7SHA1"/> の場合、
        /// このプロパティは無視され、<see cref="Pdf.SignatureDigestAlgorithm.SHA1"/> が使用されます
        /// </para>
        /// </summary>
        public SignatureDigestAlgorithm SignatureDigestAlgorithm { get; internal set; }

        /// <summary>
        /// 署名のフォーマットを取得または設定
        /// </summary>
        public SignatureFormat SignatureFormat { get; internal set; }

        /// <summary>
        /// PDF のデジタル署名にタイムスタンプを含めるために使われる
        ///  <see cref="Pdf.TimeStamp"/> オブジェクトを取得または設定
        /// </summary>
        public TimeStamp TimeStamp { get; internal set; }

        /// <summary>
        /// 電子署名を保存するために使用されるフォームフィールドを取得または設定
        /// </summary>
        public Field SignatureField { get; internal set; }
    }
}

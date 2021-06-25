using System.Threading.Tasks;

namespace SupportApi.Collaboration.Storages
{
    /// <summary>
    /// ICollaborationStorage インターフェースを使用して、カスタマイズされた共有ドキュメントの
    /// ストレージタイプを実装します
    /// </summary>
    public interface ICollaborationStorage
    {

        /// <summary>
        /// ストレージからデータを読み込むためのメソッド。
        /// 与えられたキーのデータが存在しない場合は、NULLを返す必要があります。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<byte[]> ReadData(string key);

        /// <summary>
        /// ストレージにデータを書き込むためのメソッド。
        /// データがNULLの場合は、与えられたキーの古いデータを削除する必要があるので注意してください。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        Task WriteData(string key, byte[] data);
    }
}
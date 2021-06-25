using SupportApi.Collaboration.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Collaboration
{

    /// <summary>
    /// コラボレーションストレージの設定
    /// </summary>
    public class SharedStorageSettings
    {

        public SharedStorageSettings()
        {

        }

        /// <summary>
        /// メモリストレージを使用（デフォルト）
        /// </summary>
        /// <param name="lifeTimeHours"></param>
        public void UseMemory(double lifeTimeHours = 8)
        {
            var sharedStorage = SharedDocumentsStorage.Instance();
            sharedStorage.SetStorage(null);
            sharedStorage.LifeTimeHours = lifeTimeHours;
        }

        /// <summary>
        /// ファイルシステムストレージを使用
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="lifeTimeHours"></param>
        public void UseFileSystem(string directoryPath, double lifeTimeHours = 0)
        {
            var sharedStorage = SharedDocumentsStorage.Instance();
            sharedStorage.SetStorage(new FileSystemStorage(directoryPath));
            sharedStorage.LifeTimeHours = lifeTimeHours;
        }

        /// <summary>
        /// カスタマイズされたコラボレーションストレージタイプを使用
        /// </summary>
        /// <param name="customStorage"></param>
        public void UseCustomStorage(ICollaborationStorage customStorage, double lifeTimeHours = 0)
        {
            var sharedStorage = SharedDocumentsStorage.Instance();
            sharedStorage.SetStorage(customStorage);
            sharedStorage.LifeTimeHours = lifeTimeHours;
        }

    }

}

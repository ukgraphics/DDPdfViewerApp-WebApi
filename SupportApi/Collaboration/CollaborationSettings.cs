using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Collaboration
{
    /// <summary>
    /// コラボレーションモードのオプション
    /// </summary>
    public class CollaborationSettings
    {

        /// <summary>
        /// 共有ストレージのオプション
        /// </summary>
        public SharedStorageSettings Storage { get; private set; } = new SharedStorageSettings();

        /// <summary>
        /// クライアント接続を破棄する前に待機する時間の間隔
        /// デフォルトの時間間隔は30分
        /// </summary>
        public TimeSpan WaitForReconnectTime { get; internal set; } = new TimeSpan(0, 30, 0);
    }
}

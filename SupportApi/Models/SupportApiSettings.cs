using GrapeCity.Documents.Pdf;
using SupportApi.Collaboration;
using SupportApi.Localization;
using SupportApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SupportApi.Models
{
    public class SupportApiSettings
    {
        /// <summary>
        /// 墨消し実行中に使用する追加のオプション
        /// </summary>
        public RedactOptions RedactOptions { get; set; }

        /// <summary>
        /// デジタル署名の生成に使用される証明書を取得または設定
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        public CollaborationSettings Collaboration { get; private set; } = new CollaborationSettings();

        /// <summary>
        /// 利用可能なすべてのユーザー名のリストを取得または設定
        /// リスト内のユーザ名は、[ドキュメントの共有]のダイアログボックスに表示されます。
        /// </summary>
        public List<string> AvailableUsers { get; private set; } = new List<string>();

        public ErrorMessages ErrorMessages { get; private set; } = Localizer.GetErrorMessages();


        /// <summary>
        /// このイベントは、クライアントがSupportAPIのメソッドにアクセスしたときに発生します。
        /// クライアントから渡された認証トークンを確認するために使用されるイベントです。
        /// </summary>
        public event VerifyTokenEventHandler VerifyToken
        {
            // 入力デリゲートをコレクションに追加
            add
            {
                verifyTokenEventDelegates.AddHandler(verifyTokenEventKey, value);
            }
            // コレクションから入力デリゲートを削除
            remove
            {
                verifyTokenEventDelegates.RemoveHandler(verifyTokenEventKey, value);
            }
        }

        internal void OnVerifyToken(VerifyTokenEventArgs e)
        {
            VerifyTokenEventHandler eventDelegate = (VerifyTokenEventHandler)verifyTokenEventDelegates[verifyTokenEventKey];
            if (eventDelegate != null)
            {
                eventDelegate(this, e);
            }
        }

        static readonly object verifyTokenEventKey = new object();
        protected EventHandlerList verifyTokenEventDelegates = new EventHandlerList();
    }

    public delegate void VerifyTokenEventHandler(Object sender, VerifyTokenEventArgs e);
}

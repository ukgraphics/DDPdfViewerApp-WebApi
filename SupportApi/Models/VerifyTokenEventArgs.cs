using System;

#if WEB_FORMS
using System.Web.Http.Controllers;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif

namespace SupportApi.Models
{

    /// <summary>
    /// このイベントは、クライアントがSupportAPIのメソッドにアクセスしたときに発生します。
    /// </summary>
    public class VerifyTokenEventArgs : EventArgs
    {

#if WEB_FORMS
        public VerifyTokenEventArgs(HttpControllerContext controllerContext, string token, string actionName): base()
#else
        public VerifyTokenEventArgs(ControllerContext controllerContext, string token, string actionName) : base()
#endif        
        {
            ControllerContext = controllerContext;
            Token = token;
            ActionName = actionName;
            Reject = false;
        }

#if WEB_FORMS
        public HttpControllerContext ControllerContext { get; set; }
#else
        public ControllerContext ControllerContext { get; set; }
#endif


        /// <summary>
        /// クライアントから渡されたトークン文字列を取得
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// クライアントがアクセスしているSupportAPIのメソッド名を取得
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// トークン文字列の検証に失敗した場合、Rejectプロパティをtrueに設定
        /// </summary>
        public bool Reject { get; set; }

        

    }
}

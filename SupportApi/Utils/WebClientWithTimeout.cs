using System;
using System.Net;

namespace SupportApi.Utils
{
    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 15000;
            return wr;
        }

    }

}

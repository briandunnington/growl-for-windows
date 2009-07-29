using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Growl
{
    public class WebClientEx : WebClient
    {
        static WebClientEx()
        {
            //ServicePointManager.MaxServicePointIdleTime = 1000;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.MaxIdleTime = 1000;
            return request;
        }
    }
}

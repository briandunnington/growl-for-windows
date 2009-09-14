using System;
using System.Collections.Generic;
using System.Configuration;
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
            // handle proxy stuff here - by default, we will auto-detect and use any IE settings and also read any <defaultProxy> settings (including custom address and port)
            this.Proxy = WebRequest.DefaultWebProxy;

            // only override proxy address if specified in the .config file
            string proxyAddress = ConfigurationManager.AppSettings["ProxyAddress"];
            if (!String.IsNullOrEmpty(proxyAddress))
            {
                this.Proxy = new WebProxy(proxyAddress, true);
            }

            // only set credentials if specified in the .config file
            NetworkCredential credentials = null;
            string username = ConfigurationManager.AppSettings["ProxyUsername"];
            string password = ConfigurationManager.AppSettings["ProxyPassword"];
            string domain = ConfigurationManager.AppSettings["ProxyDomain"];
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                credentials = new NetworkCredential(username, password, domain);
                this.Proxy.Credentials = credentials;
            }

            // log some stuff
            string proxyInfo = String.Format("No proxy required to access '{0}'", address.ToString());
            string proxyAuthInfo = null;
            bool isByPassed = this.Proxy.IsBypassed(address);
            if (!isByPassed)
            {
                Uri proxyUri = this.Proxy.GetProxy(address);
                proxyInfo = String.Format("Proxy required to access '{0}' - using proxy at '{1}'", address.ToString(), proxyUri.ToString());

                proxyAuthInfo = "Proxy authentication not required or is using default credentials";
                if (credentials != null)
                    proxyAuthInfo = String.Format("Proxy authentication required - using username '{0}' and domain '{1}'", credentials.UserName, credentials.Domain);
            }
            Utility.WriteDebugInfo(proxyInfo);
            if(!String.IsNullOrEmpty(proxyAuthInfo)) Utility.WriteDebugInfo(proxyAuthInfo);

            // deal with a bug related to connections expiring at different times on the client and server
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.MaxIdleTime = 1000;
            return request;
        }
    }
}

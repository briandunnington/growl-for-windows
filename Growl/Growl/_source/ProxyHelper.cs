using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Net;

namespace Growl
{
    public static class ProxyHelper
    {
        public static void SetProxy()
        {
            try
            {
                // handle proxy stuff here - by default, we will auto-detect and use any IE settings and also read any <defaultProxy> settings (including custom address and port)
                IWebProxy proxy = WebRequest.DefaultWebProxy;

                // only override proxy address if specified in the .config file
                string proxyAddress = ConfigurationManager.AppSettings["ProxyAddress"];
                if (!String.IsNullOrEmpty(proxyAddress))
                {
                    proxy = new WebProxy(proxyAddress, true);
                }

                // only set credentials if specified in the .config file
                NetworkCredential credentials = null;
                string username = ConfigurationManager.AppSettings["ProxyUsername"];
                string password = ConfigurationManager.AppSettings["ProxyPassword"];
                string domain = ConfigurationManager.AppSettings["ProxyDomain"];
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    credentials = new NetworkCredential(username, password, domain);
                    proxy.Credentials = credentials;
                }

                // update the default proxy information
                WebRequest.DefaultWebProxy = proxy;
            }
            catch
            {
                //TODO: log this?
            }
        }
    }
}

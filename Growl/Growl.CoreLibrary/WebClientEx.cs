using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// A specialized <see cref="WebClient"/> class that automatically handles using 
    /// the user's GfW proxy settings and logging the appropriate debug information.
    /// </summary>
    /// <remarks>
    /// Plugin developers that require making calls to websites/services should use this
    /// class in place of the standard <see cref="WebClient"/> to ensure that the user's 
    /// proxy settings are honored.
    /// 
    /// For developers using the lower-level HttpWebRequest class, the default 
    /// <see cref="System.Net.HttpWebRequest"/> is already configured with the user's 
    /// proxy settings so no special class is required.
    /// </remarks>
    public class WebClientEx : WebClient
    {
        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webrequest = base.GetWebRequest(address);

            // log some proxy-related stuff
            string proxyInfo = String.Format("No proxy required to access '{0}'", address.ToString());
            string proxyAuthInfo = null;
            bool isByPassed = this.Proxy.IsBypassed(address);
            if (!isByPassed)
            {
                Uri proxyUri = this.Proxy.GetProxy(address);
                proxyInfo = String.Format("Proxy required to access '{0}' - using proxy at '{1}'", address.ToString(), proxyUri.ToString());

                proxyAuthInfo = "Proxy authentication not required or is using default credentials";
                if (webrequest.Proxy.Credentials != null)
                {
                    NetworkCredential credentials = webrequest.Proxy.Credentials as NetworkCredential;
                    if (credentials != null)
                        proxyAuthInfo = String.Format("Proxy authentication required - using username '{0}' and domain '{1}'", credentials.UserName, credentials.Domain);
                }
            }
            Growl.CoreLibrary.DebugInfo.WriteLine(proxyInfo);
            if (!String.IsNullOrEmpty(proxyAuthInfo)) Growl.CoreLibrary.DebugInfo.WriteLine(proxyAuthInfo);

            // deal with a bug related to connections expiring at different times on the client and server
            HttpWebRequest request = webrequest as HttpWebRequest;
            if (request != null)
            {
                request.KeepAlive = false;
                request.ServicePoint.MaxIdleTime = 1000;
                request.ServicePoint.Expect100Continue = false; // specifically, this is required for Twitter forwarding, but is useful for other things as well
                //request.ProtocolVersion = HttpVersion.Version10;  // we cant use this with 'Transfer-Encoding: chunked' =(
            }

            return webrequest;
        }
    }
}

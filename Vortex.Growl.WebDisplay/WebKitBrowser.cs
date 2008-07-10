using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Vortex.Growl.WebDisplay
{
    public class WebKitBrowser : WebKit.WebKitBrowser
    //public class WebKitBrowser : WebBrowser
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return createParams;
            }
        }

        /*
        public void SetHtml(string html, string baseUrl)
        {
            this.DocumentText = html;
        }

        public string UserAgent
        {
            get
            {
                return null;
            }
            set
            {
                // nothing
            }
        }
         * */
    }
}

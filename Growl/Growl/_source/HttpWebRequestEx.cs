using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace Growl
{
    public class HttpWebRequestHelper
    {
        public delegate void AsyncResponseCallback(HttpWebRequest request, HttpWebResponse response, object state);

        private HttpWebRequest request;
        private bool busy;

        public HttpWebRequestHelper(HttpWebRequest request)
        {
            this.request = request;
        }

        public void GetResponseAsync(AsyncResponseCallback callback, object state)
        {
            if (!this.busy)
            {
                this.busy = true;
                HttpWebRequestState internalState = new HttpWebRequestState(this.request, callback, state);
                ThreadPool.QueueUserWorkItem(DoGetResponse, internalState);
            }
        }

        private void DoGetResponse(object obj)
        {
            HttpWebRequestState internalState = (HttpWebRequestState)obj;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)internalState.Request.GetResponse();
            }
            catch
            {
                // this catches time-outs and other stuff like that
            }
            if (internalState.Callback != null)
            {
                internalState.Callback(internalState.Request, response, internalState.State);
            }
            this.busy = false;
        }

        private class HttpWebRequestState
        {
            public HttpWebRequestState(HttpWebRequest request, AsyncResponseCallback callback, object state)
            {
                this.Request = request;
                this.Callback = callback;
                this.State = state;
            }

            public HttpWebRequest Request;
            public AsyncResponseCallback Callback;
            public object State;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Growl
{
    public class AsyncResult : IDisposable, IAsyncResult
    {
        AsyncCallback callback;

        object state;

        ManualResetEvent waitHandle;

        public AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.waitHandle = new ManualResetEvent(false);
        }

        public void Complete()
        {
            try
            {
                waitHandle.Set();
                if (null != callback)
                    callback(this);
            }
            catch
            { }
        }

        public void Validate()
        {
            if (null == waitHandle)
                throw new InvalidOperationException();
        }

        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        public AsyncCallback Callback
        {
            get
            {
                return this.callback;
            }
        }

        public ManualResetEvent AsyncWaitHandle
        {
            get
            {
                return waitHandle;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                return this.AsyncWaitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return waitHandle.WaitOne(0, false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (null != waitHandle)
            {
                waitHandle.Close();
                waitHandle = null;
            }
        }
    }
}

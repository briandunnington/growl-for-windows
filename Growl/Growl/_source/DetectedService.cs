using System;
using System.Collections.Generic;
using System.Text;
using ZeroconfService;

namespace Growl
{
    public class DetectedService : IDisposable
    {
        private NetService service;
        private ForwardDestinationPlatformType platform;

        public DetectedService(NetService service, ForwardDestinationPlatformType platform)
        {
            this.service = service;
            this.platform = platform;
        }

        public NetService Service
        {
            get
            {
                return this.service;
            }
        }

        public ForwardDestinationPlatformType Platform
        {
            get
            {
                return this.platform;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (this.service != null) this.service.Dispose();
                }
                catch
                {
                    // suppress
                }
            }
        }

        #endregion
    }
}

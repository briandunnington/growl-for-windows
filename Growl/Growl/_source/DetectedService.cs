using System;
using System.Collections.Generic;
using System.Text;
using Mono.Zeroconf;

namespace Growl
{
    public class DetectedService : IDisposable
    {
        private IResolvableService service;
        private ForwardDestinationPlatformType platform;

        public DetectedService(IResolvableService service, ForwardDestinationPlatformType platform)
        {
            this.service = service;
            this.platform = platform;
        }

        public IResolvableService Service
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

        public string Hostname
        {
            get
            {
                return GetHostname(this.service);
            }
        }

        public static string GetHostname(IResolvableService service)
        {
            string hostname = null;

            if (service != null)
            {
                hostname = service.HostTarget;

                if (String.IsNullOrEmpty(hostname))
                {
                    if (service.HostEntry != null)
                    {
                        hostname = service.HostEntry.HostName;

                        if (String.IsNullOrEmpty(hostname))
                        {
                            if (service.HostEntry.AddressList != null && service.HostEntry.AddressList.Length > 0)
                            {
                                hostname = service.HostEntry.AddressList[0].ToString();
                            }
                        }
                    }
                }
            }

            return hostname;
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

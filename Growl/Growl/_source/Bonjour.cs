using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mono.Zeroconf;

namespace Growl
{
    /// <summary>
    /// Provides methos for browsing advertised services.
    /// </summary>
    public class Bonjour : IDisposable
    {
        public delegate void NetServiceFoundEventHandler(Bonjour sender, IResolvableService service, GrowlBonjourEventArgs args);
        public delegate void NetServiceRemovedEventHandler(Bonjour sender, IResolvableService service);
        public event NetServiceFoundEventHandler ServiceFound;
        public event NetServiceRemovedEventHandler ServiceRemoved;

        private static bool isSupported;


        /// <summary>
        /// Indicates if the Bonjour service browser is started
        /// </summary>
        private bool isStarted;

        /// <summary>
        /// The service browser that monitors for other bonjour services
        /// </summary>
        private ServiceBrowser serviceBrowser;

        /// <summary>
        /// A list of other bonjour services found
        /// </summary>
        private Dictionary<string, DetectedService> servicesFound = new Dictionary<string, DetectedService>();

        static Bonjour()
        {
            // since we are providing our own mDNS, Bonjour is always supported
            isSupported = true;
        }


        /// <summary>
        /// Starts the service browser that monitors for other Bonjour services.
        /// </summary>
        public void Start()
        {
            if(isSupported && !this.isStarted)
            {
                try
                {
                    this.serviceBrowser = new ServiceBrowser();
                    this.serviceBrowser.ServiceAdded += new ServiceBrowseEventHandler(serviceBrowser_ServiceAdded);
                    this.serviceBrowser.ServiceRemoved += new ServiceBrowseEventHandler(serviceBrowser_ServiceRemoved);
                    this.serviceBrowser.Browse(Growl.Daemon.GrowlServer.BONJOUR_SERVICE_TYPE, null);
                    this.isStarted = true;
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Bonjour service browser not started - {0}", ex.Message));
                    isStarted = false;
                }
            }
        }

        void serviceBrowser_ServiceAdded(object o, ServiceBrowseEventArgs args)
        {
            IResolvableService service = args.Service;
            Console.WriteLine(String.Format("Bonjour service detected: {0}", service.Name));

            // check if we simply found ourself or another service
            bool isSelf = IsOwnInstance(service);
            if (!isSelf)
            {
                Console.WriteLine(String.Format("Bonjour Growl service detected: {0}", service.Name));

                service.Resolved += new ServiceResolvedEventHandler(service_Resolved);
                service.Resolve();
            }
        }

        void service_Resolved(object o, ServiceResolvedEventArgs args)
        {
            IResolvableService service = args.Service;

            ForwardDestinationPlatformType fcPlatform = ForwardDestinationPlatformType.Other;
            if (service.TxtRecord != null)
            {
                foreach (TxtRecordItem record in service.TxtRecord)
                {
                    if (record.Key == "platform")
                    {
                        string platform = record.ValueString;
                        fcPlatform = ForwardDestinationPlatformType.FromString(platform);
                        break;
                    }
                }
            }
            GrowlBonjourEventArgs e = new GrowlBonjourEventArgs(fcPlatform);

            this.OnServiceFound(service, e);
        }

        void serviceBrowser_ServiceRemoved(object o, ServiceBrowseEventArgs args)
        {
            IResolvableService service = args.Service;

            Console.WriteLine("Bonjour service removed: {0}", service.Name);

            service.Resolved -= service_Resolved;

            this.OnServiceRemoved(args.Service);
        }

        /// <summary>
        /// Stops monitoring for other Bonjour services.
        /// </summary>
        public void Stop()
        {
            if (this.isStarted)
            {
                if (this.serviceBrowser != null)
                {
                    this.serviceBrowser.ServiceAdded -= serviceBrowser_ServiceAdded;
                    this.serviceBrowser.ServiceRemoved -= serviceBrowser_ServiceRemoved;
                    this.serviceBrowser.Dispose();
                    this.serviceBrowser = null;
                }
                this.isStarted = false;
            }
        }

        /// <summary>
        /// Indicates if the Bonjour monitor has been started.
        /// </summary>
        /// <value>
        /// <c>true</c> if the monitor has been started,
        /// <c>false</c> otherwise
        /// </value>
        public bool IsStarted
        {
            get
            {
                return isStarted;
            }
        }

        /// <summary>
        /// Indicates if Bonjour is supported on the current platform
        /// </summary>
        /// <value>
        /// <c>true</c> if Bonjour is supported and available on the current platform,
        /// <c>false</c> otherwise (usually due to not being installed and/or started)
        /// </value>
        public static bool IsSupported
        {
            get
            {
                return isSupported;
            }
        }

        public Dictionary<string, DetectedService> ServicesAvailable
        {
            get
            {
                return servicesFound;
            }
        }

        protected void OnServiceFound(IResolvableService service, GrowlBonjourEventArgs args)
        {
            if (!servicesFound.ContainsKey(service.Name))
            {
                DetectedService ds = new DetectedService(service, args.Platform);
                servicesFound.Add(service.Name, ds);
            }
            if (this.ServiceFound != null)
            {
                this.ServiceFound(this, service, args);
            }
        }

        protected void OnServiceRemoved(IResolvableService service)
        {
            servicesFound.Remove(service.Name);
            if (this.ServiceRemoved != null)
            {
                this.ServiceRemoved(this, service);
            }
        }

        public Dictionary<string, DetectedService> GetAvailableServices(List<string> excludeFilter)
        {
            Dictionary<string, DetectedService> services = new Dictionary<string, DetectedService>();
            foreach (DetectedService ds in this.ServicesAvailable.Values)
            {
                if (!excludeFilter.Contains(ds.Service.Name))
                {
                    services.Add(ds.Service.Name, ds);
                }
            }
            return services;
        }

        private static bool IsOwnInstance(IResolvableService service)
        {
            if (service.Name == Growl.Daemon.GrowlServer.BonjourServiceName)
                return true;
            else
                return false;
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
                    Stop();
                    if (this.serviceBrowser != null) this.serviceBrowser.Dispose();
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

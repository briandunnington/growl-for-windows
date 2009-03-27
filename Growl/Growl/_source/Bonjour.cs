using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ZeroconfService;

namespace Growl
{
    /// <summary>
    /// Provides methos for browsing advertised services.
    /// </summary>
    public class Bonjour
    {
        public delegate void NetServiceFoundEventHandler(Bonjour sender, NetService service, GrowlBonjourEventArgs args);
        public delegate void NetServiceRemovedEventHandler(Bonjour sender, NetService service);
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
        private NetServiceBrowser serviceBrowser;

        /// <summary>
        /// A list of other bonjour services found
        /// </summary>
        private Dictionary<string, DetectedService> servicesFound = new Dictionary<string, DetectedService>();

        static Bonjour()
        {
            // we need to determine if Bonjour is available. this is kind of a crappy
            // way to do it, but there is no other way to know for sure
            try
            {
                ZeroconfService.NetServiceBrowser nsb = new NetServiceBrowser();
                int version = nsb.GetVersion(); // we dont need the version, but this is a quick way to check if the necessary bonjour files are available
                isSupported = true;
            }
            catch
            {
                Console.WriteLine("Bonjour is not supported");
                isSupported = false;
            }
        }


        /// <summary>
        /// Starts the service browser that monitors for other Bonjour services.
        /// </summary>
        public void Start()
        {
            if(!this.isStarted)
            {
                try
                {
                    this.serviceBrowser = new NetServiceBrowser();
                    this.serviceBrowser.AllowMultithreadedCallbacks = true;
                    this.serviceBrowser.DidFindService += new NetServiceBrowser.ServiceFound(serviceBrowser_DidFindService);
                    this.serviceBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(serviceBrowser_DidRemoveService);
                    this.serviceBrowser.SearchForService(Growl.Daemon.GrowlServer.BONJOUR_SERVICE_TYPE, Growl.Daemon.GrowlServer.BONJOUR_SERVICE_DOMAIN);
                    this.isStarted = true;
                }
                catch(DNSServiceException)
                {
                    isSupported = false;
                }
            }
        }

        /// <summary>
        /// Stops monitoring for other Bonjour services.
        /// </summary>
        public void Stop()
        {
            if (this.isStarted)
            {
                if (this.serviceBrowser != null) this.serviceBrowser.Stop();
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

        protected void OnServiceFound(NetService service, GrowlBonjourEventArgs args)
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

        protected void OnServiceRemoved(NetService service)
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

        void serviceBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            Console.WriteLine(String.Format("Bonjour service detected - trying to resolve : {0}", service.Name));

            service.DidResolveService += new NetService.ServiceResolved(service_DidResolveService);
            service.StartMonitoring();
            service.ResolveWithTimeout(5);
            // DONT fire ServiceFound here, we do that in DidResolveService
        }

        void service_DidResolveService(NetService service)
        {
            Console.WriteLine(String.Format("Bonjour service resolved: {0}", service.Name));

            bool isSelf = IsOwnInstance(service);

            ForwardComputerPlatformType fcPlatform = ForwardComputerPlatformType.Other;
            IDictionary dict = ConvertTXTRecordToDictionary(service);
            if (dict != null)
            {
                if (dict.Contains("platform"))
                {
                    byte[] bytes = (byte[])dict["platform"];
                    string platform = Encoding.UTF8.GetString(bytes);
                    fcPlatform = ForwardComputerPlatformType.FromString(platform);
                }
            }

            // otherwise, this is a different service, so lets keep track of it
            if (!isSelf)
            {
                Console.WriteLine(String.Format("Bonjour Growl service detected: {0} - {1}", service.Name, "XXX"));

                GrowlBonjourEventArgs args = new GrowlBonjourEventArgs(fcPlatform);

                this.OnServiceFound(service, args);
            }

            /*
            // NOTE: this is debug info only - not needed
            if (service.Addresses != null)
            {
                foreach (System.Net.IPEndPoint ep in service.Addresses)
                {
                    Console.WriteLine(ep.ToString());
                }
            }
             * */
        }

        void serviceBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            Console.WriteLine("Bonjour service removed: {0}", service.Name);

            this.OnServiceRemoved(service);
            service.Dispose();
        }

        private static bool IsOwnInstance(NetService service)
        {
            if (service.Name == Growl.Daemon.GrowlServer.BonjourServiceName)
                return true;
            else
                return false;
        }

        private static IDictionary ConvertTXTRecordToDictionary(NetService service)
        {
            IDictionary dict = null;

            if (service != null && service.TXTRecordData != null)
            {
                byte[] txt = service.TXTRecordData;
                dict = NetService.DictionaryFromTXTRecordData(txt);
            }

            return dict;
        }
    }
}

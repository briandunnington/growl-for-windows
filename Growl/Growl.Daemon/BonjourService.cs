using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ZeroconfService;

namespace Growl.Daemon
{
    /// <summary>
    /// Provides methods for advertising a service via Bonjour
    /// </summary>
    public class BonjourService : IDisposable
    {
        private static bool isSupported;

        /// <summary>
        /// The default domain to search
        /// </summary>
        private const string DOMAIN = "";

        /// <summary>
        /// The TXT dictionary key that holds the GUID
        /// </summary>
        private const string GUID_KEY = "guid";

        /// <summary>
        /// The service name
        /// </summary>
        private string serviceName;

        /// <summary>
        /// The service type
        /// </summary>
        private string serviceType;

        /// <summary>
        /// Indicates if the bonjour service advertising this server is started
        /// </summary>
        private bool isStarted;

        /// <summary>
        /// The unique id of this instance of the service
        /// </summary>
        private string guid;

        /// <summary>
        /// The service that advertises this server
        /// </summary>
        private NetService service;

        static BonjourService()
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
        /// Creates a new instance of the BonjourService class
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <param name="serviceType">The service type</param>
        internal BonjourService(string serviceName, string serviceType)
        {
            this.guid = System.Guid.NewGuid().ToString();
            this.serviceName = serviceName;
            this.serviceType = serviceType;
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

        /// <summary>
        /// Starts the Bonjour service that advertises this server
        /// </summary>
        /// <param name="port">The port the actual server is running on</param>
        internal void Start(int port)
        {
            if(!isStarted)
            {
                try
                {
                    this.service = Publish(DOMAIN, this.serviceType, this.serviceName, port);
                    this.isStarted = true;
                }
                catch(DNSServiceException)
                {
                    this.isStarted = false;
                }
            }
        }

        /// <summary>
        /// Stops advertising this server, and stops monitoring for other Bonjour services.
        /// </summary>
        internal void Stop()
        {
            if (this.isStarted)
            {
                if (this.service != null) this.service.Stop();
            }
        }

        /// <summary>
        /// Indicates if the Bonjour services that advertise this server have been started.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service has been started,
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
        /// The unique id of this instance of the service
        /// </summary>
        /// <value>guid string</value>
        public string GUID
        {
            get
            {
                return this.guid;
            }
        }

        /// <summary>
        /// Publishes the service information to the domain
        /// </summary>
        /// <param name="domain">The domain to publish to</param>
        /// <param name="type">The service type</param>
        /// <param name="name">The service name</param>
        /// <param name="port">The port being advertised</param>
        /// <returns><see cref="NetService"/></returns>
        private NetService Publish(string domain, string type, string name, int port)
        {
            NetService service = new NetService(domain, type, name, port);
            service.AllowMultithreadedCallbacks = true;

            System.Collections.Hashtable dict = new System.Collections.Hashtable();
            dict.Add("txtvers", "1");
            dict.Add(GUID_KEY, guid);
            dict.Add("platform", "windows");
            service.setTXTRecordData(NetService.DataFromTXTRecordDictionary(dict));

            service.Publish();
            return service;
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

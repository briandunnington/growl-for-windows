using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
        //private Network.Bonjour.Service service;
        private Mono.Zeroconf.RegisterService service;

        static BonjourService()
        {
            // since we are providing our own mDNS support, Bonjour is always available
            isSupported = true;
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
            if(isSupported && !isStarted)
            {
                try
                {
                    this.service = Publish(DOMAIN, this.serviceType, this.serviceName, port);

                    this.isStarted = true;
                }
                catch(Exception ex)
                {
                    Growl.CoreLibrary.DebugInfo.WriteLine(String.Format("Bonjour service not published - {0}", ex.Message));
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
                if (this.service != null) this.service.Dispose();
                this.service = null;
                this.isStarted = false;
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
        /// <returns><see cref="Mono.Zeroconf.RegisterService"/></returns>
        private Mono.Zeroconf.RegisterService Publish(string domain, string type, string name, int port)
        {
            Stop();

            Mono.Zeroconf.TxtRecord txt = new Mono.Zeroconf.TxtRecord();
            txt.Add("txtvers", "1");
            txt.Add("platform", "windows");

            Mono.Zeroconf.RegisterService s = new Mono.Zeroconf.RegisterService();
            s.Name = name;
            s.UPort = 23053;
            s.RegType = type;
            s.ReplyDomain = "";
            s.TxtRecord = txt;
            s.Register();
            return s;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Stop();
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

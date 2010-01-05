using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Growl.Connector;

namespace Growl.COM
{
    /// <summary>
    /// Provides a COM-visible interface for using the standard Growl.Connector .NET library.
    /// 
    /// After creating a new instanceof the Connector() class, you *must* call either
    /// .ConfigureLocal() or .ConfigureRemote to initialize the connection details.
    /// 
    /// If you are calling this component from VBScript or another language that does not 
    /// have strongly-typed variables, you must use the .Register2() method instead of
    /// the normal .Register()
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComSourceInterfaces(typeof(IResponseHandler))]
    public class Connector
    {
        public static int DEFAULT_PORT = Growl.Connector.GrowlConnector.TCP_PORT;

        public delegate void OKEventHandler();
        public delegate void ErrorEventHandler(Error error);
        public delegate void CallbackEventHandler(CallbackData callbackData);

        public event OKEventHandler OKReceived;
        public event ErrorEventHandler ErrorReceived;
        public event CallbackEventHandler CallbackReceived;

        GrowlConnector growl;

        public Connector()
        {
        }

        public void ConfigureLocal(string password)
        {
            this.growl = new GrowlConnector(password);
            ConfigureAll();
        }

        public void ConfigureRemote(string hostname, int port, string password)
        {
            this.growl = new GrowlConnector(password, hostname, port);
            ConfigureAll();
        }

        private void ConfigureAll()
        {
            this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

            this.growl.OKResponse += new GrowlConnector.ResponseEventHandler(growl_OKResponse);
            this.growl.ErrorResponse += new GrowlConnector.ResponseEventHandler(growl_ErrorResponse);
            this.growl.NotificationCallback += new GrowlConnector.CallbackEventHandler(growl_NotificationCallback);
        }

        void growl_ErrorResponse(Response response)
        {
            Error error = new Error(response.ErrorCode, response.ErrorDescription);
            OnError(error);
        }

        void growl_OKResponse(Response response)
        {
            OnOK();
        }

        void growl_NotificationCallback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData)
        {
            CallbackData cd = new CallbackData(callbackData);
            OnCallback(cd);
        }

        protected void OnError(Error error)
        {
            if (this.ErrorReceived != null)
            {
                this.ErrorReceived(error);
            }
        }

        protected void OnOK()
        {
            if (this.OKReceived != null)
            {
                this.OKReceived();
            }
        }

        protected void OnCallback(CallbackData callbackData)
        {
            if (this.CallbackReceived != null)
            {
                this.CallbackReceived(callbackData);
            }
        }

        public string Password
        {
            get
            {
                return this.growl.Password;
            }
            set
            {
                this.growl.Password = value;
            }
        }

        public Cryptography.HashAlgorithmType KeyHashAlgorithm
        {
            get
            {
                return this.growl.KeyHashAlgorithm;
            }
            set
            {
                this.growl.KeyHashAlgorithm = value;
            }
        }

        public Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return this.growl.EncryptionAlgorithm;
            }
            set
            {
                this.growl.EncryptionAlgorithm = value;
            }
        }

        /// <summary>
        /// This method should only be used by VBScript or other languages that do not
        /// have strongly-typed variables. The objects passed in the <paramref name="items"/>
        /// array should be NotificationType objects.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="items"></param>
        public void Register2(Application application, object[] items)
        {
            List<NotificationType> list = new List<NotificationType>();

            for (int i = 0; i < items.Length; i++)
            {
                NotificationType nt = items[i] as NotificationType;
                if (nt != null)
                {
                    list.Add(nt);
                }
            }
            NotificationType[] types = list.ToArray();
            Register(application, ref types);
        }

        /// <summary>
        /// NOTE: If you are calling this method from VBScript or another language that
        /// does not have strongly-typed variables, use Register2() instead.
        /// </summary>
        public void Register(Application application, ref NotificationType[] notificationTypes)
        {
            if (application == null)
                throw new ArgumentException("The application cannot be null", "application");
            if (notificationTypes == null || notificationTypes.Length == 0)
                throw new ArgumentException("The list of notification types to register must not be null and must contain at least one notification type.");

            Growl.Connector.Application a = application.UnderlyingApplication;
            Growl.Connector.NotificationType[] nts = new Growl.Connector.NotificationType[notificationTypes.Length];
            for(int i=0;i<notificationTypes.Length;i++)
            {
                nts[i] = notificationTypes[i].UnderlyingNotificationType;
            }
            this.growl.Register(a, nts);
        }

        public void Notify(string applicationName, Notification notification, CallbackContext callbackContext)
        {
            if (String.IsNullOrEmpty(applicationName))
                throw new ArgumentException("The application name cannot be null or empty", "applicationName");
            if (notification == null)
                throw new ArgumentException("The notification cannot be null");

            Growl.Connector.Notification n = notification.UnderlyingNotification;
            Growl.Connector.CallbackContext cbc = (callbackContext != null ? callbackContext.UnderlyingCallbackContext : null);
            this.growl.Notify(n, cbc);
        }
    }
}
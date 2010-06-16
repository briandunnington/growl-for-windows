using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Growl.Connector
{
    /// <summary>
    /// Used by applications to communicate with a Growl instance
    /// </summary>
    public class GrowlConnector : ConnectorBase
    {
        /// <summary>
        /// Represents methods that handle Growl responses
        /// </summary>
        /// <param name="response">The <see cref="Response"/> from Growl</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public delegate void ResponseEventHandler(Response response, object state);

        /// <summary>
        /// Represents methods that handle Growl callbacks
        /// </summary>
        /// <param name="response">The <see cref="Response"/> from Growl</param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public delegate void CallbackEventHandler(Response response, CallbackData callbackData, object state);

        /// <summary>
        /// Occurs when an 'OK' response is received.
        /// </summary>
        public event ResponseEventHandler OKResponse;

        /// <summary>
        /// Occurs when an 'ERROR' response is received.
        /// </summary>
        public event ResponseEventHandler ErrorResponse;

        /// <summary>
        /// Occurs when a 'CALLBACK' response is received.
        /// </summary>
        public event CallbackEventHandler NotificationCallback;


        /// <summary>
        /// Initializes a new instance of the <see cref="GrowlConnector"/> class
        /// using the default hostname and port, with no password set.
        /// </summary>
        public GrowlConnector() : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrowlConnector"/> class
        /// using the default hostname and port and the supplied password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption.</param>
        public GrowlConnector(string password)
            : base(password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrowlConnector"/> class
        /// using the supplied hostname, port, and password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption.</param>
        /// <param name="hostname">The hostname of the Growl instance to connect to.</param>
        /// <param name="port">The port of the Growl instance to connect to.</param>
        public GrowlConnector(string password, string hostname, int port)
            : base(password, hostname, port)
        {
        }

        /// <summary>
        /// Detects if Growl is currently running on the local machine.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Growl is running;
        /// <c>false</c> if Growl is not running;
        /// </returns>
        /// <remarks>
        /// This method is deprecated. Use the static IsGrowlRunningLocally() method instead.
        /// 
        /// This method only detects if Growl is running on the local machine where this assembly is running. 
        /// It does not detect if Growl is running on a remote client machine, even if the GrowlConnector instance is 
        /// configured to point to a remote machine.
        /// </remarks>
        [Obsolete("This method only detects if Growl is running on the local machine where this assembly is running. It does not detect if Growl is running on a remote client machine, even if the GrowlConnector instance is configured to point to a remote machine. Use the static IsGrowlRunningLocally() method instead.", false)]
        public bool IsGrowlRunning()
        {
            return Growl.CoreLibrary.Detector.DetectIfGrowlIsRunning();
        }

        /// <summary>
        /// Detects if Growl is currently running on the local machine.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Growl is running;
        /// <c>false</c> if Growl is not running;
        /// </returns>
        public static bool IsGrowlRunningLocally()
        {
            return Growl.CoreLibrary.Detector.DetectIfGrowlIsRunning();
        }

        /// <summary>
        /// Registers the specified application and notification types.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes)
        {
            Register(application, notificationTypes, null, null);
        }

        /// <summary>
        /// Registers the specified application and notification types.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes, object state)
        {
            Register(application, notificationTypes, null, state);
        }

        /// <summary>
        /// Registers the specified application and notification types and allows for additional request data.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes, RequestData requestData)
        {
            Register(application, notificationTypes, requestData, null);
        }

        /// <summary>
        /// Registers the specified application and notification types and allows for additional request data.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes, RequestData requestData, object state)
        {
            HeaderCollection appHeaders = application.ToHeaders();
            List<HeaderCollection> notifications = new List<HeaderCollection>();
            foreach (NotificationType notificationType in notificationTypes)
            {
                HeaderCollection notificationHeaders = notificationType.ToHeaders();
                notifications.Add(notificationHeaders);
            }

            MessageBuilder mb = new MessageBuilder(RequestType.REGISTER, this.GetKey());
            foreach(Header header in appHeaders)
            {
                mb.AddHeader(header);
            }
            mb.AddHeader(new Header(Header.NOTIFICATIONS_COUNT, notificationTypes.Length.ToString()));

            // handle any additional request data
            if (requestData != null)
            {
                HeaderCollection requestDataHeaders = requestData.ToHeaders();
                foreach (Header header in requestDataHeaders)
                {
                    mb.AddHeader(header);
                }
            }

            foreach(HeaderCollection headers in notifications)
            {
                MessageSection ms = new MessageSection();
                foreach(Header header in headers)
                {
                    ms.AddHeader(header);
                }
                mb.AddMessageSection(ms);
            }

            Send(mb, OnResponseReceived, false, state);
        }

        /// <summary>
        /// Sends a notification to Growl.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        public virtual void Notify(Notification notification)
        {
            Notify(notification, null, null);
        }

        /// <summary>
        /// Sends a notification to Growl.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Notify(Notification notification, object state)
        {
            Notify(notification, null, null, state);
        }

        /// <summary>
        /// Sends a notification to Growl and allows for additional request data.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Notify(Notification notification, RequestData requestData)
        {
            Notify(notification, null, requestData, null);
        }

        /// <summary>
        /// Sends a notification to Growl and allows for additional request data.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Notify(Notification notification, RequestData requestData, object state)
        {
            Notify(notification, null, requestData, state);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext)
        {
            Notify(notification, callbackContext, null, null);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext, object state)
        {
            Notify(notification, callbackContext, null, state);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext, RequestData requestData)
        {
            Notify(notification, callbackContext, requestData, null);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information and allows for additional request data.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext, RequestData requestData, object state)
        {
            bool waitForCallback = false;
            HeaderCollection notificationHeaders = notification.ToHeaders();
            MessageBuilder mb = new MessageBuilder(RequestType.NOTIFY, this.GetKey());
            foreach (Header header in notificationHeaders)
            {
                mb.AddHeader(header);
            }

            if (callbackContext != null)
            {
                string url = callbackContext.CallbackUrl;
                if (!String.IsNullOrEmpty(url))
                {
                    mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_TARGET, url));
                }
                else
                {
                    mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT, callbackContext.Data));
                    mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE, callbackContext.Type.ToString()));
                    waitForCallback = true;
                }
            }

            // handle any additional request data
            if (requestData != null)
            {
                HeaderCollection requestDataHeaders = requestData.ToHeaders();
                foreach (Header header in requestDataHeaders)
                {
                    mb.AddHeader(header);
                }
            }

            Send(mb, OnResponseReceived, waitForCallback, state);
        }

        /// <summary>
        /// Parses the response and raises the appropriate event
        /// </summary>
        /// <param name="responseText">The raw GNTP response</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected override void OnResponseReceived(string responseText, object state)
        {
            CallbackData cd;
            MessageParser mp = new MessageParser();
            Response response = mp.Parse(responseText, out cd);

            if (response.IsCallback)
                this.OnNotificationCallback(response, cd, state);
            else if (response.IsOK)
                this.OnOKResponse(response, state);
            else
                this.OnErrorResponse(response, state);
        }

        /// <summary>
        /// Occurs when any of the following network conditions occur:
        /// 1. Unable to connect to target host for any reason
        /// 2. Write request fails
        /// 3. Read request fails
        /// </summary>
        /// <param name="response">The <see cref="Response"/> that contains information about the failure</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected override void OnCommunicationFailure(Response response, object state)
        {
            this.OnErrorResponse(response, state);
        }

        /// <summary>
        /// Called when an 'OK' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected void OnOKResponse(Response response, object state)
        {
            if (this.OKResponse != null)
            {
                this.OKResponse(response, state);
            }
        }

        /// <summary>
        /// Called when an 'ERROR' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected void OnErrorResponse(Response response, object state)
        {
            if (this.ErrorResponse != null)
            {
                this.ErrorResponse(response, state);
            }
        }

        /// <summary>
        /// Called when an 'CALLBACK' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected void OnNotificationCallback(Response response, CallbackData callbackData, object state)
        {
            if (callbackData != null && callbackData.Data != null && this.NotificationCallback != null)
            {
                this.NotificationCallback(response, callbackData, state);
            }
        }
    }
}

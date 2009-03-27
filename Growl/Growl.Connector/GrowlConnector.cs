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
        public delegate void ResponseEventHandler(Response response);

        /// <summary>
        /// Represents methods that handle Growl callbacks
        /// </summary>
        /// <param name="response">The <see cref="Response"/> from Growl</param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        public delegate void CallbackEventHandler(Response response, CallbackData callbackData);

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
        /// Registers the specified application and notification types.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes)
        {
            Register(application, notificationTypes, null);
        }

        /// <summary>
        /// Registers the specified application and notification types and allows for additional request data.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> to register.</param>
        /// <param name="notificationTypes">The <see cref="NotificationType"/>s to register.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Register(Application application, NotificationType[] notificationTypes, RequestData requestData)
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

            Send(mb, OnResponseReceived, false);
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
        /// Sends a notification to Growl and allows for additional request data.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Notify(Notification notification, RequestData requestData)
        {
            Notify(notification, null, requestData);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext)
        {
            Notify(notification, callbackContext, null);
        }

        /// <summary>
        /// Sends a notification to Growl that specifies callback information and allows for additional request data.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> to send.</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> containing the callback information.</param>
        /// <param name="requestData">The <see cref="RequestData"/> containing the additional information.</param>
        public virtual void Notify(Notification notification, CallbackContext callbackContext, RequestData requestData)
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
                mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT, callbackContext.Data));
                mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE, callbackContext.Type.ToString()));

                UrlCallbackTarget target = callbackContext.GetUrlCallbackTarget();
                if (target != null && !String.IsNullOrEmpty(target.Url))
                {
                    mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT_TARGET, target.Url));
                    mb.AddHeader(new Header(Header.NOTIFICATION_CALLBACK_CONTEXT_TARGET_METHOD, target.Method));
                }
                waitForCallback = true;
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

            Send(mb, OnResponseReceived, waitForCallback);
        }

        /// <summary>
        /// Parses the response and raises the appropriate event
        /// </summary>
        /// <param name="responseText">The raw GNTP response</param>
        protected override void OnResponseReceived(string responseText)
        {
            CallbackData cd;
            MessageParser mp = new MessageParser();
            Response response = mp.Parse(responseText, out cd);

            if (response.IsCallback)
                this.OnNotificationCallback(response, cd);
            else if (response.IsOK)
                this.OnOKResponse(response);
            else
                this.OnErrorResponse(response);
        }

        /// <summary>
        /// Occurs when any of the following network conditions occur:
        /// 1. Unable to connect to target host for any reason
        /// 2. Write request fails
        /// 3. Read request fails
        /// </summary>
        /// <param name="response">The <see cref="Response"/> that contains information about the failure</param>
        protected override void OnCommunicationFailure(Response response)
        {
            this.OnErrorResponse(response);
        }

        /// <summary>
        /// Called when an 'OK' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        protected void OnOKResponse(Response response)
        {
            if (this.OKResponse != null)
            {
                this.OKResponse(response);
            }
        }

        /// <summary>
        /// Called when an 'ERROR' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        protected void OnErrorResponse(Response response)
        {
            if (this.ErrorResponse != null)
            {
                this.ErrorResponse(response);
            }
        }

        /// <summary>
        /// Called when an 'CALLBACK' response occurs.
        /// </summary>
        /// <param name="response">The <see cref="Response"/></param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        protected void OnNotificationCallback(Response response, CallbackData callbackData)
        {
            if (callbackData != null && callbackData.Data != null && this.NotificationCallback != null)
            {
                this.NotificationCallback(response, callbackData);
            }
        }
    }
}

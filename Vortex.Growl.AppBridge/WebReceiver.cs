using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Vortex.Growl.Framework;
using JsonConverter = System.Net.Json;

namespace Vortex.Growl.AppBridge
{
    class WebReceiver : BaseReceiver
    {
        private const int WEB_DEFAULT_PORT = 9889;
        private const string RESPONSE_RUNNING_TEXT = "eval(Growl.setStatus(true))";
        private const string RESPONSE_OK_TEXT = "true";
        private const string RESPONSE_ERROR_TEXT = "false";

        private SimpleWebServer ws;

        internal WebReceiver()
        {
            // this class is like a singleton.
            // instances should only be created by the parent AppBridge
            this.port = WEB_DEFAULT_PORT;
            this.localMessagesOnly = true;

            this.ws = new SimpleWebServer(this.port);
            this.ws.RequestReceived += new SimpleWebServer.RequestHandler(ws_RequestReceived);
        }

        private string ws_RequestReceived(string request, string receivedFrom)
        {
            string response = RESPONSE_ERROR_TEXT;
            try
            {
                // since we used a GET request, we have to scrape our data from the url
                string json = request;
                json = json.Substring(0, json.IndexOf(Environment.NewLine));
                json = json.Substring(8);
                json = json.Substring(0, json.LastIndexOf("&u="));
                json = System.Web.HttpUtility.UrlDecode(json);

                try
                {
                    if (json.Length > 1)
                    {
                        JsonConverter.JsonTextParser parser = new JsonConverter.JsonTextParser();
                        JsonConverter.JsonObject obj = parser.Parse(json);
                        if (obj is JsonConverter.JsonObjectCollection)
                        {
                            JsonConverter.JsonObjectCollection joc = (JsonConverter.JsonObjectCollection)obj;
                            JsonConverter.JsonObject jsonAction = joc["action"];
                            if (jsonAction != null)
                            {
                                string action = jsonAction.GetValue().ToString();
                                PacketType type = (PacketType)Convert.ToInt32(action);
                                string appName = joc["applicationName"].GetValue().ToString();
                                switch (type)
                                {
                                    case PacketType.Registration:
                                        List<NotificationType> notificationTypes = new List<NotificationType>();
                                        JsonConverter.JsonArrayCollection jsonNotificationTypes = (JsonConverter.JsonArrayCollection)joc["notificationTypes"];
                                        foreach (JsonConverter.JsonObjectCollection jsonNotificationType in jsonNotificationTypes)
                                        {
                                            string ntName = jsonNotificationType["name"].GetValue().ToString();
                                            bool ntEnabled = Convert.ToBoolean(jsonNotificationType["enabled"].GetValue().ToString());
                                            NotificationType notificationType = new NotificationType(ntName, ntEnabled);
                                            notificationTypes.Add(notificationType);
                                        }
                                        RegistrationPacket rp = new RegistrationPacket(1, appName, "", notificationTypes);
                                        this.OnRegistrationPacketReceived(rp, receivedFrom);
                                        response = RESPONSE_OK_TEXT;
                                        break;
                                    case PacketType.Notification:
                                        JsonConverter.JsonObjectCollection jsonNt = (JsonConverter.JsonObjectCollection)joc["notificationType"];
                                        string name = jsonNt["name"].GetValue().ToString();
                                        bool enabled = Convert.ToBoolean(jsonNt["enabled"].GetValue().ToString());
                                        NotificationType nt = new NotificationType(name, enabled);
                                        string title = joc["title"].GetValue().ToString();
                                        string description = joc["description"].GetValue().ToString();
                                        Priority priority = (Priority)Convert.ToInt32(joc["priority"].GetValue().ToString());
                                        bool sticky = Convert.ToBoolean(joc["sticky"].GetValue().ToString());
                                        NotificationPacket np = new NotificationPacket(1, appName, "", nt, title, description, priority, sticky);
                                        this.OnNotificationPacketReceived(np, receivedFrom);
                                        response = RESPONSE_OK_TEXT;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // this is a special case where a request with no data was sent.
                        // we want to let the calling code know that Growl is at least running.
                        response = RESPONSE_RUNNING_TEXT;
                    }
                }
                catch
                {
                }
            }
            catch
            {
                response = RESPONSE_ERROR_TEXT;
            }
            return response;
        }

        public override void Start()
        {
            this.ws.Start();
        }

        public override void Stop()
        {
            this.ws.Stop();
        }

        public override bool IsRunning
        {
            get
            {
                bool isRunning = false;
                if (this.ws != null)
                    isRunning = this.ws.IsRunning;
                return isRunning;
            }
        }

        protected override void OnRegistrationPacketReceived(RegistrationPacket rp, string receivedFrom)
        {
            HandlePacketReceivedInAnotherThread(rp, receivedFrom);
        }

        protected override void OnNotificationPacketReceived(NotificationPacket np, string receivedFrom)
        {
            HandlePacketReceivedInAnotherThread(np, receivedFrom);
        }

        private void HandlePacketReceivedInAnotherThread(BasePacket packet, string receivedFrom)
        {
            PacketReceivedWrapper prw = new PacketReceivedWrapper();
            prw.Packet = packet;
            prw.ReceivedFrom = receivedFrom;

            ParameterizedThreadStart pts = new ParameterizedThreadStart(this.OnPacketReceived);
            Thread t = new Thread(pts);
            t.Start(prw);
        }

        private void OnPacketReceived(object obj)
        {
            try
            {
                if (obj is PacketReceivedWrapper)
                {
                    PacketReceivedWrapper prw = (PacketReceivedWrapper)obj;
                    if (prw.Packet is RegistrationPacket)
                        base.OnRegistrationPacketReceived((RegistrationPacket)prw.Packet, prw.ReceivedFrom);
                    else if (prw.Packet is NotificationPacket)
                        base.OnNotificationPacketReceived((NotificationPacket)prw.Packet, prw.ReceivedFrom);
                }
            }
            catch
            {
                // suppress any exceptions here
            }
        }

        private struct PacketReceivedWrapper
        {
            public BasePacket Packet;
            public string ReceivedFrom;
        }
    }
}

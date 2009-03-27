using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.Daemon;

namespace Test_Server
{
    public class Forwarder : GrowlConnector
    {
        public Forwarder(string password, string hostname, int port, RequestInfo requestInfo)
            : base(password, hostname, port)
        {
            this.RequestInfo = requestInfo;
        }

        public RequestInfo RequestInfo;

        protected override bool OnBeforeSend(Growl.Connector.MessageBuilder mb)
        {
            //from <hostname> by <hostname> [with Growl] [id <identifier>]; <ISO 8601 date>

            foreach (Header header in this.RequestInfo.PreviousReceivedHeaders)
            {
                mb.AddHeader(header);
            }

            string received = String.Format("from {0} by {1}{2}{3}; {4}", this.RequestInfo.ReceivedFrom, this.RequestInfo.ReceivedBy, (this.RequestInfo.ReceivedWith != null ? String.Format(" with {0}", this.RequestInfo.ReceivedWith) : String.Empty), (this.RequestInfo.RequestID != null ? String.Format(" id {0}", this.RequestInfo.RequestID) : String.Empty), this.RequestInfo.TimeReceived.ToString("u"));
            Header receivedHeader = new Header("Received", received);
            mb.AddHeader(receivedHeader);

            string msg = mb.ToString();
            Console.WriteLine(msg);

            return base.OnBeforeSend(mb);
        }
    }
}

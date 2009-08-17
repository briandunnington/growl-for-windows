using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Connector;

namespace Test_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ExtensibleObject.SetPlatformInformation("internet", "1.0");
        }

        private Growl.Daemon.GrowlServer server;
        private bool isRunning = false;
        //string keyHex = "8917EEF78F63044182DB218FDC9715C16EE45AE1179A17A4521B0AFF559272AC";
        string password = "secret";
        string altPassword = "alternate";

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.isRunning)
            {
                this.server.Stop();
                this.isRunning = false;
                this.button3.Text = "start server";
                Write("server stopped");

                //this.pollClient.Stop();
                //this.pollServer.Stop();
            }
            else
            {
                string historyFolder = System.Windows.Forms.Application.UserAppDataPath + @"\history";

                Growl.Connector.PasswordManager pm = new Growl.Connector.PasswordManager();
                //pm.Add(password);
                pm.Add(altPassword, true);
                this.server = new Growl.Daemon.GrowlServer(24000, pm, historyFolder);
                //this.server = new Growl.Daemon.GrowlServer(Growl.Connector.GrowlConnector.TCP_PORT, pm, historyFolder);
                this.server.RegisterReceived += new Growl.Daemon.GrowlServer.RegisterReceivedEventHandler(server_RegisterReceived);
                this.server.NotifyReceived += new Growl.Daemon.GrowlServer.NotifyReceivedEventHandler(server_NotifyReceived);
                this.server.SubscribeReceived += new Growl.Daemon.GrowlServer.SubscribeReceivedEventHandler(server_SubscribeReceived);

                this.server.ServerMessage += new Growl.Daemon.GrowlServer.ServerMessageEventHandler(server_ServerMessage);

                this.server.Start();
                this.isRunning = true;
                this.button3.Text = "stop server";
                this.textBox1.Text = "";
                Write("server is listening");

                //this.pollServer = new Growl.Polling.PollServer(7777, keyHex);
                //this.pollServer.Start();

                //this.pollClient = new Growl.Polling.PollClient(keyHex, "brian@elementcodeproject.com", "localhost", 7777, 10, "localhost", Growl.Connector.GrowlConnector.TCP_PORT);
                //this.pollClient.Start();
            }
        }

        Growl.Daemon.SubscriptionResponse server_SubscribeReceived(Growl.Daemon.Subscriber subscriber, Growl.Daemon.RequestInfo requestInfo)
        {
            Growl.Daemon.SubscriptionResponse r = new Growl.Daemon.SubscriptionResponse(300);
            return r;
        }

        Response server_NotifyReceived(Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo)
        {
            Response response = new Response();

            if (callbackInfo != null)
            {
                if (callbackInfo.ShouldKeepConnectionOpen())
                {
                    response.SetCallbackData(notification.ID, callbackInfo.Context, Growl.CoreLibrary.CallbackResult.CLICK);

                    // simulate a wait
                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    string url = callbackInfo.Context.CallbackUrl;
                    server_ServerMessage(null, Growl.Daemon.GrowlServer.LogMessageType.Information, url);
                }
            }

            //Console.WriteLine("notification response");
            return response;
        }

        Response server_RegisterReceived(Growl.Connector.Application application, List<NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo)
        {
            if (requestInfo.PreviousReceivedHeaders.Count < 0)  // change to a positive number to forward
            {
                Forwarder fwd = new Forwarder(password, "127.0.0.1", Growl.Connector.GrowlConnector.TCP_PORT, requestInfo);
                fwd.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;
                fwd.Register(application, notificationTypes.ToArray());
            }

            Response response = new Response();
            return response;
        }

        void server_ServerMessage(Growl.Daemon.GrowlServer sender, Growl.Daemon.GrowlServer.LogMessageType type, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DoWrite(Write), new object[] { message });
            }
            else
            {
                Write(message);
            }
        }

        private delegate void DoWrite(string message);

        private void Write(string message)
        {
            this.textBox1.AppendText(message + "\r\n");
            System.IO.StreamWriter w = System.IO.File.AppendText("log.txt");
            w.WriteLine(message);
            w.Flush();
            w.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.server != null) this.server.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("test");
            byte[] hashBytes = md5.ComputeHash(buffer);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "");
            //string hash = System.Text.Encoding.UTF8.GetString(hashBytes);
            Console.WriteLine(hash);
             * */

            /*
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))\s+|((?<EncryptionAlgorithm>\S+)\s+))(?<KeyHashAlgorithm>(\S+)):(?<KeyHash>(\S+))\s*[\r\n]");
            string s = "GNTP/1.0 REGISTER AES:12345 MD5:12345\r\n";
            System.Text.RegularExpressions.Match m = r.Match(s);
            Console.WriteLine(m.Success);
            Console.WriteLine(m.Groups["EncryptionAlgorithm"].Value);
            Console.WriteLine(m.Groups["IV"].Value);

            s = "GNTP/1.0 REGISTER NONE MD5:12345\r\n";
            m = r.Match(s);
            Console.WriteLine(m.Success);
            Console.WriteLine(m.Groups["EncryptionAlgorithm"].Value);
            Console.WriteLine(m.Groups["IV"].Value);
             * */

            KeysConverter kc = new KeysConverter();
            object k = kc.ConvertFromString("Ctrl+Shift+A");
            Keys keys = (Keys)k;
            Console.WriteLine(keys);
            Console.WriteLine(keys == Keys.A);
            Console.WriteLine(keys == Keys.B);
            Console.WriteLine(keys == Keys.Shift);
            Console.WriteLine(keys == Keys.Control);
            Console.WriteLine(keys == (Keys.A | Keys.Shift | Keys.Control));
            Console.WriteLine(keys == (Keys.B | Keys.Shift | Keys.Control));
        }

        private string subscriberID;
        private void button2_Click(object sender, EventArgs e)
        {
            this.subscriberID = Growl.Daemon.Subscriber.GenerateID();
            Growl.Daemon.Subscriber subscriber = new Growl.Daemon.Subscriber(this.subscriberID, "Superman", 24000);
            Growl.Daemon.SubscriptionConnector sc = new Growl.Daemon.SubscriptionConnector(subscriber, password, "127.0.0.1");
            sc.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
            sc.OKResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_OKResponse);
            sc.ErrorResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_ErrorResponse);
            sc.Subscribe();
        }

        void sc_OKResponse(Growl.Daemon.SubscriptionResponse response)
        {
            Console.WriteLine("you were subscribed - TTL: " + response.TTL.ToString());
            this.server.PasswordManager.Add(password + this.subscriberID, true);
        }

        void sc_ErrorResponse(Growl.Daemon.SubscriptionResponse response)
        {
            Console.WriteLine("failed to subscribe: " + response.ErrorDescription);
            //this.server.PasswordManager.Remove(password);
        }
    }
}
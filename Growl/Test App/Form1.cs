using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Test_App
{
    public partial class Form1 : Form
    {
        private Growl.Connector.GrowlConnector growl;
        private Growl.Connector.Application app;

        public Form1()
        {
            InitializeComponent();

            growl = new GrowlConnector();
            //growl.Password = this.textBox2.Text;
            this.growl.OKResponse += new GrowlConnector.ResponseEventHandler(growl_OKResponse);
            this.growl.ErrorResponse += new GrowlConnector.ResponseEventHandler(growl_ErrorResponse);
            this.growl.NotificationCallback +=new GrowlConnector.CallbackEventHandler(growl_NotificationCallback);

            growl.KeyHashAlgorithm = Cryptography.HashAlgorithmType.SHA256;
            growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.DES;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.TripleDES;
            //growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;

            this.app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            //app.Icon = @"c:\apple.png";
            //app.Icon = Properties.Resources.Apple;
            //app.CustomTextAttributes.Add("Creator", "Apple Software");
            //app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");
            //app.CustomBinaryAttributes.Add("Sound", "http://fake.net/app.wav");
            app.Icon = @"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAALEwAACxMBAJqcGAAAAAd0SU1FB9YGARc5KB0XV+IAAAAddEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIFRoZSBHSU1Q72QlbgAAAF1JREFUGNO9zL0NglAAxPEfdLTs4BZM4DIO4C7OwQg2JoQ9LE1exdlYvBBeZ7jqch9//q1uH4TLzw4d6+ErXMMcXuHWxId3KOETnnXXV6MJpcq2MLaI97CER3N0vr4MkhoXe0rZigAAAABJRU5ErkJggg==";


            Growl.CoreLibrary.Detector detector = new Detector();
            if (detector.IsInstalled)
            {
                InvokeWrite(String.Format("Growl (v{0}; f{1}; a{2}) is installed at {3} ({4})", detector.FileVersion.ProductVersion, detector.FileVersion.FileVersion, detector.AssemblyVersion.ToString(), detector.InstallationFolder, detector.DisplaysFolder));
            }
            else
            {
                InvokeWrite("Growl is not available on this machine");
            }

            if (growl.IsGrowlRunning())
            {
                InvokeWrite("Growl is running");
            }
            else
            {
                InvokeWrite("Growl is not running");
            }
        }

        void growl_ErrorResponse(Response response, object state)
        {
            InvokeWrite("Error - " + response.ErrorCode + " : " + response.ErrorDescription);
        }

        void growl_OKResponse(Response response, object state)
        {
            if (state != null) InvokeWrite(state.ToString());

            InvokeWrite("OK - " + response.MachineName);
        }

        void growl_NotificationCallback(Response response, CallbackData callbackContext, object state)
        {
            string s = String.Format("CALLBACK RECEIVED: {0} - {1} - {2} - {3}", callbackContext.NotificationID, callbackContext.Data, callbackContext.Type, callbackContext.Result);
            InvokeWrite(s);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            System.IO.FileStream fs1 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f1.ico", System.IO.FileMode.Open);
            System.Drawing.Image i1 = System.Drawing.Bitmap.FromStream(fs1);

            System.IO.FileStream fs2 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f2.ico", System.IO.FileMode.Open);
            System.Drawing.Image i2 = System.Drawing.Bitmap.FromStream(fs2);

            System.IO.FileStream fs3 = System.IO.File.Open(@"C:\Documents and Settings\brian\Desktop\f3.ico", System.IO.FileMode.Open);
            //System.Drawing.Image i3 = System.Drawing.Bitmap.FromStream(fs3);
            Icon icon = new Icon(fs3);
            Image i3 = icon.ToBitmap();

            System.Drawing.Image i4 = System.Drawing.Bitmap.FromFile(@"C:\Documents and Settings\brian\Desktop\f3.ico");

            System.Net.WebClient wc = new System.Net.WebClient();
            using (wc)
            {
                byte[] bytes = wc.DownloadData("http://haxe.org/favicon.ico");
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes, false);
                using (ms)
                {
                    ms.Position = 0;
                    System.Drawing.Image tempImage = System.Drawing.Bitmap.FromStream(ms, false, false);
                    // dont close stream yet, first create a copy
                    using (tempImage)
                    {
                        Image image = new Bitmap(tempImage);
                        Console.WriteLine(image);
                    }
                }
            }
            */

            NotificationType nt1 = new NotificationType("Download Complete");
            //nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.Icon = "http://www.hamradio.pl/images/thumb/e/e5/Icon_48x48_star_01.png/48px-Icon_48x48_star_01.png";
            nt1.Enabled = false;
            //nt1.CustomTextAttributes.Add("Language", "English");
            //nt1.CustomTextAttributes.Add("Timezone", "PST");
            //nt1.CustomBinaryAttributes.Add("Sound", "http://fake.net/nt.wav");
            NotificationType nt2 = new NotificationType("Document Published", "Document successfully published", null, true);
            nt2.Icon = "http://coaching.typepad.com/EspressoPundit/feed-icon-legacy_blue_38.png";
            //nt2.CustomBinaryAttributes.Add("Sound", "http://fake.net/sound.wav");
            //nt2.CustomBinaryAttributes.Add("Sound-Alt", new BinaryData(new byte[] { 70, 71, 72, 73 }));

            NotificationType[] types = new NotificationType[] { nt1, nt2 };

            //Growl.Connector.RequestData rd = new RequestData();
            //rd.Add("Return-To-Me", "some text value");
            //rd.Add("Return-To-Me2", "another value");
            Growl.Connector.RequestData rd = null;

            try
            {
                growl.Register(this.app, types, rd, "some_user_state_data");
                this.textBox1.Text = "REGISTER sent";
            }
            catch (Exception ex)
            {
                this.textBox1.Text = ex.Message;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            app.Icon = @"c:\apple.png";
            app.CustomTextAttributes.Add("Creator", "Apple Software");
            app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");

            NotificationType nt1 = new NotificationType("Download Complete", "Download completed");
            //nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.CustomTextAttributes.Add("Language", "English");
            nt1.CustomTextAttributes.Add("Timezone", "PST");

            Notification notification = new Notification(app.Name, nt1.Name, "123456", "\u2065 Your docu;ment\nwas publi&shed", "File 'c:\\file.txt' was successfully published at 8:57pm.\n\nClick this notification to open the file.\n\nThis is a test of the expanding displays.");
            notification.Sticky = false;
            notification.Priority = Priority.Emergency;
            notification.Icon = "http://atomicbride.com/Apple.gif";
            //notification.Icon = "http://haxe.org/favicon.ico";
            notification.CustomTextAttributes.Add("Filename", @"c:\file.txt");
            notification.CustomTextAttributes.Add("Timestamp", "8:57pm");
            notification.CustomBinaryAttributes.Add("File", new BinaryData(new byte[] { 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78 }));
            notification.CoalescingID = "secretfaketest";

            //string url = "http://localhost/growl-callback.aspx";
            //string url = "mailto:brian@elementcodeproject.com";
            string url = "itpc:http://www.npr.org/rss/podcast.php?id=35";
            CallbackContext callback = new CallbackContext(url);

            Growl.Connector.RequestData rd = new RequestData();
            rd.Add("Return-To-Me", "some text value");
            rd.Add("Return-To-Me2", "this is some longer value, including a \n few \n line \n breaks");
            rd.Add("Can I have spaces?", "dont know");

            growl.Notify(notification, callback, rd);
            this.textBox1.Text = "NOTIFY sent";
        }

        private System.Timers.Timer timer;

        private void button4_Click(object sender, EventArgs e)
        {
            /*
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");

            string s = growl.Status(app);
            this.textBox1.Text = s;
             * */

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }
            else
            {
                this.timer = new System.Timers.Timer(5 * 1000);
                this.timer.AutoReset = true;
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                this.timer.Start();
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Growl.Connector.Notification notification = new Notification(this.app.Name, "Download Complete", String.Empty, "fake", "fake");
            this.growl.Notify(notification);
        }

        void InvokeWrite(string response)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DoWrite(Write), new object[] { response });
            }
            else
            {
                Write(response);
            }
        }

        private delegate void DoWrite(string message);

        private void Write(string message)
        {
            this.textBox1.Text = message;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Growl.Connector.Application app = new Growl.Connector.Application("SurfWriter");
            //app.Icon = "http://atomicbride.com/Apple.gif";
            //app.Icon = "http://www.thetroyers.com/images/Apple_Logo.jpg";
            app.Icon = @"c:\apple.png";
            app.CustomTextAttributes.Add("Creator", "Apple Software");
            app.CustomTextAttributes.Add("Application-ID", "08d6c05a21512a79a1dfeb9d2a8f262f");

            NotificationType nt1 = new NotificationType("Download Complete", "Download completed");
            nt1.Icon = new BinaryData(new byte[] { 65, 66, 67, 68 });
            nt1.CustomTextAttributes.Add("Language", "English");
            nt1.CustomTextAttributes.Add("Timezone", "PST");

            Notification notification = new Notification(app.Name, nt1.Name, "123456", "\u2605 Your document was published", @"File 'c:\file.txt' was successfully published at 8:57pm.");
            notification.Sticky = false;
            notification.Priority = Priority.Emergency;
            //notification.Icon = "http://atomicbride.com/Apple.gif";
            app.Icon = @"c:\apple.png";
            notification.CustomTextAttributes.Add("Filename", @"c:\file.txt");
            notification.CustomTextAttributes.Add("Timestamp", "8:57pm");
            notification.CustomBinaryAttributes.Add("File", new BinaryData(new byte[] { 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78, 78 }));

            string data = "this is my context\nthis is after a line break";
            string type = typeof(string).ToString();
            CallbackContext context = new CallbackContext(data, type);

            Growl.Connector.RequestData rd = new RequestData();
            rd.Add("Return-To-Me", "some text value");
            rd.Add("Return-To-Me2", "another value");

            growl.Notify(notification, context, rd);
            this.textBox1.Text = "NOTIFY sent";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //System.IO.File.WriteAllBytes(@"C:\Documents and Settings\brian\Desktop\growl protocol\tests\Request_FlashPolicyRequest.txt", System.Text.Encoding.UTF8.GetBytes("<policy-file-request/>\0"));


            string dir = @"C:\Documents and Settings\brian\Desktop\growl\growl protocol\tests\";
            string resultsdir = dir + @"results\";

            if (System.IO.Directory.Exists(resultsdir))
                System.IO.Directory.Delete(resultsdir, true);
            System.IO.Directory.CreateDirectory(resultsdir);

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);

            System.IO.FileInfo[] files = di.GetFiles();

            foreach (System.IO.FileInfo f in files)
            {
                string filename = f.FullName;
                byte[] rb = new byte[4096];

                byte[] b = System.IO.File.ReadAllBytes(filename);

                System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient("127.0.0.1", Growl.Connector.GrowlConnector.TCP_PORT);    //local
                //System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient("superman", Growl.Connector.GrowlConnector.TCP_PORT);       //remote
                System.Net.Sockets.NetworkStream ns = tcp.GetStream();
                ns.Write(b, 0, b.Length);

                int count = ns.Read(rb, 0, rb.Length);
                byte[] a = new byte[count];
                Array.Copy(rb, a, count);

                System.IO.File.WriteAllBytes(resultsdir + f.Name, a);

            }

            

            string testName = null;

            // test
            testName = "Junk Data";

        }

        private void PasswordTest()
        {
            string password = "Password";
            byte[] saltBytes = new byte[] {1, 2, 3, 4};

            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            Console.WriteLine(passwordBytes);

            byte[] keyBasisBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Array.Copy(passwordBytes, 0, keyBasisBytes, 0, passwordBytes.Length);
            Array.Copy(saltBytes, 0, keyBasisBytes, passwordBytes.Length, saltBytes.Length);
            Console.WriteLine(keyBasisBytes);

            byte[] keyBytes = Cryptography.ComputeHash(keyBasisBytes, Cryptography.HashAlgorithmType.MD5);
            Console.WriteLine(keyBytes);

            byte[] keyHashBytes = Cryptography.ComputeHash(keyBytes, Cryptography.HashAlgorithmType.MD5);
            string keyHash = Cryptography.HexEncode(keyHashBytes);
            Console.WriteLine(keyHash);

            string saltHash = Cryptography.HexEncode(saltBytes);
            Console.WriteLine(saltHash);

            Console.WriteLine(String.Format("MD5:{0}.{1}", keyHash, saltHash));
        }

        private void Test_Register(string testName, Growl.Connector.Application app, List<NotificationType> types, Cryptography.SymmetricAlgorithmType ea, Cryptography.HashAlgorithmType ha)
        {
            GrowlConnector g = new GrowlConnector(this.textBox2.Text);
            g.EncryptionAlgorithm = ea;
            g.KeyHashAlgorithm = ha;

            //string r = g.Register(app, types.ToArray());

            //WriteTestRequest(r);
        }

        private void WriteTestRequest(string r)
        {
            System.IO.StreamWriter writer = System.IO.File.CreateText(String.Format("Tests/Request-{0}.txt"));
            writer.Write(r);
            writer.Close();
        }

        private void WriteTestResponse(string r)
        {
            System.IO.StreamWriter writer = System.IO.File.CreateText(String.Format("Tests/Response-{0}.txt"));
            writer.Write(r);
            writer.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.growl.Password = textBox2.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Detector detector = new Detector();
            detector.ShowSettingsWindow();

            /*
            string pass = "testing";
            byte[] salt = Growl.Connector.Cryptography.HexUnencode("bbdf0d5db70ab6f0bf6a18b804a4c3c0");

            Key key = Key.GenerateKey(pass, Cryptography.HashAlgorithmType.SHA256, Cryptography.SymmetricAlgorithmType.TripleDES);

            StringBuilder sb = new StringBuilder();
            sb.Append("Application-Name: Growl\r\n");
            sb.Append("Application-Icon: x-growl-resource://1aa5ef7f6ff637cd70b4d35463889f8e\r\n");
            sb.Append("X-Application-BundleID: com.Growl.GrowlHelperApp\r\n");
            sb.Append("Notifications-Count: 3\r\n");
            sb.Append("Sent-By: zoidberg\r\n");
            sb.Append("Origin-Machine-Name: zoidberg\r\n");
            sb.Append("Origin-Software-Name: Growl\r\n");
            sb.Append("Origin-Software-Version: 1.2\r\n");
            sb.Append("Origin-Platform-Name: Mac OS X\r\n");
            sb.Append("Origin-Platform-Version: 10.6.1\r\n");
            sb.Append("\r\n");
            sb.Append("Notification-Name: Growl update available\r\n");
            sb.Append("Notification-Display-Name: Growl update available\r\n");
            sb.Append("Notification-Enabled: Yes\r\n");
            sb.Append("\r\n");
            sb.Append("Notification-Name: User went idle\r\n");
            sb.Append("Notification-Display-Name: User went idle\r\n");
            sb.Append("Notification-Enabled: no\r\n");
            sb.Append("\r\n");
            sb.Append("Notification-Name: User returned\r\n");
            sb.Append("Notification-Display-Name: User returned\r\n");
            sb.Append("Notification-Enabled: no\r\n");
            sb.Append("\r\n");

            string input = sb.ToString();
            byte[] b = Encoding.UTF8.GetBytes(input);

            byte[] iv = Cryptography.HexUnencode("af88602d2e17c145");
            EncryptionResult er = key.Encrypt(b, ref iv);
            //EncryptionResult er = key.Encrypt(b);

            string ivHex = Cryptography.HexEncode(er.IV);
            Console.WriteLine(ivHex);

            string eb = Cryptography.HexEncode(er.EncryptedBytes);
            Console.WriteLine(eb);

            byte[] g = Cryptography.HexUnencode("18b337f2e8bd00254c395e358d1cd619889bd374eb48d3e74cc1c4137ddb5dc1e23d914b5a529462e9c9cb990ac38aa771ed825c172f5981f3fed7ec54a9f9cfa65bc590c0bbf58cf3e32fdda14fe568cab913f0bca3b833a34083b0093f6ac611ea1e15763d8a6d028f62cb15ebb98321316ce7578a14376fc4c4167a3d2c46838b146810d334d5578dbffa25cf3d44bb333e978afc70b4cf9367c0a5201facca5fa5a241858dde7e973dae17d0f5ef9c7fba0c752d6a935d06ecfa641e798f3ec5e6d68c4ecc8b4dc511cf431d5beef37cb2c4457a7ff1e6bc5913fbb75a67a75c483f43093302c05f5443c15fef78cea1f8efd14ac7d3a666ee60b4a17f143a4647e0aa1d7169872e34fe2b3af7bdc0f2fac5532a499ad4512b44497d67cc4b0fb0038cc0f72a153ec34e6d165b6dc46b783f5450aebc08bce0b3be9e515afd3a9022a2214ab2ceb70a2f6c8219d2741eda9e7cc4e0dffe3f7f7303491c97965127f45d24c13c3a5c03f7db3c34ab1f4b34b6fe24435e0ccfc299ee055d938596d805a70e9af10c133735cf077660412ec23673d64d283835003b6c07dadac74db61e76437b22235aa9803450c833ee422b2e5beb6754923e8bcbd9f5c6a1d8942784a62b651881ca11770080331030acb6a66a38a3e5afc37cb7c946a84f19bc90fd6cf5cd7ad95771dadc3e5dbd3e6908dc85739c774176884c2f030cad7c817769607401c9f752cad3582160eab41f216e3cb3d519576366aa1c9bce342954641281c5133826113c556f7179c2a07b284481acedf1085f7f9768fd74ee854b6c04ba04566187699f81e351e528323d50e439d0d168ecfb1500f9ef8b8f8018220f2814948940a5fda118d44ad1dc7c58c66dfbc81c66f3f229b8c57dbd30c789c8d98c2c0117140e431369c37fa23d77c8d92bfcd4fa42db3c5f86f30cba4e198a786d4591");
            Console.WriteLine(g.Length);

            byte[] d = key.Decrypt(g, iv);
            Console.WriteLine(d.Length);
            */
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Growl.Connector;

namespace growlnotify
{
    public class Program
    {
        static GrowlConnector growl;
        static EventWaitHandle ewh;
        static bool silent = false;
        static int r = -1;

        public static int Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Invalid arguments. See /? for usage.");
                return -1;
            }

            if (args[0] == "/?")
            {
                Console.WriteLine();
                string usage = global::growlnotify.Properties.Resources.usage;
                Console.WriteLine(usage);
                return 0;
            }

            // parse parameters
            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
            try
            {
                foreach (string arg in args)
                {
                    Parameter p = GetParameterValue(arg);
                    parameters.Add(p.Argument, p);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Bad arguments : " + ex.Message);
                return -1;
            }
            
            // set default values
            string messageText = "";
            string title = "growlnotify";
            string id = "";
            string coalescingid = null;
            bool sticky = false;
            int priorityInt = 0;
            string iconFile = null;
            string applicationName = "growlnotify";
            string appIconFile = null;
            string[] notificationTypesToRegister = null;
            string notificationType = "General Notification";
            string callbackUrl = null;
            string protocol = "GNTP";
            string host = "localhost";
            string password = null;

            Cryptography.SymmetricAlgorithmType encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
            Cryptography.HashAlgorithmType hashAlgorithm = Cryptography.HashAlgorithmType.MD5;
            int port = GrowlConnector.TCP_PORT;

            // validate required parameters
            if (!parameters.ContainsKey("messagetext"))
            {
                Console.WriteLine("Missing 'messagetext' argument. See /? for usage");
                return -1;
            }
            else
            {
                messageText = parameters["messagetext"].Value;
            }
            if (parameters.ContainsKey("/t"))
            {
                title = parameters["/t"].Value;
            }
            if (parameters.ContainsKey("/id"))
            {
                id = parameters["/id"].Value;
            }
            if (parameters.ContainsKey("/s"))
            {
                string s = parameters["/s"].Value.ToLower();
                if (s == "true") sticky = true;
            }
            if (parameters.ContainsKey("/p"))
            {
                priorityInt = Convert.ToInt32(parameters["/p"].Value);
            }
            if (parameters.ContainsKey("/i"))
            {
                iconFile = parameters["/i"].Value;
                if (iconFile.StartsWith("."))
                {
                    string root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    iconFile = System.IO.Path.Combine(root, iconFile);
                }
            }
            if (parameters.ContainsKey("/c"))
            {
                coalescingid = parameters["/c"].Value;
            }
            if (parameters.ContainsKey("/a"))
            {
                applicationName = parameters["/a"].Value;
            }
            if (parameters.ContainsKey("/ai"))
            {
                appIconFile = parameters["/ai"].Value;
                if (appIconFile.StartsWith("."))
                {
                    string root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    appIconFile = System.IO.Path.Combine(root, iconFile);
                }
            }
            if (parameters.ContainsKey("/r"))
            {
                string[] parts = parameters["/r"].Value.Split(',');
                if (parts != null && parts.Length > 0)
                {
                    notificationTypesToRegister = new string[parts.Length];
                    for (int p = 0; p < parts.Length; p++)
                    {
                        string val = parts[p];
                        if (val.StartsWith("\""))
                        {
                            val = val.Substring(1, val.Length - 1);
                        }
                        if(val.EndsWith("\""))
                        {
                            val = val.Substring(0, val.Length - 1);
                        }
                        notificationTypesToRegister[p] = val;
                    }
                }
            }
            if (parameters.ContainsKey("/n"))
            {
                notificationType = parameters["/n"].Value;
            }
            if (parameters.ContainsKey("/cu"))
            {
                callbackUrl = parameters["/cu"].Value;
            }
            if (parameters.ContainsKey("/protocol"))
            {
                protocol = parameters["/protocol"].Value;
            }
            if (parameters.ContainsKey("/host"))
            {
                host = parameters["/host"].Value;
            }
            if (parameters.ContainsKey("/port"))
            {
                port = Convert.ToInt32(parameters["/port"].Value);
            }
            else
            {
                if (protocol == "UDP") port = 9887;
            }
            if (parameters.ContainsKey("/pass"))
            {
                password = parameters["/pass"].Value;
            }
            if (parameters.ContainsKey("/enc"))
            {
                switch (parameters["/enc"].Value.ToUpper())
                {
                    case "DES" :
                        encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.DES;
                        break;
                    case "3DES":
                        encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.TripleDES;
                        break;
                    case "AES":
                        encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;
                        break;
                    default :
                        encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
                        break;
                }
            }
            if (parameters.ContainsKey("/hash"))
            {
                switch (parameters["/hash"].Value.ToUpper())
                {
                    case "SHA1":
                        hashAlgorithm = Cryptography.HashAlgorithmType.SHA1;
                        break;
                    case "SHA256":
                        hashAlgorithm = Cryptography.HashAlgorithmType.SHA256;
                        break;
                    case "SHA512":
                        hashAlgorithm = Cryptography.HashAlgorithmType.SHA512;
                        break;
                    default:
                        hashAlgorithm = Cryptography.HashAlgorithmType.MD5;
                        break;
                }
            }
            if (parameters.ContainsKey("/silent"))
            {
                string s = parameters["/silent"].Value.ToLower();
                if (s == "true") silent = true;
            }

            // set up a waithandle so we can wait for responses
            ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

            // set up growl connector
            growl = new GrowlConnector(password, host, port);
            growl.EncryptionAlgorithm = encryptionAlgorithm;
            growl.KeyHashAlgorithm = hashAlgorithm;
            growl.OKResponse += new GrowlConnector.ResponseEventHandler(growl_Response);
            growl.ErrorResponse += new GrowlConnector.ResponseEventHandler(growl_Response);

            // do any registration first
            if (notificationTypesToRegister != null || applicationName == "growlnotify")
            {
                Growl.CoreLibrary.Resource appIcon = null;
                if (!String.IsNullOrEmpty(appIconFile))
                {
                    Uri uri = new Uri(appIconFile);
                    if (uri.IsFile && System.IO.File.Exists(uri.LocalPath))
                    {
                        appIcon = Growl.CoreLibrary.ImageConverter.ImageFromUrl(uri.LocalPath);
                    }
                    else
                    {
                        appIcon = appIconFile;
                    }
                }

                if (notificationTypesToRegister == null)
                {
                    notificationTypesToRegister = new string[] { "General Notification" };
                }
                NotificationType[] types = new NotificationType[notificationTypesToRegister.Length];
                for(int t=0;t<types.Length;t++)
                {
                    string nttr = notificationTypesToRegister[t];
                    NotificationType type = new NotificationType(nttr);
                    types[t] = type;
                }
                Application application = new Application(applicationName);
                application.Icon = appIcon;
                growl.Register(application, types);
                ewh.WaitOne();  // wait just to be sure the registration gets there first
            }

            // handle any callback information
            CallbackContext callback = null;
            if (!String.IsNullOrEmpty(callbackUrl))
            {
                callback = new CallbackContext(callbackUrl);
            }

            ewh.Reset();

            // handle icons (local icons will be sent as binary data, url-based icons will be sent as urls)
            Growl.CoreLibrary.Resource icon = null;
            if (!String.IsNullOrEmpty(iconFile))
            {
                Uri uri = new Uri(iconFile);
                if (uri.IsFile && System.IO.File.Exists(uri.LocalPath))
                {
                    icon = Growl.CoreLibrary.ImageConverter.ImageFromUrl(uri.LocalPath);
                }
                else
                {
                    icon = iconFile;
                }
            }

            // send the notification
            Priority priority = (Enum.IsDefined(typeof(Priority), priorityInt) ? (Priority)priorityInt : Priority.Normal);
            Notification notification = new Notification(applicationName, notificationType, id, title, messageText, icon, sticky, priority, coalescingid);
            growl.Notify(notification, callback);
            ewh.WaitOne();

            Console.WriteLine();
            return r;
        }

        static void growl_Response(Response response, object state)
        {
            if (!silent)
            {
                if (response.IsOK)
                {
                    r = 0;
                    Console.WriteLine("Notification sent successfully");
                }
                else
                {
                    r = response.ErrorCode;
                    Console.WriteLine(String.Format("Notification failed: {0} - {1}", response.ErrorCode, response.ErrorDescription));
                }
            }

            // signal that a response was received
            ewh.Set();
        }

        private static Parameter GetParameterValue(string argument)
        {
            if (argument.StartsWith("/"))
            {
                string[] parts = argument.Split(new char[] { ':' }, 2);
                string val = parts[1];
                if (val.StartsWith("\"") && val.EndsWith("\""))
                {
                    val = val.Substring(1, val.Length - 2);
                }
                return new Parameter(parts[0], val);
            }
            else
                return new Parameter("messagetext", argument);
        }

        private struct Parameter
        {
            public Parameter(string arg, string val)
            {
                this.Argument = arg;

                if (val == null) val = String.Empty;
                val = val.Trim();
                val = val.Replace("\\n", "\n");
                val = val.Replace("\\\n", "\\n");
                this.Value = val;
            }

            public string Argument;
            public string Value;
        }
    }
}

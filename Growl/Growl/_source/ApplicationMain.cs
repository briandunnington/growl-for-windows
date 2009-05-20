using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Growl
{
    static class ApplicationMain
    {
        static Program program;

        static bool appIsAlreadyRunning;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(false);

            SingleInstanceApplication app = new SingleInstanceApplication("GrowlForWindows");
            using (app)
            {
                appIsAlreadyRunning = app.IsAlreadyRunning;

                // handle protocol-triggered operations
                if (args != null && args.Length == 1)
                {
                    string protocolArgument = args[0];
                    Installation.ProtocolHandler handler = new Growl.Installation.ProtocolHandler(appIsAlreadyRunning);
                    handler.Process(protocolArgument);
                }

                if (!appIsAlreadyRunning)
                {
                    app.AnotherInstanceStarted += new EventHandler(app_AnotherInstanceStarted);
                    try
                    {
                        // handle command line options
                        Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                        if (args != null)
                        {
                            foreach (string arg in args)
                            {
                                Parameter p = GetParameterValue(arg);
                                if(p.Argument != null) parameters.Add(p.Argument, p);
                            }
                        }

                        bool enableLogging = false;
                        if (parameters.ContainsKey("/log"))
                        {
                            string log = parameters["/log"].Value.ToLower();
                            if (log == "true") enableLogging = true;
                            Properties.Settings.Default.EnableLogging = enableLogging;
                        }

                        program = new Program();
                        app.Run(program);
                    }
                    catch (Exception ex)
                    {
                        string source = "Growl";
                        string logtext = String.Format("{0}\r\n\r\n{1}", ex.Message, ex.StackTrace);
                        string msgtext = "Growl encountered a fatal error and cannot continue.\r\n\r\nPlease see the Event Viewer for details.";
                        if (!System.Diagnostics.EventLog.SourceExists(source))
                        {
                            System.Diagnostics.EventLog.CreateEventSource(source, "Application");
                        }
                        System.Diagnostics.EventLog elog = new System.Diagnostics.EventLog();
                        elog.Source = source;
                        elog.WriteEntry(logtext, System.Diagnostics.EventLogEntryType.Error);
                        MessageBox.Show(msgtext, "Growl - Fatal Exception", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                    Keyboard.Unhook();
                }
                else
                {
                    app.SignalFirstInstance();
                }
            }
        }

        static void app_AnotherInstanceStarted(object sender, EventArgs e)
        {
            // show notification that growl is already running...
            Console.WriteLine("INSTANCE: growl is already running");

            program.AlreadyRunning();
        }

        static public Program Program
        {
            get
            {
                return program;
            }
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
            return Parameter.Empty;
        }

        private struct Parameter
        {
            public static Parameter Empty = new Parameter(null, null);

            public Parameter(string arg, string val)
            {
                this.Argument = arg;

                //if (val == null) val = String.Empty;
                //val = val.Replace("\\n", "\n");
                //val = val.Replace("\\\n", "\\n");
                this.Value = val;
            }

            public string Argument;
            public string Value;
        }
    }
}
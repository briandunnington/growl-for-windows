using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Growl
{
    static class ApplicationMain
    {
        [Flags()]
        internal enum Signal
        {
            Silent = 1,
            ReloadDisplays = 2,
            UpdateLanguage = 4,
            HandleListenUrl = 8,
            ReloadForwarders = 16,
            ReloadSubscribers = 32
        }

        static Program program;
        static bool appIsAlreadyRunning;
        static bool silentMode;
        static bool loggingEnabled;
        static List<InternalNotification> queuedNotifications = new List<InternalNotification>();

        public static DateTime st;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread()]
        static void Main(string[] args)
        {
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif

            try
            {
                st = DateTime.Now;
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                SingleInstanceApplication app = new SingleInstanceApplication("GrowlForWindows");
                using (app)
                {
                    // register support for data: uris
                    DataWebRequest.Register();

                    // set global (default) proxy info
                    ProxyHelper.SetProxy();

                    Signal signalFlag = 0;
                    int signalValue = 0;
                    appIsAlreadyRunning = app.IsAlreadyRunning;

                    // handle protocol-triggered operations
                    if (args != null && args.Length == 1)
                    {
                        string protocolArgument = args[0];
                        Installation.ProtocolHandler handler = new Growl.Installation.ProtocolHandler(appIsAlreadyRunning);
                        signalFlag = handler.Process(protocolArgument, ref queuedNotifications, ref signalValue);
                    }

                    // handle command line options
                    try
                    {
                        Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
                        if (args != null)
                        {
                            foreach (string arg in args)
                            {
                                Parameter p = GetParameterValue(arg);
                                if (p.Argument != null) parameters.Add(p.Argument, p);
                            }
                        }

                        if (parameters.ContainsKey("/log"))
                        {
                            string log = parameters["/log"].Value.ToLower();
                            if (log == "true") loggingEnabled = true;
                        }
                        bool debugMode = false;
                        if (parameters.ContainsKey("/debug"))
                        {
                            string debug = parameters["/debug"].Value.ToLower();
                            if (debug == "true") debugMode = true;
                            Utility.DebugMode = debugMode;
                            if (debugMode) MessageBox.Show("growl is now in debug mode");
                        }
                        if (parameters.ContainsKey("/silent"))
                        {
                            string silent = parameters["/silent"].Value.ToLower();
                            if (silent == "true") silentMode = true;
                            if (silentMode)
                                signalFlag = signalFlag | Signal.Silent;
                        }
                        string listenUrlFile = null;
                        if (parameters.ContainsKey("/listenurl")) listenUrlFile = parameters["/listenurl"].Value;
                        else if (args != null && args.Length == 1) listenUrlFile = args[0];
                        if (!String.IsNullOrEmpty(listenUrlFile) && System.IO.File.Exists(listenUrlFile))
                        {
                            string filename = String.Format("{0}.ListenUrl", System.IO.Path.GetFileNameWithoutExtension(listenUrlFile));    // we have to do this since Firefox will rename the temp file to file.ListenUrl-X.txt
                            string dest = System.IO.Path.Combine(Utility.UserSettingFolder, filename);
                            System.IO.File.Copy(listenUrlFile, dest, true);
                            signalFlag = signalFlag | Signal.HandleListenUrl;
                            signalFlag = signalFlag | Signal.Silent;    // go silent to suppress the 'Growl is running' notification
                        }
                    }
                    catch (Exception ex)
                    {
                        // dont fail on bad arguments
                        Utility.WriteDebugInfo("Bad arguments: " + ex.Message);
                    }

                    if (!appIsAlreadyRunning)
                    {
                        program = new Program();
                        program.ProgramRunning += new EventHandler(program_ProgramRunning);
                        program.Run();
                        app.AnotherInstanceStarted += new SingleInstanceApplication.AnotherInstanceStartedEventHandler(app_AnotherInstanceStarted);
                        app.Run(program);
                        app.AnotherInstanceStarted -= new SingleInstanceApplication.AnotherInstanceStartedEventHandler(app_AnotherInstanceStarted);
                        program.ProgramRunning -= new EventHandler(program_ProgramRunning);
                        program.Dispose();
                        program = null;
                    }
                    else
                    {
                        InternalNotification.SaveToDisk(ref queuedNotifications);
                        app.SignalFirstInstance((int)signalFlag, signalValue);
                    }
                }
                app.Dispose();
                app = null;
            }
            catch (Exception ex)
            {
                HandleUnhandledException(ex);
            }
        }

        static void program_ProgramRunning(object sender, EventArgs e)
        {
            program.HandleSystemNotifications(ref queuedNotifications);
            program.HandleListenUrls();
        }

        static void app_AnotherInstanceStarted(int signalFlag, int signalValue)
        {
            Utility.WriteDebugInfo("INSTANCE: growl is already running");
            if (program != null)
            {
                program.AlreadyRunning(signalFlag, signalValue);
                program.HandleSystemNotifications();
            }
        }

        static public bool HasProgramLaunchedYet
        {
            get
            {
                return (appIsAlreadyRunning || program != null);
            }
        }

        static public bool SilentMode
        {
            get
            {
                return silentMode;
            }
            set
            {
                silentMode = value;
            }
        }

        static public bool LoggingEnabled
        {
            get
            {
                bool enabled = (loggingEnabled || Properties.Settings.Default.EnableLogging);

// always enable logging in debug builds
#if (DEBUG)
                enabled = true;
#endif

                return enabled;
            }
        }

        static public Program Program
        {
            get
            {
                return program;
            }
        }

        static public void ForceGC()
        {
            Utility.WriteDebugInfo("Forcing GC.Collect() - BEGIN");
            System.GC.Collect();
            Utility.WriteDebugInfo("Forcing GC.Collect() - First collection done. Waiting for pending finalizers.");
            System.GC.WaitForPendingFinalizers(); // this method may block while it runs the finalizers
            System.Threading.Thread.CurrentThread.Join(100);
            Utility.WriteDebugInfo("Forcing GC.Collect() - Pending finalizers done. Begin second collection.");
            System.GC.Collect();
            Utility.WriteDebugInfo("Forcing GC.Collect() - END");
        }

        private static Parameter GetParameterValue(string argument)
        {
            if (argument.StartsWith("/"))
            {
                string val = "";
                string[] parts = argument.Split(new char[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    val = parts[1];
                    if (val.StartsWith("\"") && val.EndsWith("\""))
                    {
                        val = val.Substring(1, val.Length - 2);
                    }
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

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleUnhandledException(ex);
        }

        static void HandleUnhandledException(Exception e)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                while (e != null)
                {
                    sb.AppendFormat("{0}\r\n\r\n{1}\r\n\r\n", e.Message, e.StackTrace);
                    e = e.InnerException;
                }
                string logtext = sb.ToString();

                string msgtext = "Growl encountered a fatal error and cannot continue.\r\n\r\nPlease see the Event Viewer for details.";
                Utility.WriteDebugInfo(msgtext);
                Utility.WriteDebugInfo(logtext);

                string source = "Growl";
                if (!System.Diagnostics.EventLog.SourceExists(source))
                {
                    System.Diagnostics.EventLog.CreateEventSource(source, "Application");
                }
                System.Diagnostics.EventLog elog = new System.Diagnostics.EventLog();
                elog.Source = source;
                elog.WriteEntry(logtext, System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show(msgtext, "Growl - Fatal Exception", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            catch
            {
                // dont fail on our global unhandled exception handler - that would just not be right
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
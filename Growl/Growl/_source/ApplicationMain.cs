using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Growl
{
    static class ApplicationMain
    {
        [Flags()]
        internal enum Signal
        {
            CancelLaunching = -1,

            Silent = 1,
            ReloadDisplays = 2,
            UpdateLanguage = 4,
            HandleListenUrl = 8,
            ReloadForwarders = 16,
            ReloadSubscribers = 32,
            ShowSettings = 64
        }

        static Program program;
        static bool appIsAlreadyRunning;
        static bool silentMode;
        static bool loggingEnabled;
        static bool showSettingsOnLaunch;
        static List<InternalNotification> queuedNotifications = new List<InternalNotification>();

        public static DateTime st;

        public static float ScalingFactor = 1;

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

                // handle setting/overriding the culture information (this has to be done early so that installation dialogs are translated)
                SetCulture(Properties.Settings.Default.CultureCode);

                // launch app
                SingleInstanceApplication app = null;
                try
                {
                    app = new SingleInstanceApplication("GrowlForWindows");
                }
                catch(Exception e)
                {
                    HandleUnhandledException(e, "Growl", "Growl is already running under another session on this computer. Only one instance of Growl can run at a time.");
                    return;
                }

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

                        // if for some reason we were told to cancel launching the app, lets honor that
                        if (signalFlag == Signal.CancelLaunching) return;
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

                        // the 'cmd' option is special in that it may require Growl to *not* launch, depending on the current state and command issued.
                        // we handle it here first - if other parameters are also passed, they will be ignored.
                        if(parameters.ContainsKey("/cmd"))
                        {
                            string cmd = parameters["/cmd"].Value.ToLower();
                            // currently supported values: start, stop, show
                            switch (cmd)
                            {
                                case "start" :
                                    // 1. Growl is not running. Start it (service will start automatically)
                                    // 2. Growl is running and started. do nothing
                                    // 3. Growl is running and stopped. start it
                                    // TODO: this is not complete. i am not sure if i am going to include this ever or not
                                    break;
                                case "stop" :
                                    // 1. Growl is not running. do not launch it
                                    // 2. Growl is running and started. stop it but do not close
                                    // 3. Growl is running and stopped. do nothing
                                    // TODO: this is not complete. i am not sure if i am going to include this ever or not
                                    break;
                                case "show" :
                                    // 1. Growl is not running. Start it and open Settings window
                                    // 2. Growl is running. Show Settings window
                                    // 3. Growl is running and Settings window is already open. do nothing
                                    if (appIsAlreadyRunning)
                                    {
                                        signalFlag = signalFlag | Signal.ShowSettings;
                                        signalFlag = signalFlag | Signal.Silent;    // go silent to suppress the 'Growl is running' notification
                                    }
                                    else
                                    {
                                        showSettingsOnLaunch = true;
                                    }
                                    break;
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

            if (showSettingsOnLaunch) program.ShowForm();
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

        static public void SetCulture(string cultureCode)
        {
            if (!String.IsNullOrEmpty(cultureCode))
            {
                try
                {
                    CultureInfo culture = new CultureInfo(cultureCode);
                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                }
                catch
                {
                    // suppress any exception (in case the culture in the .config file is not valid)
                }
            }
            Properties.Resources.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            Properties.Settings.Default.CultureCode = Properties.Resources.Culture.ToString();
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
            HandleUnhandledException(e, "Growl - Fatal Exception", "Growl encountered a fatal error and cannot continue.\r\n\r\nPlease see the Event Viewer for details.");
        }

        static void HandleUnhandledException(Exception e, string title, string msgtext)
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
                MessageBox.Show(msgtext, title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
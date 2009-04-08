using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Growl
{
    static class ApplicationMain
    {
        static Program program;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] commandLine)
        {
            Application.SetCompatibleTextRenderingDefault(false);

            SingleInstanceApplication app = new SingleInstanceApplication("GrowlForWindows");
            app.AnotherInstanceStarted += new EventHandler(app_AnotherInstanceStarted);
            if (!app.IsAlreadyRunning)
            {
                try
                {
                    program = new Program();
                    //GC.KeepAlive(program);
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

        static void app_AnotherInstanceStarted(object sender, EventArgs e)
        {
            // show notification that growl is already running...
            Console.WriteLine("INSTANCE: growl is already running");

            program.AlreadyRunning();
        }
    }
}
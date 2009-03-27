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
                program = new Program();
                //GC.KeepAlive(program);
                app.Run(program);
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
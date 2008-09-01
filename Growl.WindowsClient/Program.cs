using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace Growl.WindowsClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] commandLine)
        {
            Application.SetCompatibleTextRenderingDefault(false);

            StartupNextInstanceEventHandler startupNextInstanceEventHandler = new StartupNextInstanceEventHandler(app_StartupNextInstance);
            ShutdownEventHandler shutdownEventHandler = new ShutdownEventHandler(app_Shutdown);
            SingleInstanceApplication.Run(new MainForm(), commandLine, startupNextInstanceEventHandler, shutdownEventHandler);
        }

        static void app_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            SingleInstanceApplication app = (SingleInstanceApplication)sender;
            MainForm mainForm = (MainForm)app.MainForm;
            mainForm.ShowForm();
        }

        static void app_Shutdown(object sender, EventArgs e)
        {
            SingleInstanceApplication app = (SingleInstanceApplication)sender;
            MainForm mainForm = (MainForm)app.MainForm;
            if (mainForm != null) mainForm.Close();
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace Vortex.Growl.GrowlProtocolHandler
{
    internal class SingleInstanceApplication : WindowsFormsApplicationBase
    {
        private SingleInstanceApplication()
        {
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        }

        public new Form MainForm
        {
            get
            {
                return base.MainForm;
            }
            set
            {
                base.MainForm = value;
            }
        }

        public static void Run(Form mainForm, string[] commandLine, StartupNextInstanceEventHandler startUpNextInstanceEventHandler, ShutdownEventHandler shutdownEventHandler)
        {
            SingleInstanceApplication app = new SingleInstanceApplication();
            if (startUpNextInstanceEventHandler != null) app.StartupNextInstance += startUpNextInstanceEventHandler;
            if (shutdownEventHandler != null) app.Shutdown += shutdownEventHandler;
            app.MainForm = mainForm;
            app.Run(commandLine);
            app.OnShutdown();
        }
    }
}

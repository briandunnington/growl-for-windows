using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace ITunesPluginApp
{
    internal class SingleInstanceApplication : WindowsFormsApplicationBase
    {
        private SingleInstanceApplication()
        {
            //this.MainForm = new InvisibleForm();

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
            if (mainForm != null) app.MainForm = mainForm;
            app.Run(commandLine);
            app.OnShutdown();
        }

        /*
        protected override void OnRun()
        {
            if (this.MainForm == null)
            {
                this.OnCreateMainForm();
            }
            try
            {
                Application.Run();
            }
            finally
            {
                if (this.m_NetworkObject != null)
                {
                    this.m_NetworkObject.DisconnectListener();
                }
                if (this.m_FirstInstanceSemaphore != null)
                {
                    this.m_FirstInstanceSemaphore.Close();
                    this.m_FirstInstanceSemaphore = null;
                }
                AsyncOperationManager.SynchronizationContext = this.m_AppSyncronizationContext;
                this.m_AppSyncronizationContext = null;
            }

        }
         * */
    }
}

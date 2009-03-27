using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace ITunesPluginApp
{
    public class ITunesPluginHelperApp
    {
        static public void Run(ITunesHandler handler, Form mainForm, string[] commandLine)
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            StartupNextInstanceEventHandler startupNextInstanceEventHandler = new StartupNextInstanceEventHandler(app_StartupNextInstance);
            ShutdownEventHandler shutdownEventHandler = new ShutdownEventHandler(app_Shutdown);

            SingleInstanceApplication.Run(mainForm, commandLine, startupNextInstanceEventHandler, shutdownEventHandler);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                Application.Exit();
            }
            catch
            {
            }
        }

        static void app_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            SingleInstanceApplication app = (SingleInstanceApplication)sender;
            Form mainForm = (Form)app.MainForm;
            if (mainForm != null)
            {
                mainForm.Show();
            }
        }

        static void app_Shutdown(object sender, EventArgs e)
        {
            try
            {
                SingleInstanceApplication app = (SingleInstanceApplication)sender;
                Form mainForm = (Form)app.MainForm;
                if (mainForm != null) mainForm.Close();
            }
            catch
            {
            }
        }
    }
}

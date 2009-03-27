using System;
using System.Windows.Forms;
using ITunesPluginApp;

namespace GrowlExtras.ITunesPlugin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] commandLine)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GrowlPlugin gp = new GrowlPlugin();
            MainForm mf = new MainForm(gp);

            ITunesPluginHelperApp.Run(gp, mf, commandLine);
        }
    }
}

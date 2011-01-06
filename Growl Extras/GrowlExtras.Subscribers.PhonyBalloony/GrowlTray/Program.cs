using System;
using System.Windows.Forms;

namespace GrowlTray
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!IsApplicationAlreadyRunning())
            {
                GrowlTrayAppContext context = new GrowlTrayAppContext();
                if (context.Start())
                {
                    Application.Run(context);
                }
            }
        }

        static bool IsApplicationAlreadyRunning()
        {
            string proc = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(proc);
            if (processes.Length > 1)
                return true;
            else
                return false;
        }
    }
}

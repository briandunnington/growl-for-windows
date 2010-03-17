using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GrowlExtras.PhonyBalloony
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
            AppContext context = new AppContext();
            using (context)
            {
                context.Start();
                Application.Run(context);
                context.Stop();
            }
        }
    }
}
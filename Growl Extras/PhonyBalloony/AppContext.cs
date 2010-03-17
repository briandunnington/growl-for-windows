using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.PhonyBalloony
{
    class AppContext : ApplicationContext, IDisposable
    {
        private WndProcReader wpr;
        private SystemBalloonIntercepter sbi;

        public AppContext()
        {
            this.wpr = new WndProcReader(WndProc);
            Microsoft.Win32.SystemEvents.SessionEnding += new Microsoft.Win32.SessionEndingEventHandler(SystemEvents_SessionEnding);

            // this is just a useful addition for debugging. this allows us to stop the process without logging of or shutting down the computer
#if DEBUG
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
#endif
        }

        public void Start()
        {
            Stop();

            this.sbi = new SystemBalloonIntercepter(this.wpr.Handle);
            sbi.Start();
        }

        public void Stop()
        {
            if (sbi != null)
            {
                this.sbi.Stop();
                this.sbi = null;
            }
        }

        private void WndProc(ref Message m)
        {
            /* this is for handling intercepted system balloon messages */
            if (this.sbi != null)
                sbi.ProcessWindowMessage(ref m);
        }

        void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            Stop();
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if(e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
                Stop();
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Microsoft.Win32.SystemEvents.SessionEnding -= new Microsoft.Win32.SessionEndingEventHandler(SystemEvents_SessionEnding);
#if DEBUG
                    Microsoft.Win32.SystemEvents.SessionSwitch -= new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
#endif
                    this.Stop();
                }
                catch
                {
                    // suppress
                }
            }
        }

        #endregion
    }
}

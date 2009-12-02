using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Growl
{
    public class SingleInstanceApplication : IMessageFilter, IDisposable
    {
        public delegate void AnotherInstanceStartedEventHandler(int signalFlag, int signalValue);
        public event AnotherInstanceStartedEventHandler AnotherInstanceStarted;

        public static readonly int WM_SIGNALFIRSTINSTANCE = RegisterWindowMessage("WM_SIGNALFIRSTINSTANCE");
        private string uniqueName;
        private Mutex mutex;
        private bool isAlreadyRunning = false;
        private bool filter = true;
        private System.Timers.Timer filterTimer;
        private bool disposed;

        public SingleInstanceApplication(string uniqueName)
        {
            this.uniqueName = uniqueName;

            bool firstInstance = false;
            this.mutex = new Mutex(true, String.Format(@"Global\{0}", this.uniqueName), out firstInstance); // this is *not* the same mutex as used in Detector.IsGrowlRunning - this mutex only indicates that the program is open, not whether the program is actively listening for notifications
            if (!firstInstance)
            {
                this.isAlreadyRunning = true;
            }
            else
            {
                this.filterTimer = new System.Timers.Timer(250);
                this.filterTimer.AutoReset = false;
                this.filterTimer.Elapsed += new System.Timers.ElapsedEventHandler(filterTimer_Elapsed);
            }
        }

        void filterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.filter = true;
        }

        public bool IsAlreadyRunning
        {
            get
            {
                return this.isAlreadyRunning;
            }
        }

        public void Run()
        {
            RunInternal(null, null);
        }

        public void Run(Form mainForm)
        {
            RunInternal(null, mainForm);
        }

        public void Run(ApplicationContext applicationContext)
        {
            RunInternal(applicationContext, null);
        }

        private void RunInternal(ApplicationContext applicationContext, Form mainForm)
        {
            if (!this.isAlreadyRunning)
            {
                Application.AddMessageFilter(this);

                if (applicationContext == null)
                    applicationContext = new ApplicationContext();
                if (mainForm != null)
                    applicationContext.MainForm = mainForm;

                Application.Run(applicationContext);
            }
        }

        public void SignalFirstInstance(int signalFlag, int signalValue)
        {
            PostMessage(HWND_BROADCAST, WM_SIGNALFIRSTINSTANCE, (IntPtr) signalFlag, (IntPtr) signalValue);
        }

        protected void OnAnotherInstanceStarted(int signalFlag, int signalValue)
        {
            if (AnotherInstanceStarted != null)
            {
                this.AnotherInstanceStarted(signalFlag, signalValue);
            }
        }

        #region IMessageFilter Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool PreFilterMessage(ref Message m)
        {
#if !MONO
            if (m.Msg == WM_SIGNALFIRSTINSTANCE && filter)
            {
                filter = false;
                //Console.WriteLine(m.ToString());
                this.OnAnotherInstanceStarted((int) m.WParam, (int) m.LParam);
                this.filterTimer.Start();
            }
#endif
            return false;
        }

        #endregion

        private IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
		
		
#if !MONO
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
#else
		private static bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			return true;
		}
#endif

#if !MONO
        [DllImport("user32", CharSet=CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);
#else
		private static int RegisterWindowMessage(string message)
		{
			return 0;
		}
#endif
        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if(this.mutex != null) this.mutex.Close();
                    if(this.filterTimer != null) this.filterTimer.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}

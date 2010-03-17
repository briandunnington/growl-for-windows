using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Growl.Connector;

namespace GrowlExtras.PhonyBalloony
{
    public delegate void HookReplacedEventHandler();

    public class SystemBalloonIntercepter : Hook
    {
        public const int WM_COPYDATA = 74;

        private IntPtr MSG_REPLACED;   // value will come from RegisterWindowMessage
        private const string MSG_NAME_REPLACED = "GFW_HOOK_CALLWNDPROC_REPLACED";

        // icon flags are mutually exclusive and take only the lowest 2 bits
        public const int NIIF_NONE = 0x00000000;
        public const int NIIF_INFO = 0x00000001;
        public const int NIIF_WARNING = 0x00000002;
        public const int NIIF_ERROR = 0x00000003;
        public const int NIIF_USER = 0x00000004;

        private static System.Drawing.Image IMAGE_INFO = Properties.Resources.info;
        private static System.Drawing.Image IMAGE_ERROR = Properties.Resources.error;
        private static System.Drawing.Image IMAGE_WARNING = Properties.Resources.warning;

        public event HookReplacedEventHandler HookReplaced;

        private bool registered;
        private bool hooked;
        private IntPtr handle;
        private GrowlConnector growl;
        private Growl.Connector.Application app;
        private Growl.Connector.NotificationType ntBalloon;
        private System.Timers.Timer timer;

        public SystemBalloonIntercepter(IntPtr handle) : base(handle)
		{
			this.handle = handle;

            this.app = new Growl.Connector.Application("Windows");
            this.app.Icon = IMAGE_INFO;
            this.ntBalloon = new NotificationType("balloon", "System Balloons");
            this.growl = new GrowlConnector();
            this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

            this.timer = new System.Timers.Timer(10 * 1000);    // ten seconds
            this.timer.AutoReset = false;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
		}

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TrySetHook();
        }

        ~SystemBalloonIntercepter()
		{
			this.Stop();
		}

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            MSG_REPLACED = RegisterWindowMessage(MSG_NAME_REPLACED);

            TrySetHook();
        }

        protected override void OnStop()
        {
            UninitializeCallWndProcHook();
        }

        private bool Register()
        {
            if (growl.IsGrowlRunning())
            {
                this.growl.Register(this.app, new NotificationType[] { this.ntBalloon });
                this.registered = true;
            }
            return this.registered;
        }

        private void TrySetHook()
        {
            bool ok = Register();

            // Start the hook
            if (ok && !this.hooked)
            {
                this.hooked = true;
                InitializeCallWndProcHook(IntPtr.Zero, _Handle);
            }

            // try again later if Growl is not yet running
            if (!ok)
            {
                this.timer.Start();
            }
        }

        public override void ProcessWindowMessage(ref Message m)
        {
            if (m.Msg == WM_COPYDATA)
            {
                if ((int)m.WParam == 97)
                {
                    if (growl.IsGrowlRunning())
                    {
                        COPYDATASTRUCT cds = new COPYDATASTRUCT();
                        cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                        //Utility.WriteLine(cds.cbData);
                        //Utility.WriteLine(cds.dwData);
                        //Utility.WriteLine(cds.lpData);

                        TRAYINFO ti = new TRAYINFO();
                        ti = (TRAYINFO)Marshal.PtrToStructure(cds.lpData, typeof(TRAYINFO));
                        //Console.WriteLine("_title: " + ti.nid.szInfoTitle);
                        //Console.WriteLine("_text: " + ti.nid.szInfo);

                        // get the icon
                        // it is important to check these in descending order since the 'flags' are not mutually exclusive
                        System.Drawing.Image image = IMAGE_INFO;  // default to INFO
                        if (IsFlagSet(ti.nid.dwInfoFlags, NIIF_USER) && ti.nid.hIcon != IntPtr.Zero)
                        {
                            System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(ti.nid.hIcon);
                            using (icon)
                            {
                                image = icon.ToBitmap();
                            }
                        }
                        else if (IsFlagSet(ti.nid.dwInfoFlags, NIIF_ERROR))
                            image = IMAGE_ERROR;
                        else if (IsFlagSet(ti.nid.dwInfoFlags, NIIF_WARNING))
                            image = IMAGE_WARNING;
                        // flag 1 == IMAGE_INFO anyway

                        if (!String.IsNullOrEmpty(ti.nid.szInfo))
                            OnSystemBalloonIntercepted(ti.nid.szInfoTitle, ti.nid.szInfo, image);

                        // zero == success
                        m.Result = IntPtr.Zero;
                    }
                    else
                    {
                        // non-zero == growl not running
                        // the hook will show the standard system balloon instead
                        m.Result = new IntPtr(int.MaxValue);
                    }
                }
            }
            else if (m.Msg == (int) MSG_REPLACED)
            {
                if (HookReplaced != null)
                    HookReplaced();
            }
        }

        private void OnSystemBalloonIntercepted(string title, string text, System.Drawing.Image icon)
        {
            if (String.IsNullOrEmpty(title)) title = "[no title]";
            if (String.IsNullOrEmpty(text)) text = "[no text]";

            Notification notification = new Notification(this.app.Name, this.ntBalloon.Name, String.Empty, title, text);
            notification.Icon = icon;
            this.growl.Notify(notification);
        }

        private bool IsFlagSet(int val, int flag)
        {
            int result = (val & flag);
            return (result == flag);
        }

        private static void InitializeCallWndProcHook(IntPtr threadID, IntPtr DestWindow)
        {
            if (IntPtr.Size == 8)
                InitializeCallWndProcHook64(threadID, DestWindow);
            else
                InitializeCallWndProcHook32(threadID, DestWindow);
        }

        private static void UninitializeCallWndProcHook()
        {
            if (IntPtr.Size == 8)
                UninitializeCallWndProcHook64();
            else
                UninitializeCallWndProcHook32();
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct TRAYINFO
        {
            public IntPtr x;
            public IntPtr y;
            public NOTIFYICONDATA nid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct NOTIFYICONDATA
        {
            public System.Int32 cbSize; // DWORD
            public System.IntPtr hWnd; // HWND
            public System.Int32 uID; // UINT
            public System.Int32 uFlags; // UINT
            public System.Int32 uCallbackMessage; // UINT
            public System.IntPtr hIcon; // HICON
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public System.String szTip; // char[128]
            public System.Int32 dwState; // DWORD
            public System.Int32 dwStateMask; // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public System.String szInfo; // char[256]
            public System.Int32 uTimeoutOrVersion; // UINT
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public System.String szInfoTitle; // char[64]
            public System.Int32 dwInfoFlags; // DWORD
            //GUID guidItem; > IE 6
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public IntPtr cbData;
            public IntPtr lpData;
        }

        // Functions imported from our unmanaged DLL (32-bit)
        [DllImport("SystemBalloonIntercepter.dll", EntryPoint = "InitializeCallWndProcHook")]
        private static extern void InitializeCallWndProcHook32(IntPtr threadID, IntPtr DestWindow);
        [DllImport("SystemBalloonIntercepter.dll", EntryPoint = "UninitializeCallWndProcHook")]
        private static extern void UninitializeCallWndProcHook32();
        
        // Functions imported from our unmanaged DLL (64-bit)
        [DllImport("SystemBalloonIntercepter64.dll", EntryPoint = "InitializeCallWndProcHook")]
        private static extern void InitializeCallWndProcHook64(IntPtr threadID, IntPtr DestWindow);
        [DllImport("SystemBalloonIntercepter64.dll", EntryPoint = "UninitializeCallWndProcHook")]
        private static extern void UninitializeCallWndProcHook64();

        // Win32 API methods
        [DllImport("user32.dll")]
        private static extern IntPtr RegisterWindowMessage(string lpString);
    }

    public abstract class Hook
    {
        protected bool _IsActive = false;
        protected IntPtr _Handle;

        protected Hook(IntPtr Handle)
        {
            _Handle = Handle;
        }

        public void Start()
        {
            if (!_IsActive)
            {
                _IsActive = true;
                OnStart();
            }
        }

        public void Stop()
        {
            if (_IsActive)
            {
                OnStop();
                _IsActive = false;
            }
        }

        ~Hook()
        {
            Stop();
        }

        public bool IsActive
        {
            get { return _IsActive; }
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        public abstract void ProcessWindowMessage(ref System.Windows.Forms.Message m);
    }
}
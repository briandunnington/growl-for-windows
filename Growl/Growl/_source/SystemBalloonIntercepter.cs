using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Growl.Connector;

namespace Growl
{
    public delegate void HookReplacedEventHandler();
    //public delegate void SystemBalloonInterceptedEventHandler(string title, string text, string icon);

    public class SystemBalloonIntercepter : Hook
    {
        public const int WM_COPYDATA = 74;

        private const string MSG_NAME_REPLACED = "GFW_HOOK_CALLWNDPROC_REPLACED";
        private const string MSG_NAME_SYSNOT = "GFW_HOOK_INTERCEPT_SYSNOT";

        // Values retreived with RegisterWindowMessage
        private int MSG_REPLACED;
        private int MSG_SYSNOT;

        public event HookReplacedEventHandler HookReplaced;
        //public event SystemBalloonInterceptedEventHandler SystemBalloonIntercepted;

        private IntPtr handle;
        private GrowlConnector growl;
        private Growl.Connector.Application app;
        private Growl.Connector.NotificationType ntBalloon;

        public SystemBalloonIntercepter(IntPtr handle) : base(handle)
		{
			this.handle = handle;

            this.app = new Growl.Connector.Application("Windows");
            this.app.Icon = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, @"icons\info.png");
            this.ntBalloon = new NotificationType("balloon", "System Balloons");
            this.growl = new GrowlConnector();
            this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
		}

        ~SystemBalloonIntercepter()
		{
			this.Stop();
		}

        public void Start()
        {
            OnStart();

            this.growl.Register(this.app, new NotificationType[] { this.ntBalloon });
        }

        public void Stop()
        {
            OnStop();
        }

        protected override void OnStart()
        {
            // Retreive the message IDs that we'll look for in WndProc
            MSG_REPLACED = RegisterWindowMessage(MSG_NAME_SYSNOT);
            MSG_SYSNOT = RegisterWindowMessage(MSG_NAME_SYSNOT);

            // Start the hook
            InitializeCallWndProcHook(0, _Handle);
        }

        protected override void OnStop()
        {
            UninitializeCallWndProcHook();
        }

        public override void ProcessWindowMessage(ref Message m)
        {
            if (m.Msg == MSG_SYSNOT || m.Msg == WM_COPYDATA)
            {
                if ((int)m.WParam == 97)
                {
                    COPYDATASTRUCT cds = new COPYDATASTRUCT();
                    cds = (COPYDATASTRUCT) m.GetLParam(typeof(COPYDATASTRUCT));
                    //Console.WriteLine(cds.cbData);
                    //Console.WriteLine(cds.dwData);
                    //Console.WriteLine(cds.lpData);

                    TRAYINFO ti = new TRAYINFO();
                    ti = (TRAYINFO)Marshal.PtrToStructure(cds.lpData, typeof(TRAYINFO));
                    Console.WriteLine("_title: " + ti.nid.szInfoTitle);
                    Console.WriteLine("_text: " + ti.nid.szInfo);

                    if(!String.IsNullOrEmpty(ti.nid.szInfo))
                        OnSystemBalloonIntercepted(ti.nid.szInfoTitle, ti.nid.szInfo, null);
                }
            }
            else if (m.Msg == MSG_REPLACED)
            {
                if (HookReplaced != null)
                    HookReplaced();
            }
        }

        private void OnSystemBalloonIntercepted(string title, string text, string icon)
        {
            Notification notification = new Notification(this.app.Name, this.ntBalloon.Name, String.Empty, title, text);
            notification.Icon = icon;
            this.growl.Notify(notification);

            /*
            if (SystemBalloonIntercepted != null)
            {
                //this.SystemBalloonIntercepted(title, text, icon);

                Notification notification = new Notification(this.app.Name, this.ntBalloon.Name, String.Empty, title, text, icon, false, Priority.Normal, String.Empty);
                this.growl.Notify(notification);
            }
             * */
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct TRAYINFO
        {
            public int x;
            public int y;
            public NOTIFYICONDATA nid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct NOTIFYICONDATA
        {
            /// <summary>
            /// Size of this structure, in bytes.
            /// </summary>
            public int cbSize;

            /// <summary>
            /// Handle to the window that receives notification messages associated with an icon in the
            /// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on
            /// when Shell_NotifyIcon is invoked.
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            /// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify
            /// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons
            /// associated with a single hWnd by assigning each a different uID.
            /// </summary>
            public int uID;

            /// <summary>
            /// Flags that indicate which of the other members contain valid data. This member can be
            /// a combination of the NIF_XXX constants.
            /// </summary>
            public int uFlags;

            /// <summary>
            /// Application-defined message identifier. The system uses this identifier to send
            /// notifications to the window identified in hWnd.
            /// </summary>
            public int uCallbackMessage;

            /// <summary>
            /// Handle to the icon to be added, modified, or deleted.
            /// </summary>
            public IntPtr hIcon;

            /// <summary>
            /// String with the text for a standard ToolTip. It can have a maximum of 64 characters including
            /// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of
            /// 128 characters, including the terminating NULL.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;

            /// <summary>
            /// State of the icon.
            /// </summary>
            public int dwState;

            /// <summary>
            /// A value that specifies which bits of the state member are retrieved or modified.
            /// For example, setting this member to NIS_HIDDEN causes only the item's hidden state to be retrieved.
            /// </summary>
            public int dwStateMask;

            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters.
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;

            /// <summary>
            /// NOTE: This field is also used for the Timeout value. Specifies whether the Shell notify
            /// icon interface should use Windows 95 or Windows 2000
            /// behavior. For more information on the differences in these two behaviors, see
            /// Shell_NotifyIcon. This member is only employed when using Shell_NotifyIcon to send an
            /// NIM_VERSION message.
            /// </summary>
            public int uVersion;

            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface
            /// above the text. It can have a maximum of 63 characters.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;

            /// <summary>
            /// Adds an icon to a balloon ToolTip. It is placed to the left of the title. If the
            /// szTitleInfo member is zero-length, the icon is not shown. See
            /// <see cref="BalloonIconStyle">RMUtils.WinAPI.Structs.BalloonIconStyle</see> for more
            /// information.
            /// </summary>
            public int dwInfoFlags;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public int dwData;   // DWORD - Needs to IntPtr and not Int32 for some reason to work on Vista 64. Different versions of Win32 SDK might say different things
            public int cbData;    // DWORD
            public IntPtr lpData;   // PVOID
        }


        // Functions imported from our unmanaged DLL
        [DllImport("SystemBalloonIntercepter.dll")]
        private static extern void InitializeCallWndProcHook(int threadID, IntPtr DestWindow);
        [DllImport("SystemBalloonIntercepter.dll")]
        private static extern void UninitializeCallWndProcHook();

        // Win32 API methods
        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string lpString);
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
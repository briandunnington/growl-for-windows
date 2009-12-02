using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the base class for all visual notifications
    /// </summary>
    public class NotificationWindow : Form
    {
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_APPWINDOW = 0x40000;

        /// <summary>
        /// Fires after <c>Load</c> but before <c>BeforeShown</c>
        /// </summary>
        /// <remarks>
        /// This event is a good place to put any code that determines the window's initial position or size.
        /// If positioning is done before Load is called, the notification could steal focus from the currently active window
        /// (for example, using Screen.FromControl() to get the desktop size activates the new window, but not if
        /// it is called after Load).
        /// The LayoutManager's repositioning code is also run in the AfterLoad event, but after the form's event handler.
        /// The LayoutManager needs to know the size and position of the window, so they must be set before this event
        /// handler completes.
        /// </remarks>
        public event EventHandler AfterLoad;

        /// <summary>
        /// Fires after <c>AfterLoad</c> but before <c>Shown</c>
        /// </summary>
        /// <remarks>
        /// This event is the preferred place to hook into for any <see cref="IAnimator">Animator</see> classes
        /// that want to animate the showing of the form. It runs after the form size and location are known
        /// and any repositioning has been done (ensuring enough free space for the final form).
        /// </remarks>
        public event EventHandler BeforeShown;

        /// <summary>
        /// Fires when the form is about to close due to lack of user interaction
        /// </summary>
        /// <remarks>
        /// This event is the preferred place to hook into for any <see cref="IAnimator">Animator</see> classes
        /// that want to animate the closing of the form. It only fires when the form is closed due to the user
        /// ignoring the notification, which is usually the only time you should animate the form closing.
        /// </remarks>
        public event FormClosingEventHandler AutoClosing;

        /// <summary>
        /// Fires when the notification is clicked (standard left click)
        /// </summary>
        public event NotificationCallbackEventHandler NotificationClicked;

        /// <summary>
        /// Fires when the notification is closed (either explicitly by the user or automatically)
        /// </summary>
        public event NotificationCallbackEventHandler NotificationClosed;

        /// <summary>
        /// Controls the AutoClose behavior
        /// </summary>
        private Timer displayTimer;

        /// <summary>
        /// The UUID of the notification being shown
        /// </summary>
        private string notificationUUID = null;

        /// <summary>
        /// The notification ID of the notification being shown
        /// </summary>
        private string notificationID = null;

        /// <summary>
        /// The coalescing group of the notification
        /// </summary>
        private string coalescingGroup = null;

        /// <summary>
        /// Indicates if the notification has already been clicked
        /// </summary>
        private bool alreadyClicked;

        /// <summary>
        /// Indicates if form is configured for AutoClose
        /// </summary>
        private bool isAutoClose;

        /// <summary>
        /// Indicates if the notification should be sticky
        /// </summary>
        private bool sticky;

        /// <summary>
        /// Indicates if the form should not automatically be closed when clicked
        /// </summary>
        private bool dontCloseOnClick;

        /// <summary>
        /// Indicates if child controls hooked up via HookupClickEvents should also fire the main form's Click event
        /// </summary>
        private bool fireFormClick;

        /// <summary>
        /// Specifies if the form allows focus (normally used during any loading animation)
        /// </summary>
        private bool allowFocus;

        /// <summary>
        /// A pointer to the current foreground window
        /// </summary>
        private IntPtr currentForegroundWindow;

        /// <summary>
        /// The Animator associated with the form, if any
        /// </summary>
        private IAnimator animator;

        /// <summary>
        /// Indicates if the user explicitly closed the notification window
        /// </summary>
        private bool userClosed;

        /// <summary>
        /// Indicates if the display window should not automatically close itself if the mouse cursor is over the window
        /// </summary>
        private bool pauseWhenMouseOver;


        /// <summary>
        /// Creates a new instance of the NotificationWindow class
        /// </summary>
        public NotificationWindow()
        {
            InitializeComponent();

            // dont show in taskbar
            this.ShowInTaskbar = false;

            // always show on top of other windows
            this.TopMost = true;

            this.MouseEnter += new EventHandler(NotificationWindow_MouseEnter);
            this.FormClosed += new FormClosedEventHandler(NotificationWindow_FormClosed);
            this.Activated += new EventHandler(NotificationWindow_Activated);
            this.Shown += new EventHandler(NotificationWindow_Shown);

            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
        }

        /// <summary>
        /// Overridden to set the WS_EX_TOOLWINDOW style bit so that notification windows don't
        /// show up in the Alt-Tab list.
        /// </summary>
        /// <value></value>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW; // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle &= ~WS_EX_APPWINDOW;  // turn off WS_EX_APPWINDOW style bit
                return cp;
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Load"/>, <see cref="AfterLoad"/>, and <see cref="BeforeShown"/> events.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.OnAfterLoad(this, EventArgs.Empty);
            this.OnBeforeShown(this, EventArgs.Empty);
        }

        /// <summary>
        /// Allows the form to receive focus once it is shown
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        void NotificationWindow_Shown(object sender, EventArgs e)
        {
            this.allowFocus = true;
        }

        /// <summary>
        /// Occurs when the form is activated
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        void NotificationWindow_Activated(object sender, EventArgs e)
        {
            // Prevent the form taking focus when it is initially shown.
            if (!this.allowFocus)
            {
                // Activate the window that previously had the focus.
#if !MONO
                Win32.SetForegroundWindow(this.currentForegroundWindow);
#endif
            }
        }

        /// <summary>
        /// Fires the <see cref="NotificationClosed"/> event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="FormClosedEventArgs"/> object that contains the event data.</param>
        void NotificationWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            CallbackResult result = (this.userClosed ? CallbackResult.CLOSE : CallbackResult.TIMEDOUT);
            this.OnNotificationClosed(new Growl.CoreLibrary.NotificationCallbackEventArgs(this.NotificationUUID, result));
        }

        void NotificationWindow_MouseEnter(object sender, EventArgs e)
        {
            /* this would only apply if the Animator had already started, but that is an edge case
             * and this doesnt work well anyway.
             * If a developer wants to handle that case, they can manually call Animator.CancelClosing()
            if (this.PauseWhenMouseOver)
            {
                this.StopAutoCloseTimer();
                this.StartAutoCloseTimer();
            }
             * */
        }

        /// <summary>
        /// Gets the UUID of the notification being shown
        /// </summary>
        public string NotificationUUID
        {
            get
            {
                return this.notificationUUID;
            }
        }

        /// <summary>
        /// Gets the notification ID of the notification being shown
        /// </summary>
        public string NotificationID
        {
            get
            {
                return this.notificationID;
            }
        }

        /// <summary>
        /// Gets the coalescing group.
        /// </summary>
        public string CoalescingGroup
        {
            get
            {
                return this.coalescingGroup;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the nofication wants to be sticky
        /// </summary>
        /// <remarks>
        /// In the default implementation, if a notification is sets this value to <c>true</c>
        /// then any AutoClose behavior will be ignored.
        /// </remarks>
        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
            set
            {
                this.sticky = value;
                if (value) this.SetAutoCloseInterval(0);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAnimator"/> associated with this form
        /// </summary>
        /// <value>
        /// <see cref="IAnimator"/> or <c>null</c> if the form does not have an Animator
        /// </value>
        public IAnimator Animator
        {
            get
            {
                return this.animator;
            }
            set
            {
                this.animator = value;
            }
        }

        /// <summary>
        /// Displays the notification window
        /// </summary>
        public new virtual void Show()
        {
#if !MONO
            this.currentForegroundWindow = Win32.GetForegroundWindow();
#endif
            base.Show();
#if !MONO
            Win32.SetForegroundWindow(this.currentForegroundWindow);
#endif
            StartAutoCloseTimer();
        }

        /// <summary>
        /// Closes the notification, optionally ignoring any animation behaviors
        /// </summary>
        /// <param name="immediate">If <c>true</c>, then the window will be closed immediately and any animation behaviors will be ignored</param>
        public void Close(bool immediate)
        {
            this.userClosed = immediate;
            if (immediate && this.animator != null) this.animator.Disabled = true;
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Sets the notification information
        /// </summary>
        /// <param name="n">The <see cref="Notification"/> data to show</param>
        public virtual void SetNotification(Notification n)
        {
            this.notificationUUID = n.UUID;
            this.notificationID = n.NotificationID;
            this.coalescingGroup = n.CoalescingGroup;
        }

        /// <summary>
        /// Starts the auto close timer.
        /// </summary>
        /// <remarks>
        /// If no value has been set using SetAutoCloseInterval, this method does nothing.
        /// </remarks>
        public void StartAutoCloseTimer()
        {
            if(this.isAutoClose)
                this.displayTimer.Start();
        }

        /// <summary>
        /// Stops the auto close timer.
        /// </summary>
        public void StopAutoCloseTimer()
        {
            if (this.animator != null)
            {
                this.animator.CancelClosing();
            }
            if (this.displayTimer != null)
            {
                this.displayTimer.Stop();
            }
        }

        /// <summary>
        /// Sets the auto close interval.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <remarks>
        /// If the auto close timer was already started, it will be stopped when this method is called.
        /// You must manually call StartAutoCloseTimer() again after calling this method.
        /// </remarks>
        public void SetAutoCloseInterval(int duration)
        {
            StopAutoCloseTimer();

            if (duration > 0)
            {
                this.isAutoClose = true;
                this.displayTimer.Interval = duration;
            }
            else
            {
                this.isAutoClose = false;
            }
        }

        /// <summary>
        /// Causes the form to automatically close after a set period of time
        /// </summary>
        /// <param name="duration">The amount of time (in milliseconds) to wait before automatically closing</param>
        /// <remarks>
        /// A value of zero will disable the AutoClose behavior.
        /// This method must be called before the form is shown to work properly.
        /// </remarks>
        [Obsolete("AutoClose() is obsolete. Use SetAutoCloseInterval() instead.")]
        protected void AutoClose(int duration)
        {
            SetAutoCloseInterval(duration);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the notification's auto-closing behavior should
        /// pause if the mouse cursor is over the display.
        /// </summary>
        internal protected bool PauseWhenMouseOver
        {
            get
            {
                return this.pauseWhenMouseOver;
            }
            set
            {
                this.pauseWhenMouseOver = value;
            }
        }

        /// <summary>
        /// Determines whether the mouse cursor is mouse over the form.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the mouse is over the form; otherwise, <c>false</c>.
        /// </returns>
        internal protected bool IsMouseOver()
        {
            if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Causes all click events on child controls to fire the <see cref="NotificationClicked"/> event
        /// (or <see cref="NotificationClosed"/> event if right-clicked)
        /// </summary>
        /// <param name="control">The parent control</param>
        /// <remarks>
        /// Call this method using <c>HookUpClickEvents(this)</c> from your main Form to easily
        /// hook up all child controls.
        /// </remarks>
        protected void HookUpClickEvents(Control control)
        {
            if(control != null)
            {
                //control.Click += new EventHandler(c_Click);
                control.MouseClick += new MouseEventHandler(control_MouseClick);
                foreach (Control c in control.Controls)
                {
                    HookUpClickEvents(c);
                }
            }
        }

        /// <summary>
        /// Causes all click events on child controls to fire the <see cref="NotificationClicked"/> event
        /// (or <see cref="NotificationClosed"/> event if right-clicked)
        /// </summary>
        /// <param name="control">The parent control</param>
        /// <param name="dontCloseOnClick">If <c>true</c> then the form will not be automatically closed when clicked</param>
        /// <param name="fireFormClick">If <c>true</c> then click events on child controls will also fire the form's Click event</param>
        /// <remarks>
        /// Call this method using <c>HookUpClickEvents(this)</c> from your main Form to easily
        /// hook up all child controls.
        /// </remarks>
        protected void HookUpClickEvents(Control control, bool dontCloseOnClick, bool fireFormClick)
        {
            this.dontCloseOnClick = dontCloseOnClick;
            this.fireFormClick = fireFormClick;
            HookUpClickEvents(control);
        }

        /// <summary>
        /// Fires the <see cref="NotificationClicked"/> event and optionally closes the form
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        /// <remarks>
        /// If <c>fireFormClick</c> is <c>true</c> and the click came from a child control, the main form's Click event will also be fired.
        /// If <c>dontCloseOnClick</c> is <c>true</c>, then the form will not be closed when clicked. Otherwise, the form will be closed at this point.
        /// </remarks>
        void c_Click(object sender, EventArgs e)
        {
            this.OnNotificationClicked(new Growl.CoreLibrary.NotificationCallbackEventArgs(this.NotificationUUID, Growl.CoreLibrary.CallbackResult.CLICK));
            if (this.fireFormClick && sender != this) this.OnClick(e);    // if a child control was clicked on, also fire Click on the main form
            if(!this.dontCloseOnClick) this.Close();
        }

        /// <summary>
        /// Fires the <see cref="NotificationClicked"/> or <see cref="NotificationClosed"/> event and optionally closes the form
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="MouseEventArgs"/> object that contains the mouse event data.</param>
        /// <remarks>
        /// If this is a standard (left) click, the following rules apply:
        ///     If <c>fireFormClick</c> is <c>true</c> and the click came from a child control, the main form's Click event will also be fired.
        ///     If <c>dontCloseOnClick</c> is <c>true</c>, then the form will not be closed when clicked. Otherwise, the form will be closed at this point.
        /// If this is a right click, the following rules apply:
        ///     The form is closed immediately regardless of the <c>dontCloseOnClick</c> property (consistent with using the keyboard shortcuts).
        ///     The main form's Click event is <c>not</c> fired even if <c>fireFormClick</c> is <c>true</c> (since the form is closing immediately anyway).
        /// </remarks>
        void control_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.Close(true);
            }
            else
            {
                this.OnNotificationClicked(new Growl.CoreLibrary.NotificationCallbackEventArgs(this.NotificationUUID, Growl.CoreLibrary.CallbackResult.CLICK));
                if (this.fireFormClick && sender != this) this.OnClick(e);    // if a child control was clicked on, also fire Click on the main form
                if (!this.dontCloseOnClick) this.Close();
            }
        }

        /// <summary>
        /// Fires the <see cref="NotificationClicked"/> event
        /// </summary>
        /// <param name="args"><see cref="Growl.CoreLibrary.NotificationCallbackEventArgs"/> containing data about the event</param>
        protected void OnNotificationClicked(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (!this.alreadyClicked)
            {
                this.alreadyClicked = true;
                if (this.NotificationClicked != null)
                {
                    this.NotificationClicked(args);
                }
            }
        }

        /// <summary>
        /// Fires the <see cref="NotificationClosed"/> event
        /// </summary>
        /// <param name="args"><see cref="Growl.CoreLibrary.NotificationCallbackEventArgs"/> containing data about the event</param>
        protected void OnNotificationClosed(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (!this.alreadyClicked && this.NotificationClosed != null)
            {
                this.NotificationClosed(args);
            }
        }

        /// <summary>
        /// Fires the <see cref="AfterLoad"/> event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected void OnAfterLoad(object sender, EventArgs e)
        {
            if (this.AfterLoad != null)
            {
                this.AfterLoad(sender, e);
            }
        }

        /// <summary>
        /// Fires the <see cref="BeforeShown"/> event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected void OnBeforeShown(object sender, EventArgs e)
        {
            if (this.BeforeShown != null)
            {
                this.BeforeShown(sender, e);
            }
        }

        /// <summary>
        /// Fires the <see cref="AutoClosing"/> event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected void OnAutoClosing(object sender, FormClosingEventArgs e)
        {
            if (this.AutoClosing != null)
            {
                this.AutoClosing(sender, e);
            }
        }

        /// <summary>
        /// Fires the <see cref="AutoClosing"/> event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        void displayTimer_Tick(object sender, EventArgs e)
        {
            this.StopAutoCloseTimer();
            if (!this.pauseWhenMouseOver || !this.IsMouseOver())
            {
                FormClosingEventArgs args = new FormClosingEventArgs(CloseReason.None, false);
                this.OnAutoClosing(this, args);
                if (!args.Cancel)
                    this.Close();
            }
            else
            {
                // try again
                if (this.isAutoClose)
                    this.StartAutoCloseTimer();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NotificationWindow
            // 
            this.ClientSize = new System.Drawing.Size(311, 134);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NotificationWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.Windows.Forms.Form"/>.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.displayTimer != null) this.displayTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the base implementation for on-screen (visual) displays.
    /// </summary>
    /// <remarks>
    /// Most developers should inherit their displays from this class if they are
    /// going to show a notification on-screen, as it provides useful implementation 
    /// of most common properties and methods. If your display is non-visual (email, 
    /// text-to-speech, etc), you should choose the <see cref="Display"/> class instead.
    /// </remarks>
    public abstract class VisualDisplay : Display
    {
        /// <summary>
        /// Contains a list of currently open (visible) windows associated with this display
        /// </summary>
        private List<NotificationWindow> activeWindows = new List<NotificationWindow>();

        /// <summary>
        /// Indicates if any calls to an associated <see cref="LayoutManager"/> should be suppressed
        /// </summary>
        private bool suppressLayout;

        /// <summary>
        /// The list of all open notifications associated with this display
        /// </summary>
        /// <value><see cref="List{NotificationWindow}"/></value>
        protected List<NotificationWindow> ActiveWindows
        {
            get
            {
                return activeWindows;
            }
        }

        /// <summary>
        /// Shows the notification window.
        /// </summary>
        /// <param name="win">The <see cref="NotificationWindow"/> to show</param>
        /// <remarks>
        /// Classes inheriting from <see cref="VisualDisplay"/> should always use this
        /// method to show their notifications rather than calling <c>NotificationWindow.Show</c>
        /// directly because this method hooks up some necessary events to handle click callbacks,
        /// layout management, etc.
        /// </remarks>
        protected void Show(NotificationWindow win)
        {
            win.AfterLoad += new EventHandler(win_AfterLoad);
            win.FormClosed += new System.Windows.Forms.FormClosedEventHandler(win_FormClosed);
            win.NotificationClicked += new NotificationCallbackEventHandler(win_NotificationClicked);
            win.NotificationClosed += new NotificationCallbackEventHandler(win_NotificationClosed);

            win.Show();
        }

        /// <summary>
        /// Returns a reference to the <see cref="LayoutManager"/> used to control
        /// window layout.
        /// </summary>
        /// <param name="win">The current <see cref="NotificationWindow"/> being shown or hidden</param>
        /// <returns>
        /// <see cref="LayoutManager"/> if the display is using the built-in layout management;
        /// <c>null</c> if the display is handling its own layout management
        /// </returns>
        protected virtual LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return null;
        }

        /// <summary>
        /// Fires the <see cref="NotificationClicked"/> event
        /// </summary>
        /// <param name="args"><see cref="Growl.CoreLibrary.NotificationCallbackEventArgs"/> containing information about the event</param>
        void win_NotificationClicked(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (this.NotificationClicked != null)
            {
                this.NotificationClicked(args);
            }
        }

        /// <summary>
        /// Fires the <see cref="NotificationClosed"/> event
        /// </summary>
        /// <param name="args"><see cref="Growl.CoreLibrary.NotificationCallbackEventArgs"/> containing information about the event</param>
        void win_NotificationClosed(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (this.NotificationClosed != null)
            {
                this.NotificationClosed(args);
            }
        }

        /// <summary>
        /// Makes a call to the associated LayoutManager when the notification is shown
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void win_AfterLoad(object sender, EventArgs e)
        {
            NotificationWindow win = (NotificationWindow)sender;
            this.activeWindows.Insert(0, win);

            LayoutManager lm = GetLayoutManager(win);
            if (lm != null) lm.Add(win);
        }

        /// <summary>
        /// Makes a call to the associated LayoutManager when the notification is closed
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void win_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            NotificationWindow win = (NotificationWindow)sender;
            this.activeWindows.Remove(win);

            if (!this.suppressLayout)
            {
                LayoutManager lm = GetLayoutManager(win);
                if (lm != null) lm.Remove(win);
            }
        }

        #region IDisplay Members

        /// <summary>
        /// Closes any open notifications associated with this display
        /// </summary>
        public override void CloseAllOpenNotifications()
        {
            lock (this.activeWindows)
            {
                this.suppressLayout = true;
                while (this.activeWindows.Count > 0)
                {
                    this.activeWindows[0].Close(true);  // this forces windows to close immediately and not animate
                }
                this.suppressLayout = false;
            }
        }

        /// <summary>
        /// Closes the most-recently shown notification.
        /// </summary>
        public override void CloseLastNotification()
        {
            lock (this.activeWindows)
            {
                if (this.activeWindows.Count > 0)
                {
                    this.activeWindows[this.activeWindows.Count - 1].Close(true);
                }
            }
        }

        /// <summary>
        /// Handles displaying the notification. Called each time a notification is received that is to
        /// be handled by this display.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> information</param>
        /// <param name="displayName">A string identifying the display name (used mainly by displays that provide multiple end-user selectable display styles)</param>
        /// <returns></returns>
        public override bool ProcessNotification(Notification notification, string displayName)
        {
            HandleNotification(notification, displayName);

            // returning false indicates that the notification is still 'open' (visible) and will callback later
            return false;
        }

        /// <summary>
        /// Forces any on-screen notifications to redraw themselves.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public override void Refresh()
        {
            foreach (NotificationWindow activeWindow in this.activeWindows)
            {
                activeWindow.Refresh();
            }
        }

        #endregion

        /// <summary>
        /// Fired when the notification is clicked (standard left clicks only)
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public override event NotificationCallbackEventHandler NotificationClicked;

        /// <summary>
        /// Fired when the notification is closed (either explicitly by the user, or
        /// automatically after a period of time, etc)
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// In the current version of Growl, a right mouse click explicitly closes 
        /// the notification. In this instance, the NotificationClosed event is fired,
        /// not the NotificationClicked event.
        /// </remarks>
        public override event NotificationCallbackEventHandler NotificationClosed;
    }
}

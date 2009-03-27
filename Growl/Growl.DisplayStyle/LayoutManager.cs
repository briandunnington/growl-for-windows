using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Automatically repositions open notification windows of a similar display style when
    /// new notifications are displayed or exisiting notifications are closed
    /// </summary>
    /// <remarks>
    /// The behavior of this LayoutManager is to open new notifications in their default
    /// position and move all open notification correspondingly. If you would prefer to leave
    /// exisiting notifications in their place and open new notifications in an adjusted location,
    /// you will have to provide your own layout logic for your display.
    /// </remarks>
    public class LayoutManager
    {
        /// <summary>
        /// A list of all of the open windows that this layout manager is managing
        /// </summary>
        private List<NotificationWindow> activeWindows = new List<NotificationWindow>();

        /// <summary>
        /// The direction to move exisiting notifications when new notifications are displayed
        /// </summary>
        private AutoPositionDirection direction = AutoPositionDirection.UpLeft;

        /// <summary>
        /// The amount of vertical space between notifications
        /// </summary>
        private int verticalPadding = 0;

        /// <summary>
        /// The amount of horizontal space between notifications
        /// </summary>
        private int horizontalPadding = 0;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="direction">The direction to move exisiting notifications when new notifications are displayed</param>
        /// <param name="verticalPadding">The amount of vertical space between notifications</param>
        /// <param name="horizontalPadding">The amount of horizontal space between notifications</param>
        public LayoutManager(AutoPositionDirection direction, int verticalPadding, int horizontalPadding)
        {
            this.direction = direction;
            this.verticalPadding = verticalPadding;
            this.horizontalPadding = horizontalPadding;
        }

        /// <summary>
        /// The direction to move exisiting notifications when new notifications are displayed
        /// </summary>
        /// <value><see cref="AutoPositionDirection"/></value>
        public AutoPositionDirection Direction
        {
            get
            {
                return this.direction;
            }
        }

        /// <summary>
        /// The amount of vertical space between notifications
        /// </summary>
        /// <value>int (number of pixels)</value>
        public int VerticalPadding
        {
            get
            {
                return this.verticalPadding;
            }
        }

        /// <summary>
        /// The amount of horizontal space between notifications
        /// </summary>
        /// <value>int (number of pixels)</value>
        public int HorizontalPadding
        {
            get
            {
                return this.horizontalPadding;
            }
        }

        /// <summary>
        /// The list of all open notifications that this layout manager is managing
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
        /// Adds a new notification to the list and repositions any other open notifications
        /// </summary>
        /// <param name="win">The new notification window being shown</param>
        /// <remarks>
        /// This method does not call the window's <c>Show</c> method - you must show
        /// the window yourself.
        /// </remarks>
        public void Add(NotificationWindow win)
        {
            RepositionWindows(win, false);
            this.activeWindows.Insert(0, win);
        }

        /// <summary>
        /// Removes a notification from the list and repositions any other open notifications
        /// </summary>
        /// <param name="win">The notification window being closed</param>
        /// <remarks>
        /// This method does not call the window's <c>Close</c> method - you must close
        /// the window yourself.
        /// </remarks>
        public void Remove(NotificationWindow win)
        {
            RepositionWindows(win, true);
            this.activeWindows.Remove(win);
        }

        /// <summary>
        /// Repositions any open windows
        /// </summary>
        /// <param name="currentWin">The new window being shown or the window being closed</param>
        /// <param name="closing">Indicates if the window is being closed (vs. being shown)</param>
        private void RepositionWindows(NotificationWindow currentWin, bool closing)
        {
            if (this.ActiveWindows.Count > 0)
            {
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromControl(currentWin);

                lock (this.ActiveWindows)
                {
                    if (closing)
                    {
                        int x = currentWin.DesktopLocation.X;
                        int y = (this.direction == AutoPositionDirection.UpLeft || this.direction == AutoPositionDirection.UpRight ? currentWin.Bottom : currentWin.Top);

                        int index = this.ActiveWindows.IndexOf(currentWin);
                        for (int i = index; i < this.ActiveWindows.Count; i++)
                        {
                            NotificationWindow aw = this.ActiveWindows[i];
                            if (aw != currentWin)
                            {
                                switch (this.direction)
                                {
                                    case AutoPositionDirection.DownLeft:
                                    case AutoPositionDirection.DownRight:
                                        if (y + aw.Height > screen.Bounds.Bottom)
                                        {
                                            y = screen.WorkingArea.Top;
                                            x = aw.DesktopLocation.X;
                                        }
                                        break;
                                    case AutoPositionDirection.UpLeft:
                                    case AutoPositionDirection.UpRight:
                                        y = y - aw.Height;
                                        if (y < screen.Bounds.Top)
                                        {
                                            y = screen.WorkingArea.Bottom - aw.Height;
                                            x = aw.DesktopLocation.X;
                                        }
                                        break;
                                }

                                aw.DesktopLocation = new System.Drawing.Point(x, y);
                                x = aw.DesktopLocation.X;
                                y = (this.direction == AutoPositionDirection.UpLeft || this.direction == AutoPositionDirection.UpRight ? aw.Top - this.verticalPadding : aw.Bottom + this.verticalPadding);
                            }
                        }
                    }
                    else
                    {
                        int x = currentWin.DesktopLocation.X;
                        int y = currentWin.DesktopLocation.Y;
                        foreach (NotificationWindow aw in this.ActiveWindows)
                        {
                            if (aw != currentWin)
                            {
                                switch (this.direction)
                                {
                                    case AutoPositionDirection.DownLeft:
                                        y = currentWin.Bottom + this.verticalPadding;
                                        if (y + aw.Height > screen.Bounds.Bottom)
                                        {
                                            y = screen.WorkingArea.Top;
                                            x = x - currentWin.Size.Width - this.horizontalPadding;
                                        }
                                        break;
                                    case AutoPositionDirection.DownRight:
                                        y = currentWin.Bottom + this.verticalPadding;
                                        if (y + aw.Height > screen.Bounds.Bottom)
                                        {
                                            y = screen.WorkingArea.Top;
                                            x = x + currentWin.Size.Width + this.horizontalPadding;
                                        }
                                        break;
                                    case AutoPositionDirection.UpLeft:
                                        y = currentWin.Top - this.verticalPadding - aw.Height;
                                        if (y < screen.Bounds.Top)
                                        {
                                            y = screen.WorkingArea.Bottom - aw.Height;
                                            x = x - currentWin.Size.Width - this.horizontalPadding;
                                        }
                                        break;
                                    case AutoPositionDirection.UpRight:
                                        y = currentWin.Top - this.verticalPadding - aw.Height;
                                        if (y < screen.Bounds.Top)
                                        {
                                            y = screen.WorkingArea.Bottom - aw.Height;
                                            x = x + currentWin.Size.Width + this.horizontalPadding;
                                        }
                                        break;
                                }

                                aw.DesktopLocation = new System.Drawing.Point(x, y);
                                x = aw.DesktopLocation.X;
                                currentWin = aw;
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// The direction to reposition open notifications
        /// </summary>
        public enum AutoPositionDirection
        {
            /// <summary>
            /// Notifications are repositioned above the new notification until the reach they top of the screen,
            /// and then the start a new column to the right
            /// </summary>
            UpRight,

            /// <summary>
            /// Notifications are repositioned above the new notification until the reach they top of the screen,
            /// and then the start a new column to the left
            /// </summary>
            UpLeft,

            /// <summary>
            /// Notifications are repositioned below the new notification until they reach the bottom of the screen,
            /// and then the start a new column to the right
            /// </summary>
            DownRight,

            /// <summary>
            /// Notifications are repositioned below the new notification until they reach the bottom of the screen,
            /// and then the start a new column to the left
            /// </summary>
            DownLeft
        }
    }

}

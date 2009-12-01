using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the behavior for popping up a notification (like toast)
    /// </summary>
    public class PopupAnimator : AnimatorBase, IDisposable
    {
        /// <summary>
        /// The default interval for the fade timer
        /// </summary>
        private const int TIMER_INTERVAL = 10;

        /// <summary>
        /// The timer that controls the fading
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The form window to be faded
        /// </summary>
        private NotificationWindow form;

        /// <summary>
        /// The amount of time (in milliseconds) over which the pop-up should occur
        /// </summary>
        private int popInDuration = 750;

        /// <summary>
        /// The amount of time (in milliseconds) over which the pop-down should occur
        /// </summary>
        private int popOutDuration = 750;

        /// <summary>
        /// Indicates if the form pop-up process is complete
        /// </summary>
        private bool popInComplete;

        /// <summary>
        /// The direction the window should pop
        /// </summary>
        private PopupDirection direction = PopupDirection.Up;

        /// <summary>
        /// The final X cooridinate that indicates the popup is done (for horizontal directions)
        /// </summary>
        private int finalX;

        /// <summary>
        /// The final Y cooridinate that indicates the popup is done (for vertical directions)
        /// </summary>
        private int finalY;

        /// <summary>
        /// The height of the window region
        /// </summary>
        private int regionHeight = 0;

        /// <summary>
        /// The delta amount to move the window during each cycle
        /// </summary>
        private int interval = 10;

        /// <summary>
        /// Creates a new PopupAnimator using the preset default values.
        /// </summary>
        /// <param name="form">The <see cref="NotificationWindow"/> to animate</param>
        public PopupAnimator(NotificationWindow form)
        {
            this.form = form;
            form.BeforeShown += new EventHandler(form_BeforeShown);
            form.AutoClosing += new System.Windows.Forms.FormClosingEventHandler(form_AutoClosing);

            this.timer = new Timer();
            this.timer.Interval = TIMER_INTERVAL;
            this.timer.Tick += new EventHandler(timer_Tick);
        }

        /// <summary>
        /// Creates a new PopupAnimator specifying the pop behavior values
        /// </summary>
        /// <param name="form">The <see cref="NotificationWindow"/> to animate</param>
        /// <param name="popInDuration">The amount of time (in milliseconds) over which the pop-in should occur</param>
        /// <param name="popOutDuration">The amount of time (in milliseconds) over which the pop-out should occur</param>
        /// <param name="direction">The direction the window should pop</param>
        public PopupAnimator(NotificationWindow form, int popInDuration, int popOutDuration, PopupDirection direction)
            : this(form)
        {
            this.popInDuration = popInDuration;
            this.popOutDuration = popOutDuration;
            this.direction = direction;
        }

        /// <summary>
        /// Handles starting the pop-in animation
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void form_BeforeShown(object sender, EventArgs e)
        {
            double steps = this.popInDuration / TIMER_INTERVAL;

            if (steps > 0)
            {
                this.interval = Convert.ToInt32(this.form.Height / steps);

                finalX = this.form.Location.X;
                finalY = this.form.Location.Y;

                // set initial (hidden) position
                int x = this.form.Location.X;
                int y = this.form.Bottom;
                this.form.Location = new Point(x, y);
                this.form.Region = new Region(new Rectangle(0, 0, this.form.Width, regionHeight));

                this.timer.Start();
            }
            else
            {
                // show as normal
            }
        }

        /// <summary>
        /// Handles starting the pop-out animation
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        /// <remarks>
        /// It is important to set the <c>e.Cancel</c> property to <c>true</c>
        /// if animating so that the calling code does not immediately
        /// close the form
        /// </remarks>
        void form_AutoClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            e.Cancel = Close();
        }

        /// <summary>
        /// Closes the form by animating it away.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the form will animate away;
        /// <c>false</c> if the form should close immediately with no animation
        /// </returns>
        public bool Close()
        {
            bool willAnimate = false;
            this.popInComplete = true;

            double steps = this.popOutDuration / TIMER_INTERVAL;

            if (steps > 0)
            {
                this.interval = Convert.ToInt32(this.form.Height / steps);

                willAnimate = true;

                this.timer.Interval = TIMER_INTERVAL;
                this.timer.Start();
            }
            else
            {
                // let the close continue unobstructed
            }
            return willAnimate;
        }

        /// <summary>
        /// Handles moving the form/region to perform the pop-in effect
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();

            if (!this.Disabled)
            {
                bool restart = true;
                if (this.popInComplete)
                {
                    if (!this.form.PauseWhenMouseOver || !this.form.IsMouseOver())
                    {
                        this.regionHeight -= this.interval;
                        int newTop = this.form.Top + this.interval;
                        if (newTop >= this.finalY)
                        {
                            restart = false;
                            form.Close();
                            return;
                        }
                        else
                        {
                            this.form.Region = new Region(new Rectangle(0, 0, this.form.Width, this.regionHeight));
                            this.form.Top = newTop;
                            this.form.Invalidate();
                        }
                    }
                    else
                    {
                        restart = false;
                        CancelClosing();
                        this.form.StartAutoCloseTimer();
                    }
                }
                else
                {
                    this.regionHeight += this.interval;
                    int newTop = this.form.Top - this.interval;
                    if (newTop <= this.finalY)
                    {
                        this.form.Top = this.finalY;
                        this.form.Region = null;
                        this.form.Invalidate();
                        regionHeight = this.form.Height;
                        finalX = this.form.Location.X;
                        finalY = this.form.Bottom;
                        restart = false;
                    }
                    else
                    {
                        this.form.Region = new Region(new Rectangle(0, 0, this.form.Width, this.regionHeight));
                        this.form.Top = newTop;
                        this.form.Invalidate();
                    }
                }
                if (restart) this.timer.Start();
            }
        }

        /// <summary>
        /// Cancels the closing (and thus, animation) of a display.
        /// </summary>
        /// <remarks>
        /// If the display is not yet closing, this has no effect.
        /// This only cancels the current animation - if the display is closed again later, a new
        /// animation may be started.
        /// </remarks>
        public override void CancelClosing()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
            }
        }

        /// <summary>
        /// Specifies the direction that the window should pop
        /// </summary>
        /// <remarks>
        /// At this time, the only value supported is <c>Up</c>
        /// </remarks>
        public enum PopupDirection
        {
            /// <summary>
            /// The notification window will rise up from a baseline and then slide back down when closed
            /// </summary>
            Up /*,
            Down,
            Right,
            Left */
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null) this.timer.Dispose();
                if (this.form != null) this.form.Dispose();
            }
        }

        #endregion
    }
}

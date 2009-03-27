using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the behavior for fading a form window in and out
    /// </summary>
    public class FadeAnimator : IAnimator
    {
        /// <summary>
        /// The default interval for the fade timer
        /// </summary>
        private const int TIMER_INTERVAL = 50;

        /// <summary>
        /// The maximum value for opacity. (this is kept below 1.0 to avoid a flicker when Windows converts the form to/from a layered window)
        /// </summary>
        private const double MAX_OPACITY = 0.99;

        /// <summary>
        /// The timer that controls the fading
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The form window to be faded
        /// </summary>
        private NotificationWindow form;

        /// <summary>
        /// The amount of time (in milliseconds) over which the fade-in should occur
        /// </summary>
        private int fadeInDuration = 500;

        /// <summary>
        /// The amount of time (in milliseconds) over which the fade-out should occur
        /// </summary>
        private int fadeOutDuration = 1000;

        /// <summary>
        /// The final opacity of the form once faded in
        /// </summary>
        private double finalOpacity = MAX_OPACITY;

        /// <summary>
        /// The amount to change the opacity with each cycle
        /// </summary>
        private double opacityDelta = 0;

        /// <summary>
        /// Indicates if the form fade-in process is complete
        /// </summary>
        private bool fadeInComplete;

        /// <summary>
        /// Indicates if the animator is disabled
        /// </summary>
        private bool disabled;

        /// <summary>
        /// Creates a new FadeAnimator using the preset default values.
        /// </summary>
        /// <param name="form">The <see cref="NotificationWindow"/> to animate</param>
        public FadeAnimator(NotificationWindow form)
        {
            this.form = form;
            form.Opacity = 0;
            form.BeforeShown += new EventHandler(form_BeforeShown);
            form.AutoClosing += new FormClosingEventHandler(form_AutoClosing);

            this.timer = new Timer();
            this.timer.Interval = 50;
            this.timer.Tick += new EventHandler(timer_Tick);
        }

        /// <summary>
        /// Creates a new FadeAnimator specifying the fade behavior values
        /// </summary>
        /// <param name="form">The <see cref="NotificationWindow"/> to animate</param>
        /// <param name="fadeInDuration">The amount of time (in milliseconds) over which the fade-in should occur</param>
        /// <param name="fadeOutDuration">The amount of time (in milliseconds) over which the fade-out should occur</param>
        /// <param name="finalOpacity">The final opacity of the form once faded in</param>
        public FadeAnimator(NotificationWindow form, int fadeInDuration, int fadeOutDuration, double finalOpacity) : this(form)
        {
            this.fadeInDuration = fadeInDuration;
            this.fadeOutDuration = fadeOutDuration;
            this.finalOpacity = finalOpacity;
        }

        /// <summary>
        /// Indicates if the animator is disabled
        /// </summary>
        /// <value>
        /// <c>true</c> - the animator is disabled and should not animate the window;
        /// <c>false</c> - the animator is enabled and should perform its animations
        /// </value>
        public bool Disabled
        {
            get
            {
                return this.disabled;
            }
            set
            {
                this.disabled = true;
            }
        }

        /// <summary>
        /// Handles starting the fade-in animation
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void form_BeforeShown(object sender, EventArgs e)
        {
            if (this.finalOpacity == 1.0) this.finalOpacity = MAX_OPACITY;

            double steps = this.fadeInDuration / TIMER_INTERVAL;

            if (steps > 0)
            {
                form.Opacity = 0;
                this.opacityDelta = this.finalOpacity / steps;
                this.timer.Start();
            }
            else
            {
                form.Opacity = this.finalOpacity;
            }
        }

        /// <summary>
        /// Handles increasing/decreasing the form's opacity as it fades in or out
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();
            bool restart = true;
            if (this.fadeInComplete)
            {
                double newOpacity = form.Opacity - this.opacityDelta;
                if (newOpacity <= 0)
                {
                    newOpacity = 0;
                    restart = false;
                    form.Close();
                    return;
                }
                form.Opacity = newOpacity;
            }
            else
            {
                double newOpacity = form.Opacity + this.opacityDelta;
                if (newOpacity >= this.finalOpacity)
                {
                    newOpacity = finalOpacity;
                    this.fadeInComplete = true;
                    restart = false;
                }
                form.Opacity = newOpacity;
            }
            if(restart) this.timer.Start();
        }

        /// <summary>
        /// Handles starting the fade-out animation
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Information about the event</param>
        /// <remarks>
        /// It is important to set the <c>e.Cancel</c> property to <c>true</c>
        /// if animating so that the calling code does not immediately
        /// close the form
        /// </remarks>
        void form_AutoClosing(object sender, FormClosingEventArgs e)
        {
            this.fadeInComplete = true;

            double steps = this.fadeOutDuration / TIMER_INTERVAL;
            if (steps > 0)
            {
                e.Cancel = true;
                this.opacityDelta = this.finalOpacity / steps;
                this.timer.Start();
            }
            else
            {
                // let the close continue unobstructed
            }
        }
    }
}

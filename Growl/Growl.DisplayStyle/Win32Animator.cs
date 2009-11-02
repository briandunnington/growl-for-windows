using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Animates a form when it is shown, hidden or closed using the Win32 AnimateWindow API.
    /// </summary>
    /// <remarks>
    /// MDI child forms do not support the Blend method and only support other methods while 
    /// being displayed for the first time and when closing.
    /// </remarks>
    public sealed class Win32Animator : IAnimator, IDisposable
    {
        #region Types

        /// <summary>
        /// The methods of animation available.
        /// </summary>
        public enum AnimationMethod
        {
            /// <summary>
            /// Rolls out from edge when showing and into edge when hiding.
            /// </summary>
            /// <remarks>
            /// This is the default animation method and requires a direction.
            /// </remarks>
            Roll = 0x0,
            /// <summary>
            /// Expands out from centre when showing and collapses into centre when hiding.
            /// </summary>
            Centre = 0x10,
            /// <summary>
            /// Slides out from edge when showing and slides into edge when hiding.
            /// </summary>
            /// <remarks>
            /// Requires a direction.
            /// </remarks>
            Slide = 0x40000,
            /// <summary>
            /// Fades from transaprent to opaque when showing and from opaque to transparent when hiding.
            /// </summary>
            Blend = 0x80000
        }

        /// <summary>
        /// The directions in which the Roll and Slide animations can be shown.
        /// </summary>
        /// <remarks>
        /// Horizontal and vertical directions can be combined to create diagonal animations.
        /// </remarks>
        [Flags()]
        public enum AnimationDirection
        {
            /// <summary>
            /// From left to right.
            /// </summary>
            Right = 0x1,
            /// <summary>
            /// From right to left.
            /// </summary>
            Left = 0x2,
            /// <summary>
            /// From top to bottom.
            /// </summary>
            Down = 0x4,
            /// <summary>
            /// From bottom to top.
            /// </summary>
            Up = 0x8
        }

        #endregion // Types

        #region Constants

        /// <summary>
        /// Hide the form.
        /// </summary>
        private const int AW_HIDE = 0x10000;
        /// <summary>
        /// Activate the form.
        /// </summary>
        private const int AW_ACTIVATE = 0x20000;

        /// <summary>
        /// The number of milliseconds over which the animation occurs if no value is specified.
        /// </summary>
        private const int DEFAULT_DURATION = 250;

        #endregion // Constants

        #region Variables

        /// <summary>
        /// The form to be animated.
        /// </summary>
        private Form _form;
        /// <summary>
        /// The animation method used to show and hide the form.
        /// </summary>
        private AnimationMethod _method;
        /// <summary>
        /// The direction in which to Roll or Slide the form.
        /// </summary>
        private AnimationDirection _direction;
        /// <summary>
        /// The number of milliseconds over which the animation is played.
        /// </summary>
        private int _duration;

        private bool disabled;

        #endregion // Variables

        #region Properties

        /// <summary>
        /// Gets or sets the animation method used to show and hide the form.
        /// </summary>
        /// <value>
        /// The animation method used to show and hide the form.
        /// </value>
        /// <remarks>
        /// <b>Roll</b> is used by default if no method is specified.
        /// </remarks>
        public AnimationMethod Method
        {
            get
            {
                return this._method;
            }
            set
            {
                this._method = value;
            }
        }

        /// <summary>
        /// Gets or Sets the direction in which the animation is performed.
        /// </summary>
        /// <value>
        /// The direction in which the animation is performed.
        /// </value>
        /// <remarks>
        /// The direction is only applicable to the <b>Roll</b> and <b>Slide</b> methods.
        /// </remarks>
        public AnimationDirection Direction
        {
            get
            {
                return this._direction;
            }
            set
            {
                this._direction = value;
            }
        }

        /// <summary>
        /// Gets or Sets the number of milliseconds over which the animation is played.
        /// </summary>
        /// <value>
        /// The number of milliseconds over which the animation is played.
        /// </value>
        public int Duration
        {
            get
            {
                return this._duration;
            }
            set
            {
                this._duration = value;
            }
        }

        /// <summary>
        /// Gets the form to be animated.
        /// </summary>
        /// <value>
        /// The form to be animated.
        /// </value>
        public Form Form
        {
            get
            {
                return this._form;
            }
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

        #endregion // Properties

        #region APIs

        /// <summary>
        /// Windows API function to animate a window.
        /// </summary>
        [DllImport("user32")]
        private extern static bool AnimateWindow(IntPtr hWnd,
                                                 int dwTime,
                                                 int dwFlags);

        #endregion // APIs

        #region Constructors

        /// <summary>
        /// Creates a new <b>FormAnimator</b> object for the specified form.
        /// </summary>
        /// <param name="form">
        /// The form to be animated.
        /// </param>
        /// <remarks>
        /// No animation will be used unless the <b>Method</b> and/or <b>Direction</b> properties are set independently. The <b>Duration</b> is set to quarter of a second by default.
        /// </remarks>
        public Win32Animator(Form form)
        {
            this._form = form;

            //this._form.Load += new EventHandler(Form_Load);
            this._form.VisibleChanged += new EventHandler(Form_VisibleChanged);
            this._form.Closing += new CancelEventHandler(Form_Closing);

            this._duration = DEFAULT_DURATION;
        }

        /// <summary>
        /// Creates a new <b>FormAnimator</b> object for the specified form using the specified method over the specified duration.
        /// </summary>
        /// <param name="form">
        /// The form to be animated.
        /// </param>
        /// <param name="method">
        /// The animation method used to show and hide the form.
        /// </param>
        /// <param name="duration">
        /// The number of milliseconds over which the animation is played.
        /// </param>
        /// <remarks>
        /// No animation will be used for the <b>Roll</b> or <b>Slide</b> methods unless the <b>Direction</b> property is set independently.
        /// </remarks>
        public Win32Animator(Form form,
                            AnimationMethod method,
                            int duration)
            : this(form)
        {
            this._method = method;
            this._duration = duration;
        }

        /// <summary>
        /// Creates a new <b>FormAnimator</b> object for the specified form using the specified method in the specified direction over the specified duration.
        /// </summary>
        /// <param name="form">
        /// The form to be animated.
        /// </param>
        /// <param name="method">
        /// The animation method used to show and hide the form.
        /// </param>
        /// <param name="direction">
        /// The direction in which to animate the form.
        /// </param>
        /// <param name="duration">
        /// The number of milliseconds over which the animation is played.
        /// </param>
        /// <remarks>
        /// The <i>direction</i> argument will have no effect if the <b>Centre</b> or <b>Blend</b> method is
        /// specified.
        /// </remarks>
        public Win32Animator(Form form,
                            AnimationMethod method,
                            AnimationDirection direction,
                            int duration)
            : this(form, method, duration)
        {
            this._direction = direction;
        }

        #endregion // Constructors

        #region Event Handlers

        /*
        /// <summary>
        /// Animates the form automatically when it is loaded.
        /// </summary>
        private void Form_Load(object sender, EventArgs e)
        {
            if (!this.disabled)
            {
                // MDI child forms do not support transparency so do not try to use the Blend method.
                if (this._form.MdiParent == null || this._method != AnimationMethod.Blend)
                {
                    // Activate the form.
                    AnimateWindow(this._form.Handle,
                                  this._duration,
                                  AW_ACTIVATE | (int)this._method | (int)this._direction);
                }
            }
        }
         * */

        /// <summary>
        /// Animates the form automatically when it is shown or hidden.
        /// </summary>
        private void Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.disabled)
            {
                // Do not attempt to animate MDI child forms while showing or hiding as they do not behave as expected.
                if (this._form.MdiParent == null)
                {
                    int flags = (int)this._method | (int)this._direction;

                    if (this._form.Visible)
                    {
                        // Activate the form.
                        flags = flags | AW_ACTIVATE;
                    }
                    else
                    {
                        // Hide the form.
                        flags = flags | AW_HIDE;
                    }

                    AnimateWindow(this._form.Handle,
                                  this._duration,
                                  flags);
                }
            }
        }

        /// <summary>
        /// Animates the form automatically when it closes.
        /// </summary>
        private void Form_Closing(object sender, CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                if (!this.disabled)
                {
                    // MDI child forms do not support transparency so do not try to use the Blend method.
                    if (this._form.MdiParent == null || this._method != AnimationMethod.Blend)
                    {
                        // Hide the form.
                        AnimateWindow(this._form.Handle,
                                      this._duration,
                                      AW_HIDE | (int)this._method | (int)this._direction);
                    }
                }
            }
        }

        #endregion // Event Handlers

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
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._form != null) this._form.Dispose();
            }
        }

        #endregion
    }
}

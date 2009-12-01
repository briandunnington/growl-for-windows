using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Base implementation for animators used in animated displays
    /// </summary>
    public abstract class AnimatorBase : IAnimator
    {
        private bool disabled;

        #region IAnimator Members

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
                this.disabled = value;
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
        public abstract void CancelClosing();

        #endregion
    }
}

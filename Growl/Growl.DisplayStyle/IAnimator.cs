using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Defines properties for classes that perform window animation
    /// </summary>
    /// <remarks>
    /// Normally, implementors should hook into the <see cref="NotificationWindow.BeforeShown"/>
    /// and <see cref="NotificationWindow.AutoClosing"/> events for their animation
    /// routines.
    /// <c>BeforeShown</c> is preferred because it is after <c>Load</c> and <c>AfterLoad</c>, 
    /// which are recommended places to determine the form's intial size and position, both 
    /// of which are probably necessary for animation. It is also after an LayoutManager 
    /// repositioning, which is useful if your animation will be resizing the form into view
    /// (by ensuring there will be enough free space for the final form size).
    /// <c>AutoClosing</c> is preferred because it happens before <c>FormClosing</c>, allowing 
    /// implementors the ability to control when the form is actually closed (usually after
    /// the animation). <c>AutoClosing</c> is also only fired when the notification has been
    /// ignored by the user, which is an appropriate time to animate away. If the form is 
    /// actively closed by the user via click, the window should close immediately in most
    /// cases and skip any animation. If the notification is closed via keyboard shortcut
    /// (<c>CloseAllOpenNotifications</c> or <c>CloseLastNotification</c>), the windows
    /// should be closed immediately in all cases and not animated away.
    /// </remarks>
    public interface IAnimator
    {
        /// <summary>
        /// Indicates if the animator is disabled (and thus should not peform any animation)
        /// </summary>
        /// <remarks>
        /// <see cref="IAnimator"/> implementors should take care to check this property before
        /// performing any animations. Growl's visual notifications can be animated when shown, or
        /// when automatically closing after a time-out, but they should close themselves immediately
        /// (and not animate) when called from <see cref="VisualDisplay.CloseAllOpenNotifications"/>
        /// or <see cref="VisualDisplay.CloseLastNotification"/>. The <c>Disabled</c> property will
        /// automatically be set to <c>true</c> for calls from either of those methods, so it is best
        /// to simply check it before animating. Also, <c>AutoClose</c> is not called in those cases,
        /// so as long as you hook into that event for your closing animation, you should be OK
        /// anyway.
        /// </remarks>
        bool Disabled { get;set;}
    }
}

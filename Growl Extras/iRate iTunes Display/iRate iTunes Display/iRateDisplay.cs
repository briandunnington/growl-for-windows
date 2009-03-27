using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace iRate
{
    public class iRateDisplay : VisualDisplay
    {
        LayoutManager lm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 10, 10);

        public override string Author
        {
            get { return "Vortex Software"; }
        }

        public override string Description
        {
            get { return "Displays iTunes track information and allows rating of the song."; }
        }

        public override string Name
        {
            get { return "iRate for iTunes"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Website
        {
            get { return "http://www.website.net"; }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            iRateWindow win = new iRateWindow();
            win.SetNotification(notification);
            Show(win);
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return this.lm;
        }
    }
}

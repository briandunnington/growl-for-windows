using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public interface IRegisteredObject
    {
        string Name { get;}
        //System.Drawing.Image Icon { get;set;}
        string IconID { get;set;}
        System.Drawing.Image GetIcon();
        bool Enabled { get;}
        //Display Display { get;}

        Growl.Connector.Priority Priority(Growl.Connector.Priority requestedPriority);

        bool ShouldStayOnScreen(bool stayWhenIdle, bool isUserIdle, bool requested);

        bool ShouldForward(bool forwardingEnabled, out List<string> limitToTheseComputers);

        bool ShouldPlaySound(PrefSound defaultSound, out string soundFile);
    }
}

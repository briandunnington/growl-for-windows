using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.UI
{
    public delegate void ValidChangedEventHandler(bool isValid);

    public interface IForwardInputs
    {
        event ValidChangedEventHandler ValidChanged;
        void Initialize(bool isSubscription, ForwardListItem obj);
        System.Windows.Forms.UserControl GetControl();
        ForwardComputer Save();
    }
}

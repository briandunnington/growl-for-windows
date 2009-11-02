using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class ForwardDestinationSettingsPanel : UserControl
    {
        public delegate void ValidChangedEventHandler(bool isValid);

        public event ValidChangedEventHandler ValidChanged;

        public ForwardDestinationSettingsPanel()
        {
            InitializeComponent();
        }

        public virtual void Initialize(bool isSubscription, ForwardDestinationListItem fdli, ForwardDestination fd)
        {
            throw new NotImplementedException();
        }

        public virtual ForwardDestination Create()
        {
            throw new NotImplementedException();
        }

        public virtual void Update(ForwardDestination fd)
        {
            throw new NotImplementedException();
        }

        protected void OnValidChanged(bool isValid)
        {
            if (ValidChanged != null)
            {
                ValidChanged(isValid);
            }
        }
    }
}

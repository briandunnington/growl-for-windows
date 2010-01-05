using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class DestinationSettingsPanel : UserControl
    {
        public delegate void ValidChangedEventHandler(bool isValid);

        public event ValidChangedEventHandler ValidChanged;

        public DestinationSettingsPanel()
        {
            InitializeComponent();
        }

        public virtual void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase db)
        {
            throw new NotImplementedException();
        }

        public virtual DestinationBase Create()
        {
            throw new NotImplementedException();
        }

        public virtual void Update(DestinationBase db)
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

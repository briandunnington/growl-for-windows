using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace GrowlExtras.Subscribers.PhonyBalloony
{
    public partial class PhonyBalloonySettings : DestinationSettingsPanel
    {
        public PhonyBalloonySettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the configuration UI when a subscription is being added or edited.
        /// </summary>
        /// <param name="isSubscription">will always be <c>true</c> for <see cref="Subscription"/>s</param>
        /// <param name="dli">The <see cref="DestinationListItem"/> that the user selected</param>
        /// <param name="db">The <see cref="DestinationBase"/> of the item if it is being edited;<c>null</c> otherwise</param>
        /// <remarks>
        /// When an instance is being edited (<paramref name="dli"/> != null), make sure to repopulate any
        /// inputs with the current values.
        /// 
        /// By default, the 'Save' button is disabled and you must call <see cref="DestinationSettingsPanel.OnValidChanged"/>
        /// in order to enable it when appropriate.
        /// </remarks>
        public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase db)
        {
            OnValidChanged(true);
        }

        /// <summary>
        /// Creates a new instance of the subscriber.
        /// </summary>
        /// <returns>New <see cref="FeedSubscription"/></returns>
        /// <remarks>
        /// This is called when the user is adding a new subscription and clicks the 'Save' button.
        /// </remarks>
        public override DestinationBase Create()
        {
            PhonyBalloonySubscription pbs = new PhonyBalloonySubscription(true);
            pbs.Subscribe();
            return pbs;
        }

        /// <summary>
        /// Updates the specified subscription instance.
        /// </summary>
        /// <param name="db">The <see cref="FeedSubscription"/> to update</param>
        /// <remarks>
        /// This is called when a user is editing an existing subscription and clicks the 'Save' button.
        /// </remarks>
        public override void Update(DestinationBase db)
        {
            // do nothing
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.Destinations
{
    /// <summary>
    /// Contains the UI used to create/edit/configure destinations.
    /// </summary>
    public partial class DestinationSettingsPanel : UserControl
    {
        /// <summary>
        /// Event handler for the <see cref="DestinationSettingsPanel.ValidChanged"/> event.
        /// </summary>
        public delegate void ValidChangedEventHandler(bool isValid);

        /// <summary>
        /// Occurs when the input values change between valid/invalid.
        /// </summary>
        public event ValidChangedEventHandler ValidChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationSettingsPanel"/> class.
        /// </summary>
        protected DestinationSettingsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the settings UI panel.
        /// </summary>
        /// <param name="isSubscription"><c>true</c> if the destination is a <see cref="Subscription"/>;<c>false</c> if the destination is a <see cref="ForwardDestination"/></param>
        /// <param name="fdli">The <see cref="DestinationListItem"/> selected by the user.</param>
        /// <param name="db">The <see cref="DestinationBase"/> being edited if editing an existing instance;<c>null</c> otherwise</param>
        public virtual void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase db)
        {
            throw new NotImplementedException("DestinationSettingsPanel.Initialize() not implemented");
        }

        /// <summary>
        /// Creates a new instance of the destination.
        /// </summary>
        /// <returns>New <see cref="DestinationBase"/></returns>
        public virtual DestinationBase Create()
        {
            throw new NotImplementedException("DestinationSettingsPanel.Create() not implemented");
        }

        /// <summary>
        /// Updates the specified destination instance.
        /// </summary>
        /// <param name="db">The <see cref="DestinationBase"/> to update</param>
        public virtual void Update(DestinationBase db)
        {
            throw new NotImplementedException("DestinationSettingsPanel.Update() not implemented");
        }

        /// <summary>
        /// Called when the input values change between valid/invalid.
        /// </summary>
        /// <param name="isValid"><c>true</c> if all inputs are valid;<c>false</c> otherwise</param>
        protected void OnValidChanged(bool isValid)
        {
            if (ValidChanged != null)
            {
                ValidChanged(isValid);
            }
        }
    }
}

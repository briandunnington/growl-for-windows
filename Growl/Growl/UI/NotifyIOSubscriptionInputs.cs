using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace Growl.UI
{
    public partial class NotifyIOSubscriptionInputs : DestinationSettingsPanel
    {
        private bool doValidation;

        public NotifyIOSubscriptionInputs()
        {
            InitializeComponent();

            // localize text
            this.labelDescription.Text = "Name";    //TODO: //LOCAL: //LOCALIZE:
            this.labelOutletUrl.Text = "Outlet Url";
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxAPIKey_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

         public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase fd)
        {
            this.doValidation = true;

            // set text box values
            this.textBoxDescription.Text = String.Empty;
            this.textBoxDescription.Enabled = true;
            this.textBoxOutletUrl.Text = String.Empty;
            this.textBoxOutletUrl.Enabled = true;

            NotifyIOSubscription nios = fd as NotifyIOSubscription;
            if (nios != null)
            {
                this.textBoxDescription.Text = nios.Description;
                this.textBoxOutletUrl.Text = nios.OutletUrl;
            }

            ValidateInputs();

            this.textBoxDescription.Focus();
        }

        public override DestinationBase Create()
        {
            NotifyIOSubscription nios = new NotifyIOSubscription(this.textBoxDescription.Text, true, this.textBoxOutletUrl.Text);
            return nios;
        }

        public override void Update(DestinationBase fd)
        {
            NotifyIOSubscription nios = fd as NotifyIOSubscription;
            if (nios != null)
            {
                nios.Description = this.textBoxDescription.Text;
                nios.OutletUrl = this.textBoxOutletUrl.Text;
                nios.Subscribe();
            }
        }

        private void ValidateInputs()
        {
            bool valid = true;
            if (this.doValidation)
            {
                if (String.IsNullOrEmpty(this.textBoxDescription.Text))
                {
                    this.textBoxDescription.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxDescription.Unhighlight();
                }

                if (String.IsNullOrEmpty(this.textBoxOutletUrl.Text))
                {
                    this.textBoxOutletUrl.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxOutletUrl.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }
    }
}

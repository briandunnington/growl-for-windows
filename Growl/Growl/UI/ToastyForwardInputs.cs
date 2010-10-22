using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace Growl.UI
{
    public partial class ToastyForwardInputs : DestinationSettingsPanel
    {
        private bool doValidation;

        public ToastyForwardInputs()
        {
            InitializeComponent();
        }

        public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase fd)
        {
            this.doValidation = true;

            this.comboBoxMinimumPriority.Items.Add(new PriorityChoice(null));
            Array priorities = Enum.GetValues(typeof(Growl.Connector.Priority));
            Array.Sort(priorities, new PrioritySortComparer());
            foreach(Growl.Connector.Priority priority in priorities)
            {
                this.comboBoxMinimumPriority.Items.Add(new PriorityChoice(priority));
            }
            this.comboBoxQuietDays.Items.Add(ToastyForwardDestination.QuietHoursDayChoice.Everyday);
            this.comboBoxQuietDays.Items.Add(ToastyForwardDestination.QuietHoursDayChoice.Weekdays);
            this.comboBoxQuietDays.Items.Add(ToastyForwardDestination.QuietHoursDayChoice.Weekends);

            // set initial values
            this.textBoxDescription.Text = String.Empty;
            this.textBoxDescription.Enabled = true;
            this.textBoxDeviceID.Text = String.Empty;
            this.textBoxDeviceID.Enabled = true;
            this.comboBoxMinimumPriority.SelectedIndex = 0;
            this.comboBoxMinimumPriority.Enabled = true;
            this.comboBoxQuietDays.SelectedIndex = 0;

            ToastyForwardDestination tfd = fd as ToastyForwardDestination;
            if (tfd != null)
            {
                this.textBoxDescription.Text = tfd.Description;
                this.textBoxDeviceID.Text = tfd.DeviceID;
                if (tfd.MinimumPriority != null && tfd.MinimumPriority.HasValue)
                {
                    foreach (object item in this.comboBoxMinimumPriority.Items)
                    {
                        PriorityChoice pc = (PriorityChoice)item;
                        if(pc.Priority == tfd.MinimumPriority)
                        this.comboBoxMinimumPriority.SelectedItem = item;
                    }
                }
                this.checkBoxOnlyWhenIdle.Checked = tfd.OnlyWhenIdle;
                this.checkBoxQuietHours.Checked = tfd.EnableQuietHours;
                this.comboBoxQuietDays.SelectedItem = tfd.QuietHoursDaysChoice;
                this.dateTimePickerStart.Value = tfd.QuietHoursStart;
                this.dateTimePickerEnd.Value = tfd.QuietHoursEnd;
            }

            ValidateInputs();

            this.textBoxDescription.Focus();
        }

        public override DestinationBase Create()
        {
            PriorityChoice pc = (PriorityChoice)this.comboBoxMinimumPriority.SelectedItem;
            Growl.Connector.Priority? priority = pc.Priority;
            ToastyForwardDestination.QuietHoursDayChoice quietHoursDays = (ToastyForwardDestination.QuietHoursDayChoice)this.comboBoxQuietDays.SelectedItem;
            ToastyForwardDestination tfd = new ToastyForwardDestination(this.textBoxDescription.Text, true, this.textBoxDeviceID.Text, priority, this.checkBoxOnlyWhenIdle.Checked, this.checkBoxQuietHours.Checked, this.dateTimePickerStart.Value, this.dateTimePickerEnd.Value, quietHoursDays);
            SendConfirmation(tfd);
            return tfd;
        }

        public override void Update(DestinationBase fd)
        {
            ToastyForwardDestination tfd = fd as ToastyForwardDestination;
            if (tfd != null)
            {
                tfd.Description = this.textBoxDescription.Text;
                tfd.DeviceID = this.textBoxDeviceID.Text;
                tfd.OnlyWhenIdle = this.checkBoxOnlyWhenIdle.Checked;
                tfd.EnableQuietHours = this.checkBoxQuietHours.Checked;
                tfd.QuietHoursStart = this.dateTimePickerStart.Value;
                tfd.QuietHoursEnd = this.dateTimePickerEnd.Value;
                tfd.QuietHoursDaysChoice = (ToastyForwardDestination.QuietHoursDayChoice)this.comboBoxQuietDays.SelectedItem;

                PriorityChoice pc = (PriorityChoice)this.comboBoxMinimumPriority.SelectedItem;
                Growl.Connector.Priority? priority = pc.Priority;
                tfd.MinimumPriority = priority;

                SendConfirmation(tfd);
            }
        }

        private void SendConfirmation(ToastyForwardDestination tfd)
        {
            // always use Emergency priority in case they have it configured to restrict by priority
            Growl.Connector.Notification notification = new Growl.Connector.Notification("Growl", "Toasty Test", null, "Toasty Test", "You have successfully configured Growl to forward notifications to Toasty", Properties.Resources.toasty, false, Growl.Connector.Priority.Emergency, null);
            // always use isIdle in case they have it configured to only send when idle
            tfd.ForwardNotification(notification, null, null, true, null);
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

                if (String.IsNullOrEmpty(this.textBoxDeviceID.Text))
                {
                    this.textBoxDeviceID.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxDeviceID.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void checkBoxQuietHours_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxQuietDays.Enabled = checkBoxQuietHours.Checked;
            dateTimePickerStart.Enabled = checkBoxQuietHours.Checked;
            dateTimePickerEnd.Enabled = checkBoxQuietHours.Checked;
            labelTo.Enabled = checkBoxQuietHours.Checked;
        }

        private class PrioritySortComparer : System.Collections.IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                int xi = (int)x;
                int yi = (int)y;
                return -xi.CompareTo(yi);
            }

            #endregion
        }

        private class PriorityChoice
        {
            Growl.Connector.Priority? priority;
            string name;

            public PriorityChoice(Growl.Connector.Priority? priority)
            {
                this.priority = priority;
                if (priority != null && this.priority.HasValue)
                    this.name = ToastyForwardDestinationHandler.Fetch(priority);
                else
                    this.name = "[Any Priority]";
            }

            public Growl.Connector.Priority? Priority
            {
                get
                {
                    return this.priority;
                }
            }

            public override string ToString()
            {
                return this.name;
            }
        }
}
}

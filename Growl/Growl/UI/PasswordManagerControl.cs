using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class PasswordManagerControl : UserControl
    {
        public event EventHandler Updated;

        private const int NONE = -1;
        private ToolTip tooltip;
        private int currentTooltipItem = NONE;
        private Growl.Connector.PasswordManager pm;
        private Color highlightColor = Color.FromArgb(254, 250, 184);

        public PasswordManagerControl()
        {
            InitializeComponent();

            // localize text
            this.contextMenuEdit.Text = Properties.Resources.Security_PasswordManager_Edit;
            this.labelDescriptionBox.Text = Properties.Resources.Security_PasswordManager_DescriptionLabel;
            this.labelPasswordBox.Text = Properties.Resources.Security_PasswordManager_PasswordLabel;
            this.labelInstructions.Text = Properties.Resources.Security_PasswordManager_Instructions;
            this.buttonCancel.Text = Properties.Resources.Button_Cancel;
            this.buttonSave.Text = Properties.Resources.Button_Save;

            this.tooltip = new ToolTip();
        }

        public void SetPasswordManager(Growl.Connector.PasswordManager passwordManager)
        {
            this.pm = passwordManager;
            RefreshItems();
        }

        private void RefreshItems()
        {
            this.passwordListBox.SuspendLayout();
            this.passwordListBox.Items.Clear();
            foreach (Growl.Connector.Password password in this.pm.Passwords.Values)
            {
                if (password.Permanent)
                {
                    PasswordManagerControlListItem pi = new PasswordManagerControlListItem(password);
                    this.passwordListBox.Items.Add(pi);
                }
            }
            this.passwordListBox.ResumeLayout();
        }

        private void PasswordManagerControl_Load(object sender, EventArgs e)
        {
            this.textBoxPassword.HighlightColor = highlightColor;
            this.textBoxDescription.HighlightColor = highlightColor;
        }

        private void passwordListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int index = this.passwordListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (index != this.currentTooltipItem)
                {
                    this.currentTooltipItem = index;
                    PasswordManagerControlListItem pi = (PasswordManagerControlListItem)this.passwordListBox.Items[index];
                    this.tooltip.Show(pi.Password.Description, this.passwordListBox, e.Location, 2000);
                }
                // otherwise, dont keep moving the tooltip around
            }
            else
            {
                this.tooltip.Hide(this.passwordListBox);
                this.currentTooltipItem = NONE;
            }
        }

        private void passwordListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.passwordListBox.SelectedIndex != ListBox.NoMatches)
            {
                this.buttonRemovePassword.Enabled = true;
            }
            else
            {
                this.buttonRemovePassword.Enabled = false;
            }
        }

        private void buttonRemovePassword_Click(object sender, EventArgs e)
        {
            if (this.passwordListBox.SelectedIndex != ListBox.NoMatches)
            {
                PasswordManagerControlListItem pi = (PasswordManagerControlListItem)this.passwordListBox.Items[this.passwordListBox.SelectedIndex];
                this.pm.Remove(pi.Password.ActualPassword);
                this.passwordListBox.Items.RemoveAt(this.passwordListBox.SelectedIndex);
                this.passwordListBox.ClearSelected();
                this.OnUpdated();
            }
        }

        private void passwordListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (this.passwordListBox.Items != null && this.passwordListBox.Items.Count > 0)
            {
                if (e.Index != ListBox.NoMatches)
                {
                    object obj = this.passwordListBox.Items[e.Index];

                    // handle background
                    if ((e.State & DrawItemState.Selected) != 0)
                    {
                        e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                        ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
                    }
                    else
                    {
                        e.DrawBackground();
                    }

                    // handle text
                    e.Graphics.DrawString(obj.ToString(), e.Font, SystemBrushes.ControlText, e.Bounds.X, e.Bounds.Y, StringFormat.GenericDefault);
                }
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.passwordListBox.SelectedIndex != ListBox.NoMatches)
            {
                PasswordManagerControlListItem pi = (PasswordManagerControlListItem)this.contextMenuEdit.Tag;

                this.textBoxPassword.Text = pi.Password.ActualPassword;
                this.textBoxDescription.Text = pi.Password.Description;
                ValidateInputs();
                this.editPanel.Tag = pi;
                this.editPanel.Visible = true;
            }
        }

        private void passwordListBox_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuEdit.Hide();

            if (e.Button == MouseButtons.Right)
            {
                int index = this.passwordListBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    this.passwordListBox.SelectedIndex = index;
                    this.contextMenuEdit.Tag = this.passwordListBox.SelectedItem;
                    this.contextMenuEdit.Show(this.passwordListBox, e.Location);
                }
            }
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.editPanel.Visible = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string passwordToRemove = this.textBoxPassword.Text;
            if (this.editPanel.Tag != null)
            {
                PasswordManagerControlListItem pi = (PasswordManagerControlListItem)this.editPanel.Tag;
                passwordToRemove = pi.Password.ActualPassword;
            }

            this.pm.Remove(passwordToRemove); // just in case the same password already exists
            Growl.Connector.Password p = new Growl.Connector.Password(this.textBoxPassword.Text, this.textBoxDescription.Text, true);
            this.pm.Add(p);
            RefreshItems();

            this.editPanel.Tag = null;
            this.editPanel.Visible = false;
            this.OnUpdated();
        }

        private void ValidateInputs()
        {
            bool valid = true;

            if (String.IsNullOrEmpty(this.textBoxPassword.Text))
            {
                this.textBoxPassword.Highlight();
                valid = false;
            }
            else
            {
                this.textBoxPassword.Unhighlight();
            }

            if (String.IsNullOrEmpty(this.textBoxDescription.Text))
            {
                this.textBoxDescription.Highlight();
                valid = false;
            }
            else
            {
                this.textBoxDescription.Unhighlight();
            }

            this.buttonSave.Enabled = valid;
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void buttonAddPassword_Click(object sender, EventArgs e)
        {
            this.textBoxPassword.Text = "";
            this.textBoxDescription.Text = "";
            ValidateInputs();
            this.editPanel.Tag = null;
            this.editPanel.Visible = true;
            this.textBoxPassword.Focus();
        }

        /// <summary>
        /// Fires the Updated event.
        /// </summary>
        protected void OnUpdated()
        {
            if (this.Updated != null)
            {
                this.Updated(this, EventArgs.Empty);
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.tooltip != null)
                {
                    this.tooltip.Dispose();
                    this.tooltip = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}

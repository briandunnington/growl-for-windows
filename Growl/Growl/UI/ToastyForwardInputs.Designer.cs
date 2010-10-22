namespace Growl.UI
{
    partial class ToastyForwardInputs
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxOnlyWhenIdle = new System.Windows.Forms.CheckBox();
            this.comboBoxMinimumPriority = new System.Windows.Forms.ComboBox();
            this.labelMinimumPriority = new System.Windows.Forms.Label();
            this.textBoxDeviceID = new Growl.Destinations.HighlightTextBox();
            this.labelDeviceID = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.Destinations.HighlightTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxQuietHours = new System.Windows.Forms.CheckBox();
            this.comboBoxQuietDays = new System.Windows.Forms.ComboBox();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.labelTo = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.labelTo);
            this.panelDetails.Controls.Add(this.dateTimePickerEnd);
            this.panelDetails.Controls.Add(this.dateTimePickerStart);
            this.panelDetails.Controls.Add(this.comboBoxQuietDays);
            this.panelDetails.Controls.Add(this.checkBoxQuietHours);
            this.panelDetails.Controls.Add(this.checkBoxOnlyWhenIdle);
            this.panelDetails.Controls.Add(this.comboBoxMinimumPriority);
            this.panelDetails.Controls.Add(this.labelMinimumPriority);
            this.panelDetails.Controls.Add(this.textBoxDeviceID);
            this.panelDetails.Controls.Add(this.labelDeviceID);
            this.panelDetails.Controls.Add(this.textBoxDescription);
            this.panelDetails.Controls.Add(this.labelDescription);
            // 
            // checkBoxOnlyWhenIdle
            // 
            this.checkBoxOnlyWhenIdle.AutoSize = true;
            this.checkBoxOnlyWhenIdle.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.checkBoxOnlyWhenIdle.Location = new System.Drawing.Point(28, 100);
            this.checkBoxOnlyWhenIdle.Name = "checkBoxOnlyWhenIdle";
            this.checkBoxOnlyWhenIdle.Size = new System.Drawing.Size(204, 22);
            this.checkBoxOnlyWhenIdle.TabIndex = 5;
            this.checkBoxOnlyWhenIdle.Text = "Only forward when idle or away";
            this.checkBoxOnlyWhenIdle.UseVisualStyleBackColor = true;
            // 
            // comboBoxMinimumPriority
            // 
            this.comboBoxMinimumPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinimumPriority.FormattingEnabled = true;
            this.comboBoxMinimumPriority.Location = new System.Drawing.Point(47, 78);
            this.comboBoxMinimumPriority.Name = "comboBoxMinimumPriority";
            this.comboBoxMinimumPriority.Size = new System.Drawing.Size(109, 21);
            this.comboBoxMinimumPriority.TabIndex = 4;
            // 
            // labelMinimumPriority
            // 
            this.labelMinimumPriority.AutoSize = true;
            this.labelMinimumPriority.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMinimumPriority.Location = new System.Drawing.Point(25, 58);
            this.labelMinimumPriority.Name = "labelMinimumPriority";
            this.labelMinimumPriority.Size = new System.Drawing.Size(220, 18);
            this.labelMinimumPriority.TabIndex = 15;
            this.labelMinimumPriority.Text = "Only forward when priority is at least:";
            // 
            // textBoxDeviceID
            // 
            this.textBoxDeviceID.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxDeviceID.Location = new System.Drawing.Point(115, 24);
            this.textBoxDeviceID.Name = "textBoxDeviceID";
            this.textBoxDeviceID.Size = new System.Drawing.Size(199, 20);
            this.textBoxDeviceID.TabIndex = 2;
            this.textBoxDeviceID.TextChanged += new System.EventHandler(this.textBoxUsername_TextChanged);
            // 
            // labelDeviceID
            // 
            this.labelDeviceID.AutoSize = true;
            this.labelDeviceID.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeviceID.Location = new System.Drawing.Point(25, 25);
            this.labelDeviceID.Name = "labelDeviceID";
            this.labelDeviceID.Size = new System.Drawing.Size(62, 18);
            this.labelDeviceID.TabIndex = 13;
            this.labelDeviceID.Text = "Device ID:";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxDescription.Location = new System.Drawing.Point(115, 3);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(199, 20);
            this.textBoxDescription.TabIndex = 1;
            this.textBoxDescription.TextChanged += new System.EventHandler(this.textBoxDescription_TextChanged);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescription.Location = new System.Drawing.Point(25, 4);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(42, 18);
            this.labelDescription.TabIndex = 11;
            this.labelDescription.Text = "Name:";
            // 
            // checkBoxQuietHours
            // 
            this.checkBoxQuietHours.AutoSize = true;
            this.checkBoxQuietHours.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.checkBoxQuietHours.Location = new System.Drawing.Point(28, 119);
            this.checkBoxQuietHours.Name = "checkBoxQuietHours";
            this.checkBoxQuietHours.Size = new System.Drawing.Size(231, 22);
            this.checkBoxQuietHours.TabIndex = 19;
            this.checkBoxQuietHours.Text = "Suppress notifications (Quiet hours):";
            this.checkBoxQuietHours.UseVisualStyleBackColor = true;
            this.checkBoxQuietHours.CheckedChanged += new System.EventHandler(this.checkBoxQuietHours_CheckedChanged);
            // 
            // comboBoxQuietDays
            // 
            this.comboBoxQuietDays.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxQuietDays.Enabled = false;
            this.comboBoxQuietDays.FormattingEnabled = true;
            this.comboBoxQuietDays.Location = new System.Drawing.Point(47, 140);
            this.comboBoxQuietDays.Name = "comboBoxQuietDays";
            this.comboBoxQuietDays.Size = new System.Drawing.Size(105, 21);
            this.comboBoxQuietDays.TabIndex = 20;
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.CustomFormat = "h:mm tt";
            this.dateTimePickerStart.Enabled = false;
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(158, 139);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.ShowUpDown = true;
            this.dateTimePickerStart.Size = new System.Drawing.Size(74, 20);
            this.dateTimePickerStart.TabIndex = 21;
            this.dateTimePickerStart.Value = new System.DateTime(2010, 4, 12, 22, 0, 0, 0);
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.CustomFormat = "h:mm tt";
            this.dateTimePickerEnd.Enabled = false;
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(252, 139);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.ShowUpDown = true;
            this.dateTimePickerEnd.Size = new System.Drawing.Size(74, 20);
            this.dateTimePickerEnd.TabIndex = 22;
            this.dateTimePickerEnd.Value = new System.DateTime(2010, 4, 12, 23, 0, 0, 0);
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(233, 143);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(16, 13);
            this.labelTo.TabIndex = 23;
            this.labelTo.Text = "to";
            // 
            // ToastyForwardInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ToastyForwardInputs";
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxOnlyWhenIdle;
        private System.Windows.Forms.ComboBox comboBoxMinimumPriority;
        private System.Windows.Forms.Label labelMinimumPriority;
        private Growl.Destinations.HighlightTextBox textBoxDeviceID;
        private System.Windows.Forms.Label labelDeviceID;
        private Growl.Destinations.HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkBoxQuietHours;
        private System.Windows.Forms.ComboBox comboBoxQuietDays;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label labelTo;
    }
}

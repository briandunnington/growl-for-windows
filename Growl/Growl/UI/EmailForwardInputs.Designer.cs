namespace Growl.UI
{
    partial class EmailForwardInputs
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
            this.comboBoxMinimumPriority = new System.Windows.Forms.ComboBox();
            this.labelMinimumPriority = new System.Windows.Forms.Label();
            this.textBoxUsername = new Growl.UI.HighlightTextBox();
            this.labelEmail = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.UI.HighlightTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.checkBoxOnlyWhenIdle = new System.Windows.Forms.CheckBox();
            this.labelSMTPSettings = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxMinimumPriority
            // 
            this.comboBoxMinimumPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinimumPriority.FormattingEnabled = true;
            this.comboBoxMinimumPriority.Location = new System.Drawing.Point(109, 111);
            this.comboBoxMinimumPriority.Name = "comboBoxMinimumPriority";
            this.comboBoxMinimumPriority.Size = new System.Drawing.Size(109, 21);
            this.comboBoxMinimumPriority.TabIndex = 9;
            // 
            // labelMinimumPriority
            // 
            this.labelMinimumPriority.AutoSize = true;
            this.labelMinimumPriority.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMinimumPriority.Location = new System.Drawing.Point(19, 85);
            this.labelMinimumPriority.Name = "labelMinimumPriority";
            this.labelMinimumPriority.Size = new System.Drawing.Size(220, 18);
            this.labelMinimumPriority.TabIndex = 8;
            this.labelMinimumPriority.Text = "Only forward when priority is at least:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.HighlightColor = System.Drawing.Color.Red;
            this.textBoxUsername.Location = new System.Drawing.Point(109, 33);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(199, 20);
            this.textBoxUsername.TabIndex = 3;
            this.textBoxUsername.TextChanged += new System.EventHandler(this.textBoxUsername_TextChanged);
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmail.Location = new System.Drawing.Point(19, 34);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(86, 18);
            this.labelEmail.TabIndex = 2;
            this.labelEmail.Text = "Email Address:";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.HighlightColor = System.Drawing.Color.Red;
            this.textBoxDescription.Location = new System.Drawing.Point(109, 7);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(199, 20);
            this.textBoxDescription.TabIndex = 1;
            this.textBoxDescription.TextChanged += new System.EventHandler(this.textBoxDescription_TextChanged);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescription.Location = new System.Drawing.Point(19, 8);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(42, 18);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Name:";
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.checkBoxOnlyWhenIdle);
            this.panelDetails.Controls.Add(this.comboBoxMinimumPriority);
            this.panelDetails.Controls.Add(this.labelMinimumPriority);
            this.panelDetails.Controls.Add(this.labelSMTPSettings);
            this.panelDetails.Controls.Add(this.textBoxUsername);
            this.panelDetails.Controls.Add(this.labelEmail);
            this.panelDetails.Controls.Add(this.textBoxDescription);
            this.panelDetails.Controls.Add(this.labelDescription);
            this.panelDetails.Location = new System.Drawing.Point(0, 0);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(338, 168);
            this.panelDetails.TabIndex = 7;
            // 
            // checkBoxOnlyWhenIdle
            // 
            this.checkBoxOnlyWhenIdle.AutoSize = true;
            this.checkBoxOnlyWhenIdle.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.checkBoxOnlyWhenIdle.Location = new System.Drawing.Point(22, 137);
            this.checkBoxOnlyWhenIdle.Name = "checkBoxOnlyWhenIdle";
            this.checkBoxOnlyWhenIdle.Size = new System.Drawing.Size(204, 22);
            this.checkBoxOnlyWhenIdle.TabIndex = 10;
            this.checkBoxOnlyWhenIdle.Text = "Only forward when idle or away";
            this.checkBoxOnlyWhenIdle.UseVisualStyleBackColor = true;
            // 
            // labelSMTPSettings
            // 
            this.labelSMTPSettings.AutoSize = true;
            this.labelSMTPSettings.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSMTPSettings.Location = new System.Drawing.Point(19, 60);
            this.labelSMTPSettings.Name = "labelSMTPSettings";
            this.labelSMTPSettings.Size = new System.Drawing.Size(90, 18);
            this.labelSMTPSettings.TabIndex = 6;
            this.labelSMTPSettings.Text = "SMTP Settings:";
            // 
            // EmailForwardInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelDetails);
            this.Name = "EmailForwardInputs";
            this.Controls.SetChildIndex(this.panelDetails, 0);
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxMinimumPriority;
        private System.Windows.Forms.Label labelMinimumPriority;
        private HighlightTextBox textBoxUsername;
        private System.Windows.Forms.Label labelEmail;
        private HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.Label labelSMTPSettings;
        private System.Windows.Forms.CheckBox checkBoxOnlyWhenIdle;
    }
}

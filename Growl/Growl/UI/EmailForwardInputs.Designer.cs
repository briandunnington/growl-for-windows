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
            this.linkLabelEditSMTPValues = new System.Windows.Forms.LinkLabel();
            this.labelSMTPValues = new System.Windows.Forms.Label();
            this.checkBoxOnlyWhenIdle = new System.Windows.Forms.CheckBox();
            this.labelSMTPSettings = new System.Windows.Forms.Label();
            this.panelSMTPSettings = new System.Windows.Forms.Panel();
            this.labelSMTPServer = new System.Windows.Forms.Label();
            this.textBoxSMTPServer = new System.Windows.Forms.TextBox();
            this.labelSMTPPort = new System.Windows.Forms.Label();
            this.textBoxSMTPPort = new System.Windows.Forms.TextBox();
            this.checkBoxSMTPUseAuthentication = new System.Windows.Forms.CheckBox();
            this.checkBoxSMTPUseSSL = new System.Windows.Forms.CheckBox();
            this.labelSMTPUsername = new System.Windows.Forms.Label();
            this.textBoxSMTPUsername = new System.Windows.Forms.TextBox();
            this.labelSMTPPassword = new System.Windows.Forms.Label();
            this.textBoxSMTPPassword = new System.Windows.Forms.TextBox();
            this.linkLabelSMTPDone = new System.Windows.Forms.LinkLabel();
            this.panelDetails.SuspendLayout();
            this.panelSMTPSettings.SuspendLayout();
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
            this.panelDetails.Controls.Add(this.panelSMTPSettings);
            this.panelDetails.Controls.Add(this.linkLabelEditSMTPValues);
            this.panelDetails.Controls.Add(this.labelSMTPValues);
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
            // linkLabelEditSMTPValues
            // 
            this.linkLabelEditSMTPValues.AutoSize = true;
            this.linkLabelEditSMTPValues.Location = new System.Drawing.Point(284, 62);
            this.linkLabelEditSMTPValues.Name = "linkLabelEditSMTPValues";
            this.linkLabelEditSMTPValues.Size = new System.Drawing.Size(24, 13);
            this.linkLabelEditSMTPValues.TabIndex = 12;
            this.linkLabelEditSMTPValues.TabStop = true;
            this.linkLabelEditSMTPValues.Text = "edit";
            this.linkLabelEditSMTPValues.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelEditSMTPValues_LinkClicked);
            // 
            // labelSMTPValues
            // 
            this.labelSMTPValues.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.labelSMTPValues.Location = new System.Drawing.Point(115, 60);
            this.labelSMTPValues.Name = "labelSMTPValues";
            this.labelSMTPValues.Size = new System.Drawing.Size(149, 23);
            this.labelSMTPValues.TabIndex = 11;
            this.labelSMTPValues.Text = "[smtp values]";
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
            // panelSMTPSettings
            // 
            this.panelSMTPSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSMTPSettings.Controls.Add(this.linkLabelSMTPDone);
            this.panelSMTPSettings.Controls.Add(this.textBoxSMTPPassword);
            this.panelSMTPSettings.Controls.Add(this.labelSMTPPassword);
            this.panelSMTPSettings.Controls.Add(this.textBoxSMTPUsername);
            this.panelSMTPSettings.Controls.Add(this.labelSMTPUsername);
            this.panelSMTPSettings.Controls.Add(this.checkBoxSMTPUseSSL);
            this.panelSMTPSettings.Controls.Add(this.checkBoxSMTPUseAuthentication);
            this.panelSMTPSettings.Controls.Add(this.textBoxSMTPPort);
            this.panelSMTPSettings.Controls.Add(this.labelSMTPPort);
            this.panelSMTPSettings.Controls.Add(this.textBoxSMTPServer);
            this.panelSMTPSettings.Controls.Add(this.labelSMTPServer);
            this.panelSMTPSettings.Location = new System.Drawing.Point(22, 62);
            this.panelSMTPSettings.Name = "panelSMTPSettings";
            this.panelSMTPSettings.Size = new System.Drawing.Size(286, 97);
            this.panelSMTPSettings.TabIndex = 13;
            // 
            // labelSMTPServer
            // 
            this.labelSMTPServer.AutoSize = true;
            this.labelSMTPServer.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSMTPServer.Location = new System.Drawing.Point(2, 7);
            this.labelSMTPServer.Name = "labelSMTPServer";
            this.labelSMTPServer.Size = new System.Drawing.Size(45, 16);
            this.labelSMTPServer.TabIndex = 0;
            this.labelSMTPServer.Text = "Server:";
            // 
            // textBoxSMTPServer
            // 
            this.textBoxSMTPServer.Location = new System.Drawing.Point(45, 5);
            this.textBoxSMTPServer.Name = "textBoxSMTPServer";
            this.textBoxSMTPServer.Size = new System.Drawing.Size(158, 20);
            this.textBoxSMTPServer.TabIndex = 1;
            // 
            // labelSMTPPort
            // 
            this.labelSMTPPort.AutoSize = true;
            this.labelSMTPPort.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSMTPPort.Location = new System.Drawing.Point(209, 7);
            this.labelSMTPPort.Name = "labelSMTPPort";
            this.labelSMTPPort.Size = new System.Drawing.Size(34, 16);
            this.labelSMTPPort.TabIndex = 2;
            this.labelSMTPPort.Text = "Port:";
            // 
            // textBoxSMTPPort
            // 
            this.textBoxSMTPPort.Location = new System.Drawing.Point(242, 5);
            this.textBoxSMTPPort.Name = "textBoxSMTPPort";
            this.textBoxSMTPPort.Size = new System.Drawing.Size(33, 20);
            this.textBoxSMTPPort.TabIndex = 3;
            this.textBoxSMTPPort.Text = "25";
            // 
            // checkBoxSMTPUseAuthentication
            // 
            this.checkBoxSMTPUseAuthentication.AutoSize = true;
            this.checkBoxSMTPUseAuthentication.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSMTPUseAuthentication.Location = new System.Drawing.Point(5, 27);
            this.checkBoxSMTPUseAuthentication.Name = "checkBoxSMTPUseAuthentication";
            this.checkBoxSMTPUseAuthentication.Size = new System.Drawing.Size(123, 20);
            this.checkBoxSMTPUseAuthentication.TabIndex = 4;
            this.checkBoxSMTPUseAuthentication.Text = "Use Authentication";
            this.checkBoxSMTPUseAuthentication.UseVisualStyleBackColor = true;
            this.checkBoxSMTPUseAuthentication.CheckedChanged += new System.EventHandler(this.checkBoxSMTPUseAuthentication_CheckedChanged);
            // 
            // checkBoxSMTPUseSSL
            // 
            this.checkBoxSMTPUseSSL.AutoSize = true;
            this.checkBoxSMTPUseSSL.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSMTPUseSSL.Location = new System.Drawing.Point(211, 27);
            this.checkBoxSMTPUseSSL.Name = "checkBoxSMTPUseSSL";
            this.checkBoxSMTPUseSSL.Size = new System.Drawing.Size(64, 20);
            this.checkBoxSMTPUseSSL.TabIndex = 5;
            this.checkBoxSMTPUseSSL.Text = "Use SSL";
            this.checkBoxSMTPUseSSL.UseVisualStyleBackColor = true;
            // 
            // labelSMTPUsername
            // 
            this.labelSMTPUsername.AutoSize = true;
            this.labelSMTPUsername.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSMTPUsername.Location = new System.Drawing.Point(22, 48);
            this.labelSMTPUsername.Name = "labelSMTPUsername";
            this.labelSMTPUsername.Size = new System.Drawing.Size(61, 16);
            this.labelSMTPUsername.TabIndex = 6;
            this.labelSMTPUsername.Text = "Username:";
            // 
            // textBoxSMTPUsername
            // 
            this.textBoxSMTPUsername.Location = new System.Drawing.Point(86, 46);
            this.textBoxSMTPUsername.Name = "textBoxSMTPUsername";
            this.textBoxSMTPUsername.Size = new System.Drawing.Size(138, 20);
            this.textBoxSMTPUsername.TabIndex = 7;
            // 
            // labelSMTPPassword
            // 
            this.labelSMTPPassword.AutoSize = true;
            this.labelSMTPPassword.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSMTPPassword.Location = new System.Drawing.Point(22, 69);
            this.labelSMTPPassword.Name = "labelSMTPPassword";
            this.labelSMTPPassword.Size = new System.Drawing.Size(59, 16);
            this.labelSMTPPassword.TabIndex = 8;
            this.labelSMTPPassword.Text = "Password:";
            // 
            // textBoxSMTPPassword
            // 
            this.textBoxSMTPPassword.Location = new System.Drawing.Point(86, 67);
            this.textBoxSMTPPassword.Name = "textBoxSMTPPassword";
            this.textBoxSMTPPassword.Size = new System.Drawing.Size(138, 20);
            this.textBoxSMTPPassword.TabIndex = 9;
            // 
            // linkLabelSMTPDone
            // 
            this.linkLabelSMTPDone.AutoSize = true;
            this.linkLabelSMTPDone.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelSMTPDone.Location = new System.Drawing.Point(243, 69);
            this.linkLabelSMTPDone.Name = "linkLabelSMTPDone";
            this.linkLabelSMTPDone.Size = new System.Drawing.Size(32, 16);
            this.linkLabelSMTPDone.TabIndex = 10;
            this.linkLabelSMTPDone.TabStop = true;
            this.linkLabelSMTPDone.Text = "done";
            this.linkLabelSMTPDone.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSMTPDone_LinkClicked);
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
            this.panelSMTPSettings.ResumeLayout(false);
            this.panelSMTPSettings.PerformLayout();
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
        private System.Windows.Forms.Label labelSMTPValues;
        private System.Windows.Forms.LinkLabel linkLabelEditSMTPValues;
        private System.Windows.Forms.Panel panelSMTPSettings;
        private System.Windows.Forms.TextBox textBoxSMTPPort;
        private System.Windows.Forms.Label labelSMTPPort;
        private System.Windows.Forms.TextBox textBoxSMTPServer;
        private System.Windows.Forms.Label labelSMTPServer;
        private System.Windows.Forms.Label labelSMTPPassword;
        private System.Windows.Forms.TextBox textBoxSMTPUsername;
        private System.Windows.Forms.Label labelSMTPUsername;
        private System.Windows.Forms.CheckBox checkBoxSMTPUseSSL;
        private System.Windows.Forms.CheckBox checkBoxSMTPUseAuthentication;
        private System.Windows.Forms.LinkLabel linkLabelSMTPDone;
        private System.Windows.Forms.TextBox textBoxSMTPPassword;
    }
}

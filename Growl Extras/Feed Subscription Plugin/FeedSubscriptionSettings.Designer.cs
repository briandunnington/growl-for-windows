namespace GrowlExtras.Subscriptions.FeedMonitor
{
    partial class FeedSubscriptionSettings
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
            this.textBoxUrl = new Growl.Destinations.HighlightTextBox();
            this.labelUrl = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new Growl.Destinations.HighlightTextBox();
            this.comboBoxPoll = new System.Windows.Forms.ComboBox();
            this.labelPoll = new System.Windows.Forms.Label();
            this.labelUsername = new System.Windows.Forms.Label();
            this.textBoxUsername = new Growl.Destinations.HighlightTextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new Growl.Destinations.HighlightTextBox();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.textBoxPassword);
            this.panelDetails.Controls.Add(this.labelPassword);
            this.panelDetails.Controls.Add(this.textBoxUsername);
            this.panelDetails.Controls.Add(this.labelUsername);
            this.panelDetails.Controls.Add(this.labelPoll);
            this.panelDetails.Controls.Add(this.comboBoxPoll);
            this.panelDetails.Controls.Add(this.textBoxName);
            this.panelDetails.Controls.Add(this.labelName);
            this.panelDetails.Controls.Add(this.labelUrl);
            this.panelDetails.Controls.Add(this.textBoxUrl);
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxUrl.Location = new System.Drawing.Point(109, 37);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(199, 20);
            this.textBoxUrl.TabIndex = 2;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.textBoxUrl_TextChanged);
            // 
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.Location = new System.Drawing.Point(13, 40);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(50, 13);
            this.labelUrl.TabIndex = 1;
            this.labelUrl.Text = "Feed Url:";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(13, 16);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(38, 13);
            this.labelName.TabIndex = 2;
            this.labelName.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxName.Location = new System.Drawing.Point(109, 13);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(199, 20);
            this.textBoxName.TabIndex = 1;
            this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
            // 
            // comboBoxPoll
            // 
            this.comboBoxPoll.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPoll.FormattingEnabled = true;
            this.comboBoxPoll.Location = new System.Drawing.Point(140, 61);
            this.comboBoxPoll.Name = "comboBoxPoll";
            this.comboBoxPoll.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPoll.TabIndex = 3;
            // 
            // labelPoll
            // 
            this.labelPoll.AutoSize = true;
            this.labelPoll.Location = new System.Drawing.Point(13, 64);
            this.labelPoll.Name = "labelPoll";
            this.labelPoll.Size = new System.Drawing.Size(121, 13);
            this.labelPoll.TabIndex = 5;
            this.labelPoll.Text = "Poll for new items every:";
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(13, 112);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(58, 13);
            this.labelUsername.TabIndex = 6;
            this.labelUsername.Text = "Username:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxUsername.Location = new System.Drawing.Point(109, 109);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(199, 20);
            this.textBoxUsername.TabIndex = 4;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(13, 136);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(56, 13);
            this.labelPassword.TabIndex = 8;
            this.labelPassword.Text = "Password:";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.HighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(184)))));
            this.textBoxPassword.Location = new System.Drawing.Point(109, 133);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(199, 20);
            this.textBoxPassword.TabIndex = 5;
            this.textBoxPassword.UseSystemPasswordChar = true;
            // 
            // FeedSubscriptionSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "FeedSubscriptionSettings";
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Growl.Destinations.HighlightTextBox textBoxUrl;
        private System.Windows.Forms.Label labelUrl;
        private Growl.Destinations.HighlightTextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelPoll;
        private System.Windows.Forms.ComboBox comboBoxPoll;
        private System.Windows.Forms.Label labelUsername;
        private Growl.Destinations.HighlightTextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private Growl.Destinations.HighlightTextBox textBoxUsername;

    }
}

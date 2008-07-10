namespace Sample_Net_Growl_App
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.notificationTextBox = new System.Windows.Forms.TextBox();
            this.sendNotificationButton = new System.Windows.Forms.Button();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // notificationTextBox
            // 
            this.notificationTextBox.Location = new System.Drawing.Point(13, 13);
            this.notificationTextBox.Name = "notificationTextBox";
            this.notificationTextBox.Size = new System.Drawing.Size(267, 20);
            this.notificationTextBox.TabIndex = 0;
            this.notificationTextBox.Text = "This is a sample message";
            // 
            // sendNotificationButton
            // 
            this.sendNotificationButton.Location = new System.Drawing.Point(13, 40);
            this.sendNotificationButton.Name = "sendNotificationButton";
            this.sendNotificationButton.Size = new System.Drawing.Size(97, 23);
            this.sendNotificationButton.TabIndex = 1;
            this.sendNotificationButton.Text = "send notification";
            this.sendNotificationButton.UseVisualStyleBackColor = true;
            this.sendNotificationButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(74, 154);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(206, 20);
            this.passwordTextBox.TabIndex = 2;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(13, 157);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(55, 13);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.Text = "password:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.sendNotificationButton);
            this.Controls.Add(this.notificationTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox notificationTextBox;
        private System.Windows.Forms.Button sendNotificationButton;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label passwordLabel;
    }
}


namespace Sample_Growl_App
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
            this.sendNotificationButton = new System.Windows.Forms.Button();
            this.notificationTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // sendNotificationButton
            // 
            this.sendNotificationButton.Location = new System.Drawing.Point(12, 38);
            this.sendNotificationButton.Name = "sendNotificationButton";
            this.sendNotificationButton.Size = new System.Drawing.Size(75, 23);
            this.sendNotificationButton.TabIndex = 0;
            this.sendNotificationButton.Text = "send";
            this.sendNotificationButton.UseVisualStyleBackColor = true;
            this.sendNotificationButton.Click += new System.EventHandler(this.sendNotificationButton_Click);
            // 
            // notificationTextBox
            // 
            this.notificationTextBox.Location = new System.Drawing.Point(12, 12);
            this.notificationTextBox.Name = "notificationTextBox";
            this.notificationTextBox.Size = new System.Drawing.Size(268, 20);
            this.notificationTextBox.TabIndex = 1;
            this.notificationTextBox.Text = "This is a sample message";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.notificationTextBox);
            this.Controls.Add(this.sendNotificationButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendNotificationButton;
        private System.Windows.Forms.TextBox notificationTextBox;
    }
}


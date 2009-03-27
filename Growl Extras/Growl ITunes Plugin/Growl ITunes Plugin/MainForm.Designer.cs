namespace GrowlExtras.ITunesPlugin
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.radioButtonSendNotifications = new System.Windows.Forms.RadioButton();
            this.radioButtonSendUDP = new System.Windows.Forms.RadioButton();
            this.radioButtonDontSend = new System.Windows.Forms.RadioButton();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(9, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // radioButtonSendNotifications
            // 
            this.radioButtonSendNotifications.AutoSize = true;
            this.radioButtonSendNotifications.Checked = true;
            this.radioButtonSendNotifications.Location = new System.Drawing.Point(80, 7);
            this.radioButtonSendNotifications.Name = "radioButtonSendNotifications";
            this.radioButtonSendNotifications.Size = new System.Drawing.Size(111, 17);
            this.radioButtonSendNotifications.TabIndex = 3;
            this.radioButtonSendNotifications.TabStop = true;
            this.radioButtonSendNotifications.Text = "Send Notifications";
            this.radioButtonSendNotifications.UseVisualStyleBackColor = true;
            this.radioButtonSendNotifications.CheckedChanged += new System.EventHandler(this.radioButtonSendNotifications_CheckedChanged);
            // 
            // radioButtonSendUDP
            // 
            this.radioButtonSendUDP.AutoSize = true;
            this.radioButtonSendUDP.Location = new System.Drawing.Point(80, 24);
            this.radioButtonSendUDP.Name = "radioButtonSendUDP";
            this.radioButtonSendUDP.Size = new System.Drawing.Size(188, 17);
            this.radioButtonSendUDP.TabIndex = 4;
            this.radioButtonSendUDP.Text = "Send Old-Style Notifications (UDP)";
            this.radioButtonSendUDP.UseVisualStyleBackColor = true;
            this.radioButtonSendUDP.CheckedChanged += new System.EventHandler(this.radioButtonSendUDP_CheckedChanged);
            // 
            // radioButtonDontSend
            // 
            this.radioButtonDontSend.AutoSize = true;
            this.radioButtonDontSend.Location = new System.Drawing.Point(80, 41);
            this.radioButtonDontSend.Name = "radioButtonDontSend";
            this.radioButtonDontSend.Size = new System.Drawing.Size(121, 17);
            this.radioButtonDontSend.TabIndex = 5;
            this.radioButtonDontSend.Text = "Disable Notifications";
            this.radioButtonDontSend.UseVisualStyleBackColor = true;
            this.radioButtonDontSend.CheckedChanged += new System.EventHandler(this.radioButtonDontSend_CheckedChanged);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(80, 69);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(56, 13);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "Password:";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(142, 66);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(126, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 94);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.radioButtonDontSend);
            this.Controls.Add(this.radioButtonSendUDP);
            this.Controls.Add(this.radioButtonSendNotifications);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "iTunes Growl Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton radioButtonSendNotifications;
        private System.Windows.Forms.RadioButton radioButtonSendUDP;
        private System.Windows.Forms.RadioButton radioButtonDontSend;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxPassword;

    }
}
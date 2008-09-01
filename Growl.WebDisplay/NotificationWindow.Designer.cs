namespace Growl.WebDisplay
{
    partial class NotificationWindow
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
            this.transparentPanel1 = new TransparentPanel();
            this.webKitBrowser = new WebKitBrowser();
            this.SuspendLayout();
            // 
            // transparentPanel1
            // 
            this.transparentPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.transparentPanel1.BackColor = System.Drawing.Color.Lime;
            this.transparentPanel1.Location = new System.Drawing.Point(0, 0);
            this.transparentPanel1.Name = "transparentPanel1";
            this.transparentPanel1.Size = new System.Drawing.Size(330, 300);
            this.transparentPanel1.TabIndex = 1;
            this.transparentPanel1.Click += new System.EventHandler(this.transparentPanel1_Click);
            // 
            // webKitBrowser
            // 
            this.webKitBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webKitBrowser.BackColor = System.Drawing.Color.White;
            this.webKitBrowser.Location = new System.Drawing.Point(0, 0);
            this.webKitBrowser.Name = "webKitBrowser";
            this.webKitBrowser.Size = new System.Drawing.Size(330, 300);
            this.webKitBrowser.TabIndex = 0;
            this.webKitBrowser.UserAgent = null;
            // 
            // NotificationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(330, 300);
            this.ControlBox = false;
            this.Controls.Add(this.transparentPanel1);
            this.Controls.Add(this.webKitBrowser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationWindow";
            this.Opacity = 0.9;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "webkitform";
            this.TransparencyKey = System.Drawing.Color.White;
            this.AutoScroll = false;
            this.ResumeLayout(false);


        }

        #endregion

        public WebKitBrowser webKitBrowser;
        private TransparentPanel transparentPanel1;
    }
}
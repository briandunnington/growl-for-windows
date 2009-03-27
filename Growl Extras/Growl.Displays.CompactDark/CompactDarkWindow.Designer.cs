namespace Growl.Displays.CompactDark
{
    partial class CompactDarkWindow
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
            this.titleLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.applicationNameLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.descriptionLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.pictureBoxApp = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApp)).BeginInit();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.titleLabel.Location = new System.Drawing.Point(5, 36);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(223, 15);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "[title]";
            this.titleLabel.UseMnemonic = false;
            this.titleLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.titleLabel_LabelHeightChanged);
            // 
            // applicationNameLabel
            // 
            this.applicationNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.applicationNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.applicationNameLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.applicationNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.applicationNameLabel.Location = new System.Drawing.Point(44, 4);
            this.applicationNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.applicationNameLabel.Name = "applicationNameLabel";
            this.applicationNameLabel.Size = new System.Drawing.Size(184, 30);
            this.applicationNameLabel.TabIndex = 2;
            this.applicationNameLabel.Text = "[appname]";
            this.applicationNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.applicationNameLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.applicationNameLabel_LabelHeightChanged);
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.descriptionLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.ForeColor = System.Drawing.Color.White;
            this.descriptionLabel.Location = new System.Drawing.Point(11, 51);
            this.descriptionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(217, 20);
            this.descriptionLabel.TabIndex = 3;
            this.descriptionLabel.Text = "[description]";
            this.descriptionLabel.UseMnemonic = false;
            this.descriptionLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.descriptionLabel_LabelHeightChanged);
            // 
            // pictureBoxApp
            // 
            this.pictureBoxApp.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxApp.Location = new System.Drawing.Point(8, 2);
            this.pictureBoxApp.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxApp.Name = "pictureBoxApp";
            this.pictureBoxApp.Padding = new System.Windows.Forms.Padding(2);
            this.pictureBoxApp.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxApp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxApp.TabIndex = 4;
            this.pictureBoxApp.TabStop = false;
            this.pictureBoxApp.Visible = false;
            // 
            // CompactDarkWindow
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(230, 76);
            this.Controls.Add(this.pictureBoxApp);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.applicationNameLabel);
            this.Controls.Add(this.titleLabel);
            this.Name = "CompactDarkWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxApp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Growl.DisplayStyle.ExpandingLabel titleLabel;
        private Growl.DisplayStyle.ExpandingLabel applicationNameLabel;
        private Growl.DisplayStyle.ExpandingLabel descriptionLabel;
        private System.Windows.Forms.PictureBox pictureBoxApp;
    }
}

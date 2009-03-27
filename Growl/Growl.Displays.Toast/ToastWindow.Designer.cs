namespace Growl.Displays.Toast
{
    partial class ToastWindow
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.descriptionLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.applicationNameLabel = new System.Windows.Forms.Label();
            this.titleLabel = new Growl.DisplayStyle.ExpandingLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(18, 32);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.descriptionLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.Location = new System.Drawing.Point(15, 35);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(242, 30);
            this.descriptionLabel.TabIndex = 5;
            this.descriptionLabel.Text = "[description]";
            this.descriptionLabel.UseMnemonic = false;
            this.descriptionLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.descriptionLabel_LabelHeightChanged);
            // 
            // applicationNameLabel
            // 
            this.applicationNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.applicationNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.applicationNameLabel.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.applicationNameLabel.Location = new System.Drawing.Point(21, 102);
            this.applicationNameLabel.Name = "applicationNameLabel";
            this.applicationNameLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.applicationNameLabel.Size = new System.Drawing.Size(239, 12);
            this.applicationNameLabel.TabIndex = 6;
            this.applicationNameLabel.Text = "[applicationName]";
            this.applicationNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.applicationNameLabel.UseMnemonic = false;
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(15, 14);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(242, 16);
            this.titleLabel.TabIndex = 4;
            this.titleLabel.Text = "[title]";
            this.titleLabel.UseMnemonic = false;
            this.titleLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.titleLabel_LabelHeightChanged);
            // 
            // ToastWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(272, 123);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.applicationNameLabel);
            this.Controls.Add(this.titleLabel);
            this.Name = "ToastWindow";
            this.Text = "ToastWindow";
            this.Click += new System.EventHandler(this.ToastWindow_Click);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private Growl.DisplayStyle.ExpandingLabel descriptionLabel;
        private System.Windows.Forms.Label applicationNameLabel;
        private Growl.DisplayStyle.ExpandingLabel titleLabel;
    }
}
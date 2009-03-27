namespace iRate
{
    partial class iRateWindow
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
            this.songNameLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.artistNameLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.albumLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.starRating = new iRate.Controls.StarRating();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // songNameLabel
            // 
            this.songNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.songNameLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.songNameLabel.ForeColor = System.Drawing.Color.White;
            this.songNameLabel.Location = new System.Drawing.Point(12, 9);
            this.songNameLabel.Name = "songNameLabel";
            this.songNameLabel.Size = new System.Drawing.Size(232, 16);
            this.songNameLabel.TabIndex = 1;
            this.songNameLabel.Text = "[song name]";
            this.songNameLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.songNameLabel_LabelHeightChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Location = new System.Drawing.Point(0, 109);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 256);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // artistNameLabel
            // 
            this.artistNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.artistNameLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.artistNameLabel.ForeColor = System.Drawing.Color.White;
            this.artistNameLabel.Location = new System.Drawing.Point(12, 34);
            this.artistNameLabel.Name = "artistNameLabel";
            this.artistNameLabel.Size = new System.Drawing.Size(232, 16);
            this.artistNameLabel.TabIndex = 3;
            this.artistNameLabel.Text = "[artist]";
            this.artistNameLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.artistNameLabel_LabelHeightChanged);
            // 
            // albumLabel
            // 
            this.albumLabel.BackColor = System.Drawing.Color.Transparent;
            this.albumLabel.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.albumLabel.ForeColor = System.Drawing.Color.White;
            this.albumLabel.Location = new System.Drawing.Point(12, 58);
            this.albumLabel.Name = "albumLabel";
            this.albumLabel.Size = new System.Drawing.Size(232, 16);
            this.albumLabel.TabIndex = 4;
            this.albumLabel.Text = "[album]";
            this.albumLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.albumLabel_LabelHeightChanged);
            // 
            // starRating
            // 
            this.starRating.BackColor = System.Drawing.Color.Transparent;
            this.starRating.ControlLayout = iRate.Controls.StarRating.Layouts.Horizontal;
            this.starRating.Location = new System.Drawing.Point(15, 83);
            this.starRating.Margin = new System.Windows.Forms.Padding(0);
            this.starRating.Name = "starRating";
            this.starRating.Padding = new System.Windows.Forms.Padding(1);
            this.starRating.Rating = 0;
            this.starRating.Size = new System.Drawing.Size(102, 22);
            this.starRating.TabIndex = 0;
            this.starRating.WrapperPanelBorderStyle = System.Windows.Forms.BorderStyle.None;
            // 
            // iRateWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(256, 365);
            this.Controls.Add(this.albumLabel);
            this.Controls.Add(this.artistNameLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.songNameLabel);
            this.Controls.Add(this.starRating);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "iRateWindow";
            this.Opacity = 0.85;
            this.Text = "iRateWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private iRate.Controls.StarRating starRating;
        private Growl.DisplayStyle.ExpandingLabel songNameLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Growl.DisplayStyle.ExpandingLabel artistNameLabel;
        private Growl.DisplayStyle.ExpandingLabel albumLabel;
    }
}
namespace Sample_Visual_Display
{
    partial class SampleVisualWindow
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
            this.titleLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.descriptionLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.applicationNameLabel = new Growl.DisplayStyle.ExpandingLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(10, 10);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(87, 10);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(260, 18);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "[title]";
            this.titleLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.titleLabel_LabelHeightChanged);
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.descriptionLabel.Location = new System.Drawing.Point(87, 28);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(260, 18);
            this.descriptionLabel.TabIndex = 2;
            this.descriptionLabel.Text = "[description]";
            this.descriptionLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.descriptionLabel_LabelHeightChanged);
            // 
            // applicationNameLabel
            // 
            this.applicationNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.applicationNameLabel.Location = new System.Drawing.Point(87, 57);
            this.applicationNameLabel.Name = "applicationNameLabel";
            this.applicationNameLabel.Size = new System.Drawing.Size(260, 18);
            this.applicationNameLabel.TabIndex = 3;
            this.applicationNameLabel.Text = "From: [application]";
            this.applicationNameLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.applicationNameLabel_LabelHeightChanged);
            // 
            // SampleVisualWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 84);
            this.Controls.Add(this.applicationNameLabel);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SampleVisualWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SampleVisualWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private Growl.DisplayStyle.ExpandingLabel titleLabel;
        private Growl.DisplayStyle.ExpandingLabel descriptionLabel;
        private Growl.DisplayStyle.ExpandingLabel applicationNameLabel;
    }
}
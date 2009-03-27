namespace iRate.Controls
{
    partial class StarRating
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StarRating));
            this.pbNoRating = new System.Windows.Forms.PictureBox();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.pbStar5 = new iRate.Controls.Star();
            this.pbStar1 = new iRate.Controls.Star();
            this.pbStar2 = new iRate.Controls.Star();
            this.pbStar3 = new iRate.Controls.Star();
            this.pbStar4 = new iRate.Controls.Star();
            ((System.ComponentModel.ISupportInitialize)(this.pbNoRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar4)).BeginInit();
            this.SuspendLayout();
            // 
            // pbNoRating
            // 
            this.pbNoRating.Image = global::iRate.Properties.Resources.delete;
            this.pbNoRating.Location = new System.Drawing.Point(1, 4);
            this.pbNoRating.Margin = new System.Windows.Forms.Padding(0);
            this.pbNoRating.Name = "pbNoRating";
            this.pbNoRating.Size = new System.Drawing.Size(16, 16);
            this.pbNoRating.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbNoRating.TabIndex = 11;
            this.pbNoRating.TabStop = false;
            this.pbNoRating.MouseLeave += new System.EventHandler(this.pbNoRating_MouseLeave);
            this.pbNoRating.Click += new System.EventHandler(this.pbNoRating_Click);
            this.pbNoRating.MouseEnter += new System.EventHandler(this.pbNoRating_MouseEnter);
            // 
            // pbStar5
            // 
            this.pbStar5.Image = ((System.Drawing.Image)(resources.GetObject("pbStar5.Image")));
            this.pbStar5.Location = new System.Drawing.Point(81, 3);
            this.pbStar5.Margin = new System.Windows.Forms.Padding(0);
            this.pbStar5.Name = "pbStar5";
            this.pbStar5.On = false;
            this.pbStar5.RatingValue = 100;
            this.pbStar5.Size = new System.Drawing.Size(16, 16);
            this.pbStar5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStar5.TabIndex = 10;
            this.pbStar5.TabStop = false;
            this.pbStar5.MouseLeave += new System.EventHandler(this.pbStar5_MouseLeave);
            this.pbStar5.Click += new System.EventHandler(this.pbStar5_Click);
            this.pbStar5.MouseEnter += new System.EventHandler(this.pbStar5_MouseEnter);
            // 
            // pbStar1
            // 
            this.pbStar1.Image = ((System.Drawing.Image)(resources.GetObject("pbStar1.Image")));
            this.pbStar1.Location = new System.Drawing.Point(17, 3);
            this.pbStar1.Margin = new System.Windows.Forms.Padding(0);
            this.pbStar1.Name = "pbStar1";
            this.pbStar1.On = false;
            this.pbStar1.RatingValue = 20;
            this.pbStar1.Size = new System.Drawing.Size(16, 16);
            this.pbStar1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStar1.TabIndex = 6;
            this.pbStar1.TabStop = false;
            this.pbStar1.MouseLeave += new System.EventHandler(this.pbStar1_MouseLeave);
            this.pbStar1.Click += new System.EventHandler(this.pbStar1_Click);
            this.pbStar1.MouseEnter += new System.EventHandler(this.pbStar1_MouseEnter);
            // 
            // pbStar2
            // 
            this.pbStar2.Image = ((System.Drawing.Image)(resources.GetObject("pbStar2.Image")));
            this.pbStar2.Location = new System.Drawing.Point(33, 3);
            this.pbStar2.Margin = new System.Windows.Forms.Padding(0);
            this.pbStar2.Name = "pbStar2";
            this.pbStar2.On = false;
            this.pbStar2.RatingValue = 40;
            this.pbStar2.Size = new System.Drawing.Size(16, 16);
            this.pbStar2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStar2.TabIndex = 7;
            this.pbStar2.TabStop = false;
            this.pbStar2.MouseLeave += new System.EventHandler(this.pbStar2_MouseLeave);
            this.pbStar2.Click += new System.EventHandler(this.pbStar2_Click);
            this.pbStar2.MouseEnter += new System.EventHandler(this.pbStar2_MouseEnter);
            // 
            // pbStar3
            // 
            this.pbStar3.Image = ((System.Drawing.Image)(resources.GetObject("pbStar3.Image")));
            this.pbStar3.Location = new System.Drawing.Point(49, 3);
            this.pbStar3.Margin = new System.Windows.Forms.Padding(0);
            this.pbStar3.Name = "pbStar3";
            this.pbStar3.On = false;
            this.pbStar3.RatingValue = 60;
            this.pbStar3.Size = new System.Drawing.Size(16, 16);
            this.pbStar3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStar3.TabIndex = 8;
            this.pbStar3.TabStop = false;
            this.pbStar3.MouseLeave += new System.EventHandler(this.pbStar3_MouseLeave);
            this.pbStar3.Click += new System.EventHandler(this.pbStar3_Click);
            this.pbStar3.MouseEnter += new System.EventHandler(this.pbStar3_MouseEnter);
            // 
            // pbStar4
            // 
            this.pbStar4.Image = ((System.Drawing.Image)(resources.GetObject("pbStar4.Image")));
            this.pbStar4.Location = new System.Drawing.Point(65, 3);
            this.pbStar4.Margin = new System.Windows.Forms.Padding(0);
            this.pbStar4.Name = "pbStar4";
            this.pbStar4.On = false;
            this.pbStar4.RatingValue = 80;
            this.pbStar4.Size = new System.Drawing.Size(16, 16);
            this.pbStar4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbStar4.TabIndex = 9;
            this.pbStar4.TabStop = false;
            this.pbStar4.MouseLeave += new System.EventHandler(this.pbStar4_MouseLeave);
            this.pbStar4.Click += new System.EventHandler(this.pbStar4_Click);
            this.pbStar4.MouseEnter += new System.EventHandler(this.pbStar4_MouseEnter);
            // 
            // StarRating
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pbNoRating);
            this.Controls.Add(this.pbStar5);
            this.Controls.Add(this.pbStar1);
            this.Controls.Add(this.pbStar2);
            this.Controls.Add(this.pbStar3);
            this.Controls.Add(this.pbStar4);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "StarRating";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(102, 22);
            this.Load += new System.EventHandler(this.StarRating_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbNoRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.PictureBox pbNoRating;
        internal Star pbStar5;
        internal Star pbStar1;
        internal Star pbStar2;
        internal System.Windows.Forms.ToolTip toolTips;
        internal Star pbStar3;
        internal Star pbStar4;
    }
}

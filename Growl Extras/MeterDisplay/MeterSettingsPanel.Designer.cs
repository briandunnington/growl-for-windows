namespace Meter
{
    partial class MeterSettingsPanel
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
            this.computerScreenPictureBox = new System.Windows.Forms.PictureBox();
            this.labelDirections = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.computerScreenPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // computerScreenPictureBox
            // 
            this.computerScreenPictureBox.Location = new System.Drawing.Point(27, 3);
            this.computerScreenPictureBox.Name = "computerScreenPictureBox";
            this.computerScreenPictureBox.Size = new System.Drawing.Size(144, 144);
            this.computerScreenPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.computerScreenPictureBox.TabIndex = 4;
            this.computerScreenPictureBox.TabStop = false;
            this.computerScreenPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.computerScreenPictureBox_MouseClick);
            // 
            // labelDirections
            // 
            this.labelDirections.BackColor = System.Drawing.Color.Transparent;
            this.labelDirections.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDirections.Location = new System.Drawing.Point(189, 26);
            this.labelDirections.Name = "labelDirections";
            this.labelDirections.Size = new System.Drawing.Size(128, 81);
            this.labelDirections.TabIndex = 5;
            this.labelDirections.Text = "Click the area of the screen where you would like these notifications to appear.";
            // 
            // SmokestackSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelDirections);
            this.Controls.Add(this.computerScreenPictureBox);
            this.Name = "IphoneSettingsPanel";
            this.Load += new System.EventHandler(this.SmokestackSettingsPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.computerScreenPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox computerScreenPictureBox;
        private System.Windows.Forms.Label labelDirections;
    }
}

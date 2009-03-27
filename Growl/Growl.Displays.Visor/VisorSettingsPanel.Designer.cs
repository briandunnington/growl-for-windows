namespace Growl.Displays.Visor
{
    partial class VisorSettingsPanel
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
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.backgroundColorLabel = new System.Windows.Forms.Label();
            this.currentBgColorPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.currentBgColorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // backgroundColorLabel
            // 
            this.backgroundColorLabel.AutoSize = true;
            this.backgroundColorLabel.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backgroundColorLabel.Location = new System.Drawing.Point(21, 25);
            this.backgroundColorLabel.Name = "backgroundColorLabel";
            this.backgroundColorLabel.Size = new System.Drawing.Size(100, 16);
            this.backgroundColorLabel.TabIndex = 0;
            this.backgroundColorLabel.Text = "Background Color:";
            // 
            // currentBgColorPictureBox
            // 
            this.currentBgColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.currentBgColorPictureBox.Location = new System.Drawing.Point(127, 21);
            this.currentBgColorPictureBox.Name = "currentBgColorPictureBox";
            this.currentBgColorPictureBox.Size = new System.Drawing.Size(41, 22);
            this.currentBgColorPictureBox.TabIndex = 2;
            this.currentBgColorPictureBox.TabStop = false;
            this.currentBgColorPictureBox.Click += new System.EventHandler(this.currentBgColorPictureBox_Click);
            // 
            // VisorSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.currentBgColorPictureBox);
            this.Controls.Add(this.backgroundColorLabel);
            this.Name = "VisorSettingsPanel";
            this.Load += new System.EventHandler(this.VisorSettingsPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.currentBgColorPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label backgroundColorLabel;
        private System.Windows.Forms.PictureBox currentBgColorPictureBox;
    }
}

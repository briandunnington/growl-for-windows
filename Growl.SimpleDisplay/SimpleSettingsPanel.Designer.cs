namespace Growl.SimpleDisplay
{
    partial class SimpleSettingsPanel
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
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.color1Label = new System.Windows.Forms.Label();
            this.color1PictureBox = new System.Windows.Forms.PictureBox();
            this.color2Label = new System.Windows.Forms.Label();
            this.color2PictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.color1PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.color2PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // color1Label
            // 
            this.color1Label.AutoSize = true;
            this.color1Label.Location = new System.Drawing.Point(72, 26);
            this.color1Label.Name = "color1Label";
            this.color1Label.Size = new System.Drawing.Size(34, 13);
            this.color1Label.TabIndex = 0;
            this.color1Label.Text = "Color:";
            // 
            // color1PictureBox
            // 
            this.color1PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.color1PictureBox.Location = new System.Drawing.Point(127, 21);
            this.color1PictureBox.Name = "color1PictureBox";
            this.color1PictureBox.Size = new System.Drawing.Size(41, 22);
            this.color1PictureBox.TabIndex = 2;
            this.color1PictureBox.TabStop = false;
            this.color1PictureBox.Click += new System.EventHandler(this.color1PictureBox_Click);
            // 
            // color2Label
            // 
            this.color2Label.AutoSize = true;
            this.color2Label.Location = new System.Drawing.Point(71, 57);
            this.color2Label.Name = "color2Label";
            this.color2Label.Size = new System.Drawing.Size(34, 13);
            this.color2Label.TabIndex = 3;
            this.color2Label.Text = "Color:";
            // 
            // color2PictureBox
            // 
            this.color2PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.color2PictureBox.Location = new System.Drawing.Point(127, 53);
            this.color2PictureBox.Name = "color2PictureBox";
            this.color2PictureBox.Size = new System.Drawing.Size(41, 22);
            this.color2PictureBox.TabIndex = 4;
            this.color2PictureBox.TabStop = false;
            this.color2PictureBox.Click += new System.EventHandler(this.color2PictureBox_Click);
            // 
            // SimpleSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.color2PictureBox);
            this.Controls.Add(this.color2Label);
            this.Controls.Add(this.color1PictureBox);
            this.Controls.Add(this.color1Label);
            this.Name = "SimpleSettingsPanel";
            this.Load += new System.EventHandler(this.SimpleSettingsPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.color1PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.color2PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label color1Label;
        private System.Windows.Forms.PictureBox color1PictureBox;
        private System.Windows.Forms.Label color2Label;
        private System.Windows.Forms.PictureBox color2PictureBox;
    }
}

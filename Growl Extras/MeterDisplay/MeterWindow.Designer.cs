namespace Meter
{
    partial class MeterWindow
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
            this.pictureBoxBars = new System.Windows.Forms.PictureBox();
            this.labelAltText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBars)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBoxBars
            // 
            this.pictureBoxBars.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxBars.Location = new System.Drawing.Point(98, 13);
            this.pictureBoxBars.Name = "pictureBoxBars";
            this.pictureBoxBars.Size = new System.Drawing.Size(190, 66);
            this.pictureBoxBars.TabIndex = 1;
            this.pictureBoxBars.TabStop = false;
            // 
            // labelAltText
            // 
            this.labelAltText.BackColor = System.Drawing.Color.Transparent;
            this.labelAltText.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAltText.ForeColor = System.Drawing.Color.White;
            this.labelAltText.Location = new System.Drawing.Point(98, 13);
            this.labelAltText.Name = "labelAltText";
            this.labelAltText.Size = new System.Drawing.Size(190, 64);
            this.labelAltText.TabIndex = 2;
            this.labelAltText.Text = "[this will show alternate text if the notification text cannot be converted to a " +
                "number]";
            // 
            // MeterWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Red;
            this.BackgroundImage = global::Meter.Properties.Resources.bg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(300, 90);
            this.Controls.Add(this.labelAltText);
            this.Controls.Add(this.pictureBoxBars);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.Name = "MeterWindow";
            this.Text = "MeterWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBars)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBoxBars;
        private System.Windows.Forms.Label labelAltText;
    }
}
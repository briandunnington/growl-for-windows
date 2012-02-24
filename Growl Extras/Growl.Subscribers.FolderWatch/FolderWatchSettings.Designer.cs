namespace Growl.Subscribers.FolderWatch
{
    partial class FolderWatchSettings
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
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonChoose = new System.Windows.Forms.Button();
            this.checkBoxSubdirectories = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelPath = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.labelInfo);
            this.panelDetails.Controls.Add(this.labelPath);
            this.panelDetails.Controls.Add(this.pictureBox1);
            this.panelDetails.Controls.Add(this.checkBoxSubdirectories);
            this.panelDetails.Controls.Add(this.buttonChoose);
            this.panelDetails.Controls.Add(this.textBoxPath);
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new System.Drawing.Point(58, 74);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(236, 20);
            this.textBoxPath.TabIndex = 0;
            this.textBoxPath.TextChanged += new System.EventHandler(this.textBoxPath_TextChanged);
            // 
            // buttonChoose
            // 
            this.buttonChoose.Location = new System.Drawing.Point(300, 72);
            this.buttonChoose.Name = "buttonChoose";
            this.buttonChoose.Size = new System.Drawing.Size(25, 23);
            this.buttonChoose.TabIndex = 1;
            this.buttonChoose.Text = "...";
            this.buttonChoose.UseVisualStyleBackColor = true;
            this.buttonChoose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // checkBoxSubdirectories
            // 
            this.checkBoxSubdirectories.AutoSize = true;
            this.checkBoxSubdirectories.Location = new System.Drawing.Point(58, 100);
            this.checkBoxSubdirectories.Name = "checkBoxSubdirectories";
            this.checkBoxSubdirectories.Size = new System.Drawing.Size(129, 17);
            this.checkBoxSubdirectories.TabIndex = 2;
            this.checkBoxSubdirectories.Text = "Include subdirectories";
            this.checkBoxSubdirectories.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Growl.Subscribers.FolderWatch.Properties.Resources.folderwatch;
            this.pictureBox1.Location = new System.Drawing.Point(16, 16);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(13, 77);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(39, 13);
            this.labelPath.TabIndex = 4;
            this.labelPath.Text = "Folder:";
            // 
            // labelInfo
            // 
            this.labelInfo.Location = new System.Drawing.Point(70, 16);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(224, 48);
            this.labelInfo.TabIndex = 5;
            this.labelInfo.Text = "Enter the folder path below to be notified when files are added, deleted, or chan" +
                "ged.";
            // 
            // FolderWatchSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "FolderWatchSettings";
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Button buttonChoose;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox checkBoxSubdirectories;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

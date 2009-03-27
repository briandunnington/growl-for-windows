namespace Growl.DisplayStyle
{
    partial class DefaultSettingsPanel
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
            this.noSettingsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // noSettingsLabel
            // 
            this.noSettingsLabel.Location = new System.Drawing.Point(19, 26);
            this.noSettingsLabel.Name = "noSettingsLabel";
            this.noSettingsLabel.Size = new System.Drawing.Size(290, 74);
            this.noSettingsLabel.TabIndex = 1;
            this.noSettingsLabel.Text = "There are no user-customizable settings for this display style.";
            // 
            // DefaultSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.noSettingsLabel);
            this.Name = "DefaultSettingsPanel";
            this.Size = new System.Drawing.Size(343, 214);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label noSettingsLabel;
    }
}

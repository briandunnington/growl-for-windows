namespace Growl.Installation
{
    partial class InstallDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallDisplay));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.OKButton = new Growl.UI.ButtonEx();
            this.NoButton = new Growl.UI.ButtonEx();
            this.YesButton = new Growl.UI.ButtonEx();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.Green;
            this.progressBar1.Location = new System.Drawing.Point(9, 98);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 26);
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // InfoLabel
            // 
            this.InfoLabel.Location = new System.Drawing.Point(9, 9);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(320, 115);
            this.InfoLabel.TabIndex = 5;
            this.InfoLabel.Text = "Do you want to install the following display:";
            this.InfoLabel.UseMnemonic = false;
            // 
            // OKButton
            // 
            this.OKButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OKButton.BackgroundImage")));
            this.OKButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.FlatAppearance.BorderSize = 0;
            this.OKButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.OKButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.OKButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.OKButton.Location = new System.Drawing.Point(130, 128);
            this.OKButton.Margin = new System.Windows.Forms.Padding(0);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(73, 32);
            this.OKButton.TabIndex = 7;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // NoButton
            // 
            this.NoButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("NoButton.BackgroundImage")));
            this.NoButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.NoButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.NoButton.FlatAppearance.BorderSize = 0;
            this.NoButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.NoButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.NoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NoButton.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.NoButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.NoButton.Location = new System.Drawing.Point(9, 128);
            this.NoButton.Margin = new System.Windows.Forms.Padding(0);
            this.NoButton.Name = "NoButton";
            this.NoButton.Size = new System.Drawing.Size(73, 32);
            this.NoButton.TabIndex = 8;
            this.NoButton.Text = "No";
            this.NoButton.UseVisualStyleBackColor = true;
            this.NoButton.Visible = false;
            this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
            // 
            // YesButton
            // 
            this.YesButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("YesButton.BackgroundImage")));
            this.YesButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.YesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.YesButton.FlatAppearance.BorderSize = 0;
            this.YesButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.YesButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.YesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.YesButton.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.YesButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.YesButton.Location = new System.Drawing.Point(256, 128);
            this.YesButton.Margin = new System.Windows.Forms.Padding(0);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(73, 32);
            this.YesButton.TabIndex = 6;
            this.YesButton.Text = "Yes";
            this.YesButton.UseVisualStyleBackColor = true;
            this.YesButton.Visible = false;
            this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
            // 
            // InstallDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(338, 165);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.NoButton);
            this.Controls.Add(this.YesButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.InfoLabel);
            this.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InstallDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Install Display";
            this.ResumeLayout(false);

        }

        #endregion

        private Growl.UI.ButtonEx OKButton;
        private Growl.UI.ButtonEx NoButton;
        private Growl.UI.ButtonEx YesButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label InfoLabel;
    }
}
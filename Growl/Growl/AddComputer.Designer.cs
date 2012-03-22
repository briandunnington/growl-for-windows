namespace Growl
{
    partial class AddComputer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddComputer));
            this.panelBonjour = new System.Windows.Forms.Panel();
            this.bonjourListBox1 = new Growl.UI.ForwardListBox();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.buttonSave = new Growl.UI.ButtonEx();
            this.buttonClose = new Growl.UI.ButtonEx();
            this.panelBonjour.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBonjour
            // 
            this.panelBonjour.Controls.Add(this.bonjourListBox1);
            this.panelBonjour.Location = new System.Drawing.Point(12, 12);
            this.panelBonjour.Name = "panelBonjour";
            this.panelBonjour.Size = new System.Drawing.Size(338, 168);
            this.panelBonjour.TabIndex = 5;
            // 
            // bonjourListBox1
            // 
            this.bonjourListBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.bonjourListBox1.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bonjourListBox1.FormattingEnabled = true;
            this.bonjourListBox1.IntegralHeight = false;
            this.bonjourListBox1.ItemHeight = 48;
            this.bonjourListBox1.Location = new System.Drawing.Point(0, 0);
            this.bonjourListBox1.Name = "bonjourListBox1";
            this.bonjourListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.bonjourListBox1.Size = new System.Drawing.Size(338, 168);
            this.bonjourListBox1.TabIndex = 4;
            // 
            // panelDetails
            // 
            this.panelDetails.Location = new System.Drawing.Point(12, 12);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(338, 168);
            this.panelDetails.TabIndex = 6;
            this.panelDetails.Visible = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonSave.BackgroundImage")));
            this.buttonSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonSave.Enabled = false;
            this.buttonSave.FlatAppearance.BorderSize = 0;
            this.buttonSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.buttonSave.Location = new System.Drawing.Point(277, 194);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(73, 32);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Visible = false;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonClose.BackgroundImage")));
            this.buttonClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonClose.FlatAppearance.BorderSize = 0;
            this.buttonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClose.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.buttonClose.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.buttonClose.Location = new System.Drawing.Point(12, 194);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(0);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(73, 32);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // AddComputer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 235);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.panelBonjour);
            this.Controls.Add(this.panelDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddComputer";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Forward Notifications";
            this.Load += new System.EventHandler(this.AddComputer_Load);
            this.panelBonjour.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Growl.UI.ButtonEx buttonClose;
        private Growl.UI.ForwardListBox bonjourListBox1;
        private System.Windows.Forms.Panel panelBonjour;
        private System.Windows.Forms.Panel panelDetails;
        private Growl.UI.ButtonEx buttonSave;
    }
}
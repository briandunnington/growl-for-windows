namespace Growl.UI
{
    partial class NotifyIOSubscriptionInputs
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
            this.textBoxOutletUrl = new Growl.Destinations.HighlightTextBox();
            this.labelOutletUrl = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.Destinations.HighlightTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxOutletUrl
            // 
            this.textBoxOutletUrl.Location = new System.Drawing.Point(109, 33);
            this.textBoxOutletUrl.Name = "textBoxOutletUrl";
            this.textBoxOutletUrl.Size = new System.Drawing.Size(199, 20);
            this.textBoxOutletUrl.TabIndex = 3;
            this.textBoxOutletUrl.TextChanged += new System.EventHandler(this.textBoxAPIKey_TextChanged);
            // 
            // labelOutletUrl
            // 
            this.labelOutletUrl.AutoSize = true;
            this.labelOutletUrl.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOutletUrl.Location = new System.Drawing.Point(19, 34);
            this.labelOutletUrl.Name = "labelOutletUrl";
            this.labelOutletUrl.Size = new System.Drawing.Size(68, 18);
            this.labelOutletUrl.TabIndex = 2;
            this.labelOutletUrl.Text = "Outlet Url:";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(109, 7);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(199, 20);
            this.textBoxDescription.TabIndex = 1;
            this.textBoxDescription.TextChanged += new System.EventHandler(this.textBoxDescription_TextChanged);
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescription.Location = new System.Drawing.Point(19, 8);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(42, 18);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Name:";
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.textBoxOutletUrl);
            this.panelDetails.Controls.Add(this.labelOutletUrl);
            this.panelDetails.Controls.Add(this.textBoxDescription);
            this.panelDetails.Controls.Add(this.labelDescription);

            // 
            // NotifyIOSubscriptionInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Name = "NotifyIOSubscriptionInputs";
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Growl.Destinations.HighlightTextBox textBoxOutletUrl;
        private System.Windows.Forms.Label labelOutletUrl;
        private Growl.Destinations.HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
    }
}

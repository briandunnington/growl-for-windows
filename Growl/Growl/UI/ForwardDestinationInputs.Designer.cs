namespace Growl.UI
{
    partial class ForwardDestinationInputs
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
            this.comboBoxFormat = new System.Windows.Forms.ComboBox();
            this.labelFormat = new System.Windows.Forms.Label();
            this.textBoxAddress = new Growl.UI.HighlightTextBox();
            this.labelAddress = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.UI.HighlightTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.textBoxPassword = new Growl.UI.HighlightTextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPort = new Growl.UI.HighlightTextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxFormat
            // 
            this.comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFormat.FormattingEnabled = true;
            this.comboBoxFormat.Location = new System.Drawing.Point(109, 111);
            this.comboBoxFormat.Name = "comboBoxFormat";
            this.comboBoxFormat.Size = new System.Drawing.Size(109, 21);
            this.comboBoxFormat.TabIndex = 9;
            // 
            // labelFormat
            // 
            this.labelFormat.AutoSize = true;
            this.labelFormat.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFormat.Location = new System.Drawing.Point(19, 112);
            this.labelFormat.Name = "labelFormat";
            this.labelFormat.Size = new System.Drawing.Size(50, 18);
            this.labelFormat.TabIndex = 8;
            this.labelFormat.Text = "Format:";
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.HighlightColor = System.Drawing.Color.Red;
            this.textBoxAddress.Location = new System.Drawing.Point(109, 33);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(199, 20);
            this.textBoxAddress.TabIndex = 3;
            this.textBoxAddress.TextChanged += new System.EventHandler(this.textBoxAddress_TextChanged);
            // 
            // labelAddress
            // 
            this.labelAddress.AutoSize = true;
            this.labelAddress.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddress.Location = new System.Drawing.Point(19, 34);
            this.labelAddress.Name = "labelAddress";
            this.labelAddress.Size = new System.Drawing.Size(66, 18);
            this.labelAddress.TabIndex = 2;
            this.labelAddress.Text = "Hostname:";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.HighlightColor = System.Drawing.Color.Red;
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
            this.panelDetails.Controls.Add(this.comboBoxFormat);
            this.panelDetails.Controls.Add(this.labelFormat);
            this.panelDetails.Controls.Add(this.textBoxPassword);
            this.panelDetails.Controls.Add(this.labelPassword);
            this.panelDetails.Controls.Add(this.textBoxPort);
            this.panelDetails.Controls.Add(this.labelPort);
            this.panelDetails.Controls.Add(this.textBoxAddress);
            this.panelDetails.Controls.Add(this.labelAddress);
            this.panelDetails.Controls.Add(this.textBoxDescription);
            this.panelDetails.Controls.Add(this.labelDescription);
            this.panelDetails.Location = new System.Drawing.Point(0, 0);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(338, 168);
            this.panelDetails.TabIndex = 7;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.HighlightColor = System.Drawing.Color.Red;
            this.textBoxPassword.Location = new System.Drawing.Point(109, 85);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(199, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPassword.Location = new System.Drawing.Point(19, 86);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(63, 18);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "Password:";
            // 
            // textBoxPort
            // 
            this.textBoxPort.HighlightColor = System.Drawing.Color.Red;
            this.textBoxPort.Location = new System.Drawing.Point(109, 59);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(199, 20);
            this.textBoxPort.TabIndex = 5;
            this.textBoxPort.TextChanged += new System.EventHandler(this.textBoxPort_TextChanged);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPort.Location = new System.Drawing.Point(19, 60);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(36, 18);
            this.labelPort.TabIndex = 4;
            this.labelPort.Text = "Port:";
            // 
            // ForwardDestinationInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelDetails);
            this.Name = "ForwardDestinationInputs";
            this.Controls.SetChildIndex(this.panelDetails, 0);
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxFormat;
        private System.Windows.Forms.Label labelFormat;
        private HighlightTextBox textBoxAddress;
        private System.Windows.Forms.Label labelAddress;
        private HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Panel panelDetails;
        private HighlightTextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private HighlightTextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
    }
}

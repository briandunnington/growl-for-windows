namespace Growl.UI
{
    partial class TwitterForwardInputs
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
            this.comboBoxMinimumPriority = new System.Windows.Forms.ComboBox();
            this.labelMinimumPriority = new System.Windows.Forms.Label();
            this.textBoxUsername = new Growl.UI.HighlightTextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.textBoxFormat = new Growl.UI.HighlightTextBox();
            this.labelFormat = new System.Windows.Forms.Label();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.checkBoxOnlyWhenIdle = new System.Windows.Forms.CheckBox();
            this.textBoxPassword = new Growl.UI.HighlightTextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxMinimumPriority
            // 
            this.comboBoxMinimumPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinimumPriority.FormattingEnabled = true;
            this.comboBoxMinimumPriority.Location = new System.Drawing.Point(109, 111);
            this.comboBoxMinimumPriority.Name = "comboBoxMinimumPriority";
            this.comboBoxMinimumPriority.Size = new System.Drawing.Size(109, 21);
            this.comboBoxMinimumPriority.TabIndex = 9;
            // 
            // labelMinimumPriority
            // 
            this.labelMinimumPriority.AutoSize = true;
            this.labelMinimumPriority.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMinimumPriority.Location = new System.Drawing.Point(19, 85);
            this.labelMinimumPriority.Name = "labelMinimumPriority";
            this.labelMinimumPriority.Size = new System.Drawing.Size(220, 18);
            this.labelMinimumPriority.TabIndex = 8;
            this.labelMinimumPriority.Text = "Only forward when priority is at least:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.HighlightColor = System.Drawing.Color.Red;
            this.textBoxUsername.Location = new System.Drawing.Point(109, 7);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(199, 20);
            this.textBoxUsername.TabIndex = 3;
            this.textBoxUsername.TextChanged += new System.EventHandler(this.textBoxUsername_TextChanged);
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUsername.Location = new System.Drawing.Point(19, 8);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(66, 18);
            this.labelUsername.TabIndex = 2;
            this.labelUsername.Text = "Username:";
            // 
            // textBoxFormat
            // 
            this.textBoxFormat.HighlightColor = System.Drawing.Color.Red;
            this.textBoxFormat.Location = new System.Drawing.Point(109, 59);
            this.textBoxFormat.Name = "textBoxFormat";
            this.textBoxFormat.Size = new System.Drawing.Size(199, 20);
            this.textBoxFormat.TabIndex = 1;
            this.textBoxFormat.TextChanged += new System.EventHandler(this.textBoxDescription_TextChanged);
            // 
            // labelFormat
            // 
            this.labelFormat.AutoSize = true;
            this.labelFormat.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFormat.Location = new System.Drawing.Point(19, 60);
            this.labelFormat.Name = "labelFormat";
            this.labelFormat.Size = new System.Drawing.Size(50, 18);
            this.labelFormat.TabIndex = 0;
            this.labelFormat.Text = "Format:";
            // 
            // panelDetails
            // 
            this.panelDetails.Controls.Add(this.checkBoxOnlyWhenIdle);
            this.panelDetails.Controls.Add(this.comboBoxMinimumPriority);
            this.panelDetails.Controls.Add(this.labelMinimumPriority);
            this.panelDetails.Controls.Add(this.textBoxPassword);
            this.panelDetails.Controls.Add(this.labelPassword);
            this.panelDetails.Controls.Add(this.textBoxUsername);
            this.panelDetails.Controls.Add(this.labelUsername);
            this.panelDetails.Controls.Add(this.textBoxFormat);
            this.panelDetails.Controls.Add(this.labelFormat);
            this.panelDetails.Location = new System.Drawing.Point(0, 0);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(338, 168);
            this.panelDetails.TabIndex = 7;
            // 
            // checkBoxOnlyWhenIdle
            // 
            this.checkBoxOnlyWhenIdle.AutoSize = true;
            this.checkBoxOnlyWhenIdle.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.checkBoxOnlyWhenIdle.Location = new System.Drawing.Point(22, 137);
            this.checkBoxOnlyWhenIdle.Name = "checkBoxOnlyWhenIdle";
            this.checkBoxOnlyWhenIdle.Size = new System.Drawing.Size(204, 22);
            this.checkBoxOnlyWhenIdle.TabIndex = 10;
            this.checkBoxOnlyWhenIdle.Text = "Only forward when idle or away";
            this.checkBoxOnlyWhenIdle.UseVisualStyleBackColor = true;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.HighlightColor = System.Drawing.Color.Red;
            this.textBoxPassword.Location = new System.Drawing.Point(109, 33);
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
            this.labelPassword.Location = new System.Drawing.Point(19, 34);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(63, 18);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "Password:";
            // 
            // TwitterForwardInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelDetails);
            this.Name = "TwitterForwardInputs";
            this.Controls.SetChildIndex(this.panelDetails, 0);
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxMinimumPriority;
        private System.Windows.Forms.Label labelMinimumPriority;
        private HighlightTextBox textBoxUsername;
        private System.Windows.Forms.Label labelUsername;
        private HighlightTextBox textBoxFormat;
        private System.Windows.Forms.Label labelFormat;
        private System.Windows.Forms.Panel panelDetails;
        private HighlightTextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.CheckBox checkBoxOnlyWhenIdle;
    }
}

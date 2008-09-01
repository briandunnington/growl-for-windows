namespace Growl.EmailDisplay
{
    partial class EmailSettingsPanel
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
            this.emailLabel = new System.Windows.Forms.Label();
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.priorityGroupBox = new System.Windows.Forms.GroupBox();
            this.priorityVeryLowCheckBox = new System.Windows.Forms.CheckBox();
            this.priorityModerateCheckBox = new System.Windows.Forms.CheckBox();
            this.priorityNormalCheckBox = new System.Windows.Forms.CheckBox();
            this.priorityHighCheckBox = new System.Windows.Forms.CheckBox();
            this.priorityEmergencyCheckBox = new System.Windows.Forms.CheckBox();
            this.priorityGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // emailLabel
            // 
            this.emailLabel.AutoSize = true;
            this.emailLabel.Location = new System.Drawing.Point(18, 20);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(76, 13);
            this.emailLabel.TabIndex = 0;
            this.emailLabel.Text = "Email Address:";
            // 
            // emailTextBox
            // 
            this.emailTextBox.Location = new System.Drawing.Point(98, 17);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(174, 20);
            this.emailTextBox.TabIndex = 0;
            this.emailTextBox.TextChanged += new System.EventHandler(this.emailTextBox_TextChanged);
            // 
            // priorityGroupBox
            // 
            this.priorityGroupBox.Controls.Add(this.priorityVeryLowCheckBox);
            this.priorityGroupBox.Controls.Add(this.priorityModerateCheckBox);
            this.priorityGroupBox.Controls.Add(this.priorityNormalCheckBox);
            this.priorityGroupBox.Controls.Add(this.priorityHighCheckBox);
            this.priorityGroupBox.Controls.Add(this.priorityEmergencyCheckBox);
            this.priorityGroupBox.Location = new System.Drawing.Point(21, 54);
            this.priorityGroupBox.Name = "priorityGroupBox";
            this.priorityGroupBox.Size = new System.Drawing.Size(251, 141);
            this.priorityGroupBox.TabIndex = 1;
            this.priorityGroupBox.TabStop = false;
            this.priorityGroupBox.Text = "Priority Levels";
            // 
            // priorityVeryLowCheckBox
            // 
            this.priorityVeryLowCheckBox.AutoSize = true;
            this.priorityVeryLowCheckBox.Location = new System.Drawing.Point(29, 114);
            this.priorityVeryLowCheckBox.Name = "priorityVeryLowCheckBox";
            this.priorityVeryLowCheckBox.Size = new System.Drawing.Size(70, 17);
            this.priorityVeryLowCheckBox.TabIndex = 4;
            this.priorityVeryLowCheckBox.Text = "Very Low";
            this.priorityVeryLowCheckBox.UseVisualStyleBackColor = true;
            this.priorityVeryLowCheckBox.CheckedChanged += new System.EventHandler(this.priorityVeryLowCheckBox_CheckedChanged);
            // 
            // priorityModerateCheckBox
            // 
            this.priorityModerateCheckBox.AutoSize = true;
            this.priorityModerateCheckBox.Location = new System.Drawing.Point(29, 91);
            this.priorityModerateCheckBox.Name = "priorityModerateCheckBox";
            this.priorityModerateCheckBox.Size = new System.Drawing.Size(71, 17);
            this.priorityModerateCheckBox.TabIndex = 3;
            this.priorityModerateCheckBox.Text = "Moderate";
            this.priorityModerateCheckBox.UseVisualStyleBackColor = true;
            this.priorityModerateCheckBox.CheckedChanged += new System.EventHandler(this.priorityModerateCheckBox_CheckedChanged);
            // 
            // priorityNormalCheckBox
            // 
            this.priorityNormalCheckBox.AutoSize = true;
            this.priorityNormalCheckBox.Location = new System.Drawing.Point(29, 67);
            this.priorityNormalCheckBox.Name = "priorityNormalCheckBox";
            this.priorityNormalCheckBox.Size = new System.Drawing.Size(59, 17);
            this.priorityNormalCheckBox.TabIndex = 2;
            this.priorityNormalCheckBox.Text = "Normal";
            this.priorityNormalCheckBox.UseVisualStyleBackColor = true;
            this.priorityNormalCheckBox.CheckedChanged += new System.EventHandler(this.priorityNormalCheckBox_CheckedChanged);
            // 
            // priorityHighCheckBox
            // 
            this.priorityHighCheckBox.AutoSize = true;
            this.priorityHighCheckBox.Location = new System.Drawing.Point(29, 43);
            this.priorityHighCheckBox.Name = "priorityHighCheckBox";
            this.priorityHighCheckBox.Size = new System.Drawing.Size(48, 17);
            this.priorityHighCheckBox.TabIndex = 1;
            this.priorityHighCheckBox.Text = "High";
            this.priorityHighCheckBox.UseVisualStyleBackColor = true;
            this.priorityHighCheckBox.CheckedChanged += new System.EventHandler(this.priorityHighCheckBox_CheckedChanged);
            // 
            // priorityEmergencyCheckBox
            // 
            this.priorityEmergencyCheckBox.AutoSize = true;
            this.priorityEmergencyCheckBox.Location = new System.Drawing.Point(29, 19);
            this.priorityEmergencyCheckBox.Name = "priorityEmergencyCheckBox";
            this.priorityEmergencyCheckBox.Size = new System.Drawing.Size(79, 17);
            this.priorityEmergencyCheckBox.TabIndex = 0;
            this.priorityEmergencyCheckBox.Text = "Emergency";
            this.priorityEmergencyCheckBox.UseVisualStyleBackColor = true;
            this.priorityEmergencyCheckBox.CheckedChanged += new System.EventHandler(this.priorityEmergencyCheckBox_CheckedChanged);
            // 
            // EmailSettingsPanel
            // 
            this.Controls.Add(this.priorityGroupBox);
            this.Controls.Add(this.emailLabel);
            this.Controls.Add(this.emailTextBox);
            this.Name = "EmailSettingsPanel";
            this.Size = new System.Drawing.Size(331, 216);
            this.Load += new System.EventHandler(this.EmailSettingsPanel_Load);
            this.priorityGroupBox.ResumeLayout(false);
            this.priorityGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label emailLabel;
        private System.Windows.Forms.TextBox emailTextBox;
        private System.Windows.Forms.GroupBox priorityGroupBox;
        private System.Windows.Forms.CheckBox priorityEmergencyCheckBox;
        private System.Windows.Forms.CheckBox priorityVeryLowCheckBox;
        private System.Windows.Forms.CheckBox priorityModerateCheckBox;
        private System.Windows.Forms.CheckBox priorityNormalCheckBox;
        private System.Windows.Forms.CheckBox priorityHighCheckBox;
    }
}

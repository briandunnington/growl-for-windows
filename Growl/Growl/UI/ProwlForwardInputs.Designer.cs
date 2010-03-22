namespace Growl.UI
{
    partial class ProwlForwardInputs
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
            this.textBoxAPIKey = new Growl.Destinations.HighlightTextBox();
            this.labelAPIKey = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.Destinations.HighlightTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxOnlyWhenIdle = new System.Windows.Forms.CheckBox();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxMinimumPriority
            // 
            this.comboBoxMinimumPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinimumPriority.FormattingEnabled = true;
            this.comboBoxMinimumPriority.Location = new System.Drawing.Point(109, 86);
            this.comboBoxMinimumPriority.Name = "comboBoxMinimumPriority";
            this.comboBoxMinimumPriority.Size = new System.Drawing.Size(109, 21);
            this.comboBoxMinimumPriority.TabIndex = 9;
            // 
            // labelMinimumPriority
            // 
            this.labelMinimumPriority.AutoSize = true;
            this.labelMinimumPriority.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMinimumPriority.Location = new System.Drawing.Point(19, 60);
            this.labelMinimumPriority.Name = "labelMinimumPriority";
            this.labelMinimumPriority.Size = new System.Drawing.Size(220, 18);
            this.labelMinimumPriority.TabIndex = 8;
            this.labelMinimumPriority.Text = "Only forward when priority is at least:";
            // 
            // textBoxAPIKey
            // 
            this.textBoxAPIKey.Location = new System.Drawing.Point(109, 33);
            this.textBoxAPIKey.Name = "textBoxAPIKey";
            this.textBoxAPIKey.Size = new System.Drawing.Size(199, 20);
            this.textBoxAPIKey.TabIndex = 3;
            this.textBoxAPIKey.TextChanged += new System.EventHandler(this.textBoxAPIKey_TextChanged);
            // 
            // labelAPIKey
            // 
            this.labelAPIKey.AutoSize = true;
            this.labelAPIKey.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAPIKey.Location = new System.Drawing.Point(19, 34);
            this.labelAPIKey.Name = "labelAPIKey";
            this.labelAPIKey.Size = new System.Drawing.Size(53, 18);
            this.labelAPIKey.TabIndex = 2;
            this.labelAPIKey.Text = "API Key:";
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
            this.panelDetails.Controls.Add(this.checkBoxOnlyWhenIdle);
            this.panelDetails.Controls.Add(this.comboBoxMinimumPriority);
            this.panelDetails.Controls.Add(this.labelMinimumPriority);
            this.panelDetails.Controls.Add(this.textBoxAPIKey);
            this.panelDetails.Controls.Add(this.labelAPIKey);
            this.panelDetails.Controls.Add(this.textBoxDescription);
            this.panelDetails.Controls.Add(this.labelDescription);
            // 
            // checkBoxOnlyWhenIdle
            // 
            this.checkBoxOnlyWhenIdle.AutoSize = true;
            this.checkBoxOnlyWhenIdle.Font = new System.Drawing.Font("Trebuchet MS", 9F);
            this.checkBoxOnlyWhenIdle.Location = new System.Drawing.Point(22, 112);
            this.checkBoxOnlyWhenIdle.Name = "checkBoxOnlyWhenIdle";
            this.checkBoxOnlyWhenIdle.Size = new System.Drawing.Size(204, 22);
            this.checkBoxOnlyWhenIdle.TabIndex = 10;
            this.checkBoxOnlyWhenIdle.Text = "Only forward when idle or away";
            this.checkBoxOnlyWhenIdle.UseVisualStyleBackColor = true;
            // 
            // ProwlForwardInputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Name = "ProwlForwardInputs";
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxMinimumPriority;
        private System.Windows.Forms.Label labelMinimumPriority;
        private Growl.Destinations.HighlightTextBox textBoxAPIKey;
        private System.Windows.Forms.Label labelAPIKey;
        private Growl.Destinations.HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkBoxOnlyWhenIdle;
    }
}

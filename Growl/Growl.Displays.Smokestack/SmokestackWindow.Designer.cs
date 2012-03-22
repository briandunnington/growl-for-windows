namespace Growl.Displays.Smokestack
{
    partial class SmokestackWindow
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
            this.panel1 = new Growl.Displays.Smokestack.PanelEx();
            this.descriptionLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.titleLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.applicationNameLabel = new Growl.DisplayStyle.ExpandingLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.descriptionLabel);
            this.panel1.Controls.Add(this.titleLabel);
            this.panel1.Controls.Add(this.applicationNameLabel);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(242, 100);
            this.panel1.TabIndex = 0;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.descriptionLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.ForeColor = System.Drawing.Color.White;
            this.descriptionLabel.Location = new System.Drawing.Point(10, 71);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(228, 19);
            this.descriptionLabel.TabIndex = 2;
            this.descriptionLabel.Text = "[description]";
            this.descriptionLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.descriptionLabel.UseMnemonic = false;
            this.descriptionLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.descriptionLabel_LabelHeightChanged);
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.ForeColor = System.Drawing.Color.White;
            this.titleLabel.Location = new System.Drawing.Point(10, 48);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(174, 19);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "[title]";
            this.titleLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.titleLabel.UseMnemonic = false;
            this.titleLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.titleLabel_LabelHeightChanged);
            // 
            // applicationNameLabel
            // 
            this.applicationNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.applicationNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.applicationNameLabel.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.applicationNameLabel.ForeColor = System.Drawing.Color.White;
            this.applicationNameLabel.Location = new System.Drawing.Point(5, 10);
            this.applicationNameLabel.Name = "applicationNameLabel";
            this.applicationNameLabel.Size = new System.Drawing.Size(174, 29);
            this.applicationNameLabel.TabIndex = 0;
            this.applicationNameLabel.Text = "[app]";
            this.applicationNameLabel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            this.applicationNameLabel.UseMnemonic = false;
            this.applicationNameLabel.LabelHeightChanged += new Growl.DisplayStyle.ExpandingLabel.LabelHeightChangedEventHandler(this.applicationNameLabel_LabelHeightChanged);
            // 
            // SmokestackWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Red;
            this.ClientSize = new System.Drawing.Size(250, 101);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "SmokestackWindow";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelEx panel1;
        private Growl.DisplayStyle.ExpandingLabel descriptionLabel;
        private Growl.DisplayStyle.ExpandingLabel titleLabel;
        private Growl.DisplayStyle.ExpandingLabel applicationNameLabel;

    }
}


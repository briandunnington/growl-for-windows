namespace Growl.UI
{
    partial class PasswordManagerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PasswordManagerControl));
            this.passwordListBox = new System.Windows.Forms.ListBox();
            this.contextMenuEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPanel = new System.Windows.Forms.Panel();
            this.labelDescriptionBox = new System.Windows.Forms.Label();
            this.labelPasswordBox = new System.Windows.Forms.Label();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.textBoxDescription = new Growl.Destinations.HighlightTextBox();
            this.textBoxPassword = new Growl.Destinations.HighlightTextBox();
            this.buttonCancel = new Growl.UI.ButtonEx();
            this.buttonSave = new Growl.UI.ButtonEx();
            this.buttonRemovePassword = new Growl.UI.ImageButton();
            this.buttonAddPassword = new Growl.UI.ImageButton();
            this.contextMenuEdit.SuspendLayout();
            this.editPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRemovePassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonAddPassword)).BeginInit();
            this.SuspendLayout();
            // 
            // passwordListBox
            // 
            this.passwordListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.passwordListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.passwordListBox.FormattingEnabled = true;
            this.passwordListBox.ItemHeight = 18;
            this.passwordListBox.Location = new System.Drawing.Point(0, 0);
            this.passwordListBox.Name = "passwordListBox";
            this.passwordListBox.Size = new System.Drawing.Size(195, 184);
            this.passwordListBox.TabIndex = 0;
            this.passwordListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.passwordListBox_DrawItem);
            this.passwordListBox.SelectedIndexChanged += new System.EventHandler(this.passwordListBox_SelectedIndexChanged);
            this.passwordListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.passwordListBox_MouseMove);
            this.passwordListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.passwordListBox_MouseDown);
            // 
            // contextMenuEdit
            // 
            this.contextMenuEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.contextMenuEdit.Name = "contextMenuEdit";
            this.contextMenuEdit.ShowImageMargin = false;
            this.contextMenuEdit.Size = new System.Drawing.Size(79, 26);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(78, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // editPanel
            // 
            this.editPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.editPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editPanel.Controls.Add(this.labelDescriptionBox);
            this.editPanel.Controls.Add(this.labelPasswordBox);
            this.editPanel.Controls.Add(this.labelInstructions);
            this.editPanel.Controls.Add(this.textBoxDescription);
            this.editPanel.Controls.Add(this.textBoxPassword);
            this.editPanel.Controls.Add(this.buttonCancel);
            this.editPanel.Controls.Add(this.buttonSave);
            this.editPanel.Location = new System.Drawing.Point(0, 0);
            this.editPanel.Name = "editPanel";
            this.editPanel.Size = new System.Drawing.Size(195, 219);
            this.editPanel.TabIndex = 5;
            this.editPanel.Visible = false;
            // 
            // labelDescriptionBox
            // 
            this.labelDescriptionBox.AutoSize = true;
            this.labelDescriptionBox.Location = new System.Drawing.Point(4, 114);
            this.labelDescriptionBox.Name = "labelDescriptionBox";
            this.labelDescriptionBox.Size = new System.Drawing.Size(79, 18);
            this.labelDescriptionBox.TabIndex = 10;
            this.labelDescriptionBox.Text = "Description:";
            // 
            // labelPasswordBox
            // 
            this.labelPasswordBox.AutoSize = true;
            this.labelPasswordBox.Location = new System.Drawing.Point(4, 67);
            this.labelPasswordBox.Name = "labelPasswordBox";
            this.labelPasswordBox.Size = new System.Drawing.Size(67, 18);
            this.labelPasswordBox.TabIndex = 9;
            this.labelPasswordBox.Text = "Password:";
            // 
            // labelInstructions
            // 
            this.labelInstructions.Location = new System.Drawing.Point(4, 2);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(186, 59);
            this.labelInstructions.TabIndex = 8;
            this.labelInstructions.Text = "Enter your password below as well as a short description to identify the password" +
                ".";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDescription.Location = new System.Drawing.Point(5, 134);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(184, 23);
            this.textBoxDescription.TabIndex = 7;
            this.textBoxDescription.TextChanged += new System.EventHandler(this.textBoxDescription_TextChanged);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(5, 88);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(184, 23);
            this.textBoxPassword.TabIndex = 6;
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonCancel.BackgroundImage")));
            this.buttonCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.buttonCancel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.buttonCancel.Location = new System.Drawing.Point(4, 182);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(73, 32);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonSave.BackgroundImage")));
            this.buttonSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSave.Enabled = false;
            this.buttonSave.FlatAppearance.BorderSize = 0;
            this.buttonSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSave.Font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);
            this.buttonSave.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.buttonSave.Location = new System.Drawing.Point(116, 182);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(73, 32);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonRemovePassword
            // 
            this.buttonRemovePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemovePassword.DisabledImage = Growl.FormResources.button_remove_dim;
            this.buttonRemovePassword.Enabled = false;
            this.buttonRemovePassword.Image = Growl.FormResources.button_remove;
            this.buttonRemovePassword.Location = new System.Drawing.Point(36, 187);
            this.buttonRemovePassword.Margin = new System.Windows.Forms.Padding(0);
            this.buttonRemovePassword.Name = "buttonRemovePassword";
            this.buttonRemovePassword.Size = new System.Drawing.Size(32, 32);
            this.buttonRemovePassword.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.buttonRemovePassword.TabIndex = 3;
            this.buttonRemovePassword.TabStop = false;
            this.buttonRemovePassword.Click += new System.EventHandler(this.buttonRemovePassword_Click);
            // 
            // buttonAddPassword
            // 
            this.buttonAddPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddPassword.DisabledImage = Growl.FormResources.button_add_dim;
            this.buttonAddPassword.Image = Growl.FormResources.button_add;
            this.buttonAddPassword.Location = new System.Drawing.Point(0, 187);
            this.buttonAddPassword.Margin = new System.Windows.Forms.Padding(0);
            this.buttonAddPassword.Name = "buttonAddPassword";
            this.buttonAddPassword.Size = new System.Drawing.Size(32, 32);
            this.buttonAddPassword.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.buttonAddPassword.TabIndex = 2;
            this.buttonAddPassword.TabStop = false;
            this.buttonAddPassword.Click += new System.EventHandler(this.buttonAddPassword_Click);
            // 
            // PasswordManagerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.editPanel);
            this.Controls.Add(this.buttonRemovePassword);
            this.Controls.Add(this.buttonAddPassword);
            this.Controls.Add(this.passwordListBox);
            this.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "PasswordManagerControl";
            this.Size = new System.Drawing.Size(195, 243);
            this.Load += new System.EventHandler(this.PasswordManagerControl_Load);
            this.contextMenuEdit.ResumeLayout(false);
            this.editPanel.ResumeLayout(false);
            this.editPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRemovePassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonAddPassword)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox passwordListBox;
        private ImageButton buttonAddPassword;
        private ImageButton buttonRemovePassword;
        private System.Windows.Forms.ContextMenuStrip contextMenuEdit;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.Panel editPanel;
        private ButtonEx buttonCancel;
        private ButtonEx buttonSave;
        private Growl.Destinations.HighlightTextBox textBoxPassword;
        private Growl.Destinations.HighlightTextBox textBoxDescription;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.Label labelDescriptionBox;
        private System.Windows.Forms.Label labelPasswordBox;
    }
}

namespace Growl.UI
{
    partial class MissedNotificationsWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelMessage = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxClose = new System.Windows.Forms.PictureBox();
            this.historyListView1 = new Growl.UI.HistoryListView();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClose)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            this.labelMessage.BackColor = System.Drawing.Color.Transparent;
            this.labelMessage.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.Location = new System.Drawing.Point(70, 14);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(170, 43);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "You missed {0} messages while you were away.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBoxClose
            // 
            this.pictureBoxClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxClose.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxClose.Location = new System.Drawing.Point(237, 1);
            this.pictureBoxClose.Name = "pictureBoxClose";
            this.pictureBoxClose.Size = new System.Drawing.Size(42, 17);
            this.pictureBoxClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxClose.TabIndex = 3;
            this.pictureBoxClose.TabStop = false;
            this.pictureBoxClose.MouseLeave += new System.EventHandler(this.pictureBoxClose_MouseLeave);
            this.pictureBoxClose.Click += new System.EventHandler(this.pictureBoxClose_Click);
            this.pictureBoxClose.MouseEnter += new System.EventHandler(this.pictureBoxClose_MouseEnter);
            // 
            // historyListView1
            // 
            this.historyListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.historyListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.historyListView1.GroupBy = Growl.UI.HistoryGroupItemsBy.Date;
            this.historyListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.historyListView1.LabelWrap = false;
            this.historyListView1.Location = new System.Drawing.Point(1, 64);
            this.historyListView1.MultiSelect = false;
            this.historyListView1.Name = "historyListView1";
            this.historyListView1.NumberOfDays = 7;
            this.historyListView1.OwnerDraw = true;
            this.historyListView1.PastNotifications = null;
            this.historyListView1.Size = new System.Drawing.Size(289, 221);
            this.historyListView1.TabIndex = 0;
            this.historyListView1.TileSize = new System.Drawing.Size(251, 56);
            this.historyListView1.UseCompatibleStateImageBehavior = false;
            this.historyListView1.UseCustomToolTips = true;
            this.historyListView1.View = System.Windows.Forms.View.Tile;
            // 
            // MissedNotificationsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(292, 302);
            this.Controls.Add(this.pictureBoxClose);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.historyListView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MissedNotificationsWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MissedNotificationWindow";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClose)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private HistoryListView historyListView1;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBoxClose;
    }
}
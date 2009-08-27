namespace GrowlExtras.FeedMonitor
{
    partial class MainForm
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
            this.pictureBoxClose = new System.Windows.Forms.PictureBox();
            this.labelAddFeed = new System.Windows.Forms.Label();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setIntervalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oneMinuteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.twoMinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fiveMinutesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tenMinutesToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.thirtyMinutesToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelAdd = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFeedUrl = new System.Windows.Forms.TextBox();
            this.listViewFeeds = new GrowlExtras.FeedMonitor.FeedListView(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClose)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxClose
            // 
            this.pictureBoxClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxClose.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxClose.Location = new System.Drawing.Point(237, 1);
            this.pictureBoxClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBoxClose.Name = "pictureBoxClose";
            this.pictureBoxClose.Size = new System.Drawing.Size(42, 17);
            this.pictureBoxClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxClose.TabIndex = 4;
            this.pictureBoxClose.TabStop = false;
            this.pictureBoxClose.MouseLeave += new System.EventHandler(this.pictureBoxClose_MouseLeave);
            this.pictureBoxClose.Click += new System.EventHandler(this.pictureBoxClose_Click);
            this.pictureBoxClose.MouseEnter += new System.EventHandler(this.pictureBoxClose_MouseEnter);
            // 
            // labelAddFeed
            // 
            this.labelAddFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAddFeed.AutoSize = true;
            this.labelAddFeed.BackColor = System.Drawing.Color.Transparent;
            this.labelAddFeed.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddFeed.ForeColor = System.Drawing.Color.DarkOrange;
            this.labelAddFeed.Location = new System.Drawing.Point(12, 72);
            this.labelAddFeed.Name = "labelAddFeed";
            this.labelAddFeed.Size = new System.Drawing.Size(70, 16);
            this.labelAddFeed.TabIndex = 7;
            this.labelAddFeed.Text = "Add feed...";
            this.labelAddFeed.MouseLeave += new System.EventHandler(this.labelAddFeed_MouseLeave);
            this.labelAddFeed.Click += new System.EventHandler(this.labelAddFeed_Click);
            this.labelAddFeed.MouseEnter += new System.EventHandler(this.labelAddFeed_MouseEnter);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkNowToolStripMenuItem,
            this.setIntervalToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Size = new System.Drawing.Size(128, 92);
            // 
            // checkNowToolStripMenuItem
            // 
            this.checkNowToolStripMenuItem.Name = "checkNowToolStripMenuItem";
            this.checkNowToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.checkNowToolStripMenuItem.Text = "Check Now";
            this.checkNowToolStripMenuItem.Click += new System.EventHandler(this.checkNowToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // setIntervalToolStripMenuItem
            // 
            this.setIntervalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oneMinuteToolStripMenuItem,
            this.twoMinutesToolStripMenuItem,
            this.fiveMinutesToolStripMenuItem1,
            this.tenMinutesToolStripMenuItem2,
            this.thirtyMinutesToolStripMenuItem3});
            this.setIntervalToolStripMenuItem.Name = "setIntervalToolStripMenuItem";
            this.setIntervalToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.setIntervalToolStripMenuItem.Text = "Set Interval";
            // 
            // oneMinuteToolStripMenuItem
            // 
            this.oneMinuteToolStripMenuItem.Name = "oneMinuteToolStripMenuItem";
            this.oneMinuteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.oneMinuteToolStripMenuItem.Tag = "1";
            this.oneMinuteToolStripMenuItem.Text = "1 minute";
            this.oneMinuteToolStripMenuItem.Click += new System.EventHandler(this.oneMinuteToolStripMenuItem_Click);
            // 
            // twoMinutesToolStripMenuItem
            // 
            this.twoMinutesToolStripMenuItem.Name = "twoMinutesToolStripMenuItem";
            this.twoMinutesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.twoMinutesToolStripMenuItem.Tag = "2";
            this.twoMinutesToolStripMenuItem.Text = "2 minutes";
            this.twoMinutesToolStripMenuItem.Click += new System.EventHandler(this.twoMinutesToolStripMenuItem_Click);
            // 
            // fiveMinutesToolStripMenuItem1
            // 
            this.fiveMinutesToolStripMenuItem1.Name = "fiveMinutesToolStripMenuItem1";
            this.fiveMinutesToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.fiveMinutesToolStripMenuItem1.Tag = "5";
            this.fiveMinutesToolStripMenuItem1.Text = "5 minutes";
            this.fiveMinutesToolStripMenuItem1.Click += new System.EventHandler(this.fiveMinutesToolStripMenuItem1_Click);
            // 
            // tenMinutesToolStripMenuItem2
            // 
            this.tenMinutesToolStripMenuItem2.Name = "tenMinutesToolStripMenuItem2";
            this.tenMinutesToolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.tenMinutesToolStripMenuItem2.Tag = "10";
            this.tenMinutesToolStripMenuItem2.Text = "10 minutes";
            this.tenMinutesToolStripMenuItem2.Click += new System.EventHandler(this.tenMinutesToolStripMenuItem2_Click);
            // 
            // thirtyMinutesToolStripMenuItem3
            // 
            this.thirtyMinutesToolStripMenuItem3.Name = "thirtyMinutesToolStripMenuItem3";
            this.thirtyMinutesToolStripMenuItem3.Size = new System.Drawing.Size(152, 22);
            this.thirtyMinutesToolStripMenuItem3.Tag = "30";
            this.thirtyMinutesToolStripMenuItem3.Text = "30 minutes";
            this.thirtyMinutesToolStripMenuItem3.Click += new System.EventHandler(this.thirtyMinutesToolStripMenuItem3_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Controls.Add(this.labelAdd);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBoxFeedUrl);
            this.panel1.Location = new System.Drawing.Point(1, 65);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(289, 24);
            this.panel1.TabIndex = 8;
            this.panel1.Visible = false;
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.BackColor = System.Drawing.Color.Transparent;
            this.labelAdd.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelAdd.ForeColor = System.Drawing.Color.DarkOrange;
            this.labelAdd.Location = new System.Drawing.Point(239, 4);
            this.labelAdd.Name = "labelAdd";
            this.labelAdd.Size = new System.Drawing.Size(28, 16);
            this.labelAdd.TabIndex = 2;
            this.labelAdd.Text = "add";
            this.labelAdd.MouseLeave += new System.EventHandler(this.labelAdd_MouseLeave);
            this.labelAdd.Click += new System.EventHandler(this.labelAdd_Click);
            this.labelAdd.MouseEnter += new System.EventHandler(this.labelAdd_MouseEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "url:";
            // 
            // textBoxFeedUrl
            // 
            this.textBoxFeedUrl.Location = new System.Drawing.Point(30, 2);
            this.textBoxFeedUrl.Name = "textBoxFeedUrl";
            this.textBoxFeedUrl.Size = new System.Drawing.Size(203, 20);
            this.textBoxFeedUrl.TabIndex = 0;
            // 
            // listViewFeeds
            // 
            this.listViewFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFeeds.BackColor = System.Drawing.Color.White;
            this.listViewFeeds.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewFeeds.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewFeeds.LabelWrap = false;
            this.listViewFeeds.Location = new System.Drawing.Point(1, 22);
            this.listViewFeeds.MultiSelect = false;
            this.listViewFeeds.Name = "listViewFeeds";
            this.listViewFeeds.OwnerDraw = true;
            this.listViewFeeds.Scrollable = false;
            this.listViewFeeds.ShowItemToolTips = true;
            this.listViewFeeds.Size = new System.Drawing.Size(289, 37);
            this.listViewFeeds.TabIndex = 5;
            this.listViewFeeds.TileSize = new System.Drawing.Size(290, 37);
            this.listViewFeeds.UseCompatibleStateImageBehavior = false;
            this.listViewFeeds.View = System.Windows.Forms.View.Tile;
            this.listViewFeeds.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewFeeds_MouseClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(292, 101);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelAddFeed);
            this.Controls.Add(this.listViewFeeds);
            this.Controls.Add(this.pictureBoxClose);
            this.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClose)).EndInit();
            this.contextMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxClose;
        private FeedListView listViewFeeds;
        private System.Windows.Forms.Label labelAddFeed;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem checkNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelAdd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFeedUrl;
        private System.Windows.Forms.ToolStripMenuItem setIntervalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oneMinuteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem twoMinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fiveMinutesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tenMinutesToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem thirtyMinutesToolStripMenuItem3;
    }
}


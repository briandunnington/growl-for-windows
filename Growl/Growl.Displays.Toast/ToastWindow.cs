using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Toast
{
    public partial class ToastWindow : NotificationWindow
    {
        private bool isClosing;

        private Brush borderBrush;

        public ToastWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(ToastWindow_Load);
            this.Shown += new EventHandler(ToastWindow_Shown);
            this.FormClosed += new FormClosedEventHandler(ToastWindow_FormClosed);
            this.AutoClosing += new FormClosingEventHandler(ToastWindow_AutoClosing);

            HookUpClickEvents(this, true, true);

            this.Animator = new PopupAnimator(this, 400, 150, PopupAnimator.PopupDirection.Up);

            this.SetAutoCloseInterval(4000);

            this.PauseWhenMouseOver = true;

            this.SetStyle(ControlStyles.DoubleBuffer, true); 
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); 
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        void ToastWindow_AutoClosing(object sender, FormClosingEventArgs e)
        {
            this.isClosing = true;
        }

        void ToastWindow_Load(object sender, EventArgs e)
        {
            // multiple monitor support
            MultiMonitorVisualDisplay d = (MultiMonitorVisualDisplay)this.Tag;
            Screen screen = d.GetPreferredDisplay();
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);
        }

        void ToastWindow_Shown(object sender, EventArgs e)
        {
            //this.Animator.Direction = Win32Animator.AnimationDirection.Down;
        }

        void ToastWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.pictureBox1.Image != null)
                this.pictureBox1.Image.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            // paint border
            e.Graphics.Clear(Color.Black);

            // white space
            rect.Inflate(-1, -1);
            e.Graphics.FillRectangle(Brushes.White, rect);

            // green frame
            rect.Inflate(-2, -2);
            e.Graphics.FillRectangle(this.borderBrush, rect);

            // white middle
            rect.Inflate(-6, -6);
            e.Graphics.FillRectangle(Brushes.White, rect);

            base.OnPaint(e);
        }

        private void ToastWindow_Click(object sender, EventArgs e)
        {
            PopupAnimator pa = this.Animator as PopupAnimator;
            pa.Close();
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.SetAutoCloseInterval(n.Duration * 1000);

            //Image image = n.GetImage();
            Image image = n.Image;
            if (image != null)
            {
                this.pictureBox1.Image = image;
                this.pictureBox1.Visible = true;
                int offset = this.pictureBox1.Width + 6;
                this.descriptionLabel.Left = this.descriptionLabel.Left + offset;
                this.descriptionLabel.Width = this.descriptionLabel.Width - offset;
            }

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;

            this.borderBrush = GetBorderBrushFromPriority(n.Priority);
        }

        private Brush GetBorderBrushFromPriority(int p)
        {
            Brush b = Brushes.Black;
            switch (p)
            {
                case 2:
                    b = Brushes.Crimson;
                    break;
                case 1:
                    b = Brushes.DarkOrange;
                    break;
                case -1:
                    b = Brushes.MidnightBlue;
                    break;
                case -2:
                    b = Brushes.MediumOrchid;
                    break;
                default:
                    b = Brushes.ForestGreen;
                    break;
            }
            return b;
        }

        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.pictureBox1.Top += args.HeightChange;
            this.descriptionLabel.Top += args.HeightChange;
            descriptionLabel_LabelHeightChanged(args);
        }

        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Size = new Size(this.Size.Width, this.Size.Height + args.HeightChange);
                this.Location = new Point(this.Location.X, this.Location.Y - args.HeightChange);
            }
        }
    }
}
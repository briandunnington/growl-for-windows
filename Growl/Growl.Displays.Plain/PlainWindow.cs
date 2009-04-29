using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Plain
{
    public partial class PlainWindow : NotificationWindow
    {
        private PlainDisplay.Location location = PlainDisplay.Location.TopRight;

        private int leftXLocation = 0;
        private int rightXLocation = 0;
        private int topYLocation = 0;
        private int bottomYLocation = 0;

        public PlainWindow()
        {
            InitializeComponent();

            this.BackColor = Color.White;
            this.ForeColor = Color.FromArgb(51, 51, 51);

            this.AfterLoad += new EventHandler(PlainWindow_AfterLoad);

            this.Animator = new FadeAnimator(this);

            HookUpClickEvents(this);

            this.AutoClose(4000);
        }

        void PlainWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        public PlainDisplay.Location DisplayLocation
        {
            get
            {
                return this.location;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //Rectangle rect = new Rectangle(this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
            //Rectangle rect = new Rectangle(this.Location.X, this.Location.Y, this.Width, this.Height);
            //Rectangle rect = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            e.Graphics.Clear(this.ForeColor);

            rect.Inflate(-1, -1);
            Brush b2 = new SolidBrush(this.BackColor);
            using (b2)
            {
                e.Graphics.FillRectangle(b2, rect);
            }
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            // handle the image. if the image is not set, move the other controls over to compensate
            Image image = n.Image;
            if (image != null)
            {
                this.pictureBox1.Image = image;
                this.pictureBox1.Visible = true;

                int offset = this.pictureBox1.Width + 6;
                this.applicationNameLabel.Left = this.titleLabel.Left + offset;
                this.applicationNameLabel.Width = this.titleLabel.Width - offset;
                this.titleLabel.Left = this.titleLabel.Left + offset;
                this.titleLabel.Width = this.titleLabel.Width - offset;
                this.descriptionLabel.Left = this.descriptionLabel.Left + offset;
                this.descriptionLabel.Width = this.descriptionLabel.Width - offset;
            }
            else
            {
                this.pictureBox1.Visible = false;
            }

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;
        }

        public void SetDisplayLocation(PlainDisplay.Location location)
        {
            this.location = location;
        }

        /// <summary>
        /// By handling the LabelHeightChanged events, you can make sure you notification window
        /// will expand properly to fit all of the text. In order to take advantage of this event,
        /// you must use ExpandingLabel class in place of normal Labels.
        /// </summary>
        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.pictureBox1.Top += args.HeightChange;
            this.descriptionLabel.Top += args.HeightChange;
            descriptionLabel_LabelHeightChanged(args);
        }

        /// <summary>
        /// By handling the LabelHeightChanged events, you can make sure you notification window
        /// will expand properly to fit all of the text. In order to take advantage of this event,
        /// you must use ExpandingLabel class in place of normal Labels.
        /// </summary>
        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.applicationNameLabel.Top += args.HeightChange;
            applicationNameLabel_LabelHeightChanged(args);
        }

        /// <summary>
        /// By handling the LabelHeightChanged events, you can make sure you notification window
        /// will expand properly to fit all of the text. In order to take advantage of this event,
        /// you must use ExpandingLabel class in place of normal Labels.
        /// </summary>
        private void applicationNameLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Size = new Size(this.Size.Width, this.Size.Height + args.HeightChange);
                this.Location = new Point(this.Location.X, this.Location.Y - args.HeightChange);
            }
        }

        private void DoBeforeShow()
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height;
            this.leftXLocation = 0;
            this.rightXLocation = x;
            this.topYLocation = 0;
            this.bottomYLocation = y;
            this.DesktopLocation = new Point(x, y);

            switch (location)
            {
                case PlainDisplay.Location.TopLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.topYLocation);
                    break;
                case PlainDisplay.Location.BottomLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.bottomYLocation - this.Height);
                    break;
                case PlainDisplay.Location.BottomRight:
                    this.DesktopLocation = new Point(this.rightXLocation, this.bottomYLocation - this.Height);
                    break;
                default: // TopRight
                    this.DesktopLocation = new Point(this.rightXLocation, this.topYLocation);
                    break;
            }
        }
    }
}
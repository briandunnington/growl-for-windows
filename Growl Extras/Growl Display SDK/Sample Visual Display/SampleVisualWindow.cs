using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Sample_Visual_Display
{
    /// <summary>
    /// All visual display windows must inherit from NotificationWindow
    /// </summary>
    public partial class SampleVisualWindow : NotificationWindow
    {
        public SampleVisualWindow()
        {
            InitializeComponent();

            // Any initial window sizing or positioning should be done in the AfterLoad event.
            // By doing it there, it is late enough to prevent most focus-stealing issues,
            // but early enough to allow proper operation of animators and layout managers.
            this.AfterLoad += new EventHandler(SampleVisualWindow_AfterLoad);

            // Set the Animator property if you want to do any animation of your window
            // when it is shown or closed. Built-in Animators include FadeAnimator,
            // PopupAnimator, and Win32Animator (which uses the AnimateWindow Win32 API)
            // If you dont want to perform any animation, you can leave this property null
            this.Animator = new FadeAnimator(this);

            // By calling the HookUpClickEvents method, the form will automatically hook
            // up the Click event of any child controls on the form to fire the
            // NotificationClicked event. It is almost always preferred to call this method
            // and let the base class handle the logic for you.
            HookUpClickEvents(this);

            // If you want your notification to automatically close after a set amount of time,
            // call the AutoClose property to set the duration. The autoclose behavior will
            // automatically trigger any associated Animator to peform any closing animation.
            // Setting the .Sticky property of the notification will override the AutoClose
            // behavior in the default implementation. It is almost always preferred to call
            // this method and let the base class handle the logic for you.
            this.AutoClose(4000);
        }

        void SampleVisualWindow_AfterLoad(object sender, EventArgs e)
        {
            // set initial location - see above for why it is important to do this here
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);
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
            }
            else
            {
                int offset = this.pictureBox1.Width - 6;
                this.applicationNameLabel.Left = this.titleLabel.Left + offset;
                this.applicationNameLabel.Width = this.titleLabel.Width - offset;
                this.titleLabel.Left = this.titleLabel.Left + offset;
                this.titleLabel.Width = this.titleLabel.Width - offset;
                this.descriptionLabel.Left = this.descriptionLabel.Left + offset;
                this.descriptionLabel.Width = this.descriptionLabel.Width - offset;
                this.pictureBox1.Visible = false;
            }

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;
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
    }
}
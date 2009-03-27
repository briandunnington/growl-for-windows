using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace iRate
{
    public partial class iRateWindow : NotificationWindow
    {
        public iRateWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(iRateWindow_Load);

            this.Animator = new FadeAnimator(this, 500, 1000, 0.9);

            HookUpClickEvents(this);

            this.starRating.RatingValueChanged += new iRate.Controls.StarRating.RatingValueChangedEventHandler(starRating_RatingValueChanged);

            this.AutoClose(5000);
        }

        void iRateWindow_Load(object sender, EventArgs e)
        {
            // set location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height - this.Height;
            this.DesktopLocation = new Point(x, y);
        }

        void starRating_RatingValueChanged(object sender, iRate.Controls.RatingChangedEventArgs e)
        {
            Growl.CoreLibrary.NotificationCallbackEventArgs args = new Growl.CoreLibrary.NotificationCallbackEventArgs(this.NotificationID, Growl.CoreLibrary.CallbackResult.CLICK);
            args.CustomInfo.Add("Rating", e.NewRating.ToString());
            this.OnNotificationClicked(args);
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            Image image = n.Image;
            if (image != null)
            {
                if (image.Width > this.pictureBox1.Width) this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                this.pictureBox1.Image = image;
                this.pictureBox1.Visible = true;
            }

            string artist = n.Description;
            if (n.CustomTextAttributes != null && n.CustomTextAttributes.ContainsKey("iTunes-Artist"))
                artist = n.CustomTextAttributes["iTunes-Artist"];

            string album = String.Empty;
            if (n.CustomTextAttributes != null && n.CustomTextAttributes.ContainsKey("iTunes-Album"))
                album = n.CustomTextAttributes["iTunes-Album"];

            int rating = 0;
            if (n.CustomTextAttributes != null && n.CustomTextAttributes.ContainsKey("iTunes-Rating"))
            {
                string r = n.CustomTextAttributes["iTunes-Rating"];
                if (!String.IsNullOrEmpty(r))
                {
                    rating = Convert.ToInt32(r);
                }
            }

            this.songNameLabel.Text = n.Title;
            this.artistNameLabel.Text = artist;
            this.albumLabel.Text = album;
            this.starRating.Rating = rating;
        }

        private void songNameLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.artistNameLabel.Top += args.HeightChange;
            this.artistNameLabel_LabelHeightChanged(args);
        }

        private void artistNameLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.albumLabel.Top += args.HeightChange;
            this.albumLabel_LabelHeightChanged(args);
        }

        private void albumLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.starRating.Top += args.HeightChange;
            this.Height += args.HeightChange;
        }
    }
}
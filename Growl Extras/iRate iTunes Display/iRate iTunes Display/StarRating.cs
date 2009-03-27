using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace iRate.Controls
{
    public partial class StarRating : UserControl
    {
        private Timer mouseOutTimer;
        private bool isOut = true;

        public enum Layouts
        {
            Horizontal,
            Vertical
        }

        public event RatingValueChangedEventHandler RatingValueChanged;
        public delegate void RatingValueChangedEventHandler(object sender, RatingChangedEventArgs e);

        private Layouts _controlLayout = Layouts.Horizontal;
        public Layouts ControlLayout
        {
            get { return _controlLayout; }
            set
            {
                _controlLayout = value;
                OrientControl();
            }
        }

        private int _rating = 0;
        public int Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
                ShowRating();
            }
        }

        private BorderStyle _wrapperPanelBorderStyle = BorderStyle.None;
        public BorderStyle WrapperPanelBorderStyle
        {
            get { return this.BorderStyle; }
            set
            {
                _wrapperPanelBorderStyle = value;
                this.BorderStyle = value;
            }
        }

        public StarRating()
        {
            InitializeComponent();
        }

        private void StarRating_Load(object sender, EventArgs e)
        {

            this.mouseOutTimer = new Timer();
            this.mouseOutTimer.Interval = 100;
            this.mouseOutTimer.Tick += new EventHandler(mouseOutTimer_Tick);

            OrientControl();
            ShowRating();
        }

        void mouseOutTimer_Tick(object sender, EventArgs e)
        {
            ShowRatingX();
        }

        private void OrientControl()
        {

            switch (ControlLayout)
            {

                case Layouts.Vertical:
                    this.Size = new Size(22, 102);
                    pbNoRating.Location = new Point(2, 82);
                    pbStar1.Location = new Point(2, 66);
                    pbStar2.Location = new Point(2, 50);
                    pbStar3.Location = new Point(2, 34);
                    pbStar4.Location = new Point(2, 18);
                    pbStar5.Location = new Point(2, 2);
                    break;

                case Layouts.Horizontal:
                    this.Size = new Size(102, 22);
                    pbNoRating.Location = new Point(1, 2);
                    pbStar1.Location = new Point(17, 1);
                    pbStar2.Location = new Point(32, 1);
                    pbStar3.Location = new Point(48, 1);
                    pbStar4.Location = new Point(64, 1);
                    pbStar5.Location = new Point(80, 1);
                    break;

            }

        }

        private void ShowRating()
        {
            this.isOut = true;
            if (this.mouseOutTimer != null) this.mouseOutTimer.Start();
        }

        private void ShowRatingX()
        {
            this.mouseOutTimer.Stop();

            if (!this.isOut) return;

            switch (this.Rating)
            {
                case 0:
                    pbStar1.On = false;
                    pbStar2.On = false;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;

                    toolTips.SetToolTip(pbNoRating, "No Rating");
                    toolTips.SetToolTip(pbStar1, "Rate 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rate 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rate 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rate 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rate 5 Stars");
                    break;

                case 20:
                    pbStar1.On = true;
                    pbStar2.On = false;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;

                    toolTips.SetToolTip(pbNoRating, "Remove Rating");
                    toolTips.SetToolTip(pbStar1, "Rated 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rate 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rate 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rate 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rate 5 Stars");
                    break;

                case 40:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;

                    toolTips.SetToolTip(pbNoRating, "Remove Rating");
                    toolTips.SetToolTip(pbStar1, "Rate 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rated 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rate 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rate 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rate 5 Stars");
                    break;

                case 60:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = false;
                    pbStar5.On = false;

                    toolTips.SetToolTip(pbNoRating, "Remove Rating");
                    toolTips.SetToolTip(pbStar1, "Rate 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rate 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rated 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rate 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rate 5 Stars");
                    break;

                case 80:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = true;
                    pbStar5.On = false;

                    toolTips.SetToolTip(pbNoRating, "Remove Rating");
                    toolTips.SetToolTip(pbStar1, "Rate 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rate 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rate 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rated 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rate 5 Stars");
                    break;

                case 100:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = true;
                    pbStar5.On = true;

                    toolTips.SetToolTip(pbNoRating, "Remove Rating");
                    toolTips.SetToolTip(pbStar1, "Rate 1 Star");
                    toolTips.SetToolTip(pbStar2, "Rate 2 Stars");
                    toolTips.SetToolTip(pbStar3, "Rate 3 Stars");
                    toolTips.SetToolTip(pbStar4, "Rate 4 Stars");
                    toolTips.SetToolTip(pbStar5, "Rated 5 Stars");
                    break;

            }

        }

        private void ShowRatingHover(int newRating)
        {

            this.isOut = false;
            this.mouseOutTimer.Stop();

            switch (newRating)
            {
                case 0:
                    pbStar1.On = false;
                    pbStar2.On = false;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;
                    break;

                case 20:
                    pbStar1.On = true;
                    pbStar2.On = false;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;
                    break;

                case 40:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = false;
                    pbStar4.On = false;
                    pbStar5.On = false;
                    break;

                case 60:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = false;
                    pbStar5.On = false;
                    break;

                case 80:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = true;
                    pbStar5.On = false;
                    break;

                case 100:
                    pbStar1.On = true;
                    pbStar2.On = true;
                    pbStar3.On = true;
                    pbStar4.On = true;
                    pbStar5.On = true;
                    break;

            }

        }

        #region Click Events

        private void FireRatingValueChanged(object sender)
        {
            int oldValue = this.Rating;
            int newValue = 0;
            if (sender is Star)
            {
                newValue = ((Star)sender).RatingValue;
            }
            if (RatingValueChanged != null)
            {
                RatingValueChanged(this, new RatingChangedEventArgs(oldValue, newValue));
            }
        }

        private void pbNoRating_Click(object sender, System.EventArgs e)
        {
            int pre = Rating;
            Rating = 0;
            if (RatingValueChanged != null)
            {
                RatingValueChanged(this, new RatingChangedEventArgs(pre, Rating));
            }
        }

        private void pbStar1_Click(object sender, System.EventArgs e)
        {
            FireRatingValueChanged(sender);
        }

        private void pbStar2_Click(object sender, System.EventArgs e)
        {
            FireRatingValueChanged(sender);
        }

        private void pbStar3_Click(object sender, System.EventArgs e)
        {
            FireRatingValueChanged(sender);
        }

        private void pbStar4_Click(object sender, System.EventArgs e)
        {
            FireRatingValueChanged(sender);
        }

        private void pbStar5_Click(object sender, System.EventArgs e)
        {
            FireRatingValueChanged(sender);
        }

        #endregion


        #region Mouse Leave Events

        private void pbNoRating_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        private void pbStar1_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        private void pbStar2_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        private void pbStar3_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        private void pbStar4_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        private void pbStar5_MouseLeave(object sender, System.EventArgs e)
        {
            ShowRating();
        }

        #endregion

        private void Hover(object sender)
        {
            if (sender is Star)
            {
                ShowRatingHover(((Star)sender).RatingValue);
            }
        }

        private void pbStar5_MouseEnter(object sender, EventArgs e)
        {
            Hover(sender);
        }

        private void pbStar4_MouseEnter(object sender, EventArgs e)
        {
            Hover(sender);
        }

        private void pbStar3_MouseEnter(object sender, EventArgs e)
        {
            Hover(sender);
        }

        private void pbStar2_MouseEnter(object sender, EventArgs e)
        {
            Hover(sender);
        }

        private void pbStar1_MouseEnter(object sender, EventArgs e)
        {
            Hover(sender);
        }

        private void pbNoRating_MouseEnter(object sender, EventArgs e)
        {
            ShowRatingHover(0);
        }
    }

}

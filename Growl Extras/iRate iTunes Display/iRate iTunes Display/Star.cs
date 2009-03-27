using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace iRate.Controls
{
    public class Star : PictureBox
    {
        private bool on = false;
        private int ratingValue = 0;

        public Star()
        {
            this.Image = Properties.Resources.rating_star_disabled;
        }

        public bool On
        {
            get
            {
                return on;
            }
            set
            {
                if (this.on != value)
                {
                    this.on = value;
                    this.Image = (value ? Properties.Resources.rating_star_enabled : Properties.Resources.rating_star_disabled);
                }
            }
        }

        [DefaultValue(0)]
        [Description("The rating value of this star")]
        public int RatingValue
        {
            get
            {
                return this.ratingValue;
            }
            set
            {
                this.ratingValue = value;
            }
        }
    }
}

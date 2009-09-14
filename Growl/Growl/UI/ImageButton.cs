using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace Growl.UI
{
    public class ImageButton : PictureBox
    {
        private Image image;
        private Image disabledImage;

        public ImageButton()
        {
        }

        public new Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
                //this.disabledImage = ConvertImagetoGrayScale(value);
                this.Enabled = base.Enabled;    // do this so the correct image is drawn
            }
        }

        [System.ComponentModel.Description("The image to use in if the control is disabled")]
        public Image DisabledImage
        {
            get
            {
                return this.disabledImage;
            }
            set
            {
                this.disabledImage = value;
                this.Enabled = base.Enabled;    // do this so the correct image is drawn
            }
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                if (this.disabledImage != null)
                {
                    base.Image = (value ? this.image : this.disabledImage);
                    this.Invalidate(true);
                }
            }
        }

        /* I AM JUST SAVING THIS FOR NOW
        private Image ConvertImagetoGrayScale(Image original)
        {
            if (original != null)
            {
                //create a blank bitmap the same size as original
                Bitmap newBitmap = new Bitmap(original.Width, original.Height);

                //get a graphics object from the new image
                Graphics g = Graphics.FromImage(newBitmap);

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

                //create some image attributes
                ImageAttributes attributes = new ImageAttributes();

                //set the color matrix attribute
                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                g.DrawImage(original,
                   new Rectangle(0, 0, original.Width, original.Height),
                   0, 0, original.Width, original.Height,
                   GraphicsUnit.Pixel, attributes);

                //dispose the Graphics object
                g.Dispose();
                return newBitmap;
            }
            return null;
        }
         * */

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Location = new Point(this.Location.X + 1, this.Location.Y + 1);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Location = new Point(this.Location.X - 1, this.Location.Y - 1);
            base.OnMouseUp(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.image != null)
                {
                    this.image.Dispose();
                    this.image = null;
                }

                if (this.disabledImage != null)
                {
                    this.disabledImage.Dispose();
                    this.disabledImage = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}

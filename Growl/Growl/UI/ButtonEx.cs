using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ButtonEx : Button
    {
        Image bgImage = Growl.FormResources.button_bg;
        Image bgImageDisabled = Growl.FormResources.button_bg_dim;
        Color foreColor = Color.WhiteSmoke;
        Color foreColorDisabled = Color.LightGray;
        Font font = new System.Drawing.Font("Trebuchet MS", 10.25F, System.Drawing.FontStyle.Bold);

        public ButtonEx()
        {
            this.BackgroundImage = this.bgImage;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Font = this.font;
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.Location = new System.Drawing.Point(364, 243);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(73, 32);
            this.TabIndex = 3;
            this.Text = "button1";
            this.UseVisualStyleBackColor = true;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.Enabled)
            {
                this.BackgroundImage = bgImage;
                this.ForeColor = foreColor;
            }
            else
            {
                this.BackgroundImage = bgImageDisabled;
                this.ForeColor = foreColorDisabled;
            }
            base.OnEnabledChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.bgImage != null)
                {
                    this.bgImage.Dispose();
                    this.bgImage = null;
                }

                if (this.bgImageDisabled != null)
                {
                    this.bgImageDisabled.Dispose();
                    this.bgImageDisabled = null;
                }

                if (this.font != null)
                {
                    this.font.Dispose();
                    this.font = null;
                }

                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }

                if (this.BackgroundImage != null)
                {
                    this.BackgroundImage.Dispose();
                    this.BackgroundImage = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}

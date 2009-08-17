using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    public static class Utility
    {
        public static System.Drawing.Region CreateRoundedRegion(int top, int left, int width, int height, int xradius, int yradius)
        {
            System.Drawing.Region r = null;
            IntPtr hRgn = IntPtr.Zero;
            try
            {
                hRgn = Win32.CreateRoundRectRgn(top, left, width, height, xradius, yradius);
                r = System.Drawing.Region.FromHrgn(hRgn);
            }
            finally
            {
                Win32.DeleteObject(hRgn);
            }
            return r;
        }

        public static void UpdateLayeredWindow(Bitmap bitmap, Form form, int x, int y)
        {
            UpdateLayeredWindow(bitmap, form, x, y, 255);
        }

        public static void UpdateLayeredWindow(Bitmap b, Form form, int x, int y, byte opacity)
        {
            if (b.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

            Bitmap bitmap = new Bitmap(b);
            using (bitmap)
            {
                // The idea of this is very simple,
                // 1. Create a compatible DC with screen;
                // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
                // 3. Call the UpdateLayeredWindow.

                IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
                IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr oldBitmap = IntPtr.Zero;

                try
                {
                    Graphics g = Graphics.FromImage(bitmap);
                    using (g)
                    {
                        foreach (Control ctrl in form.Controls)
                        {
                            if (ctrl is PictureBox)
                            {
                                PictureBox pb = (PictureBox)ctrl;
                                if(pb.Image != null) g.DrawImage(pb.Image, ctrl.Bounds);
                            }
                            else if (ctrl is Label)
                            {
                                // text looks crappy if we use the DrawControlToBitmap method, so we have to do it manually
                                Label el = (Label)ctrl;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                //g.CompositingQuality = CompositingQuality.HighQuality;
                                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                if (el is ExpandingLabel)
                                    g.TextRenderingHint = ((ExpandingLabel)el).TextRenderingHint;
                                SolidBrush brush = new SolidBrush(el.ForeColor);
                                using (brush)
                                {
                                    g.DrawString(el.Text, el.Font, brush, new RectangleF(el.Bounds.X, el.Bounds.Y, el.Bounds.Width, el.Bounds.Height), System.Drawing.StringFormat.GenericTypographic);
                                }
                            }
                            else
                            {
                                DrawControlToBitmap(ctrl, bitmap, ctrl.Bounds); //ctrl.DrawToBitmap leaks memory, so we use our own version
                            }
                        }
                    }

                    hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                    oldBitmap = Win32.SelectObject(memDc, hBitmap);

                    Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                    Win32.Point pointSource = new Win32.Point(0, 0);
                    Win32.Point topPos = new Win32.Point(x, y);
                    Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                    blend.BlendOp = Win32.AC_SRC_OVER;
                    blend.BlendFlags = 0;
                    blend.SourceConstantAlpha = opacity;
                    blend.AlphaFormat = Win32.AC_SRC_ALPHA;

                    Win32.UpdateLayeredWindow(form.Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
                }
                finally
                {
                    Win32.ReleaseDC(IntPtr.Zero, screenDc);
                    if (hBitmap != IntPtr.Zero)
                    {
                        Win32.SelectObject(memDc, oldBitmap);
                        Win32.DeleteObject(hBitmap);
                    }
                    Win32.DeleteDC(memDc);
                }
            }
        }

        private static void DrawControlToBitmap(Control control, Bitmap bitmap, Rectangle targetBounds)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }
            if (((targetBounds.Width <= 0) || (targetBounds.Height <= 0)) || ((targetBounds.X < 0) || (targetBounds.Y < 0)))
            {
                throw new ArgumentException("targetBounds");
            }
            // ensure the handle is created
            IntPtr ctrlHandle = control.Handle;

            IntPtr hdc = IntPtr.Zero;
            IntPtr handle = IntPtr.Zero;
            int width = Math.Min(control.Width, targetBounds.Width);
            int height = Math.Min(control.Height, targetBounds.Height);
            Bitmap image = new Bitmap(width, height, bitmap.PixelFormat);
            using (image)
            {
                Graphics graphics = Graphics.FromImage(image);
                using (graphics)
                {
                    hdc = graphics.GetHdc();
                    Win32.SendMessage(new HandleRef(control, control.Handle), 0x317, hdc, (IntPtr)30);
                    Graphics graphics2 = Graphics.FromImage(bitmap);
                    using (graphics2)
                    {
                        handle = graphics2.GetHdc();
                        Win32.BitBlt(new HandleRef(graphics2, handle), targetBounds.X, targetBounds.Y, width, height, new HandleRef(graphics, hdc), 0, 0, 0xcc0020);
                        graphics2.ReleaseHdcInternal(handle);
                    }
                    graphics.ReleaseHdcInternal(hdc);
                }
            }
        }
    }
}

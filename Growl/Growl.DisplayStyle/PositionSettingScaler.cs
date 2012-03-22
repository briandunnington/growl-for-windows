using System;
using System.Drawing;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    public class PositionSettingScaler
    {
        float BASE_DPI = 96;
        static Point TOP_LEFT_REF_POINT = new Point(19, 14);
        static Point BOT_LEFT_REF_POINT = new Point(17, 95);

        Point tl;
        Point tr;
        Point bl;
        Point br;

        int leftRightLine;
        int topBottomLine;

        public PositionSettingScaler(PictureBox background, Bitmap overlay)
        {
            Graphics g = Graphics.FromImage(overlay);
            float currentDPI = BASE_DPI;
            using (g)
            {
                currentDPI = g.DpiX;
            }
            float scalingFactor = currentDPI / BASE_DPI;

            int bgWidth = background.Width;
            int bgHeight = background.Height;
            int ovWidth = overlay.Width;
            int ovHeight = overlay.Height;
            int tFudge = GetScaledValue(2, scalingFactor);
            int bFudge = GetScaledValue(4, scalingFactor);

            Point tRef = GetScaledPoint(TOP_LEFT_REF_POINT, scalingFactor);
            Point bRef = GetScaledPoint(BOT_LEFT_REF_POINT, scalingFactor);

            tl = tRef;
            tr = new Point(bgWidth - tl.X - ovWidth + tFudge, tl.Y);
            bl = new Point(bRef.X, bRef.Y - ovHeight);
            br = new Point(bgWidth - bl.X - ovWidth + bFudge, bl.Y);

            leftRightLine = bgWidth / 2;
            topBottomLine = (bgHeight - GetScaledValue(40, scalingFactor)) / 2;
        }

        private Point GetScaledPoint(Point point, float scalingFactor)
        {
            return GetScaledPoint(point.X, point.Y, scalingFactor);
        }

        private Point GetScaledPoint(int x, int y, float scalingFactor)
        {
            return new Point(GetScaledValue(x, scalingFactor), GetScaledValue(y, scalingFactor));
        }

        private int GetScaledValue(int v, float scalingFactor)
        {
            return (int)(v * scalingFactor);
        }

        public Point TopLeft
        {
            get
            {
                return tl;
            }
        }

        public Point TopRight
        {
            get
            {
                return tr;
            }
        }

        public Point BottomLeft
        {
            get
            {
                return bl;
            }
        }

        public Point BottomRight
        {
            get
            {
                return br;
            }
        }

        public Point GetQuadrantFromCoordinates(int x, int y)
        {
            Point p = TopRight;

            if (x > leftRightLine)
            {
                p = (y > topBottomLine ? BottomRight : TopRight);
            }
            else
            {
                p = (y > topBottomLine ? BottomLeft : TopLeft);
            }

            return p;
        }
    }
}

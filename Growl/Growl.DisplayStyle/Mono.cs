using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Growl.DisplayStyle
{
	
	
	public class Mono
	{
		
		public Mono()
		{
		}
		
		public static Region CreateRoundRectRegion(
            int x, 
            int y, 
            int width, 
            int height, 
            int xradius,
            int yradius 
            )
		{
			GraphicsPath path = new GraphicsPath();
			Rectangle baserect = new Rectangle(x, y, width, height);
			
			PointF p = new PointF(Math.Min(xradius * 2, width), Math.Min(yradius * 2, height));
			path.StartFigure();
			path.AddArc(baserect.X, baserect.Y, p.X, p.Y, 180, 90);
			path.AddArc(baserect.Right - p.X, baserect.Y, p.X, p.Y, 270, 90);
			path.AddArc(baserect.Right - p.X, baserect.Bottom - p.Y, p.X, p.Y, 0, 90);
			path.AddArc(baserect.X, baserect.Bottom - p.Y, p.X, p.Y, 90, 90);
			path.CloseFigure();
			Region rgn = new Region(path);

			return rgn;
		}
		

	}
	
}

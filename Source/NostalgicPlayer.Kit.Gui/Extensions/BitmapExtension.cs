/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Polycode.NostalgicPlayer.Kit.Gui.Extensions
{
	/// <summary>
	/// Extension methods to the Bitmap class
	/// </summary>
	public static class BitmapExtension
	{
		/********************************************************************/
		/// <summary>
		/// Create a circular bitmap from the given source bitmap
		/// </summary>
		/********************************************************************/
		public static Bitmap CreateCircularBitmap(this Bitmap bitmap, int borderWidth = 0)
		{
			int size = Math.Min(bitmap.Width, bitmap.Height);
			Bitmap newBitmap = new Bitmap(size, size);

			using (Graphics g = Graphics.FromImage(newBitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;

				using (GraphicsPath path = new GraphicsPath())
				{
					path.AddEllipse(0, 0, size, size);
					g.SetClip(path);
					g.DrawImage(bitmap, 0, 0, size, size);
					g.ResetClip();
				}

				if (borderWidth > 0)
				{
					using (Pen borderPen = new Pen(Color.White, borderWidth))
					{
						float offset = borderWidth / 2.0f;
						g.DrawEllipse(borderPen, offset, offset, size - borderWidth, size - borderWidth);
					}
				}
			}

			return newBitmap;
		}
	}
}

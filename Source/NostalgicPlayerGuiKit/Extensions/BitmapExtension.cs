/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Polycode.NostalgicPlayer.GuiKit.Extensions
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
		public static Bitmap CreateCircularBitmap(this Bitmap bitmap)
		{
			int size = Math.Min(bitmap.Width, bitmap.Height);
			Bitmap newBitmap = new Bitmap(size, size);

			using (Graphics g = Graphics.FromImage(newBitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;

				using (Brush brush = new TextureBrush(bitmap))
				{
					GraphicsPath path = new GraphicsPath();
					path.AddEllipse(0, 0, size, size);
					g.FillPath(brush, path);
				}
			}

			return newBitmap;
		}
	}
}

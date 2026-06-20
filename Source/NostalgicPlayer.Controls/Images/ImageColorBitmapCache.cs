/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds a cache of the same bitmap but with different colors
	/// </summary>
	internal class ImageColorBitmapCache : ImageBase
	{
		private readonly string category;
		private readonly string resourceName;
		private readonly int width;
		private readonly int height;

		private readonly Dictionary<Color, Bitmap> bitmaps;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ImageColorBitmapCache(string category, string resourceName, int width, int height)
		{
			this.category = category;
			this.resourceName = resourceName;
			this.width = width;
			this.height = height;

			bitmaps = new Dictionary<Color, Bitmap>();
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			FlushBitmaps();
		}



		/********************************************************************/
		/// <summary>
		/// Return the bitmap with the color given
		/// </summary>
		/********************************************************************/
		public Bitmap GetBitmap(Color color)
		{
			if (!bitmaps.TryGetValue(color, out Bitmap bitmap))
			{
				bitmap = GetSvgBitmap(category, resourceName, color, width, height);
				bitmaps.Add(color, bitmap);
			}

			return bitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Flush all bitmaps
		/// </summary>
		/********************************************************************/
		private void FlushBitmaps()
		{
			foreach (Bitmap bitmap in bitmaps.Values)
				bitmap.Dispose();

			bitmaps.Clear();
		}
	}
}

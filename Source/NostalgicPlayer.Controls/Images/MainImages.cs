/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Main window
	/// </summary>
	internal class MainImages : ThemedImageBase, IMainImages
	{
		private const string Category = "Main";

		private Dictionary<Color, Bitmap> playingItems;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainImages(IThemeManager themeManager) : base(themeManager)
		{
			playingItems = new Dictionary<Color, Bitmap>();
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			base.Dispose();

			FlushImages();
		}



		/********************************************************************/
		/// <summary>
		/// Flush images
		/// </summary>
		/********************************************************************/
		protected override void FlushImages()
		{
			FlushPlayingItems();
		}



		/********************************************************************/
		/// <summary>
		/// Gets the playing item image
		/// </summary>
		/********************************************************************/
		public Bitmap GetPlayingItem(Color color)
		{
			if (!playingItems.TryGetValue(color, out Bitmap bitmap))
			{
				if (playingItems.Count == 2)
					FlushPlayingItems();

				bitmap = GetSvgBitmap(Category, "PlayingItem", color, 10, 10);
				playingItems.Add(color, bitmap);
			}

			return bitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Flush all playing item bitmaps
		/// </summary>
		/********************************************************************/
		private void FlushPlayingItems()
		{
			foreach (Bitmap bitmap in playingItems.Values)
				bitmap.Dispose();

			playingItems.Clear();
		}
	}
}

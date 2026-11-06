/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple.Images
{
	/// <summary>
	/// Purple theme colors for ModLibrary window images
	/// </summary>
	internal class PurpleImageModLibraryColors : IImageModLibraryColors
	{
		private static readonly Color directoryColor = Color.FromArgb(0, 0, 0);
		private static readonly Color fileColor = Color.FromArgb(0, 0, 0);
		private static readonly Color favoriteColor = Color.FromArgb(237, 105, 255);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DirectoryColor => directoryColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FileColor => fileColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FavoriteColor => favoriteColor;
	}
}

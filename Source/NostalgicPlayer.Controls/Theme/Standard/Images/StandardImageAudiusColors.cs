/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard.Images
{
	/// <summary>
	/// Standard theme colors for Audius window images
	/// </summary>
	internal class StandardImageAudiusColors : IImageAudiusColors
	{
		private static readonly Color repostColor = Color.FromArgb(237, 105, 255);
		private static readonly Color favoriteColor = Color.FromArgb(237, 105, 255);
		private static readonly Color showProfileInfoColor = Color.FromArgb(0, 0, 0);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color RepostColor => repostColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FavoriteColor => favoriteColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ShowProfileInfoColor => showProfileInfoColor;
	}
}

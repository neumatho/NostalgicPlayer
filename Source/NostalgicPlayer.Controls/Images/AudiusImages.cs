/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Main window
	/// </summary>
	internal class AudiusImages : ThemedImageBase, IAudiusImages
	{
		private const string Category = "Audius";

		private Bitmap repost;
		private Bitmap favorite;

		private Bitmap showProfileInfo;

		private Bitmap close;

		private Bitmap unknownAlbum;
		private Bitmap unknownProfile;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusImages(IThemeManager themeManager) : base(themeManager)
		{
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
			repost?.Dispose();
			repost = null;
			favorite?.Dispose();
			favorite = null;

			showProfileInfo?.Dispose();
			showProfileInfo = null;

			close?.Dispose();
			close = null;

			unknownAlbum?.Dispose();
			unknownAlbum = null;
			unknownProfile?.Dispose();
			unknownProfile = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the repost image
		/// </summary>
		/********************************************************************/
		public Bitmap Repost
		{
			get
			{
				if (repost == null)
					repost = GetSvgBitmap(Category, nameof(IAudiusImages.Repost), CurrentColors.RepostColor, 16, 16);

				return repost;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the favorite image
		/// </summary>
		/********************************************************************/
		public Bitmap Favorite
		{
			get
			{
				if (favorite == null)
					favorite = GetSvgBitmap(Category, nameof(IAudiusImages.Favorite), CurrentColors.FavoriteColor, 16, 16);

				return favorite;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the show profile information image
		/// </summary>
		/********************************************************************/
		public Bitmap ShowProfileInfo
		{
			get
			{
				if (showProfileInfo == null)
					showProfileInfo = GetSvgBitmap(Category, nameof(IAudiusImages.ShowProfileInfo), CurrentColors.ShowProfileInfoColor, 20, 20);

				return showProfileInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the close profile image
		/// </summary>
		/********************************************************************/
		public Bitmap Close
		{
			get
			{
				if (close == null)
					close = GetSvgBitmap(Category, nameof(IAudiusImages.Close), Color.White, 16, 16);

				return close;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the unknown album cover image
		/// </summary>
		/********************************************************************/
		public Bitmap UnknownAlbumCover
		{
			get
			{
				if (unknownAlbum == null)
					unknownAlbum = GetBitmap(Category, "UnknownAlbumCover.png");

				return unknownAlbum;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the unknown profile image
		/// </summary>
		/********************************************************************/
		public Bitmap UnknownProfile
		{
			get
			{
				if (unknownProfile == null)
					unknownProfile = GetBitmap(Category, "UnknownProfile.png");

				return unknownProfile;
			}
		}
	}
}

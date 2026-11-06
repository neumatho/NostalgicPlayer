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
	/// Holds all the images needed by the Module Library window
	/// </summary>
	internal class ModuleLibraryImages : ThemedImageBase, IModuleLibraryImages
	{
		private const string Category = "ModuleLibrary";

		private Bitmap directory;
		private Bitmap file;

		private Bitmap favorite;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleLibraryImages(IThemeManager themeManager) : base(themeManager)
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
			directory?.Dispose();
			directory = null;
			file?.Dispose();
			file = null;

			favorite?.Dispose();
			favorite = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the directory image
		/// </summary>
		/********************************************************************/
		public Bitmap Directory
		{
			get
			{
				if (directory == null)
					directory = GetSvgBitmap(Category, nameof(IModuleLibraryImages.Directory), CurrentColors.DirectoryColor, 12, 12);

				return directory;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the file image
		/// </summary>
		/********************************************************************/
		public Bitmap File
		{
			get
			{
				if (file == null)
					file = GetSvgBitmap(Category, nameof(IModuleLibraryImages.File), CurrentColors.FileColor, 12, 12);

				return file;
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
					favorite = GetSvgBitmap(Category, nameof(IModuleLibraryImages.Favorite), CurrentColors.FavoriteColor, 12, 12);

				return favorite;
			}
		}
	}
}

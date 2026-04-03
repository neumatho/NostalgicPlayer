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
	/// Holds all the images needed by the Module Information window
	/// </summary>
	internal class ModuleInformationImages : ThemedImageBase, IModuleInformationImages
	{
		private const string Category = "ModuleInformation";

		private Bitmap previousPicture;
		private Bitmap nextPicture;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInformationImages(IThemeManager themeManager) : base(themeManager)
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
			previousPicture?.Dispose();
			previousPicture = null;

			nextPicture?.Dispose();
			nextPicture = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the previous picture image
		/// </summary>
		/********************************************************************/
		public Bitmap PreviousPicture
		{
			get
			{
				if (previousPicture == null)
					previousPicture = GetSvgBitmap(Category, "PreviousPicture", CurrentColors.PreviousPictureColor, 24, 24);

				return previousPicture;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the next picture image
		/// </summary>
		/********************************************************************/
		public Bitmap NextPicture
		{
			get
			{
				if (nextPicture == null)
					nextPicture = GetSvgBitmap(Category, "NextPicture", CurrentColors.NextPictureColor, 24, 24);

				return nextPicture;
			}
		}
	}
}

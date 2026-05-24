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
	/// Holds all the general usable images
	/// </summary>
	internal class GeneralImages : ThemedImageBase, IGeneralImages
	{
		private const string Category = "General";

		private Bitmap logo;
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public GeneralImages(IThemeManager themeManager) : base(themeManager)
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
			logo?.Dispose();
			logo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the logo image
		/// </summary>
		/********************************************************************/
		public Bitmap Logo
		{
			get
			{
				if (logo == null)
					logo = GetBitmap(Category, "Logo.png");

				return logo;
			}
		}
	}
}

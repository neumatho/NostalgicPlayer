/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the form
	/// </summary>
	internal class FormImages : ImageBase, IFormImages
	{
		private const string Category = "Form";

		private Bitmap captionClose;
		private Bitmap captionMaximize;
		private Bitmap captionMinimize;
		private Bitmap captionNormalize;

		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			captionClose?.Dispose();
			captionClose = null;

			captionMaximize?.Dispose();
			captionMaximize = null;

			captionMinimize?.Dispose();
			captionMinimize = null;

			captionNormalize?.Dispose();
			captionNormalize = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the close button image
		/// </summary>
		/********************************************************************/
		public Bitmap CaptionClose
		{
			get
			{
				if (captionClose == null)
					captionClose = GetBitmap(Category, "CaptionClose.png");

				return captionClose;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the maximize button image
		/// </summary>
		/********************************************************************/
		public Bitmap CaptionMaximize
		{
			get
			{
				if (captionMaximize == null)
					captionMaximize = GetBitmap(Category, "CaptionMaximize.png");

				return captionMaximize;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the minimize button image
		/// </summary>
		/********************************************************************/
		public Bitmap CaptionMinimize
		{
			get
			{
				if (captionMinimize == null)
					captionMinimize = GetBitmap(Category, "CaptionMinimize.png");

				return captionMinimize;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the normalize button image
		/// </summary>
		/********************************************************************/
		public Bitmap CaptionNormalize
		{
			get
			{
				if (captionNormalize == null)
					captionNormalize = GetBitmap(Category, "CaptionNormalize.png");

				return captionNormalize;
			}
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Module Information window
	/// </summary>
	internal class ModuleInformationImages : ImageBase, IModuleInformationImages, IDisposable
	{
		private const string Category = "ModuleInformation";

		private Bitmap previousPicture;
		private Bitmap nextPicture;

		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public void Dispose()
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
		public Bitmap GetPreviousPicture(Color color)
		{
			if (previousPicture == null)
				previousPicture = GetSvgBitmap(Category, "PreviousPicture", color, 24, 24);

			return previousPicture;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the next picture image
		/// </summary>
		/********************************************************************/
		public Bitmap GetNextPicture(Color color)
		{
			if (nextPicture == null)
				nextPicture = GetSvgBitmap(Category, "NextPicture", color, 24, 24);

			return nextPicture;
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Contains all the images used in NostalgicPlayer
	/// </summary>
	internal class NostalgicImageBank : INostalgicImageBank, IDisposable
	{
		private readonly FormImages formImages = new FormImages();
		private readonly ModuleInformationImages moduleInformationImages = new ModuleInformationImages();

		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			formImages.Dispose();
			moduleInformationImages.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the form
		/// </summary>
		/********************************************************************/
		public IFormImages Form => formImages;



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the Module Information window
		/// </summary>
		/********************************************************************/
		public IModuleInformationImages ModuleInformation => moduleInformationImages;
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Contains all the images used in NostalgicPlayer
	/// </summary>
	internal class NostalgicImageBank : INostalgicImageBank, IDisposable
	{
		private readonly FormImages formImages;
		private readonly ModuleInformationImages moduleInformationImages;
		private readonly SampleInformationImages sampleInformationImages;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicImageBank(IThemeManager themeManager)
		{
			formImages = new FormImages();
			moduleInformationImages = new ModuleInformationImages(themeManager);
			sampleInformationImages = new SampleInformationImages(themeManager);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			formImages.Dispose();
			moduleInformationImages.Dispose();
			sampleInformationImages.Dispose();
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



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the Sample Information window
		/// </summary>
		/********************************************************************/
		public ISampleInformationImages SampleInformation => sampleInformationImages;
	}
}

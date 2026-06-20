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
		private readonly IThemeManager themeManager;

		private GeneralImages generalImages;
		private FormImages formImages;
		private MainImages mainImages;
		private ModuleInformationImages moduleInformationImages;
		private SampleInformationImages sampleInformationImages;
		private AudiusImages audiusImages;

		private GeneralImages generalImagesCopy;
		private FormImages formImagesCopy;
		private MainImages mainImagesCopy;
		private ModuleInformationImages moduleInformationImagesCopy;
		private SampleInformationImages sampleInformationImagesCopy;
		private AudiusImages audiusImagesCopy;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicImageBank(IThemeManager themeManager)
		{
			this.themeManager = themeManager;

			themeManager.BeforeThemeChange += BeforeThemeChange;
			themeManager.AfterThemeChange += AfterThemeChange;

			CreateImageContainers();
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			themeManager.BeforeThemeChange -= BeforeThemeChange;
			themeManager.AfterThemeChange -= AfterThemeChange;

			generalImages.Dispose();
			formImages.Dispose();
			mainImages.Dispose();
			moduleInformationImages.Dispose();
			sampleInformationImages.Dispose();
			audiusImages.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Holds all the general usable images
		/// </summary>
		/********************************************************************/
		public IGeneralImages General => generalImages;



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the form
		/// </summary>
		/********************************************************************/
		public IFormImages Form => formImages;



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the Main window
		/// </summary>
		/********************************************************************/
		public IMainImages Main => mainImages;



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



		/********************************************************************/
		/// <summary>
		/// Holds all the images needed by the Audius window
		/// </summary>
		/********************************************************************/
		public IAudiusImages Audius => audiusImages;



		/********************************************************************/
		/// <summary>
		/// Is called right before the theme changes. Make a copy of all
		/// bitmaps
		/// </summary>
		/********************************************************************/
		private void BeforeThemeChange(object sender, EventArgs e)
		{
			generalImagesCopy = generalImages;
			formImagesCopy = formImages;
			mainImagesCopy = mainImages;
			moduleInformationImagesCopy = moduleInformationImages;
			sampleInformationImagesCopy = sampleInformationImages;
			audiusImagesCopy = audiusImages;

			CreateImageContainers();
		}



		/********************************************************************/
		/// <summary>
		/// Is called after the theme changes. At this time, all controls
		/// will reference new bitmaps, so it is safe to dispose the old ones
		/// </summary>
		/********************************************************************/
		private void AfterThemeChange(object sender, EventArgs e)
		{
			generalImagesCopy.Dispose();
			generalImagesCopy = null;

			formImagesCopy.Dispose();
			formImagesCopy = null;

			mainImagesCopy.Dispose();
			mainImagesCopy = null;

			moduleInformationImagesCopy.Dispose();
			moduleInformationImagesCopy = null;

			sampleInformationImagesCopy.Dispose();
			sampleInformationImagesCopy = null;

			audiusImagesCopy.Dispose();
			audiusImagesCopy = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create new instances of the image containers
		/// </summary>
		/********************************************************************/
		private void CreateImageContainers()
		{
			generalImages = new GeneralImages(themeManager);
			formImages = new FormImages();
			mainImages = new MainImages(themeManager);
			moduleInformationImages = new ModuleInformationImages(themeManager);
			sampleInformationImages = new SampleInformationImages(themeManager);
			audiusImages = new AudiusImages(themeManager);
		}
	}
}

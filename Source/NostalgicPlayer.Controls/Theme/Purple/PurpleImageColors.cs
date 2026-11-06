/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Purple.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	/// <summary>
	/// Holds all colors used by the images
	/// </summary>
	internal class PurpleImageColors : IImageColors
	{
		private static readonly PurpleImageGeneralColors imageGeneralColors = new PurpleImageGeneralColors();
		private static readonly PurpleImageMainColors imageMainColors = new PurpleImageMainColors();
		private static readonly PurpleImageModuleInformationColors imageModuleInformationColors = new PurpleImageModuleInformationColors();
		private static readonly PurpleImageSampleInformationColors imageSampleInformationColors = new PurpleImageSampleInformationColors();
		private static readonly PurpleImageAudiusColors imageAudiusColors = new PurpleImageAudiusColors();
		private static readonly PurpleImageModLibraryColors imageModLibraryColors = new PurpleImageModLibraryColors();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageGeneralColors GeneralColors => imageGeneralColors;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageMainColors MainColors => imageMainColors;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageModuleInformationColors ModuleInformationColors => imageModuleInformationColors;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageSampleInformationColors SampleInformationColors => imageSampleInformationColors;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageAudiusColors AudiusColors => imageAudiusColors;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IImageModLibraryColors ModLibraryColors => imageModLibraryColors;
	}
}

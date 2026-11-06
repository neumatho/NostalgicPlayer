/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Standard.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard
{
	/// <summary>
	/// Holds all colors used by the images
	/// </summary>
	internal class StandardImageColors : IImageColors
	{
		private static readonly StandardImageGeneralColors imageGeneralColors = new StandardImageGeneralColors();
		private static readonly StandardImageMainColors imageMainColors = new StandardImageMainColors();
		private static readonly StandardImageModuleInformationColors imageModuleInformationColors = new StandardImageModuleInformationColors();
		private static readonly StandardImageSampleInformationColors imageSampleInformationColors = new StandardImageSampleInformationColors();
		private static readonly StandardImageAudiusColors imageAudiusColors = new StandardImageAudiusColors();
		private static readonly StandardImageModLibraryColors imageModLibraryColors = new StandardImageModLibraryColors();

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

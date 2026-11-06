/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by the images
	/// </summary>
	public interface IImageColors
	{
		/// <summary></summary>
		IImageGeneralColors GeneralColors { get; }
		/// <summary></summary>
		IImageMainColors MainColors { get; }
		/// <summary></summary>
		IImageModuleInformationColors ModuleInformationColors { get; }
		/// <summary></summary>
		IImageSampleInformationColors SampleInformationColors { get; }
		/// <summary></summary>
		IImageAudiusColors AudiusColors { get; }
		/// <summary></summary>
		IImageModLibraryColors ModLibraryColors { get; }
	}
}

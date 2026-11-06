/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images
{
	/// <summary>
	/// Holds the Sample Information image colors
	/// </summary>
	public interface IImageSampleInformationColors
	{
		/// <summary></summary>
		Color SampleLoopColor { get; }
		/// <summary></summary>
		Color SamplePingPongColor { get; }
		/// <summary></summary>
		Color SampleStereoColor { get; }
		/// <summary></summary>
		Color SampleMultiOctavesColor { get; }
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all colors used by the images
	/// </summary>
	public interface IImageColors
	{
		/// <summary></summary>
		Color PreviousPictureColor { get; }
		/// <summary></summary>
		Color NextPictureColor { get; }

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

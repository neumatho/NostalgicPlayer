/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the images needed by the Sample Information window
	/// </summary>
	public interface ISampleInformationImages
	{
		/// <summary>
		/// Gets the sample loop image
		/// </summary>
		Bitmap SampleLoop { get; }

		/// <summary>
		/// Gets the sample ping-pong image
		/// </summary>
		Bitmap SamplePingPong { get; }

		/// <summary>
		/// Gets the sample stereo image
		/// </summary>
		Bitmap SampleStereo { get; }

		/// <summary>
		/// Gets the sample multi octaves image
		/// </summary>
		Bitmap SampleMultiOctaves { get; }
	}
}

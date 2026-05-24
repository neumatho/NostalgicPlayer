/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers
{
	/// <summary>
	/// Different methods to generate the samples
	/// </summary>
	public enum SamplingMethod
	{
		/// <summary>
		/// Linear interpolation (fast but low quality)
		/// </summary>
		DECIMATE,

		/// <summary>
		/// Sinc resampling (high quality but CPU intensive)
		/// </summary>
		RESAMPLE,

		/// <summary>
		/// No resampling (raw 1 MHz output)
		/// </summary>
		NONE
	}
}

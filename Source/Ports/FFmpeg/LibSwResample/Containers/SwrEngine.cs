/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// Resampling Engines
	/// </summary>
	public enum SwrEngine
	{
		/// <summary>
		/// SW Resampler
		/// </summary>
		Swr,

		/// <summary>
		/// SoX Resampler
		/// </summary>
		Soxr,

		/// <summary>
		/// Not part of API/ABI
		/// </summary>
		Nb
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// Resampling Filter Types
	/// </summary>
	public enum SwrFilterType
	{
		/// <summary>
		/// Cubic
		/// </summary>
		Cubic,

		/// <summary>
		/// Blackman Nuttall windowed sinc
		/// </summary>
		Blackman_Nuttall,

		/// <summary>
		/// Kaiser windowed sinc
		/// </summary>
		Kaiser
	}
}

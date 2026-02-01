/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// The duration of a video can be estimated through various ways, and this enum can be used
	/// to know how the duration was estimated
	/// </summary>
	public enum AvDurationEstimationMethod
	{
		/// <summary>
		/// Duration accurately estimated from PTSes
		/// </summary>
		From_Pts,

		/// <summary>
		/// Duration estimated from a stream with a known duration
		/// </summary>
		From_Stream,

		/// <summary>
		/// Duration estimated from bitrate (less accurate)
		/// </summary>
		From_Bitrate,
	}
}

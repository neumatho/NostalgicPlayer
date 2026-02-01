/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvStreamParseType
	{

		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// Full parsing and repack
		/// </summary>
		Full,

		/// <summary>
		/// Only parse headers, do not repack
		/// </summary>
		Headers,

		/// <summary>
		/// Full parsing and interpolation of timestamps for frames not starting on a packet boundary
		/// </summary>
		Timestamps,

		/// <summary>
		/// Full parsing and repack of the first frame only, only implemented for H.264 currently
		/// </summary>
		Full_Once,

		/// <summary>
		/// Full parsing and repack with timestamp and position generation by parser for raw
		/// this assumes that each packet in the file contains no demuxer level headers and
		/// just codec level data, otherwise position generation would fail
		/// </summary>
		Full_Raw
	}
}

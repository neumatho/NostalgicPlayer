/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Defs
	{
		/// <summary>
		/// Required number of additionally allocated bytes at the end of the input bitstream for decoding.
		/// This is mainly needed because some optimized bitstream readers read
		/// 32 or 64 bit at once and could read over the end.
		///
		/// Note: If the first 23 bits of the additional bytes are not 0, then damaged
		/// MPEG bitstreams could cause overread and segfault
		/// </summary>
		public const c_int Av_Input_Buffer_Padding_Size = 64;
	}
}

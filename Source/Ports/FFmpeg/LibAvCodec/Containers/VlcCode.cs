/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VlcCode
	{
		/// <summary>
		/// 
		/// </summary>
		public uint8_t Bits;

		/// <summary>
		/// 
		/// </summary>
		public VlcBaseType Symbol;

		/// <summary>
		/// Codeword, with the first bit-to-be-read in the msb
		/// (even if intended for a little-endian bitstream reader)
		/// </summary>
		public uint32_t Code;
	}
}

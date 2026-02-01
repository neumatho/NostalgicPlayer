/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class GetBitContext
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buffer;

		/// <summary>
		/// 
		/// </summary>
		public c_int Index;

		/// <summary>
		/// 
		/// </summary>
		public c_int Size_In_Bits;

		/// <summary>
		/// 
		/// </summary>
		public c_int Size_In_Bits_Plus8;
	}
}

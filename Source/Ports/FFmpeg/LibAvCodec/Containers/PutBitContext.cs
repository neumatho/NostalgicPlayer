/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using BitBuf = System.UInt64;

using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class PutBitContext
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly BitBuf Bit_Buf = new BitBuf();

		/// <summary>
		/// 
		/// </summary>
		public c_int Bit_Left;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buf;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buf_Ptr;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buf_End;
	}
}

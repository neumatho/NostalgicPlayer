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
	public class GetByteContext
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buffer;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buffer_End;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buffer_Start;
	}
}

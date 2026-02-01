/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvSha
	{
		internal delegate void Transform_Delegate(CPointer<uint32_t> state, CPointer<uint8_t> buffer);

		/// <summary>
		/// Digest length in 32-bit words
		/// </summary>
		internal uint8_t Digest_Len;

		/// <summary>
		/// Number of bytes in buffer
		/// </summary>
		internal uint64_t Count;

		/// <summary>
		/// 512-bit buffer of input values used in hash updating
		/// </summary>
		internal readonly uint8_t[] Buffer = new uint8_t[64];

		/// <summary>
		/// Current hash value
		/// </summary>
		internal readonly uint32_t[] State = new uint32_t[8];

		/// <summary>
		/// Function used to update hash for 512-bit input block
		/// </summary>
		internal Transform_Delegate Transform;
	}
}

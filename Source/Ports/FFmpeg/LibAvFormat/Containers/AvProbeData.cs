/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvProbeData
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> FileName;

		/// <summary>
		/// Buffer must have AVPROBE_PADDING_SIZE of extra allocated bytes filled with zero
		/// </summary>
		public CPointer<c_uchar> Buf;

		/// <summary>
		/// Size of buf except extra allocated bytes
		/// </summary>
		public c_int Buf_Size;

		/// <summary>
		/// Mime_type, when known
		/// </summary>
		public CPointer<char> Mime_Type;
	}
}

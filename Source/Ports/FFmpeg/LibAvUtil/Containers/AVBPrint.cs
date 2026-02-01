/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Buffer to print data progressively
	///
	/// The string buffer grows as necessary and is always 0-terminated.
	/// The content of the string is never accessed, and thus is
	/// encoding-agnostic and can even hold binary data.
	///
	/// Small buffers are kept in the structure itself, and thus require no
	/// memory allocation at all (unless the contents of the buffer is needed
	/// after the structure goes out of scope). This is almost as lightweight as
	/// declaring a local `char buf[512]`.
	///
	/// The length of the string can go beyond the allocated size: the buffer is
	/// then truncated, but the functions still keep account of the actual total
	/// length.
	///
	/// In other words, AVBPrint.len can be greater than AVBPrint.size and records
	/// the total length of what would have been to the buffer if there had been
	/// enough memory.
	///
	/// Append operations do not need to be tested for failure: if a memory
	/// allocation fails, data stop being appended to the buffer, but the length
	/// is still updated. This situation can be tested with
	/// av_bprint_is_complete()
	/// </summary>
	public class AVBPrint
	{
		/// <summary>
		/// String so far
		/// </summary>
		public CPointer<char> Str;

		/// <summary>
		/// Length so far
		/// </summary>
		public c_uint Len;

		/// <summary>
		/// Allocated memory
		/// </summary>
		public c_uint Size;

		/// <summary>
		/// Maximum allocated memory
		/// </summary>
		public c_uint Size_Max;

		/// <summary>
		/// 
		/// </summary>
		internal CPointer<char> Internal_Buffer = new CPointer<char>(1024 - 20);
	}
}

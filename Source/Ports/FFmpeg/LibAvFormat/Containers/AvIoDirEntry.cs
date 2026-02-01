/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Describes single entry of the directory.
	///
	/// Only name and type fields are guaranteed be set.
	/// Rest of fields are protocol or/and platform dependent and might be unknown
	/// </summary>
	public class AvIoDirEntry
	{
		/// <summary>
		/// Filename
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Type of the entry
		/// </summary>
		public AvIoDirEntryType Type;

		/// <summary>
		/// Set to 1 when name is encoded with UTF-8, 0 otherwise.
		/// Name can be encoded with UTF-8 even though 0 is set
		/// </summary>
		public c_int Utf8;

		/// <summary>
		/// File size in bytes, -1 if unknown
		/// </summary>
		public int64_t Size;

		/// <summary>
		/// Time of last modification in microseconds since unix
		/// epoch, -1 if unknown
		/// </summary>
		public int64_t Modification_Timestamp;

		/// <summary>
		/// Time of last access in microseconds since unix epoch,
		/// -1 if unknown
		/// </summary>
		public int64_t Access_Timestamp;

		/// <summary>
		/// Time of last status change in microseconds since unix
		/// epoch, -1 if unknown
		/// </summary>
		public int64_t Status_Change_Timestamp;

		/// <summary>
		/// User ID of owner, -1 if unknown
		/// </summary>
		public int64_t User_Id;

		/// <summary>
		/// Group ID of owner, -1 if unknown
		/// </summary>
		public int64_t Group_Id;

		/// <summary>
		/// Unix file mode, -1 if unknown
		/// </summary>
		public int64_t FileMode;
	}
}

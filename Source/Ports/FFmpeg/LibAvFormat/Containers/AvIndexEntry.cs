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
	public class AvIndexEntry
	{

		/// <summary>
		/// 
		/// </summary>
		public int64_t Pos;

		/// <summary>
		/// Timestamp in AVStream.time_base units, preferably the time from which on correctly decoded frames are available
		/// when seeking to this entry. That means preferable PTS on keyframe based formats.
		/// But demuxers can choose to store a different timestamp, if it is more convenient for the implementation or nothing better
		/// is known
		/// </summary>
		public int64_t Timestamp;

		/// <summary>
		/// Flag is used to indicate which frame should be discarded after decoding
		/// </summary>
		public AvIndex Flags;

		/// <summary>
		/// 
		/// </summary>
		public c_int Size;

		/// <summary>
		/// Minimum distance between this and the previous keyframe, used to avoid unneeded searching
		/// </summary>
		public c_int Min_Distance;
	}
}

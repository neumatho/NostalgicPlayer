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
	public class AvExifMetadata
	{
		/// <summary>
		/// Array of EXIF metadata entries
		/// </summary>
		public CPointer<AvExifEntry> Entries;

		/// <summary>
		/// Number of entries in this array
		/// </summary>
		public c_uint Count;

		/// <summary>
		/// Size of the buffer, used for av_fast_realloc
		/// </summary>
		public c_uint Size;
	}
}

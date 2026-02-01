/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvFmt
	{
		/// <summary>
		/// 
		/// </summary>
		NoFile = 0x0001,

		/// <summary>
		/// Needs '%d' in filename
		/// </summary>
		NeedNumber = 0x0002,

		/// <summary>
		/// 
		/// </summary>
		Experimental = 0x0004,

		/// <summary>
		/// Show format stream IDs numbers
		/// </summary>
		Show_Ids = 0x0008,

		/// <summary>
		/// Format wants global header
		/// </summary>
		GlobalHeader = 0x0040,

		/// <summary>
		/// Format does not need / have any timestamps
		/// </summary>
		NoTimestamps = 0x0080,

		/// <summary>
		/// Use generic index building code
		/// </summary>
		Generic_Index = 0x0100,

		/// <summary>
		/// Format allows timestamp discontinuities.
		/// Note, muxers always require valid (monotone) timestamps
		/// </summary>
		Ts_Discont = 0x0200,

		/// <summary>
		/// Format allows variable fps
		/// </summary>
		Variable_Fps = 0x0400,

		/// <summary>
		/// Format does not need width/height
		/// </summary>
		NoDimensions = 0x0800,

		/// <summary>
		/// Format does not require any streams
		/// </summary>
		NoStreams = 0x1000,

		/// <summary>
		/// Format does not allow to fall back on binary search via read_timestamp
		/// </summary>
		NoBinSearch = 0x2000,

		/// <summary>
		/// Format does not allow to fall back on generic search
		/// </summary>
		NoGenSearch = 0x4000,

		/// <summary>
		/// Format does not allow seeking by bytes
		/// </summary>
		No_Byte_Seek = 0x8000,

		/// <summary>
		/// Format does not require strictly increasing timestamps,
		/// but they must still be monotonic
		/// </summary>
		Ts_Nonstrict = 0x20000,

		/// <summary>
		/// Format allows muxing negative timestamps. If not set the timestamp
		/// will be shifted in av_write_frame and av_interleaved_write_frame
		/// so they start from 0.
		/// The user or muxer can override this through AVFormatContext.avoid_negative_ts
		/// </summary>
		Ts_Negative = 0x40000,

		/// <summary>
		/// Seeking is based on PTS
		/// </summary>
		Seek_To_Pts = 0x4000000
	}
}

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
	public enum AvFmtFlag
	{
		/// <summary>
		/// Generate missing pts even if it requires parsing future frames
		/// </summary>
		GenPts = 0x0001,

		/// <summary>
		/// Ignore index
		/// </summary>
		IgnIdx = 0x0002,

		/// <summary>
		/// Do not block when reading packets from input
		/// </summary>
		NonBlock = 0x0004,

		/// <summary>
		/// Ignore DTS on frames that contain both DTS and PTS
		/// </summary>
		IgnDts = 0x0008,

		/// <summary>
		/// Do not infer any values from other values, just return what is stored in the container
		/// </summary>
		NoFillIn = 0x0010,

		/// <summary>
		/// Do not use AVParsers, you also must set AVFMT_FLAG_NOFILLIN as the filling code works on frames and no parsing -> no frames. Also seeking to frames can not work if parsing to find frame boundaries has been disabled
		/// </summary>
		NoParse = 0x0020,

		/// <summary>
		/// Do not buffer frames when possible
		/// </summary>
		NoBuffer = 0x0040,

		/// <summary>
		/// The caller has supplied a custom AVIOContext, don't avio_close() it
		/// </summary>
		Custom_Io = 0x0080,

		/// <summary>
		/// Discard frames marked corrupted
		/// </summary>
		Discard_Corrupt = 0x0100,

		/// <summary>
		/// Flush the AVIOContext every packet
		/// </summary>
		Flush_Packets = 0x0200,

		/// <summary>
		/// When muxing, try to avoid writing any random/volatile data to the output.
		/// This includes any random IDs, real-time timestamps/dates, muxer version, etc.
		///
		/// This flag is mainly intended for testing
		/// </summary>
		BitExact = 0x0400,

		/// <summary>
		/// Try to interleave outputted packets by dts (using this flag can slow demuxing down)
		/// </summary>
		Sort_Dts = 0x10000,

		/// <summary>
		/// Enable fast, but inaccurate seeks for some formats
		/// </summary>
		Fast_Seek = 0x80000,

		/// <summary>
		/// Add bitstream filters as requested by the muxer
		/// </summary>
		Auto_Bsf = 0x200000
	}
}

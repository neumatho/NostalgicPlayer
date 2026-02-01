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
	public enum FFInFmtFlag
	{
		/// <summary>
		/// For an FFInputFormat with this flag set read_close() needs to be called
		/// by the caller upon read_header() failure
		/// </summary>
		Init_Cleanup = 1 << 0,

		/// <summary>
		/// Prefer the codec framerate for avg_frame_rate computation
		/// </summary>
		Prefer_Codec_FrameRate = 1 << 1,

		/// <summary>
		/// Automatically parse ID3v2 metadata
		/// </summary>
		Id3V2_Auto = 1 << 2
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvCodecFlag2
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Allow non spec compliant speedup tricks
		/// </summary>
		Fast = 1 << 0,

		/// <summary>
		/// Skip bitstream encoding
		/// </summary>
		No_Output = 1 << 2,

		/// <summary>
		/// Place global headers at every keyframe instead of in extradata
		/// </summary>
		Local_Header = 1 << 3,

		/// <summary>
		/// Input bitstream might be truncated at a packet boundaries
		/// instead of only at frame boundaries
		/// </summary>
		Chunks = 1 << 15,

		/// <summary>
		/// Discard cropping information from SPS
		/// </summary>
		Ignore_Crop = 1 << 16,

		/// <summary>
		/// Show all frames before the first keyframe
		/// </summary>
		Show_All = 1 << 22,

		/// <summary>
		/// Export motion vectors through frame side data
		/// </summary>
		Export_Mvs = 1 << 28,

		/// <summary>
		/// Do not skip samples and export skip information as frame side data
		/// </summary>
		Skip_Manual = 1 << 29,

		/// <summary>
		/// Do not reset ASS ReadOrder field on flush (subtitles decoding)
		/// </summary>
		Ro_Flush_Noop = 1 << 30,

		/// <summary>
		/// Generate/parse ICC profiles on encode/decode, as appropriate for the type of
		/// file. No effect on codecs which cannot contain embedded ICC profiles, or
		/// when compiled without support for lcms2
		/// </summary>
		Icc_Profiles = 1 << 31
	}
}

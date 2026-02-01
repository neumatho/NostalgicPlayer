/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvInputFormat
	{
		/// <summary>
		/// A comma separated list of short names for the format. New names
		/// may be appended with a minor bump
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Descriptive name for the format, meant to be more human-readable
		/// than name. You should use the NULL_IF_CONFIG_SMALL() macro
		/// to define it
		/// </summary>
		public CPointer<char> Long_Name;

		/// <summary>
		/// Can use flags: AVFMT_NOFILE, AVFMT_NEEDNUMBER, AVFMT_SHOW_IDS,
		/// AVFMT_NOTIMESTAMPS, AVFMT_GENERIC_INDEX, AVFMT_TS_DISCONT, AVFMT_NOBINSEARCH,
		/// AVFMT_NOGENSEARCH, AVFMT_NO_BYTE_SEEK, AVFMT_SEEK_TO_PTS
		/// </summary>
		public AvFmt Flags;

		/// <summary>
		/// If extensions are defined, then no probe is done. You should
		/// usually not use extension format guessing because it is not
		/// reliable enough
		/// </summary>
		public CPointer<char> Extensions;

		// <summary>
		// 
		// </summary>
//		internal AvCodecTag Codec_Tag;

		/// <summary>
		/// AVClass for the private context
		/// </summary>
		internal AvClass Priv_Class;

		/// <summary>
		/// Comma-separated list of mime types.
		/// It is used check for matching mime types while probing.
		/// See av_probe_input_format2
		/// </summary>
		public CPointer<char> Mime_Type;
	}
}

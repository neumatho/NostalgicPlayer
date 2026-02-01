/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvOutputFormat
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Descriptive name for the format, meant to be more human-readable
		/// than name. You should use the NULL_IF_CONFIG_SMALL() macro
		/// to define it
		/// </summary>
		public CPointer<char> Long_Name;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Mime_Type;

		/// <summary>
		/// Comma-separated filename extensions
		/// </summary>
		public CPointer<char> Extensions;

		/// <summary>
		/// Default audio codec
		/// </summary>
		public AvCodecId Audio_Codec;

		/// <summary>
		/// Default video codec
		/// </summary>
		public AvCodecId Video_Codec;

		/// <summary>
		/// Default subtitle codec
		/// </summary>
		public AvCodecId Subtitle_Codec;

		/// <summary>
		/// Can use flags: AVFMT_NOFILE, AVFMT_NEEDNUMBER,
		/// AVFMT_GLOBALHEADER, AVFMT_NOTIMESTAMPS, AVFMT_VARIABLE_FPS,
		/// AVFMT_NODIMENSIONS, AVFMT_NOSTREAMS,
		/// AVFMT_TS_NONSTRICT, AVFMT_TS_NEGATIVE
		/// </summary>
		public AvFmt Flags;

		// <summary>
		// List of supported codec_id-codec_tag pairs, ordered by "better
		// choice first". The arrays are all terminated by AV_CODEC_ID_NONE
		// </summary>
//		internal AvCodecTag Codec_Tag;

		/// <summary>
		/// AVClass for the private context
		/// </summary>
		internal readonly AvClass Priv_Class = null;
	}
}

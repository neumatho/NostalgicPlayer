/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvOptFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// A generic parameter which can be set by the user for muxing or encoding
		/// </summary>
		Encoding_Param = 1 << 0,

		/// <summary>
		/// A generic parameter which can be set by the user for demuxing or decoding
		/// </summary>
		Decoding_Param = 1 << 1,

		/// <summary>
		/// 
		/// </summary>
		Audio_Param = 1 << 3,

		/// <summary>
		/// 
		/// </summary>
		Video_Param = 1 << 4,

		/// <summary>
		/// 
		/// </summary>
		Subtitle_Param = 1 << 5,

		/// <summary>
		/// The option is intended for exporting values to the caller
		/// </summary>
		Export = 1 << 6,

		/// <summary>
		/// The option may not be set through the AVOptions API, only read.
		/// This flag only makes sense when AV_OPT_FLAG_EXPORT is also set
		/// </summary>
		Readonly = 1 << 7,

		/// <summary>
		/// A generic parameter which can be set by the user for bit stream filtering
		/// </summary>
		Bsf_Param = 1 << 8,

		/// <summary>
		/// A generic parameter which can be set by the user at runtime
		/// </summary>
		Runtime_Param = 1 << 15,

		/// <summary>
		/// A generic parameter which can be set by the user for filtering
		/// </summary>
		Filtering_Param = 1 << 16,

		/// <summary>
		/// Set if option is deprecated, users should refer to AVOption.help text for
		/// more information
		/// </summary>
		Deprecated = 1 << 17,

		/// <summary>
		/// Set if option constants can also reside in child objects
		/// </summary>
		Child_Consts = 1 << 18,

		/// <summary>
		/// 
		/// </summary>
		All = -1
	}
}

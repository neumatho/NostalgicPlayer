/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// An option type determines:
	///  - for native access, the underlying C type of the field that an AVOption
	///    refers to;
	///  - for foreign access, the semantics of accessing the option through this API,
	///    e.g. which av_opt_get_*() and av_opt_set_*() functions can be called, or
	///    what format will av_opt_get()/av_opt_set() expect/produce
	/// </summary>
	[Flags]
	public enum AvOptionType
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Underlying C type is unsigned int
		/// </summary>
		Flags = 1,

		/// <summary>
		/// Underlying C type is int
		/// </summary>
		Int,

		/// <summary>
		/// Underlying C type is int64_t
		/// </summary>
		Int64,

		/// <summary>
		/// Underlying C type is double
		/// </summary>
		Double,

		/// <summary>
		/// Underlying C type is float
		/// </summary>
		Float,

		/// <summary>
		/// Underlying C type is a uint8_t* that is either NULL or points to a C
		/// string allocated with the av_malloc() family of functions
		/// </summary>
		String,

		/// <summary>
		/// Underlying C type is AVRational
		/// </summary>
		Rational,

		/// <summary>
		/// Underlying C type is a uint8_t* that is either NULL or points to an array
		/// allocated with the av_malloc() family of functions. The pointer is
		/// immediately followed by an int containing the array length in bytes
		/// </summary>
		Binary,

		/// <summary>
		/// Underlying C type is AVDictionary*
		/// </summary>
		Dict,

		/// <summary>
		/// Underlying C type is uint64_t
		/// </summary>
		UInt64,

		/// <summary>
		/// Special option type for declaring named constants. Does not correspond to
		/// an actual field in the object, offset must be 0
		/// </summary>
		Const,

		/// <summary>
		/// Underlying C type is two consecutive integers
		/// </summary>
		Image_Size,

		/// <summary>
		/// Underlying C type is enum AVPixelFormat
		/// </summary>
		Pixel_Fmt,

		/// <summary>
		/// Underlying C type is enum AVSampleFormat
		/// </summary>
		Sample_Fmt,

		/// <summary>
		/// Underlying C type is AVRational
		/// </summary>
		Video_Rate,

		/// <summary>
		/// Underlying C type is int64_t
		/// </summary>
		Duration,

		/// <summary>
		/// Underlying C type is uint8_t[4]
		/// </summary>
		Color,

		/// <summary>
		/// Underlying C type is int
		/// </summary>
		Bool,

		/// <summary>
		/// Underlying C type is AVChannelLayout
		/// </summary>
		ChLayout,

		/// <summary>
		/// Underlying C type is unsigned int
		/// </summary>
		UInt,

		/// <summary>
		/// 
		/// </summary>
		Nb,

		/// <summary>
		/// May be combined with another regular option type to declare an array
		/// option.
		///
		/// For array options, AVOption.offset should refer to a pointer
		/// corresponding to the option type. The pointer should be immediately
		/// followed by an unsigned int that will store the number of elements in the
		/// array
		/// </summary>
		Array = 1 << 16,

		/// <summary>
		/// 
		/// </summary>
		All = -1
	}
}

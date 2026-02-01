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
	public enum AvEscapeFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Consider spaces special and escape them even in the middle of the
		/// string.
		/// 
		/// This is equivalent to adding the whitespace characters to the special
		/// characters lists, except it is guaranteed to use the exact same list
		/// of whitespace characters as the rest of libavutil
		/// </summary>
		Whitespace = 1 << 0,

		/// <summary>
		/// Escape only specified special characters.
		/// Without this flag, escape also any characters that may be considered
		/// special by av_get_token(), such as the single quote
		/// </summary>
		Strict = 1 << 1,

		/// <summary>
		/// Within AV_ESCAPE_MODE_XML, additionally escape single quotes for single
		/// quoted attributes
		/// </summary>
		Xml_Single_Quotes = 1 << 2,

		/// <summary>
		/// Within AV_ESCAPE_MODE_XML, additionally escape double quotes for double
		/// quoted attributes
		/// </summary>
		Xml_Double_Quotes = 1 << 3
	}
}

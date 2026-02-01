/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Parsers
	{
		private static readonly AvCodecParser[] parser_List =
		[
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvCodecParser> Av_Parser_Iterate()
		{
			return parser_List;
		}
	}
}

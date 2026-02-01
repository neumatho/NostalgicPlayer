/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class AllFormats
	{
		/********************************************************************/
		/// <summary>
		/// Iterate over all registered muxers
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvOutputFormat> Av_Muxer_Iterate()//XX 592
		{
			foreach (FFOutputFormat f in Supported.Muxer_List)
			{
				yield return f.P;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over all registered demuxers
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvInputFormat> Av_Demuxer_Iterate()//XX 613
		{
			foreach (FFInputFormat f in Supported.Demuxer_List)
			{
				yield return f.P;
			}
		}
	}
}

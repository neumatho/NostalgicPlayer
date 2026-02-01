/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class Dump
	{
		/********************************************************************/
		/// <summary>
		/// Send a nice hexadecimal dump of a buffer to the log
		/// </summary>
		/********************************************************************/
		public static void Av_Hex_Dump_Log(IContext avcl, c_int level, CPointer<uint8_t> buf, c_int size)
		{
		}
	}
}

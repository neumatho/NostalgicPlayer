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
	public enum ParserFlag
	{
		/// <summary>
		/// 
		/// </summary>
		Complete_Frames = 0x0001,

		/// <summary>
		/// 
		/// </summary>
		Once = 0x0002,

		/// <summary>
		/// 
		/// </summary>
		Fetched_Offset = 0x0004,

		/// <summary>
		/// 
		/// </summary>
		Use_Codec_Ts = 0x1000
	}
}

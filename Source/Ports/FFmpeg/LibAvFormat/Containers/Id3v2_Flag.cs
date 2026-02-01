/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum Id3v2_Flag
	{
		/// <summary></summary>
		None = 0,
		/// <summary></summary>
		DataLen = 0x0001,
		/// <summary></summary>
		Unsync = 0x0002,
		/// <summary></summary>
		Encryption = 0x0004,
		/// <summary></summary>
		Compression = 0x0008
	}
}

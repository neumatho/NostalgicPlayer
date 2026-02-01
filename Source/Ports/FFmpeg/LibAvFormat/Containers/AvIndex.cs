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
	public enum AvIndex
	{
		/// <summary>
		/// 
		/// </summary>
		KeyFrame = 0x0001,

		/// <summary>
		/// 
		/// </summary>
		Discard_Frame = 0x0002
	}
}

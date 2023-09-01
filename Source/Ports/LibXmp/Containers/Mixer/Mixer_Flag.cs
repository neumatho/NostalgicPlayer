/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum Mixer_Flag
	{
		Voice_Release = (1 << 0),
		AntiClick = (1 << 1),
		Sample_Loop = (1 << 2),
		Voice_Reverse = (1 << 3),
		Voice_BiDir = (1 << 4)
	}
}

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
	internal enum Mixer_Index_Flag
	{
		_16_Bits = 0x01,
		Stereo = 0x02,
		Filter = 0x04,
		Active = 0x10,

		FlagMask = _16_Bits | Stereo | Filter
	}
}

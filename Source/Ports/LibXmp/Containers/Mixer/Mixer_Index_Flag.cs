/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer
{
	/// <summary>
	/// Mixers array index:
	///
	/// Bit 0: 0=8 bit sample, 1=16 bit sample
	/// Bit 1: 0=mono sample, 1=stereo sample
	/// Bit 2: 0=mono output, 1=stereo output
	/// Bit 3: 0=unfiltered, 1=filtered
	/// </summary>
	[Flags]
	internal enum Mixer_Index_Flag
	{
		_16_Bits = 0x01,
		Stereo = 0x02,
		StereoOut = 0x04,
		Filter = 0x08,
		Active = 0x10,

		FlagMask = _16_Bits | Stereo | StereoOut | Filter
	}
}

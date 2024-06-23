/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Different instrument flags
	/// </summary>
	[Flags]
	internal enum InstrumentFlag : uint
	{
		PlayOnMidi = 0x01,
		Mute = 0x02
	}
}

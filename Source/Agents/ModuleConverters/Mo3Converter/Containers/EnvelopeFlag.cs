/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Different envelope flags
	/// </summary>
	[Flags]
	internal enum EnvelopeFlag : byte
	{
		Enabled = 0x01,
		Sustain = 0x02,
		Loop = 0x04,
		Filter = 0x10,
		Carry = 0x20
	}
}

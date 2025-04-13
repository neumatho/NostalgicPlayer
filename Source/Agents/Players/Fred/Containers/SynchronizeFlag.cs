/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Synchronize flags
	/// </summary>
	[Flags]
	internal enum SynchronizeFlag
	{
		PulseXShot = 0x01,
		PulseSync = 0x02,
		BlendXShot = 0x04,
		BlendSync = 0x08
	}
}

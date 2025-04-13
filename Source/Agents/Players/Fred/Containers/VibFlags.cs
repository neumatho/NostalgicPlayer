/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Different vibrato flags
	/// </summary>
	[Flags]
	internal enum VibFlags
	{
		None = 0,
		VibDirection = 0x01,
		PeriodDirection = 0x02
	}
}

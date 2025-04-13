/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Song flags
	/// </summary>
	[Flags]
	internal enum MmdFlag3
	{
		Stereo = 0x01,				// Mixing in Stereo mode
		FreePan = 0x02,				// Free panning
		Gm = 0x04					// Module designed for GM/XG compatibility
	}
}
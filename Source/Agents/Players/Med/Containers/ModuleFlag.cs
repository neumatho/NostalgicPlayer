/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Med.Containers
{
	/// <summary>
	/// Module flags
	/// </summary>
	[Flags]
	internal enum ModuleFlag : byte
	{
		FilterOn = 0x01,
		Jumping = 0x02,
		Every8th = 0x04,
		SamplesAttached = 0x08
	}
}

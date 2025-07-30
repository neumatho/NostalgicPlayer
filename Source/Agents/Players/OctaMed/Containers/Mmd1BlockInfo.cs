/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD1 block info structure
	/// </summary>
	internal class Mmd1BlockInfo
	{
		public uint HlMask { get; set; }
		public uint BlockName { get; set; }
		public uint BlockNameLen { get; set; }
		public uint PageTable { get; set; }
		public uint CmdExtTable { get; set; }
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// Channel structure
	/// </summary>
	internal class ChannelInfo
	{
		public byte Left { get; set; }
		public byte Right { get; set; }

		public ushort Len { get; set; }
		public ushort LLoop { get; set; }
		public ushort RLoop { get; set; }

		public ChStep[] Steps { get; set; }
	}
}

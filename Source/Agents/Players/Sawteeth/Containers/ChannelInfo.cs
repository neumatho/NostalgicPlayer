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
		public byte Left;
		public byte Right;

		public ushort Len;
		public ushort LLoop;
		public ushort RLoop;

		public ChStep[] Steps;
	}
}

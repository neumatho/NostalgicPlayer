/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// Part structure
	/// </summary>
	internal class Part
	{
		public string Name;

		public Step[] Steps;
		public byte Sps;				// PAL-screens per step
		public byte Len;
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// Step structure
	/// </summary>
	internal class Step
	{
		public byte Note { get; set; }
		public byte Ins { get; set; }
		public byte Eff { get; set; }				// 4 bits eff, 4 bits value
	}
}

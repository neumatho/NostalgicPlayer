/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// InsStep structure
	/// </summary>
	internal class InsStep
	{
		public byte Note { get; set; }				// 8 bit note
		public bool Relative { get; set; }
		public byte WForm { get; set; }				// Max 15
	}
}

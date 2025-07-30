/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// Holds information about a single block line
	/// </summary>
	internal class BlockLine
	{
		public byte Instrument { get; set; }
		public byte Note { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }
	}
}

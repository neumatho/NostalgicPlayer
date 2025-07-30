/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Synthesis.Containers
{
	/// <summary>
	/// A single line in a track
	/// </summary>
	internal class TrackLine
	{
		public byte Note { get; set; }
		public byte Instrument { get; set; }
		public byte Arpeggio { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }
	}
}

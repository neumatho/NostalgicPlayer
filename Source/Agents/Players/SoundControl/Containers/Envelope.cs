/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds information about a single envelope
	/// </summary>
	internal class Envelope
	{
		public byte AttackSpeed { get; set; }
		public byte AttackIncrement { get; set; }
		public byte DecaySpeed { get; set; }
		public byte DecayDecrement { get; set; }
		public ushort DecayValue { get; set; }
		public byte ReleaseSpeed { get; set; }
		public byte ReleaseDecrement { get; set; }
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers
{
	/// <summary>
	/// A single track
	/// </summary>
	internal class TrackLine
	{
		public byte Sample { get; set; }
		public ushort Period { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }
	}
}

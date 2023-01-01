/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers
{
	/// <summary>
	/// TrackLine structure
	/// </summary>
	internal class TrackLine
	{
		public ushort Period;	// The note to play
		public byte Sample;		// The sample
		public Effect Effect;	// The effect to use
		public byte EffectArg;	// Effect argument
	}
}

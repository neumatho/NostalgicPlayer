/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// A single track
	/// </summary>
	internal class TrackLine
	{
		public byte Sample;		// The sample
		public sbyte Note;		// The note to play
		public Effect Effect;	// The effect to use
		public byte EffectArg;	// Effect argument
	}
}

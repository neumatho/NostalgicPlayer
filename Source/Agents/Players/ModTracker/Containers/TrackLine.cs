/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// TrackLine structure
	/// </summary>
	internal class TrackLine : IDeepCloneable<TrackLine>
	{
		public byte Note;		// The note to play
		public byte Sample;		// The sample
		public Effect Effect;	// The effect to use
		public byte EffectArg;	// Effect argument

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public TrackLine MakeDeepClone()
		{
			return (TrackLine)MemberwiseClone();
		}
	}
}

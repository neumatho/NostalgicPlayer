/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// TNE: C# does not support values as generic types, so
	/// this wrapper has been created to get around it
	/// </summary>
	internal class PlayBehaviourSet : EnumBitSet<PlayBehaviour>, IDeepCloneable<PlayBehaviourSet>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayBehaviourSet() : base(PlayBehaviour.MaxPlayBehaviours)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public override PlayBehaviourSet MakeDeepClone()
		{
			PlayBehaviourSet clone = new PlayBehaviourSet();

			CopyTo(clone);

			return clone;
		}
	}
}

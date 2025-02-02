/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds the state of a single LFO
	/// </summary>
	internal class LfoState : IDeepCloneable<LfoState>
	{
		public LfoTarget Target;
		public bool LoopModulation;

		public sbyte[] ShapeTable;
		public int ShapeTablePosition;
		public ushort ModulationSpeed;
		public ushort ModulationDepth;
		public short ModulationValue;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public LfoState MakeDeepClone()
		{
			return (LfoState)MemberwiseClone();
		}
	}
}

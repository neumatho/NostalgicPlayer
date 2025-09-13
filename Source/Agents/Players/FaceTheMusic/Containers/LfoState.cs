/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds the state of a single LFO
	/// </summary>
	internal class LfoState : IDeepCloneable<LfoState>
	{
		public LfoTarget Target { get; set; }
		public bool LoopModulation { get; set; }

		public sbyte[] ShapeTable { get; set; }
		public int ShapeTablePosition { get; set; }
		public ushort ModulationSpeed { get; set; }
		public ushort ModulationDepth { get; set; }
		public short ModulationValue { get; set; }

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

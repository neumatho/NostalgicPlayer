/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Pdb : IDeepCloneable<Pdb>
	{
		public uint PAddr;
		public byte PNum;
		public sbyte PxPose;
		public ushort PLoop;
		public ushort PStep;
		public byte PWait;
		public uint PrOAddr;
		public ushort PrOStep;
		public bool Looped;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Pdb MakeDeepClone()
		{
			return (Pdb)MemberwiseClone();
		}
	}
}

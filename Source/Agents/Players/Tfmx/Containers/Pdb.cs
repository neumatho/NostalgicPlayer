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
		public uint PAddr { get; set; }
		public byte PNum { get; set; }
		public sbyte PxPose { get; set; }
		public ushort PLoop { get; set; }
		public ushort PStep { get; set; }
		public byte PWait { get; set; }
		public uint PrOAddr { get; set; }
		public ushort PrOStep { get; set; }
		public bool Looped { get; set; }

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

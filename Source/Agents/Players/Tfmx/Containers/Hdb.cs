/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	internal delegate bool Loop(Hdb hw);

	/// <summary>
	/// </summary>
	internal class Hdb : IDeepCloneable<Hdb>
	{
		public uint Pos { get; set; }
		public uint Delta { get; set; }
		public ushort SLen { get; set; }
		public ushort SampleLength { get; set; }
		public int SBeg { get; set; }
		public int SampleStart { get; set; }
		public byte Vol { get; set; }
		public byte Mode { get; set; }
		public Loop Loop { get; set; }
		public Cdb C { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Hdb MakeDeepClone()
		{
			Hdb clone = (Hdb)MemberwiseClone();

			clone.C = C.MakeDeepClone();
			clone.C.Hw = clone;

			return clone;
		}
	}
}

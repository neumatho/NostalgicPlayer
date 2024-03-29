﻿/******************************************************************************/
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
		public uint Pos;
		public uint Delta;
		public ushort SLen;
		public ushort SampleLength;
		public int SBeg;
		public int SampleStart;
		public byte Vol;
		public byte Mode;
		public Loop Loop;
		public Cdb C;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Hdb MakeDeepClone()
		{
			Hdb clone = (Hdb)MemberwiseClone();

			clone.C = C.MakeDeepClone();
			clone.C.hw = clone;

			return clone;
		}
	}
}

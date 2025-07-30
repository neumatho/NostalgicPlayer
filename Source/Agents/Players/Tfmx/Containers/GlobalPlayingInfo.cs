/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public Hdb[] Hdb { get; set; }
		public Mdb Mdb { get; set; }
		public Cdb[] Cdb { get; set; }
		public PdBlk Pdb { get; set; }
		public Idb Idb { get; set; }

		public bool MultiMode { get; set; }

		public int Loops { get; set; }

		public uint EClocks { get; set; }
		public int ERem { get; set; }

		public int[] LastTrackPlayed { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.Hdb = ArrayHelper.CloneObjectArray(Hdb);
			clone.Mdb = Mdb.MakeDeepClone();
			clone.Cdb = ArrayHelper.CloneObjectArray(Cdb);
			clone.Pdb = Pdb.MakeDeepClone();
			clone.Idb = Idb.MakeDeepClone();
			clone.LastTrackPlayed = ArrayHelper.CloneArray(LastTrackPlayed);

			for (int i = clone.Hdb.Length - 1; i >= 0; i--)
				clone.Cdb[i].Hw = clone.Hdb[i];

			return clone;
		}
	}
}

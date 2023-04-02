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
		public Hdb[] Hdb;
		public Mdb Mdb;
		public Cdb[] Cdb;
		public PdBlk Pdb;
		public Idb Idb;

		public bool MultiMode;

		public int Loops;

		public uint EClocks;
		public int ERem;

		public int[] LastTrackPlayed;

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
				clone.Cdb[i].hw = clone.Hdb[i];

			return clone;
		}
	}
}

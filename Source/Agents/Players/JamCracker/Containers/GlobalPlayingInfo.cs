/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public NoteInfo[] Address { get; set; }

		public int AddressIndex { get; set; }

		public ushort TmpDmacon { get; set; }

		public ushort SongPos { get; set; }
		public ushort NoteCnt { get; set; }

		public byte Wait { get; set; }
		public byte WaitCnt { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo)MemberwiseClone();
		}
	}
}

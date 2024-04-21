/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Implementation;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		public GlobalPlayingInfo PlayingInfo;
		public HvlVoice[] Channels;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(GlobalPlayingInfo playingInfo, HvlVoice[] channels)
		{
			PlayingInfo = playingInfo.MakeDeepClone();
			Channels = ArrayHelper.CloneObjectArray(channels);
		}
	}
}

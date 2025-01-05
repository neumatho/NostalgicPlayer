/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		public GlobalPlayingInfo PlayingInfo;
		public VoiceInfo[] Voices;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(GlobalPlayingInfo playingInfo, VoiceInfo[] voices)
		{
			PlayingInfo = playingInfo.MakeDeepClone();
			Voices = ArrayHelper.CloneObjectArray(voices);
		}
	}
}

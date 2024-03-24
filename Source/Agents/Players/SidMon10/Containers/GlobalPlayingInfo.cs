/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public uint NumberOfRows;
		public uint Speed;
		public uint SpeedCounter;

		public bool NewTrack;
		public bool LoopSong;

		public int CurrentRow;
		public uint CurrentPosition;

		public MixPlayingInfo Mix1PlayingInfo = new MixPlayingInfo();
		public MixPlayingInfo Mix2PlayingInfo = new MixPlayingInfo();

		public VoiceInfo[] VoiceInfo;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.Mix1PlayingInfo = Mix1PlayingInfo.MakeDeepClone();
			clone.Mix2PlayingInfo = Mix2PlayingInfo.MakeDeepClone();

			clone.VoiceInfo = ArrayHelper.CloneObjectArray(VoiceInfo);

			return clone;
		}
	}
}

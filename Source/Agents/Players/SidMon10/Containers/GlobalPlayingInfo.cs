/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public uint NumberOfRows { get; set; }
		public uint Speed { get; set; }
		public uint SpeedCounter { get; set; }

		public bool NewTrack { get; set; }
		public bool LoopSong { get; set; }

		public int CurrentRow { get; set; }
		public uint CurrentPosition { get; set; }

		public MixPlayingInfo Mix1PlayingInfo { get; set; } = new MixPlayingInfo();
		public MixPlayingInfo Mix2PlayingInfo { get; set; } = new MixPlayingInfo();

		public VoiceInfo[] VoiceInfo { get; set; }

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

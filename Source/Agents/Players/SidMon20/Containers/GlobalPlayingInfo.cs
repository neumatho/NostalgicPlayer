/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public sbyte CurrentPosition { get; set; }
		public byte CurrentRow { get; set; }
		public byte PatternLength { get; set; }
		public byte CurrentRast { get; set; }
		public byte CurrentRast2 { get; set; }
		public byte Speed { get; set; }

		public VoiceInfo[] VoiceInfo { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.VoiceInfo = ArrayHelper.CloneObjectArray(VoiceInfo);

			return clone;
		}
	}
}

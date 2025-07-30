/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort Speed { get; set; }
		public ushort CurrentSpeed { get; set; }
		public ushort LastShownSpeed { get; set; }
		public bool NewPattern { get; set; }
		public bool NewRow { get; set; }
		public ushort CurrentPosition { get; set; }
		public ushort SongLength { get; set; }
		public ushort CurrentRow { get; set; }
		public ushort PatternLength { get; set; }

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

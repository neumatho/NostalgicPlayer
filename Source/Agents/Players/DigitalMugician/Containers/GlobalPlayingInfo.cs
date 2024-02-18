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
		public ushort Speed;
		public ushort CurrentSpeed;
		public ushort LastShownSpeed;
		public bool NewPattern;
		public bool NewInstrument;
		public ushort CurrentPosition;
		public ushort SongLength;
		public ushort CurrentRow;
		public ushort PatternLength;

		public VoiceInfo[] VoiceInfo;

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

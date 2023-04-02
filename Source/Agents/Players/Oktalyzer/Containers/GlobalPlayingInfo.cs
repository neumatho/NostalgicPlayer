/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort CurrentSpeed;
		public ushort SpeedCounter;
		public short SongPos;
		public short NewSongPos;
		public short PattPos;
		public bool FilterStatus;

		public PatternLine[] CurrLine;
		public ChannelInfo[] ChanInfo;
		public sbyte[] ChanVol;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.ChanInfo = ArrayHelper.CloneObjectArray(ChanInfo);
			clone.ChanVol = ArrayHelper.CloneArray(ChanVol);

			return clone;
		}
	}
}

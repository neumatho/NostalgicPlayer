/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public ushort CurrentSpeed { get; set; }
		public ushort SpeedCounter { get; set; }
		public short SongPos { get; set; }
		public short NewSongPos { get; set; }
		public short PattPos { get; set; }
		public bool FilterStatus { get; set; }

		public PatternLine[] CurrLine { get; set; }
		public ChannelInfo[] ChanInfo { get; set; }
		public sbyte[] ChanVol { get; set; }

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

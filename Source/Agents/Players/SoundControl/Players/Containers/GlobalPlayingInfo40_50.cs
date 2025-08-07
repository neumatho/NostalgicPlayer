/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Holds global information about the playing state.
	/// Only used by the SoundControl 4.0/5.0 player
	/// </summary>
	internal class GlobalPlayingInfo40_50 : IGlobalPlayingInfo
	{
		public ushort SongPosition { get; set; }
		public ushort Speed { get; set; }
		public ushort MaxSpeed { get; set; }
		public ushort SpeedCounter { get; set; }
		public ushort ChannelCounter { get; set; }      // Only used by 5.0 player

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IGlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo40_50)MemberwiseClone();
		}
	}
}

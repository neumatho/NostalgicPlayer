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
	/// Only used by the SoundControl 3.x player
	/// </summary>
	internal class GlobalPlayingInfo3x : IGlobalPlayingInfo
	{
		public ushort SongPosition { get; set; }
		public ushort SpeedCounter { get; set; }
		public ushort SpeedCounter2 { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IGlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo3x)MemberwiseClone();
		}
	}
}

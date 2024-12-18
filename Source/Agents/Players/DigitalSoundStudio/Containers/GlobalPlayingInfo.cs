/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public byte CurrentSongTempo;
		public byte CurrentSongSpeed;
		public byte SongSpeedCounter;

		public ushort CurrentPosition;
		public short CurrentRow;
		public bool PositionJump;
		public ushort NewPosition;

		public bool SetLoopRow;
		public short LoopRow;

		public ushort InverseMasterVolume;
		public ushort NextRetrigTickNumber;
		public ushort ArpeggioCounter;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo)MemberwiseClone();
		}
	}
}

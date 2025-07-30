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
		public byte CurrentSongTempo { get; set; }
		public byte CurrentSongSpeed { get; set; }
		public byte SongSpeedCounter { get; set; }

		public ushort CurrentPosition { get; set; }
		public short CurrentRow { get; set; }
		public bool PositionJump { get; set; }
		public ushort NewPosition { get; set; }

		public bool SetLoopRow { get; set; }
		public short LoopRow { get; set; }

		public ushort InverseMasterVolume { get; set; }
		public ushort NextRetrigTickNumber { get; set; }
		public ushort ArpeggioCounter { get; set; }

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

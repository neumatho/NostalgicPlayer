/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns.Containers
{
	/// <summary>
	/// Contains all global player info that changes
	/// </summary>
	internal class PlayerInfo : IDeepCloneable<PlayerInfo>
	{
		public Track[] Track;

		public
		(
			udword Checksum,			// The main one we use to identify the module

			sword Speed,				// Speed and speed count
			sword Count,
			c_int StartSong,
			c_int Songs,

			bool Initialized,			// true => restartable
			bool Looped,				// whether sequencer has processed last track

			ubyte NextVoiceNum
		) Admin;

		public
		(
			ubyte Tracks,
			ubyte Tables,
			ubyte TableSize,

			// Track progression. Same for all tracks
			(
				uword First,
				uword Last,
				uword Current,
				uword Loop,
				udword CurrentOffset,
				ubyte Size,
				bool Next				// Whether a pattern triggered next track step
			) Step,

			// Pattern progression. Same for all tracks
			(
				uword Pos,
				uword Length
			) Pattern
		) Sequencer;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public PlayerInfo MakeDeepClone()
		{
			PlayerInfo clone = (PlayerInfo)MemberwiseClone();

			clone.Track = ArrayHelper.CloneObjectArray(Track);

			return clone;
		}
	}
}

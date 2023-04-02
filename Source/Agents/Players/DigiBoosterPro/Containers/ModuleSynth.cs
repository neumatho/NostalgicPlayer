/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModuleSynth : IDeepCloneable<ModuleSynth>
	{
		public DB3Module Module;				// The module played
		public ModuleTrack[] Tracks;			// Table of tracks
		public ModuleMode Mode;					// Sequencer mode (row/pattern/song/song_once)
		public int Pattern;						// Current pattern
		public int Song;						// Current song
		public int Row;							// Current row (position) in a pattern
		public int Order;						// Current order number in a song
		public int Tick;						// Current tick of a position
		public int Speed;						// Ticks per position
		public int Tempo;						// Beats per minute
		public int PatternDelay;				// (EEx) Pattern delay in positions, also used for sequencer stopping
		public bool DelayModuleEnd;				// Module ending (because of end of play list or F00 command) is also delayed
		public int DelayPatternBreak;			// (Dxx) Delayed pattern break position (position num)
		public int DelayPatternJump;			// (Bxx) Delayed pattern jump (pattern num)
		public int DelayLoop;					// (E6x) Track number containing the loop information if there is a loop to start, else -1
		public int16_t GlobalVolume;			// (Gxx, Hxx)
		public int16_t GlobalVolumeSlide;		// (Hxx)
		public uint8_t OldGlobalVolumeSlide;	// (Hxx)

		public int16_t MinVolume;				// Current sliding limits prescaled with speed
		public int16_t MaxVolume;
		public int16_t MinPanning;
		public int16_t MaxPanning;
		public int16_t MinPitch;
		public int16_t MaxPitch;

		public uint8_t ArpCounter;				// Arpeggio counter

		public bool ManualUpdate;				// Send (one) tracker position update being in HALTED mode

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModuleSynth MakeDeepClone()
		{
			ModuleSynth clone = (ModuleSynth)MemberwiseClone();

			clone.Tracks = ArrayHelper.CloneObjectArray(Tracks);

			return clone;
		}
	}
}

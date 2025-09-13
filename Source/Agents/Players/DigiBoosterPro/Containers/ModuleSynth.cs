/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModuleSynth : IDeepCloneable<ModuleSynth>
	{
		public DB3Module Module { get; set; }			// The module played
		public ModuleTrack[] Tracks { get; set; }		// Table of tracks
		public ModuleMode Mode { get; set; }			// Sequencer mode (row/pattern/song/song_once)
		public int Pattern { get; set; }				// Current pattern
		public int Song { get; set; }					// Current song
		public int Row { get; set; }					// Current row (position) in a pattern
		public int Order { get; set; }					// Current order number in a song
		public int Tick { get; set; }					// Current tick of a position
		public int Speed { get; set; }					// Ticks per position
		public int Tempo { get; set; }					// Beats per minute
		public int PatternDelay { get; set; }			// (EEx) Pattern delay in positions, also used for sequencer stopping
		public bool DelayModuleEnd { get; set; }		// Module ending (because of end of play list or F00 command) is also delayed
		public int DelayPatternBreak { get; set; }		// (Dxx) Delayed pattern break position (position num)
		public int DelayPatternJump { get; set; }		// (Bxx) Delayed pattern jump (pattern num)
		public int DelayLoop { get; set; }				// (E6x) Track number containing the loop information if there is a loop to start, else -1
		public int16_t GlobalVolume { get; set; }		// (Gxx, Hxx)
		public int16_t GlobalVolumeSlide { get; set; }	// (Hxx)
		public uint8_t OldGlobalVolumeSlide { get; set; }// (Hxx)

		public int16_t MinVolume { get; set; }			// Current sliding limits prescaled with speed
		public int16_t MaxVolume { get; set; }
		public int16_t MinPanning { get; set; }
		public int16_t MaxPanning { get; set; }
		public int16_t MinPitch { get; set; }
		public int16_t MaxPitch { get; set; }

		public uint8_t ArpCounter { get; set; }			// Arpeggio counter

		public bool ManualUpdate { get; set; }			// Send (one) tracker position update being in HALTED mode

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

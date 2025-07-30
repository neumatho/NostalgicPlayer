/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModuleTrack : IDeepCloneable<ModuleTrack>
	{
		public int16_t TrackNumber { get; set; }

		public int Instrument { get; set; }				// A currently set instrument number (from 1!)
		public bool IsOn { get; set; }					// True if channel active

		public int32_t Volume { get; set; }				// Speed prescaled, <0, 64>
		public int32_t Panning { get; set; }			// Speed prescaled, <-128, +128>
		public int32_t Note { get; set; }				// Current note (only used in version 2 modules)
		public int32_t Pitch { get; set; }				// Speed prescaled, <96, 768>
		public int16_t VolumeDelta { get; set; }		// Speed prescaled, <-30, +30>
		public int16_t PanningDelta { get; set; }		// Speed prescaled, <-30, +30>
		public int16_t PitchDelta { get; set; }			// Speed prescaled, <-30, +30>
		public int16_t Porta3Delta { get; set; }		// Speed prescaled
		public int16_t[] ArpTable { get; set; } = new int16_t[3];	// Arpeggio, speed prescaled
		public int16_t Porta3Target { get; set; }		// This is *not* speed prescaled, <96, 767>
		public int16_t VibratoSpeed { get; set; }
		public int16_t VibratoDepth { get; set; }
		public int16_t VibratoCounter { get; set; }

		public EnvelopeInterpolator VolumeEnvelope { get; set; } = new EnvelopeInterpolator();	// Volume envelope interpolator data
		public EnvelopeInterpolator PanningEnvelope { get; set; } = new EnvelopeInterpolator();	// Panning envelope interpolator data
		public int16_t VolumeEnvelopeCurrent { get; set; }	// Current volume envelope value
		public int16_t PanningEnvelopeCurrent { get; set; }	// Current panning envelope value

		public int32_t TriggerCounter { get; set; }		// Triggers instrument when counts down to 0
		public int32_t CutCounter { get; set; }			// Switches track off at 0, inhibit triggers then
		public int32_t Retrigger { get; set; }			// Retrigger period in ticks (0 for no retrigger)
		public int32_t TriggerOffset { get; set; }		// Apply at next trigger
		public bool PlayBackwards { get; set; }			// E3x command handling
		public OldValues Old { get; set; } = new OldValues();// Old values for parameter reuse

		public int LoopCounter { get; set; }			// (E6x) Loop counter
		public int LoopOrder { get; set; }				// (E6x) Loop order number in a song
		public int LoopRow { get; set; }				// (E6x) Loop row

		public int EchoDelay { get; set; }				// 0 to 255, 2 ms (tracker units)
		public int EchoFeedback { get; set; }			// 0 to 255
		public int EchoMix { get; set; }				// 0 dry, 255 wet
		public int EchoCross { get; set; }				// 0 to 255

		// Here are information which is given to NostalgicPlayer to play the sample
		public Array SampleData { get; set; }
		public uint SampleLength { get; set; }
		public byte SampleBitSize { get; set; }
		public uint SampleLoopStartOffset { get; set; }
		public uint SampleLoopLength { get; set; }
		public ChannelLoopType SampleLoopType { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModuleTrack MakeDeepClone()
		{
			ModuleTrack clone = (ModuleTrack)MemberwiseClone();

			clone.ArpTable = ArrayHelper.CloneArray(ArpTable);
			clone.VolumeEnvelope = VolumeEnvelope.MakeDeepClone();
			clone.PanningEnvelope = PanningEnvelope.MakeDeepClone();
			clone.Old = Old.MakeDeepClone();

			return clone;
		}
	}
}

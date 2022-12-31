/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModuleTrack
	{
		public IChannel Channel;
		public int16_t TrackNumber;

		public int Instrument;					// A currently set instrument number (from 1!)
		public bool IsOn;						// True if channel active

		public int32_t Volume;					// Speed prescaled, <0, 64>
		public int32_t Panning;					// Speed prescaled, <-128, +128>
		public int32_t Note;					// Current note (only used in version 2 modules)
		public int32_t Pitch;					// Speed prescaled, <96, 768>
		public int16_t VolumeDelta;				// Speed prescaled, <-30, +30>
		public int16_t PanningDelta;			// Speed prescaled, <-30, +30>
		public int16_t PitchDelta;				// Speed prescaled, <-30, +30>
		public int16_t Porta3Delta;				// Speed prescaled
		public int16_t[] ArpTable = new int16_t[3];	// Arpeggio, speed prescaled
		public int16_t Porta3Target;			// This is *not* speed prescaled, <96, 767>
		public int16_t VibratoSpeed;
		public int16_t VibratoDepth;
		public int16_t VibratoCounter;

		public EnvelopeInterpolator VolumeEnvelope = new EnvelopeInterpolator();	// Volume envelope interpolator data
		public EnvelopeInterpolator PanningEnvelope = new EnvelopeInterpolator();	// Panning envelope interpolator data
		public int16_t VolumeEnvelopeCurrent;	// Current volume envelope value
		public int16_t PanningEnvelopeCurrent;	// Current panning envelope value

		public int32_t TriggerCounter;			// Triggers instrument when counts down to 0
		public int32_t CutCounter;				// Switches track off at 0, inhibit triggers then
		public int32_t Retrigger;				// Retrigger period in ticks (0 for no retrigger)
		public int32_t TriggerOffset;			// Apply at next trigger
		public bool PlayBackwards;				// E3x command handling
		public OldValues Old;					// Old values for parameter reuse

		public int LoopCounter;					// (E6x) Loop counter
		public int LoopOrder;					// (E6x) Loop order number in a song
		public int LoopRow;						// (E6x) Loop row

		public int EchoDelay;					// 0 to 255, 2 ms (tracker units)
		public int EchoFeedback;				// 0 to 255
		public int EchoMix;						// 0 dry, 255 wet
		public int EchoCross;					// 0 to 255

		// Here are information which is given to NostalgicPlayer to play the sample
		public Array SampleData;
		public uint SampleLength;
		public byte SampleBitSize;
		public uint SampleLoopStartOffset;
		public uint SampleLoopLength;
		public ChannelLoopType SampleLoopType;
	}
}

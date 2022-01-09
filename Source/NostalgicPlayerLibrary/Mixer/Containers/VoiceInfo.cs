/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers
{
	/// <summary>
	/// Mixer information for a single voice
	/// </summary>
	internal struct VoiceInfo
	{
		public bool Enabled;				// True -> the channel is enabled
		public bool Kick;					// True -> sample has to be restarted
		public bool Active;					// True -> sample is playing
		public SampleFlag Flags;			// 16/8 bits, looping/one-shot etc.
		public Array Address;				// Address to the sample
		public uint Start;					// Start index
		public uint Size;					// Sample size
		public uint RepeatPosition;			// Loop start
		public uint RepeatEnd;				// Loop end
		public Array NewLoopAddress;		// Address to loop point when loop is restarted (mostly the same as Address above)
		public uint NewRepeatPosition;		// New loop start when loop is restarted
		public uint NewRepeatEnd;			// New loop end when loop is restarted
		public uint ReleaseEnd;				// Release end
		public uint Frequency;				// Current frequency
		public int Volume;					// Current volume
		public int Panning;					// Current panning position
		public int RampVolume;
		public int LeftVolumeSelected;		// Volume factor in range 0-255
		public int RightVolumeSelected;
		public int OldLeftVolume;
		public int OldRightVolume;
		public long Current;				// Current index in the sample
		public long Increment;				// Increment value
	}
}

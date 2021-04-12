/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
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
		public sbyte[] Address;				// Address to the sample
		public sbyte[] LoopAddress;			// Address to the loop point (mostly the same as Address above)
		public uint Start;					// Start index
		public uint Size;					// Sample size
		public uint RepeatPosition;			// Loop start
		public uint RepeatEnd;				// Loop end
		public uint ReleaseLength;			// Release length
		public uint Frequency;				// Current frequency
		public int LeftVolume;				// Current volume in left speaker
		public int RightVolume;				// Current volume in right speaker
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

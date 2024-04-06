/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
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
		public Array[] Addresses;			// Addresses to the sample (left/right speaker)
		public uint Start;					// Start index
		public uint Size;					// Sample size
		public uint RepeatPosition;			// Loop start
		public uint RepeatEnd;				// Loop end
		public Array[] NewLoopAddresses;	// Addresses to loop point when loop is restarted (mostly the same as Addresses above)
		public uint NewRepeatPosition;		// New loop start when loop is restarted
		public uint NewRepeatEnd;			// New loop end when loop is restarted
		public int NewPosition;				// New position to use immediately
		public bool RelativePosition;		// Indicate if the new position is relative or absolute
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

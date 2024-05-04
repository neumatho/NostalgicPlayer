/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
{
	/// <summary>
	/// Mixer information for a single voice
	/// </summary>
	internal class VoiceInfo
	{
		public bool Enabled;					// True -> the channel is enabled
		public bool Kick;						// True -> sample has to be restarted
		public bool Active;						// True -> sample is playing
		public VoiceFlag Flags;					// Special flags for the voice, not the sample
		public VoiceSampleInfo SampleInfo;		// Information for the current playing sample
		public VoiceSampleInfo NewSampleInfo;	// Information for a new sample to use when loop is restarted
		public int NewPosition;					// New position to use immediately
		public bool RelativePosition;			// Indicate if the new position is relative or absolute
		public uint ReleaseEnd;					// Release end
		public uint Frequency;					// Current frequency
		public int Volume;						// Current volume
		public int Panning;						// Current panning position
		public int RampVolume;
		public int LeftVolumeSelected;			// Volume factor in range 0-255
		public int RightVolumeSelected;
		public int OldLeftVolume;
		public int OldRightVolume;
		public long Current;					// Current index in the sample
		public long Increment;					// Increment value
	}
}

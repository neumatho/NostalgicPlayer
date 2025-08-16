/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers
{
	/// <summary>
	/// Mixer information for a single voice
	/// </summary>
	internal class VoiceInfo
	{
		public bool Enabled { get; set; }					// True -> the channel is enabled
		public bool Kick { get; set; }						// True -> sample has to be restarted
		public bool Active { get; set; }					// True -> sample is playing
		public VoiceFlag Flags { get; set; }				// Special flags for the voice, not the sample
		public VoiceSampleInfo SampleInfo { get; set; }		// Information for the current playing sample
		public VoiceSampleInfo NewSampleInfo { get; set; }	// Information for a new sample to use when loop is restarted
		public int NewPosition { get; set; }				// New position to use immediately
		public bool RelativePosition { get; set; }			// Indicate if the new position is relative or absolute
		public uint Frequency { get; set; }					// Current frequency
		public int Volume { get; set; }						// Current volume
		public int Panning { get; set; }					// Current panning position
		public int RampVolume { get; set; }
		public int[] PanningVolume { get; set; }			// Volume factor in range 0-255
		public int[] OldPanningVolume { get; set; }
		public long Current { get; set; }					// Current index in the sample
		public long Increment { get; set; }					// Increment value
	}
}

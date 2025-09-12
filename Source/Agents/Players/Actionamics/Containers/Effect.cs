/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// All track effects
	/// </summary>
	internal enum Effect : ushort
	{
		None = 0x00,
		Arpeggio = 0x70,
		SlideUp = 0x71,
		SlideDown = 0x72,
		VolumeSlideAfterEnvelope = 0x73,
		Vibrato = 0x74,
		SetRows = 0x75,
		SetSampleOffset = 0x76,
		NoteDelay = 0x77,
		Mute = 0x78,
		SampleRestart = 0x79,
		Tremolo = 0x7a,
		Break = 0x7b,
		SetVolume = 0x7c,
		VolumeSlide = 0x7d,
		VolumeSlideAndVibrato = 0x7e,
		SetSpeed = 0x7f
	}
}

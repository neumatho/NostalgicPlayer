/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public byte SampleNumber;

		public byte Attack;
		public byte Decay_Sustain;
		public byte Release;

		public byte VibratoDelay;
		public byte VibratoSpeed;
		public byte VibratoLevel;

		public byte Arpeggio;
		public byte FxArp_SpdLp;

		public byte Hold;

		public byte Key_WaveRate;
		public byte WaveLevel_Speed;
	}
}

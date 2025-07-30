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
		public byte SampleNumber { get; set; }

		public byte Attack { get; set; }
		public byte Decay_Sustain { get; set; }
		public byte Release { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoLevel { get; set; }

		public byte Arpeggio { get; set; }
		public byte FxArp_SpdLp { get; set; }

		public byte Hold { get; set; }

		public byte Key_WaveRate { get; set; }
		public byte WaveLevel_Speed { get; set; }
	}
}

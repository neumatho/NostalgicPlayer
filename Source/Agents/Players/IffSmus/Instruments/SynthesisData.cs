/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Holds data for synthesis format
	/// </summary>
	internal class SynthesisData
	{
		public sbyte[] Oscillator { get; } = new sbyte[128];
		public sbyte[] Lfo { get; } = new sbyte[256];

		public WaveformType Waveform { get; set; }
		public ushort WaveAmt { get; set; }

		public ushort AmplitudeVolume { get; set; }
		public ushort AmplitudeEnabled { get; set; }
		public ushort AmplitudeLfo { get; set; }

		public ushort FrequencyPort { get; set; }
		public ushort FrequencyLfo { get; set; }

		public ushort FilterFrequency { get; set; }
		public ushort FilterEg { get; set; }
		public ushort FilterLfo { get; set; }

		public ushort LfoSpeed { get; set; }
		public LfoStatus LfoEnabled { get; set; }
		public ushort LfoDelay { get; set; }

		public ushort PhaseSpeed { get; set; }
		public ushort PhaseDepth { get; set; }

		public ushort[] EnvelopeLevels { get; } = new ushort[4];
		public ushort[] EnvelopeRates { get; } = new ushort[4];

		public sbyte[][] Samples { get; set; }
	}
}

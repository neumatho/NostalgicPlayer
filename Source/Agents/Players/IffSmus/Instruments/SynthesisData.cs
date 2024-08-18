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
		public sbyte[] Oscillator = new sbyte[128];
		public sbyte[] Lfo = new sbyte[256];

		public WaveformType Waveform;
		public ushort WaveAmt;

		public ushort AmplitudeVolume;
		public ushort AmplitudeEnabled;
		public ushort AmplitudeLfo;

		public ushort FrequencyPort;
		public ushort FrequencyLfo;

		public ushort FilterFrequency;
		public ushort FilterEg;
		public ushort FilterLfo;

		public ushort LfoSpeed;
		public LfoStatus LfoEnabled;
		public ushort LfoDelay;

		public ushort PhaseSpeed;
		public ushort PhaseDepth;

		public ushort[] EnvelopeLevels = new ushort[4];
		public ushort[] EnvelopeRates = new ushort[4];

		public sbyte[][] Samples;
	}
}

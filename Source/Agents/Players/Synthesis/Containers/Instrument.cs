/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Synthesis.Containers
{
	/// <summary>
	/// Instrument information
	/// </summary>
	internal class Instrument
	{
		public byte WaveformNumber { get; set; }			// For normal samples, this is the sample number
		public bool SynthesisEnabled { get; set; }
		public ushort WaveformLength { get; set; }			// For normal samples, this is the length before the loop (one shot)
		public ushort RepeatLength { get; set; }
		public byte Volume { get; set; }
		public sbyte PortamentoSpeed { get; set; }
		public bool AdsrEnabled { get; set; }
		public byte AdsrTableNumber { get; set; }
		public ushort AdsrTableLength { get; set; }
		public byte ArpeggioStart { get; set; }
		public byte ArpeggioLength { get; set; }
		public byte ArpeggioRepeatLength { get; set; }
		public SynthesisEffect Effect { get; set; }
		public byte EffectArg1 { get; set; }
		public byte EffectArg2 { get; set; }
		public byte EffectArg3 { get; set; }
		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoLevel { get; set; }
		public byte EnvelopeGeneratorCounterOffset { get; set; }
		public EnvelopeGeneratorCounterMode EnvelopeGeneratorCounterMode { get; set; }
		public byte EnvelopeGeneratorCounterTableNumber { get; set; }
		public ushort EnvelopeGeneratorCounterTableLength { get; set; }
	}
}

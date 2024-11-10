/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers
{
	/// <summary>
	/// Instrument information
	/// </summary>
	internal class Instrument
	{
		public byte WaveformNumber;					// For normal samples, this is the sample number
		public bool SynthesisEnabled;
		public ushort WaveformLength;				// For normal samples, this is the length before the loop (one shot)
		public ushort RepeatLength;
		public byte Volume;
		public sbyte PortamentoSpeed;
		public bool AdsrEnabled;
		public byte AdsrTableNumber;
		public ushort AdsrTableLength;
		public bool PortamentoEnabled;
		public byte VibratoDelay;
		public byte VibratoSpeed;
		public byte VibratoLevel;
		public byte EnvelopeGeneratorCounterOffset;
		public EnvelopeGeneratorCounterMode EnvelopeGeneratorCounterMode;
		public byte EnvelopeGeneratorCounterTableNumber;
		public ushort EnvelopeGeneratorCounterTableLength;
	}
}

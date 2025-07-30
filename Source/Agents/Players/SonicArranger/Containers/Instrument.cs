/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers
{
	/// <summary>
	/// Instrument information
	/// </summary>
	internal class Instrument
	{
		public string Name { get; set; }

		public InstrumentType Type { get; set; }
		public ushort WaveformNumber { get; set; }				// For normal samples, this is the sample number
		public ushort WaveformLength { get; set; }				// For normal samples, this is the length before the loop (one shot)
		public ushort RepeatLength { get; set; }

		public ushort Volume { get; set; }
		public short FineTuning { get; set; }

		public ushort PortamentoSpeed { get; set; }

		public ushort VibratoDelay { get; set; }
		public ushort VibratoSpeed { get; set; }
		public ushort VibratoLevel { get; set; }

		public ushort AmfNumber { get; set; }
		public ushort AmfDelay { get; set; }
		public ushort AmfLength { get; set; }
		public ushort AmfRepeat { get; set; }

		public ushort AdsrNumber { get; set; }
		public ushort AdsrDelay { get; set; }
		public ushort AdsrLength { get; set; }
		public ushort AdsrRepeat { get; set; }
		public ushort SustainPoint { get; set; }
		public ushort SustainDelay { get; set; }

		public SynthesisEffect Effect { get; set; }
		public ushort EffectArg1 { get; set; }
		public ushort EffectArg2 { get; set; }
		public ushort EffectArg3 { get; set; }
		public ushort EffectDelay { get; set; }

		public Arpeggio[] Arpeggios { get; } = ArrayHelper.InitializeArray<Arpeggio>(3);
	}
}

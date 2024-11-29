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
		public string Name;

		public InstrumentType Type;
		public ushort WaveformNumber;				// For normal samples, this is the sample number
		public ushort WaveformLength;				// For normal samples, this is the length before the loop (one shot)
		public ushort RepeatLength;

		public ushort Volume;
		public short FineTuning;

		public ushort PortamentoSpeed;

		public ushort VibratoDelay;
		public ushort VibratoSpeed;
		public ushort VibratoLevel;

		public ushort AmfNumber;
		public ushort AmfDelay;
		public ushort AmfLength;
		public ushort AmfRepeat;

		public ushort AdsrNumber;
		public ushort AdsrDelay;
		public ushort AdsrLength;
		public ushort AdsrRepeat;
		public ushort SustainPoint;
		public ushort SustainDelay;

		public SynthesisEffect Effect;
		public ushort EffectArg1;
		public ushort EffectArg2;
		public ushort EffectArg3;
		public ushort EffectDelay;

		public Arpeggio[] Arpeggios = ArrayHelper.InitializeArray<Arpeggio>(3);
	}
}

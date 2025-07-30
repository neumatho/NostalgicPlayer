/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// Instrument information
	/// </summary>
	internal class Instrument
	{
		public string Name { get; set; }
		public ushort WaveformLength { get; set; }
		public byte Volume { get; set; }
		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoLevel { get; set; }
		public byte PortamentoSpeed { get; set; }
		public byte AdsrLength { get; set; }
		public byte AdsrRepeat { get; set; }
		public byte SustainPoint { get; set; }
		public byte SustainSpeed { get; set; }
		public byte AmfLength { get; set; }
		public byte AmfRepeat { get; set; }
		public EnvelopeGeneratorMode EnvelopeGeneratorMode { get; set; }
		public byte StartLen { get; set; }
		public byte StopRep { get; set; }
		public byte SpeedUp { get; set; }
		public byte SpeedDown { get; set; }
		public byte[] AdsrTable { get; } = new byte[128];
		public sbyte[] LfoTable { get; } = new sbyte[128];
		public Arpeggio[] Arpeggios { get; } = ArrayHelper.InitializeArray<Arpeggio>(3);
		public byte[] EnvelopeGeneratorTable { get; } = new byte[128];
		public sbyte[] Waveform1 { get; } = new sbyte[256];
		public sbyte[] Waveform2 { get; } = new sbyte[256];
	}
}

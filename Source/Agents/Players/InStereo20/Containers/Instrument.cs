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
		public string Name;
		public ushort WaveformLength;
		public byte Volume;
		public byte VibratoDelay;
		public byte VibratoSpeed;
		public byte VibratoLevel;
		public byte PortamentoSpeed;
		public byte AdsrLength;
		public byte AdsrRepeat;
		public byte SustainPoint;
		public byte SustainSpeed;
		public byte AmfLength;
		public byte AmfRepeat;
		public EnvelopeGeneratorMode EnvelopeGeneratorMode;
		public byte StartLen;
		public byte StopRep;
		public byte SpeedUp;
		public byte SpeedDown;
		public byte[] AdsrTable = new byte[128];
		public sbyte[] LfoTable = new sbyte[128];
		public Arpeggio[] Arpeggios = ArrayHelper.InitializeArray<Arpeggio>(3);
		public byte[] EnvelopeGeneratorTable = new byte[128];
		public sbyte[] Waveform1 = new sbyte[256];
		public sbyte[] Waveform2 = new sbyte[256];
	}
}

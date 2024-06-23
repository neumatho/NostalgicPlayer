/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds a single instrument
	/// </summary>
	internal class Instrument
	{
		public byte[] InstrumentName;
		public byte[] FileName;

		public InstrumentFlag Flags;
		public ushort[] SampleMap = new ushort[10 * 12 * 2];

		public Envelope VolEnv = new Envelope();
		public Envelope PanEnv = new Envelope();
		public Envelope PitchEnv = new Envelope();

		public XmVibratoSettings Vibrato = new XmVibratoSettings();

		public ushort FadeOut;
		public byte MidiChannel;
		public byte MidiBank;
		public byte MidiPatch;
		public byte MidiBend;
		public byte GlobalVol;				// 0...128
		public ushort Panning;				// 0...256 if enabled, 0xffff otherwise
		public byte Nna;
		public byte Pps;
		public byte Ppc;
		public byte Dct;
		public byte Dca;
		public ushort VolSwing;				// 0...100
		public ushort PanSwing;				// 0...256
		public byte CutOff;					// 0...127, + 128 if enabled
		public byte Resonance;				// 0...127, + 128 if enabled
	}
}

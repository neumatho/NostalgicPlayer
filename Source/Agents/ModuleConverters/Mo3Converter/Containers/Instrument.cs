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
		public byte[] InstrumentName { get; set; }
		public byte[] FileName { get; set; }

		public InstrumentFlag Flags { get; set; }
		public ushort[] SampleMap { get; } = new ushort[10 * 12 * 2];

		public Envelope VolEnv { get; } = new Envelope();
		public Envelope PanEnv { get; } = new Envelope();
		public Envelope PitchEnv { get; } = new Envelope();

		public XmVibratoSettings Vibrato { get; } = new XmVibratoSettings();

		public ushort FadeOut { get; set; }
		public byte MidiChannel { get; set; }
		public byte MidiBank { get; set; }
		public byte MidiPatch { get; set; }
		public byte MidiBend { get; set; }
		public byte GlobalVol { get; set; }				// 0...128
		public ushort Panning { get; set; }				// 0...256 if enabled, 0xffff otherwise
		public byte Nna { get; set; }
		public byte Pps { get; set; }
		public byte Ppc { get; set; }
		public byte Dct { get; set; }
		public byte Dca { get; set; }
		public ushort VolSwing { get; set; }			// 0...100
		public ushort PanSwing { get; set; }			// 0...256
		public byte CutOff { get; set; }				// 0...127, + 128 if enabled
		public byte Resonance { get; set; }				// 0...127, + 128 if enabled
	}
}

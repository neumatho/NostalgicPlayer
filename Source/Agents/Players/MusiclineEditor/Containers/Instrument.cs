/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Hold information about a single instrument
	/// </summary>
	internal class Instrument : InstrumentSampleBase
	{
		/// <summary>
		/// 
		/// </summary>
		public byte SampleNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleRepeatStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort SampleRepeatLen { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort Volume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Transposable { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte SlideSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public InstrumentEffect1 Effect1 { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public InstrumentEffect2 Effect2 { get; set; }

		// Envelope

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeAttackLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeDecayLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeSustainLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeReleaseLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeAttackSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeDecaySpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeSustainSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeReleaseSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeAttackVolume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeDecayVolume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeSustainVolume { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort EnvelopeReleaseVolume { get; set; }

		// Vibrato

		/// <summary>
		/// 
		/// </summary>
		public Direction VibratoDirection { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public WaveType VibratoWaveType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort VibratoSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort VibratoDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort VibratoAttackSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort VibratoAttack { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort VibratoDepth { get; set; }

		// Tremolo

		/// <summary>
		/// 
		/// </summary>
		public Direction TremoloDirection { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public WaveType TremoloWaveType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TremoloSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TremoloDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TremoloAttackSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TremoloAttack { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TremoloDepth { get; set; }

		// Arpeggio

		/// <summary>
		/// 
		/// </summary>
		public ushort ArpeggioTable { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte ArpeggioSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte ArpeggioGroove { get; set; }

		// Transform

		/// <summary>
		/// 
		/// </summary>
		public InstrumentFlag EnvTraPhaFilBits { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte[] TransformWaveNumbers { get; } = new byte[5];

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformTurns { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort TransformDelay { get; set; }

		// Phase

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseTurns { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort PhaseDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public PhaseType PhaseType { get; set; }

		// Mix

		/// <summary>
		/// 
		/// </summary>
		public MixFlag MixResLooBits { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte MixWaveNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixTurns { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort MixDelay { get; set; }

		// Resonance

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceTurns { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort ResonanceDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public BoostFlag MixResFilBoost { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte ResponanceAmp { get; set; }

		// Filter

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterSpeed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterTurns { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort FilterDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public FilterType FilterType { get; set; }

		// Play loop

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopStart { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopRepeat { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopRepeatEnd { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopLoopStep { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopWait { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopDelay { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ushort LoopTurns { get; set; }
	}
}

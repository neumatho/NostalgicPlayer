/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }
		public bool VoiceOff { get; set; }
		public byte Speed { get; set; }
		public byte Groove { get; set; }
		public byte SpeedPart { get; set; }
		public byte GroovePart { get; set; }
		public byte TuneSpeed { get; set; }
		public byte TuneGroove { get; set; }
		public byte SpeedCounter { get; set; }
		public byte ArpeggioSpeedCounter { get; set; }
		public bool PartGroove { get; set; }
		public bool ArpeggioGroove { get; set; }
		public byte WaveSampleNumber { get; set; }
		public byte WaveSampleNumberOld { get; set; }
		public Instrument Instrument { get; set; }
		public Sample WaveSample { get; set; }
		public byte PartNote { get; set; }
		public byte PartInstrument { get; set; }
		public PartEffectEntry[] PartEffects { get; private set; } = ArrayHelper.InitializeArray<PartEffectEntry>(5);
		public ArpeggioFlag Arpeggio { get; set; }
		public byte ArpeggioPosition { get; set; }
		public byte ArpeggioTable { get; set; }
		public bool ArpeggioWait { get; set; }
		public byte ArpeggioNote { get; set; }
		public ArpeggioEffect ArpeggioVolumeSlide { get; set; }
		public ArpeggioEffect ArpeggioPitchSlide { get; set; }
		public Direction ArpeggioPitchSlideType { get; set; }
		public ushort ArpeggioCalculatedNote { get; set; }

		public byte TunePosition { get; set; }
		public byte PartPosition { get; set; }
		public byte PartPositionWork { get; set; }
		public byte TuneJumpCounter { get; set; }
		public short TuneJumpPosition { get; set; }
		public byte PartJumpCounter { get; set; }

		public SamplePointer? WaveSampleRepeatPointerOriginal { get; set; }
		public SamplePointer? WaveSamplePointer { get; set; }
		public ushort WaveSampleLength { get; set; }
		public SamplePointer? WaveSampleRepeatPointer { get; set; }
		public ushort WaveSampleRepeatLength { get; set;}

		public ushort Volume1 { get; set; }
		public ushort Volume2 { get; set; }
		public ushort Volume3 { get; set; }

		public ushort Note { get; set; }
		public ushort Period1 { get; set; }
		public ushort Period2 { get; set; }

		public short Transpose { get; set; }
		public short SemiTone { get; set; }
		public short FineTune { get; set; }

		public bool SampleOffsetActive { get; set; }
		public byte SampleOffset { get; set; }
		public byte OldInstrument { get; set; }
		public RestartFlag Restart { get; set; }

		public PartEffect VolumeAdd { get; set; }
		public PartEffect VolumeSlide { get; set; }
		public PartEffect ChannelVolumeSlide { get; set; }
		public PartEffect MasterVolumeSlide { get; set; }
		public ushort VolumeSet { get; set; }
		public ushort ChannelVolume { get; set; }
		public short VolumeAddNumber { get; set; }
		public short ChannelVolumeAddNumber { get; set; }
		public short MasterVolumeAddNumber { get; set; }
		public ushort VolumeSlideSpeed { get; set; }
		public ushort ChannelVolumeSlideSpeed { get; set; }
		public ushort MasterVolumeSlideSpeed { get; set; }

		public ushort VolumeSlideVolume { get; set; }
		public ushort ChannelVolumeSlideVolume { get; set; }
		public ushort MasterVolumeSlideVolume { get; set; }
		public ushort VolumeSlideToVolume { get; set; }
		public ushort ChannelVolumeSlideToVolume { get; set; }
		public ushort MasterVolumeSlideToVolume { get; set; }
		public Direction VolumeSlideType { get; set; }
		public Direction ChannelVolumeSlideType { get; set; }
		public Direction MasterVolumeSlideType { get; set; }
		public bool VolumeSlideToVolumeOff { get; set; }
		public bool ChannelVolumeSlideToVolumeOff { get; set; }
		public bool MasterVolumeSlideToVolumeOff { get; set; }

		public PartEffect Volume { get; set; }

		public PartEffect InstrumentPitchSlide { get; set; }
		public BoostFlag MixResFilBoost { get; set; }

		public byte TransposeNumber { get; set; }
		public ushort PartNumber { get; set; }
		public PartEffect PitchSlide { get; set; }
		public Direction PitchSlideType { get; set; }
		public ushort PitchSlideSpeed { get; set; }
		public ushort PitchSlideNote { get; set; }
		public short PitchSlideToNote { get; set; }
		public short PitchAdd { get; set; }
		public ushort ArpeggioVolumeSlideSpeed { get; set; }
		public ushort ArpeggioPitchSlideSpeed { get; set; }
		public short ArpeggioPitchSlideToNote { get; set; }
		public ushort ArpeggioPitchSlideNote { get; set; }

		public PartEffect PtPitchSlide { get; set; }
		public Direction PtPitchSlideType { get; set; }
		public ushort PtPitchSlideSpeed { get; set; }
		public ushort PtPitchSlideSpeed2 { get; set; }
		public ushort PtPitchSlideNote { get; set; }
		public short PtPitchSlideToNote { get; set; }
		public short PtPitchAdd { get; set; }

		public InstrumentEffect1Entry Effects1 { get; private set; } = new InstrumentEffect1Entry();
		public InstrumentEffect2Entry Effects2 { get; private set; } = new InstrumentEffect2Entry();
		public ushort EnvelopeVolume { get; set; }
		public ushort[] EnvelopeData { get; private set; } = new ushort[12];
		public PlayFlag Play { get; set; }
		public SampleType WaveOrSample { get; set; }
		public byte PhaseInit { get; set; }
		public byte FilterInit { get; set; }
		public byte TransformInit { get; set; }
		public byte TuneWait { get; set; }

		public PartEffect Vibrato { get; set; }
		public Direction VibratoDirection { get; set; }
		public WaveType VibratoWaveType { get; set; }
		public WaveType PartVibratoWaveType { get; set; }
		public ushort VibratoCounter { get; set; }
		public ushort VibratoCommandSpeed { get; set; }
		public ushort VibratoCommandDepth { get; set; }
		public ushort VibratoCommandDelay { get; set; }
		public ushort VibratoAttackSpeed { get; set; }
		public ushort VibratoAttackLength { get; set; }
		public ushort VibratoDepth { get; set; }
		public ushort VibratoNote { get; set; }

		public sbyte PtTremoloPosition { get; set; }
		public byte PtTremoloCommand { get; set; }
		public PtWaveType PtTremoloWaveType { get; set; }
		public sbyte PtVibratoPosition { get; set; }
		public byte PtVibratoCommand { get; set; }
		public PtWaveType PtVibratoWaveType { get; set; }
		public ushort PtVibratoNote { get; set; }

		public PartEffect Tremolo { get; set; }
		public Direction TremoloDirection { get; set; }
		public WaveType TremoloWaveType { get; set; }
		public WaveType PartTremoloWaveType { get; set; }
		public ushort TremoloCounter { get; set; }
		public ushort TremoloCommandSpeed { get; set; }
		public ushort TremoloCommandDepth { get; set; }
		public ushort TremoloCommandDelay { get; set; }
		public ushort TremoloAttackSpeed { get; set; }
		public ushort TremoloAttackLength { get; set; }
		public ushort TremoloDepth { get; set; }

		public sbyte FilterLastSample { get; set; }
		public sbyte ResonanceLastSample { get; set; }
		public bool FilterLastInit { get; set; }
		public bool ResonanceLastInit { get; set; }
		public byte ResonanceAmp { get; set; }
		public byte ResonanceInit { get; set; }
		public PhaseType PhaseType { get; set; }
		public FilterType FilterType { get; set; }
		public CounterInfo TransformCounter { get; private set; } = new CounterInfo();
		public ushort TransformSpeed { get; set; }
		public CounterInfo PhaseCounter { get; private set; } = new CounterInfo();
		public ushort PhaseSpeed { get; set; }
		public CounterInfo MixCounter { get; private set; } = new CounterInfo();
		public ushort MixSpeed { get; set; }
		public CounterInfo ResonanceCounter { get; private set; } = new CounterInfo();
		public ushort ResonanceSpeed { get; set; }
		public CounterInfo FilterCounter { get; private set; } = new CounterInfo();
		public ushort FilterSpeed { get; set; }
		public byte MixWaveNumber { get; set; }
		public byte MixInit { get; set; }
		public byte[] TransformWaveSampleNumbers { get; private set; } = new byte[6];
		public byte LoopInit { get; set; }
		public ushort LoopRepeat { get; set; }
		public ushort LoopRepeatEnd { get; set; }
		public ushort LoopLength { get; set; }
		public int LoopStep { get; set; }
		public ushort LoopWait { get; set; }
		public short LoopWaitCounter { get; set; }
		public short LoopDelay { get; set; }
		public short LoopTurns { get; set; }
		public ushort LoopCounter { get; set; }
		public ushort LoopCounterSave { get; set; }
		public ushort LoopWaveSampleCounterMax { get; set; }
		public short LoopSpeed { get; set; }
		public SamplePointer? LoopWaveSample { get; set; }
		public sbyte[] TransformWaveBuffer { get; private set; } = new sbyte[256];
		public sbyte[] PhaseWaveBuffer { get; private set; } = new sbyte[256];
		public sbyte[] MixWaveBuffer { get; private set; } = new sbyte[256];
		public sbyte[] ResonanceWaveBuffer { get; private set; } = new sbyte[256];
		public sbyte[] FilterWaveBuffer { get; private set; } = new sbyte[256];
		public sbyte[] WaveBuffer { get; private set; } = new sbyte[256];

		/********************************************************************/
		/// <summary>
		/// Test a bit, then clear it and return if the bit was set or not
		/// </summary>
		/********************************************************************/
		public bool TestAndClearArpFlag(ArpeggioFlag mask)
		{
			bool wasSet = Arpeggio.HasFlag(mask);
			Arpeggio &= ~mask;

			return wasSet;
		}



		/********************************************************************/
		/// <summary>
		/// Test a boolean, then clear it and return if it was set or not
		/// </summary>
		/********************************************************************/
		public bool TestAndClearArpWait()
		{
			bool wasTrue = ArpeggioWait;
			ArpeggioWait = false;

			return wasTrue;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.PartEffects = ArrayHelper.CloneArray(PartEffects);
			clone.Effects1 = Effects1.MakeDeepClone();
			clone.Effects2 = Effects2.MakeDeepClone();
			clone.EnvelopeData = ArrayHelper.CloneArray(EnvelopeData);
			clone.TransformCounter = TransformCounter.MakeDeepClone();
			clone.PhaseCounter = PhaseCounter.MakeDeepClone();
			clone.MixCounter = MixCounter.MakeDeepClone();
			clone.ResonanceCounter = ResonanceCounter.MakeDeepClone();
			clone.FilterCounter = FilterCounter.MakeDeepClone();
			clone.TransformWaveSampleNumbers = ArrayHelper.CloneArray(TransformWaveSampleNumbers);
			clone.TransformWaveBuffer = ArrayHelper.CloneArray(TransformWaveBuffer);
			clone.PhaseWaveBuffer = ArrayHelper.CloneArray(PhaseWaveBuffer);
			clone.MixWaveBuffer = ArrayHelper.CloneArray(MixWaveBuffer);
			clone.ResonanceWaveBuffer = ArrayHelper.CloneArray(ResonanceWaveBuffer);
			clone.FilterWaveBuffer = ArrayHelper.CloneArray(FilterWaveBuffer);
			clone.WaveBuffer = ArrayHelper.CloneArray(WaveBuffer);

			return clone;
		}
	}
}

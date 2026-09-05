/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Tracker-specific playback behaviour
	/// Note: The index of every flag has to be fixed, so do not remove flags. Always add new flags at the end!
	/// </summary>
	internal enum PlayBehaviour
	{
		/// <summary>
		/// No-op - only used during loading (Old general compatibility flag for IT/MPT/XM)
		/// </summary>
		Msf_Compatible_Play,

		/// <summary>
		/// MPT 1.16 swing behaviour (IT/MPT, deprecated)
		/// </summary>
		MptOldSwingBehaviour,

		/// <summary>
		/// Emulate broken volume MIDI CC behaviour (IT/MPT/XM, deprecated)
		/// </summary>
		MidiCCBugEmulation,

		/// <summary>
		/// Old VST MIDI pitch bend behaviour (IT/MPT/XM, deprecated)
		/// </summary>
		OldMidiPitchBends,

		/// <summary>
		/// Smooth volume ramping like in FT2 (XM)
		/// </summary>
		Ft2VolumeRamping,

		/// <summary>
		/// F21 and above set speed instead of tempo
		/// </summary>
		ModVBlankTiming,

		/// <summary>
		/// Execute normal slides at speed 1 as if they were fine slides
		/// </summary>
		SlidesAtSpeed1,

		/// <summary>
		/// Compute note frequency in Hertz rather than periods
		/// </summary>
		PeriodsAreHertz,

		/// <summary>
		/// Clamp tempo to 32-255 range
		/// </summary>
		TempoClamp,

		/// <summary>
		/// Global volume slide memory is per-channel
		/// </summary>
		PerChannelGlobalVolSlide,

		/// <summary>
		/// Panning commands override surround and random pan variation
		/// </summary>
		PanOverride,

		/// <summary>
		/// Avoid instrument handling if there is no note
		/// </summary>
		ItInstrWithoutNote,

		/// <summary>
		/// Volume column portamento never does fine portamento
		/// </summary>
		ItVolColFinePortamento,

		/// <summary>
		/// IT arpeggio algorithm
		/// </summary>
		ItArpeggio,

		/// <summary>
		/// Out-of-range delay command behaviour in IT
		/// </summary>
		ItOutOfRangeDelay,

		/// <summary>
		/// Gxx shares memory with Exx and Fxx
		/// </summary>
		ItPortaMemoryShare,

		/// <summary>
		/// After finishing a pattern loop, set the pattern loop target to the next row
		/// </summary>
		ItPatternLoopTargetReset,

		/// <summary>
		/// Nested pattern loop behaviour
		/// </summary>
		ItFt2PatternLoop,

		/// <summary>
		/// Don't reset ping pong direction with instrument numbers
		/// </summary>
		ItPingPongNoReset,

		/// <summary>
		/// IT envelope reset behaviour
		/// </summary>
		ItEnvelopeReset,

		/// <summary>
		/// Forget the previous note after cutting it
		/// </summary>
		ItClearOldNoteAfterCut,

		/// <summary>
		/// More IT-like Hxx / hx, Rxx, Yxx and autovibrato handling, including more precise LUTs
		/// </summary>
		ItVibratoTremoloPanbrello,

		/// <summary>
		/// Ixx behaves like in IT
		/// </summary>
		ItTremor,

		/// <summary>
		/// Qxx behaves like in IT
		/// </summary>
		ItRetrigger,

		/// <summary>
		/// Properly update C-5 frequency when changing in multisampled instrument
		/// </summary>
		ItMultiSampleBehaviour,

		/// <summary>
		/// Clear portamento target after it has been reached
		/// </summary>
		ItPortaTargetReached,

		/// <summary>
		/// Don't reset loop count on pattern break
		/// </summary>
		ItPatternLoopBreak,

		/// <summary>
		/// IT-style Oxx edge case handling
		/// </summary>
		ItOffset,

		/// <summary>
		/// IT's swing behaviour
		/// </summary>
		ItSwingBehaviour,

		/// <summary>
		/// NNA is reset on every note change, not every instrument change
		/// </summary>
		ItNnaReset,

		/// <summary>
		/// SCx really stops the sample and does not just mute it
		/// </summary>
		ItSCxStopsSample,

		/// <summary>
		/// IT-style envelope position advance + enable/disable behaviour
		/// </summary>
		ItEnvelopePositionHandling,

		/// <summary>
		/// No sample changes during portamento with Compatible Gxx enabled, instrument envelope reset with portamento
		/// </summary>
		ItPortamentoInstrument,

		/// <summary>
		/// Don't repeat last sample point in ping pong loop, like IT's software mixer
		/// </summary>
		ItPingPongMode,

		/// <summary>
		/// Use triggered note rather than translated note for PPS and other effects
		/// </summary>
		ItRealNoteMapping,

		/// <summary>
		/// SAx should not apply an offset effect to a note next to it
		/// </summary>
		ItHighOffsetNoRetrig,

		/// <summary>
		/// User IT's filter coefficients (unless extended filter range is used)
		/// </summary>
		ItFilterBehaviour,

		/// <summary>
		/// Panning and surround are mutually exclusive
		/// </summary>
		ItNoSurroundPan,

		/// <summary>
		/// Don't retrigger already stopped channels
		/// </summary>
		ItShortSampleRetrig,

		/// <summary>
		/// Don't apply any portamento if no previous note is playing
		/// </summary>
		ItPortaNoNote,

		/// <summary>
		/// Only reset note-off status on portamento in IT Compatible Gxx mode
		/// </summary>
		ItFt2DontResetNoteOffOnPorta,

		/// <summary>
		/// IT volume column effects share their memory with the effect column
		/// </summary>
		ItVolColMemory,

		/// <summary>
		/// Portamento with sample swap plays the new sample from the beginning
		/// </summary>
		ItPortamentoSwapResetsPos,

		/// <summary>
		/// IT ignores instrument note map entries with no note completely
		/// </summary>
		ItEmptyNoteMapSlot,

		/// <summary>
		/// IT-style first tick handling
		/// </summary>
		ItFirstTickHandling,

		/// <summary>
		/// IT-style sample＆hold panbrello waveform
		/// </summary>
		ItSampleAndHoldPanbrello,

		/// <summary>
		/// New notes reset portamento target in IT
		/// </summary>
		ItClearPortaTarget,

		/// <summary>
		/// Don't reset panbrello effect until next note or panning effect
		/// </summary>
		ItPanbrelloHold,

		/// <summary>
		/// Sample and instrument panning is only applied on note change, not instrument change
		/// </summary>
		ItPanningReset,

		/// <summary>
		/// Bxx on the same row as SBx terminates the loop in IT (old implementation of kITPatternLoopWithJumps)
		/// </summary>
		ItPatternLoopWithJumpsOld,

		/// <summary>
		/// Instrument number with note-off recalls default volume
		/// </summary>
		ItInstrWithNoteOff,

		/// <summary>
		/// FT2 arpeggio algorithm
		/// </summary>
		Ft2Arpeggio,

		/// <summary>
		/// Rxx behaves like in FT2
		/// </summary>
		Ft2Retrigger,

		/// <summary>
		/// Vibrato depth in volume column does not actually execute the vibrato effect
		/// </summary>
		Ft2VolColVibrato,

		/// <summary>
		/// Don't play portamento-ed note if no previous note is playing
		/// </summary>
		Ft2PortaNoNote,

		/// <summary>
		/// FT2-style Kxx handling
		/// </summary>
		Ft2KeyOff,

		/// <summary>
		/// Volume-column pan slides should be handled like fine slides
		/// </summary>
		Ft2PanSlide,

		/// <summary>
		/// Offset past sample end stops the note
		/// </summary>
		Ft2St3OffsetOutOfRange,

		/// <summary>
		/// Don't allow MPT extensions to Xxx command in XM
		/// </summary>
		Ft2RestrictXCommand,

		/// <summary>
		/// Retrigger envelopes if there is a note delay with no note
		/// </summary>
		Ft2RetrigWithNoteDelay,

		/// <summary>
		/// Lxx only sets the pan env position if the volume envelope's sustain flag is set
		/// </summary>
		Ft2SetPanEnvPos,

		/// <summary>
		/// Portamento plus instrument number applies the volume settings of the new sample, but not the new sample itself
		/// </summary>
		Ft2PortaIgnoreInstr,

		/// <summary>
		/// No volume column memory in FT2
		/// </summary>
		Ft2VolColMemory,

		/// <summary>
		/// Next pattern starts on the same row as the last E60 command
		/// </summary>
		Ft2LoopE60Restart,

		/// <summary>
		/// Keep processing silent channels for later 3xx pickup
		/// </summary>
		Ft2ProcessSilentChannels,

		/// <summary>
		/// Reload sample settings even if a note-off is placed next to an instrument number
		/// </summary>
		Ft2ReloadSampleSettings,

		/// <summary>
		/// Portamento with note delay next to it is ignored in FT2
		/// </summary>
		Ft2PortaDelay,

		/// <summary>
		/// Out-of-range transposed notes in FT2
		/// </summary>
		Ft2Transpose,

		/// <summary>
		/// Bxx or Dxx on the same row as E6x terminates the loop in FT2
		/// </summary>
		Ft2PatternLoopWithJumps,

		/// <summary>
		/// Portamento target is not reset with new notes in FT2
		/// </summary>
		Ft2PortaTargetNoReset,

		/// <summary>
		/// FT2 sustain point at end of envelope
		/// </summary>
		Ft2EnvelopeEscape,

		/// <summary>
		/// Txx behaves like in FT2
		/// </summary>
		Ft2Tremor,

		/// <summary>
		/// Out-of-range delay command behaviour in FT2
		/// </summary>
		Ft2OutOfRangeDelay,

		/// <summary>
		/// Use FT2's broken period handling
		/// </summary>
		Ft2Periods,

		/// <summary>
		/// Pan command with delayed note-off
		/// </summary>
		Ft2PanWithDelayedNoteOff,

		/// <summary>
		/// FT2-style volume column handling if there is a note delay
		/// </summary>
		Ft2VolColDelay,

		/// <summary>
		/// Only take the upper 4 bits of sample finetune
		/// </summary>
		Ft2FineTunePrecision,

		/// <summary>
		/// Don't process any effects on muted S3M channels
		/// </summary>
		St3NoMutedChannels,

		/// <summary>
		/// Most effects share the same memory in ST3
		/// </summary>
		St3EffectMemory,

		/// <summary>
		/// Portamento plus instrument number applies the volume settings of the new sample, but not the new sample itself (GUS behaviour)
		/// </summary>
		St3PortaSampleChange,

		/// <summary>
		/// Do not remember vibrato type in effect memory
		/// </summary>
		St3VibratoMemory,

		/// <summary>
		/// Cut note instead of limiting final period (ModPlug Tracker style)
		/// </summary>
		St3LimitPeriod,

		/// <summary>
		/// Portamento after arpeggio continues at the note where the arpeggio left off
		/// </summary>
		St3PortaAfterArpeggio,

		/// <summary>
		/// Allow ProTracker-like oneshot loops
		/// </summary>
		ModOneShotLoops,

		/// <summary>
		/// Do not process any panning commands
		/// </summary>
		ModIgnorePanning,

		/// <summary>
		/// On-the-fly sample swapping
		/// </summary>
		ModSampleSwap,

		/// <summary>
		/// Set and reset the correct fade/key-off flags with note-off and instrument number after note-off
		/// </summary>
		Ft2NoteOffFlags,

		/// <summary>
		/// After portamento to different sample within multi-sampled instrument, lone instrument numbers in patterns always recall the new sample's default settings
		/// </summary>
		ItMultiSampleInstrumentNumber,

		/// <summary>
		/// Retrigger note delays on every reptition of a row
		/// </summary>
		RowDelayWithNoteDelay,

		/// <summary>
		/// FT2-/ProTracker-compatible tremolo ramp down / triangle waveform
		/// </summary>
		Ft2ModTremoloRampWaveform,

		/// <summary>
		/// Portamento up and down have separate memory
		/// </summary>
		Ft2PortaUpDownMemory,

		/// <summary>
		/// ProTracker behaviour for out-of-range note delays
		/// </summary>
		ModOutOfRangeNoteDelay,

		/// <summary>
		/// ProTracker sets tempo after the first tick
		/// </summary>
		ModTempoOnSecondTick,

		/// <summary>
		/// If the sustain point of a panning envelope is reached before key-off, FT2 does not escape it anymore
		/// </summary>
		Ft2PanSustainRelease,

		/// <summary>
		/// Legacy release node volume processing
		/// </summary>
		LegacyReleaseNode,

		/// <summary>
		/// Emulate beating FM oscillators from CDFM / Composer 670
		/// </summary>
		OplBeatingOscillators,

		/// <summary>
		/// Note without instrument uses same offset as previous note
		/// </summary>
		St3OffsetWithoutInstrument,

		/// <summary>
		/// OpenMPT 1.23.01.02 / r4009 broke release nodes past the sustain point, fixed in OpenMPT 1.28
		/// </summary>
		ReleaseNodePastSustainBug,

		/// <summary>
		/// Sometime between OpenMPT 1.18.03.00 and 1.19.01.00, delayed instrument-less notes in XM started recalling the default sample volume and panning
		/// </summary>
		Ft2NoteDelayWithoutInstr,

		/// <summary>
		/// Full control after note-off over OPL voices, ^^^ sends note cut instead of just note-off
		/// </summary>
		OplFlexibleNoteOff,

		/// <summary>
		/// Instrument number with note-off recalls default volume - special cases with Old Effects enabled
		/// </summary>
		ItInstrWithNoteOffOldEffects,

		/// <summary>
		/// Update MIDI channel volume on note-off (legacy bug emulation)
		/// </summary>
		MidiVolumeOnNoteOffBug,

		/// <summary>
		/// Sample / instrument pan does not override channel pan for following samples / instruments that are not panned
		/// </summary>
		ItDoNotOverrideChannelPan,

		/// <summary>
		/// Bxx right of SBx terminates the loop in IT
		/// </summary>
		ItPatternLoopWithJumps,

		/// <summary>
		/// DCT="Sample" requires sample instrument, DCT="Note" checks old pattern note against new pattern note (previously was checking old pattern note against new translated note)
		/// </summary>
		ItDctBehaviour,

		/// <summary>
		/// NNA note-off / fade are applied to OPL channels
		/// </summary>
		OplWithNna,

		/// <summary>
		/// Qxy does not retrigger note after it has been cut with ^^^ or SCx
		/// </summary>
		St3RetrigAfterNoteCut,

		/// <summary>
		/// On-the-fly sample swapping (SoundBlaster behaviour)
		/// </summary>
		St3SampleSwap,

		/// <summary>
		/// Retrigger effect (Qxy) restarts OPL notes
		/// </summary>
		OplRealRetrig,

		/// <summary>
		/// Do not reset OPL channel status at end of envelope (OpenMPT 1.28 inconsistency with samples)
		/// </summary>
		OplNoResetAtEnvelopeEnd,

		/// <summary>
		/// Set note frequency to 0 Hz to "stop" OPL notes
		/// </summary>
		OplNoteStopWith0Hz,

		/// <summary>
		/// Send note-off events for old note on every note change
		/// </summary>
		OplNoteOffOnNoteChange,

		/// <summary>
		/// Reset portamento direction when reaching portamento target from below
		/// </summary>
		Ft2PortaResetDirection,

		/// <summary>
		/// Enforce m_nMaxPeriod
		/// </summary>
		ApplyUpperPeriodLimit,

		/// <summary>
		/// Offset commands even work when there's no note next to them (e.g. DMF, MDL, PLM formats)
		/// </summary>
		ApplyOffsetWithoutNote,

		/// <summary>
		/// Pitch/Pan Separation can be overridden by panning commands (this also fixes a bug where any "special" notes affect PPS)
		/// </summary>
		ItPitchPanSeparation,

		/// <summary>
		/// Use old (less precise) ping-pong overshoot calculation
		/// </summary>
		ImprecisePingPongLoops,

		/// <summary>
		/// Use old tone portamento behaviour for plugins (XM: no plugin pitch slides with commands E1x/E2x/X1x/X2x)
		/// </summary>
		PluginIgnoreTonePortamento,

		/// <summary>
		/// Adlib note next to tone portamento is delayed until next row
		/// </summary>
		St3TonePortaWithAdlibNote,

		/// <summary>
		/// Filter is reset on portamento if sample is swapped
		/// </summary>
		ItResetFilterOnPortaSmpChange,

		/// <summary>
		/// Initial "last note memory" for each channel is C-0 and not "no note"
		/// </summary>
		ItInitialNoteMemory,

		/// <summary>
		/// Default program and bank is set to 1 for plugins, so if an instrument is set to either of those, the program / bank change event is not sent to the plugin
		/// </summary>
		PluginDefaultProgramAndBank1,

		/// <summary>
		/// Do not re-enable sustain loop on portamento, even when switching between samples
		/// </summary>
		ItNoSustainOnPortamento,

		/// <summary>
		/// IT ignores the entire pattern cell when trying to play an unmapped note of an instrument
		/// </summary>
		ItEmptyNoteMapSlotIgnoreCell,

		/// <summary>
		/// IT applies offset commands even if just an instrument number without note is present
		/// </summary>
		ItOffsetWithInstrNumber,

		/// <summary>
		/// FTM: A note without instrument number continues looped samples with the new pitch instead of retriggering them
		/// </summary>
		ContinueSampleWithoutInstr,

		/// <summary>
		/// Behaviour before OpenMPT 1.26: Channel plugin can be used to send MIDI notes
		/// </summary>
		MidiNotesFromChannelPlugin,

		/// <summary>
		/// IT only reads parameters once per row, so if two commands sharing effect parameters are found in the two effect columns, they influence each other
		/// </summary>
		ItDoublePortamentoSlides,

		/// <summary>
		/// S3M commands Kxy and Lxy ignore fine slides
		/// </summary>
		S3MIgnoreCombinedFineSlides,

		/// <summary>
		/// Key-off before auto-vibrato sweep-in is complete resets auto-vibrato depth
		/// </summary>
		Ft2AutoVibratoAbortSweep,

		/// <summary>
		/// Report fake PPQ position to VST plugins
		/// </summary>
		LegacyPpqPos,

		/// <summary>
		/// Plugin notes with NNA=continue are affected by note-offs etc.
		/// </summary>
		LegacyPluginNnaBehaviour,

		/// <summary>
		/// Envelope Carry continues to function as normal even after note-off
		/// </summary>
		ItCarryAfterNoteOff,

		/// <summary>
		/// Offset memory is only updated when offset command is next to a note
		/// </summary>
		Ft2OffsetMemoryRequiresNote,

		/// <summary>
		/// Note Cut (SCx) resets note frequency and interacts with tone portamento with row delay
		/// </summary>
		ItNoteCutWithPorta,

		/// <summary>
		/// Don't propagate volume command c/d parameter to regular command D memory
		/// </summary>
		ItVolColNoSlidePropagation,

		/// <summary>
		/// Stopped filter envelope is still applied even when its first tick has not been processed yet
		/// </summary>
		ItStoppedFilterEnvAtStart,

		/// <summary>
		/// 
		/// </summary>
		MaxPlayBehaviours
	}
}

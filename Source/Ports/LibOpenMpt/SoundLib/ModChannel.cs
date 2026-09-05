/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// The ModChannel struct represents the state of one mixer channel.
	/// ModChannelSettings represents the default settings of one pattern channel
	/// </summary>
	internal class ModChannel : IDeepCloneable<ModChannel>, ICopyTo<ModChannel>
	{
		#region EnvInfo class
		/// <summary>
		/// Envelope playback info
		/// </summary>
		public class EnvInfo : IDeepCloneable<EnvInfo>, ICopyTo<EnvInfo>
		{
			public uint32 nEnvPosition = 0;
			public int16 nEnvValueAtReleaseJump = Snd_Def.Not_Yet_Released;
			public EnvelopeFlags Flags;

			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				nEnvPosition = 0;
				nEnvValueAtReleaseJump = Snd_Def.Not_Yet_Released;
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public EnvInfo MakeDeepClone()
			{
				return (EnvInfo)MemberwiseClone();
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object into another object
			/// </summary>
			/********************************************************************/
			public void CopyTo(EnvInfo destination)
			{
				destination.nEnvPosition = nEnvPosition;
				destination.nEnvValueAtReleaseJump = nEnvValueAtReleaseJump;
				destination.Flags = Flags;
			}
		}
		#endregion

		#region AutoSlideStatus class
		public class AutoSlideStatus : IDeepCloneable<AutoSlideStatus>, ICopyTo<AutoSlideStatus>
		{
			private EnumBitSet<AutoSlideCommand> m_Set = new EnumBitSet<AutoSlideCommand>(AutoSlideCommand.NumCommands);

			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool AnyActive()
			{
				return m_Set.any();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsActive(AutoSlideCommand cmd)
			{
				return m_Set[cmd];
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetActive(AutoSlideCommand cmd, bool active = true)
			{
				m_Set[cmd] = active;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				m_Set.reset();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool AnyPitchSlideActive()
			{
				return IsActive(AutoSlideCommand.TonePortamento) ||
				       IsActive(AutoSlideCommand.PortamentoUp) || IsActive(AutoSlideCommand.PortamentoDown) ||
					   IsActive(AutoSlideCommand.FinePortamentoUp) || IsActive(AutoSlideCommand.FinePortamentoDown) ||
					   IsActive(AutoSlideCommand.PortamentoFc);
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public AutoSlideStatus MakeDeepClone()
			{
				AutoSlideStatus clone = new AutoSlideStatus();

				clone.m_Set = m_Set.MakeDeepClone();

				return clone;
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object into another object
			/// </summary>
			/********************************************************************/
			public void CopyTo(AutoSlideStatus destination)
			{
				m_Set.CopyTo(destination.m_Set);
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		[Flags]
		public enum ResetFlags
		{
			/// <summary>
			/// Reload initial channel settings
			/// </summary>
			ChannelSettings = 1,

			/// <summary>
			/// Reset basic runtime channel attributes
			/// </summary>
			SetPosBasic = 2,

			/// <summary>
			/// Reset more runtime channel attributes
			/// </summary>
			SetPosAdvanced = 4,

			/// <summary>
			/// Reset all runtime channel attributes
			/// </summary>
			SetPosFull = SetPosBasic | SetPosAdvanced | ChannelSettings,

			/// <summary>
			/// 
			/// </summary>
			Total = SetPosFull
		}

		// Information used in the mixer

		/// <summary>
		/// Current play position (fixed point)
		/// </summary>
		public SamplePosition Position;

		/// <summary>
		/// Sample speed relative to mixing frequency (fixed point)
		/// </summary>
		public SamplePosition Increment;

		/// <summary>
		/// Currently playing sample (nullptr if no sample is playing)
		/// </summary>
		public IPointer pCurrentSample;

		/// <summary>
		/// 0...4096 (12 bits, since 16 bits + 12 bits = 28 bits = 0dB in integer mixer, see MIXING_ATTENUATION)
		/// </summary>
		public int32 LeftVol;

		/// <summary>
		/// Ditto
		/// </summary>
		public int32 RightVol;

		/// <summary>
		/// Ramping delta, 20.12 fixed point (see VOLUMERAMPPRECISION)
		/// </summary>
		public int32 LeftRamp;

		/// <summary>
		/// Ditto
		/// </summary>
		public int32 RightRamp;

		/// <summary>
		/// Current ramping volume, 20.12 fixed point (see VOLUMERAMPPRECISION)
		/// </summary>
		public int32 RampLeftVol;

		/// <summary>
		/// Ditto
		/// </summary>
		public int32 RampRightVol;

		/// <summary>
		/// Filter memory - two history items per sample channel
		/// </summary>
		public mixsample_t[][] nFilter_Y = ArrayHelper.Initialize2Arrays<mixsample_t>(2, 2);

		/// <summary>
		/// Filter coeffs
		/// </summary>
		public mixsample_t nFilter_A0;

		/// <summary>
		/// 
		/// </summary>
		public mixsample_t nFilter_B0;

		/// <summary>
		/// 
		/// </summary>
		public mixsample_t nFilter_B1;

		/// <summary>
		/// 
		/// </summary>
		public mixsample_t nFilter_HP;

		/// <summary>
		/// 
		/// </summary>
		public SmpLength nLength;

		/// <summary>
		/// 
		/// </summary>
		public SmpLength nLoopStart;

		/// <summary>
		/// 
		/// </summary>
		public SmpLength nLoopEnd;

		/// <summary>
		/// 
		/// </summary>
		public ChannelFlags dwFlags;

		/// <summary>
		/// 
		/// </summary>
		public mixsample_t nROfs;

		/// <summary>
		/// 
		/// </summary>
		public mixsample_t nLOfs;

		/// <summary>
		/// 
		/// </summary>
		public uint32 nRampLength;

		/// <summary>
		/// Currently assigned sample slot (may already be stopped)
		/// </summary>
		public ModSample pModSample;

		/// <summary>
		/// 
		/// </summary>
		public InstrumentSynth.States SynthState = new InstrumentSynth.States();

		// Information not used in the mixer

		/// <summary>
		/// Currently assigned instrument slot
		/// </summary>
		public ModInstrument pModInstrument;

		/// <summary>
		/// Offset for instrument-less notes for ProTracker/ScreamTracker
		/// </summary>
		public SmpLength PrevNoteOffset;

		/// <summary>
		/// Offset command memory
		/// </summary>
		public SmpLength OldOffset;

		/// <summary>
		/// Flags from previous tick
		/// </summary>
		public ChannelFlags dwOldFlags;

		/// <summary>
		/// 
		/// </summary>
		public int32 NewLeftVol;

		/// <summary>
		/// 
		/// </summary>
		public int32 NewRightVol;

		/// <summary>
		/// 
		/// </summary>
		public int32 nRealVolume;

		/// <summary>
		/// 
		/// </summary>
		public int32 nRealPan;

		/// <summary>
		/// 
		/// </summary>
		public int32 nVolume;

		/// <summary>
		/// 
		/// </summary>
		public int32 nPan;

		/// <summary>
		/// 
		/// </summary>
		public int32 nFadeOutVol;

		/// <summary>
		/// Frequency in Hz if CSoundFile::PeriodsAreFrequencies() or using custom tuning, 4x Amiga periods otherwise
		/// </summary>
		public int32 nPeriod;

		/// <summary>
		/// 
		/// </summary>
		public int32 nC5Speed;

		/// <summary>
		/// 
		/// </summary>
		public int32 nPortamentoDest;

		/// <summary>
		/// 
		/// </summary>
		public int32 CachedPeriod;

		/// <summary>
		/// 
		/// </summary>
		public int32 GlissandoPeriod;

		/// <summary>
		/// Calculated channel volume, 14-Bit (without global volume, pre-amp etc applied) - for MIDI macros
		/// </summary>
		public int32 nCalcVolume;

		/// <summary>
		/// Envelope playback info
		/// </summary>
		public EnvInfo VolEnv = new EnvInfo();

		/// <summary>
		/// 
		/// </summary>
		public EnvInfo PanEnv = new EnvInfo();

		/// <summary>
		/// 
		/// </summary>
		public EnvInfo PitchEnv = new EnvInfo();

		/// <summary>
		/// 
		/// </summary>
		public int32 nAutoVibDepth;

		/// <summary>
		/// Offset memory for Invert Loop (EFx, .MOD only)
		/// </summary>
		public uint32 nEFxOffset;

		/// <summary>
		/// 
		/// </summary>
		public RowIndex nPatternLoop;

		/// <summary>
		/// 
		/// </summary>
		public AutoSlideStatus AutoSlide = new AutoSlideStatus();

		/// <summary>
		/// 
		/// </summary>
		public uint16 PortamentoSlide;

		/// <summary>
		/// 
		/// </summary>
		public int16 nFineTune;

		/// <summary>
		/// Micro-tuning / MIDI pitch wheel command
		/// </summary>
		public int16 MicroTuning;

		/// <summary>
		/// 
		/// </summary>
		public int16 nVolSwing;

		/// <summary>
		/// 
		/// </summary>
		public int16 nPanSwing;

		/// <summary>
		/// 
		/// </summary>
		public int16 nCutSwing;

		/// <summary>
		/// 
		/// </summary>
		public int16 nResSwing;

		/// <summary>
		/// 
		/// </summary>
		public uint16 VolSlideDownRemain;

		/// <summary>
		/// 
		/// </summary>
		public uint16 VolSlideDownTotal;

		/// <summary>
		/// If › 0, nPan should be set to nRestorePanOnNewNote - 1 on new note.
		/// Used to recover from pan swing and IT sample / instrument panning.
		/// High bit set = surround
		/// </summary>
		public ref uint16 nRestorePanOnNewNote
		{
			get => ref restoreAndNnaUnion;
		}

		/// <summary>
		/// If channel is moved to background (NNA), this counts up how old it is
		/// </summary>
		public ref uint16 NnaChannelAge
		{
			get => ref restoreAndNnaUnion;
		}

		/// <summary>
		/// Backend field for both nRestorePanOnNewNote and NnaChannelAge,
		/// since they are a union
		/// </summary>
		private uint16 restoreAndNnaUnion;

		/// <summary>
		/// For PlaybackTest implementation
		/// </summary>
		public uint16 NnaGeneration;

		/// <summary>
		/// 
		/// </summary>
		public ChannelIndex nMasterChn;

		/// <summary>
		/// Sample to swap to when current sample (loop) has finished playing
		/// </summary>
		public SampleIndex SwapSampleIndex;

		/// <summary>
		/// 
		/// </summary>
		public ModCommand RowCommand = new ModCommand();

		// 8-bit members

		/// <summary>
		/// Channel volume (CV in ITTECH.TXT) 0...64
		/// </summary>
		public uint8 nGlobalVol;

		/// <summary>
		/// Sample / Instrument volume (SV * IV in ITTECH.TXT) 0...64
		/// </summary>
		public uint8 nInsVol;

		/// <summary>
		/// 
		/// </summary>
		public int8 nTranspose;

		/// <summary>
		/// 
		/// </summary>
		public ResamplingMode ResamplingMode;

		/// <summary>
		/// See nRestorePanOnNewNote
		/// </summary>
		public uint8 nRestoreResonanceOnNewNote;

		/// <summary>
		/// Ditto
		/// </summary>
		public uint8 nRestoreCutOffOnNewNote;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nNote;

		/// <summary>
		/// 
		/// </summary>
		public NewNoteAction nNna;

		/// <summary>
		/// Last note, ignoring note offs and cuts - for MIDI macros
		/// </summary>
		public uint8 nLastNote;

		/// <summary>
		/// For plugin arpeggio and NNA handling
		/// </summary>
		public uint8 nArpeggioLastNote;

		/// <summary>
		/// 
		/// </summary>
		public uint8 LastMidiNoteWithoutArp;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nNewNote;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nNewIns;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldIns;

		/// <summary>
		/// 
		/// </summary>
		public EffectCommand nCommand;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nArpeggio;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nRetrigParam;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nRetrigCount;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldVolumeSlide;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldFineVolUpDown;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldPortaUp;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldPortaDown;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldFinePortaUpDown;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldExtraFinePortaUpDown;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldPanSlide;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldChnVolSlide;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldGlobalVolSlide;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nAutoVibPos;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nVibratoPos;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremoloPos;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPanbrelloPos;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nVibratoType;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nVibratoSpeed;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nVibratoDepth;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremoloType;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremoloSpeed;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremoloDepth;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPanbrelloType;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPanbrelloSpeed;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPanbrelloDepth;

		/// <summary>
		/// 
		/// </summary>
		public int8 nPanbrelloOffset;

		/// <summary>
		/// 
		/// </summary>
		public int8 nPanbrelloRandomMemory;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldCmdEx;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldVolParam;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldTempo;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nOldHiOffset;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nCutOff;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nResonance;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremorCount;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nTremorParam;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPatternLoopCount;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nActiveMacro;

		/// <summary>
		/// 
		/// </summary>
		public uint8 VolSlideDownStart;

		/// <summary>
		/// 
		/// </summary>
		public FilterMode nFilterMode;

		/// <summary>
		/// Memory for Invert Loop (EFx, .MOD only)
		/// </summary>
		public uint8 nEFxSpeed;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nEFxDelay;

		/// <summary>
		/// IMF / PTM Note Slide
		/// </summary>
		public uint8 NoteSlideParam;

		/// <summary>
		/// 
		/// </summary>
		public uint8 NoteSlideCounter;

		/// <summary>
		/// Memory for \xx slides
		/// </summary>
		public uint8 LastZxxParam;

		/// <summary>
		/// Execute tick-0 effects on this channel? (condition differs between formats due to Pattern Delay commands)
		/// </summary>
		public bool IsFirstTick;

		/// <summary>
		/// Trigger note on this tick on this channel if there is one?
		/// </summary>
		public bool TriggerNote;

		/// <summary>
		/// Notes preview in editor
		/// </summary>
		public bool IsPreviewNote;

		/// <summary>
		/// Don't mix or increment channel position, but keep the note alive
		/// </summary>
		public bool IsPaused;

		/// <summary>
		/// Tone portamento is finished
		/// </summary>
		public bool PortaTargetReached;

		/// <summary>
		/// Future Composer portamento state
		/// </summary>
		public bool FcPortaTick;

		/// <summary>
		/// Variables used to make user-definable tuning modes work with pattern effects.
		/// If true, freq should be recalculated in ReadNote() on first tick.
		/// Currently used only for vibrato things - using in other context might be
		/// problematic 
		/// </summary>
		public bool m_RecalculateFreqOnFirstTick;

		/// <summary>
		/// To tell whether to calculate frequency
		/// </summary>
		public bool m_CalculateFreq;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_PortamentoFineSteps;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_PortamentoTickSlide;

		/// <summary>
		/// NOTE_PCs memory
		/// </summary>
		public c_float m_PlugParamValueStep;

		/// <summary>
		/// 
		/// </summary>
		public c_float m_PlugParamTargetValue;

		/// <summary>
		/// 
		/// </summary>
		public uint16 m_RowPlugParam;

		/// <summary>
		/// 
		/// </summary>
		public PlugIndex m_RowPlug;

		/********************************************************************/
		/// <summary>
		/// Get a reference to a specific envelope of this channel
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EnvInfo GetEnvelope(EnvelopeType envType)
		{
			switch (envType)
			{
				case EnvelopeType.Volume:
				default:
					return VolEnv;

				case EnvelopeType.Panning:
					return PanEnv;

				case EnvelopeType.Pitch:
					return PitchEnv;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetEnvelopes()
		{
			VolEnv.Reset();
			PanEnv.Reset();
			PitchEnv.Reset();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSamplePlaying()
		{
			return !Increment.IsZero();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Reset(ResetFlags resetMask, CSoundFile sndFile, ChannelIndex sourceChannel, ChannelFlags muteFlag)//XX 19
		{
			// For "the ultimate beeper.mod"
			ModSample defaultSample = (sndFile.GetType_() == ModType.Mod) && sndFile.GetSample(0).HasSampleData() ? sndFile.GetSample(0) : null;

			if ((resetMask & ResetFlags.SetPosBasic) != 0)
			{
				// IT compatibility: Initial "last note memory" of channel is C-0 (so a lonely instrument number without note will play that note).
				// Test case: InitialNoteMemory.it
				nNote = nNewNote = (sndFile.m_PlayBehaviour[PlayBehaviour.ItInitialNoteMemory] ? ModCommand.Note_Min : ModCommand.Note_None);
				nArpeggioLastNote = LastMidiNoteWithoutArp = ModCommand.Note_None;
				nNewIns = nOldIns = 0;
				SwapSampleIndex = 0;
				pModSample = defaultSample;
				pModInstrument = null;
				nPortamentoDest = 0;
				nCommand = EffectCommand.None;
				nPatternLoopCount = 0;
				nPatternLoop = 0;
				nFadeOutVol = 0;
				dwFlags.Set(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade);
				dwOldFlags.Reset();
				AutoSlide.Reset();
				nInsVol = 64;
				NnaGeneration = 0;

				// IT compatibility 15. Retrigger
				if (sndFile.m_PlayBehaviour[PlayBehaviour.ItRetrigger])
				{
					nRetrigParam = 1;
					nRetrigCount = 0;
				}

				MicroTuning = 0;
				nTremorCount = 0;
				nEFxSpeed = 0;
				PrevNoteOffset = 0;
				LastZxxParam = 0xff;
				IsFirstTick = false;
				TriggerNote = false;
				IsPreviewNote = false;
				IsPaused = false;
				PortaTargetReached = false;
				RowCommand.Clear();
				SynthState = new InstrumentSynth.States();
			}

			if ((resetMask & ResetFlags.SetPosAdvanced) != 0)
			{
				Increment = new SamplePosition(0);
				nPeriod = 0;
				Position.Set(0);
				nLength = 0;
				nLoopStart = 0;
				nLoopEnd = 0;
				nROfs = nLOfs = 0;
				pModSample = defaultSample;
				pModInstrument = null;
				nCutOff = 0x7f;
				nResonance = 0;
				nFilterMode = FilterMode.LowPass;
				RightVol = LeftVol = 0;
				NewRightVol = NewLeftVol = 0;
				RightRamp = LeftRamp = 0;
				nVolume = 0;		// Needs to be 0 for SMP_NODEDEFAULTVOLUME flag
				nVibratoPos = nTremoloPos = nPanbrelloPos = 0;
				nOldHiOffset = 0;
				nOldExtraFinePortaUpDown = nOldFinePortaUpDown = nOldPortaDown = nOldPortaUp = 0;
				PortamentoSlide = 0;
				nMasterChn = 0;

				// Custom tuning related
				m_RecalculateFreqOnFirstTick = false;
				m_CalculateFreq = false;
				m_PortamentoFineSteps = 0;
				m_PortamentoTickSlide = 0;
			}

			if ((resetMask & ResetFlags.ChannelSettings) != 0)
			{
				if (sourceChannel < sndFile.ChnSettings.size())
				{
					dwFlags = sndFile.ChnSettings[sourceChannel].dwFlags;
					nPan = sndFile.ChnSettings[sourceChannel].nPan;
					nGlobalVol = sndFile.ChnSettings[sourceChannel].nVolume;

					if (dwFlags.Test(ChannelFlags.Chn_Mute))
					{
						dwFlags.Reset(ChannelFlags.Chn_Mute);
						dwFlags.Set(muteFlag);
					}
				}
				else
				{
					dwFlags.Reset();
					nPan = 128;
					nGlobalVol = 64;
				}

				nRestorePanOnNewNote = 0;
				nRestoreCutOffOnNewNote = 0;
				nRestoreResonanceOnNewNote = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Stop()//XX 120
		{
			nPeriod = 0;
			Increment.Set(0);
			Position.Set(0);
			nVolume = 0;
			pCurrentSample = null;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void UpdateInstrumentVolume(ModSample smp, ModInstrument ins)//XX 131
		{
			nInsVol = 64;

			if (smp != null)
				nInsVol = (uint8)smp.nGlobalVol;

			if (ins != null)
				nInsVol = (uint8)((nInsVol * ins.nGlobalVol) / 64);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public uint32 GetVstVolume()//XX 141
		{
			return pModInstrument != null ? pModInstrument.nGlobalVol * 4 : (uint32)nVolume;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the channel has a valid MIDI output
		/// </summary>
		/********************************************************************/
		public bool HasMidiOutput()//XX 163
		{
			return (pModInstrument != null) && pModInstrument.HasValidMidiChannel();
		}



		/********************************************************************/
		/// <summary>
		/// Check if the channel uses custom tuning
		/// </summary>
		/********************************************************************/
		public bool HasCustomTuning()//XX 169
		{
			return (pModInstrument != null) && (pModInstrument.pTuning != null);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool InSustainLoop()//XX 175
		{
			return ((dwFlags & (ChannelFlags.Chn_Loop | ChannelFlags.Chn_KeyOff)) == ChannelFlags.Chn_Loop) && pModSample.uFlags.Test(ChannelFlags.Chn_SustainLoop);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void SetInstrumentPan(int32 pan, CSoundFile sndFile)//XX 181
		{
			// IT compatibility: Instrument and sample panning does not override channel panning
			// Test case: PanResetInstr.it
			if (sndFile.m_PlayBehaviour[PlayBehaviour.ItDoNotOverrideChannelPan])
			{
				nRestorePanOnNewNote = (uint16)(nPan + 1);

				if (dwFlags.Test(ChannelFlags.Chn_Surround))
					nRestorePanOnNewNote |= 0x8000;
			}

			nPan = pan;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void RestorePanAndFilter()//XX 195
		{
			if (nRestorePanOnNewNote > 0)
			{
				nPan = (nRestorePanOnNewNote & 0x7fff) - 1;

				if ((nRestorePanOnNewNote & 0x8000) != 0)
					dwFlags.Set(ChannelFlags.Chn_Surround);

				nRestorePanOnNewNote = 0;
			}

			if (nRestoreResonanceOnNewNote > 0)
			{
				nResonance = (uint8)(nRestoreResonanceOnNewNote - 1);
				nRestoreResonanceOnNewNote = 0;
			}

			if (nRestoreCutOffOnNewNote > 0)
			{
				nCutOff = (uint8)(nRestoreCutOffOnNewNote - 1);
				nRestoreCutOffOnNewNote = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void RecalcTuningFreq(TuningRatioType vibratoFactor, TuningNoteIndexType arpeggioSteps, CSoundFile sndFile)//XX 217
		{
			if (!HasCustomTuning())
				return;

			ModCommandNote note = ModCommand.IsNote(nNote) ? nNote : nLastNote;

			if (sndFile.m_PlayBehaviour[PlayBehaviour.ItRealNoteMapping] && (note >= ModCommand.Note_Min) && (note <= ModCommand.Note_Max))
				note = pModInstrument.NoteMap[note - ModCommand.Note_Min];

			nPeriod = (int32)SaturateRound.Saturate_Round<uint32, c_float>(nC5Speed * vibratoFactor * pModInstrument.pTuning.GetRatio((TuningNoteIndexType)(note - ModCommand.Note_MiddleC + arpeggioSteps), nFineTune + m_PortamentoFineSteps) * (1 << Snd_Def.Freq_FracBits));
		}



		/********************************************************************/
		/// <summary>
		/// IT command S73-S7E
		/// </summary>
		/********************************************************************/
		public void InstrumentControl(uint8 param, CSoundFile sndFile)//XX 232
		{
			param &= 0x0f;

			switch (param)
			{
				case 0x3:
				{
					nNna = NewNoteAction.NoteCut;
					break;
				}

				case 0x4:
				{
					nNna = NewNoteAction.Continue;
					break;
				}

				case 0x5:
				{
					nNna = NewNoteAction.NoteOff;
					break;
				}

				case 0x6:
				{
					nNna = NewNoteAction.NoteFade;
					break;
				}

				case 0x7:
				{
					VolEnv.Flags.Reset(EnvelopeFlags.Enabled);
					break;
				}

				case 0x8:
				{
					VolEnv.Flags.Set(EnvelopeFlags.Enabled);
					break;
				}

				case 0x9:
				{
					PanEnv.Flags.Reset(EnvelopeFlags.Enabled);
					break;
				}

				case 0xa:
				{
					PanEnv.Flags.Set(EnvelopeFlags.Enabled);
					break;
				}

				case 0xb:
				{
					PitchEnv.Flags.Reset(EnvelopeFlags.Enabled);
					break;
				}

				case 0xc:
				{
					PitchEnv.Flags.Set(EnvelopeFlags.Enabled);
					break;
				}

				case 0xd:	// S7D: Enable pitch envelope, force to play as pitch envelope
				case 0xe:	// S7E: Enable pitch envelope, force to play as filter envelope
				{
					if (sndFile.GetType_() == ModType.Mpt)
					{
						PitchEnv.Flags.Set(EnvelopeFlags.Enabled);
						PitchEnv.Flags.Set(EnvelopeFlags.Filter, param != 0xd);
					}

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Volume command: xx
		/// </summary>
		/********************************************************************/
		public void PlayControl(uint8 param)//XX 260
		{
			switch (param)
			{
				case 0:
				{
					IsPaused = true;
					break;
				}

				case 1:
				{
					IsPaused = false;
					break;
				}

				case 2:
				{
					dwFlags.Set(ChannelFlags.Chn_PingPongFlag, false);
					break;
				}

				case 3:
				{
					dwFlags.Set(ChannelFlags.Chn_PingPongFlag, true);
					break;
				}

				case 4:
				{
					dwFlags.Flip(ChannelFlags.Chn_PingPongFlag);
					break;
				}

				case 5:
				{
					OldOffset = Position.GetUInt();
					break;
				}

				case 6:
				{
					Position.Set((int32)OldOffset);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModChannel MakeDeepClone()
		{
			ModChannel clone = (ModChannel)MemberwiseClone();

			clone.nFilter_Y = ArrayHelper.CloneArray(nFilter_Y);
			clone.SynthState = SynthState.MakeDeepClone();
			clone.VolEnv = VolEnv.MakeDeepClone();
			clone.PanEnv = PanEnv.MakeDeepClone();
			clone.PitchEnv = PitchEnv.MakeDeepClone();
			clone.AutoSlide = AutoSlide.MakeDeepClone();
			clone.RowCommand = RowCommand.MakeDeepClone();

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(ModChannel destination)
		{
			destination.Position = Position;
			destination.Increment = Increment;
			destination.pCurrentSample = pCurrentSample;
			destination.LeftVol = LeftVol;
			destination.RightVol = RightVol;
			destination.LeftRamp = LeftRamp;
			destination.RightRamp = RightRamp;
			destination.RampLeftVol = RampLeftVol;
			destination.RampRightVol = RampRightVol;

			for (int i = 0; i < nFilter_Y.Length; i++)
				nFilter_Y[i].CopyTo(destination.nFilter_Y[i]);

			destination.nFilter_A0 = nFilter_A0;
			destination.nFilter_B0 = nFilter_B0;
			destination.nFilter_B1 = nFilter_B1;
			destination.nFilter_HP = nFilter_HP;
			destination.nLength = nLength;
			destination.nLoopStart = nLoopStart;
			destination.nLoopEnd = nLoopEnd;
			destination.dwFlags = dwFlags;
			destination.nROfs = nROfs;
			destination.nLOfs = nLOfs;
			destination.nRampLength = nRampLength;
			destination.pModSample = pModSample;
			SynthState.CopyTo(destination.SynthState);
			destination.pModInstrument = pModInstrument;
			destination.PrevNoteOffset = PrevNoteOffset;
			destination.OldOffset = OldOffset;
			destination.dwOldFlags = dwOldFlags;
			destination.NewLeftVol = NewLeftVol;
			destination.NewRightVol = NewRightVol;
			destination.nRealVolume = nRealVolume;
			destination.nRealPan = nRealPan;
			destination.nVolume = nVolume;
			destination.nPan = nPan;
			destination.nFadeOutVol = nFadeOutVol;
			destination.nPeriod = nPeriod;
			destination.nC5Speed = nC5Speed;
			destination.nPortamentoDest = nPortamentoDest;
			destination.CachedPeriod = CachedPeriod;
			destination.GlissandoPeriod = GlissandoPeriod;
			destination.nCalcVolume = nCalcVolume;
			VolEnv.CopyTo(destination.VolEnv);
			PanEnv.CopyTo(destination.PanEnv);
			PitchEnv.CopyTo(destination.PitchEnv);
			destination.nAutoVibDepth = nAutoVibDepth;
			destination.nEFxOffset = nEFxOffset;
			destination.nPatternLoop = nPatternLoop;
			AutoSlide.CopyTo(destination.AutoSlide);
			destination.PortamentoSlide = PortamentoSlide;
			destination.nFineTune = nFineTune;
			destination.MicroTuning = MicroTuning;
			destination.nVolSwing = nVolSwing;
			destination.nPanSwing = nPanSwing;
			destination.nCutSwing = nCutSwing;
			destination.nResSwing = nResSwing;
			destination.VolSlideDownRemain = VolSlideDownRemain;
			destination.VolSlideDownTotal = VolSlideDownTotal;
			destination.restoreAndNnaUnion = restoreAndNnaUnion;
			destination.NnaGeneration = NnaGeneration;
			destination.nMasterChn = nMasterChn;
			destination.SwapSampleIndex = SwapSampleIndex;
			RowCommand.CopyTo(destination.RowCommand);
			destination.nGlobalVol = nGlobalVol;
			destination.nInsVol = nInsVol;
			destination.nTranspose = nTranspose;
			destination.ResamplingMode = ResamplingMode;
			destination.nRestoreResonanceOnNewNote = nRestoreResonanceOnNewNote;
			destination.nRestoreCutOffOnNewNote = nRestoreCutOffOnNewNote;
			destination.nNote = nNote;
			destination.nNna = nNna;
			destination.nLastNote = nLastNote;
			destination.nArpeggioLastNote = nArpeggioLastNote;
			destination.LastMidiNoteWithoutArp = LastMidiNoteWithoutArp;
			destination.nNewNote = nNewNote;
			destination.nNewIns = nNewIns;
			destination.nOldIns = nOldIns;
			destination.nCommand = nCommand;
			destination.nArpeggio = nArpeggio;
			destination.nRetrigParam = nRetrigParam;
			destination.nRetrigCount = nRetrigCount;
			destination.nOldVolumeSlide = nOldVolumeSlide;
			destination.nOldFineVolUpDown = nOldFineVolUpDown;
			destination.nOldPortaUp = nOldPortaUp;
			destination.nOldPortaDown = nOldPortaDown;
			destination.nOldFinePortaUpDown = nOldFinePortaUpDown;
			destination.nOldExtraFinePortaUpDown = nOldExtraFinePortaUpDown;
			destination.nOldPanSlide = nOldPanSlide;
			destination.nOldChnVolSlide = nOldChnVolSlide;
			destination.nOldGlobalVolSlide = nOldGlobalVolSlide;
			destination.nAutoVibPos = nAutoVibPos;
			destination.nVibratoPos = nVibratoPos;
			destination.nTremoloPos = nTremoloPos;
			destination.nPanbrelloPos = nPanbrelloPos;
			destination.nVibratoType = nVibratoType;
			destination.nVibratoSpeed = nVibratoSpeed;
			destination.nVibratoDepth = nVibratoDepth;
			destination.nTremoloType = nTremoloType;
			destination.nTremoloSpeed = nTremoloSpeed;
			destination.nTremoloDepth = nTremoloDepth;
			destination.nPanbrelloType = nPanbrelloType;
			destination.nPanbrelloSpeed = nPanbrelloSpeed;
			destination.nPanbrelloDepth = nPanbrelloDepth;
			destination.nPanbrelloOffset = nPanbrelloOffset;
			destination.nPanbrelloRandomMemory = nPanbrelloRandomMemory;
			destination.nOldCmdEx = nOldCmdEx;
			destination.nOldVolParam = nOldVolParam;
			destination.nOldTempo = nOldTempo;
			destination.nOldHiOffset = nOldHiOffset;
			destination.nCutOff = nCutOff;
			destination.nResonance = nResonance;
			destination.nTremorCount = nTremorCount;
			destination.nTremorParam = nTremorParam;
			destination.nPatternLoopCount = nPatternLoopCount;
			destination.nActiveMacro = nActiveMacro;
			destination.VolSlideDownStart = VolSlideDownStart;
			destination.nFilterMode = nFilterMode;
			destination.nEFxSpeed = nEFxSpeed;
			destination.nEFxDelay = nEFxDelay;
			destination.NoteSlideParam = NoteSlideParam;
			destination.NoteSlideCounter = NoteSlideCounter;
			destination.LastZxxParam = LastZxxParam;
			destination.IsFirstTick = IsFirstTick;
			destination.TriggerNote = TriggerNote;
			destination.IsPreviewNote = IsPreviewNote;
			destination.IsPaused = IsPaused;
			destination.PortaTargetReached = PortaTargetReached;
			destination.FcPortaTick = FcPortaTick;
			destination.m_RecalculateFreqOnFirstTick = m_RecalculateFreqOnFirstTick;
			destination.m_CalculateFreq = m_CalculateFreq;
			destination.m_PortamentoFineSteps = m_PortamentoFineSteps;
			destination.m_PortamentoTickSlide = m_PortamentoTickSlide;
			destination.m_PlugParamValueStep = m_PlugParamValueStep;
			destination.m_PlugParamTargetValue = m_PlugParamTargetValue;
			destination.m_RowPlugParam = m_RowPlugParam;
			destination.m_RowPlug = m_RowPlug;
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal partial class CSoundFile
	{
		#region UpgradePatternData class
		private class UpgradePatternData : CPatternContainer.IForeach<UpgradePatternData, ModCommand>
		{
			private CSoundFile sndFile;
			private ChannelIndex chn = 0;
			private bool compatPlay;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public UpgradePatternData(CSoundFile sf)
			{
				sndFile = sf;
				compatPlay = sf.m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play];
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public void Invoke(CPointer<ModCommand> arr)
			{
				ModCommand m = arr[0];
				ChannelIndex curChn = chn;
				chn++;

				if (chn >= sndFile.GetNumChannels())
					chn = 0;

				if (m.IsPcNote())
					return;

				Version version = sndFile.m_dwLastSavedWithVersion;
				ModType modType = sndFile.GetType_();

				if (modType == ModType.S3M)
				{
					// Out-of-range global volume commands should be ignored in S3M. Fixed in OpenMPT 1.19 (r831).
					// So for tracks made with older versions of OpenMPT, we limit invalid global volume commands
					if ((version < Version.MPT_V._1_19_00_00) && (m.Command == EffectCommand.GlobalVolume))
						OpenMpt.LimitMax(ref m.Param, (ModCommandParam)64);
				}
				else if ((modType & (ModType.It | ModType.Mpt)) != 0)
				{
					if ((version < Version.MPT_V._1_17_03_02) || (!compatPlay && (version < Version.MPT_V._1_20_00_00)))
					{
						if (m.Command == EffectCommand.GlobalVolume)
						{
							// Out-of-range global volume commands should be ignored in IT.
							// OpenMPT 1.17.03.02 fixed this in compatible mode, OpenMPT 1.20 fixes it in normal mode as well.
							// So for tracks made with older versions than OpenMPT 1.17.03.02 or tracks made with 1.17.03.02 <= version < 1.20, we limit invalid global volume commands
							OpenMpt.LimitMax(ref m.Param, (ModCommandParam)128);
						}
						// SC0 and SD0 should be interpreted as SC1 and SD1 in IT files.
						// OpenMPT 1.17.03.02 fixed this in compatible mode, OpenMPT 1.20 fixes it in normal mode as well
						else if (m.Command == EffectCommand.S3MCmdEx)
						{
							if (m.Param == 0xc0)
							{
								m.Command = EffectCommand.None;
								m.Note = ModCommand.Note_NoteCut;
							}
							else if (m.Param == 0xd0)
								m.Command = EffectCommand.None;
						}
					}

					// In the IT format, slide commands with both nibbles set should be ignored.
					// For note volume slides, OpenMPT 1.18 fixes this in compatible mode, OpenMPT 1.20 fixes this in normal mode as well
					bool noteVolSlide = ((version < Version.MPT_V._1_18_00_00) || (!compatPlay && (version < Version.MPT_V._1_20_00_00))) &&
										((m.Command == EffectCommand.VolumeSlide) || (m.Command == EffectCommand.VibratoVol) || (m.Command == EffectCommand.TonePortaVol) || (m.Command == EffectCommand.PanningSlide));

					// OpenMPT 1.20 also fixes this for global volume and channel volume slides
					bool chanVolSlide = (version < Version.MPT_V._1_20_00_00) && ((m.Command == EffectCommand.GlobalVolSlide) || (m.Command == EffectCommand.ChannelVolSlide));

					if (noteVolSlide || chanVolSlide)
					{
						if (((m.Param & 0x0f) != 0x00) && ((m.Param & 0x0f) != 0x0f) && ((m.Param & 0xf0) != 0x00) && ((m.Param & 0xf0) != 0xf0))
						{
							if (m.Command == EffectCommand.GlobalVolSlide)
								m.Param &= 0xf0;
							else
								m.Param &= 0x0f;
						}
					}

					if ((version < Version.MPT_V._1_22_01_04) && (version != Version.MPT_V._1_22_00_00))	// Ignore compatibility export
					{
						// OpenMPT 1.22.01.04 fixes illegal (out of range) instrument numbers; they should do nothing. In previous versions, they stopped the playing sample
						if ((sndFile.GetNumInstruments() != 0) && (m.Instr > sndFile.GetNumInstruments()) && !compatPlay)
						{
							m.VolCmd = VolumeCommand.Volume;
							m.Vol = 0;
						}
					}

					// Command I11 accidentally behaved the same as command I00 with compatible IT tremor and old effects disabled
					if ((m.Command == EffectCommand.Tremor) && (m.Param == 0x11) && (version < Version.MPT_V._1_29_12_02) && sndFile.m_PlayBehaviour[PlayBehaviour.ItTremor] && !sndFile.m_SongFlags.Test(SongFlags.ItOldEffects))
						m.Param = 0;
				}
				else if (modType == ModType.Xm)
				{
					// Something made be believe that out-of-range global volume commands are ignored in XM
					// just like they are ignored in IT, but apparently they are not. Aaaaaargh!
					if ((((version >= Version.MPT_V._1_17_03_02) && compatPlay) || (version >= Version.MPT_V._1_20_00_00)) && (version < Version.MPT_V._1_24_02_02) && (m.Command == EffectCommand.GlobalVolume) && (m.Param > 64))
						m.Command = EffectCommand.None;

					if ((version < Version.MPT_V._1_19_00_00) || (!compatPlay && (version < Version.MPT_V._1_20_00_00)))
					{
						if ((m.Command == EffectCommand.Offset) && (m.VolCmd == VolumeCommand.TonePortamento))
						{
							// If there are both a portamento and an offset effect, the portamento should be preferred in XM files.
							// OpenMPT 1.19 fixed this in compatible mode, OpenMPT 1.20 fixes it in normal mode as well
							m.Command = EffectCommand.None;
						}
					}

					if ((version < Version.MPT_V._1_20_01_10) && (m.VolCmd == VolumeCommand.TonePortamento) && (m.Command == EffectCommand.TonePortamento) && ((m.Vol != 0) || compatPlay) && (m.Param != 0))
					{
						// Mx and 3xx on the same row does weird things in FT2: 3xx is completely ignored and the Mx parameter is doubled. Fixed in revision 1312 / OpenMPT 1.20.01.10
						// Previously the values were just added up, so let's fix this!
						m.VolCmd = VolumeCommand.None;

						uint16 param = (uint16)(m.Param + (m.Vol << 4));
						m.Param = ModCommandParam.CreateSaturating(param);
					}

					if ((version < Version.MPT_V._1_22_07_09) && (m.Command == EffectCommand.Speed) && (m.Param == 0))
					{
						// OpenMPT can emulate FT2's F00 behaviour now
						m.Command = EffectCommand.None;
					}
				}

				if (version < Version.MPT_V._1_20_00_00)
				{
					// Pattern Delay fixes
					bool fixS6x = (m.Command == EffectCommand.S3MCmdEx) && ((m.Param & 0xf0) == 0x60);

					// We also fix X6x commands in hacked XM files, since they are treated identically to the S6x command in IT/S3M files.
					// We don't treat them in files made with OpenMPT 1.18+ that have compatible play enabled, though, since they are ignored there anyway
					bool fixX6x = (m.Command == EffectCommand.XFinePortaUpDown) && ((m.Param & 0xf0) == 0x60) && (!(compatPlay && (modType == ModType.Xm)) || (version < Version.MPT_V._1_18_00_00));

					if (fixS6x || fixX6x)
					{
						// OpenMPT 1.20 fixes multiple fine pattern delays on the same row. Previously, only the last command was considered,
						// but all commands should be added up. Since Scream Tracker 3 itself doesn't support S6x, we also use Impulse Tracker's behaviour here,
						// since we can assume that most S3Ms that make use of S6x were composed with Impulse Tracker
						for (CPointer<ModCommand> fixCmd_ = arr -curChn; !ReferenceEquals(fixCmd_[0], m); fixCmd_++)
						{
							ModCommand fixCmd = fixCmd_[0];

							if (((fixCmd.Command == EffectCommand.S3MCmdEx) || (fixCmd.Command == EffectCommand.XFinePortaUpDown)) && ((fixCmd.Param & 0xf0) == 0x60))
								fixCmd.Command = EffectCommand.None;
						}
					}

					if ((m.Command == EffectCommand.S3MCmdEx) && ((m.Param & 0xf0) == 0xe0))
					{
						// OpenMPT 1.20 fixes multiple pattern delays on the same row. Previously, only the *last* command was considered,
						// but Scream Tracker 3 and Impulse Tracker only consider the *first* command
						for (CPointer<ModCommand> fixCmd_ = arr -curChn; !ReferenceEquals(fixCmd_[0], m); fixCmd_++)
						{
							ModCommand fixCmd = fixCmd_[0];

							if ((fixCmd.Command == EffectCommand.S3MCmdEx) && ((fixCmd.Param & 0xf0) == 0xe0))
								fixCmd.Command = EffectCommand.None;
						}
					}
				}

				if ((m.VolCmd == VolumeCommand.VibratoDepth) && (version < Version.MPT_V._1_27_00_37) && (version != Version.MPT_V._1_27_00_00))
				{
					// Fix handling of double vibrato commands - previously only one of them was applied at a time
					if ((m.Command == EffectCommand.VibratoVol) && (m.Vol > 0))
						m.Command = EffectCommand.VolumeSlide;
					else if (((m.Command == EffectCommand.Vibrato) || (m.Command == EffectCommand.FineVibrato)) && ((m.Param & 0x0f) == 0))
					{
						m.Command = EffectCommand.Vibrato;
						m.Param |= (uint8)(m.Vol & 0x0f);
						m.VolCmd = VolumeCommand.None;
					}
					else if ((m.Command == EffectCommand.Vibrato) || (m.Command == EffectCommand.VibratoVol) || (m.Command == EffectCommand.FineVibrato))
						m.VolCmd = VolumeCommand.None;
				}

				// Volume column offset in IT/XM is bad, mkay?
				if ((modType != ModType.Mpt) && (m.VolCmd == VolumeCommand.Offset) && (m.Command == EffectCommand.None))
				{
					m.Command = EffectCommand.Offset;
					m.Param = (uint8)(m.Vol << 3);
					m.VolCmd = VolumeCommand.None;
				}

				// Previously CMD_OFFSET simply overrode VOLCMD_OFFSET, now they work together as a combined command
				if ((m.VolCmd == VolumeCommand.Offset) && (m.Command == EffectCommand.Offset) && (version < Version.MPT_V._1_30_00_14))
				{
					if ((m.Param != 0) || (m.Vol == 0))
						m.VolCmd = VolumeCommand.None;
					else
						m.Command = EffectCommand.None;
				}
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public UpgradePatternData MakeDeepClone()
			{
				return (UpgradePatternData)MemberwiseClone();
			}
		}
		#endregion

		#region PlayBehaviourVersion class
		private class PlayBehaviourVersion
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PlayBehaviourVersion(PlayBehaviour behaviour, Version version)
			{
				Behaviour = behaviour;
				Version = version;
			}

			public PlayBehaviour Behaviour { get; }
			public Version Version { get; }
		}
		#endregion

		private static readonly PlayBehaviourVersion[] behaviours1 =
		[
			new PlayBehaviourVersion(PlayBehaviour.TempoClamp, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.PerChannelGlobalVolSlide, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.PanOverride, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItInstrWithoutNote, Version.MPT_V._1_17_02_46),
			new PlayBehaviourVersion(PlayBehaviour.ItVolColFinePortamento, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItArpeggio, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItOutOfRangeDelay, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItPortaMemoryShare, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItPatternLoopTargetReset, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItFt2PatternLoop, Version.MPT_V._1_17_02_49),
			new PlayBehaviourVersion(PlayBehaviour.ItPingPongNoReset, Version.MPT_V._1_17_02_51),
			new PlayBehaviourVersion(PlayBehaviour.ItEnvelopeReset, Version.MPT_V._1_17_02_51),
			new PlayBehaviourVersion(PlayBehaviour.ItClearOldNoteAfterCut, Version.MPT_V._1_17_02_52),
			new PlayBehaviourVersion(PlayBehaviour.ItVibratoTremoloPanbrello, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItTremor, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItRetrigger, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItMultiSampleBehaviour, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItPortaTargetReached, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItPatternLoopBreak, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItOffset, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItSwingBehaviour, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.ItNnaReset, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.ItSCxStopsSample, Version.MPT_V._1_18_00_01),
			new PlayBehaviourVersion(PlayBehaviour.ItEnvelopePositionHandling, Version.MPT_V._1_18_01_00),
			new PlayBehaviourVersion(PlayBehaviour.ItPortamentoInstrument, Version.MPT_V._1_19_00_01),
			new PlayBehaviourVersion(PlayBehaviour.ItPingPongMode, Version.MPT_V._1_19_00_21),
			new PlayBehaviourVersion(PlayBehaviour.ItRealNoteMapping, Version.MPT_V._1_19_00_30),
			new PlayBehaviourVersion(PlayBehaviour.ItHighOffsetNoRetrig, Version.MPT_V._1_20_00_14),
			new PlayBehaviourVersion(PlayBehaviour.ItFilterBehaviour, Version.MPT_V._1_20_00_35),
			new PlayBehaviourVersion(PlayBehaviour.ItNoSurroundPan, Version.MPT_V._1_20_00_53),
			new PlayBehaviourVersion(PlayBehaviour.ItShortSampleRetrig, Version.MPT_V._1_20_00_54),
			new PlayBehaviourVersion(PlayBehaviour.ItPortaNoNote, Version.MPT_V._1_20_00_56),
			new PlayBehaviourVersion(PlayBehaviour.RowDelayWithNoteDelay, Version.MPT_V._1_20_00_76),
			new PlayBehaviourVersion(PlayBehaviour.ItFt2DontResetNoteOffOnPorta, Version.MPT_V._1_20_02_06),
			new PlayBehaviourVersion(PlayBehaviour.ItVolColMemory, Version.MPT_V._1_21_01_16),
			new PlayBehaviourVersion(PlayBehaviour.ItPortamentoSwapResetsPos, Version.MPT_V._1_21_01_25),
			new PlayBehaviourVersion(PlayBehaviour.ItEmptyNoteMapSlot, Version.MPT_V._1_21_01_25),
			new PlayBehaviourVersion(PlayBehaviour.ItFirstTickHandling, Version.MPT_V._1_22_07_09),
			new PlayBehaviourVersion(PlayBehaviour.ItSampleAndHoldPanbrello, Version.MPT_V._1_22_07_19),
			new PlayBehaviourVersion(PlayBehaviour.ItClearPortaTarget, Version.MPT_V._1_23_04_03),
			new PlayBehaviourVersion(PlayBehaviour.ItPanbrelloHold, Version.MPT_V._1_24_01_06),
			new PlayBehaviourVersion(PlayBehaviour.ItPanningReset, Version.MPT_V._1_24_01_06),
			new PlayBehaviourVersion(PlayBehaviour.ItPatternLoopWithJumpsOld, Version.MPT_V._1_25_00_19)
		];


		private static readonly PlayBehaviourVersion[] behaviours2 =
		[
			new PlayBehaviourVersion(PlayBehaviour.TempoClamp, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.PerChannelGlobalVolSlide, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.PanOverride, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.ItFt2PatternLoop, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2Arpeggio, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2Retrigger, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2VolColVibrato, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaNoNote, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2KeyOff, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PanSlide, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2St3OffsetOutOfRange, Version.MPT_V._1_17_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2RestrictXCommand, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.Ft2RetrigWithNoteDelay, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.Ft2SetPanEnvPos, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaIgnoreInstr, Version.MPT_V._1_18_00_01),
			new PlayBehaviourVersion(PlayBehaviour.Ft2VolColMemory, Version.MPT_V._1_18_01_00),
			new PlayBehaviourVersion(PlayBehaviour.Ft2LoopE60Restart, Version.MPT_V._1_18_02_01),
			new PlayBehaviourVersion(PlayBehaviour.Ft2ProcessSilentChannels, Version.MPT_V._1_18_02_01),
			new PlayBehaviourVersion(PlayBehaviour.Ft2ReloadSampleSettings, Version.MPT_V._1_20_00_36),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaDelay, Version.MPT_V._1_20_00_40),
			new PlayBehaviourVersion(PlayBehaviour.Ft2Transpose, Version.MPT_V._1_20_00_62),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PatternLoopWithJumps, Version.MPT_V._1_20_00_69),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaTargetNoReset, Version.MPT_V._1_20_00_69),
			new PlayBehaviourVersion(PlayBehaviour.Ft2EnvelopeEscape, Version.MPT_V._1_20_00_77),
			new PlayBehaviourVersion(PlayBehaviour.Ft2Tremor, Version.MPT_V._1_20_01_11),
			new PlayBehaviourVersion(PlayBehaviour.Ft2OutOfRangeDelay, Version.MPT_V._1_20_02_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2Periods, Version.MPT_V._1_22_03_01),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PanWithDelayedNoteOff, Version.MPT_V._1_22_03_02),
			new PlayBehaviourVersion(PlayBehaviour.Ft2VolColDelay, Version.MPT_V._1_22_07_19),
			new PlayBehaviourVersion(PlayBehaviour.Ft2FineTunePrecision, Version.MPT_V._1_22_07_19)
		];

		private static readonly PlayBehaviourVersion[] behaviours3 =
		[
			new PlayBehaviourVersion(PlayBehaviour.ItInstrWithNoteOff, Version.MPT_V._1_26_00_01),
			new PlayBehaviourVersion(PlayBehaviour.ItMultiSampleInstrumentNumber, Version.MPT_V._1_27_00_27),
			new PlayBehaviourVersion(PlayBehaviour.ItInstrWithNoteOffOldEffects, Version.MPT_V._1_28_02_06),
			new PlayBehaviourVersion(PlayBehaviour.ItDoNotOverrideChannelPan, Version.MPT_V._1_29_00_22),
			new PlayBehaviourVersion(PlayBehaviour.ItPatternLoopWithJumps, Version.MPT_V._1_29_00_32),
			new PlayBehaviourVersion(PlayBehaviour.ItDctBehaviour, Version.MPT_V._1_29_00_57),
			new PlayBehaviourVersion(PlayBehaviour.ItPitchPanSeparation, Version.MPT_V._1_30_00_53),
			new PlayBehaviourVersion(PlayBehaviour.ItResetFilterOnPortaSmpChange, Version.MPT_V._1_30_08_02),
			new PlayBehaviourVersion(PlayBehaviour.ItInitialNoteMemory, Version.MPT_V._1_31_00_25),
			new PlayBehaviourVersion(PlayBehaviour.ItNoSustainOnPortamento, Version.MPT_V._1_32_00_13),
			new PlayBehaviourVersion(PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell, Version.MPT_V._1_32_00_13),
			new PlayBehaviourVersion(PlayBehaviour.ItOffsetWithInstrNumber, Version.MPT_V._1_32_00_15),
			new PlayBehaviourVersion(PlayBehaviour.ItDoublePortamentoSlides, Version.MPT_V._1_32_00_27),
			new PlayBehaviourVersion(PlayBehaviour.ItCarryAfterNoteOff, Version.MPT_V._1_32_00_40),
			new PlayBehaviourVersion(PlayBehaviour.ItNoteCutWithPorta, Version.MPT_V._1_32_01_02),
			new PlayBehaviourVersion(PlayBehaviour.ItVolColNoSlidePropagation, Version.MPT_V._1_32_02_03),
			new PlayBehaviourVersion(PlayBehaviour.ItStoppedFilterEnvAtStart, Version.MPT_V._1_32_03_04)
		];

		private static readonly PlayBehaviourVersion[] behaviours4 =
		[
			new PlayBehaviourVersion(PlayBehaviour.Ft2NoteOffFlags, Version.MPT_V._1_27_00_27),
			new PlayBehaviourVersion(PlayBehaviour.RowDelayWithNoteDelay, Version.MPT_V._1_27_00_37),
			new PlayBehaviourVersion(PlayBehaviour.Ft2ModTremoloRampWaveform, Version.MPT_V._1_27_00_37),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaUpDownMemory, Version.MPT_V._1_27_00_37),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PanSustainRelease, Version.MPT_V._1_28_00_09),
			new PlayBehaviourVersion(PlayBehaviour.Ft2NoteDelayWithoutInstr, Version.MPT_V._1_28_00_44),
			new PlayBehaviourVersion(PlayBehaviour.ItFt2DontResetNoteOffOnPorta, Version.MPT_V._1_29_00_34),
			new PlayBehaviourVersion(PlayBehaviour.Ft2PortaResetDirection, Version.MPT_V._1_30_00_40),
			new PlayBehaviourVersion(PlayBehaviour.Ft2AutoVibratoAbortSweep, Version.MPT_V._1_32_00_29),
			new PlayBehaviourVersion(PlayBehaviour.Ft2OffsetMemoryRequiresNote, Version.MPT_V._1_32_00_43)
		];

		private static readonly PlayBehaviourVersion[] behaviours5 =
		[
			new PlayBehaviourVersion(PlayBehaviour.St3NoMutedChannels, Version.MPT_V._1_18_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3EffectMemory, Version.MPT_V._1_20_00_00),
			new PlayBehaviourVersion(PlayBehaviour.RowDelayWithNoteDelay, Version.MPT_V._1_20_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3PortaSampleChange, Version.MPT_V._1_22_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3VibratoMemory, Version.MPT_V._1_26_00_00),
			new PlayBehaviourVersion(PlayBehaviour.ItPanbrelloHold, Version.MPT_V._1_26_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3PortaAfterArpeggio, Version.MPT_V._1_27_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3OffsetWithoutInstrument, Version.MPT_V._1_28_00_00),
			new PlayBehaviourVersion(PlayBehaviour.St3RetrigAfterNoteCut, Version.MPT_V._1_29_00_00),
			new PlayBehaviourVersion(PlayBehaviour.Ft2St3OffsetOutOfRange, Version.MPT_V._1_29_00_00),
			new PlayBehaviourVersion(PlayBehaviour.ApplyUpperPeriodLimit, Version.MPT_V._1_30_00_45),
			new PlayBehaviourVersion(PlayBehaviour.St3TonePortaWithAdlibNote, Version.MPT_V._1_31_00_13)
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void UpgradeModule()
		{
			if ((m_dwLastSavedWithVersion < Version.MPT_V._1_17_02_46) && (m_dwLastSavedWithVersion != Version.MPT_V._1_17_00_00))
			{
				// Compatible playback mode didn't exist in earlier versions, so definitely disable it
				m_PlayBehaviour.reset(PlayBehaviour.Msf_Compatible_Play);
			}

			bool compatModeIt = m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play] && ((GetType_() & (ModType.It | ModType.Mpt)) != 0);
			bool compatModeXm = m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play] && (GetType_() == ModType.Xm);

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_20_00_00)
			{
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					ModInstrument ins = Instruments[i];

					if (ins == null)
						continue;

					// Previously, volume swing values ranged from 0 to 64. They should reach from 0 to 100 instead
					ins.nVolSwing = (uint8)(Math.Min((uint32)(ins.nVolSwing * 100 / 64), 100U));

					if (!compatModeIt || (m_dwLastSavedWithVersion < Version.MPT_V._1_18_00_00))
					{
						// Previously, Pitch/Pan Separation was only half depth (plot twist: it was actually only quarter depth).
						// This was corrected in compatible mode in OpenMPT 1.18, and in OpenMPT 1.20 it is corrected in normal mode as well
						ins.nPps = (int8)((ins.nPps + (ins.nPps >= 0 ? 1 : -1)) / 2);
					}

					if (!compatModeIt || (m_dwLastSavedWithVersion < Version.MPT_V._1_17_03_02))
					{
						// IT compatibility 24. Short envelope loops
						// Previously, the pitch / filter envelope loop handling was broken, the loop was shortened by a tick (like in XM).
						// This was corrected in compatible mode in OpenMPT 1.17.03.02, and in OpenMPT 1.20 it is corrected in normal mode as well
						ins.GetEnvelope(EnvelopeType.Pitch).Convert(ModType.Xm, GetType_());
					}

					if ((m_dwLastSavedWithVersion >= Version.MPT_V._1_17_00_00) && (m_dwLastSavedWithVersion < Version.MPT_V._1_17_02_50))
					{
						// If there are any plugins that can receive volume commands, enable volume bug emulation
						if ((ins.nMixPlug != 0) && ins.HasValidMidiChannel())
							m_PlayBehaviour.set(PlayBehaviour.MidiCCBugEmulation);
					}

					if ((m_dwLastSavedWithVersion < Version.MPT_V._1_17_02_50) && ((ins.nVolSwing != 0) | (ins.nPanSwing != 0) | (ins.nCutSwing != 0) | (ins.nResSwing != 0)))
					{
						// If there are any instruments with random variation, enable the old random variation behaviour
						m_PlayBehaviour.set(PlayBehaviour.MptOldSwingBehaviour);
						break;
					}
				}

				if (((GetType_() & (ModType.It | ModType.Mpt)) != 0) && ((m_dwLastSavedWithVersion < Version.MPT_V._1_17_03_02) || !compatModeIt))
				{
					// In the IT format, a sweep value of 0 shouldn't apply vibrato at all. Previously, a value of 0 was treated as "no sweep".
					// In OpenMPT 1.17.03.02, this was corrected in compatible mode, in OpenMPT 1.20 it is corrected in normal mode as well,
					// so we have to fix the setting while loading
					for (SampleIndex i = 1; i <= GetNumSamples(); i++)
					{
						if ((Samples[i].nVibSweep == 0) && ((Samples[i].nVibDepth | (Samples[i].nVibRate)) != 0))
							Samples[i].nVibSweep = 255;
					}
				}

				// Fix old nasty broken (non-standard) MIDI configs in files
				m_MidiCfg.UpgradeMacros();
			}

			if ((m_dwLastSavedWithVersion < Version.MPT_V._1_20_02_10) && (m_dwLastSavedWithVersion != Version.MPT_V._1_20_00_00) && ((GetType_() & (ModType.Xm | ModType.It | ModType.Mpt)) != 0))
			{
				bool instrPlugs = false;

				// Old pitch wheel commands were closest to sample pitch bend commands if the PWD is 13
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && (Instruments[i].nMidiChannel != Snd_Def.MidiNoChannel))
					{
						Instruments[i].MidiPwd = 13;
						instrPlugs = true;
					}
				}

				if (instrPlugs)
					m_PlayBehaviour.set(PlayBehaviour.OldMidiPitchBends);
			}

			if ((m_dwLastSavedWithVersion < Version.MPT_V._1_22_03_12) && (m_dwLastSavedWithVersion != Version.MPT_V._1_22_00_00) && ((GetType_() & (ModType.It | ModType.Mpt)) != 0) &&
				(m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play] || m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour]))
			{
				// The "correct" pan swing implementation did nothing if the instrument also had a pan envelope.
				// If there's a pan envelope, disable pan swing for such modules
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && (Instruments[i].nPanSwing != 0) && Instruments[i].PanEnv.dwFlags.Test(EnvelopeFlags.Enabled))
						Instruments[i].nPanSwing = 0;
				}
			}

			// Starting from OpenMPT 1.22.07.19, FT2-style panning was applied in compatible mix mode.
			// Starting from OpenMPT 1.23.01.04, FT2-style panning has its own mix mode instead
			if (GetType_() == ModType.Xm)
			{
				if ((m_dwLastSavedWithVersion >= Version.MPT_V._1_22_07_19) && (m_dwLastSavedWithVersion < Version.MPT_V._1_23_01_04) && (GetMixLevels() == MixLevels.Compatible))
					SetMixLevels(MixLevels.CompatibleFT2);
			}

			if ((m_dwLastSavedWithVersion < Version.MPT_V._1_25_00_07) && (m_dwLastSavedWithVersion != Version.MPT_V._1_25_00_00))
			{
				// Instrument plugins can now receive random volume variation.
				// For old instruments, disable volume swing in case there was no sample associated
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && (Instruments[i].nVolSwing != 0) && (Instruments[i].nMidiChannel != Snd_Def.MidiNoChannel))
					{
						bool hasSample = false;

						foreach (SampleIndex smp in Instruments[i].Keyboard)
						{
							if (smp != 0)
							{
								hasSample = true;
								break;
							}
						}

						if (!hasSample)
							Instruments[i].nVolSwing = 0;
					}
				}
			}

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_26_00_00)
			{
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					ModInstrument ins = Instruments[i];

					if (ins == null)
						continue;

					// Even after fixing it in OpenMPT 1.18, instrument PPS was only half the depth
					ins.nPps = (int8)((ins.nPps + (ins.nPps >= 0 ? 1 : -1)) / 2);

					// OpenMPT 1.18 fixed the depth of random pan in compatible mode.
					// OpenMPT 1.26 fixes it in normal mode too
					if (!compatModeIt || (m_dwLastSavedWithVersion < Version.MPT_V._1_18_00_00))
						ins.nPanSwing = (uint8)((ins.nPanSwing + 3) / 4U);

					// Before OpenMPT 1.26 (r6129), it was possible to trigger MIDI notes using channel plugins if the instrument had a valid MIDI channel
					if ((ins.nMixPlug == 0) && ins.HasValidMidiChannel() && (m_dwLastSavedWithVersion >= Version.MPT_V._1_17_00_00))
						m_PlayBehaviour.set(PlayBehaviour.MidiNotesFromChannelPlugin);
				}
			}

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_28_00_12)
			{
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && (Instruments[i].VolEnv.nReleaseNode != Snd_Def.Env_Release_Node_Unset))
					{
						m_PlayBehaviour.set(PlayBehaviour.LegacyReleaseNode);
						break;
					}
				}
			}

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_28_03_04)
			{
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && ((Instruments[i].PluginVolumeHandling == PlugVolumeHandling.Midi) || (Instruments[i].PluginVolumeHandling == PlugVolumeHandling.DryWet)))
					{
						m_PlayBehaviour.set(PlayBehaviour.MidiVolumeOnNoteOffBug);
						break;
					}
				}
			}

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_30_00_54)
			{
				for (SampleIndex i = 1; i <= GetNumSamples(); i++)
				{
					if (Samples[i].HasSampleData() && Samples[i].uFlags.Test(ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_PingPongSustain))
					{
						m_PlayBehaviour.set(PlayBehaviour.ImprecisePingPongLoops);
						break;
					}
				}
			}

			Patterns.ForEachModCommand(new UpgradePatternData(this));

			// Convert compatibility flags
			// NOTE: Some of these version numbers are just approximations.
			// Sometimes a quirk flag is shared by several code locations which might have been fixed at different times.
			// Sometimes the quirk behaviour has been revised over time, in which case the first version that emulated the quirk enables it
			if (compatModeIt && (m_dwLastSavedWithVersion < Version.MPT_V._1_26_00_00))
			{
				// Pre-1.26: Detailed compatibility flags did not exist
				foreach (PlayBehaviourVersion b in behaviours1)
					m_PlayBehaviour.set(b.Behaviour, (m_dwLastSavedWithVersion >= b.Version) || (m_dwLastSavedWithVersion == b.Version.Masked(0xffff0000U)));
			}
			else if (compatModeXm && (m_dwLastSavedWithVersion < Version.MPT_V._1_26_00_00))
			{
				// Pre-1.26: Detailed compatibility flags did not exist
				foreach (PlayBehaviourVersion b in behaviours2)
					m_PlayBehaviour.set(b.Behaviour, m_dwLastSavedWithVersion >= b.Version);
			}

			if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
			{
				// The following behaviours were added in/after OpenMPT 1.26, so are not affected by the upgrade mechanism above
				foreach (PlayBehaviourVersion b in behaviours3)
				{
					if (m_dwLastSavedWithVersion < b.Version.Masked(0xffff0000U))
						m_PlayBehaviour.reset(b.Behaviour);
					// Full version information available, i.e. not compatibility-exported
					else if ((m_dwLastSavedWithVersion > b.Version.Masked(0xffff0000U)) && (m_dwLastSavedWithVersion < b.Version))
						m_PlayBehaviour.reset(b.Behaviour);
				}
			}
			else if (GetType_() == ModType.Xm)
			{
				// The following behaviours were added after OpenMPT 1.26, so are not affected by the upgrade mechanism above
				foreach (PlayBehaviourVersion b in behaviours4)
				{
					if (m_dwLastSavedWithVersion < b.Version)
						m_PlayBehaviour.reset(b.Behaviour);
				}
			}
			else if (GetType_() == ModType.S3M)
			{
				// We do not store any of these flags in S3M files
				foreach (PlayBehaviourVersion b in behaviours5)
				{
					if (m_dwLastSavedWithVersion < b.Version)
						m_PlayBehaviour.reset(b.Behaviour);
				}
			}

			if ((GetType_() == ModType.Xm) && (m_dwLastSavedWithVersion < Version.MPT_V._1_19_00_00))
			{
				// This bug was introduced sometime between 1.18.03.00 and 1.19.01.00
				m_PlayBehaviour.set(PlayBehaviour.Ft2NoteDelayWithoutInstr);
			}

			if ((m_dwLastSavedWithVersion >= Version.MPT_V._1_27_00_27) && (m_dwLastSavedWithVersion < Version.MPT_V._1_27_00_49))
			{
				// OpenMPT 1.27 inserted some IT/FT2 flags before the S3M flags that are never saved to files anyway, to keep the flag IDs a bit more compact.
				// However, it was overlooked that these flags would still be read by OpenMPT 1.26 and thus S3M-specific behaviour would be enabled in IT/XM files.
				// Hence, in OpenMPT 1.27.00.49 the flag IDs got remapped to no longer conflict with OpenMPT 1.26.
				// Files made with the affected pre-release versions of OpenMPT 1.27 are upgraded here to use the new IDs
				for (c_int i = 0; i < 5; i++)
				{
					m_PlayBehaviour.set(PlayBehaviour.Ft2NoteOffFlags + i, m_PlayBehaviour[PlayBehaviour.St3NoMutedChannels + i]);
					m_PlayBehaviour.reset(PlayBehaviour.St3NoMutedChannels + i);
				}
			}

			if (m_dwLastSavedWithVersion < Version.MPT_V._1_17_00_00)
			{
				// MPT 1.16 has a maximum tempo of 255
				m_PlayBehaviour.set(PlayBehaviour.TempoClamp);
			}
			else if ((m_dwLastSavedWithVersion >= Version.MPT_V._1_17_00_00) && (m_dwLastSavedWithVersion <= Version.MPT_V._1_20_01_03) && (m_dwLastSavedWithVersion != Version.MPT_V._1_20_00_00))
			{
				// OpenMPT introduced some "fixes" that execute regular portamentos also at speed 1
				m_PlayBehaviour.set(PlayBehaviour.SlidesAtSpeed1);
			}

			if (m_SongFlags.Test(SongFlags.LinearSlides))
			{
				if (m_dwLastSavedWithVersion < Version.MPT_V._1_24_00_00)
				{
					// No frequency slides in Hz before OpenMPT 1.24
					m_PlayBehaviour.reset(PlayBehaviour.PeriodsAreHertz);
				}
				else if ((m_dwLastSavedWithVersion >= Version.MPT_V._1_24_00_00) && (m_dwLastSavedWithVersion < Version.MPT_V._1_26_00_00) && ((GetType_() & (ModType.It | ModType.Mpt)) != 0))
				{
					// Frequency slides were always in Hz rather than periods in this version range
					m_PlayBehaviour.set(PlayBehaviour.PeriodsAreHertz);
				}
			}
			else
			{
				if ((m_dwLastSavedWithVersion < Version.MPT_V._1_30_00_36) && (m_dwLastSavedWithVersion != Version.MPT_V._1_30_00_00))
				{
					// No frequency slides in Hz before OpenMPT 1.30
					m_PlayBehaviour.reset(PlayBehaviour.PeriodsAreHertz);
				}
			}

			if (m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling] && (m_dwLastSavedWithVersion >= Version.MPT_V._1_23_01_02) && (m_dwLastSavedWithVersion < Version.MPT_V._1_28_00_43))
			{
				// Bug that effectively clamped the release node to the sustain end
				for (InstrumentIndex i = 1; i <= GetNumInstruments(); i++)
				{
					if ((Instruments[i] != null) && (Instruments[i].VolEnv.nReleaseNode != Snd_Def.Env_Release_Node_Unset) && Instruments[i].VolEnv.dwFlags.Test(EnvelopeFlags.Sustain) && (Instruments[i].VolEnv.nReleaseNode > Instruments[i].VolEnv.nSustainEnd))
					{
						m_PlayBehaviour.set(PlayBehaviour.ReleaseNodePastSustainBug);
						break;
					}
				}
			}

			if ((GetType_() & (ModType.Mpt | ModType.S3M)) != 0)
			{
				for (SampleIndex i = 1; i <= GetNumSamples(); i++)
				{
					if (Samples[i].uFlags.Test(ChannelFlags.Chn_Adlib))
					{
						if ((GetType_() == ModType.Mpt) && (GetNumInstruments() != 0) && (m_dwLastSavedWithVersion >= Version.MPT_V._1_28_00_20) && (m_dwLastSavedWithVersion <= Version.MPT_V._1_29_00_55))
							m_PlayBehaviour.set(PlayBehaviour.OplNoResetAtEnvelopeEnd);

						if ((m_dwLastSavedWithVersion <= Version.MPT_V._1_30_00_34) && (m_dwLastSavedWithVersion != Version.MPT_V._1_30))
							m_PlayBehaviour.reset(PlayBehaviour.OplNoteOffOnNoteChange);

						if ((GetType_() == ModType.S3M) && (m_dwLastSavedWithVersion < Version.MPT_V._1_29))
							m_PlayBehaviour.set(PlayBehaviour.OplRealRetrig);
						else if (GetType_() != ModType.S3M)
							m_PlayBehaviour.reset(PlayBehaviour.OplRealRetrig);

						break;
					}
				}
			}
		}
	}
}

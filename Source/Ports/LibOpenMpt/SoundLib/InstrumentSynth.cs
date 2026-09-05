/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using InstrumentSynthEvents = Polycode.NostalgicPlayer.Kit.C.Std.vector<Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.InstrumentSynth.Event>;

using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class InstrumentSynth : ICopyTo<InstrumentSynth>
	{
		#region Event class
		public class Event : ICopyTo<Event>
		{
			public enum Type : uint8
			{
				/// <summary>
				/// No parameter
				/// </summary>
				StopScript,

				/// <summary>
				/// Parameter: Event index (uint16)
				/// </summary>
				Jump,

				/// <summary>
				/// Parameter: Event index (uint16)
				/// </summary>
				JumpIfTrue,

				/// <summary>
				/// Parameter: Number of ticks (uint16)
				/// </summary>
				Delay,

				/// <summary>
				/// Parameter: Speed (uint8), update speed now? (bool)
				/// </summary>
				SetStepSpeed,

				/// <summary>
				/// Parameter: Marker ID (uint16)
				/// </summary>
				JumpMarker,

				/// <summary>
				/// Parameter: Offset (uint32)
				/// </summary>
				SampleOffset,

				/// <summary>
				/// Parameter: Offset (uint32)
				/// </summary>
				SampleOffsetAdd,

				/// <summary>
				/// Parameter: Offset (uint32)
				/// </summary>
				SampleOffsetSub,

				/// <summary>
				/// Parameter: Count (uint16), force? (bool)
				/// </summary>
				SetLoopCounter,

				/// <summary>
				/// Parameter: Event index (uint16)
				/// </summary>
				EvaluateLoopCounter,

				/// <summary>
				/// No parameter
				/// </summary>
				NoteCut,

				/// <summary>
				/// Parameter: Jump target once key is released (uint16)
				/// </summary>
				Gtk_KeyOff,

				/// <summary>
				/// Parameter: Volume (uint16)
				/// </summary>
				Gtk_SetVolume,

				/// <summary>
				/// Parameter: Pitch (uint16)
				/// </summary>
				Gtk_SetPitch,

				/// <summary>
				/// Parameter: Panning (uint16)
				/// </summary>
				Gtk_SetPanning,

				/// <summary>
				/// Parameter: Step size (int16)
				/// </summary>
				Gtk_SetVolumeStep,

				/// <summary>
				/// Parameter: Step size (int16)
				/// </summary>
				Gtk_SetPitchStep,

				/// <summary>
				/// Parameter: Step size (int16)
				/// </summary>
				Gtk_SetPanningStep,

				/// <summary>
				/// Parameter: Speed (uint8)
				/// </summary>
				Gtk_SetSpeed,

				/// <summary>
				/// Parameter: Enable (uint8)
				/// </summary>
				Gtk_EnableTremor,

				/// <summary>
				/// Parameter: On time (uint8), off time (uint8)
				/// </summary>
				Gtk_SetTremorTime,

				/// <summary>
				/// Parameter: Enable (uint8)
				/// </summary>
				Gtk_EnableTremolo,

				/// <summary>
				/// Parameter: Enable (uint8)
				/// </summary>
				Gtk_EnableVibrato,

				/// <summary>
				/// Parameter: Width (uint8), speed (uint8)
				/// </summary>
				Gtk_SetVibratoParams,

				/// <summary>
				/// Parameter: Waveform (uint8), wavestorm step (uint8), number of waveforms to cycle (uint8)
				/// </summary>
				Puma_SetWaveform,

				/// <summary>
				/// Parameter: Start volume (uint8), end volume (uint8), number of ticks (uint8)
				/// </summary>
				Puma_VolumeRamp,

				/// <summary>
				/// No parameter
				/// </summary>
				Puma_StopVoice,

				/// <summary>
				/// Parameter: Pitch offset (int8), ‹unused› (uint8), number of ticks (uint8)
				/// </summary>
				Puma_SetPitch,

				/// <summary>
				/// Parameter: Start pitch offset (int8), end pitch offset (int8), number of ticks (uint8)
				/// </summary>
				Puma_PitchRamp,

				/// <summary>
				/// Parameter: Source instrument (uint8), waveform (uint8), volume (uint8)
				/// </summary>
				Mupp_SetWaveform,

				/// <summary>
				/// Parameter: Arpeggio note (uint8), arp length or 0 if it's not the first note (uint16)
				/// </summary>
				Med_DefineArpeggio,

				/// <summary>
				/// Parameter: Script index (uint8), jump target (uint16 - JumpMarker ID, not event index!)
				/// </summary>
				Med_JumpScript,

				/// <summary>
				/// Parameter: Envelope index (uint8), loop on/off (uint8), is volume envelope (uint8)
				/// </summary>
				Med_SetEnvelope,

				/// <summary>
				/// Parameter: Volume (uint8)
				/// </summary>
				Med_SetVolume,

				/// <summary>
				/// Parameter: Waveform (uint8)
				/// </summary>
				Med_SetWaveform,

				/// <summary>
				/// Parameter: Speed (uint8)
				/// </summary>
				Med_SetVibratoSpeed,

				/// <summary>
				/// Parameter: Depth (uint8)
				/// </summary>
				Med_SetVibratoDepth,

				/// <summary>
				/// Parameter: Volume step (int16)
				/// </summary>
				Med_SetVolumeStep,

				/// <summary>
				/// Parameter: Period step (int16)
				/// </summary>
				Med_SetPeriodStep,

				/// <summary>
				/// Parameter: Hold time (uint8), decay point (uint16)
				/// </summary>
				Med_HoldDecay,

				/// <summary>
				/// No parameter
				/// </summary>
				Ftm_PlaySample,

				/// <summary>
				/// Parameter: New pitch (uint16)
				/// </summary>
				Ftm_SetPitch,

				/// <summary>
				/// Parameter: Pitch amount (int16)
				/// </summary>
				Ftm_AddPitch,

				/// <summary>
				/// Parameter: Detune amount (uint16)
				/// </summary>
				Ftm_SetDetune,

				/// <summary>
				/// Parameter: Detune amount (int16)
				/// </summary>
				Ftm_AddDetune,

				/// <summary>
				/// Parameter: Channel volume (uint8)
				/// </summary>
				Ftm_SetVolume,

				/// <summary>
				/// Parameter: Volume amount (int16)
				/// </summary>
				Ftm_AddVolume,

				/// <summary>
				/// Parameter: New sample (uint8)
				/// </summary>
				Ftm_SetSample,

				/// <summary>
				/// Parameter: Pitch/volume threshold (uint16), condition type (uint8)
				/// </summary>
				Ftm_SetCondition,

				/// <summary>
				/// Parameter: Jump target (uint16), interrupt type (uint8)
				/// </summary>
				Ftm_SetInterrupt,

				/// <summary>
				/// Parameter: Offset (uint16), modification type (uint8)
				/// </summary>
				Ftm_SetSampleStart,

				/// <summary>
				/// Parameter: Length (uint16), modification type (uint8)
				/// </summary>
				Ftm_SetOneShotLength,

				/// <summary>
				/// Parameter: Length (uint16), modification type (uint8)
				/// </summary>
				Ftm_SetRepeatLength,

				/// <summary>
				/// Parameter: Track (uint8), properties (uint8)
				/// </summary>
				Ftm_CloneTrack,

				/// <summary>
				/// Parameter: LFO index (uint8), target/waveform (uint8)
				/// </summary>
				Ftm_StartLfo,

				/// <summary>
				/// Parameter: LFO/addSub (uint8), speed (uint8), depth (uint8)
				/// </summary>
				Ftm_LfoAddSub,

				/// <summary>
				/// Parameter: Channel index (uint8), is relative? (bool)
				/// </summary>
				Ftm_SetWorkTrack,

				/// <summary>
				/// Parameter: Global volume (uint16)
				/// </summary>
				Ftm_SetGlobalVolume,

				/// <summary>
				/// Parameter: Tempo (uint16)
				/// </summary>
				Ftm_SetTempo,

				/// <summary>
				/// Parameter: Speed (uint16)
				/// </summary>
				Ftm_SetSpeed,

				/// <summary>
				/// Parameter: Pattern to play (uint16), row in pattern (uint8)
				/// </summary>
				Ftm_SetPlayPosition,

				/// <summary>
				/// Parameter: Command type (uint8), waveform (uint8), sample pack (uint8)
				/// </summary>
				Fc_SetWaveform,

				/// <summary>
				/// Parameter: Pitch (int8)
				/// </summary>
				Fc_SetPitch,

				/// <summary>
				/// Parameter: Speed (uint8), depth (uint8), delay (uint8)
				/// </summary>
				Fc_SetVibrato,

				/// <summary>
				/// Parameter: Speed (uint8), time (uint8)
				/// </summary>
				Fc_PitchSlide,

				/// <summary>
				/// Parameter: Speed (uint8), time (uint8)
				/// </summary>
				Fc_VolumeSlide
			}

			public static readonly Type[] JumpEvents =
			[
				Type.Jump, Type.JumpIfTrue, Type.EvaluateLoopCounter,
				Type.Gtk_KeyOff,
				Type.Med_HoldDecay,
				Type.Ftm_SetInterrupt
			];

			public Type type = Type.StopScript;

			public uint8 U8 = 0;
			public int8 I8 => (int8)U8;
			public uint16 U16;
			public int16 I16 => (int16)U16;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Event(Type type, uint8 b1, uint8 b2, uint8 b3)
			{
				this.type = type;
				U8 = b1;
				U16 = (uint16)((b3 << 8) | b2);
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Event(Type type, uint16 u16)
			{
				this.type = type;
				U8 = 0;
				U16 = u16;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Event Jump(uint16 target)
			{
				return new Event(Type.Jump, target);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Event Mupp_SetWaveform(uint8 instr, uint8 waveform, uint8 volume)
			{
				return new Event(Type.Mupp_SetWaveform, instr, waveform, volume);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsJumpEvent()
			{
				return JumpEvents.Contains(type);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint8 Byte0()
			{
				return U8;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint8 Byte1()
			{
				return (uint8)(U16 & 0xff);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint8 Byte2()
			{
				return (uint8)(U16 >> 8);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint32 Value24Bit()
			{
				return (uint32)(Byte0() | (Byte1() << 8) | (Byte2() << 16));
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object into another object
			/// </summary>
			/********************************************************************/
			public void CopyTo(Event destination)
			{
				destination.type = type;
				destination.U8 = U8;
				destination.U16 = U16;
			}
		}
		#endregion

		#region States class
		public class States : IDeepCloneable<States>, ICopyTo<States>
		{
			#region State class
			public class State : IDeepCloneable<State>
			{
				public const uint16 Stop_Row = uint16.MaxValue;

				public enum Flags
				{
					JumpConditionSet,
					GtkTremorEnabled,
					GtkTremorMute,
					GtkTremoloEnabled,
					GtkVibratoEnabled,
					FcVibratoDelaySet,
					FcVibratoStep,
					FcPitchBendStep,
					FcVolumeBendStep,
					NumFlags
				}

				#region Lfo class
				public class Lfo : IDeepCloneable<Lfo>
				{
					public uint8 TargetWaveform = 0;
					public uint8 Speed = 0;
					public uint8 Depth = 0;
					public uint8 Position = 0;

					/********************************************************************/
					/// <summary>
					/// Make a deep copy of the current object
					/// </summary>
					/********************************************************************/
					public Lfo MakeDeepClone()
					{
						return (Lfo)MemberwiseClone();
					}
				}
				#endregion

				public EnumBitSet<Flags> m_Flags = new EnumBitSet<Flags>(Flags.NumFlags);

				public uint16 m_CurrentRow = Stop_Row;
				public uint16 m_NextRow = 0;
				public uint16 m_TicksRemain = 0;
				public uint8 m_StepSpeed = 1;
				public uint8 m_StepsRemain = 0;

				public uint16 m_VolumeFactor = 16384;
				public int16 m_VolumeAdd = int16.MinValue;
				public uint16 m_Panning = 2048;
				public int16 m_LinearPitchFactor = 0;
				public int16 m_PeriodFreqSlide = 0;
				public int16 m_PeriodAdd = 0;
				public uint16 m_LoopCount = 0;

				public uint16 m_GtkKeyOffOffset = Stop_Row;
				public int16 m_GtkVolumeStep = 0;
				public int16 m_GtkPitchStep = 0;
				public int16 m_GtkPanningStep = 0;
				public uint16 m_GtkPitch = 4096;
				public uint8 m_GtkSpeed = 1;
				public uint8 m_GtkSpeedRemain = 1;
				public uint8 m_GtkTremorOnTime = 3;
				public uint8 m_GtkTremorOffTime = 3;
				public uint8 m_GtkTremorPos = 0;
				public uint8 m_GtkVibratoWidth = 0;
				public uint8 m_GtkVibratoSpeed = 0;
				public uint8 m_GtkVibratoPos = 0;

				public uint8 m_PumaStartWaveform = 0;
				public uint8 m_PumaEndWaveform = 0;
				public uint8 m_PumaWaveform = 0;
				public int8 m_PumaWaveformStep = 0;

				public uint8 m_MedVibratoEnvelope = uint8.MaxValue;
				public uint8 m_MedVibratoSpeed = 0;
				public uint8 m_MedVibratoDepth = 0;
				public int8 m_MedVibratoValue = 0;
				public uint16 m_MedVibratoPos = 0;
				public int16 m_MedVolumeStep = 0;
				public int16 m_MedPeriodStep = 0;
				public uint16 m_MedArpOffset = Stop_Row;
				public uint8 m_MedArpPos = 0;
				public uint8 m_MedHold = uint8.MaxValue;
				public uint16 m_MedDecay = Stop_Row;
				public uint8 m_MedVolumeEnv = uint8.MaxValue;
				public uint8 m_MedVolumeEnvPos = 0;

				public uint32 m_FtmSampleStart = 0;
				public int16 m_FtmDetune = 1;
				public uint16 m_FtmVolumeChangeJump = Stop_Row;
				public uint16 m_FtmPitchChangeJump = Stop_Row;
				public uint16 m_FtmSampleChangeJump = Stop_Row;
				public uint16 m_FtmReleaseJump = Stop_Row;
				public uint16 m_FtmVolumeDownJump = Stop_Row;
				public uint16 m_FtmPortamentoJump = Stop_Row;
				public array<Lfo> m_FtmLfo = new array<Lfo>(4);
				public uint8 m_FtmWorkTrack = 0;

				public int8 m_FcPitch = 0;
				public int16 m_FcVibratoValue = 0;
				public uint8 m_FcVibratoDelay = 0;
				public uint8 m_FcVibratoSpeed = 0;
				public uint8 m_FcVibratoDepth = 0;
				public int8 m_FcVolumeBendSpeed = 0;
				public int8 m_FcPitchBendSpeed = 0;
				public uint8 m_FcVolumeBendRemain = 0;
				public uint8 m_FcPitchBendRemain = 0;

				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public ChannelIndex FtmRealChannel(ChannelIndex channel, CSoundFile sndFile)//XX 103
				{
					if (m_FtmWorkTrack != 0)
						return (ChannelIndex)((m_FtmWorkTrack - 1) % sndFile.GetNumChannels());
					else
						return channel;
				}



				/********************************************************************/
				/// <summary>
				///
				/// </summary>
				/********************************************************************/
				public static int32 ApplyLinearPitchSlide(int32 target, int32 totalAmount, bool periodsAreFrequencies)//XX 113
				{
					uint32[] table = (periodsAreFrequencies ^ (totalAmount < 0)) ? Tables.LinearSlideUpTable : Tables.LinearSlideDownTable;
					size_t value = (size_t)CMath.abs(totalAmount);

					while (value > 0)
					{
						size_t amount = Math.Min(value, (size_t)table.Length - 1);
						target = Util.MulDivR(target, (int32)table[amount], 65536);
						value -= amount;
					}

					return target;
				}



				/********************************************************************/
				/// <summary>
				///
				/// </summary>
				/********************************************************************/
				public static int16 TranslateGt2Pitch(uint16 pitch)//XX 127
				{
					// 4096 = normal, 8192 = one octave up
					return SaturateRound.Saturate_Round<int16, c_double>((CMath.log2(8192.0 / Math.Max(pitch, (uint16)1)) - 1.0) * (16 * 12));
				}



				/********************************************************************/
				/// <summary>
				///
				/// </summary>
				/********************************************************************/
				public static int32 TranslateFtmPitch(uint16 pitch, ModChannel chn, CSoundFile sndFile)//XX 134
				{
					int32 period = (int32)sndFile.GetPeriodFromNote((uint32)(ModCommand.Note_MiddleC - 12 + (pitch / 16)), chn.nFineTune, (uint32)chn.nC5Speed);
					sndFile.DoFreqSlide(chn, ref period, (pitch % 16) * 4);

					return period;
				}



				/********************************************************************/
				/// <summary>
				///
				/// </summary>
				/********************************************************************/
				public void JumpToPosition(InstrumentSynthEvents events, uint16 position)//XX 241
				{
					for (size_t pos = 0; pos < events.size(); pos++)
					{
						if ((events[pos].type == Event.Type.JumpMarker) && (events[pos].U16 >= position))
						{
							m_NextRow = (uint16)pos;
							m_TicksRemain = 0;
							return;
						}
					}
				}



				/********************************************************************/
				/// <summary>
				///
				/// </summary>
				/********************************************************************/
				public void NextTick(InstrumentSynthEvents events, PlayState playState, ChannelIndex channel, CSoundFile sndFile, States states)//XX 255
				{
					if (events.empty())
						return;

					ChannelIndex origChannel = channel;
					channel = FtmRealChannel(channel, sndFile);
					ModChannel chn = playState.Chn[channel];

					if ((m_GtkKeyOffOffset != Stop_Row) && chn.dwFlags.Test(SampleFlags.Chn_KeyOff))
					{
						m_NextRow = m_GtkKeyOffOffset;
						m_TicksRemain = 0;
						m_GtkKeyOffOffset = Stop_Row;
					}

					if (m_PumaWaveformStep != 0)
					{
						m_PumaWaveform = (uint8)OpenMpt.Clamp(m_PumaWaveform + m_PumaWaveformStep, (c_int)m_PumaStartWaveform, m_PumaEndWaveform);

						if ((m_PumaWaveform <= m_PumaStartWaveform) || (m_PumaWaveform >= m_PumaEndWaveform))
							m_PumaWaveformStep = (int8)(-m_PumaWaveformStep);

						ChannelSetSample(chn, sndFile, m_PumaWaveform);
					}

					if (m_MedHold != uint8.MaxValue)
					{
						if (m_MedHold-- == 0)
							m_NextRow = m_MedDecay;
					}

					ModCommand m = chn.RowCommand;
					HandleFtmInterrupt(ref m_FtmPitchChangeJump, m.IsNote());
					HandleFtmInterrupt(ref m_FtmVolumeChangeJump, m.Command == ModCommandCommand.ChannelVolume);
					HandleFtmInterrupt(ref m_FtmSampleChangeJump, m.Instr != 0);
					HandleFtmInterrupt(ref m_FtmReleaseJump, m.Note == ModCommand.Note_KeyOff);
					HandleFtmInterrupt(ref m_FtmVolumeDownJump, m.Command == ModCommandCommand.VolumeDown_Duration);
					HandleFtmInterrupt(ref m_FtmPortamentoJump, m.Command == ModCommandCommand.TonePorta_Duration);

					if (!HandleFcVolumeBend() && (m_StepSpeed != 0) && (m_StepsRemain-- == 0))
					{
						// Yep, MED executes this before a potential SPD command may change the step speed on this very row...
						m_StepsRemain = (uint8)(m_StepSpeed - 1);

						if (m_MedVolumeStep != 0)
							m_VolumeFactor = (uint16)Math.Clamp(m_VolumeFactor + m_MedVolumeStep, 0, 16384);

						if (m_MedPeriodStep != 0)
							m_PeriodAdd = int16.CreateSaturating(m_PeriodAdd - m_MedPeriodStep);

						if ((m_MedVolumeEnv != uint8.MaxValue) && (chn.pModInstrument != null))
						{
							m_VolumeFactor = (uint16)Math.Clamp((MedEnvelopeFromSample(chn.pModInstrument, sndFile, (uint8)(m_MedVolumeEnv & 0x7f), m_MedVolumeEnvPos) + 128) * 64, 0, 16384);

							if (m_MedVolumeEnvPos < 127)
								m_MedVolumeEnvPos++;
							else if ((m_MedVolumeEnv & 0x80) != 0)
								m_MedVolumeEnvPos = 0;
						}

						if (m_TicksRemain != 0)
						{
							if (m_CurrentRow < events.size())
								EvaluateRunningEvent(events[m_CurrentRow]);

							m_TicksRemain--;
						}
						else
						{
							uint8 jumpCount = 0;

							while (m_TicksRemain == 0)
							{
								m_CurrentRow = m_NextRow;

								if (m_CurrentRow >= Math.Min(events.size(), Stop_Row))
									break;

								m_NextRow++;

								if (EvaluateEvent(events[m_CurrentRow], playState, channel, sndFile, states))
									break;

								if (events[m_CurrentRow].IsJumpEvent())
								{
									// This smells like an infinite loop
									if (jumpCount++ > 10)
										break;
								}

								channel = FtmRealChannel(origChannel, sndFile);
								chn = playState.Chn[channel];
							}
						}
					}

					// MED stuff
					if ((m_MedArpOffset < events.size()) && (events[m_MedArpOffset].U16 != 0))
					{
						m_LinearPitchFactor = (int16)(16 * events[m_MedArpOffset + m_MedArpPos].U8);
						m_MedArpPos = (uint8)((m_MedArpPos + 1) % events[m_MedArpOffset].U16);
					}

					if (m_MedVibratoDepth != 0)
					{
						uint16 offset = (uint16)(m_MedVibratoPos / 16U);

						if (m_MedVibratoEnvelope == uint8.MaxValue)
							m_MedVibratoValue = Tables.ModSinusTable[(offset * 2) % Tables.ModSinusTable.Length];
						else if (chn.pModInstrument != null)
							m_MedVibratoValue = MedEnvelopeFromSample(chn.pModInstrument, sndFile, m_MedVibratoEnvelope, offset);

						m_MedVibratoPos = (uint16)((m_MedVibratoPos + m_MedVibratoSpeed) % (32U * 16U));
					}

					// GTK stuff
					if ((m_CurrentRow < events.size()) && (m_GtkSpeed != 0) && (--m_GtkSpeedRemain == 0))
					{
						m_GtkSpeedRemain = m_GtkSpeed;

						if (m_GtkVolumeStep != 0)
							m_VolumeFactor = (uint16)Math.Clamp(m_VolumeFactor + m_GtkVolumeStep, 0, 16384);

						if (m_GtkPanningStep != 0)
							m_Panning = (uint16)Math.Clamp(m_Panning + m_GtkPanningStep, 0, 4096);

						if (m_GtkPitchStep != 0)
						{
							m_GtkPitch = (uint16)Math.Clamp(m_GtkPitch + m_GtkPitchStep, 0, 32768);
							m_LinearPitchFactor = TranslateGt2Pitch(m_GtkPitch);
						}
					}

					if (m_Flags[Flags.GtkTremorEnabled])
					{
						if (m_GtkTremorPos >= (m_GtkTremorOnTime + m_GtkTremorOffTime))
							m_GtkTremorPos = 0;

						m_Flags.set(Flags.GtkTremorMute, m_GtkTremorPos >= m_GtkTremorOnTime);
						m_GtkTremorPos++;
					}

					if (m_Flags[Flags.GtkTremoloEnabled])
					{
						m_VolumeAdd = (int16)(Tables.ModSinusTable[(m_GtkVibratoPos / 4U) % Tables.ModSinusTable.Length] * m_GtkVibratoWidth / 2);
						m_GtkVibratoPos += m_GtkVibratoSpeed;
					}

					if (m_Flags[Flags.GtkVibratoEnabled])
					{
						m_PeriodFreqSlide = (int16)(-Tables.ModSinusTable[(m_GtkVibratoPos / 4U) % Tables.ModSinusTable.Length] * m_GtkVibratoWidth / 96);
						m_GtkVibratoPos += m_GtkVibratoSpeed;
					}

					// FTM LFOs
					foreach (Lfo lfo in m_FtmLfo)
					{
						if ((lfo.Speed == 0) && (lfo.Depth == 0))
							continue;

						uint8 lutPos = (uint8)Util.MulDivR_Unsigned(lfo.Position, 256, 192);
						int32 value = 0;

						switch (lfo.TargetWaveform & 0x07)
						{
							case 0:
							{
								value = Tables.ItSinusTable[lutPos];
								break;
							}

							case 1:
							{
								value = lutPos < 128 ? 64 : -64;
								break;
							}

							case 2:
							{
								value = 64 - CMath.abs(((lutPos + 64) % 256) - 128);
								break;
							}

							case 3:
							{
								value = 64 - (lutPos / 2);
								break;
							}

							case 4:
							{
								value = (lutPos / 2) - 64;
								break;
							}
						}

						if ((lfo.TargetWaveform & 0xf0) < 0xa0)
							value += 64;

						value *= lfo.Depth;		// -8192...+8192 or 0...16384 for LFO targets

						switch (lfo.TargetWaveform & 0xf0)
						{
							case 0x10:
							{
								m_FtmLfo[0].Speed = (uint8)(value / 64);
								break;
							}

							case 0x20:
							{
								m_FtmLfo[1].Speed = (uint8)(value / 64);
								break;
							}

							case 0x30:
							{
								m_FtmLfo[2].Speed = (uint8)(value / 64);
								break;
							}

							case 0x40:
							{
								m_FtmLfo[3].Speed = (uint8)(value / 64);
								break;
							}

							case 0x50:
							{
								m_FtmLfo[0].Depth = (uint8)(value / 64);
								break;
							}

							case 0x60:
							{
								m_FtmLfo[1].Depth = (uint8)(value / 64);
								break;
							}

							case 0x70:
							{
								m_FtmLfo[2].Depth = (uint8)(value / 64);
								break;
							}

							case 0x80:
							{
								m_FtmLfo[3].Depth = (uint8)(value / 64);
								break;
							}

							case 0xa0:
							{
								m_VolumeAdd = int16.CreateSaturating(value * 4);
								break;
							}

							case 0xf0:
							{
								m_PeriodFreqSlide = (int16)(value / 8);
								break;
							}
						}

						uint16 newPos = (uint16)(lfo.Position + lfo.Speed);

						if (newPos >= 192)
						{
							newPos -= 192;

							if ((lfo.TargetWaveform & 0x08) != 0)
							{
								lfo.Speed = 0;
								newPos = 191;
							}
						}

						lfo.Position = (uint8)newPos;
					}

					// Future Composer stuff
					if (m_Flags[Flags.FcVibratoDelaySet] && (m_FcVibratoDelay > 0))
						m_FcVibratoDelay--;
					else if (m_FcVibratoDepth != 0)
					{
						if (m_Flags[Flags.FcVibratoStep])
						{
							int16 delta = (int16)(m_FcVibratoDepth * 2);
							m_FcVibratoValue += m_FcVibratoSpeed;

							if (m_FcVibratoValue > delta)
							{
								m_FcVibratoValue = delta;
								m_Flags.flip(Flags.FcVibratoStep);
							}
						}
						else
						{
							m_FcVibratoValue -= m_FcVibratoSpeed;

							if (m_FcVibratoValue < 0)
							{
								m_FcVibratoValue = 0;
								m_Flags.flip(Flags.FcVibratoStep);
							}
						}
					}

					if (m_FcPitchBendRemain != 0)
					{
						m_Flags.flip(Flags.FcPitchBendStep);

						if (m_Flags[Flags.FcPitchBendStep])
						{
							m_FcPitchBendRemain--;
							m_PeriodAdd -= (int16)(m_FcPitchBendSpeed * 4);
						}
					}
				}



				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public void ApplyChannelState(ModChannel chn, ref int32 period, CSoundFile sndFile)//XX 474
				{
					if (m_VolumeFactor != 16384)
						chn.nRealVolume = Util.MulDivR(chn.nRealVolume, m_VolumeFactor, 16384);

					if (m_VolumeAdd != int16.MinValue)
						chn.nRealVolume = Math.Clamp(chn.nRealVolume + m_VolumeAdd, 0, 16384);

					if (m_Flags[Flags.GtkTremorEnabled] && m_Flags[Flags.GtkTremorMute])
						chn.nRealVolume = 0;

					if (m_Panning != 2048)
					{
						if (chn.nRealPan >= 128)
							chn.nRealPan += ((m_Panning - 2048) * (256 - chn.nRealPan)) / 2048;
						else
							chn.nRealPan += ((m_Panning - 2048) * chn.nRealPan) / 2048;
					}

					bool periodsAreFrequencies = sndFile.PeriodsAreFrequencies();

					if (m_LinearPitchFactor != 0)
						period = ApplyLinearPitchSlide(period, m_LinearPitchFactor, periodsAreFrequencies);

					if (m_PeriodFreqSlide != 0)
						sndFile.DoFreqSlide(chn, ref period, m_PeriodFreqSlide);

					if (periodsAreFrequencies)
						period -= m_PeriodAdd;
					else
						period += m_PeriodAdd;

					if (m_MedVibratoDepth != 0)
						period += m_MedVibratoValue * m_MedVibratoDepth / 64;

					int16 vibratoFc = (int16)(m_FcVibratoValue - m_FcVibratoDepth);
					bool doVibratoFc = (vibratoFc != 0) && (m_FcVibratoDelay < 1);

					if ((m_FcPitch != 0) || doVibratoFc)
					{
						uint8 fcNote = (uint8)((m_FcPitch >= 0 ? m_FcPitch + chn.nLastNote - ModCommand.Note_Min : m_FcPitch) & 0x7f);

						if ((m_FcPitch != 0) && ModCommand.IsNote(chn.nLastNote))
							period += (int32)(sndFile.GetPeriodFromNote(chn.pModInstrument.NoteMap[fcNote], chn.nFineTune, (uint32)chn.nC5Speed) - sndFile.GetPeriodFromNote(chn.pModInstrument.NoteMap[chn.nLastNote - ModCommand.Note_Min], chn.nFineTune, (uint32)chn.nC5Speed));

						if (doVibratoFc)
						{
							c_int note = (fcNote * 2) + 160;

							while (note < 256)
							{
								vibratoFc *= 2;
								note += 24;
							}

							period += vibratoFc * 4;
						}
					}

					if (((m_LinearPitchFactor != 0) || (m_PeriodFreqSlide != 0) || (m_PeriodAdd != 0)) && !sndFile.PeriodsAreFrequencies())
					{
						if (period < sndFile.m_nMinPeriod)
							period = sndFile.m_nMinPeriod;
						else if ((period > sndFile.m_nMaxPeriod) && sndFile.m_PlayBehaviour[PlayBehaviour.ApplyUpperPeriodLimit])
							period = sndFile.m_nMaxPeriod;
					}

					if (period < 1)
						period = 1;

					if (m_FtmDetune != 1)
						chn.MicroTuning = m_FtmDetune;
				}



				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public bool EvaluateEvent(Event @event, PlayState playState, ChannelIndex channel, CSoundFile sndFile, States states)//XX 539
				{
					// Return true to indicate end of processing for this tick
					ModChannel chn = playState.Chn[channel];

					switch (@event.type)
					{
						case Event.Type.StopScript:
						{
							m_NextRow = Stop_Row;
							return true;
						}

						case Event.Type.Jump:
						{
							m_NextRow = @event.U16;
							return false;
						}

						case Event.Type.JumpIfTrue:
						{
							if (m_Flags[Flags.JumpConditionSet])
								m_NextRow = @event.U16;

							return false;
						}

						case Event.Type.Delay:
						{
							m_TicksRemain = @event.U16;
							return true;
						}

						case Event.Type.SetStepSpeed:
						{
							m_StepSpeed = @event.U8;

							if (@event.Byte1() != 0)
								m_StepsRemain = (uint8)(m_StepSpeed - 1);

							return false;
						}

						case Event.Type.JumpMarker:
							return false;

						case Event.Type.SampleOffset:
						case Event.Type.SampleOffsetAdd:
						case Event.Type.SampleOffsetSub:
						{
							int64 pos = @event.Value24Bit();

							if (@event.type == Event.Type.SampleOffsetAdd)
								pos += chn.Position.GetInt();
							else if (@event.type == Event.Type.SampleOffsetSub)
								pos = chn.Position.GetInt() - pos;
							else
								pos += m_FtmSampleStart;

							chn.Position.Set((int32)Math.Min(chn.nLength, SmpLength.CreateSaturating(pos)), 0);
							return false;
						}

						case Event.Type.SetLoopCounter:
						{
							if ((m_LoopCount == 0) || (@event.U8 != 0))
								m_LoopCount = (uint16)(1 + Math.Min(@event.U16, (uint16)0xfffe));

							return false;
						}

						case Event.Type.EvaluateLoopCounter:
						{
							if (m_LoopCount > 1)
								m_NextRow = @event.U16;

							if (m_LoopCount != 0)
								m_LoopCount--;

							return false;
						}

						case Event.Type.NoteCut:
						{
							chn.nFadeOutVol = 0;
							chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
							return false;
						}

						case Event.Type.Gtk_KeyOff:
						{
							m_GtkKeyOffOffset = @event.U16;
							return false;
						}

						case Event.Type.Gtk_SetVolume:
						{
							m_VolumeFactor = @event.U16;
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							return false;
						}

						case Event.Type.Gtk_SetPitch:
						{
							m_GtkPitch = @event.U16;
							m_LinearPitchFactor = TranslateGt2Pitch(@event.U16);
							m_PeriodAdd = 0;
							return false;
						}

						case Event.Type.Gtk_SetPanning:
						{
							m_Panning = @event.U16;
							return false;
						}

						case Event.Type.Gtk_SetVolumeStep:
						{
							m_GtkVolumeStep = @event.I16;
							return false;
						}

						case Event.Type.Gtk_SetPitchStep:
						{
							m_GtkPitchStep = @event.I16;
							return false;
						}

						case Event.Type.Gtk_SetPanningStep:
						{
							m_GtkPanningStep = @event.I16;
							return false;
						}

						case Event.Type.Gtk_SetSpeed:
						{
							m_GtkSpeed = m_GtkSpeedRemain = @event.U8;
							return false;
						}

						case Event.Type.Gtk_EnableTremor:
						{
							m_Flags.set(Flags.GtkTremorEnabled, @event.U8 != 0);
							return false;
						}

						case Event.Type.Gtk_SetTremorTime:
						{
							if (@event.Byte0() != 0)
								m_GtkTremorOnTime = @event.Byte0();

							if (@event.Byte1() != 0)
								m_GtkTremorOffTime = @event.Byte1();

							m_GtkTremorPos = 0;
							return false;
						}

						case Event.Type.Gtk_EnableTremolo:
						{
							m_Flags.set(Flags.GtkTremoloEnabled, @event.U8 != 0);
							m_GtkVibratoPos = 0;

							if (m_GtkVibratoWidth == 0)
								m_GtkVibratoWidth = 8;

							if (m_GtkVibratoSpeed == 0)
								m_GtkVibratoSpeed = 16;

							return false;
						}

						case Event.Type.Gtk_EnableVibrato:
						{
							m_Flags.set(Flags.GtkVibratoEnabled, @event.U8 != 0);
							m_PeriodFreqSlide = 0;
							m_GtkVibratoPos = 0;

							if (m_GtkVibratoWidth == 0)
								m_GtkVibratoWidth = 3;

							if (m_GtkVibratoSpeed == 0)
								m_GtkVibratoSpeed = 8;

							return false;
						}

						case Event.Type.Gtk_SetVibratoParams:
						{
							if (@event.Byte0() != 0)
								m_GtkVibratoWidth = @event.Byte0();

							if (@event.Byte1() != 0)
								m_GtkVibratoSpeed = @event.Byte1();

							return false;
						}

						case Event.Type.Puma_SetWaveform:
						{
							m_PumaWaveform = m_PumaStartWaveform = (uint8)(@event.Byte0() + 1);

							if (@event.Byte0() < 10)
								m_PumaWaveformStep = 0;
							else
							{
								m_PumaWaveformStep = (int8)@event.Byte1();
								m_PumaEndWaveform = (uint8)(@event.Byte2() + m_PumaWaveform);
							}

							ChannelSetSample(chn, sndFile, m_PumaWaveform);
							return false;
						}

						case Event.Type.Puma_VolumeRamp:
						{
							m_TicksRemain = @event.Byte2();
							m_VolumeAdd = (int16)((@event.Byte0() * 256) - 16384);
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							return true;
						}

						case Event.Type.Puma_StopVoice:
						{
							chn.Stop();
							m_NextRow = Stop_Row;
							return true;
						}

						case Event.Type.Puma_SetPitch:
						{
							m_LinearPitchFactor = (int16)(@event.I8 * 8);
							m_PeriodAdd = 0;
							m_TicksRemain = (uint16)(Math.Max(@event.Byte2(), 1U) - 1);
							return true;
						}

						case Event.Type.Puma_PitchRamp:
						{
							m_LinearPitchFactor = 0;
							m_PeriodAdd = (int16)(@event.I8 * 4);
							m_TicksRemain = (uint16)(Math.Max(@event.Byte2(), 1U) - 1);
							return true;
						}

						case Event.Type.Mupp_SetWaveform:
						{
							ChannelSetSample(chn, sndFile, (SampleIndex)(32 + (@event.Byte0() * 28) + @event.Byte1()));
							m_VolumeFactor = (uint16)(Math.Min(@event.Byte2() & 0x7f, 64) * 256U);
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							return true;
						}

						case Event.Type.Med_DefineArpeggio:
						{
							if (@event.U16 == 0)
								return false;

							m_NextRow = (uint16)(m_CurrentRow + @event.U16);
							m_MedArpOffset = m_CurrentRow;
							m_MedArpPos = 0;
							return true;
						}

						case Event.Type.Med_JumpScript:
						{
							if ((@event.U8 < chn.SynthState.states.size()) && (chn.pModInstrument != null) && (@event.U8 < chn.pModInstrument.Synth.m_Scripts.size()))
							{
								chn.SynthState.states[@event.U8].JumpToPosition(chn.pModInstrument.Synth.m_Scripts[@event.U8], @event.U16);
								chn.SynthState.states[@event.U8].m_StepsRemain = 0;
							}

							return false;
						}

						case Event.Type.Med_SetEnvelope:
						{
							if (@event.Byte2() != 0)
								m_MedVolumeEnv = (uint8)((@event.Byte0() & 0x3f) | (@event.Byte1() != 0 ? 0x80 : 0x00));
							else
								m_MedVibratoEnvelope = @event.Byte0();

							m_MedVolumeEnvPos = 0;
							return false;
						}

						case Event.Type.Med_SetVolume:
						{
							m_VolumeFactor = (uint16)(@event.U8 * 256U);
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							return true;
						}

						case Event.Type.Med_SetWaveform:
						{
							if (chn.pModInstrument != null)
								ChannelSetSample(chn, sndFile, (SampleIndex)(chn.pModInstrument.Keyboard[ModCommand.Note_MiddleC - ModCommand.Note_Min] + @event.U8));

							return true;
						}

						case Event.Type.Med_SetVibratoSpeed:
						{
							m_MedVibratoSpeed = @event.U8;
							return false;
						}

						case Event.Type.Med_SetVibratoDepth:
						{
							m_MedVibratoDepth = @event.U8;
							return false;
						}

						case Event.Type.Med_SetVolumeStep:
						{
							m_MedVolumeStep = (int16)(@event.I16 * 256);
							return false;
						}

						case Event.Type.Med_SetPeriodStep:
						{
							m_MedPeriodStep = (int16)(@event.I16 * 4);
							return false;
						}

						case Event.Type.Med_HoldDecay:
						{
							m_MedHold = @event.U8;
							m_MedDecay = @event.U16;
							return false;
						}

						case Event.Type.Ftm_SetCondition:
						{
							int32 threshold = @event.U8 < 3 ? (int32.MaxValue - TranslateFtmPitch(@event.U16, chn, sndFile)) : @event.U16;
							int32 compare = @event.U8 < 3 ? (int32.MaxValue - chn.nPeriod) : chn.nGlobalVol;

							switch (@event.U8 % 3U)
							{
								case 0:
								{
									m_Flags.set(Flags.JumpConditionSet, compare == threshold);
									break;
								}

								case 1:
								{
									m_Flags.set(Flags.JumpConditionSet, compare < threshold);
									break;
								}

								case 2:
								{
									m_Flags.set(Flags.JumpConditionSet, compare > threshold);
									break;
								}
							}

							return false;
						}

						case Event.Type.Ftm_SetInterrupt:
						{
							if ((@event.U8 & 0x01) != 0)
								m_FtmPitchChangeJump = @event.U16;

							if ((@event.U8 & 0x02) != 0)
								m_FtmVolumeChangeJump = @event.U16;

							if ((@event.U8 & 0x04) != 0)
								m_FtmSampleChangeJump = @event.U16;

							if ((@event.U8 & 0x08) != 0)
								m_FtmReleaseJump = @event.U16;

							if ((@event.U8 & 0x10) != 0)
								m_FtmPortamentoJump = @event.U16;

							if ((@event.U8 & 0x20) != 0)
								m_FtmVolumeDownJump = @event.U16;

							return false;
						}

						case Event.Type.Ftm_PlaySample:
						{
							if ((chn.nNewIns > 0) && (chn.nNewIns <= sndFile.GetNumSamples()))
								chn.pModSample = sndFile.GetSample(chn.nNewIns);

							if (chn.pModSample != null)
							{
								ModSample sample = chn.pModSample;
								chn.nVolume = sample.nVolume;
								chn.UpdateInstrumentVolume(sample, null);
								chn.nC5Speed = (int32)sample.nC5Speed;
								chn.dwFlags = (chn.dwFlags & (Snd_Def.Chn_ChannelFlags ^ ChannelFlags.Chn_NoteFade)) | (sample.uFlags & Snd_Def.Smp_FlagsMask);
								chn.nLength = chn.pModSample.uFlags.Test(ChannelFlags.Chn_Loop) ? sample.nLoopEnd : sample.nLength;
								chn.nLoopStart = sample.nLoopStart;
								chn.nLoopEnd = sample.nLoopEnd;
							}

							chn.Position.Set(0);
							return false;
						}

						case Event.Type.Ftm_SetPitch:
						{
							chn.nPeriod = TranslateFtmPitch((uint16)(@event.U16 * 2), chn, sndFile);
							return false;
						}

						case Event.Type.Ftm_SetDetune:
						{
							// Detune always applies to the first channel of a channel pair (and only if the other channel is playing a sample)
							states.states[channel & ~1].m_FtmDetune = (int16)(@event.U16 * -8);
							return false;
						}

						case Event.Type.Ftm_AddDetune:
						{
							states.states[channel & ~1].m_FtmDetune -= (int16)(@event.I16 * 8);
							return false;
						}

						case Event.Type.Ftm_AddPitch:
						{
							if (@event.I16 != 0)
							{
								sndFile.DoFreqSlide(chn, ref chn.nPeriod, @event.I16 * 8);
								int32 limit = TranslateFtmPitch((uint16)(@event.I16 < 0 ? 0 : 0x21e), chn, sndFile);

								if ((@event.I16 > 0) == sndFile.PeriodsAreFrequencies())
									chn.nPeriod = Math.Min(chn.nPeriod, limit);
								else
									chn.nPeriod = Math.Max(chn.nPeriod, limit);
							}

							return false;
						}

						case Event.Type.Ftm_SetVolume:
						{
							chn.nGlobalVol = Math.Min(@event.U8, (uint8)64);
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							return false;
						}

						case Event.Type.Ftm_AddVolume:
						{
							chn.nGlobalVol = (uint8)Math.Clamp(chn.nGlobalVol + @event.I16, 0, 64);
							return false;
						}

						case Event.Type.Ftm_SetSample:
						{
							chn.SwapSampleIndex = (uint16)(@event.U8 + 1);
							return false;
						}

						case Event.Type.Ftm_SetSampleStart:
						{
							// Documentation says this should be in words, but it really appears to work with bytes.
							// The relative variants appear to be completely broken
							if (@event.U8 == 1)
								m_FtmSampleStart += Math.Min(@event.U16, uint32.MaxValue - m_FtmSampleStart);
							else if (@event.U8 == 2)
								m_FtmSampleStart -= Math.Min(@event.U16, m_FtmSampleStart);
							else
								m_FtmSampleStart = @event.U16 * 2U;

							return false;
						}

						case Event.Type.Ftm_SetOneShotLength:
						{
							if (chn.pModSample != null)
							{
								SmpLength loopLength = chn.nLoopEnd - chn.nLoopStart;
								int64 loopStart = @event.U16 * 2;

								if (@event.U8 == 1)
									loopStart += chn.nLoopStart;
								else if (@event.U8 == 2)
									loopStart = chn.nLoopStart - loopStart;

								loopStart = Math.Clamp(loopStart, 0, chn.pModSample.nLength);
								chn.nLoopStart = (SmpLength)loopStart;
								chn.nLoopEnd = chn.nLoopStart + loopLength;
								OpenMpt.LimitMax(ref chn.nLoopEnd, chn.pModSample.nLength);
								chn.nLength = chn.nLoopEnd;
								chn.dwFlags.Set(ChannelFlags.Chn_Loop, chn.nLoopEnd > chn.nLoopStart);

								if ((chn.Position.GetUInt() >= chn.nLength) && chn.dwFlags.Test(ChannelFlags.Chn_Loop))
									chn.Position.SetInt((int32)chn.nLoopStart);
							}

							return false;
						}

						case Event.Type.Ftm_SetRepeatLength:
						{
							if (chn.pModSample != null)
							{
								int64 loopEnd = chn.nLoopStart + (@event.U16 * 2);

								if (@event.U8 == 1)
									loopEnd = chn.nLoopEnd + (@event.U16 * 2);
								else if (@event.U8 == 2)
									loopEnd = chn.nLoopEnd - (@event.U16 * 2);

								loopEnd = Math.Clamp(loopEnd, chn.nLoopStart, chn.pModSample.nLength);
								chn.nLoopEnd = (SmpLength)loopEnd;
								chn.nLength = chn.nLoopEnd;
								chn.dwFlags.Set(ChannelFlags.Chn_Loop, chn.nLoopEnd > chn.nLoopStart);

								if ((chn.Position.GetUInt() >= chn.nLength) && chn.dwFlags.Test(ChannelFlags.Chn_Loop))
									chn.Position.SetInt((int32)chn.nLoopStart);
							}

							return false;
						}

						case Event.Type.Ftm_CloneTrack:
						{
							if (@event.Byte0() < sndFile.GetNumChannels())
							{
								ModChannel srcChn = playState.Chn[@event.Byte0()];

								if ((@event.Byte1() & (0x01 | 0x08)) != 0)
									chn.nPeriod = srcChn.nPeriod;

								if ((@event.Byte1() & (0x02 | 0x08)) != 0)
									chn.nGlobalVol = srcChn.nGlobalVol;

								if ((@event.Byte1() & (0x04 | 0x08)) != 0)
								{
									chn.nNewIns = srcChn.nNewIns;
									chn.SwapSampleIndex = srcChn.SwapSampleIndex;
									chn.pModSample = srcChn.pModSample;
									chn.Position = srcChn.Position;
									chn.dwFlags = (chn.dwFlags & Snd_Def.Chn_ChannelFlags) | (srcChn.dwFlags & Snd_Def.Chn_SampleFlags);
									chn.nLength = srcChn.nLength;
									chn.nLoopStart = srcChn.nLoopStart;
									chn.nLoopEnd = srcChn.nLoopEnd;
								}

								if ((@event.Byte1() & 0x08) != 0)
								{
									// Note: This does not appear to behave entirely as documented.
									// When the command is triggered, it copies frequency, volume, sample and the state of running slide commands.
									// Running LFOs are not copied. But any notes and effects (including newly triggered LFOs) on the source track on following rows are copied.
									// There appears to be no way to stop this cloning once it has started.
									// As no FTM in the wild makes use of this command, we will glance over this ugly detail
									chn.Position = srcChn.Position;
									chn.PortamentoSlide = srcChn.PortamentoSlide;
									chn.nPortamentoDest = srcChn.nPortamentoDest;
									chn.VolSlideDownStart = srcChn.VolSlideDownStart;
									chn.VolSlideDownTotal = srcChn.VolSlideDownTotal;
									chn.VolSlideDownRemain = srcChn.VolSlideDownRemain;
									chn.AutoSlide.SetActive(AutoSlideCommand.TonePortamentoWithDuration, srcChn.AutoSlide.IsActive(AutoSlideCommand.TonePortamentoWithDuration));
									chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownWithDuration, srcChn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownWithDuration));
								}
							}

							return false;
						}

						case Event.Type.Ftm_StartLfo:
						{
							Lfo lfo = m_FtmLfo[@event.Byte0() & 3];
							lfo.TargetWaveform = @event.Byte1();
							lfo.Speed = lfo.Depth = lfo.Position = 0;
							return false;
						}

						case Event.Type.Ftm_LfoAddSub:
						{
							Lfo lfo = m_FtmLfo[@event.Byte0() & 3];
							c_int factor = (@event.Byte0() & 4) != 0 ? -1 : 1;
							lfo.Speed = Math.Min(uint8.CreateSaturating(lfo.Speed + (@event.Byte1() * factor)), (uint8)0xbf);
							lfo.Depth = Math.Min(uint8.CreateSaturating(lfo.Depth + (@event.Byte2() * factor)), (uint8)0x7f);
							return false;
						}

						case Event.Type.Ftm_SetWorkTrack:
						{
							if (@event.Byte0() == uint8.MaxValue)
								m_FtmWorkTrack = 0;
							else
							{
								bool isRelative = @event.Byte1() != 0;

								if (isRelative && (@event.Byte0() != 0))
								{
									if (m_FtmWorkTrack == 0)
										m_FtmWorkTrack = (uint8)(channel + 1);

									m_FtmWorkTrack = (uint8)(((m_FtmWorkTrack - 1U + @event.Byte0()) % sndFile.GetNumChannels()) + 1);
								}
								else if (!isRelative)
									m_FtmWorkTrack = (uint8)(@event.Byte0() + 1);
							}

							return false;
						}

						case Event.Type.Ftm_SetGlobalVolume:
						{
							playState.m_nGlobalVolume = @event.U16;
							return false;
						}

						case Event.Type.Ftm_SetTempo:
						{
							playState.m_nMusicTempo = new Tempo(1777517.482 / Math.Clamp(@event.U16, (uint16)0x1000, (uint16)0x4fff));
							return false;
						}

						case Event.Type.Ftm_SetSpeed:
						{
							if (@event.U16 != 0)
								playState.m_nMusicSpeed = @event.U16;
							else
								playState.m_nMusicSpeed = uint16.MaxValue;

							return false;
						}

						case Event.Type.Ftm_SetPlayPosition:
						{
							OrderIndex playPos = sndFile.Order.Current.FindOrder(@event.U16, @event.U16);

							if (playPos != Snd_Def.OrderIndex_Invalid)
							{
								playState.m_nNextOrder = playPos;
								playState.m_nNextRow = @event.U8;
							}

							return false;
						}

						case Event.Type.Fc_SetWaveform:
						{
							uint8 waveform = (uint8)(@event.Byte1() + 1);

							if (@event.Byte0() == 0xe9)
								waveform += (uint8)((@event.Byte2() * 10) + 90);

							ChannelSetSample(chn, sndFile, waveform, @event.Byte0() == 0xe4);
							return false;
						}

						case Event.Type.Fc_SetPitch:
						{
							m_FcPitch = @event.I8;
							return true;
						}

						case Event.Type.Fc_SetVibrato:
						{
							m_FcVibratoSpeed = @event.Byte0();
							m_FcVibratoDepth = @event.Byte1();

							if (!m_Flags[Flags.FcVibratoDelaySet])
							{
								m_Flags.set(Flags.FcVibratoDelaySet);
								m_FcVibratoDelay = @event.Byte2();
								m_FcVibratoValue = m_FcVibratoDepth;
							}

							return false;
						}

						case Event.Type.Fc_PitchSlide:
						{
							m_FcPitchBendSpeed = (int8)@event.Byte0();
							m_FcPitchBendRemain = @event.Byte1();
							return false;
						}

						case Event.Type.Fc_VolumeSlide:
						{
							m_FcVolumeBendSpeed = (int8)@event.Byte0();
							m_FcVolumeBendRemain = @event.Byte1();

							HandleFcVolumeBend(true);
							return true;
						}
					}

					return false;
				}



				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public void EvaluateRunningEvent(Event @event)//XX 975
				{
					switch (@event.type)
					{
						case Event.Type.Puma_VolumeRamp:
						{
							if (@event.Byte2() > 0)
								m_VolumeAdd = (int16)(((@event.Byte1() + Util.MulDivR(@event.Byte0() - @event.Byte1(), m_TicksRemain, @event.Byte2())) * 256) - 16384);

							break;
						}

						case Event.Type.Puma_PitchRamp:
						{
							if (@event.Byte2() > 0)
								m_PeriodAdd = (int16)(((int8)@event.Byte1() + Util.MulDivR((int8)@event.Byte0() - (int8)@event.Byte1(), m_TicksRemain, @event.Byte2())) * 4);

							break;
						}
					}
				}



				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public void HandleFtmInterrupt(ref uint16 target, bool condition)//XX 993
				{
					if ((target == Stop_Row) || !condition)
						return;

					m_NextRow = target;
					m_TicksRemain = 0;
					m_StepsRemain = 0;
					target = Stop_Row;
				}



				/********************************************************************/
				/// <summary>
				/// 
				/// </summary>
				/********************************************************************/
				public bool HandleFcVolumeBend(bool forceRun = false)//XX 1004
				{
					if ((m_FcVolumeBendRemain == 0) && !forceRun)
						return false;

					m_Flags.flip(Flags.FcVolumeBendStep);

					if (m_Flags[Flags.FcVolumeBendStep])
					{
						m_FcVolumeBendRemain--;
						int32 target = m_VolumeFactor + (m_FcVolumeBendSpeed * 256);

						if ((target < 0) || (target >= 32768))
							m_FcVolumeBendRemain = 0;

						m_VolumeFactor = (uint16)Math.Clamp(target, 0, 16384);
					}

					return true;
				}



				/********************************************************************/
				/// <summary>
				/// Make a deep copy of the current object
				/// </summary>
				/********************************************************************/
				public State MakeDeepClone()
				{
					State clone = (State)MemberwiseClone();

					clone.m_Flags = m_Flags.MakeDeepClone();
					clone.m_FtmLfo = m_FtmLfo.MakeDeepClone();

					return clone;
				}
			}
			#endregion

			protected vector<State> states = new vector<State>();

			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public void NextTick(PlayState playState, ChannelIndex channel, CSoundFile sndFile)//XX 198
			{
				ModChannel chn = playState.Chn[channel];

				if ((chn.pModInstrument == null) || !chn.pModInstrument.Synth.HasScripts())
					return;

				vector<InstrumentSynthEvents> scripts = chn.pModInstrument.Synth.m_Scripts;
				states.resize(scripts.size());

				for (size_t i = 0; i < scripts.size(); i++)
				{
					InstrumentSynthEvents script = scripts[i];
					State state = states[i];

					if (chn.TriggerNote)
						state = states[i] = new State();

					if ((i == 1) && (chn.RowCommand.Command == EffectCommand.Med_Synth_Jump) && chn.IsFirstTick)
					{
						// Ugly special case: If the script didn't run yet (not triggered on same row), we need to run at least the first SetStepSpeed command
						if ((state.m_NextRow == 0) && !script.empty() && (script[0].type == Event.Type.SetStepSpeed))
						{
							state.EvaluateEvent(script[0], playState, channel, sndFile, this);
							state.m_StepsRemain = 0;
						}

						state.JumpToPosition(script, chn.RowCommand.Param);
					}

					state.NextTick(script, playState, channel, sndFile, this);
				}
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void ApplyChannelState(ModChannel chn, ref int32 period, CSoundFile sndFile)//XX 229
			{
				if ((chn.pModInstrument == null) || !chn.pModInstrument.Synth.HasScripts())
					return;

				foreach (State state in states)
					state.ApplyChannelState(chn, ref period, sndFile);
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public virtual States MakeDeepClone()
			{
				States clone = new States();

				clone.states = states.MakeDeepClone();

				return clone;
			}



			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object into another object
			/// </summary>
			/********************************************************************/
			public void CopyTo(States destination)
			{
				states.CopyTo(destination.states);
			}
		}
		#endregion

		public readonly vector<InstrumentSynthEvents> m_Scripts = new vector<InstrumentSynthEvents>();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasScripts()
		{
			return !m_Scripts.empty();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Sanitize()//XX 1059
		{
			foreach (InstrumentSynthEvents script in m_Scripts)
			{
				if (script.size() >= States.State.Stop_Row)
					script.resize(States.State.Stop_Row - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(InstrumentSynth destination)
		{
			m_Scripts.CopyTo(destination.m_Scripts);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int8 MedEnvelopeFromSample(ModInstrument instr, CSoundFile sndFile, uint8 envelope, uint16 envelopePos)//XX 142
		{
			SampleIndex smp = (SampleIndex)(instr.Keyboard[ModCommand.Note_MiddleC - ModCommand.Note_Min] + envelope);

			if ((smp < 1) || (smp > sndFile.GetNumSamples()))
				return 0;

			ModSample mptSmp = sndFile.GetSample(smp);

			if ((envelopePos >= mptSmp.nLength) || mptSmp.uFlags.Test(ChannelFlags.Chn_16Bit) || mptSmp.Sample8().IsNull)
				return 0;

			return mptSmp.Sample8()[envelopePos];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ChannelSetSample(ModChannel chn, CSoundFile sndFile, SampleIndex smp, bool swapAtEnd = true)//XX 156
		{
			if ((smp < 1) || (smp > sndFile.GetNumSamples()))
				return;

			bool channelIsActive = (chn.pCurrentSample != null) && (chn.nLength != 0);

			if (sndFile.m_PlayBehaviour[PlayBehaviour.ModSampleSwap] && (smp <= uint8.MaxValue) && swapAtEnd && channelIsActive)
			{
				chn.SwapSampleIndex = smp;
				return;
			}

			ModSample sample = sndFile.GetSample(smp);

			if ((chn.pModSample == sample) && channelIsActive)
				return;

			if (chn.Increment.IsZero() && (chn.nLength == 0) && (chn.nVolume == 0))
				chn.nVolume = 256;

			chn.pModSample = sample;
			chn.pCurrentSample = sample.SampleEv();
			chn.dwFlags = (chn.dwFlags & Snd_Def.Chn_ChannelFlags) | (sample.uFlags & Snd_Def.Smp_FlagsMask);
			chn.nLength = sample.uFlags.Test(ChannelFlags.Chn_Loop) ? sample.nLoopEnd : sample.nLength;
			chn.nLoopStart = sample.nLoopStart;
			chn.nLoopEnd = sample.nLoopEnd;

			if (chn.Position.GetUInt() >= chn.nLength)
				chn.Position.Set(0);
		}
		#endregion
	}
}

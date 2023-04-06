/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Implementation
{
	/// <summary>
	/// DBM play code
	/// </summary>
	internal class Player
	{
		private ModuleSynth modSynth;
		private DigiBoosterProWorker worker;
		private EffectMaster effectMaster;

		private bool endReached;
		private bool restartSong;
		private int lastPosition;

		private const int InfoPositionLine = 4;
		private const int InfoPatternLine = 5;
		private const int InfoSpeedLine = 6;
		private const int InfoTempoLine = 7;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Player(DB3Module m, DigiBoosterProWorker worker)
		{
			this.worker = worker;

			modSynth = new ModuleSynth();

			modSynth.Tracks = ArrayHelper.InitializeArray<ModuleTrack>(m.NumberOfTracks);
			modSynth.Module = m;
		}



		/********************************************************************/
		/// <summary>
		/// Creates and setups a new module synthesizer
		/// </summary>
		/********************************************************************/
		public static Player NewEngine(DB3Module m, DigiBoosterProWorker worker)
		{
			return new Player(m, worker);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			effectMaster = new EffectMaster(new EchoArguments
			{
				EchoDelay = modSynth.Module.DspDefaults.EchoDelay,
				EchoFeedback = modSynth.Module.DspDefaults.EchoFeedback,
				EchoMix = modSynth.Module.DspDefaults.EchoMix,
				EchoCross = modSynth.Module.DspDefaults.EchoCross
			});

			MSynth_Reset(modSynth);

			endReached = false;
			restartSong = false;
			lastPosition = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public ushort GetSongLength()
		{
			return modSynth.Module.Songs[modSynth.Song].NumberOfOrders;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the current sequencer position
		/// </summary>
		/********************************************************************/
		public int GetPosition()
		{
			return modSynth.Order;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the current pattern number
		/// </summary>
		/********************************************************************/
		public int GetPattern()
		{
			return modSynth.Pattern;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the current sequencer position
		/// </summary>
		/********************************************************************/
		public void SetPosition(uint32_t order, uint32_t row)
		{
			modSynth.Order = (int)order;
			modSynth.Pattern = modSynth.Module.Songs[modSynth.Song].PlayList[order];
			modSynth.Row = (int)row;
			modSynth.Tick = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the song to use
		/// </summary>
		/********************************************************************/
		public void SetSong(int32_t song)
		{
			modSynth.Song = song;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the current speed
		/// </summary>
		/********************************************************************/
		public int GetSpeed()
		{
			return modSynth.Speed;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the current tempo
		/// </summary>
		/********************************************************************/
		public int GetTempo()
		{
			return modSynth.Tempo;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the effect master
		/// </summary>
		/********************************************************************/
		public IEffectMaster GetEffectMaster()
		{
			return effectMaster;
		}



		/********************************************************************/
		/// <summary>
		/// Mix the next chunk of module
		/// </summary>
		/********************************************************************/
		public void Mix()
		{
			MSynth_Next_Tick(modSynth);

			for (int track = 0; track < modSynth.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = modSynth.Tracks[track];

				if (!mt.IsOn)
					worker.VirtualChannels[track].Mute();
			}

			if (endReached)
			{
				worker.ModuleEnded();
				endReached = false;

				if (restartSong)
				{
					worker.Restart();
					restartSong = false;
				}
				else
				{
					if (modSynth.GlobalVolume == 0)
					{
						for (int track = 0; track < modSynth.Module.NumberOfTracks; track++)
							worker.VirtualChannels[track].Mute();
					}

					modSynth.GlobalVolume = 64;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		public void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPattern();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		public ISnapshot CreateSnapshot()
		{
			return new Snapshot(modSynth, effectMaster);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		public void SetSnapshot(ISnapshot snapshot)
		{
			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.ModuleSynth, currentSnapshot.EffectMaster);

			modSynth = clonedSnapshot.ModuleSynth;
			effectMaster = clonedSnapshot.EffectMaster;
		}



		/********************************************************************/
		/// <summary>
		/// Disposes a module synthesizer
		/// </summary>
		/********************************************************************/
		public void DisposeEngine()
		{
			modSynth = null;
			worker = null;
			effectMaster = null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool PortamentoToNote(DB3ModuleEntry me)
		{
			return (me.Command1 == Effect.PortamentoToNote) || (me.Command1 == Effect.PortamentoToNoteVolumeSlide) || (me.Command2 == Effect.PortamentoToNote) || (me.Command2 == Effect.PortamentoToNoteVolumeSlide);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8_t Bcd2Bin(uint8_t x)
		{
			uint8_t r;

			if ((x >> 4) < 10)
				r = (uint8_t)((x >> 4) * 10);
			else
				return 0;

			if ((x & 0x0f) < 10)
				r += (uint8_t)(x & 0x0f);
			else
				return 0;

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Trigger(DB3Module m, ModuleTrack mt, int channel)
		{
			mt.VolumeEnvelopeCurrent = 16384;
			mt.PanningEnvelopeCurrent = 0;

			// Reset envelopes interpolators
			mt.VolumeEnvelope.Section = 0;
			mt.VolumeEnvelope.TickCounter = 0;

			if (mt.VolumeEnvelope.Index != 0xffff)
			{
				mt.VolumeEnvelope.SustainA = m.VolumeEnvelopes[mt.VolumeEnvelope.Index].SustainA;
				mt.VolumeEnvelope.SustainB = m.VolumeEnvelopes[mt.VolumeEnvelope.Index].SustainB;
				mt.VolumeEnvelope.LoopEnd = m.VolumeEnvelopes[mt.VolumeEnvelope.Index].LoopLast;
			}

			mt.PanningEnvelope.Section = 0;
			mt.PanningEnvelope.TickCounter = 0;

			if (mt.PanningEnvelope.Index != 0xffff)
			{
				mt.PanningEnvelope.SustainA = m.PanningEnvelopes[mt.PanningEnvelope.Index].SustainA;
				mt.PanningEnvelope.SustainB = m.PanningEnvelopes[mt.PanningEnvelope.Index].SustainB;
				mt.PanningEnvelope.LoopEnd = m.PanningEnvelopes[mt.PanningEnvelope.Index].LoopLast;
			}

			// Set sample offset and loop direction
			if (mt.TriggerOffset > mt.SampleLength)
				mt.TriggerOffset = (int)mt.SampleLength;
			else if (mt.TriggerOffset < 0)
				mt.TriggerOffset = 0;

			worker.VirtualChannels[channel].PlaySample((short)(mt.Instrument - 1), mt.SampleData, (uint)mt.TriggerOffset, mt.SampleLength, mt.SampleBitSize, mt.PlayBackwards);

			if (mt.SampleLoopLength > 0)
				worker.VirtualChannels[channel].SetLoop(mt.SampleLoopStartOffset, mt.SampleLoopLength, mt.SampleLoopType);

			mt.VibratoCounter = 0;
			mt.IsOn = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Instrument(ModuleSynth mSyn, ModuleTrack mt, int instr)
		{
			mt.Instrument = 0;
			mt.IsOn = false;

			// Some modules contain triggers of non-existing
			// instruments. Such instruments should be silently ignored
			if (instr > mSyn.Module.NumberOfInstruments)
				return;

			DB3ModuleInstrument mi = mSyn.Module.Instruments[instr - 1];

			// Enable envelope interpolators
			mt.VolumeEnvelope.Index = mi.VolumeEnvelope;
			mt.PanningEnvelope.Index = mi.PanningEnvelope;

			switch (mi.Type)
			{
				case InstrumentType.Sample:
				{
					DB3ModuleSampleInstrument mis = (DB3ModuleSampleInstrument)mi;
					DB3ModuleSample ms = mSyn.Module.Samples[mis.SampleNumber];

					// There may be instruments with proper, but empty samples. If such an
					// instrument is triggered, just turn the channel off to the next trigger
					if ((ms == null) || ((ms.Data8 == null) && (ms.Data16 == null)) || (ms.Frames == 0))
						return;

					mt.SampleData = (Array)ms.Data8 ?? ms.Data16;
					mt.SampleBitSize = (byte)(ms.Data16 != null ? 16 : 8);
					mt.SampleLength = (uint)ms.Frames;

					if ((mis.Flags & InstrumentFlag.Loop_Mask) == InstrumentFlag.No_Loop)
					{
						mt.SampleLoopStartOffset = 0;
						mt.SampleLoopLength = 0;
					}
					else
					{
						mt.SampleLoopStartOffset = (uint)mis.LoopStart;
						mt.SampleLoopLength = (uint)mis.LoopLength;

						switch (mis.Flags & InstrumentFlag.Loop_Mask)
						{
							case InstrumentFlag.Forward_Loop:
							{
								mt.SampleLoopType = ChannelLoopType.Normal;
								break;
							}

							case InstrumentFlag.PingPong_Loop:
							{
								mt.SampleLoopType = ChannelLoopType.PingPong;
								break;
							}
						}
					}
					break;
				}
			}

			mt.Instrument = instr;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the pitch for the current instrument on the track. Does
		/// neither set nor retrigger the instrument. 'pitch' parameter is in
		/// fine tune unit prescaled by current module speed. 0 of pitch is
		/// C-0 note
		/// </summary>
		/********************************************************************/
		private void MSynth_Pitch(ModuleSynth mSyn, ModuleTrack mt, int channel)
		{
			int32_t pitch = mt.Pitch;

			if (mt.Instrument != 0)
			{
				DB3ModuleInstrument mi = mSyn.Module.Instruments[mt.Instrument - 1];

				if (mi != null)
				{
					switch (mi.Type)
					{
						case InstrumentType.Sample:
						{
							DB3ModuleSampleInstrument mis = (DB3ModuleSampleInstrument)mi;

							if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
							{
								int16_t arp = mt.ArpTable[mSyn.ArpCounter];
								if ((mt.Note + arp) < Tables.Periods.Length)
									pitch = Tables.Periods[mt.Note + arp];

								int32_t frequency = 3579545 / pitch;
								pitch = (int32_t)(3579545 / (((mis.C3Frequency * 256) / 8363 * frequency) / 256));

								int32_t vibVal = ((Tables.Vibrato[mt.VibratoCounter] * 5 / 3) * mt.VibratoDepth) / 128;
								pitch += vibVal;

								int32_t tempoPitch = 256;//(mSyn.Tempo * 256) / 125;

								frequency = (3546895 / pitch) * tempoPitch / 64;
								worker.VirtualChannels[channel].SetFrequency((uint)frequency);
							}
							else
							{
								pitch += mt.ArpTable[mSyn.ArpCounter];
								int32_t vibVal = Tables.Vibrato[mt.VibratoCounter] * mt.VibratoDepth;
								pitch += vibVal >> 8;

								uint16_t octave = 0;

								uint16_t sPorta = (uint16_t)(pitch % mSyn.Speed);
								uint16_t fTune = (uint16_t)(pitch / mSyn.Speed);
								uint32_t alpha = Tables.SmoothPorta[mSyn.Speed][sPorta];
								fTune -= 96;

								while (fTune >= 96)
								{
									fTune -= 96;
									octave++;
								}

								uint32_t beta = Tables.MusicScale[fTune];
								uint64_t frequency64 = (uint64_t)mis.C3Frequency * beta * alpha;
								frequency64 >>= 19 - octave;
								uint32_t frequency = (uint32_t)(frequency64 / 65536);
								worker.VirtualChannels[channel].SetFrequency(frequency);
							}
							break;
						}
					}

					mt.VibratoCounter += mt.VibratoSpeed;
					mt.VibratoCounter &= 0x3f;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_DefaultVolume(ModuleSynth mSyn, ModuleTrack mt)
		{
			if (mt.Instrument != 0)
			{
				DB3ModuleInstrument mInst = mSyn.Module.Instruments[mt.Instrument - 1];

				mt.Volume = mInst.Volume * mSyn.Speed;
				mt.Panning = mInst.Panning * mSyn.Speed;

				// Volume reset should also restart panning and volume
				// envelopes for the instrument
				mt.VolumeEnvelope.Section = 0;
				mt.VolumeEnvelope.TickCounter = 0;
				mt.PanningEnvelope.Section = 0;
				mt.PanningEnvelope.TickCounter = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Reset_Delayed(ModuleSynth mSyn)
		{
			mSyn.DelayPatternBreak = -1;
			mSyn.DelayPatternJump = -1;
			mSyn.DelayLoop = -1;
			mSyn.DelayModuleEnd = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Reset_Loop(ModuleTrack mt)
		{
			mt.LoopCounter = 0;
			mt.LoopOrder = 0;
			mt.LoopRow = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Apply_Delayed(ModuleSynth mSyn)
		{
			DB3ModuleSong song = mSyn.Module.Songs[mSyn.Song];

			// Position jump
			if (mSyn.DelayPatternJump != -1)
			{
				if (mSyn.DelayPatternJump < song.NumberOfOrders)
				{
					if (mSyn.DelayPatternJump < mSyn.Order)
						mSyn.DelayModuleEnd = true;

					mSyn.Order = mSyn.DelayPatternJump;
				}
				else
				{
					mSyn.Order = 0;
					mSyn.DelayModuleEnd = true;
				}

				mSyn.Pattern = song.PlayList[mSyn.Order];
				mSyn.Row = 0;
			}

			// Pattern break
			if (mSyn.DelayPatternBreak != -1)
			{
				if ((mSyn.DelayPatternJump == -1) && (mSyn.Row > 0))
				{
					if (++mSyn.Order >= song.NumberOfOrders)
					{
						mSyn.Order = 0;
						mSyn.DelayModuleEnd = true;
					}
				}

				mSyn.Pattern = song.PlayList[mSyn.Order];
				DB3ModulePattern mPatt = mSyn.Module.Patterns[mSyn.Pattern];

				if (mSyn.DelayPatternBreak < mPatt.NumberOfRows)
					mSyn.Row = mSyn.DelayPatternBreak;
				else
					mSyn.Row = mPatt.NumberOfRows - 1;
			}

			// Loops
			if (mSyn.DelayLoop != -1)
			{
				mSyn.Order = mSyn.Tracks[mSyn.DelayLoop].LoopOrder;
				mSyn.Pattern = song.PlayList[mSyn.Order];
				mSyn.Row = mSyn.Tracks[mSyn.DelayLoop].LoopRow;
			}
			else
			{
				// Module end
				if (mSyn.DelayModuleEnd)
					endReached = true;
			}

			// Reset all delayed things
			MSynth_Reset_Delayed(mSyn);
		}



		/********************************************************************/
		/// <summary>
		/// Used for both standard, global echo and new per-track echo. The
		/// only difference is echo type, as per-track echo parameters
		/// default to standard ones
		/// </summary>
		/********************************************************************/
		private void MSynth_Echo_On_For_Track(ModuleTrack mt, EchoType type)
		{
			// Check if echo has already been added to the track
			if (effectMaster.GetEffectGroup(mt.TrackNumber) != -1)
				return;

			// Add echo to the track
			if (type == EchoType.Old)
				effectMaster.AddToDefaultEffectGroup(mt.TrackNumber);
			else
			{
				effectMaster.AddToEffectGroup(mt.TrackNumber, new EchoArguments
				{
					EchoDelay = mt.EchoDelay,
					EchoFeedback = mt.EchoFeedback,
					EchoMix = mt.EchoMix,
					EchoCross = mt.EchoCross
				});
			}
		}



		/********************************************************************/
		/// <summary>
		/// The function removes echo only if it matches passed 'type'
		/// </summary>
		/********************************************************************/
		private void MSynth_Echo_Off_For_Track(ModuleTrack mt, EchoType type)
		{
			int effectGroup = effectMaster.GetEffectGroup(mt.TrackNumber);
			if (effectGroup == -1)
				return;

			if ((type == EchoType.Old) && (effectGroup != EffectMaster.DefaultEffectGroup))
				return;

			if ((type == EchoType.New) && (effectGroup == EffectMaster.DefaultEffectGroup))
				return;

			effectMaster.RemoveFromEffectGroup(mt.TrackNumber);
		}



		/********************************************************************/
		/// <summary>
		/// Adds old style (global controlled) echo for all tracks not having
		/// any echo
		/// </summary>
		/********************************************************************/
		private void MSynth_Echo_On_For_All_Tracks(ModuleSynth mSyn)
		{
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
				MSynth_Echo_On_For_Track(mSyn.Tracks[track], EchoType.Old);
		}



		/********************************************************************/
		/// <summary>
		/// It only switches off the standard echo. New style echo must be
		/// switched off for each track with V21 command
		/// </summary>
		/********************************************************************/
		private void MSynth_Echo_Off_For_All_Tracks(ModuleSynth mSyn)
		{
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
				MSynth_Echo_Off_For_Track(mSyn.Tracks[track], EchoType.Old);
		}



		/********************************************************************/
		/// <summary>
		/// This function checks the echo type in the track. If there is no
		/// echo, or echo is DSPV_EchoType_Old, echo parameters are set for
		/// all tracks with old echo. If echo is DSPV_EchoType_New,
		/// parameters are changed for this track only
		/// </summary>
		/********************************************************************/
		private void MSynth_Change_Echo_Parameters(ModuleTrack mt)
		{
			EchoArguments echoArguments = new EchoArguments
			{
				EchoDelay = mt.EchoDelay,
				EchoFeedback = mt.EchoFeedback,
				EchoMix = mt.EchoMix,
				EchoCross = mt.EchoCross
			};

			int effectGroup = effectMaster.GetEffectGroup(mt.TrackNumber);
			if (effectGroup == -1)
				effectGroup = EffectMaster.DefaultEffectGroup;

			effectMaster.ChangeValuesInEffectGroup(effectGroup, echoArguments);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Effect_Exx(ModuleSynth mSyn, ModuleTrack mt, int channel, uint8_t parameter)
		{
			switch ((ExtraEffect)(parameter >> 4))
			{
				case ExtraEffect.FinePortamentoUp:
				{
					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						mt.PitchDelta -= (int16_t)(parameter & 0x0f);

						if (mt.Pitch < mSyn.MinPitch)
							mt.Pitch = mSyn.MinPitch;
					}
					else
					{
						mt.Pitch += (parameter & 0xf) * mSyn.Speed;

						if (mt.Pitch > mSyn.MaxPitch)
							mt.Pitch = mSyn.MaxPitch;
					}
					break;
				}

				case ExtraEffect.FinePortamentoDown:
				{
					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						mt.PitchDelta += (int16_t)(parameter & 0x0f);

						if (mt.Pitch > mSyn.MaxPitch)
							mt.Pitch = mSyn.MaxPitch;
					}
					else
					{
						mt.Pitch -= (parameter & 0xf) * mSyn.Speed;

						if (mt.Pitch < mSyn.MinPitch)
							mt.Pitch = mSyn.MinPitch;
					}
					break;
				}

				case ExtraEffect.PlayBackwards:
				{
					if (mt.TriggerCounter != 0x7fff)
						mt.PlayBackwards = true;

					break;
				}

				case ExtraEffect.ChannelControlA:
				{
					if (parameter == 0x40)		// E40 - track mute
						mt.IsOn = false;

					break;
				}

				case ExtraEffect.SetLoop:
				{
					if ((parameter & 0x0f) != 0)
					{
						if (mt.LoopCounter == 0)
						{
							mt.LoopCounter = parameter & 0x0f;
							mSyn.DelayLoop = channel;
						}
						else
						{
							if (--mt.LoopCounter > 0)
								mSyn.DelayLoop = channel;
							else
							{
								MSynth_Reset_Loop(mt);
								mSyn.DelayLoop = -1;
							}
						}
					}
					else	// E60
					{
						if (mt.LoopCounter == 0)
						{
							mt.LoopOrder = mSyn.Order;
							mt.LoopRow = mSyn.Row;
						}
					}
					break;
				}

				case ExtraEffect.SetSampleOffset:
				{
					mt.TriggerOffset += (parameter & 0x0f) << 16;
					break;
				}

				case ExtraEffect.SetPanning:
				{
					mt.Panning = (((parameter & 0x0f) << 4) - 128) * mSyn.Speed;
					break;
				}

				case ExtraEffect.RetrigNote:
				{
					mt.Retrigger = parameter & 0x0f;
					break;
				}

				case ExtraEffect.FineVolumeSlideUp:
				{
					mt.Volume += (parameter & 0x0f) * mSyn.Speed;
					if (mt.Volume > mSyn.MaxVolume)
						mt.Volume = mSyn.MaxVolume;

					break;
				}

				case ExtraEffect.FineVolumeSlideDown:
				{
					mt.Volume -= (parameter & 0x0f) * mSyn.Speed;
					if (mt.Volume < mSyn.MinVolume)
						mt.Volume = mSyn.MinVolume;

					break;
				}

				case ExtraEffect.NoteCut:
				{
					mt.CutCounter = parameter & 0x0f;
					break;
				}

				case ExtraEffect.NoteDelay:
				{
					mt.TriggerCounter = parameter & 0x0f;
					break;
				}

				case ExtraEffect.PatternDelay:
				{
					mSyn.PatternDelay = parameter & 0x0f;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Effect(ModuleSynth mSyn, ModuleTrack mt, int channel, Effect command, uint8_t parameter)
		{
			switch (command)
			{
				case Effect.Arpreggio:
				{
					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						mt.ArpTable[1] = (int16_t)(parameter >> 4);
						mt.ArpTable[2] = (int16_t)(parameter & 0x0f);
					}
					else
					{
						mt.ArpTable[1] = (int16_t)(((parameter >> 4) << 3) * mSyn.Speed);
						mt.ArpTable[2] = (int16_t)(((parameter & 0x0f) << 3) * mSyn.Speed);
					}
					break;
				}

				// Includes 1Fx
				case Effect.PortamentoUp:
				{
					if (parameter == 0)
						parameter = mt.Old.PortamentoUp;
					else
						mt.Old.PortamentoUp = parameter;

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						if (parameter < 0xf0)
							mt.PitchDelta -= (int16_t)(parameter * 4);
						else
							mt.PitchDelta -= (int16_t)(parameter & 0x0f);			// Smooth 1Fx
					}
					else
					{
						if (parameter < 0xf0)
							mt.PitchDelta += (int16_t)(parameter * mSyn.Speed);
						else
							mt.PitchDelta += (int16_t)(parameter & 0x0f);			// Smooth 1Fx
					}
					break;
				}

				// Includes 2Fx
				case Effect.PortamentoDown:
				{
					if (parameter == 0)
						parameter = mt.Old.PortamentoDown;
					else
						mt.Old.PortamentoDown = parameter;

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						if (parameter < 0xf0)
							mt.PitchDelta += (int16_t)(parameter * 4);
						else
							mt.PitchDelta += (int16_t)(parameter & 0x0f);			// Smooth 2Fx
					}
					else
					{
						if (parameter < 0xf0)
							mt.PitchDelta -= (int16_t)(parameter * mSyn.Speed);
						else
							mt.PitchDelta -= (int16_t)(parameter & 0x0f);			// Smooth 2Fx
					}
					break;
				}

				case Effect.PortamentoToNote:
				{
					if (parameter == 0)
						parameter = mt.Old.PortamentoSpeed;
					else
						mt.Old.PortamentoSpeed = parameter;

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						if (mt.Porta3Target >= mt.Pitch)
							mt.Porta3Delta += (int16_t)(parameter * 4);
						else
							mt.Porta3Delta -= (int16_t)(parameter * 4);
					}
					else
					{
						int16_t portaTarget = (int16_t)(mt.Porta3Target * mSyn.Speed);

						if (portaTarget >= mt.Pitch)
							mt.Porta3Delta += (int16_t)(parameter * mSyn.Speed);
						else
							mt.Porta3Delta -= (int16_t)(parameter * mSyn.Speed);
					}
					break;
				}

				case Effect.Vibrato:
				{
					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						if ((parameter & 0xf0) == 0)
							parameter = (uint8_t)((parameter & 0x0f) | (mt.Old.Vibrato & 0xf0));

						if ((parameter & 0x0f) == 0)
							parameter = (uint8_t)((parameter & 0xf0) | (mt.Old.Vibrato & 0x0f));

						mt.Old.Vibrato = parameter;
					}
					else
					{
						if (parameter == 0)
							parameter = mt.Old.Vibrato;
						else
							mt.Old.Vibrato = parameter;
					}

					mt.VibratoSpeed = (int16_t)(parameter >> 4);
					mt.VibratoDepth = (int16_t)(parameter & 0x0f);

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_3)
						mt.VibratoDepth *= (int16_t)mSyn.Speed;

					break;
				}

				case Effect.PortamentoToNoteVolumeSlide:
				{
					if (parameter == 0)
						parameter = mt.Old.VolumeSlide5;
					else
						mt.Old.VolumeSlide5 = parameter;

					int16_t portaSpeed = mt.Old.PortamentoSpeed;

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						if (mt.Porta3Target >= mt.Pitch)
							mt.Porta3Delta += (int16_t)(portaSpeed * 4);
						else
							mt.Porta3Delta -= (int16_t)(portaSpeed * 4);
					}
					else
					{
						int16_t portaTarget = (int16_t)(mt.Porta3Target * mSyn.Speed);

						if (portaTarget >= mt.Pitch)
							mt.Porta3Delta += (int16_t)(portaSpeed * mSyn.Speed);
						else
							mt.Porta3Delta -= (int16_t)(portaSpeed * mSyn.Speed);
					}

					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if ((p0 == 0) || (p1 == 0))		// Normal 50x/5x0
					{
						if (p0 != 0)
							mt.VolumeDelta += (int16_t)(p0 * mSyn.Speed);

						if (p1 != 0)
							mt.VolumeDelta -= (int16_t)(p1 * mSyn.Speed);
					}
					else
					{
						if (p1 == 0x0f)
							mt.VolumeDelta += p0;
						else if (p0 == 0x0f)
							mt.VolumeDelta -= p1;
					}
					break;
				}

				case Effect.VibratoVolumeSlide:
				{
					if (parameter == 0)
						parameter = mt.Old.Vibrato6;
					else
						mt.Old.Vibrato6 = parameter;

					mt.VibratoSpeed = (int16_t)(mt.Old.Vibrato >> 4);
					mt.VibratoDepth = (int16_t)(mt.Old.Vibrato & 0x0f);

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_3)
						mt.VibratoDepth *= (int16_t)mSyn.Speed;

					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if ((p0 == 0) || (p1 == 0))		// Normal 60x/6x0
					{
						if (p0 != 0)
							mt.VolumeDelta += (int16_t)(p0 * mSyn.Speed);

						if (p1 != 0)
							mt.VolumeDelta -= (int16_t)(p1 * mSyn.Speed);
					}
					else
					{
						if (p1 == 0x0f)
							mt.VolumeDelta += p0;
						else if (p0 == 0x0f)
							mt.VolumeDelta -= p1;
					}
					break;
				}

				case Effect.SetPanning:
				{
					mt.Panning = (parameter - 128) * mSyn.Speed;
					break;
				}

				case Effect.SampleOffset:
				{
					mt.TriggerOffset += parameter << 8;
					break;
				}

				// Includes AxF and AFx
				case Effect.VolumeSlide:
				{
					if (parameter == 0)
						parameter = mt.Old.VolumeSlide;		// A00, reuse old parameter
					else
						mt.Old.VolumeSlide = parameter;

					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if ((p0 == 0) || (p1 == 0))		// Normal A0x/Ax0
					{
						if (p0 != 0)
							mt.VolumeDelta += (int16_t)(p0 * mSyn.Speed);

						if (p1 != 0)
							mt.VolumeDelta -= (int16_t)(p1 * mSyn.Speed);
					}
					else
					{
						if (p1 == 0x0f)
							mt.VolumeDelta += p0;
						else if (p0 == 0x0f)
							mt.VolumeDelta -= p1;
					}
					break;
				}

				case Effect.PositionJump:
				{
					mSyn.DelayPatternJump = parameter;
					break;
				}

				case Effect.SetVolume:
				{
					if (parameter <= 0x40)
						mt.Volume = parameter * mSyn.Speed;

					break;
				}

				case Effect.PatternBreak:
				{
					mSyn.DelayPatternBreak = Bcd2Bin(parameter);
					break;
				}

				case Effect.ExtraEffects:
				{
					MSynth_Effect_Exx(mSyn, mt, channel, parameter);
					break;
				}

				case Effect.SetGlobalVolume:
				{
					mSyn.GlobalVolume = parameter;
					if (mSyn.GlobalVolume > 0x40)
						mSyn.GlobalVolume = 0x40;

					break;
				}

				case Effect.GlobalVolumeSlide:
				{
					if (parameter == 0)
						parameter = mSyn.OldGlobalVolumeSlide;		// H00, reuse old parameter
					else
						mSyn.OldGlobalVolumeSlide = parameter;

					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if ((p0 == 0) || (p1 == 0))		// Normal H0x/Hx0
					{
						if (p0 != 0)
							mSyn.GlobalVolumeSlide += p0;

						if (p1 != 0)
							mSyn.GlobalVolumeSlide -= p1;
					}
					break;
				}

				case Effect.PanningSlide:
				{
					if (parameter == 0)
						parameter = mt.Old.PanningSlide;		// P00, reuse old parameter
					else
						mt.Old.PanningSlide = parameter;

					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if ((p0 == 0) || (p1 == 0))		// Normal P0x/Px0
					{
						if (p0 != 0)
							mt.PanningDelta += (int16_t)(p0 * mSyn.Speed);

						if (p1 != 0)
							mt.PanningDelta -= (int16_t)(p1 * mSyn.Speed);
					}
					break;
				}

				case Effect.EchoSwitch:
				{
					int16_t p0 = (int16_t)(parameter >> 4);
					int16_t p1 = (int16_t)(parameter & 0x0f);

					if (p1 == 0)		// Echo on
					{
						switch (p0)
						{
							case 0:
							{
								MSynth_Echo_On_For_Track(mt, EchoType.Old);
								break;
							}

							case 1:
							{
								MSynth_Echo_On_For_All_Tracks(mSyn);
								break;
							}

							case 2:
							{
								MSynth_Echo_On_For_Track(mt, EchoType.New);
								break;
							}
						}
					}
					else if (p1 == 1)	// Echo off
					{
						switch (p0)
						{
							case 0:
							{
								MSynth_Echo_Off_For_Track(mt, EchoType.Old);
								break;
							}

							case 1:
							{
								MSynth_Echo_Off_For_All_Tracks(mSyn);
								break;
							}

							case 2:
							{
								MSynth_Echo_Off_For_Track(mt, EchoType.New);
								break;
							}
						}
					}
					break;
				}

				case Effect.EchoDelay:
				{
					mt.EchoDelay = parameter;
					MSynth_Change_Echo_Parameters(mt);
					break;
				}

				case Effect.EchoFeedback:
				{
					mt.EchoFeedback = parameter;
					MSynth_Change_Echo_Parameters(mt);
					break;
				}

				case Effect.EchoMix:
				{
					mt.EchoMix = parameter;
					MSynth_Change_Echo_Parameters(mt);
					break;
				}

				case Effect.EchoCross:
				{
					mt.EchoCross = parameter;
					MSynth_Change_Echo_Parameters(mt);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Update_Callback()
		{
			if (modSynth.Order != lastPosition)
			{
				lastPosition = modSynth.Order;

				ShowSongPosition();
				ShowPattern();

				if (!endReached)
					worker.PositionHasChanged(modSynth.Order);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Next_Pattern(ModuleSynth mSyn)
		{
			DB3ModuleSong song = mSyn.Module.Songs[mSyn.Song];

			if (++mSyn.Order >= song.NumberOfOrders)		// End of song
			{
				mSyn.Order = 0;

				if ((mSyn.Mode == ModuleMode.Song_Once) || (mSyn.Mode == ModuleMode.Song))
					mSyn.DelayModuleEnd = true;
			}

			mSyn.Pattern = song.PlayList[mSyn.Order];
			mSyn.Row = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Scan_For_Speed(ModuleSynth mSyn)
		{
			DB3ModulePattern mPatt = mSyn.Module.Patterns[mSyn.Pattern];
			int meStartOffset = mSyn.Row * mSyn.Module.NumberOfTracks;

			// Speed/tempo effect scan
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				DB3ModuleEntry me = mPatt.Pattern[meStartOffset + track];

				if (me.Command1 == Effect.SetTempo)
				{
					if (me.Parameter1 == 0x00)
						mSyn.PatternDelay = 0x7fffffff;
					else
					{
						if (me.Parameter1 < 0x20)
							ChangeSpeed(me.Parameter1);
						else
							ChangeTempo(me.Parameter1);
					}
				}

				if (me.Command2 == Effect.SetTempo)
				{
					if (me.Parameter2 == 0x00)
						mSyn.PatternDelay = 0x7fffffff;
					else
					{
						if (me.Parameter2 < 0x20)
							ChangeSpeed(me.Parameter2);
						else
							ChangeTempo(me.Parameter2);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Next_Row(ModuleSynth mSyn)
		{
			DB3ModulePattern mPatt = mSyn.Module.Patterns[mSyn.Pattern];
			int meStartOffset = mSyn.Row * mSyn.Module.NumberOfTracks;

			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				DB3ModuleEntry me = mPatt.Pattern[meStartOffset + track];
				ModuleTrack mt = mSyn.Tracks[track];

				// Let's set trigger counter to 0x7fff. As no position can have so many
				// ticks, triggering is for now disabled for this row and track. Do the
				// same for CutCounter, so no cut is set. Disable retriggers as well
				mt.TriggerCounter = 0x7fffffff;
				mt.CutCounter = 0x7fffffff;
				mt.Retrigger = 0;
				mt.PlayBackwards = false;

				// Important! Different combinations of presence of note and instrument
				// number give different results:
				//
				// note instr    instr. retrigger   pitch change   vol. to default
				//
				//  no   no            no              no              no
				//  no  yes       change + no          no             yes
				// yes   no           yes             yes*             no
				// yes  yes       change + yes        yes*            yes
				//
				// (*) Pitch is not set, if any of the two commands is porta to note (3xx)
				// or porta to note + volume slide (5xx)

				// So well, if we have note and instrument, check if instrument is the same
				// as the previously played one on the track. If not, change instrument
				if ((me.Instrument != 0) && (me.Instrument != mt.Instrument))
					MSynth_Instrument(mSyn, mt, me.Instrument);

				// If we have note, we should change the pitch and set trigger, but not so
				// fast! There are porta to note effects! We have to watch out keyoffs too.
				// Do not trigger & pitch if note is > 11. Handle envelope control point
				// instead
				if (me.Octave != 0)
				{
					if (me.Note < 12)
					{
						if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
						{
							int32_t note = (me.Octave - 1) * 12 + me.Note;
							int16_t period = Tables.Periods[note];

							if (!PortamentoToNote(me))
							{
								mt.Note = note;
								mt.Pitch = period;
								mt.TriggerCounter = 0;			// Trigger at tick 0, effects can change it later
							}
							else
								mt.Porta3Target = period;
						}
						else
						{
							int16_t ftNote = (int16_t)(((me.Octave << 3) + (me.Octave << 2) + me.Note) << 3);

							if (!PortamentoToNote(me))
							{
								mt.Pitch = ftNote;
								mt.Pitch *= mSyn.Speed;
								mt.TriggerCounter = 0;			// Trigger at tick 0, effects can change it later
							}
							else
								mt.Porta3Target = ftNote;
						}
					}
					else
					{
						// Key off. Apply for both volume and panning envelopes, in "loop,
						// sustain A, sustain B" order. If an instrument has neither volume
						// nor panning envelope, cut the channel off
						if ((mt.VolumeEnvelope.Index == 0xffff) && (mt.PanningEnvelope.Index == 0xffff))
							mt.IsOn = false;

						if (mt.VolumeEnvelope.Index != 0xffff)
						{
							if ((mt.VolumeEnvelope.LoopEnd <= mt.VolumeEnvelope.SustainA) && (mt.VolumeEnvelope.LoopEnd <= mt.VolumeEnvelope.SustainB))
								mt.VolumeEnvelope.LoopEnd = 0xffff;
							else
							{
								if (mt.VolumeEnvelope.SustainA <= mt.VolumeEnvelope.SustainB)
									mt.VolumeEnvelope.SustainA = 0xffff;
								else
									mt.VolumeEnvelope.SustainB = 0xffff;
							}
						}

						if (mt.PanningEnvelope.Index != 0xffff)
						{
							if ((mt.PanningEnvelope.LoopEnd <= mt.PanningEnvelope.SustainA) && (mt.PanningEnvelope.LoopEnd <= mt.PanningEnvelope.SustainB))
								mt.PanningEnvelope.LoopEnd = 0xffff;
							else
							{
								if (mt.PanningEnvelope.SustainA <= mt.PanningEnvelope.SustainB)
									mt.PanningEnvelope.SustainA = 0xffff;
								else
									mt.PanningEnvelope.SustainB = 0xffff;
							}
						}
					}
				}

				// If we have instrument, let's set the default volume and reset trigger offset
				if (me.Instrument != 0)
				{
					MSynth_DefaultVolume(mSyn, mt);
					mt.TriggerOffset = 0;
				}

				// Now is the time for effects
				if ((me.Command1 != Effect.None) || (me.Parameter1 != 0))
					MSynth_Effect(mSyn, mt, track, me.Command1, me.Parameter1);

				if ((me.Command2 != Effect.None) || (me.Parameter2 != 0))
					MSynth_Effect(mSyn, mt, track, me.Command2, me.Parameter2);
			}

			// Moving to the next row (or not, depending on playback mode).
			// Nothing happens for MMODE_ROW
			switch (mSyn.Mode)
			{
				case ModuleMode.Manual:
				{
					if (++mSyn.Row >= mPatt.NumberOfRows)
						mSyn.Row = 0;

					mSyn.Mode = ModuleMode.Halted;
					mSyn.ManualUpdate = true;
					break;
				}

				case ModuleMode.ManualBack:
				{
					if (--mSyn.Row < 0)
						mSyn.Row = mPatt.NumberOfRows - 1;

					mSyn.Mode = ModuleMode.Halted;
					mSyn.ManualUpdate = true;
					break;
				}

				case ModuleMode.Pattern:
				{
					if (++mSyn.Row >= mPatt.NumberOfRows)
						mSyn.Row = 0;

					break;
				}

				case ModuleMode.PatternBack:
				{
					if (--mSyn.Row < 0)
						mSyn.Row = mPatt.NumberOfRows - 1;

					break;
				}

				case ModuleMode.Song:
				case ModuleMode.Song_Once:
				{
					if (++mSyn.Row >= mPatt.NumberOfRows)
						MSynth_Next_Pattern(mSyn);

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Post_Tick(ModuleSynth mSyn)
		{
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				// Volume slides (accumulated)
				mt.Volume += mt.VolumeDelta;

				if (mt.Volume > mSyn.MaxVolume)
					mt.Volume = mSyn.MaxVolume;

				if (mt.Volume < mSyn.MinVolume)
					mt.Volume = mSyn.MinVolume;

				// Panning slides (accumulated)
				mt.Panning += mt.PanningDelta;

				if (mt.Panning > mSyn.MaxPanning)
					mt.Panning = mSyn.MaxPanning;

				if (mt.Panning < mSyn.MinPanning)
					mt.Panning = mSyn.MinPanning;

				// Portamento to note
				if (mt.Porta3Delta != 0)
				{
					int32_t portaTarget = mt.Porta3Target;

					if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_3)
						portaTarget *= mSyn.Speed;

					mt.Pitch += mt.Porta3Delta;

					if (mt.Porta3Delta > 0)		// Porta to note goes up
					{
						if (mt.Pitch > portaTarget)
							mt.Pitch = portaTarget;
					}
					else
					{
						if (mt.Pitch < portaTarget)
							mt.Pitch = portaTarget;
					}
				}

				// Other portamentos
				mt.Pitch += mt.PitchDelta;

				// Pitch clipping
				if (mt.Pitch > mSyn.MaxPitch)
					mt.Pitch = mSyn.MaxPitch;

				if (mt.Pitch < mSyn.MinPitch)
					mt.Pitch = mSyn.MinPitch;
			}

			// Hxx, global volume slide
			mSyn.GlobalVolume += mSyn.GlobalVolumeSlide;

			if (mSyn.GlobalVolume > 64)
				mSyn.GlobalVolume = 64;

			if (mSyn.GlobalVolume < 0)
				mSyn.GlobalVolume = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Do_Triggers(ModuleSynth mSyn)
		{
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				if (mt.Instrument != 0)
				{
					if (mt.TriggerCounter == 0)
					{
						MSynth_Trigger(mSyn.Module, mt, track);
						mt.TriggerCounter = mt.Retrigger;
					}

					mt.TriggerCounter--;

					// Note that note cut does not switch the channel off, the note is being
					// continued with 0 volume
					if (mt.CutCounter-- <= 0)
						mt.Volume = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Clear_Slides(ModuleSynth mSyn)
		{
			// Scale back current volume/pan to PT units and pitch to fine tunes.
			// Clear deltas
			// Clear arpeggios table entries 1 and 2 (entry 0 is always cleared)
			// Clear arpeggio (global) counter
			// Clear vibrato
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				mt.VolumeDelta = 0;
				mt.PanningDelta = 0;
				mt.PitchDelta = 0;
				mt.Porta3Delta = 0;
				mt.Volume /= mSyn.Speed;
				mt.Panning /= mSyn.Speed;

				if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_3)
					mt.Pitch /= mSyn.Speed;

				// Do not clear arppegio and vibrato when sequencer is halted (single step mode)
				if (mSyn.Mode != ModuleMode.Halted)
				{
					mt.ArpTable[1] = 0;
					mt.ArpTable[2] = 0;
					mt.VibratoSpeed = 0;
					mt.VibratoDepth = 0;
				}
			}

			// Hxx
			mSyn.GlobalVolumeSlide = 0;

			// Do not clear arpeggio counter when sequencer is halted (single step mode)
			if (mSyn.Mode != ModuleMode.Halted)
				mSyn.ArpCounter = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Setup_Slides(ModuleSynth mSyn)
		{
			// Scale current volume, panning, pitch with current speed
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				mt.Volume *= mSyn.Speed;
				mt.Panning *= mSyn.Speed;

				if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_3)
					mt.Pitch *= mSyn.Speed;
			}

			// Calculate limits
			mSyn.MinVolume = 0;
			mSyn.MinPanning = (int16_t)(-(mSyn.Speed << 7));

			mSyn.MaxVolume = (int16_t)(mSyn.Speed << 6);
			mSyn.MaxPanning = (int16_t)(mSyn.Speed << 7);

			if (mSyn.Module.CreatorVersion == Constants.Creator_DigiBooster_2)
			{
				mSyn.MinPitch = 57;
				mSyn.MaxPitch = 13696;
			}
			else
			{
				mSyn.MinPitch = (int16_t)(mSyn.Speed * 96);
				mSyn.MaxPitch = (int16_t)(mSyn.Speed * 864);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Tick_Gains_And_Pitch(ModuleSynth mSyn)
		{
			for (int track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				MSynth_Pitch(mSyn, mt, track);

				// Convert [PT * speed] units of accumulated volume slide to 14-bit gain
				// Apply panning then
				// mt.Volume	            <0, +64 * speed>    | volc       <0, +16384>
				// mt.Panning    <-128 * speed, +128 * speed>   | pan   <-16384, +16384>
				int32_t volc = (mt.Volume << 8) / mSyn.Speed;
				int32_t pan = (mt.Panning << 7) / mSyn.Speed;

				// Apply envelopes. Volume envelopes is just multiplied with the current
				// gain and renormalized (gain: <0, +16384>, vol envelope <0, +16384>).
				// Panning envelope is a bit more complicated. Panning envelope center is
				// moved to the current panning, then pan envelope range is shrinked to
				// fit the final panning range (if the current panning is 0. there is no
				// shrinking, when panning is moving to the left or right channel,
				// shrinking increases, for full L/R panning, envelope range is reduced
				// to 0. The exact formula is:
				//
				// p = p0 + (1.0 - abs(p0)) * ev
				//
				// where:
				//  p0 - panning before applying envelope (result of default panning and
				//       panning effects.
				//  ev - Current value of panning envelope in full range
				//  p  - final panning value
				//
				// The value above is for panning normalized to <-1.0, +1.0>. We use fixed
				// point math here, so some scaling is used
				//  p0       <-16384, +16384>
				//  ev       <-16384, +16384>
				//  p        <-16384, +16384>
				volc *= mt.VolumeEnvelopeCurrent;
				volc >>= 14;
				volc *= mSyn.GlobalVolume;
				volc >>= 6;

				int16_t p1 = (int16_t)((pan > 0) ? pan : -pan);		// abs
				p1 = (int16_t)(16384 - p1);
				int32_t p2 = p1 * mt.PanningEnvelopeCurrent >> 14;
				pan += p2;

				// Tell NostalgicPlayer about volume/panning
				worker.VirtualChannels[track].SetVolume((ushort)(volc / 64));
				worker.VirtualChannels[track].SetPanning((ushort)((pan + 16384) / 128));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int16_t MSynth_VolumeEnvelope_Interpolator(EnvelopeInterpolator evi, DB3ModuleEnvelope mde)
		{
			if (evi.TickCounter == 0)		// End of section, fetch next one
			{
				if (evi.Section >= evi.LoopEnd)
					evi.Section = mde.LoopFirst;

				evi.YStart = (int16_t)(mde.Points[evi.Section].Value << 8);

				if (evi.Section == evi.SustainA)
					return (int16_t)evi.YStart;

				if (evi.Section == evi.SustainB)
					return (int16_t)evi.YStart;

				if (evi.Section >= mde.NumberOfSections)
					return (int16_t)evi.YStart;

				evi.XDelta = (int16_t)(mde.Points[evi.Section + 1].Position - mde.Points[evi.Section].Position);
				evi.TickCounter = (uint16_t)evi.XDelta;
				evi.YDelta = (int16_t)((mde.Points[evi.Section + 1].Value << 8) - evi.YStart);
				evi.Section++;
			}

			evi.PreviousValue = (int16_t)(evi.YStart + (evi.YDelta * (evi.XDelta - evi.TickCounter--)) / evi.XDelta);

			return evi.PreviousValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int16_t MSynth_PanningEnvelope_Interpolator(EnvelopeInterpolator evi, DB3ModuleEnvelope mde)
		{
			if (evi.TickCounter == 0)		// End of section, fetch next one
			{
				if (evi.Section >= evi.LoopEnd)
					evi.Section = mde.LoopFirst;

				evi.YStart = (mde.Points[evi.Section].Value + 128) << 7;

				if (evi.Section == evi.SustainA)
					return (int16_t)(evi.YStart - 16384);

				if (evi.Section == evi.SustainB)
					return (int16_t)(evi.YStart - 16384);

				if (evi.Section >= mde.NumberOfSections)
					return (int16_t)(evi.YStart - 16384);

				evi.XDelta = (int16_t)(mde.Points[evi.Section + 1].Position - mde.Points[evi.Section].Position);
				evi.TickCounter = (uint16_t)evi.XDelta;
				evi.YDelta = ((mde.Points[evi.Section + 1].Value + 128) << 7) - evi.YStart;
				evi.Section++;
			}

			evi.PreviousValue = (int16_t)((evi.YStart + (evi.YDelta * (evi.XDelta - evi.TickCounter--)) / evi.XDelta) - 16384);

			return evi.PreviousValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Do_Envelopes(ModuleSynth mSyn)
		{
			for (int16_t track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				if (mt.IsOn && (mt.VolumeEnvelope.Index != 0xffff))
					mt.VolumeEnvelopeCurrent = MSynth_VolumeEnvelope_Interpolator(mt.VolumeEnvelope, mSyn.Module.VolumeEnvelopes[mt.VolumeEnvelope.Index]);

				if (mt.IsOn && (mt.PanningEnvelope.Index != 0xffff))
					mt.PanningEnvelopeCurrent = MSynth_PanningEnvelope_Interpolator(mt.PanningEnvelope, mSyn.Module.PanningEnvelopes[mt.PanningEnvelope.Index]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Next_Tick(ModuleSynth mSyn)
		{
			MSynth_Post_Tick(mSyn);

			if (mSyn.Tick == 0)
			{
				if (mSyn.PatternDelay > 0)
				{
					// EEx can delay up to 15 ticks and is not additive. Any higher
					// delay must be caused by either song end in MMODE_SONG_ONCE mode
					// or F00 effect
					if (mSyn.PatternDelay-- > 15)
					{
						endReached = true;
						restartSong = true;
					}
				}
				else
				{
					MSynth_Apply_Delayed(mSyn);

					// When sequencer is halted, no position updates are sent, with exception of the
					// first tick zero after going from MANUAL to HALTED state. The ManualUpdate flag
					// is set in MSynth_Next_Row() when MANUAL state is turned into HALTED
					if (mSyn.Mode == ModuleMode.Halted)
					{
						if (mSyn.ManualUpdate)
						{
							mSyn.ManualUpdate = false;
							MSynth_Update_Callback();
						}
					}
					else
						MSynth_Update_Callback();

					MSynth_Clear_Slides(mSyn);

					if (mSyn.Mode != ModuleMode.Halted)
						MSynth_Scan_For_Speed(mSyn);

					MSynth_Setup_Slides(mSyn);

					if (mSyn.Mode != ModuleMode.Halted)
						MSynth_Next_Row(mSyn);
				}
			}

			MSynth_Do_Triggers(mSyn);
			MSynth_Do_Envelopes(mSyn);
			MSynth_Tick_Gains_And_Pitch(mSyn);

			if (++mSyn.ArpCounter > 2)
				mSyn.ArpCounter = 0;

			if (++mSyn.Tick == mSyn.Speed)
				mSyn.Tick = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes all tracks. Performs following operations:
		/// - Disables and unmutes track
		/// - Set instrument to 0
		/// - Set pitch to 0
		/// - Resets arpeggio and vibrato counters
		/// - Resets porta-to-note target to C-4
		/// - Resets volume/panning slide value storage (for x00) to 0
		/// - Resets playback direction to forward
		/// - Initializes list of DSP blocks
		/// </summary>
		/********************************************************************/
		private void MSynth_Init_Tracks(ModuleSynth mSyn)
		{
			for (int16_t track = 0; track < mSyn.Module.NumberOfTracks; track++)
			{
				ModuleTrack mt = mSyn.Tracks[track];

				mt.TrackNumber = track;

				mt.Instrument = 0;
				mt.IsOn = false;

				mt.Note = 0;
				mt.Pitch = 0;
				mt.ArpTable[0] = 0;			// Entries 1 and 2 are cleared before every position
				mt.Porta3Target = 576;		// C-4
				mt.Old.VolumeSlide = 0;
				mt.Old.PanningSlide = 0;
				mt.PlayBackwards = false;
				mt.VibratoCounter = 0;
				mt.TriggerCounter = 0x7fffffff;
				mt.CutCounter = 0x7fffffff;
				mt.Retrigger = 0;
				mt.VolumeDelta = 0;
				mt.PanningDelta = 0;
				mt.Volume = 0;
				mt.Panning = 0;
				mt.PitchDelta = 0;
				mt.Porta3Delta = 0;
				mt.ArpTable[1] = 0;
				mt.ArpTable[2] = 0;
				mt.VibratoSpeed = 0;
				mt.VibratoDepth = 0;

				MSynth_Reset_Loop(mt);

				// Echo should be enabled or disabled depending on effect mask in the module. Default echo parameters
				// are initialized for all tracks however in case echo is turned on later with 'V' command
				mt.EchoDelay = mSyn.Module.DspDefaults.EchoDelay;
				mt.EchoFeedback = mSyn.Module.DspDefaults.EchoFeedback;
				mt.EchoMix = mSyn.Module.DspDefaults.EchoMix;
				mt.EchoCross = mSyn.Module.DspDefaults.EchoCross;

				if ((mSyn.Module.DspDefaults.EffectMask[track] & Constants.Dsp_Mask_Echo) != 0)
					MSynth_Echo_On_For_Track(mt, EchoType.Old);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MSynth_Reset(ModuleSynth mSyn)
		{
			mSyn.Tick = 0;
			mSyn.Speed = 6;
			mSyn.Tempo = 125;
			mSyn.PatternDelay = 0;
			mSyn.Pattern = 0;
			mSyn.Row = 0;
			mSyn.Order = 0;
			mSyn.Song = 0;
			mSyn.Mode = ModuleMode.Song;
			mSyn.GlobalVolume = 64;
			mSyn.GlobalVolumeSlide = 0;
			mSyn.ArpCounter = 0;
			mSyn.MinVolume = 0;
			mSyn.MinPanning = -768;
			mSyn.MinPitch = 576;
			mSyn.MaxVolume = 384;
			mSyn.MaxPanning = 768;
			mSyn.MaxPitch = 4608;				// Up to the start of 8-th octave
			mSyn.ManualUpdate = false;

			MSynth_Reset_Delayed(mSyn);
			MSynth_Init_Tracks(mSyn);

			worker.SetTempo((ushort)mSyn.Tempo);
		}



		/********************************************************************/
		/// <summary>
		/// Change the speed
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(int speed)
		{
			modSynth.Speed = speed;

			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Change the tempo
		/// </summary>
		/********************************************************************/
		private void ChangeTempo(int tempo)
		{
			modSynth.Tempo = tempo;

			worker.SetTempo((ushort)tempo);
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			worker.UpdateModuleInfo(InfoPositionLine, modSynth.Order.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			worker.UpdateModuleInfo(InfoPatternLine, modSynth.Pattern.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			worker.UpdateModuleInfo(InfoSpeedLine, modSynth.Speed.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			worker.UpdateModuleInfo(InfoTempoLine, modSynth.Tempo.ToString());
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Plugins;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Processing of pattern commands, song length calculation...
	/// Notes: This needs some heavy refactoring.
	///        I thought of actually adding an effect interface class. Every pattern effect
	///        could then be moved into its own class that inherits from the effect interface.
	///        If effect handling differs severely between module formats, every format would have
	///        its own class for that effect. Then, a call chain of effect classes could be set up
	///        for each format, since effects cannot be processed in the same order in all formats
	/// </summary>
	internal partial class CSoundFile
	{
		private const ModType GlobalVol_7Bit_Formats_Ext = ModType.None;
		private const ModType GlobalVol_7Bit_Formats = ModType.It | ModType.Mpt | ModType.Imf | ModType.J2B | ModType.Mid | ModType.Ams | ModType.Dbm | ModType.Ptm | ModType.Mdl | ModType.Dtm | GlobalVol_7Bit_Formats_Ext;

		private static readonly uint8[] st2TempoFactor = [ 140, 50, 25, 15, 10, 7, 6, 4, 3, 3, 2, 2, 2, 2, 1, 1 ];

		#region GetLengthMemory class
		/// <summary>
		/// Memory class for GetLength() code
		/// </summary>
		private class GetLengthMemory
		{
			public class ChnSettings : IDeepCloneable<ChnSettings>
			{
				/// <summary>
				/// When using sample sync, we still need to render this many ticks
				/// </summary>
				public uint32 TicksToRender = 0;

				/// <summary>
				/// When using sample sync, note frequency has changed
				/// </summary>
				public bool IncChanged = false;

				/// <summary>
				/// 
				/// </summary>
				public uint8 Vol = 0xff;

				/********************************************************************/
				/// <summary>
				/// Make a deep copy of the current object
				/// </summary>
				/********************************************************************/
				public ChnSettings MakeDeepClone()
				{
					return (ChnSettings)MemberwiseClone();
				}
			}

			public const uint32 Ignore_Channel = uint32.MaxValue;

			protected CSoundFile SndFile;
			public PlayState State;
			public vector<ChnSettings> ChnSettings_ = new vector<ChnSettings>();
			public c_double ElapsedTime;
			public readonly SequenceIndex m_Sequence;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public GetLengthMemory(CSoundFile sf, SequenceIndex sequence)
			{
				SndFile = sf;
				State = sf.m_PlayState.MakeDeepClone();
				m_Sequence = sequence;

				Reset();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Reset()
			{
				if (State.m_MidiMacroEvaluationResults != null)
					State.m_MidiMacroEvaluationResults = new PlayState.MidiMacroEvaluationResults();

				ElapsedTime = 0.0;

				State.m_lTotalSampleCount = 0;
				State.m_nMusicSpeed = SndFile.Order[m_Sequence].GetDefaultSpeed();
				State.m_nMusicTempo = SndFile.Order[m_Sequence].GetDefaultTempo();
				State.m_ppqPosFract = 0.0;
				State.m_ppqPosBeat = 0;
				State.m_nGlobalVolume = (int32)SndFile.m_nDefaultGlobalVolume;
				State.m_GlobalScriptState.Initialize(SndFile);

				ChnSettings_.assign(SndFile.GetNumChannels(), new ChnSettings());
				ChannelFlags muteFlag = CSoundFile.GetChannelMuteFlag();

				for (ChannelIndex chn = 0; chn < SndFile.GetNumChannels(); chn++)
				{
					State.Chn[chn].Reset(ModChannel.ResetFlags.Total, SndFile, chn, muteFlag);
					State.Chn[chn].nOldGlobalVolSlide = 0;
					State.Chn[chn].nOldChnVolSlide = 0;
					State.Chn[chn].nLastNote = ModCommand.Note_None;
				}
			}



			/********************************************************************/
			/// <summary>
			/// Increment playback position of sample and envelopes on a channel
			/// </summary>
			/********************************************************************/
			public void RenderChannel(ChannelIndex channel, uint32 tickDuration, uint32 portaStart = uint32.MaxValue)
			{
				ModChannel chn = State.Chn[channel];
				uint32 numTicks = ChnSettings_[channel].TicksToRender;

				if ((numTicks == Ignore_Channel) || (numTicks == 0) || (!chn.IsSamplePlaying() && !ChnSettings_[channel].IncChanged) || (chn.pModSample == null))
					return;

				SamplePosition loopStart = new SamplePosition((int32)(chn.dwFlags.Test(ChannelFlags.Chn_Loop) ? chn.nLoopStart : 0), 0);
				SamplePosition sampleEnd = new SamplePosition((int32)(chn.dwFlags.Test(ChannelFlags.Chn_Loop) ? chn.nLoopEnd : chn.nLength), 0);
				SmpLength loopLength = chn.nLoopEnd - chn.nLoopStart;
				bool itEnvMode = SndFile.m_PlayBehaviour[PlayBehaviour.ItEnvelopePositionHandling];
				bool updatePitchEnv = (chn.PitchEnv.Flags & (EnvelopeFlags.Enabled | EnvelopeFlags.Filter)) == EnvelopeFlags.Enabled;
				bool stopNote = false;

				SamplePosition inc = chn.Increment * tickDuration;

				if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongFlag))
					inc.Negate();

				for (uint32 i = 0; i < numTicks; i++)
				{
					bool updateInc = (chn.PitchEnv.Flags & (EnvelopeFlags.Enabled | EnvelopeFlags.Filter)) == EnvelopeFlags.Enabled;

					if (i >= portaStart)
					{
						State.m_nTickCount = i - portaStart;
						chn.IsFirstTick = (i == portaStart);

						CPointer<ModCommand> mp = SndFile.Patterns[State.m_nPattern].GetpModCommand(State.m_nRow, channel);
						ModCommand m = mp[0];
						EffectCommand command = m.Command;

						switch (m.VolCmd)
						{
							case VolumeCommand.TonePortamento:
							{
								(uint16 porta, bool clearEffectCommand) = SndFile.GetVolCmdTonePorta(m, 0);
								SndFile.TonePortamento(State, channel, porta);

								if (clearEffectCommand)
									command = EffectCommand.None;

								break;
							}

							case VolumeCommand.PortaUp:
							{
								SndFile.PortamentoUp(State, channel, (ModCommandParam)(m.Vol << 2), SndFile.m_PlayBehaviour[PlayBehaviour.ItVolColFinePortamento]);
								break;
							}

							case VolumeCommand.PortaDown:
							{
								SndFile.PortamentoDown(State, channel, (ModCommandParam)(m.Vol << 2), SndFile.m_PlayBehaviour[PlayBehaviour.ItVolColFinePortamento]);
								break;
							}
						}

						switch (command)
						{
							case EffectCommand.TonePortamento:
							{
								SndFile.TonePortamento(State, channel, m.Param);
								break;
							}

							case EffectCommand.TonePortaVol:
							{
								SndFile.TonePortamento(State, channel, 0);
								break;
							}

							case EffectCommand.PortamentoUp:
							{
								if ((m.Param != 0) || ((SndFile.GetType_() & ModType.Mod) == 0))
									SndFile.PortamentoUp(State, channel, m.Param, false);

								break;
							}

							case EffectCommand.PortamentoDown:
							{
								if ((m.Param != 0) || ((SndFile.GetType_() & ModType.Mod) == 0))
									SndFile.PortamentoDown(State, channel, m.Param, false);

								break;
							}

							case EffectCommand.ModCmdEx:
							{
								if (((m.Param & 0x0f) == 0) && ((SndFile.GetType_() & (ModType.Xm | ModType.Mt2)) == 0))
									break;

								if ((m.Param & 0xf0) == 0x10)
									SndFile.FinePortamentoUp(chn, (ModCommandParam)(m.Param & 0x0f));
								else if ((m.Param & 0xf0) == 0x20)
									SndFile.FinePortamentoDown(chn, (ModCommandParam)(m.Param & 0x0f));

								break;
							}

							case EffectCommand.XFinePortaUpDown:
							{
								if ((m.Param & 0xf0) == 0x10)
									SndFile.ExtraFinePortamentoUp(chn, (ModCommandParam)(m.Param & 0x0f));
								else if ((m.Param & 0xf0) == 0x20)
									SndFile.ExtraFinePortamentoDown(chn, (ModCommandParam)(m.Param & 0x0f));

								break;
							}

							case EffectCommand.NoteSlideUp:
							case EffectCommand.NoteSlideDown:
							case EffectCommand.NoteSlideUpRetrig:
							case EffectCommand.NoteSlideDownRetrig:
							{
								SndFile.NoteSlide(chn, m.Param, (command == EffectCommand.NoteSlideUp) || (command == EffectCommand.NoteSlideUpRetrig), (command == EffectCommand.NoteSlideUpRetrig) || (command == EffectCommand.NoteSlideDownRetrig));
								break;
							}
						}

						if (chn.AutoSlide.IsActive(AutoSlideCommand.TonePortamento) && !chn.RowCommand.IsTonePortamento())
							SndFile.TonePortamento(State, channel, chn.PortamentoSlide);
						else if (chn.AutoSlide.IsActive(AutoSlideCommand.TonePortamentoWithDuration))
							SndFile.TonePortamentoWithDuration(chn, 0);

						if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoUp))
							SndFile.PortamentoUp(State, channel, chn.nOldPortaUp, true);
						else if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoDown))
							SndFile.PortamentoDown(State, channel, chn.nOldPortaDown, true);
						else if (chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoUp))
							SndFile.FinePortamentoUp(chn, chn.nOldFinePortaUpDown);
						else if (chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoDown))
							SndFile.FinePortamentoDown(chn, chn.nOldFinePortaUpDown);

						if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoFc))
							SndFile.PortamentoFc(chn);

						updateInc = true;
					}

					int32 period = chn.nPeriod;

					if (itEnvMode)
						SndFile.IncrementEnvelopePositions(chn);

					if (updatePitchEnv)
					{
						SndFile.ProcessPitchFilterEnvelope(chn, ref period);
						updateInc = true;
					}

					if (!itEnvMode)
						SndFile.IncrementEnvelopePositions(chn);

					c_int vol = 0;
					SndFile.ProcessInstrumentFade(chn, ref vol);

					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib))
						continue;

					if (updateInc || ChnSettings_[channel].IncChanged)
					{
						if (chn.m_CalculateFreq || chn.m_RecalculateFreqOnFirstTick)
						{
							chn.RecalcTuningFreq(1, 0, SndFile);

							if (!chn.m_CalculateFreq)
								chn.m_RecalculateFreqOnFirstTick = false;
							else
								chn.m_CalculateFreq = false;
						}

						chn.Increment = SndFile.GetChannelIncrement(chn, (uint32)period, 0).first;
						ChnSettings_[channel].IncChanged = false;
						inc = chn.Increment * tickDuration;

						if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongFlag))
							inc.Negate();
					}

					chn.Position += inc;

					if ((chn.Position >= sampleEnd) || ((chn.Position < loopStart) && inc.IsNegative()))
					{
						if (!chn.dwFlags.Test(ChannelFlags.Chn_Loop) || (loopLength == 0))
						{
							// Past sample end
							stopNote = true;
							break;
						}

						// We exceeded the sample loop, go back to loop start
						if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongLoop))
						{
							if (chn.Position < loopStart)
							{
								chn.Position = new SamplePosition((int32)(chn.nLoopStart + chn.nLoopStart), 0) - chn.Position;
								chn.dwFlags.Flip(ChannelFlags.Chn_PingPongFlag);
								inc.Negate();
							}

							SmpLength posInt = chn.Position.GetUInt() - chn.nLoopStart;
							SmpLength pingPongLength = loopLength * 2;

							if (SndFile.m_PlayBehaviour[PlayBehaviour.ItPingPongMode])
								pingPongLength--;

							posInt %= pingPongLength;
							bool forward = posInt < loopLength;

							if (forward)
								chn.Position.SetInt((int32)(chn.nLoopStart + posInt));
							else
								chn.Position.SetInt((int32)(chn.nLoopEnd - (posInt - loopLength)));

							if (forward == chn.dwFlags.Test(ChannelFlags.Chn_PingPongFlag))
							{
								chn.dwFlags.Flip(ChannelFlags.Chn_PingPongFlag);
								inc.Negate();
							}
						}
						else
						{
							SmpLength posInt = chn.Position.GetUInt();

							if (posInt >= (chn.nLoopEnd + loopLength))
							{
								SmpLength overshoot = posInt - chn.nLoopEnd;
								posInt -= (overshoot / loopLength) * loopLength;
							}

							while (posInt >= chn.nLoopEnd)
								posInt -= loopLength;

							chn.Position.SetInt((int32)posInt);
						}
					}
				}

				State.m_nTickCount = 0;

				if (stopNote)
				{
					chn.Stop();
					chn.nPortamentoDest = 0;
				}

				ChnSettings_[channel].TicksToRender = 0;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void GlobalVolSlide(ModChannel chn, ModCommandParam param, uint32 nonRowTicks)
			{
				if (SndFile.m_SongFlags.Test(SongFlags.Auto_GlobalVol))
					chn.AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, param != 0);

				if (param != 0)
					chn.nOldGlobalVolSlide = param;
				else
					param = chn.nOldGlobalVolSlide;

				if (((param & 0x0f) == 0x0f) && ((param & 0xf0) != 0))
				{
					param >>= 4;

					if ((SndFile.GetType_() & GlobalVol_7Bit_Formats) == 0)
						param <<= 1;

					State.m_nGlobalVolume += param << 1;
				}
				else if (((param & 0xf0) == 0xf0) && ((param & 0x0f) != 0))
				{
					param = (ModCommandParam)((param & 0x0f) << 1);

					if ((SndFile.GetType_() & GlobalVol_7Bit_Formats) == 0)
						param <<= 1;

					State.m_nGlobalVolume -= param;
				}
				else if ((param & 0xf0) != 0)
				{
					param >>= 4;
					param <<= 1;

					if ((SndFile.GetType_() & GlobalVol_7Bit_Formats) == 0)
						param <<= 1;

					State.m_nGlobalVolume += (int32)(param * nonRowTicks);
				}
				else
				{
					param = (ModCommandParam)((param & 0x0f) << 1);

					if ((SndFile.GetType_() & GlobalVol_7Bit_Formats) == 0)
						param <<= 1;

					State.m_nGlobalVolume -= (int32)(param * nonRowTicks);
				}

				OpenMpt.Limit(ref State.m_nGlobalVolume, 0, 256);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 GetLinearSlideDownTable(CSoundFile sndFile, uint32 i)//XX 43
		{
			return sndFile.m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] ? Tables.LinearSlideDownTable[i] : Tables.LinearSlideUpTable[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 GetLinearSlideUpTable(CSoundFile sndFile, uint32 i)//XX 44
		{
			return sndFile.m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] ? Tables.LinearSlideUpTable[i] : Tables.LinearSlideDownTable[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 GetFineLinearSlideDownTable(CSoundFile sndFile, uint32 i)//XX 45
		{
			return sndFile.m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] ? Tables.FineLinearSlideDownTable[i] : Tables.FineLinearSlideUpTable[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 GetFineLinearSlideUpTable(CSoundFile sndFile, uint32 i)//XX 46
		{
			return sndFile.m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] ? Tables.FineLinearSlideUpTable[i] : Tables.FineLinearSlideDownTable[i];
		}



		/********************************************************************/
		/// <summary>
		/// Minimum parameter of tempo command that is considered to be a
		/// BPM rather than a tempo slide
		/// </summary>
		/********************************************************************/
		private static Tempo GetMinimumTempoParam(ModType modType)//XX 49
		{
			return (modType & (ModType.Mdl | ModType.Med | ModType.Xm | ModType.Mod)) != 0 ? new Tempo(1, 0) : new Tempo(32, 0);
		}

		//XX 55
		#region Length
		/********************************************************************/
		/// <summary>
		/// Get mod length in various cases. Parameters:
		/// [in]  adjustMode: See enmGetLengthResetMode for possible adjust
		///       modes.
		/// [out] See definition of type GetLengthType for the returned
		///       values.
		/// </summary>
		/********************************************************************/
		public vector<GetLengthType> GetLength(EnmGetLengthResetMode adjustMode)//XX 348
		{
			return GetLength(adjustMode, new GetLengthTarget());
		}



		/********************************************************************/
		/// <summary>
		/// Get mod length in various cases. Parameters:
		/// [in]  adjustMode: See enmGetLengthResetMode for possible adjust
		///       modes.
		/// [in]  target: Time or position target which should be reached,
		///       or no target to get length of the first sub song. Use
		///       GetLengthTarget::StartPos to also specify a position from
		///       where the seeking should begin.
		/// [out] See definition of type GetLengthType for the returned
		///       values.
		/// </summary>
		/********************************************************************/
		public vector<GetLengthType> GetLength(EnmGetLengthResetMode adjustMode, GetLengthTarget target)//XX 348
		{
			vector<GetLengthType> results = new vector<GetLengthType>();
			GetLengthType retVal = new GetLengthType();

			// Are we trying to reach a certain pattern position?
			bool hasSearchTarget = (target.Mode != GetLengthTarget.Mode_.NoTarget) && (target.Mode != GetLengthTarget.Mode_.GetAllSubsongs);
			bool adjustSamplePos = (adjustMode & EnmGetLengthResetMode.eAdjustSamplePositions) == EnmGetLengthResetMode.eAdjustSamplePositions;

			SequenceIndex sequence = target.Sequence;

			if (sequence >= Order.GetNumSequences())
				sequence = Order.GetCurrentSequenceIndex();

			ModSequence orderList = Order[sequence];

			GetLengthMemory memory = new GetLengthMemory(this, sequence);
			PlayState playState = memory.State;

			// Temporary visited rows vector (so that GetLength() won't interfere with the
			// player code if the module is playing at the same time)
			RowVisitor visitedRows = new RowVisitor(this, sequence);
			RowIndex allowedPatternLoopComplexity = 32768;

			// If sequence starts with some non-existent patterns, find a better start
			while ((target.StartOrder < orderList.size()) && !orderList.IsValidPat(target.StartOrder))
			{
				target.StartOrder++;
				target.StartRow = 0;
			}

			retVal.StartRow = playState.m_nNextRow = playState.m_nRow = target.StartRow;
			retVal.StartOrder = playState.m_nNextOrder = playState.m_nCurrentOrder = target.StartOrder;

			// Fast LUTs for commands that are too weird / complicated / whatever to emulate
			// in sample position adjust mode
			EnumBitSet<EffectCommand> forbiddenCommands = new EnumBitSet<EffectCommand>(EffectCommand.Max_Effects);

			if (adjustSamplePos)
			{
				forbiddenCommands.set(EffectCommand.Arpeggio);

				if ((target.Mode == GetLengthTarget.Mode_.SeekPosition) && (target.Pos.Order < orderList.size()))
				{
					// If we know where to seek, we can directly rule out any channels on which a
					// new note would be triggered right at the start
					PatternIndex seekPat = orderList[target.Pos.Order];

					if (Patterns.IsValidPat(seekPat) && Patterns[seekPat].IsValidRow(target.Pos.Row))
					{
						CPointer<ModCommand> m = Patterns[seekPat].GetpModCommand(target.Pos.Row, 0);

						for (ChannelIndex i = 0; i < GetNumChannels(); i++, m++)
						{
							if ((m[0].Note == ModCommand.Note_NoteCut) || (m[0].Note == ModCommand.Note_KeyOff) || ((m[0].Note == ModCommand.Note_Fade) && (GetNumInstruments() != 0)) || (m[0].IsNote() && (m[0].Instr != 0) && !m[0].IsTonePortamento()))
								memory.ChnSettings_[i].TicksToRender = GetLengthMemory.Ignore_Channel;
						}
					}
				}
			}

			if ((adjustMode & EnmGetLengthResetMode.eAdjust) != 0)
				playState.m_MidiMacroEvaluationResults = new PlayState.MidiMacroEvaluationResults();

			// If samples are being synced, force them to resync if tick duration changes
			uint32 oldTickDuration = 0;
			bool breakToRow = false;

			for (;;)
			{
				bool ignoreRow = NextRow(playState, breakToRow).first;

				// Time target reached
				if ((target.Mode == GetLengthTarget.Mode_.SeekSeconds) && (memory.ElapsedTime >= target.Time))
				{
					retVal.TargetReached = true;
					break;
				}

				// Check if pattern is valid
				playState.m_nPattern = playState.m_nCurrentOrder < orderList.size() ? orderList[playState.m_nCurrentOrder] : Snd_Def.PatternIndex_Invalid;
				playState.m_nTickCount = 0;

				if (!Patterns.IsValidPat(playState.m_nPattern) && (playState.m_nPattern != Snd_Def.PatternIndex_Invalid) && (target.Mode == GetLengthTarget.Mode_.SeekPosition) && (playState.m_nCurrentOrder == target.Pos.Order))
				{
					// Early test: Target is inside +++ or non-existing pattern
					retVal.TargetReached = true;
					break;
				}

				while (playState.m_nPattern >= Patterns.Size())
				{
					// End of song?
					if ((playState.m_nPattern == Snd_Def.PatternIndex_Invalid) || (playState.m_nCurrentOrder >= orderList.size()))
					{
						if (playState.m_nCurrentOrder == orderList.GetRestartPos())
							break;
						else
							playState.m_nCurrentOrder = orderList.GetRestartPos();
					}
					else
						playState.m_nCurrentOrder++;

					playState.m_nPattern = (playState.m_nCurrentOrder < orderList.size()) ? orderList[playState.m_nCurrentOrder] : Snd_Def.PatternIndex_Invalid;
					playState.m_nNextOrder = playState.m_nCurrentOrder;

					if (!Patterns.IsValidPat(playState.m_nPattern) && visitedRows.Visit(playState.m_nCurrentOrder, 0, playState.Chn, ignoreRow))
					{
						if (!hasSearchTarget)
						{
							retVal.RestartOrder = playState.m_nCurrentOrder;
							retVal.RestartRow = 0;
						}

						if ((target.Mode == GetLengthTarget.Mode_.NoTarget) || !visitedRows.GetFirstUnvisitedRow(out playState.m_nNextOrder, out playState.m_nRow, true))
						{
							// We aren't searching for a specific row, or we couldn't find any more unvisited rows
							break;
						}
						else
						{
							// We haven't found the target row yet, but we found some other unplayed row... continue searching from here
							retVal.Duration = memory.ElapsedTime;
							results.push_back(retVal);
							retVal.StartRow = playState.m_nRow;
							retVal.StartOrder = playState.m_nNextOrder;
							memory.Reset();

							playState.m_nCurrentOrder = playState.m_nNextOrder;
							playState.m_nPattern = orderList[playState.m_nCurrentOrder];
							playState.m_nNextRow = playState.m_nRow;
							break;
						}
					}
				}

				if (playState.m_nNextOrder == Snd_Def.OrderIndex_Invalid)
				{
					// GetFirstUnvisitedRow failed, so there is nothing more to play
					break;
				}

				// Skip non-existing patterns
				if (!Patterns.IsValidPat(playState.m_nPattern))
				{
					// If there isn't even a tune, we should probably stop here
					if (playState.m_nCurrentOrder == orderList.GetRestartPos())
					{
						if ((target.Mode == GetLengthTarget.Mode_.NoTarget) || !visitedRows.GetFirstUnvisitedRow(out playState.m_nNextOrder, out playState.m_nRow, true))
						{
							// We aren't searching for a specific row, or we couldn't find any more unvisited rows
							break;
						}
						else
						{
							// We haven't found the target row yet, but we found some other unplayed row... continue searching from here
							retVal.Duration = memory.ElapsedTime;
							results.push_back(retVal);
							retVal.StartRow = playState.m_nRow;
							retVal.StartOrder = playState.m_nNextOrder;
							memory.Reset();

							playState.m_nNextRow = playState.m_nRow;
							continue;
						}
					}

					playState.m_nNextOrder = (OrderIndex)(playState.m_nCurrentOrder + 1);
					continue;
				}

				// Should never happen
				if (playState.m_nRow >= Patterns[playState.m_nPattern].GetNumRows())
					playState.m_nRow = 0;

				// Check whether target was reached
				if ((target.Mode == GetLengthTarget.Mode_.SeekPosition) && (playState.m_nCurrentOrder == target.Pos.Order) && (playState.m_nRow == target.Pos.Row))
				{
					retVal.TargetReached = true;
					break;
				}

				// If pattern loops are nested too deeply, they can cause an effectively infinite amount of loop evaluations to be generated.
				// As we don't want the user to wait forever, we bail out if the pattern loops are too complex
				bool moduleTooComplex = (target.Mode != GetLengthTarget.Mode_.SeekSeconds) && visitedRows.ModuleTooComplex(allowedPatternLoopComplexity);

				if (moduleTooComplex)
				{
					memory.ElapsedTime = c_double.PositiveInfinity;

					// Decrease allowed complexity with each subsong, as this seems to be a malicious module
					if (allowedPatternLoopComplexity > 256)
						allowedPatternLoopComplexity /= 2;

					visitedRows.ResetComplexity();
				}

				if (visitedRows.Visit(playState.m_nCurrentOrder, playState.m_nRow, playState.Chn, ignoreRow) || moduleTooComplex)
				{
					if (!hasSearchTarget)
					{
						retVal.RestartOrder = playState.m_nCurrentOrder;
						retVal.RestartRow = playState.m_nRow;
					}

					if ((target.Mode == GetLengthTarget.Mode_.NoTarget) || !visitedRows.GetFirstUnvisitedRow(out playState.m_nNextOrder, out playState.m_nRow, true))
					{
						// We aren't searching for a specific row, or we couldn't find any more unvisited rows
						break;
					}
					else
					{
						// We haven't found the target row yet, but we found some other unplayed row... continue searching from here
						retVal.Duration = memory.ElapsedTime;
						results.push_back(retVal);
						retVal.StartRow = playState.m_nRow;
						retVal.StartOrder = playState.m_nNextOrder;
						memory.Reset();

						playState.m_nNextRow = playState.m_nRow;
						continue;
					}
				}

				retVal.EndOrder = playState.m_nCurrentOrder;
				retVal.EndRow = playState.m_nRow;

				// Update next position
				SetupNextRow(playState, false);

				// Jumped to invalid pattern row?
				if (playState.m_nRow >= Patterns[playState.m_nPattern].GetNumRows())
					playState.m_nRow = 0;

				playState.UpdatePpq(breakToRow);
				playState.UpdateTimeSignature(this);

				if (ignoreRow)
					continue;

				// For various effects, we need to know first how many ticks there are in this row
				CPointer<ModCommand> pp = Patterns[playState.m_nPattern].GetpModCommand(playState.m_nRow, 0);
				bool ignoreMutedChn = m_PlayBehaviour[PlayBehaviour.St3NoMutedChannels];

				for (ChannelIndex nChn = 0; nChn < GetNumChannels(); nChn++, pp++)
				{
					ModCommand p = pp[0];
					ModChannel chn = playState.Chn[nChn];
					chn.IsFirstTick = true;

					if (p.IsEmpty() || (ignoreMutedChn && ChnSettings[nChn].dwFlags.Test(ChannelFlags.Chn_Mute)))	// Not even effects are processed on muted S3M channels
					{
						chn.RowCommand.Clear();
						continue;
					}

					if (p.IsPcNote())
					{
						chn.RowCommand.Clear();
						continue;
					}

					if (p.IsNote())
						chn.nNewNote = chn.nLastNote = p.Note;
					else if ((p.Note > ModCommand.Note_Max) && m_PlayBehaviour[PlayBehaviour.ItClearOldNoteAfterCut])
						chn.nNewNote = ModCommand.Note_None;

					if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell] && (p.Instr > 0) && (p.Instr <= GetNumInstruments()) && (Instruments[p.Instr] != null) && !Instruments[p.Instr].HasValidMidiChannel())
					{
						uint8 note = (chn.RowCommand.Note != ModCommand.Note_None) ? p.Note : chn.nNewNote;

						if (ModCommand.IsNote(note) && (Instruments[p.Instr].Keyboard[note - ModCommand.Note_Min] == 0))
						{
							chn.nNewNote = chn.nLastNote = note;
							chn.nNewIns = p.Instr;
							chn.RowCommand.Clear();
							continue;
						}
					}

					p.CopyTo(chn.RowCommand);

					switch (p.Command)
					{
						case EffectCommand.Speed:
						{
							SetSpeed(playState, p.Param);
							break;
						}

						case EffectCommand.Tempo:
						{
							if (m_PlayBehaviour[PlayBehaviour.ModVBlankTiming])
							{
								// ProTracker MODs with VBlank timing: All Fxx parameters set the tick count
								if (p.Param != 0)
									SetSpeed(playState, p.Param);
							}

							// Regular tempo handled below
							break;
						}

						case EffectCommand.S3MCmdEx:
						{
							if ((chn.RowCommand.Param == 0) && ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0))
								chn.RowCommand.Param = chn.nOldCmdEx;
							else
								chn.nOldCmdEx = (ModCommandParam)chn.RowCommand.Param;

							if ((p.Param & 0xf0) == 0x60)
							{
								// Fine Pattern Delay
								playState.m_nFrameDelay += (uint32)(p.Param & 0x0f);
							}
							else if (((p.Param & 0xf0) == 0xe0) && (playState.m_nPatternDelay == 0))
							{
								// Pattern Delay
								if (((GetType_() & ModType.S3M) == 0) || ((p.Param & 0x0f) != 0))
								{
									// While Impulse Tracker *does* count S60 as a valid row delay (and thus ignores any other row delay commands on the right),
									// Scream Tracker 3 simply ignores such commands
									playState.m_nPatternDelay = (uint32)(1 + (p.Param & 0x0f));
								}
							}

							break;
						}

						case EffectCommand.ModCmdEx:
						{
							if ((p.Param & 0xf0) == 0xe0)
							{
								// Pattern Delay
								playState.m_nPatternDelay = (uint32)(1 + (p.Param & 0x0f));
							}

							break;
						}
					}
				}

				// This may change speed/tempo/global volume/next row
				playState.m_GlobalScriptState.NextTick(playState, this);

				uint32 numTicks = playState.TicksOnRow();
				uint32 nonRowTicks = numTicks - Math.Max(playState.m_nPatternDelay, 1);

				playState.m_PatLoopRow = Snd_Def.RowIndex_Invalid;
				playState.m_BreakRow = Snd_Def.RowIndex_Invalid;
				playState.m_PosJump = Snd_Def.OrderIndex_Invalid;

				for (ChannelIndex nChn = 0; nChn < GetNumChannels(); nChn++)
				{
					ModChannel chn = playState.Chn[nChn];

					if (chn.RowCommand.IsEmpty() && !chn.AutoSlide.AnyActive())
						continue;

					ModCommandCommand command = chn.RowCommand.Command;
					ModCommandParam param = chn.RowCommand.Param;
					ModCommandNote note = chn.RowCommand.Note;

					if (((adjustMode & EnmGetLengthResetMode.eAdjust) != 0) && !chn.RowCommand.IsEmpty())
					{
						if (chn.RowCommand.Instr != 0)
						{
							chn.SwapSampleIndex = chn.nNewIns = chn.RowCommand.Instr;
							memory.ChnSettings_[nChn].Vol = 0xff;
						}

						if (chn.RowCommand.IsNote())
						{
							chn.RestorePanAndFilter();

							if (!adjustSamplePos || (memory.ChnSettings_[nChn].TicksToRender == GetLengthMemory.Ignore_Channel))
							{
								// Even if we don't intend to render anything on this channel, update instrument
								// cutoff/resonance because it might override a Zxx effect evaluated earlier
								ModInstrument instr = chn.pModInstrument;

								if ((chn.nNewIns != 0) && (chn.nNewIns <= GetNumInstruments()))
									instr = Instruments[chn.nNewIns];

								if (instr != null)
								{
									if (instr.IsCutOffEnabled())
										chn.nCutOff = instr.GetCutOff();

									if (instr.IsResonanceEnabled())
										chn.nResonance = instr.GetResonance();
								}

								bool wasGlobalSlideRunning = chn.AutoSlide.IsActive(AutoSlideCommand.GlobalVolumeSlide);
								chn.AutoSlide.Reset();
								chn.AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, wasGlobalSlideRunning);
							}
						}

						// Update channel panning
						if (chn.RowCommand.IsNote() || (chn.RowCommand.Instr != 0))
						{
							ModInstrument pIns;

							if ((chn.nNewIns > 0) && (chn.nNewIns <= GetNumInstruments()) && ((pIns = Instruments[chn.nNewIns]) != null))
							{
								if (pIns.dwFlags.Test(InstrumentFlags.SetPanning))
									chn.SetInstrumentPan((int32)pIns.nPan, this);
							}

							SampleIndex smp = GetSampleIndex(note, chn.nNewIns);

							if (smp > 0)
							{
								if (Samples[smp].uFlags.Test(ChannelFlags.Chn_Panning))
									chn.SetInstrumentPan(Samples[smp].nPan, this);
							}
						}

						switch (chn.RowCommand.VolCmd)
						{
							case VolumeCommand.Volume:
							{
								memory.ChnSettings_[nChn].Vol = chn.RowCommand.Vol;
								break;
							}

							case VolumeCommand.VolSlideUp:
							case VolumeCommand.VolSlideDown:
							{
								if (chn.RowCommand.Vol != 0)
									chn.nOldVolParam = chn.RowCommand.Vol;

								break;
							}

							case VolumeCommand.TonePortamento:
							{
								if (chn.RowCommand.Vol != 0)
								{
									(uint16 porta, bool clearEffectCommand) = GetVolCmdTonePorta(chn.RowCommand, 0);
									chn.PortamentoSlide = porta;

									if (clearEffectCommand)
										command = EffectCommand.None;
								}

								break;
							}
						}
					}

					switch (command)
					{
						// Position jump
						case EffectCommand.PositionJump:
						{
							PositionJump(playState, nChn);
							break;
						}

						// Pattern break
						case EffectCommand.PatternBreak:
						{
							RowIndex row = PatternBreak(playState, nChn, param);

							if (row != Snd_Def.RowIndex_Invalid)
								playState.m_BreakRow = row;

							break;
						}

						// Set tempo
						case EffectCommand.Tempo:
						{
							if (!m_PlayBehaviour[PlayBehaviour.ModVBlankTiming])
							{
								Tempo tempo = new Tempo(CalculateXParam(playState.m_nPattern, playState.m_nRow, nChn, out _), 0);

								if ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0)
								{
									if (tempo.GetInt() != 0)
										chn.nOldTempo = (uint8)tempo.GetInt();
									else
										tempo.Set(chn.nOldTempo);
								}

								if (tempo >= GetMinimumTempoParam(GetType_()))
								{
									playState.m_Flags.Set(PlayFlags.Song_FirstTick, !m_PlayBehaviour[PlayBehaviour.ModTempoOnSecondTick]);
									SetTempo(playState, tempo, false);
								}
								else
								{
									// Tempo Slide
									playState.m_Flags.Reset(PlayFlags.Song_FirstTick);

									for (uint32 i = 0; i < nonRowTicks; i++)
										SetTempo(playState, tempo, false);
								}
							}

							break;
						}

						case EffectCommand.S3MCmdEx:
						{
							switch (param & 0xf0)
							{
								// Pattern Loop
								case 0xb0:
								{
									PatternLoop(playState, nChn, (ModCommandParam)(param & 0x0f));
									break;
								}
							}

							break;
						}

						case EffectCommand.ModCmdEx:
						{
							switch (param & 0xf0)
							{
								// Pattern Loop
								case 0x60:
								{
									PatternLoop(playState, nChn, (ModCommandParam)(param & 0x0f));
									break;
								}
							}

							break;
						}
					}

					// The following calculations are not interesting if we just want to get the song length...
					// ...unless we're playing a Face The Music module with scripts that may modify the speed or tempo based on some volume or pitch variable (see schlendering.ftm)
					if (((adjustMode & EnmGetLengthResetMode.eAdjust) == 0) && m_GlobalScript.empty())
						continue;

					ResetAutoSlides(chn);

					switch (command)
					{
						// Portamento Up/Down
						case EffectCommand.PortamentoUp:
						{
							if (param != 0)
							{
								// FT2 compatibility: Separate effect memory for all portamento commands
								// Test case: Porta-LinkMem.xm
								if (!m_PlayBehaviour[PlayBehaviour.Ft2PortaUpDownMemory])
									chn.nOldPortaDown = param;

								chn.nOldPortaUp = param;
							}

							break;
						}

						case EffectCommand.PortamentoDown:
						{
							if (param != 0)
							{
								// FT2 compatibility: Separate effect memory for all portamento commands
								// Test case: Porta-LinkMem.xm
								if (!m_PlayBehaviour[PlayBehaviour.Ft2PortaUpDownMemory])
									chn.nOldPortaUp = param;

								chn.nOldPortaDown = param;
							}

							break;
						}

						// Tone-Portamento
						case EffectCommand.TonePortamento:
						{
							if (param != 0)
								chn.PortamentoSlide = param;

							break;
						}

						// Offset
						case EffectCommand.Offset:
						{
							if (param != 0)
								chn.OldOffset = (SmpLength)(param << 8);

							break;
						}

						// Volume Slide
						case EffectCommand.VolumeSlide:
						case EffectCommand.TonePortaVol:
						{
							if (param != 0)
								chn.nOldVolumeSlide = param;

							break;
						}

						case EffectCommand.Auto_VolumeSlide:
						{
							AutoVolumeSlide(chn, param);
							break;
						}

						case EffectCommand.VolumeDown_Etx:
						{
							VolumeDownEtx(playState, chn, param);
							break;
						}

						// Set Volume
						case EffectCommand.Volume:
						{
							memory.ChnSettings_[nChn].Vol = param;
							break;
						}

						case EffectCommand.Volume8:
						{
							memory.ChnSettings_[nChn].Vol = (uint8)((param + 3U) / 4U);
							break;
						}

						// Global Volume
						case EffectCommand.GlobalVolume:
						{
							if (((GetType_() & GlobalVol_7Bit_Formats) == 0) && (param < 128))
								param *= 2;

							// IT compatibility 16. ST3 and IT ignore out-of-range values
							if (param <= 128)
								playState.m_nGlobalVolume = param * 2;
							else if ((GetType_() & (ModType.It | ModType.Mpt | ModType.S3M)) == 0)
								playState.m_nGlobalVolume = 256;

							playState.Chn[m_PlayBehaviour[PlayBehaviour.PerChannelGlobalVolSlide] ? nChn : 0].AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, false);
							break;
						}

						// Global Volume Slide
						case EffectCommand.GlobalVolSlide:
						{
							memory.GlobalVolSlide(playState.Chn[m_PlayBehaviour[PlayBehaviour.PerChannelGlobalVolSlide] ? nChn : 0], param, nonRowTicks);
							break;
						}

						case EffectCommand.ChannelVolume:
						{
							if (param <= 64)
								chn.nGlobalVol = param;

							break;
						}

						case EffectCommand.ChannelVolSlide:
						{
							if (param != 0)
								chn.nOldChnVolSlide = param;
							else
								param = chn.nOldChnVolSlide;

							int32 volume = chn.nGlobalVol;

							if (((param & 0x0f) == 0x0f) && ((param & 0xf0) != 0))
								volume += (param >> 4);		// Fine Up
							else if (((param & 0xf0) == 0xf0) && ((param & 0x0f) != 0))
								volume -= (param & 0x0f);	// Fine Down
							else if ((param & 0x0f) != 0)	// Down
								volume -= (int32)((param & 0x0f) * nonRowTicks);
							else							// Up
								volume += (int32)(((param & 0xf0) >> 4) * nonRowTicks);

							OpenMpt.Limit(ref volume, 0, 64);
							chn.nGlobalVol = (uint8)volume;
							break;
						}

						case EffectCommand.VolumeDown_Duration:
						{
							ChannelVolumeDownWithDuration(chn, param);
							break;
						}

						case EffectCommand.Panning8:
						{
							Panning(chn, param, PanningType.Pan8Bit);
							break;
						}

						case EffectCommand.ModCmdEx:
						{
							switch (param & 0xf0)
							{
								// LED filter
								case 0x00:
								{
									for (ChannelIndex channel = 0; channel < GetNumChannels(); channel++)
										playState.Chn[channel].dwFlags.Set(ChannelFlags.Chn_AmigaFilter, (param & 1) == 0);

									break;
								}

								// Panning
								case 0x80:
								{
									Panning(chn, (uint32)(param & 0x0f), PanningType.Pan4Bit);
									break;
								}

								// Active macro
								case 0xf0:
								{
									chn.nActiveMacro = (uint8)(param & 0x0f);
									break;
								}
							}

							break;
						}

						case EffectCommand.S3MCmdEx:
						{
							switch (param & 0xf0)
							{
								// Panning
								case 0x80:
								{
									Panning(chn, (uint32)(param & 0x0f), PanningType.Pan4Bit);
									break;
								}

								// Extended channel effects
								case 0x90:
								{
									// Change play direction is handled in adjustSamplePos case
									if (param < 0x9e)
										ExtendedChannelEffect(chn, param, playState);

									break;
								}

								// High sample offset
								case 0xa0:
								{
									chn.nOldHiOffset = (uint8)(param & 0x0f);
									break;
								}

								// Active macro
								case 0xf0:
								{
									chn.nActiveMacro = (uint8)(param & 0x0f);
									break;
								}
							}

							break;
						}

						case EffectCommand.XFinePortaUpDown:
						{
							// Ignore high offset in compatible mode
							if (((param & 0xf0) == 0xa0) && !m_PlayBehaviour[PlayBehaviour.Ft2RestrictXCommand])
								chn.nOldHiOffset = (uint8)(param & 0x0f);

							break;
						}

						case EffectCommand.VibratoVol:
						{
							if (param != 0)
								chn.nOldVolumeSlide = param;

							param = 0;
							goto case EffectCommand.Vibrato;
						}

						case EffectCommand.Vibrato:
						{
							Vibrato(chn, param);
							break;
						}

						case EffectCommand.FineVibrato:
						{
							FineVibrato(chn, param);
							break;
						}

						case EffectCommand.Tremolo:
						{
							Tremolo(chn, param);
							break;
						}

						case EffectCommand.Panbrello:
						{
							Panbrello(chn, param);

							// Panbrello effect is permanent in compatible mode, so actually apply panbrello for the last tick of this row
							chn.nPanbrelloPos += (uint8)(chn.nPanbrelloSpeed * nonRowTicks);
							ProcessPanbrello(chn);
							break;
						}

						case EffectCommand.Midi:
						case EffectCommand.SmoothMidi:
						{
							if (param < 0x80)
								ProcessMidiMacro(playState, nChn, false, m_MidiCfg.SFx[chn.nActiveMacro], chn.RowCommand.Param, 0);
							else
								ProcessMidiMacro(playState, nChn, false, m_MidiCfg.Zxx[param & 0x7f], chn.RowCommand.Param, 0);

							break;
						}
					}

					switch (chn.RowCommand.VolCmd)
					{
						case VolumeCommand.Panning:
						{
							Panning(chn, chn.RowCommand.Vol, PanningType.Pan6Bit);
							break;
						}

						case VolumeCommand.VibratoSpeed:
						{
							// FT2 does not automatically enable vibrato with the "set vibrato speed" command
							if (m_PlayBehaviour[PlayBehaviour.Ft2VolColVibrato])
								chn.nVibratoSpeed = (uint8)(chn.RowCommand.Vol & 0x0f);
							else
								Vibrato(chn, (uint32)(chn.RowCommand.Vol << 4));

							break;
						}

						case VolumeCommand.VibratoDepth:
						{
							Vibrato(chn, chn.RowCommand.Vol);
							break;
						}
					}

					chn.IsFirstTick = true;

					if (chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideUp) && (command != EffectCommand.Auto_VolumeSlide))
						FineVolumeUp(chn, 0, false);

					if (chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideDown) && (command != EffectCommand.Auto_VolumeSlide))
						FineVolumeDown(chn, 0, false);

					if (chn.AutoSlide.IsActive(AutoSlideCommand.VolumeSlideStk))
					{
						for (uint32 i = 0; i < numTicks; i++)
						{
							chn.IsFirstTick = (i == 0);
							VolumeSlide(chn, 0);
						}
					}

					if (chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownWithDuration))
					{
						chn.VolSlideDownRemain -= Math.Min(chn.VolSlideDownRemain, uint16.CreateSaturating(numTicks - 1));
						ChannelVolumeDownWithDuration(chn);
					}

					if (chn.AutoSlide.IsActive(AutoSlideCommand.GlobalVolumeSlide) && (command != EffectCommand.GlobalVolSlide))
						memory.GlobalVolSlide(chn, chn.nOldGlobalVolSlide, nonRowTicks);

					if ((command == EffectCommand.Vibrato) || (command == EffectCommand.FineVibrato) || (command == EffectCommand.VibratoVol) || chn.AutoSlide.IsActive(AutoSlideCommand.Vibrato))
					{
						uint32 vibTicks = ((GetType_() & (ModType.It | ModType.Mpt)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects) ? numTicks : nonRowTicks;
						uint32 inc = chn.nVibratoSpeed * vibTicks;

						if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
							inc *= 4;

						chn.nVibratoPos += (uint8)inc;
					}

					if ((command == EffectCommand.Tremolo) || chn.AutoSlide.IsActive(AutoSlideCommand.Tremolo))
					{
						uint32 tremTicks = ((GetType_() & (ModType.It | ModType.Mpt)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects) ? numTicks : nonRowTicks;
						uint32 inc = chn.nTremoloSpeed * tremTicks;

						if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
							inc *= 4;

						chn.nTremoloPos += (uint8)inc;
					}

					if (m_PlayBehaviour[PlayBehaviour.St3EffectMemory] && (command != EffectCommand.None) && (param != 0))
						UpdateS3MEffectMemory(chn, param);
				}

				if (!m_GlobalScript.empty())
				{
					for (playState.m_nTickCount = 1; playState.m_nTickCount < numTicks; playState.m_nTickCount++)
						playState.m_GlobalScriptState.NextTick(playState, this);
				}

				// Interpret F00 effect in XM files as "stop song"
				if ((GetType_() == ModType.Xm) && (playState.m_nMusicSpeed == uint16.MaxValue))
				{
					playState.m_nNextRow = playState.m_nRow;
					playState.m_nNextOrder = playState.m_nCurrentOrder;
					continue;
				}

				uint32 tickDuration = GetTickDuration(playState);
				uint32 rowDuration = tickDuration * numTicks;
				memory.ElapsedTime += (c_double)rowDuration / m_MixerSettings.gdwMixingFreq;
				playState.m_lTotalSampleCount += rowDuration;

				RowIndex rowsPerBeat = playState.m_nCurrentRowsPerBeat != 0 ? playState.m_nCurrentRowsPerBeat : Snd_Def.Default_Rows_Per_Beat;
				playState.m_ppqPosFract += 1.0 / rowsPerBeat;

				if (adjustSamplePos)
				{
					// Super experimental and dirty sample seeking
					for (ChannelIndex nChn = 0; nChn < GetNumChannels(); nChn++)
					{
						if (memory.ChnSettings_[nChn].TicksToRender == GetLengthMemory.Ignore_Channel)
							continue;

						ModChannel chn = playState.Chn[nChn];
						ModCommand m = chn.RowCommand;

						if ((chn.nPeriod == 0) && m.IsEmpty())
							continue;

						uint32 paramHi = (uint32)(m.Param >> 4);
						uint32 paramLo = (uint32)(m.Param & 0x0f);
						uint32 startTick = 0;
						bool porta = m.IsTonePortamento();
						bool stopNote = false;

						if (m.Instr != 0)
							chn.PrevNoteOffset = 0;

						if (m.IsNote())
						{
							if (porta && memory.ChnSettings_[nChn].IncChanged)
							{
								// If there's a portamento, the current channel increment mustn't be 0 in NoteChange()
								chn.Increment = GetChannelIncrement(chn, (uint32)chn.nPeriod, 0).first;
							}

							int32 setPan = chn.nPan;

							if (chn.nNewIns != 0)
								InstrumentChange(chn, chn.nNewIns, porta);

							NoteChange(chn, m.Note, porta);
							HandleNoteChangeFilter(chn);
							HandleDigiSamplePlayDirection(playState, nChn);
							memory.ChnSettings_[nChn].IncChanged = true;

							if (((m.Command == EffectCommand.ModCmdEx) || (m.Command == EffectCommand.S3MCmdEx)) && ((m.Param & 0xf0) == 0xd0) && (paramLo < numTicks))
								startTick = paramLo;
							else if ((m.Command == EffectCommand.DelayCut) && (paramHi < numTicks))
								startTick = paramHi;

							if ((playState.m_nPatternDelay > 1) && (startTick != 0) && ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0))
								startTick += (playState.m_nMusicSpeed + playState.m_nFrameDelay) * (playState.m_nPatternDelay - 1);

							if (!porta)
								memory.ChnSettings_[nChn].TicksToRender = 0;

							// Panning commands have to be re-applied after a note change with potential pan change
							if ((m.Command == EffectCommand.Panning8) || (((m.Command == EffectCommand.ModCmdEx) || (m.Command == EffectCommand.S3MCmdEx)) && (paramHi == 0x8)) || (m.VolCmd == VolumeCommand.Panning))
								chn.nPan = setPan;
						}

						if (m.IsNote() || m_PlayBehaviour[PlayBehaviour.ApplyOffsetWithoutNote])
						{
							if (m.Command == EffectCommand.Offset)
							{
								if (!porta || ((GetType_() & (ModType.Xm | ModType.Dbm)) == 0))
									ProcessSampleOffset(chn, nChn, playState);
							}
							else if (m.Command == EffectCommand.OffsetPercentage)
								SampleOffset(chn, Util.MulDiv_Unsigned(chn.nLength, m.Param, 256));
							else if ((m.Command == EffectCommand.ReverseOffset) && (chn.pModSample != null))
							{
								memory.RenderChannel(nChn, oldTickDuration);	// Re-sync what we've got so far
								ReverseSampleOffset(chn, m.Param);
								startTick = playState.m_nMusicSpeed - 1;
							}
							else if (m.VolCmd == VolumeCommand.Offset)
							{
								if ((chn.pModSample != null) && !chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib) && (m.Vol <= chn.pModSample.Cues.size()))
								{
									SmpLength offset;

									if (m.Vol == 0)
										offset = chn.OldOffset;
									else
										offset = chn.OldOffset = chn.pModSample.Cues[m.Vol - 1];

									SampleOffset(chn, offset);
								}
							}
						}

						if ((m.Note == ModCommand.Note_KeyOff) || (m.Note == ModCommand.Note_NoteCut) || ((m.Note == ModCommand.Note_Fade) && (GetNumInstruments() != 0)) ||
							(((m.Command == EffectCommand.ModCmdEx) || (m.Command == EffectCommand.S3MCmdEx)) && ((m.Param & 0xf0) == 0xc0) && (paramLo < numTicks)) ||
							((m.Command == EffectCommand.DelayCut) && (paramLo != 0) && ((startTick + paramLo) < numTicks)) ||
							 (m.Command == EffectCommand.KeyOff))
						{
							stopNote = true;
						}

						if (m.Command == EffectCommand.Volume)
							chn.nVolume = (int32)(m.Param * 4U);
						else if (m.Command == EffectCommand.Volume8)
							chn.nVolume = m.Param;
						else if (m.VolCmd == VolumeCommand.Volume)
							chn.nVolume = (int32)(m.Vol * 4U);

						if ((chn.pModSample != null) && !stopNote)
						{
							// Check if we don't want to emulate some effect and thus stop processing
							if (m.Command < EffectCommand.Max_Effects)
							{
								if (forbiddenCommands[m.Command])
									stopNote = true;
								else if (m.Command == EffectCommand.ModCmdEx)
								{
									// Special case: Slides using extended commands
									switch (m.Param & 0xf0)
									{
										case 0x10:
										case 0x20:
										{
											stopNote = true;
											break;
										}
									}
								}
							}
						}

						if (stopNote)
						{
							chn.Stop();
							memory.ChnSettings_[nChn].TicksToRender = 0;
						}
						else
						{
							if ((oldTickDuration != tickDuration) && (oldTickDuration != 0))
								memory.RenderChannel(nChn, oldTickDuration);	// Re-sync what we've got so far

							switch (m.Command)
							{
								case EffectCommand.TonePortaVol:
								case EffectCommand.VolumeSlide:
								case EffectCommand.VibratoVol:
								{
									if ((m.Param != 0) || (GetType_() != ModType.Mod))
									{
										// ST3 compatibility: Do not run combined slides (Kxy / Lxy) on first tick
										// Test cases: NoCombinedSlidesOnFirstTick-Normal.s3m, NoCombinedSlidesOnFirstTick-Fast.s3m
										for (uint32 i = (m_PlayBehaviour[PlayBehaviour.S3MIgnoreCombinedFineSlides] ? 1U : 0U); i < numTicks; i++)
										{
											chn.IsFirstTick = (i == 0);
											VolumeSlide(chn, m.Param);
										}
									}

									break;
								}

								case EffectCommand.ModCmdEx:
								{
									if (((m.Param & 0x0f) != 0) || ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0))
									{
										chn.IsFirstTick = true;

										switch (m.Param & 0xf0)
										{
											case 0xa0:
											{
												FineVolumeUp(chn, (ModCommandParam)(m.Param & 0x0f), false);
												break;
											}

											case 0xb0:
											{
												FineVolumeDown(chn, (ModCommandParam)(m.Param & 0x0f), false);
												break;
											}
										}
									}

									break;
								}

								case EffectCommand.S3MCmdEx:
								{
									if ((m.Param & 0xf0) == 0x90)
									{
										// Change play direction - other cases already handled above
										if ((m.Param == 0x9e) || (m.Param == 0x9f))
										{
											memory.RenderChannel(nChn, oldTickDuration);	// Re-sync what we've got so far
											ExtendedChannelEffect(chn, m.Param, playState);
										}
									}
									else if ((m.Param & 0xf0) == 0x70)
									{
										if (m.Param >= 0x73)
											chn.InstrumentControl(m.Param, this);
									}

									break;
								}

								case EffectCommand.DigiReverseSample:
								{
									DigiBoosterSampleReverse(chn, m.Param);
									break;
								}

								case EffectCommand.FineTune:
								case EffectCommand.FineTune_Smooth:
								{
									memory.RenderChannel(nChn, oldTickDuration);	// Re-sync what we've got so far
									chn.MicroTuning = CalculateFineTuneTarget(playState.m_nPattern, playState.m_nRow, nChn);	// TODO should render each tick individually for CMD_FINETUNE_SMOOTH for higher sync accuracy
									break;
								}

								// Auto portamentos
								case EffectCommand.Auto_PortaUp:
								{
									chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoUp, m.Param != 0);
									chn.nOldPortaUp = m.Param;
									break;
								}

								case EffectCommand.Auto_PortaDown:
								{
									chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoDown, m.Param != 0);
									chn.nOldPortaDown = m.Param;
									break;
								}

								case EffectCommand.Auto_PortaUp_Fine:
								{
									chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoUp, m.Param != 0);
									chn.nOldFinePortaUpDown = m.Param;
									break;
								}

								case EffectCommand.Auto_PortaDown_Fine:
								{
									chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoDown, m.Param != 0);
									chn.nOldFinePortaUpDown = m.Param;
									break;
								}

								case EffectCommand.Auto_Portamento_FC:
								{
									chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoFc, m.Param != 0);
									chn.nOldPortaUp = chn.nOldPortaDown = m.Param;
									break;
								}

								case EffectCommand.TonePorta_Duration:
								{
									if (chn.RowCommand.IsNote())
										TonePortamentoWithDuration(chn, m.Param);

									break;
								}
							}

							chn.IsFirstTick = true;

							switch (m.VolCmd)
							{
								case VolumeCommand.FineVolUp:
								{
									FineVolumeUp(chn, m.Vol, m_PlayBehaviour[PlayBehaviour.ItVolColMemory]);
									break;
								}

								case VolumeCommand.FineVolDown:
								{
									FineVolumeDown(chn, m.Vol, m_PlayBehaviour[PlayBehaviour.ItVolColMemory]);
									break;
								}

								case VolumeCommand.VolSlideUp:
								case VolumeCommand.VolSlideDown:
								{
									// IT Compatibility: Volume column volume slides have their own memory
									// Test case: VolColMemory.it
									ModCommandVol vol = m.Vol;

									if ((vol == 0) && m_PlayBehaviour[PlayBehaviour.ItVolColMemory])
									{
										vol = chn.nOldVolParam;

										if (vol == 0)
											break;
									}

									if (m.VolCmd == VolumeCommand.VolSlideUp)
										vol <<= 4;

									for (uint32 i = 0; i < numTicks; i++)
									{
										chn.IsFirstTick = (i == 0);

										// IT Compatibility: Volume column volume slides must not propagate their memory to the regular effect column
										// Test case: VolColNoSlideMemoryPropagation.it
										VolumeSlide(chn, vol, m_PlayBehaviour[PlayBehaviour.ItVolColNoSlidePropagation]);
									}

									break;
								}

								case VolumeCommand.PlayControl:
								{
									if ((m.Vol >= 2) && (m.Vol <= 4))
										memory.RenderChannel(nChn, oldTickDuration);	// Re-sync what we've got so far

									chn.PlayControl(m.Vol);
									break;
								}
							}

							if (chn.IsPaused)
								continue;

							if (m.IsAnyPitchSlide() || chn.AutoSlide.AnyPitchSlideActive())
							{
								// Portamento needs immediate syncing, as the pitch changes on each tick
								uint32 portaTick = memory.ChnSettings_[nChn].TicksToRender + startTick;
								memory.ChnSettings_[nChn].TicksToRender += numTicks;
								memory.RenderChannel(nChn, tickDuration, portaTick);
							}
							else
								memory.ChnSettings_[nChn].TicksToRender += (numTicks - startTick);
						}
					}
				}

				oldTickDuration = tickDuration;

				breakToRow = HandleNextRow(playState, orderList, false);
			}

			// Now advance the sample positions for sample seeking on channels that are still playing
			if (adjustSamplePos)
			{
				for (ChannelIndex nChn = 0; nChn < GetNumChannels(); nChn++)
				{
					if (memory.ChnSettings_[nChn].TicksToRender != GetLengthMemory.Ignore_Channel)
						memory.RenderChannel(nChn, oldTickDuration);
				}
			}

			if (retVal.TargetReached)
			{
				retVal.RestartOrder = playState.m_nCurrentOrder;
				retVal.RestartRow = playState.m_nRow;
			}

			retVal.Duration = memory.ElapsedTime;
			results.push_back(retVal);

			// Store final variables
			if ((adjustMode & EnmGetLengthResetMode.eAdjust) != 0)
			{
				if (retVal.TargetReached || (target.Mode == GetLengthTarget.Mode_.NoTarget))
				{
					PlayState.MidiMacroEvaluationResults midiMacroEvaluationResults = Utility.move(playState.m_MidiMacroEvaluationResults);
					playState.m_MidiMacroEvaluationResults = null;

					// Target found, or there is no target (i.e. play whole song)...
					m_PlayState = Utility.move(playState);
					m_PlayState.ResetGlobalVolumeRamping();
					m_PlayState.m_nNextRow = m_PlayState.m_nRow;
					m_PlayState.m_nFrameDelay = m_PlayState.m_nPatternDelay = 0;
					m_PlayState.m_nTickCount = Ticks_Row_Finished;
					m_PlayState.m_Flags.Set(PlayFlags.Song_PositionChanged);

//XX					if (m_Opl != null)
//XX						m_Opl.Reset();

					for (ChannelIndex n = 0; n < GetNumChannels(); n++)
					{
						ModChannel chn = m_PlayState.Chn[n];

						if ((memory.ChnSettings_[n].Vol != 0xff) && !adjustSamplePos)
							chn.nVolume = Math.Min(memory.ChnSettings_[n].Vol, (uint8)64) * 4;

/*						if (chn.dwFlags.Test(ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute) && (chn.pModSample != null) && chn.pModSample.uFlags.Test(SampleFlags.Chn_Adlib) && (m_Opl != 0))
						{
							m_Opl.Patch(n, chn.pModSample.Adlib);
							m_Opl.NoteCut(n);
						}*///XX

						chn.pCurrentSample = null;
					}
				}
				else if (adjustMode != EnmGetLengthResetMode.eAdjustOnSuccess)
				{
					// Target not found (e.g. when jumping to a hidden sub song), reset global variables...
					m_PlayState.m_nMusicSpeed = Order[sequence].GetDefaultSpeed();
					m_PlayState.m_nMusicTempo = Order[sequence].GetDefaultTempo();
					m_PlayState.m_nGlobalVolume = (int32)m_nDefaultGlobalVolume;
				}

				// When adjusting the playback status, we will also want to update the visited rows vector according to the current position
				if (sequence != Order.GetCurrentSequenceIndex())
					Order.SetSequence(sequence);
			}

			if ((adjustMode & (EnmGetLengthResetMode.eAdjust | EnmGetLengthResetMode.eAdjustOnlyVisitedRows)) != 0)
				m_VisitedRows.MoveVisitedRowsFrom(visitedRows);

			return results;
		}
		#endregion

		//XX 1458
		#region Effects
		/********************************************************************/
		/// <summary>
		/// Change sample or instrument number
		/// </summary>
		/********************************************************************/
		public void InstrumentChange(ModChannel chn, uint32 instr, bool bPorta = false, bool bUpdVol = true, bool bResetEnv = true)//XX 1462
		{
			ModInstrument pIns = instr <= GetNumInstruments() ? Instruments[instr] : null;
			ModSample pSmp = Samples[instr <= GetNumSamples() ? instr : 0];
			uint8 oldInsVol = chn.nInsVol;
			ModCommandNote note = chn.nNewNote;

			if ((note == ModCommand.Note_None) && m_PlayBehaviour[PlayBehaviour.ItInstrWithoutNote])
				return;

			if ((pIns != null) && ModCommand.IsNote(note))
			{
				// Impulse Tracker ignores empty slots.
				// We won't ignore them if a plugin is assigned to this slot, so that VSTis still work as intended.
				// Test case: emptyslot.it, PortaInsNum.it, gxsmp.it, gxsmp2.it
				if ((pIns.Keyboard[note - ModCommand.Note_Min] == 0) && m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlot] && !pIns.HasValidMidiChannel())
				{
					chn.pModInstrument = pIns;
					return;
				}

				if (pIns.NoteMap[note - ModCommand.Note_Min] > ModCommand.Note_Max)
					return;

				uint32 n = pIns.Keyboard[note - ModCommand.Note_Min];

				if (n != 0)
					pSmp = n <= GetNumSamples() ? Samples[n] : Samples[0];
				else
					pSmp = null;
			}
			else if (GetNumInstruments() != 0)
			{
				// No valid instrument, or not a valid note
				if (note >= ModCommand.Note_Min_Special)
					return;

				if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlot] && ((pIns == null) || !pIns.HasValidMidiChannel()))
				{
					// Impulse Tracker ignores empty slots.
					// We won't ignore them if a plugin is assigned to this slot, so that VSTis still work as intended.
					// Test case: emptyslot.it, PortaInsNum.it, gxsmp.it, gxsmp2.it
					chn.pModInstrument = null;
					chn.SwapSampleIndex = chn.nNewIns = 0;
					return;
				}

				pSmp = null;
			}

			bool returnAfterVolumeAdjust = false;

			// instrumentChanged is used for IT carry-on env option
			bool instrumentChanged = pIns != chn.pModInstrument;
			bool sampleChanged = (chn.pModSample != null) && (pSmp != chn.pModSample);
			bool newTuning = (GetType_() == ModType.Mpt) && (pIns != null) && (pIns.pTuning != null);

			if (!bPorta || instrumentChanged || sampleChanged)
				chn.MicroTuning = 0;

			// Playback behavior change for MPT: With portamento don't change sample if it is in
			// the same instrument as previous sample
			if (bPorta && newTuning && (pIns == chn.pModInstrument) && sampleChanged)
				return;

			if (sampleChanged && bPorta)
			{
				// IT compatibility: No sample change (also within multi-sample instruments) during portamento when using Compatible Gxx.
				// Test case: PortaInsNumCompat.it, PortaSampleCompat.it, PortaCutCompat.it
				if (m_PlayBehaviour[PlayBehaviour.ItPortamentoInstrument] && m_SongFlags.Test(SongFlags.ItCompatGxx) && !chn.Increment.IsZero())
					pSmp = chn.pModSample;

				// Special XM hack (also applies to MOD / S3M, except when playing IT-style S3Ms, such as k_vision.s3m)
				// Test case: PortaSmpChange.mod, PortaSmpChange.s3m, PortaSwap.s3m
				if ((!instrumentChanged && ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0) && (pIns != null)) ||
				    (GetType_() == ModType.Plm) || ((GetType_() == ModType.Mod) && chn.IsSamplePlaying()) || (m_PlayBehaviour[PlayBehaviour.St3PortaSampleChange] && chn.IsSamplePlaying()))
				{
					// FT2 doesn't change the sample in this case,
					// but still uses the sample info from the old one (bug?)
					returnAfterVolumeAdjust = true;
				}

				// IT compatibility: Reset filter if portamento results in sample change
				// Test case: FilterPortaSmpChange.it, FilterPortaSmpChange-InsMode.it
				if (m_PlayBehaviour[PlayBehaviour.ItResetFilterOnPortaSmpChange] && (m_nInstruments == 0))
					chn.TriggerNote = true;
			}

			// IT compatibility: A lone instrument number should only reset sample properties to those of the corresponding sample in instrument mode.
			// C#5 01 ... <-- sample 1
			// C-5 .. g02 <-- sample 2
			// ... 01 ... <-- still sample 1, but with properties of sample 2
			// In the above example, no sample change happens on the second row. In the third row, sample 1 keeps playing but with the
			// volume and panning properties of sample 2.
			// Test case: InstrAfterMultisamplePorta.it
			if ((m_nInstruments != 0) && !instrumentChanged && sampleChanged && (chn.pCurrentSample != null) && m_PlayBehaviour[PlayBehaviour.ItMultiSampleInstrumentNumber] && !chn.RowCommand.IsNote())
				returnAfterVolumeAdjust = true;

			// IT Compatibility: Envelope pickup after SCx cut (but don't do this when working with plugins, or else envelope carry stops working)
			// Test case: cut-carry.it
			if (!chn.IsSamplePlaying() && ((GetType_() & (ModType.It | ModType.Mpt)) != 0) && ((pIns == null) || !pIns.HasValidMidiChannel()))
				instrumentChanged = true;

			// FT2 compatibility: new instrument + portamento = ignore new instrument number, but reload old instrument settings (the world of XM is upside down...)
			// And this does *not* happen if volume column portamento is used together with note delay... (handled in ProcessEffects(), where all the other note delay stuff is.)
			// Test case: porta-delay.xm, SamplePortaInInstrument.xm
			if ((instrumentChanged || sampleChanged) && bPorta && m_PlayBehaviour[PlayBehaviour.Ft2PortaIgnoreInstr] && ((chn.pModInstrument != null) || (chn.pModSample != null)))
			{
				pIns = chn.pModInstrument;
				pSmp = chn.pModSample;
				instrumentChanged = false;
			}
			else
				chn.pModInstrument = pIns;

			// Update volume
			if (bUpdVol && (((GetType_() & (ModType.Mod | ModType.S3M)) == 0) || (((pSmp != null) && pSmp.HasSampleData()) || chn.HasMidiOutput())))
			{
				if (pSmp != null)
				{
					if (!pSmp.uFlags.Test(ChannelFlags.Smp_NoDefaultVolume))
						chn.nVolume = pSmp.nVolume;
				}
				else if ((pIns != null) && (pIns.nMixPlug != 0))
					chn.nVolume = (int32)chn.GetVstVolume();
				else
					chn.nVolume = 0;
			}

			if (returnAfterVolumeAdjust && sampleChanged && (pSmp != null))
			{
				// ProTracker applies new instrument's finetune but keeps the old sample playing.
				// Test case: PortaSwapPT.mod
				if (m_PlayBehaviour[PlayBehaviour.ModSampleSwap])
					chn.nFineTune = pSmp.nFineTune;

				// ST3 does it similarly for middle-C speed.
				// Test case: PortaSwap.s3m, SampleSwap.s3m
				if ((GetType_() == ModType.S3M) && pSmp.HasSampleData())
					chn.nC5Speed = (int32)pSmp.nC5Speed;
			}

			if (returnAfterVolumeAdjust)
				return;

			// Instrument adjust
			chn.SwapSampleIndex = chn.nNewIns = 0;

			// IT Compatiblity: NNA is reset on every note change, not every instrument change (fixes s7xinsnum.it)
			if ((pIns != null) && ((!m_PlayBehaviour[PlayBehaviour.ItNnaReset] && (pSmp != null)) || (pIns.nMixPlug != 0) || instrumentChanged))
				chn.nNna = pIns.nNna;

			// Update volume
			chn.UpdateInstrumentVolume(pSmp, pIns);

			// Update panning
			// FT2 compatibility: Only reset panning on instrument numbers, not notes (bUpdVol condition)
			// Test case: PanMemory.xm
			// IT compatibility: Sample and instrument panning is only applied on note change, not instrument change
			// Test case: PanReset.it
			if ((bUpdVol || ((GetType_() & (ModType.Xm | ModType.Mt2)) == 0)) && !m_PlayBehaviour[PlayBehaviour.ItPanningReset])
				ApplyInstrumentPanning(chn, pIns, pSmp);

			// Reset envelopes
			if (bResetEnv)
			{
				// Blurb by Storlek (from the SchismTracker code):
				// Conditions experimentally determined to cause envelope reset in Impulse Tracker:
				// - no note currently playing (of course)
				// - note given, no portamento
				// - instrument number given, portamento, compat gxx enabled
				// - instrument number given, no portamento, after keyoff, old effects enabled
				// If someone can enlighten me to what the logic really is here, I'd appreciate it.
				// Seems like it's just a total mess though, probably to get XMs to play right
				bool reset, resetAlways;

				// IT Compatibility: Envelope reset
				// Test case: EnvReset.it
				if (m_PlayBehaviour[PlayBehaviour.ItEnvelopeReset])
				{
					bool insNumber = instr != 0;
					reset = (chn.nLength == 0) ||
					        (insNumber && bPorta && m_SongFlags.Test(SongFlags.ItCompatGxx)) ||
							(insNumber && !bPorta && chn.dwFlags.Test(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_KeyOff) && m_SongFlags.Test(SongFlags.ItOldEffects));

					// NOTE: Carry behaviour is not consistent between IT drivers.
					// If NNA is set to "Note Cut", carry only works if the driver uses volume ramping on cut notes.
					// This means that the normal SB and GUS drivers behave differently than what is implemented here.
					// We emulate  IT's WAV writer and SB16 MMX driver instead.
					// Test case: CarryNNA.it
					resetAlways = (chn.nFadeOutVol == 0) || instrumentChanged  || (m_PlayBehaviour[PlayBehaviour.ItCarryAfterNoteOff] ? !chn.RowCommand.IsNote() : chn.dwFlags.Test(ChannelFlags.Chn_KeyOff));
				}
				else
				{
					reset = !bPorta || ((GetType_() & (ModType.It | ModType.Mpt | ModType.Dbm)) == 0) || m_SongFlags.Test(SongFlags.ItCompatGxx) ||
							(chn.nLength == 0) || (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && (chn.nFadeOutVol == 0));
					resetAlways = ((GetType_() & (ModType.It | ModType.Mpt | ModType.Dbm)) == 0) || instrumentChanged || (pIns == null) || chn.dwFlags.Test(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade);
				}

				if (reset)
				{
					chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);

					if (pIns != null)
					{
						if (resetAlways)
							chn.ResetEnvelopes();
						else
						{
							if (!pIns.VolEnv.dwFlags.Test(EnvelopeFlags.Carry))
								chn.VolEnv.Reset();

							if (!pIns.PanEnv.dwFlags.Test(EnvelopeFlags.Carry))
								chn.PanEnv.Reset();

							if (!pIns.PitchEnv.dwFlags.Test(EnvelopeFlags.Carry))
								chn.PitchEnv.Reset();
						}
					}

					// IT Compatibility: Autovibrato reset
					if (!m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					{
						chn.nAutoVibDepth = 0;
						chn.nAutoVibPos = 0;
					}
				}
				else if ((pIns != null) && !pIns.VolEnv.dwFlags.Test(EnvelopeFlags.Enabled))
				{
					if (m_PlayBehaviour[PlayBehaviour.ItPortamentoInstrument])
						chn.VolEnv.Reset();
					else
						chn.ResetEnvelopes();
				}
			}

			// Invalid sample?
			if ((pSmp == null) && ((pIns == null) || !pIns.HasValidMidiChannel()))
			{
				chn.pModSample = null;
				chn.nInsVol = 0;
				return;
			}

			bool wasKeyOff = chn.dwFlags.Test(ChannelFlags.Chn_KeyOff);

			// Tone-Portamento doesn't reset the pingpong direction flag
			if (bPorta && (pSmp == chn.pModSample) && (pSmp != null))
			{
				// IT compatibility: Instrument change but sample stays the same: still reset the key-off flags
				// Test case: SampleSustainAfterPortaInstrMode.it
				if (instrumentChanged && (pIns != null) && m_PlayBehaviour[PlayBehaviour.ItNoSustainOnPortamento])
					chn.dwFlags.Reset(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade);

				// If channel length is 0, we cut a previous sample using SCx. In that case, we have to update sample length, loop points, etc...
				if (((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0) && (chn.nLength != 0))
					return;

				// FT2 compatibility: Do not reset key-off status on portamento without instrument number.
				// Test case: Off-Porta.xm
				if ((GetType_() != ModType.Xm) || !m_PlayBehaviour[PlayBehaviour.ItFt2DontResetNoteOffOnPorta] || (chn.RowCommand.Instr != 0))
					chn.dwFlags.Reset(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade);

				chn.dwFlags = chn.dwFlags & (Snd_Def.Chn_ChannelFlags | ChannelFlags.Chn_PingPongFlag);
			}
			else
			{
				chn.dwFlags.Reset(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade);

				// IT compatibility: Don't change bidi loop direction when no sample nor instrument is changed
				if ((m_PlayBehaviour[PlayBehaviour.ItPingPongNoReset] || ((GetType_() & (ModType.It | ModType.Mpt)) == 0)) && (pSmp == chn.pModSample) && !instrumentChanged)
					chn.dwFlags = chn.dwFlags & (Snd_Def.Chn_ChannelFlags | ChannelFlags.Chn_PingPongFlag);
				else
					chn.dwFlags = chn.dwFlags & Snd_Def.Chn_ChannelFlags;

				if (pIns != null)
				{
					// Copy envelope flags (we actually only need the "enabled" and "pitch" flag)
					chn.VolEnv.Flags = pIns.VolEnv.dwFlags;
					chn.PanEnv.Flags = pIns.PanEnv.dwFlags;
					chn.PitchEnv.Flags = pIns.PitchEnv.dwFlags;

					// A cutoff frequency of 0 should not be reset just because the filter envelope is enabled.
					// Test case: FilterEnvReset.it
					if (((pIns.PitchEnv.dwFlags & (EnvelopeFlags.Enabled | EnvelopeFlags.Filter)) == (EnvelopeFlags.Enabled | EnvelopeFlags.Filter)) && !m_PlayBehaviour[PlayBehaviour.ItFilterBehaviour])
					{
						if (chn.nCutOff == 0)
							chn.nCutOff = 0x7f;
					}

					if (pIns.IsCutOffEnabled())
						chn.nCutOff = pIns.GetCutOff();

					if (pIns.IsResonanceEnabled())
						chn.nResonance = pIns.GetResonance();
				}
			}

			if (pSmp == null)
			{
				chn.pModSample = null;
				chn.nLength = 0;
				return;
			}

			if (bPorta && (chn.nLength == 0) && (m_PlayBehaviour[PlayBehaviour.Ft2PortaNoNote] || m_PlayBehaviour[PlayBehaviour.ItPortaNoNote]))
			{
				// IT/FT2 compatibility: If the note just stopped on the previous tick, prevent it from restarting.
				// Test cases: PortaJustStoppedNote.xm, PortaJustStoppedNote.it
				chn.Increment.Set(0);
			}

			// IT compatibility: Note-off with instrument number + Old Effects retriggers envelopes.
			// If the instrument changes, keep playing the previous sample, but load the new instrument's envelopes.
			// Test case: ResetEnvNoteOffOldFx.it
			if ((chn.RowCommand.Note == ModCommand.Note_KeyOff) && m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOffOldEffects] && m_SongFlags.Test(SongFlags.ItOldEffects) && sampleChanged)
			{
				if (chn.pModSample != null)
					chn.dwFlags |= (chn.pModSample.uFlags & Snd_Def.Chn_SampleFlags);

				chn.nInsVol = oldInsVol;
				chn.nVolume = pSmp.nVolume;

				if (pSmp.uFlags.Test(ChannelFlags.Chn_Panning))
					chn.SetInstrumentPan(pSmp.nPan, this);

				return;
			}

			chn.pModSample = pSmp;
			chn.nLength = pSmp.nLength;
			chn.nLoopStart = pSmp.nLoopStart;
			chn.nLoopEnd = pSmp.nLoopEnd;

			// ProTracker "oneshot" loops (if loop start is 0, play the whole sample once and then repeat until loop end)
			if (m_PlayBehaviour[PlayBehaviour.ModOneShotLoops] && (chn.nLoopStart == 0))
				chn.nLoopEnd = pSmp.nLength;

			chn.dwFlags |= (pSmp.uFlags & Snd_Def.Chn_SampleFlags);

			// IT Compatibility: Autovibrato reset
			if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
			{
				chn.nAutoVibDepth = 0;
				chn.nAutoVibPos = 0;
			}

			if (newTuning)
			{
				chn.nC5Speed = (int32)pSmp.nC5Speed;
				chn.m_CalculateFreq = true;
				chn.nFineTune = 0;
			}
			else if (!bPorta || sampleChanged || ((GetType_() & (ModType.Mod | ModType.Xm)) == 0))
			{
				// Don't reset finetune changed by "set finetune" command.
				// Test case: finetune.xm, finetune.mod
				// But *do* change the finetune if we switch to a different sample, to fix
				// Miranda`s axe by Jamson (jam007.xm)
				chn.nC5Speed = (int32)pSmp.nC5Speed;
				chn.nFineTune = pSmp.nFineTune;
			}

			chn.nTranspose = UseFineTuneAndTranspose() ? pSmp.RelativeTone : (int8)0;

			// FT2 compatibility: Don't reset portamento target with new instrument numbers.
			// Test case: Porta-Pickup.xm
			// ProTracker does the same.
			// Test case: PortaTarget.mod
			if (!m_PlayBehaviour[PlayBehaviour.Ft2PortaTargetNoReset] && (GetType_() != ModType.Mod))
				chn.nPortamentoDest = 0;

			chn.m_PortamentoFineSteps = 0;

			// IT compatibility: Do not reset sustain loop status when using portamento after key-off
			// Test case: SampleSustainAfterPorta.it, SampleSustainAfterPortaCompatGxx.it, SampleSustainAfterPortaInstrMode.it
			if (chn.dwFlags.Test(ChannelFlags.Chn_SustainLoop) && (!m_PlayBehaviour[PlayBehaviour.ItNoSustainOnPortamento] || !bPorta || ((pIns != null) && !wasKeyOff)))
			{
				chn.nLoopStart = pSmp.nSustainStart;
				chn.nLoopEnd = pSmp.nSustainEnd;

				if (chn.dwFlags.Test(ChannelFlags.Chn_PingPongSustain))
					chn.dwFlags.Set(ChannelFlags.Chn_PingPongLoop);

				chn.dwFlags.Set(ChannelFlags.Chn_Loop);
			}

			if (chn.dwFlags.Test(ChannelFlags.Chn_Loop) && (chn.nLoopEnd < chn.nLength))
				chn.nLength = chn.nLoopEnd;

			// Fix sample position on instrument change. This is needed for IT "on the fly" sample change.
			// XXX is this actually called? In ProcessEffects(), a note-on effect is emulated if there's an on the fly sample change!
			if (chn.Position.GetUInt() >= chn.nLength)
			{
				if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
					chn.Position.Set(0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void NoteChange(ModChannel chn, c_int note, bool bPorta = false, bool bResetEnv = true, bool bManual = false, ChannelIndex channelHint = Snd_Def.ChannelIndex_Invalid)//XX 1842
		{
			if (note < ModCommand.Note_Min)
				return;

			c_int origNote = note;
			ModSample pSmp = chn.pModSample;
			ModInstrument pIns = chn.pModInstrument;

			bool newTuning = (GetType_() == ModType.Mpt) && (pIns != null) && (pIns.pTuning != null);

			// Save the note that's actually used, as it's necessary to properly calculate PPS and stuff
			c_int realNote = note;

			if ((pIns != null) && ((note - ModCommand.Note_Min) < (c_int)pIns.Keyboard.size()))
			{
				uint32 n = pIns.Keyboard[note - ModCommand.Note_Min];

				if (n > 0)
					pSmp = Samples[(n <= GetNumSamples()) ? n : 0];
				else if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlot] && !chn.HasMidiOutput())
				{
					// Impulse Tracker ignores empty slots.
					// We won't ignore them if a plugin is assigned to this slot, so that VSTis still work as intended.
					// Test case: emptyslot.it, PortaInsNum.it, gxsmp.it, gxsmp2.it
					return;
				}

				note = pIns.NoteMap[note - ModCommand.Note_Min];
			}

			// Key Off
			if (note > ModCommand.Note_Max)
			{
				// Key Off (+ Invalid Note for XM - TODO is this correct?)
				if ((note == ModCommand.Note_KeyOff) || ((GetType_() & (ModType.It | ModType.Mpt)) == 0))
				{
					KeyOff(chn);

					// IT compatibility: Note-off + instrument releases sample sustain but does not release envelopes or fade the instrument
					// Test case: noteoff3.it, ResetEnvNoteOffOldFx2.it
					if (!bPorta && m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOffOldEffects] && m_SongFlags.Test(SongFlags.ItOldEffects) && (chn.RowCommand.Instr != 0))
						chn.dwFlags.Reset(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_KeyOff);
				}
				else	// Invalid note -> Note Fade
				{
					if (GetNumInstruments() != 0)
						chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
				}

				// Note Cut
				if (note == ModCommand.Note_NoteCut)
				{
					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (GetType_() == ModType.S3M))
					{
						// OPL voices are not cut but enter the release portion of their envelope
						// In S3M we can still modify the volume after note-off, in legacy MPTM mode we can't
						chn.dwFlags.Set(ChannelFlags.Chn_KeyOff);
					}
					else
					{
						chn.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);

						// IT compatibility: Stopping sample playback by setting sample increment to 0 rather than volume
						// Test case: NoteOffInstr.it
						if (((GetType_() & (ModType.It | ModType.Mpt)) == 0) || ((m_nInstruments != 0) && !m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOff]))
							chn.nVolume = 0;

						if (m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOff])
							chn.Increment.Set(0);

						chn.nFadeOutVol = 0;
					}
				}

				// IT compatibility tentative fix: Clear channel note memory (TRANCE_N.IT by A3F)
				if (m_PlayBehaviour[PlayBehaviour.ItClearOldNoteAfterCut])
					chn.nNote = chn.nNewNote = ModCommand.Note_None;

				return;
			}

			if (newTuning)
			{
				if (!bPorta || (chn.nNote == ModCommand.Note_None))
					chn.nPortamentoDest = 0;
				else
				{
					chn.nPortamentoDest = pIns.pTuning.GetStepDistance(chn.nNote, chn.m_PortamentoFineSteps, (TuningNoteIndexType)note, 0);

					// Here chn.nPortamentoDest means 'steps to slide'
					chn.m_PortamentoFineSteps = -chn.nPortamentoDest;
				}
			}

			if (!bPorta && ((GetType_() & (ModType.Xm | ModType.Med | ModType.Mt2)) != 0))
			{
				if (pSmp != null)
				{
					chn.nTranspose = pSmp.RelativeTone;
					chn.nFineTune = pSmp.nFineTune;
				}
			}

			// IT Compatibility: Update multisample instruments frequency even if instrument is not specified (fixes the guitars in spx-shuttledeparture.it)
			// Test case: freqreset-noins.it
			if (!bPorta && (pSmp != null) && m_PlayBehaviour[PlayBehaviour.ItMultiSampleBehaviour])
				chn.nC5Speed = (int32)pSmp.nC5Speed;

			if (bPorta && !chn.IsSamplePlaying())
			{
				if (m_PlayBehaviour[PlayBehaviour.Ft2PortaNoNote] && (!chn.HasMidiOutput() || m_PlayBehaviour[PlayBehaviour.PluginIgnoreTonePortamento]))
				{
					// FT2 Compatibility: Ignore notes with portamento if there was no note playing.
					// Test case: 3xx-no-old-samp.xm
					chn.nPeriod = 0;
					return;
				}
				else if (m_PlayBehaviour[PlayBehaviour.ItPortaNoNote])
				{
					// IT Compatibility: Ignore portamento command if no note was playing (e.g. if a previous note has faded out).
					// Test case: Fade-Porta.it
					bPorta = false;
				}
			}

			if (UseFineTuneAndTranspose())
			{
				note += chn.nTranspose;

				// RealNote = PatternNote + RelativeTone; (0..118, 0 = C-0, 118 = A#9)
				OpenMpt.Limit(ref note, ModCommand.Note_Min + 11, ModCommand.Note_Min + 130);	// 119 possible notes
			}
			else
				OpenMpt.Limit(ref note, (c_int)ModCommand.Note_Min, ModCommand.Note_Max);

			if (m_PlayBehaviour[PlayBehaviour.ItRealNoteMapping])
			{
				// Need to memorize the original note for various effects (e.g. PPS)
				chn.nNote = (ModCommandNote)OpenMpt.Clamp(realNote, (c_int)ModCommand.Note_Min, ModCommand.Note_Max);
			}
			else
				chn.nNote = (ModCommandNote)note;

			chn.m_CalculateFreq = true;
			chn.IsPaused = false;

			if (!bPorta || ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0))
				chn.SwapSampleIndex = chn.nNewIns = 0;

			uint32 period = GetPeriodFromNote((uint32)note, chn.nFineTune, (uint32)chn.nC5Speed);
			chn.nPanbrelloOffset = 0;

			// IT compatibility: Sample and instrument panning is only applied on note change, not instrument change
			// Test case: PanReset.it
			if (m_PlayBehaviour[PlayBehaviour.ItPanningReset])
				ApplyInstrumentPanning(chn, pIns, pSmp);

			// IT compatibility: Pitch/Pan Separation can be overriden by panning commands, and shouldn't be affected by note-off commands
			// Test case: PitchPanReset.it
			if (m_PlayBehaviour[PlayBehaviour.ItPitchPanSeparation] && (pIns != null) && (pIns.nPps != 0))
			{
				if (chn.nRestorePanOnNewNote == 0)
					chn.nRestorePanOnNewNote = (uint16)(chn.nPan + 1);

				ProcessPitchPanSeparation(ref chn.nPan, origNote, pIns);
			}

			if (bResetEnv && !bPorta)
			{
				chn.nVolSwing = chn.nPanSwing = 0;
				chn.nResSwing = chn.nCutSwing = 0;

				if (pIns != null)
				{
					// IT Compatiblity: NNA is reset on every note change, not every instrument change (fixes spx-farspacedance.it)
					if (m_PlayBehaviour[PlayBehaviour.ItNnaReset])
						chn.nNna = pIns.nNna;

					if (!pIns.VolEnv.dwFlags.Test(EnvelopeFlags.Carry))
						chn.VolEnv.Reset();

					if (!pIns.PanEnv.dwFlags.Test(EnvelopeFlags.Carry))
						chn.PanEnv.Reset();

					if (!pIns.PitchEnv.dwFlags.Test(EnvelopeFlags.Carry))
						chn.PitchEnv.Reset();

					// Volume Swing
					if (pIns.nVolSwing != 0)
						chn.nVolSwing = (int16)((((Mpt.Random.Random.Random_<int8, uint16>(AccessPrng()) * pIns.nVolSwing) / 64) + 1) * (m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour] ? chn.nInsVol : ((chn.nVolume + 1) / 2)) / 199);

					// Pan Swing
					if (pIns.nPanSwing != 0)
					{
						chn.nPanSwing = (int16)((Mpt.Random.Random.Random_<int8, uint16>(AccessPrng()) * pIns.nPanSwing * 4) / 128);

						if (!m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour] && (chn.nRestorePanOnNewNote == 0))
							chn.nRestorePanOnNewNote = (uint16)(chn.nPan + 1);
					}

					// CutOff Swing
					if (pIns.nCutSwing != 0)
					{
						int32 d = (pIns.nCutSwing * (Mpt.Random.Random.Random_<int8, uint16>(AccessPrng()) + 1)) / 128;
						chn.nCutSwing = (int16)((d * chn.nCutOff + 1) / 128);
						chn.nRestoreCutOffOnNewNote = (uint8)(chn.nCutOff + 1);
					}

					// Resonance Swing
					if (pIns.nResSwing != 0)
					{
						int32 d = (pIns.nResSwing * (Mpt.Random.Random.Random_<int8, uint16>(AccessPrng()) + 1)) / 128;
						chn.nResSwing = (int16)((d * chn.nResonance + 1) / 128);
						chn.nRestoreResonanceOnNewNote = (uint8)(chn.nResonance + 1);
					}
				}
			}

			if (pSmp == null)
				return;

			if (period != 0)
			{
				if (!bPorta || (chn.nPeriod == 0))
					chn.nPeriod = (int32)period;

				if (!newTuning)
				{
					// FT2 compatibility: Don't reset portamento target with new notes.
					// Test case: Porta-Pickup.xm
					// ProTracker does the same.
					// Test case: PortaTarget.mod
					// IT compatibility: Portamento target is completely cleared with new notes.
					// Test case: PortaReset.it
					if (bPorta || !(m_PlayBehaviour[PlayBehaviour.Ft2PortaTargetNoReset] || m_PlayBehaviour[PlayBehaviour.ItClearPortaTarget] || (GetType_() == ModType.Mod)))
					{
						chn.nPortamentoDest = (int32)period;
						chn.PortaTargetReached = false;
					}
				}

				if (!bPorta || ((chn.nLength == 0) && ((GetType_() & ModType.S3M) == 0)))
				{
					chn.pModSample = pSmp;
					chn.nLength = pSmp.nLength;
					chn.nLoopEnd = pSmp.nLength;
					chn.nLoopStart = 0;
					chn.Position.Set(0);

					if ((m_SongFlags.Test(SongFlags.Pt_Mode) || m_PlayBehaviour[PlayBehaviour.St3OffsetWithoutInstrument] || (GetType_() == ModType.Med)) && (chn.RowCommand.Instr == 0))
						chn.Position.SetInt((int32)Math.Min(chn.PrevNoteOffset, chn.nLength - (SmpLength)1));
					else
						chn.PrevNoteOffset = 0;

					chn.dwFlags = (chn.dwFlags & Snd_Def.Chn_ChannelFlags) | (pSmp.uFlags & Snd_Def.Chn_SampleFlags);
					chn.dwFlags.Reset(ChannelFlags.Chn_Portamento);

					if (chn.dwFlags.Test(ChannelFlags.Chn_SustainLoop))
					{
						chn.nLoopStart = pSmp.nSustainStart;
						chn.nLoopEnd = pSmp.nSustainEnd;
						chn.dwFlags.Set(ChannelFlags.Chn_PingPongLoop, chn.dwFlags.Test(ChannelFlags.Chn_PingPongSustain));
						chn.dwFlags.Set(ChannelFlags.Chn_Loop);

						if (chn.nLength > chn.nLoopEnd)
							chn.nLength = chn.nLoopEnd;
					}
					else if (chn.dwFlags.Test(ChannelFlags.Chn_Loop))
					{
						chn.nLoopStart = pSmp.nLoopStart;
						chn.nLoopEnd = pSmp.nLoopEnd;

						if (chn.nLength > chn.nLoopEnd)
							chn.nLength = chn.nLoopEnd;
					}

					// ProTracker "oneshot" loops (if loop start is 0, play the whole sample once and then repeat until loop end)
					if (m_PlayBehaviour[PlayBehaviour.ModOneShotLoops] && (chn.nLoopStart == 0))
						chn.nLoopEnd = chn.nLength = pSmp.nLength;

					if (chn.dwFlags.Test(ChannelFlags.Chn_Reverse) && (chn.nLength > 0))
					{
						chn.dwFlags.Set(ChannelFlags.Chn_PingPongFlag);
						chn.Position.SetInt((int32)(chn.nLength - 1));
					}

					// Handle "retrigger" waveform type
					if (chn.nVibratoType < 4)
					{
						// IT Compatibilty: Slightly different waveform offsets (why does MPT have two different offsets here with IT old effects enabled and disabled?)
						if (!m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello] && ((GetType_() & (ModType.It | ModType.Mpt)) != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects))
							chn.nVibratoPos = 0x10;
						else if (GetType_() == ModType.Mtm)
							chn.nVibratoPos = 0x20;
						else if ((GetType_() & (ModType.Digi | ModType.Dbm)) == 0)
							chn.nVibratoPos = 0;
					}

					// IT Compatibility: No "retrigger" waveform here
					if (!m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello] && (chn.nTremoloType < 4))
						chn.nTremoloPos = 0;
				}

				if (chn.Position.GetUInt() >= chn.nLength)
					chn.Position.SetInt((int32)chn.nLoopStart);
			}
			else
				bPorta = false;

			if (!bPorta || ((GetType_() & (ModType.It | ModType.Mpt | ModType.Dbm)) == 0) || (chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && (chn.nFadeOutVol == 0)) || (m_SongFlags.Test(SongFlags.ItCompatGxx) && (chn.RowCommand.Instr != 0)))
			{
				if (((GetType_() & (ModType.It | ModType.Mpt | ModType.Dbm)) != 0) && chn.dwFlags.Test(ChannelFlags.Chn_NoteFade) && (chn.nFadeOutVol == 0))
				{
					chn.ResetEnvelopes();

					// IT Compatibility: Autovibrato reset
					if (!m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					{
						chn.nAutoVibDepth = 0;
						chn.nAutoVibPos = 0;
					}

					chn.dwFlags.Reset(ChannelFlags.Chn_NoteFade);
					chn.nFadeOutVol = 65536;
				}

				if (!bPorta || !m_SongFlags.Test(SongFlags.ItCompatGxx) || (chn.RowCommand.Instr != 0))
				{
					if (((GetType_() & (ModType.Xm | ModType.Mt2)) == 0) || (chn.RowCommand.Instr != 0))
					{
						chn.dwFlags.Reset(ChannelFlags.Chn_NoteFade);
						chn.nFadeOutVol = 65536;
					}
				}
			}

			// IT compatibility: Don't reset key-off flag on porta notes unless Compat Gxx is enabled.
			// Test case: Off-Porta.it, Off-Porta-CompatGxx.it, Off-Porta.xm
			if (m_PlayBehaviour[PlayBehaviour.ItFt2DontResetNoteOffOnPorta] && bPorta && (!m_SongFlags.Test(SongFlags.ItCompatGxx) || (chn.RowCommand.Instr == 0)))
				chn.dwFlags.Reset(ChannelFlags.Chn_ExtraLoud);
			else
				chn.dwFlags.Reset(ChannelFlags.Chn_ExtraLoud | ChannelFlags.Chn_KeyOff);

			// Enable Ramping
			if (!bPorta)
			{
				chn.TriggerNote = true;
				chn.dwFlags.Reset(ChannelFlags.Chn_Filter);
				chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);

				// IT compatibility 15. Retrigger is reset in RetrigNote (Tremor doesn't store anything here, so we just don't reset this as well)
				if (!m_PlayBehaviour[PlayBehaviour.ItRetrigger] && !m_PlayBehaviour[PlayBehaviour.ItTremor])
				{
					// FT2 compatibility: Retrigger is reset in RetrigNote, tremor in ProcessEffects
					if (!m_PlayBehaviour[PlayBehaviour.Ft2Retrigger] && !m_PlayBehaviour[PlayBehaviour.Ft2Tremor])
					{
						chn.nRetrigCount = 0;
						chn.nTremorCount = 0;
					}
				}

				if (bResetEnv)
				{
					chn.nAutoVibDepth = 0;
					chn.nAutoVibPos = 0;
				}

				chn.RightVol = chn.LeftVol = 0;

/*				if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null) && (channelHint != Snd_Def.ChannelIndex_Invalid))
				{
					// Test case: AdlibZeroVolumeNote.s3m
					if (m_PlayBehaviour[PlayBehaviour.OplNoteOffOnNoteChange])
						m_Opl.NoteOff(channelHint);
					else if (m_PlayBehaviour[PlayBehaviour.OplNoteStopWith0Hz])
						m_Opl.Frequency(channelHint, 0, true, false);
				}*///XX
			}

			// Special case for MPT
			if (bManual)
				chn.dwFlags.Reset(ChannelFlags.Chn_Mute);

			if ((chn.dwFlags.Test(ChannelFlags.Chn_Mute) && ((m_MixerSettings.MixerFlags & MixerFlags.MuteChnMode) != 0)) ||
				((chn.pModSample != null) && chn.pModSample.uFlags.Test(ChannelFlags.Chn_Mute) && !bManual) ||
				((chn.pModInstrument != null) && chn.pModInstrument.dwFlags.Test(InstrumentFlags.Mute) && !bManual))
			{
				if (!bManual)
					chn.nPeriod = 0;
			}

			bool wasGlobalSlideRunning = chn.AutoSlide.IsActive(AutoSlideCommand.GlobalVolumeSlide);
			bool wasChannelVolSlideRunning = chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownWithDuration);

			chn.AutoSlide.Reset();
			chn.AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, wasGlobalSlideRunning);
			chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownWithDuration, wasChannelVolSlideRunning);
		}



		/********************************************************************/
		/// <summary>
		/// Apply sample or instrument panning
		/// </summary>
		/********************************************************************/
		public void ApplyInstrumentPanning(ModChannel chn, ModInstrument instr, ModSample smp)//XX 2211
		{
			int32 newPan = int32.MinValue;

			// Default instrument panning
			if ((instr != null) && instr.dwFlags.Test(InstrumentFlags.SetPanning))
				newPan = (int32)instr.nPan;

			// Default sample panning
			if ((smp != null) && smp.uFlags.Test(ChannelFlags.Chn_Panning))
				newPan = smp.nPan;

			if (newPan != int32.MinValue)
			{
				chn.SetInstrumentPan(newPan, this);

				// IT compatibility: Sample and instrument panning overrides channel surround status.
				// Test case: SmpInsPanSurround.it
				if (m_PlayBehaviour[PlayBehaviour.PanOverride] && !m_PlayState.m_Flags.Test(PlayFlags.Song_SurroundPan))
					chn.dwFlags.Reset(ChannelFlags.Chn_Surround);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public ChannelIndex GetNnaChannel(ChannelIndex nChn)//XX 2234
		{
			// Check for empty channel
			for (ChannelIndex i = GetNumChannels(); i < m_PlayState.Chn.size(); i++)
			{
				ModChannel c = m_PlayState.Chn[i];

				// Sample playing?
				if (c.nLength != 0)
					continue;

				// Can a plugin potentially be playing?
				if (!c.HasMidiOutput())
					return i;

				// Has the plugin note already been released? (note: lastMidiNoteWithoutArp is set from within IMixPlugin, so this implies that there is a valid plugin assignment)
				if (c.dwFlags.Test(ChannelFlags.Chn_KeyOff | ChannelFlags.Chn_NoteFade) || (c.LastMidiNoteWithoutArp == ModCommand.Note_None))
					return i;
			}

			int32 vol = 0x800100;

			if (nChn < m_PlayState.Chn.size())
			{
				ModChannel srcChn = m_PlayState.Chn[nChn];

				if ((srcChn.nFadeOutVol == 0) && (srcChn.nLength != 0))
					return Snd_Def.ChannelIndex_Invalid;

				vol = (srcChn.nRealVolume << 9) | srcChn.nVolume;
			}

			// All channels are used: check for lowest volume
			ChannelIndex result = Snd_Def.ChannelIndex_Invalid;
			uint32 envPos = 0;

			for (ChannelIndex i = GetNumChannels(); i < m_PlayState.Chn.size(); i++)
			{
				ModChannel c = m_PlayState.Chn[i];

				// Stopped OPL channel
//XX				if (c.dwFlags.Test(ChannelFlags.Chn_Adlib) && ((m_Opl == null) || !m_Opl.IsActive(i)))
//					return i;

				if ((c.nLength != 0) && (c.nFadeOutVol == 0))
					return i;

				// Use a combination of real volume [14 bit] (which includes volume envelopes, but also potentially global volume) and note volume [9 bit].
				// Rationale: We need volume envelopes in case e.g. all NNA channels are playing at full volume but are looping on a 0-volume envelope node.
				// But if global volume is not applied to master and the global volume temporarily drops to 0, we would kill arbitrary channels. Hence, add the note volume as well
				int32 v = (c.nRealVolume << 9) | c.nVolume;

				// Less priority to looped samples
				if (c.dwFlags.Test(ChannelFlags.Chn_Loop))
					v /= 2;

				// Less priority for channels potentially held for plugin notes with NNA=continue the older they get
				if ((c.nLength == 0) && (c.nMasterChn != 0))
					v -= (int32)(Math.Min((uint32)c.NnaChannelAge * c.NnaChannelAge, (int32.MaxValue / 16)) * 16);

				if ((v < vol) || ((v == vol) && ((c.VolEnv.nEnvPosition > envPos) || !c.VolEnv.Flags.Test(EnvelopeFlags.Enabled))))
				{
					envPos = c.VolEnv.nEnvPosition;
					vol = v;
					result = i;
				}
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public ChannelIndex CheckNna(ChannelIndex nChn, uint32 instr, c_int note, bool forceCut)//XX 2292
		{
			ModChannel srcChn = m_PlayState.Chn[nChn];
			ModInstrument pIns = null;

			if (!ModCommand.IsNote((ModCommandNote)note))
				return Snd_Def.ChannelIndex_Invalid;

			// Do we need to apply New/Duplicate Note Action to an instrument plugin?
			bool applyNnaToPlug = false;

			// Always NNA cut
			if (((GetType_() & (ModType.It | ModType.Mpt | ModType.Mt2)) == 0) || (m_nInstruments == 0) || forceCut)
			{
				if (srcChn.dwFlags.Test(ChannelFlags.Chn_Mute))
					return Snd_Def.ChannelIndex_Invalid;

				if ((srcChn.nLength == 0) || ((srcChn.RightVol | srcChn.LeftVol) == 0))
					return Snd_Def.ChannelIndex_Invalid;

/*//XX				if (srcChn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
				{
					m_Opl.NoteCut(nChn, false);
					return Snd_Def.ChannelIndex_Invalid;
				}*/

				ChannelIndex nnaChn_ = GetNnaChannel(nChn);

				if (nnaChn_ == Snd_Def.ChannelIndex_Invalid)
					return Snd_Def.ChannelIndex_Invalid;

				ModChannel chn_ = m_PlayState.Chn[nnaChn_];
				StopOldNna(chn_, nnaChn_);

				// Copy channel
				srcChn.CopyTo(chn_);

				chn_.dwFlags.Reset(ChannelFlags.Chn_Vibrato | ChannelFlags.Chn_Tremolo | ChannelFlags.Chn_Mute | ChannelFlags.Chn_Portamento);
				chn_.nPanbrelloOffset = 0;
				chn_.nMasterChn = (ChannelIndex)(nChn + 1);
				chn_.nCommand = EffectCommand.None;
				chn_.RowCommand.Clear();

				// Cut the note
				chn_.nFadeOutVol = 0;
				chn_.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);
				chn_.NnaChannelAge = 0;
				chn_.NnaGeneration = ++srcChn.NnaGeneration;

				// Stop this channel
				srcChn.nLength = 0;
				srcChn.Position.Set(0);
				srcChn.nROfs = srcChn.nLOfs = 0;
				srcChn.RightVol = srcChn.LeftVol = 0;

				return nnaChn_;
			}

			if (instr > GetNumInstruments())
				instr = 0;

			ModSample pSample = srcChn.pModSample;

			// If no instrument is given, assume previous instrument to still be valid.
			// Test case: DNA-NoInstr.it
			pIns = instr > 0 ? Instruments[instr] : srcChn.pModInstrument;
			c_int dnaNote = note;

			if (pIns != null)
			{
				SampleIndex smp = pIns.Keyboard[note - ModCommand.Note_Min];

				// IT compatibility: DCT = note uses pattern notes for comparison
				// Note: This is not applied in case kITRealNoteMapping is not set to keep playback of legacy modules simple (chn.nNote is translated note in that case)
				// Test case: dct_smp_note_test.it
				if (!m_PlayBehaviour[PlayBehaviour.ItDctBehaviour] || !m_PlayBehaviour[PlayBehaviour.ItRealNoteMapping])
					dnaNote = pIns.NoteMap[note - ModCommand.Note_Min];

				if (smp > 0)
					pSample = Samples[(smp <= GetNumSamples()) ? smp : 0];
				else if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlot] && !pIns.HasValidMidiChannel())
				{
					// Impulse Tracker ignores empty slots.
					// We won't ignore them if a plugin is assigned to this slot, so that VSTis still work as intended.
					// Test case: emptyslot.it, PortaInsNum.it, gxsmp.it, gxsmp2.it
					return Snd_Def.ChannelIndex_Invalid;
				}
			}

			if (srcChn.dwFlags.Test(ChannelFlags.Chn_Mute))
				return Snd_Def.ChannelIndex_Invalid;

			for (ChannelIndex i = nChn; i < m_PlayState.Chn.size(); i++)
			{
				// Only apply to background channels, or the same pattern channel
				if ((i < GetNumChannels()) && (i != nChn))
					continue;

				ModChannel chn_ = m_PlayState.Chn[i];
#pragma warning disable CS0219 // Variable is assigned but its value is never used
				bool applyDnaToPlug = false;//XX Fjern pragma når plug-in support er lavet
#pragma warning restore CS0219 // Variable is assigned but its value is never used

				if (((chn_.nMasterChn == (nChn + 1)) || (i == nChn)) && (chn_.pModInstrument != null))
				{
					bool applyDna = false;

					// Duplicate Check Type
					switch (chn_.pModInstrument.nDct)
					{
						case DuplicateCheckType.None:
							break;

						case DuplicateCheckType.Note:
						{
							if ((dnaNote != ModCommand.Note_None) && (chn_.nNote == dnaNote) && (pIns == chn_.pModInstrument))
								applyDna = true;

							if ((pIns != null) && (pIns.nMixPlug != 0))
								applyDnaToPlug = true;

							break;
						}

						// Sample
						case DuplicateCheckType.Sample:
						{
							// IT compatibility: DCT = sample only applies to same instrument
							// Test case: dct_smp_note_test.it
							if ((pSample != null) && (pSample == chn_.pModSample) && ((pIns == chn_.pModInstrument) || !m_PlayBehaviour[PlayBehaviour.ItDctBehaviour]))
								applyDna = true;

							break;
						}

						// Instrument
						case DuplicateCheckType.Instrument:
						{
							if (pIns == chn_.pModInstrument)
								applyDna = true;

							if ((pIns != null) && (pIns.nMixPlug != 0))
								applyDnaToPlug = true;

							break;
						}

						// Plugin
						case DuplicateCheckType.Plugin:
						{
							if ((pIns != null) && (pIns.nMixPlug != 0) && (pIns.nMixPlug == chn_.pModInstrument.nMixPlug))
							{
								applyDnaToPlug = true;
								applyDna = true;
							}

							break;
						}
					}

					// Duplicate Note Action
					if (applyDna)
					{
						switch (chn_.pModInstrument.nDna)
						{
							// Cut
							case DuplicateNoteAction.NoteCut:
							{
								KeyOff(chn_);
								chn_.nVolume = 0;

//XX								if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//									m_Opl.NoteCut(i);

								break;
							}

							// Note Off
							case DuplicateNoteAction.NoteOff:
							{
								KeyOff(chn_);

//XX								if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//									m_Opl.NoteOff(i);

								break;
							}

							// Note Fade
							case DuplicateNoteAction.NoteFade:
							{
								chn_.dwFlags.Set(ChannelFlags.Chn_NoteFade);

//XX								if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null) && !m_PlayBehaviour[PlayBehaviour.OplWithNna])
//									m_Opl.NoteOff(i);

								break;
							}
						}

						if (chn_.nVolume == 0)
						{
							chn_.nFadeOutVol = 0;
							chn_.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);
						}
					}
				}
			}

			// New Note Action
			if (!srcChn.IsSamplePlaying() && !applyNnaToPlug)
				return Snd_Def.ChannelIndex_Invalid;

			ChannelIndex nnaChn = GetNnaChannel(nChn);

			if (nnaChn == Snd_Def.ChannelIndex_Invalid)
				return Snd_Def.ChannelIndex_Invalid;

			ModChannel chn = m_PlayState.Chn[nnaChn];
			StopOldNna(chn, nnaChn);

			// Copy Channel
			srcChn.CopyTo(chn);

			chn.dwFlags.Reset(ChannelFlags.Chn_Vibrato | ChannelFlags.Chn_Tremolo | ChannelFlags.Chn_Portamento);
			chn.nPanbrelloOffset = 0;

			chn.nMasterChn = (ChannelIndex)(nChn < GetNumChannels() ? nChn + 1 : 0);
			chn.nCommand = EffectCommand.None;
			chn.NnaChannelAge = 0;
			chn.NnaGeneration = ++srcChn.NnaGeneration;

			// Key Off the note
			switch (srcChn.nNna)
			{
				case NewNoteAction.NoteOff:
				{
					KeyOff(chn);

/*//XX					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
					{
						if (m_PlayBehaviour[PlayBehaviour.OplWithNna])
						{
							m_Opl.MoveChannel(nChn, nnaChn);
							m_Opl.NoteOff(nnaChn);	// This needs to be done on the NNA channel so that our PlaybackTest implementation knows that it belongs to the "old" note, not to the "new" note
						}
						else
							m_Opl.NoteOff(nChn);
					}*/

					break;
				}

				case NewNoteAction.NoteCut:
				{
					chn.nFadeOutVol = 0;
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

//XX					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//						m_Opl.NoteCut(i);

					break;
				}
				case NewNoteAction.NoteFade:
				{
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

/*//XX					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
					{
						if (m_PlayBehaviour[PlayBehaviour.OplWithNna])
							m_Opl.MoveChannel(nChn, nnaChn);
						else
							m_Opl.NoteOff(nChn);
					}*/

					break;
				}

				case NewNoteAction.Continue:
				{
//XX					if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//						m_Opl.MoveChannel(nChn, nnaChn);

					break;
				}
			}

			if (chn.nVolume == 0)
			{
				chn.nFadeOutVol = 0;
				chn.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);
			}

			// Stop this channel
			srcChn.nLength = 0;
			srcChn.Position.Set(0);
			srcChn.nROfs = srcChn.nLOfs = 0;

			return nnaChn;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void StopOldNna(ModChannel chn, ChannelIndex channel)//XX 2585
		{
//XX			if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//				m_Opl.NoteCut(channel);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool ProcessEffects()//XX 2610
		{
			m_PlayState.m_BreakRow = Snd_Def.RowIndex_Invalid;		// Is changed if a break to row command is encountered
			m_PlayState.m_PatLoopRow = Snd_Def.RowIndex_Invalid;	// Is changed if a pattern loop jump-back is executed
			m_PlayState.m_PosJump = Snd_Def.OrderIndex_Invalid;

			for (ChannelIndex nChn = 0; nChn < GetNumChannels(); nChn++)
			{
				ModChannel chn = m_PlayState.Chn[nChn];

				uint32 tickCount = m_PlayState.m_nTickCount % (m_PlayState.m_nMusicSpeed + m_PlayState.m_nFrameDelay);
				uint32 instr = chn.RowCommand.Instr;
				ModCommandVolCmd volCmd = chn.RowCommand.VolCmd;
				ModCommandVol vol = chn.RowCommand.Vol;
				ModCommandCommand cmd = chn.RowCommand.Command;
				uint32 param = chn.RowCommand.Param;
				bool bPorta = chn.RowCommand.IsTonePortamento();

				uint32 nStartTick = 0;
				chn.IsFirstTick = m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick);

				// Process parameter control note
				if (chn.RowCommand.Note == ModCommand.Note_Pc)
				{
				}

				// Process continuous parameter control note.
				// Row data is cleared after first tick so on following
				// ticks using channels m_nPlugParamValueStep to identify
				// the need for parameter control. The condition cmd == 0
				// is to make sure that m_nPlugParamValueStep != 0 because
				// of NOTE_PCS, not because of macro
				if ((chn.RowCommand.Note == ModCommand.Note_Pcs) || ((cmd == EffectCommand.None) && (chn.m_PlugParamValueStep != 0)))
				{
				}

				// Apart from changing parameters, parameter control notes are intended to be 'invisible'.
				// To achieve this, clearing the note data so that rest of the process sees the row as empty row
				if (ModCommand.IsPcNote(chn.RowCommand.Note))
				{
					chn.RowCommand.Clear();

					instr = 0;
					volCmd = VolumeCommand.None;
					vol = 0;
					cmd = EffectCommand.None;
					param = 0;
					bPorta = false;
				}

				// IT compatibility: Empty sample mapping
				// This is probably the single biggest WTF replayer bug in Impulse Tracker.
				// In instrument mode, when an note + instrument is triggered that does not map to any sample, the entire cell (including potentially present global effects!)
				// is ignored. Even better, if on a following row another instrument number (this time without a note) is encountered, we end up in the same situation!
				// Test cases: NoMap.it, NoMapEffects.it
				if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell] && (instr > 0) && (instr <= GetNumInstruments()) && (Instruments[instr] != null) && !Instruments[instr].HasValidMidiChannel())
				{
					ModCommandNote note = chn.RowCommand.Note != ModCommand.Note_None ? chn.RowCommand.Note : chn.nNewNote;

					if (ModCommand.IsNote(note) && (Instruments[instr].Keyboard[note - ModCommand.Note_Min] == 0))
					{
						chn.nNewNote = chn.nLastNote = note;
						chn.nNewIns = (ModCommandInstr)instr;
						chn.RowCommand.Clear();
						continue;
					}
				}

				bool continueNote = !bPorta && m_PlayBehaviour[PlayBehaviour.ContinueSampleWithoutInstr] && (chn.RowCommand.Instr == 0) && chn.dwFlags.Test(ChannelFlags.Chn_Loop) && (chn.pCurrentSample != null);

				if (continueNote)
					bPorta = true;

				// Process Invert Loop (MOD Effect, called every row if it's active)
				if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					InvertLoop(m_PlayState.Chn[nChn]);
				else
				{
					if (instr != 0)
						m_PlayState.Chn[nChn].nEFxOffset = 0;
				}

				// Process special effects (note delay, pattern delay, pattern loop)
				if (cmd == EffectCommand.DelayCut)
				{
					// :xy --> note delay until tick x, note cut at tick x+y
					nStartTick = (param & 0xf0) >> 4;
					uint32 cutAtTick = nStartTick + (param & 0x0f);
					NoteCut(nChn, cutAtTick, m_PlayBehaviour[PlayBehaviour.ItSCxStopsSample]);
				}
				else if ((cmd == EffectCommand.ModCmdEx) || (cmd == EffectCommand.S3MCmdEx))
				{
					if ((param == 0) && ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0))
						param = chn.nOldCmdEx;
					else
						chn.nOldCmdEx = (ModCommandParam)param;

					// Note Delay?
					if ((param & 0xf0) == 0xd0)
					{
						nStartTick = param & 0x0f;

						if (nStartTick == 0)
						{
							// IT compatibility 22. SD0 == SD1
							if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
								nStartTick = 1;
							// ST3 ignores notes with SD0 completely
							else if (GetType_() == ModType.S3M)
								continue;
						}
						else if ((nStartTick >= (m_PlayState.m_nMusicSpeed + m_PlayState.m_nFrameDelay)) && m_PlayBehaviour[PlayBehaviour.ItOutOfRangeDelay])
						{
							// IT compatibility 08. Handling of out-of-range delay command.
							// Additional test case: tickdelay.it
							if (instr != 0)
								chn.nNewIns = (ModCommandInstr)instr;

							continue;
						}
					}
					else if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						// Pattern Loop?
						if ((param & 0xf0) == 0xe0)
						{
							// Pattern Delay
							// In Scream Tracker 3 / Impulse Tracker, only the first delay command on this row is considered.
							// Test cases: PatternDelays.it, PatternDelays.s3m, PatternDelays.xm
							// XXX In Scream Tracker 3, the "left" channels are evaluated before the "right" channels, which is not emulated here!
							if (((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) == 0) || (m_PlayState.m_nPatternDelay == 0))
							{
								if (((GetType_() & ModType.S3M) == 0) || ((param & 0x0f) != 0))
								{
									// While Impulse Tracker *does* count S60 as a valid row delay (and thus ignores any other row delay commands on the right),
									// Scream Tracker 3 simply ignores such commands
									m_PlayState.m_nPatternDelay = 1 + (param & 0x0f);
								}
							}
						}
					}
				}

				if ((GetType_() == ModType.Mtm) && (cmd == EffectCommand.ModCmdEx) && ((param & 0xf0) == 0xd0))
				{
					// Apparently, retrigger and note delay have the same behaviour in MultiTracker:
					// They both restart the note at tick x, and if there is a note on the same row,
					// this note is started on the first tick
					nStartTick = 0;
					param = 0x90 | (param & 0x0f);
				}

				if ((nStartTick != 0) && (chn.RowCommand.Note == ModCommand.Note_KeyOff) && (chn.RowCommand.VolCmd == VolumeCommand.Panning) && m_PlayBehaviour[PlayBehaviour.Ft2PanWithDelayedNoteOff])
				{
					// FT2 compatibility: If there's a delayed note off, panning commands are ignored. WTF!
					// Test case: PanOff.xm
					chn.RowCommand.VolCmd = VolumeCommand.None;
				}

				bool triggerNote = m_PlayState.m_nTickCount == nStartTick;	// Can be delayed by a note delay effect

				if (m_PlayBehaviour[PlayBehaviour.Ft2OutOfRangeDelay] && (nStartTick >= m_PlayState.m_nMusicSpeed))
				{
					// FT2 compatibility: Note delays greater than the song speed should be ignored.
					// However, EEx pattern delay is *not* considered at all.
					// Test case: DelayCombination.xm, PortaDelay.xm
					triggerNote = false;
				}
				else if (m_PlayBehaviour[PlayBehaviour.RowDelayWithNoteDelay] && (nStartTick > 0) && (tickCount == nStartTick))
				{
					// IT compatibility: Delayed notes (using SDx) that are on the same row as a Row Delay effect are retriggered.
					// ProTracker / Scream Tracker 3 / FastTracker 2 do the same.
					// Test case: PatternDelay-NoteDelay.it, PatternDelay-NoteDelay.xm, PatternDelaysRetrig.mod
					triggerNote = true;
				}

				// IT compatibility: Tick-0 vs non-tick-0 effect distinction is always based on tick delay.
				// Test case: SlideDelay.it
				if (m_PlayBehaviour[PlayBehaviour.ItFirstTickHandling])
					chn.IsFirstTick = tickCount == nStartTick;

				chn.TriggerNote = false;

				// FT2 compatibility: Note + portamento + note delay = no portamento
				// Test case: PortaDelay.xm
				if (m_PlayBehaviour[PlayBehaviour.Ft2PortaDelay] && (nStartTick != 0))
					bPorta = false;

				if (m_SongFlags.Test(SongFlags.Pt_Mode) && (instr != 0) && (m_PlayState.m_nTickCount == 0))
				{
					// Instrument number resets the stacked ProTracker offset.
					// Test case: ptoffset.mod
					chn.PrevNoteOffset = 0;

					// ProTracker compatibility: Sample properties are always loaded on the first tick, even when there is a note delay.
					// Test case: InstrDelay.mod
					if (!triggerNote && chn.IsSamplePlaying())
					{
						chn.nNewIns = (ModCommandInstr)instr;
						chn.SwapSampleIndex = GetSampleIndex(chn.nLastNote, instr);

						if (instr <= GetNumSamples())
						{
							chn.nVolume = Samples[instr].nVolume;
							chn.nFineTune = Samples[instr].nFineTune;
						}
					}
				}

				// Handles note/instrument/volume changes
				if (triggerNote)
				{
					ModCommandNote note = chn.RowCommand.Note;

					if (instr != 0)
					{
						chn.nNewIns = (ModCommandInstr)instr;
						chn.SwapSampleIndex = GetSampleIndex(ModCommand.IsNote(note) ? note : chn.nLastNote, instr);
					}

					if (ModCommand.IsNote(note) && m_PlayBehaviour[PlayBehaviour.Ft2Transpose])
					{
						// Notes that exceed FT2's limit are completely ignored.
						// Test case: NoteLimit.xm
						c_int transpose = chn.nTranspose;

						if ((instr != 0) && !bPorta)
						{
							// Refresh transpose
							// Test case: NoteLimit2.xm
							SampleIndex sample = GetSampleIndex(note, instr);

							if (sample > 0)
								transpose = GetSample(sample).RelativeTone;
						}

						c_int computedNote = note + transpose;

						if ((computedNote < (ModCommand.Note_Min + 11)) || (computedNote > (ModCommand.Note_Min + 130)))
							note = ModCommand.Note_None;
					}
					else if (((GetType_() & (ModType.It | ModType.Mpt | ModType.J2B)) != 0) && (GetNumInstruments() != 0) && ModCommand.IsNoteOrEmpty(note))
					{
						// IT compatibility: Invalid instrument numbers do nothing, but they are remembered for upcoming notes and do not trigger a note in that case.
						// Test case: InstrumentNumberChange.it
						InstrumentIndex instrToCheck = (InstrumentIndex)(instr != 0 ? instr : chn.nOldIns);

						if ((instrToCheck != 0) && ((instrToCheck > GetNumInstruments()) || (Instruments[instrToCheck] == null)))
						{
							note = ModCommand.Note_None;
							instr = 0;
						}
					}

					// XM: FT2 ignores a note next to a K00 effect, and a fade-out seems to be done when no volume envelope is present (not exactly the Kxx behaviour)
					if ((cmd == EffectCommand.KeyOff) && (param == 0) && m_PlayBehaviour[PlayBehaviour.Ft2KeyOff])
					{
						note = ModCommand.Note_None;
						instr = 0;
					}

					bool retrigEnv = (note == ModCommand.Note_None) && (instr != 0);

					// Apparently, any note number in a pattern causes instruments to recall their original volume settings - no matter if there's a Note Off next to it or whatever.
					// Test cases: keyoff+instr.xm, delay.xm
					bool reloadSampleSettings = m_PlayBehaviour[PlayBehaviour.Ft2ReloadSampleSettings] && (instr != 0);
					bool keepInstr = ((GetType_() & (ModType.It | ModType.Mpt)) != 0) || m_PlayBehaviour[PlayBehaviour.St3SampleSwap];

					if (m_PlayBehaviour[PlayBehaviour.ModSampleSwap])
					{
						// ProTracker Compatibility: If a sample was stopped before, lone instrument numbers can retrigger it
						// Test cases: PTSwapEmpty.mod, PTInstrVolume.mod, PTStoppedSwap.mod
						if (!chn.IsSamplePlaying() && (instr <= GetNumSamples()) && Samples[instr].uFlags.Test(ChannelFlags.Chn_Loop))
							keepInstr = true;
					}

					// Now it's time for some FT2 crap...
					if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
					{
						// XM: Key-Off + Sample == Note Cut (BUT: Only if no instr number or volume effect is present!)
						// Test case: NoteOffVolume.xm
						if ((note == ModCommand.Note_KeyOff) && (((instr == 0) && (volCmd != VolumeCommand.Volume) && (cmd != EffectCommand.Volume)) || !m_PlayBehaviour[PlayBehaviour.Ft2KeyOff]) && ((chn.pModInstrument == null) || !chn.pModInstrument.VolEnv.dwFlags.Test(EnvelopeFlags.Enabled)))
						{
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							chn.nVolume = 0;
							note = ModCommand.Note_None;
							instr = 0;
							retrigEnv = false;

							// FT2 Compatibility: Start fading the note for notes with no delay. Only relevant when a volume command is encountered after the note-off.
							// Test case: NoteOffFadeNoEnv.xm
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) && m_PlayBehaviour[PlayBehaviour.Ft2NoteOffFlags])
								chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
						}
						else if (m_PlayBehaviour[PlayBehaviour.Ft2RetrigWithNoteDelay] && !m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						{
							// FT2 Compatibility: Some special hacks for rogue note delays... (EDx with x > 0)
							// Apparently anything that is next to a note delay behaves totally unpredictable in FT2. Swedish tracker logic. :)
							retrigEnv = true;

							// Portamento + Note Delay = No Portamento
							// Test case: porta-delay.xm
							bPorta = false;

							if (note == ModCommand.Note_None)
							{
								// If there's a note delay but no real note, retrig the last note.
								// Test case: delay2.xm, delay3.xm
								note = (ModCommandNote)(chn.nNote - chn.nTranspose);
							}
							else if (note >= ModCommand.Note_Min_Special)
							{
								// Gah! Even Note Off + Note Delay will cause envelopes to *retrigger*! How stupid is that?
								// ... Well, and that is actually all it does if there's an envelope. No fade out, no nothing. *sigh*
								// Test case: OffDelay.xm
								note = ModCommand.Note_None;
								keepInstr = false;
								reloadSampleSettings = true;
							}
							else if ((instr != 0) || !m_PlayBehaviour[PlayBehaviour.Ft2NoteDelayWithoutInstr])
							{
								// Normal note (only if there is an instrument, test case: DelayVolume.xm)
								keepInstr = true;
								reloadSampleSettings = true;
							}
						}
					}

					if ((retrigEnv && !m_PlayBehaviour[PlayBehaviour.Ft2ReloadSampleSettings]) || reloadSampleSettings)
					{
						ModSample oldSample = null;

						// Reset default volume when retriggering envelopes
						if (GetNumInstruments() != 0)
							oldSample = chn.pModSample;
						else if (instr <= GetNumSamples())
						{
							// Case: Only samples are used; no instruments
							oldSample = Samples[instr];
						}

						if (oldSample != null)
						{
							if (!oldSample.uFlags.Test(ChannelFlags.Smp_NoDefaultVolume) && ((GetType_() != ModType.S3M) || oldSample.HasSampleData()))
							{
								chn.nVolume = oldSample.nVolume;
								chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							}

							if (reloadSampleSettings)
							{
								// Also reload panning
								chn.SetInstrumentPan(oldSample.nPan, this);
							}
						}
					}

					// FT2 compatibility: Instrument number disables tremor effect
					// Test case: TremorInstr.xm, TremoRecover.xm
					if (m_PlayBehaviour[PlayBehaviour.Ft2Tremor] && (instr != 0))
						chn.nTremorCount = 0x20;

					// IT compatibility: Envelope retriggering with instrument number based on Old Effects and Compatible Gxx flags:
					// OldFX CompatGxx Env Behaviour
					// ----- --------- -------------
					//  off     off    never reset
					//  on      off    reset on instrument without portamento
					//  off     on     reset on instrument with portamento
					//  on      on     always reset
					// Test case: ins-xx.it, ins-ox.it, ins-oc.it, ins-xc.it, ResetEnvNoteOffOldFx.it, ResetEnvNoteOffOldFx2.it, noteoff3.it
					if ((GetNumInstruments() != 0) && m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOffOldEffects] && (instr != 0) && !ModCommand.IsNote(note))
					{
						if ((bPorta && m_SongFlags.Test(SongFlags.ItCompatGxx)) || (!bPorta && m_SongFlags.Test(SongFlags.ItOldEffects)))
						{
							chn.ResetEnvelopes();
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							chn.nFadeOutVol = 65536;
						}
					}

					if (retrigEnv)	// Case: instrument with no note data
					{
						// IT compatibility: Instrument with no note
						if (m_PlayBehaviour[PlayBehaviour.ItInstrWithoutNote] || (GetType_() == ModType.Plm))
						{
							// IT compatibility: Completely retrigger note after sample end to also reset portamento.
							// Test case: PortaResetAfterRetrigger.it
							bool triggerAfterSmpEnd = m_PlayBehaviour[PlayBehaviour.ItMultiSampleInstrumentNumber] && !chn.IsSamplePlaying();

							if (GetNumInstruments() != 0)
							{
								// Instrument mode
								if ((instr <= GetNumInstruments()) && ((chn.pModInstrument != Instruments[instr]) || triggerAfterSmpEnd))
									note = chn.nNote;
							}
							else
							{
								// Sample mode
								if ((instr < Snd_Def.Max_Samples) && ((chn.pModSample != Samples[instr]) || triggerAfterSmpEnd))
									note = chn.nNote;
							}
						}

						if ((GetNumInstruments() != 0) && ((GetType_() & (ModType.Xm | ModType.Mt2 | ModType.Med)) != 0))
						{
							chn.ResetEnvelopes();
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							chn.dwFlags.Reset(ChannelFlags.Chn_NoteFade);
							chn.nAutoVibDepth = 0;
							chn.nAutoVibPos = 0;
							chn.nFadeOutVol = 65536;

							// FT2 Compatibility: Reset key-off status with instrument number
							// Test case: NoteOffInstrChange.xm
							if (m_PlayBehaviour[PlayBehaviour.Ft2NoteOffFlags])
								chn.dwFlags.Reset(ChannelFlags.Chn_KeyOff);
						}

						if (!keepInstr)
							instr = 0;
					}

					// Note Cut/Off/Fade => ignore instrument
					if (note >= ModCommand.Note_Min_Special)
					{
						// IT compatibility: Default volume of sample is recalled if instrument number is next to a note-off.
						// Test case: NoteOffInstr.it, noteoff2.it
						if (m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOff] && (instr != 0))
						{
							SampleIndex smp = GetSampleIndex(chn.nLastNote, instr);

							if ((smp > 0) && !Samples[smp].uFlags.Test(ChannelFlags.Smp_NoDefaultVolume))
								chn.nVolume = Samples[smp].nVolume;
						}

						// IT compatibility: Note-off with instrument number + Old Effects retriggers envelopes.
						// Test case: ResetEnvNoteOffOldFx.it
						if (!m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOffOldEffects] || !m_SongFlags.Test(SongFlags.ItOldEffects))
							instr = 0;
					}

					uint8 previousNewNote = chn.nNewNote;

					if (ModCommand.IsNote(note))
					{
						chn.nNewNote = chn.nLastNote = note;

						// New Note Action?
						if (!bPorta)
							CheckNna(nChn, instr, note, false);

						chn.RestorePanAndFilter();
					}

					// Instrument Change?
					if (instr != 0)
					{
						ChannelOffsets ofs = GetChannelOffsets(chn, nChn);

						ModSample oldSample = chn.pModSample;

						InstrumentChange(chn, instr, bPorta, true);

//XX						if (!chn.dwFlags.Test(ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute) && (chn.pModSample != null) && chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//							m_Opl.Patch(nChn, chn.pModSample.Addlib);

						// IT compatibility: Keep new instrument number for next instrument-less note even if sample playback is stopped
						// Test case: StoppedInstrSwap.it
						if (GetType_() == ModType.Mod)
						{
							// Test case: PortaSwapPT.mod
							if (!bPorta || !m_PlayBehaviour[PlayBehaviour.ModSampleSwap])
								chn.nNewIns = 0;
						}
						else
						{
							if (!m_PlayBehaviour[PlayBehaviour.ItInstrWithNoteOff] || ModCommand.IsNote(note))
								chn.nNewIns = 0;
						}

						// When swapping samples without explicit note change (e.g. during portamento), avoid clicks at end of sample (as there won't be an NNA channel to fade the sample out)
						if ((oldSample != null) && (oldSample != chn.pModSample))
						{
							ofs.OfsL += chn.nLOfs;
							ofs.OfsR += chn.nROfs;
							chn.nLOfs = 0;
							chn.nROfs = 0;
						}

						if (m_PlayBehaviour[PlayBehaviour.ItPortamentoSwapResetsPos])
						{
							// Test cases: PortaInsNum.it, PortaSample.it
							if (ModCommand.IsNote(note) && (oldSample != chn.pModSample))
							{
								chn.Position.Set(0);
							}
						}
						else if (((GetType_() & (ModType.It | ModType.Mpt)) != 0) && (oldSample != chn.pModSample) && ModCommand.IsNote(note))
						{
							// Special IT case: portamento+note causes sample change -> ignore portamento
							bPorta = false;
						}
						else if (m_PlayBehaviour[PlayBehaviour.St3SampleSwap] && (oldSample != chn.pModSample) && (bPorta || !ModCommand.IsNote(note)) && (chn.Position.GetUInt() > chn.nLength))
						{
							// ST3 with SoundBlaster does sample swapping and continues playing the new sample where the old sample was stopped.
							// If the new sample is shorter than that, it is stopped, even if it could be looped.
							// This also applies to portamento between different samples.
							// Test case: SampleSwap.s3m
							chn.nLength = 0;
						}
						else if (m_PlayBehaviour[PlayBehaviour.ModSampleSwap] && !chn.IsSamplePlaying())
						{
							// If channel was paused and is resurrected by a lone instrument number, reset the sample position.
							// Test case: PTSwapEmpty.mod
							chn.Position.Set(0);
						}
					}

					// New Note?
					if (note != ModCommand.Note_None)
					{
						bool instrChange = (instr == 0) && (chn.nNewIns != 0) && ModCommand.IsNote(note);

						if (instrChange)
						{
							// If we change to a new instrument, we need to do so based on whatever previous note would have played
							// - so that we trigger the correct sample in a multisampled instrument (based on the previous note, not the new note).
							// Test case: InitialNoteMemoryInstrMode.it
							if (m_PlayBehaviour[PlayBehaviour.ItEmptyNoteMapSlotIgnoreCell] && ModCommand.IsNote(previousNewNote))
								chn.nNewNote = previousNewNote;

							InstrumentChange(chn, chn.nNewIns, bPorta, (chn.pModSample == null) && (chn.pModInstrument == null), (GetType_() & (ModType.Xm | ModType.Mt2)) == 0);

							chn.nNewNote = note;
							chn.SwapSampleIndex = chn.nNewIns = 0;
						}

//XX						if (!chn.dwFlags.Test(ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute) && (chn.pModSample != null) && chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null) && (instrChange || !m_Opl.IsActive(nChn)))
//							m_Opl.Patch(nChn, chn.pModSample.Adlib);

						NoteChange(chn, note, bPorta, (GetType_() & (ModType.Xm | ModType.Mt2)) == 0, false, nChn);

						if (continueNote)
							chn.nPeriod = chn.nPortamentoDest;

						if (ModCommand.IsNote(note))
							HandleDigiSamplePlayDirection(m_PlayState, nChn);

						if (bPorta && ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0) && (instr != 0))
						{
							chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							chn.ResetEnvelopes();
							chn.nAutoVibDepth = 0;
							chn.nAutoVibPos = 0;
						}

/*//XX						if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null) && ((note == ModCommand.Note_NoteCut) || (note == ModCommand.Note_KeyOff) || ((note == ModCommand.Note_Fade) && !m_PlayBehaviour[PlayBehaviour.OplFlexibleNoteOff])))
						{
							if (m_PlayBehaviour[PlayBehaviour.OplNoteStopWith0Hz])
								m_Opl.Frequency(nChn, 0, true, false);

							m_Opl.NoteOff(nChn);
						}*/
					}

					// Tick-0 only volume commands
					if (volCmd == VolumeCommand.Volume)
					{
						if (vol > 64)
							vol = 64;

						chn.nVolume = vol << 2;
						chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
					}
					else
					{
						if (volCmd == VolumeCommand.Panning)
							Panning(chn, vol, PanningType.Pan6Bit);
					}
				}

				if (m_PlayBehaviour[PlayBehaviour.St3NoMutedChannels] && ChnSettings[nChn].dwFlags.Test(ChannelFlags.Chn_Mute))	// Not even effects are processed on muted S3M channels
					continue;

				if (m_PlayState.m_nTickCount == 0)
					ResetAutoSlides(chn);

				// Volume Column Effect (except volume & panning)
				//
				// A few notes, paraphrased from ITTECH.TXT by Storlek (creator of schismtracker):
				// Ex/Fx/Gx are shared with Exx/Fxx/Gxx; Ex/Fx are 4x the 'normal' slide value
				// Gx is linked with Ex/Fx if Compat Gxx is off, just like Gxx is with Exx/Fxx
				// Gx values: 1, 4, 8, 16, 32, 64, 96, 128, 255
				// Ax/Bx/Cx/Dx values are used directly (i.e. D9 == D09), and are NOT shared with Dxx
				// (value is stored into nOldVolParam and used by A0/B0/C0/D0)
				// Hx uses the same value as Hxx and Uxx, and affects the *depth*
				// so... hxx = (hx | (oldhxx & 0xf0))  ???
				// TODO is this done correctly?
				bool doVolumeColumn = m_PlayState.m_nTickCount >= nStartTick;

				// FT2 compatibility: If there's a note delay, volume column effects are NOT executed
				// on the first tick and, if there's an instrument number, on the delayed tick.
				// Test case: VolColDelay.xm, PortaDelay.xm
				if (m_PlayBehaviour[PlayBehaviour.Ft2VolColDelay] && (nStartTick != 0))
					doVolumeColumn = (m_PlayState.m_nTickCount != 0) && ((m_PlayState.m_nTickCount != nStartTick) || ((chn.RowCommand.Instr == 0) && (volCmd != VolumeCommand.TonePortamento)));

				// IT compatibility: Various mind-boggling behaviours when combining volume colum and effect column portamentos
				// The most crucial thing here is to initialize effect memory in the exact right order.
				// Test cases: DoubleSlide.it, DoubleSlideCompatGxx.it
				if (m_PlayBehaviour[PlayBehaviour.ItDoublePortamentoSlides] && chn.IsFirstTick)
				{
					bool effectColumnTonePorta = (cmd == EffectCommand.TonePortamento) || (cmd == EffectCommand.TonePortaVol);

					if (effectColumnTonePorta)
						InitTonePortamento(chn, (uint16)(cmd == EffectCommand.TonePortaVol ? 0 : param));

					if (volCmd == VolumeCommand.TonePortamento)
						InitTonePortamento(chn, GetVolCmdTonePorta(chn.RowCommand, nStartTick).first);

					if ((vol != 0) && ((volCmd == VolumeCommand.PortaUp) || (volCmd == VolumeCommand.PortaDown)))
					{
						chn.nOldPortaUp = chn.nOldPortaDown = (uint8)(vol << 2);

						if (!effectColumnTonePorta && TonePortamentoSharesEffectMemory())
							chn.PortamentoSlide = (uint16)(vol << 2);
					}

					if ((param != 0) && ((cmd == EffectCommand.PortamentoUp) || (cmd == EffectCommand.PortamentoDown)))
					{
						chn.nOldPortaUp = chn.nOldPortaDown = (uint8)param;

						if (TonePortamentoSharesEffectMemory())
							chn.PortamentoSlide = (uint16)param;
					}
				}

				if ((volCmd > VolumeCommand.Panning) && doVolumeColumn)
				{
					if (volCmd == VolumeCommand.TonePortamento)
					{
						(uint16 porta, bool clearEffectCommand) = GetVolCmdTonePorta(chn.RowCommand, nStartTick);

						if (clearEffectCommand)
							cmd = EffectCommand.None;

						TonePortamento(nChn, porta);
					}
					else
					{
						// FT2 Compatibility: FT2 ignores some volume commands with parameter = 0
						if (m_PlayBehaviour[PlayBehaviour.Ft2VolColMemory] && (vol == 0))
						{
							switch (volCmd)
							{
								case VolumeCommand.Volume:
								case VolumeCommand.Panning:
								case VolumeCommand.VibratoDepth:
									break;

								case VolumeCommand.PanSlideLeft:
								{
									// FT2 Compatibility: Pan slide left with zero parameter causes panning to be set to full left on every non-row tick.
									// Test case: PanSlideZero.xm
									if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
										chn.nPan = 0;

									goto default;
								}

								default:
								{
									// No memory here
									volCmd = VolumeCommand.None;
									break;
								}
							}
						}
						else if (!m_PlayBehaviour[PlayBehaviour.ItVolColMemory] && (volCmd != VolumeCommand.PlayControl))
						{
							// IT Compatibility: Effects in the volume column don't have an unified memory.
							// Test case: VolColMemory.it
							if (vol != 0)
								chn.nOldVolParam = vol;
							else
								vol = chn.nOldVolParam;
						}

						switch (volCmd)
						{
							case VolumeCommand.VolSlideUp:
							case VolumeCommand.VolSlideDown:
							{
								// IT Compatibility: Volume column volume slides have their own memory
								// Test case: VolColMemory.it
								if ((vol == 0) && m_PlayBehaviour[PlayBehaviour.ItVolColMemory])
								{
									vol = chn.nOldVolParam;

									if (vol == 0)
										break;
								}
								else
									chn.nOldVolParam = vol;

								// IT Compatibility: Volume column volume slides must not propagate their memory to the regular effect column
								// Test case: VolColNoSlideMemoryPropagation.it
								VolumeSlide(chn, (ModCommandParam)(volCmd == VolumeCommand.VolSlideUp ? (vol << 4) : vol), m_PlayBehaviour[PlayBehaviour.ItVolColNoSlidePropagation]);
								break;
							}

							case VolumeCommand.FineVolUp:
							{
								// IT Compatibility: Fine volume slides in the volume column are only executed on the first tick, not on multiples of the first tick in case of pattern delay
								// Test case: FineVolColSlide.it
								if ((m_PlayState.m_nTickCount == nStartTick) || !m_PlayBehaviour[PlayBehaviour.ItVolColMemory])
								{
									// IT Compatibility: Volume column volume slides have their own memory
									// Test case: VolColMemory.it
									FineVolumeUp(chn, vol, m_PlayBehaviour[PlayBehaviour.ItVolColMemory]);
								}

								break;
							}

							case VolumeCommand.FineVolDown:
							{
								// IT Compatibility: Fine volume slides in the volume column are only executed on the first tick, not on multiples of the first tick in case of pattern delay
								// Test case: FineVolColSlide.it
								if ((m_PlayState.m_nTickCount == nStartTick) || !m_PlayBehaviour[PlayBehaviour.ItVolColMemory])
								{
									// IT Compatibility: Volume column volume slides have their own memory
									// Test case: VolColMemory.it
									FineVolumeDown(chn, vol, m_PlayBehaviour[PlayBehaviour.ItVolColMemory]);
								}

								break;
							}

							case VolumeCommand.VibratoSpeed:
							{
								// FT2 does not automatically enable vibrato with the "set vibrato speed" command
								if (m_PlayBehaviour[PlayBehaviour.Ft2VolColVibrato])
									chn.nVibratoSpeed = (uint8)(vol & 0x0f);
								else
									Vibrato(chn, (uint32)(vol << 4));

								break;
							}

							case VolumeCommand.VibratoDepth:
							{
								Vibrato(chn, vol);
								break;
							}

							case VolumeCommand.PanSlideLeft:
							{
								PanningSlide(chn, vol, !m_PlayBehaviour[PlayBehaviour.Ft2VolColMemory]);
								break;
							}

							case VolumeCommand.PanSlideRight:
							{
								PanningSlide(chn, (ModCommandParam)(vol << 4), !m_PlayBehaviour[PlayBehaviour.Ft2VolColMemory]);
								break;
							}

							case VolumeCommand.PortaUp:
							{
								// IT compatibility (one of the first testcases - link effect memory)
								PortamentoUp(nChn, (ModCommandParam)(vol << 2), m_PlayBehaviour[PlayBehaviour.ItVolColFinePortamento]);
								break;
							}

							case VolumeCommand.PortaDown:
							{
								// IT compatibility (one of the first testcases - link effect memory)
								PortamentoDown(nChn, (ModCommandParam)(vol << 2), m_PlayBehaviour[PlayBehaviour.ItVolColFinePortamento]);
								break;
							}

							case VolumeCommand.Offset:
							{
								if (triggerNote && (chn.pModSample != null) && !chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib) && (vol <= chn.pModSample.Cues.size()))
								{
									SmpLength offset;

									if (vol == 0)
										offset = chn.OldOffset;
									else
										offset = chn.OldOffset = chn.pModSample.Cues[vol - 1];

									SampleOffset(chn, offset);
								}

								break;
							}

							case VolumeCommand.PlayControl:
							{
								if (chn.IsFirstTick)
									chn.PlayControl(vol);

								break;
							}
						}
					}
				}

				// Effects
				if (cmd != EffectCommand.None)
				{
					switch (cmd)
					{
						// Set volume
						case EffectCommand.Volume:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
							{
								chn.nVolume = (int32)((param < 64) ? param * 4 : 256);
								chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							}

							break;
						}

						case EffectCommand.Volume8:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
							{
								chn.nVolume = (int32)param;
								chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							}

							break;
						}

						// Portamento up
						case EffectCommand.PortamentoUp:
						{
							if ((param != 0) || ((GetType_() & ModType.Mod) == 0))
								PortamentoUp(nChn, (ModCommandParam)param, false);

							break;
						}

						// Portamento down
						case EffectCommand.PortamentoDown:
						{
							if ((param != 0) || ((GetType_() & ModType.Mod) == 0))
								PortamentoDown(nChn, (ModCommandParam)param, false);

							break;
						}

						// Auto portamentos
						case EffectCommand.Auto_PortaUp:
						{
							chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoUp, param != 0);
							chn.nOldPortaUp = (uint8)param;
							break;
						}

						case EffectCommand.Auto_PortaDown:
						{
							chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoDown, param != 0);
							chn.nOldPortaDown = (uint8)param;
							break;
						}

						case EffectCommand.Auto_PortaUp_Fine:
						{
							chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoUp, param != 0);
							chn.nOldFinePortaUpDown = (uint8)param;
							break;
						}

						case EffectCommand.Auto_PortaDown_Fine:
						{
							chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoDown, param != 0);
							chn.nOldFinePortaUpDown = (uint8)param;
							break;
						}

						case EffectCommand.Auto_Portamento_FC:
						{
							chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoFc, param != 0);
							chn.nOldPortaUp = chn.nOldPortaDown = (uint8)param;
							break;
						}

						// Volume slide
						case EffectCommand.VolumeSlide:
						{
							if ((param != 0) || (GetType_() != ModType.Mod))
								VolumeSlide(chn, (ModCommandParam)param);

							break;
						}

						// Tone-Portamento
						case EffectCommand.TonePortamento:
						{
							TonePortamento(nChn, (uint16)param);
							break;
						}

						// Tone-Portamento + Volume Slide
						case EffectCommand.TonePortaVol:
						{
							if ((param != 0) || (GetType_() != ModType.Mod))
							{
								// ST3 compatibility: Do not run combined slides (Kxy / Lxy) on first tick
								// Test cases: NoCombinedSlidesOnFirstTick-Normal.s3m, NoCombinedSlidesOnFirstTick-Fast.s3m
								if (!chn.IsFirstTick || !m_PlayBehaviour[PlayBehaviour.S3MIgnoreCombinedFineSlides])
									VolumeSlide(chn, (ModCommandParam)param);
							}

							TonePortamento(nChn, 0);
							break;
						}

						// Vibrato
						case EffectCommand.Vibrato:
						{
							Vibrato(chn, param);
							break;
						}

						// Vibrato + Volume Slide
						case EffectCommand.VibratoVol:
						{
							if ((param != 0) || (GetType_() != ModType.Mod))
							{
								// ST3 compatibility: Do not run combined slides (Kxy / Lxy) on first tick
								// Test cases: NoCombinedSlidesOnFirstTick-Normal.s3m, NoCombinedSlidesOnFirstTick-Fast.s3m
								if (!chn.IsFirstTick || !m_PlayBehaviour[PlayBehaviour.S3MIgnoreCombinedFineSlides])
									VolumeSlide(chn, (ModCommandParam)param);
							}

							Vibrato(chn, 0);
							break;
						}

						// Set speed
						case EffectCommand.Speed:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
								SetSpeed(m_PlayState, param);

							break;
						}

						// Set tempo
						case EffectCommand.Tempo:
						{
							if (m_PlayBehaviour[PlayBehaviour.ModVBlankTiming])
							{
								// ProTracker MODs with VBlank timing: All Fxx parameters set the tick count
								if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) && (param != 0))
									SetSpeed(m_PlayState, param);
							}
							else
							{
								param = CalculateXParam(m_PlayState.m_nPattern, m_PlayState.m_nRow, nChn, out _);

								if ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0)
								{
									if (param != 0)
										chn.nOldTempo = (ModCommandParam)param;
									else
										param = chn.nOldTempo;
								}

								SetTempo(m_PlayState, new Tempo(param, 0));
							}

							break;
						}

						// Set Offset
						case EffectCommand.Offset:
						{
							if (triggerNote)
							{
								// FT2 compatibility: Portamento + Offset = Ignore offset
								// Test case: porta-offset.xm
								if (bPorta && ((GetType_() & (ModType.Xm | ModType.Dbm)) != 0))
									break;

								ProcessSampleOffset(chn, nChn, m_PlayState);
							}

							break;
						}

						// Disorder Tracker 2 percentage offset
						case EffectCommand.OffsetPercentage:
						{
							if (triggerNote)
								SampleOffset(chn, Util.MulDiv_Unsigned(chn.nLength, param, 256));

							break;
						}

						// Arpeggio
						case EffectCommand.Arpeggio:
						{
							// IT compatibility 01. Don't ignore Arpeggio if no note is playing (also valid for ST3)
							if (m_PlayState.m_nTickCount != 0)
								break;

							if (((chn.nPeriod == 0) || (chn.nNote == 0)) && ((chn.pModInstrument == null) || !chn.pModInstrument.HasValidMidiChannel()) && !m_PlayBehaviour[PlayBehaviour.ItArpeggio] && ((GetType_() & (ModType.It | ModType.Mpt)) != 0))
								break;

							if ((param == 0) && ((GetType_() & (ModType.Xm | ModType.Mod)) != 0))
								break;	// Only important when editing MOD/XM files (000 effects are removed when loading files where this means "no effect")

							chn.nCommand = EffectCommand.Arpeggio;

							if (param != 0)
								chn.nArpeggio = (ModCommandParam)param;

							break;
						}

						// Retrig
						case EffectCommand.Retrig:
						{
							if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
							{
								if ((param & 0xf0) == 0)
									param |= (ModCommandParam)(chn.nRetrigParam & 0xf0);

								if ((param & 0x0f) == 0)
									param |= (ModCommandParam)(chn.nRetrigParam & 0x0f);

								param |= 0x100;	// Increment retrig count on first row
							}

							// IT compatibility 15. Retrigger
							if (m_PlayBehaviour[PlayBehaviour.ItRetrigger])
							{
								if (param != 0)
									chn.nRetrigParam = (uint8)(param & 0xff);

								RetrigNote(nChn, chn.nRetrigParam, (volCmd == VolumeCommand.Offset ? vol + 1 : 0));
							}
							else
							{
								// XM Retrig
								if (param != 0)
									chn.nRetrigParam = (uint8)(param & 0xff);
								else
									param = chn.nRetrigParam;

								RetrigNote(nChn, (c_int)param, (volCmd == VolumeCommand.Offset ? vol + 1 : 0));
							}

							break;
						}

						// Tremor
						case EffectCommand.Tremor:
						{
							if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
								break;

							// IT compatibility 12. / 13. Tremor (using modified DUMB's Tremor logic here because of old effects - http://dumb.sf.net/)
							if (m_PlayBehaviour[PlayBehaviour.ItTremor])
							{
								if ((param != 0) && !m_SongFlags.Test(SongFlags.ItOldEffects))
								{
									// Old effects have different length interpretation (+1 for both on and off)
									if ((param & 0xf0) != 0)
										param -= 0x10;

									if ((param & 0x0f) != 0)
										param -= 0x01;

									chn.nTremorParam = (ModCommandParam)param;
								}

								chn.nTremorCount |= 0x80;	// Set on/off flag
							}
							else if (m_PlayBehaviour[PlayBehaviour.Ft2Tremor])
							{
								// XM Tremor. Logic is being processed in sndmix.cpp
								chn.nTremorCount |= 0x80;	// Set on/off flag
							}

							chn.nCommand = EffectCommand.Tremor;

							if (param != 0)
								chn.nTremorParam = (ModCommandParam)param;

							break;
						}

						// Set Global Volume
						case EffectCommand.GlobalVolume:
						{
							// IT compatibility: Only apply global volume on first tick (and multiples)
							// Test case: GlobalVolFirstTick.it
							if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
								break;

							if ((GetType_() & (GlobalVol_7Bit_Formats)) != 0)
								param *= 2;

							// IT compatibility 16. ST3 and IT ignore out-of-range values.
							// Test case: globalvol-invalid.it
							if (param <= 128)
								m_PlayState.m_nGlobalVolume = (int32)(param * 2);
							else if ((GetType_() & (ModType.It | ModType.Mpt | ModType.S3M)) == 0)
								m_PlayState.m_nGlobalVolume = 256;

							m_PlayState.Chn[m_PlayBehaviour[PlayBehaviour.PerChannelGlobalVolSlide] ? nChn : 0].AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, false);
							break;
						}

						// Global Volume Slide
						case EffectCommand.GlobalVolSlide:
						{
							// IT compatibility 16. Saving last global volume slide param per channel (FT2/IT)
							GlobalVolSlide(m_PlayState, (ModCommandParam)param, (ChannelIndex)(m_PlayBehaviour[PlayBehaviour.PerChannelGlobalVolSlide] ? nChn : 0));
							break;
						}

						// Set 8-bit Panning
						case EffectCommand.Panning8:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
								Panning(chn, param, PanningType.Pan8Bit);

							break;
						}

						// Panning Slide
						case EffectCommand.PanningSlide:
						{
							PanningSlide(chn, (ModCommandParam)param);
							break;
						}

						// Tremolo
						case EffectCommand.Tremolo:
						{
							Tremolo(chn, param);
							break;
						}

						// Fine Vibrato
						case EffectCommand.FineVibrato:
						{
							FineVibrato(chn, param);
							break;
						}

						// MOD/XM Exx Extended Commands
						case EffectCommand.ModCmdEx:
						{
							ExtendedModCommands(nChn, (ModCommandParam)param);
							break;
						}

						// S3M/IT Sxx Extended Commands
						case EffectCommand.S3MCmdEx:
						{
							ExtendedS3MCommands(nChn, (ModCommandParam)param);
							break;
						}

						// Key Off
						case EffectCommand.KeyOff:
						{
							// This is how Key Off is supposed to sound... (in FT2 at least)
							if (m_PlayBehaviour[PlayBehaviour.Ft2KeyOff])
							{
								if (m_PlayState.m_nTickCount == param)
								{
									// XM: Key-Off + Sample == Note Cut
									if ((chn.pModInstrument == null) || !chn.pModInstrument.VolEnv.dwFlags.Test(EnvelopeFlags.Enabled))
									{
										if ((param == 0) && ((chn.RowCommand.Instr != 0) || (chn.RowCommand.VolCmd != VolumeCommand.None)))	// FT2 is weird....
											chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
										else
										{
											chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
											chn.nVolume = 0;
										}
									}

									KeyOff(chn);
								}
							}
							// This is how it's NOT supposed to sound...
							else
							{
								if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
									KeyOff(chn);
							}

							break;
						}

						// Extra-fine porta up/down
						case EffectCommand.XFinePortaUpDown:
						{
							switch (param & 0xf0)
							{
								case 0x10:
								{
									ExtraFinePortamentoUp(chn, (ModCommandParam)(param & 0x0f));

									if (!m_PlayBehaviour[PlayBehaviour.PluginIgnoreTonePortamento])
										MidiPortamento(nChn, (c_int)(0xe0 | (param & 0x0f)), true);

									break;
								}

								case 0x20:
								{
									ExtraFinePortamentoDown(chn, (ModCommandParam)(param & 0x0f));

									if (!m_PlayBehaviour[PlayBehaviour.PluginIgnoreTonePortamento])
										MidiPortamento(nChn, -(c_int)(0xe0 | (param & 0x0f)), true);

									break;
								}

								// ModPlug XM Extensions (ignore in compatible mode)
								case 0x50:
								case 0x60:
								case 0x70:
								case 0x90:
								case 0xa0:
								{
									if (!m_PlayBehaviour[PlayBehaviour.Ft2RestrictXCommand])
										ExtendedS3MCommands(nChn, (ModCommandParam)param);

									break;
								}
							}

							break;
						}

						case EffectCommand.FineTune:
						case EffectCommand.FineTune_Smooth:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) || (cmd == EffectCommand.FineTune_Smooth))
								SetFineTune(m_PlayState.m_nPattern, m_PlayState.m_nRow, nChn, m_PlayState, cmd == EffectCommand.FineTune_Smooth);

							break;
						}

						// Set Channel Global Volume
						case EffectCommand.ChannelVolume:
						{
							if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
								break;

							if (param <= 64)
							{
								chn.nGlobalVol = (uint8)param;
								chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
							}

							break;
						}

						// Channel volume slide
						case EffectCommand.ChannelVolSlide:
						{
							ChannelVolSlide(chn, (ModCommandParam)param);
							break;
						}

						// Panbrello (IT)
						case EffectCommand.Panbrello:
						{
							Panbrello(chn, param);
							break;
						}

						// Set Envelope Position
						case EffectCommand.SetEnvPosition:
						{
							if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
							{
								chn.VolEnv.nEnvPosition = param;

								// FT2 compatibility: FT2 only sets the position of the panning envelope if the volume envelope's sustain flag is set
								// Test case: SetEnvPos.xm
								if (!m_PlayBehaviour[PlayBehaviour.Ft2SetPanEnvPos] || chn.VolEnv.Flags.Test(EnvelopeFlags.Sustain))
								{
									chn.PanEnv.nEnvPosition = param;
									chn.PitchEnv.nEnvPosition = param;
								}
							}

							break;
						}

						// MED Synth Jump (handled in InstrumentSynth) / MIDI Panning
						case EffectCommand.Med_Synth_Jump:
						{
							break;
						}

						// Position Jump
						case EffectCommand.PositionJump:
						{
							PositionJump(m_PlayState, nChn);
							break;
						}

						// Pattern Break
						case EffectCommand.PatternBreak:
						{
							RowIndex row = PatternBreak(m_PlayState, nChn, (ModCommandParam)param);

							if (row != Snd_Def.RowIndex_Invalid)
							{
								m_PlayState.m_BreakRow = row;

								if (m_PlayState.m_Flags.Test(PlayFlags.Song_PatternLoop))
								{
									// If song is set to loop and a pattern break occurs we should stay on the same pattern.
									// Use nPosJump to force playback to "jump to this pattern" rather than move to next, as by default
									m_PlayState.m_PosJump = m_PlayState.m_nCurrentOrder;
								}
							}

							break;
						}

						// IMF / PTM Note Slides
						case EffectCommand.NoteSlideUp:
						case EffectCommand.NoteSlideDown:
						case EffectCommand.NoteSlideUpRetrig:
						case EffectCommand.NoteSlideDownRetrig:
						{
							// Note that this command seems to be a bit buggy in Polytracker... Luckily, no tune seems to seriously use this
							// (Vic uses it e.g. in Spaceman or Perfect Reason to slide effect samples, noone will notice the difference :)
							NoteSlide(chn, param, (cmd == EffectCommand.NoteSlideUp) || (cmd == EffectCommand.NoteSlideUpRetrig), (cmd == EffectCommand.NoteSlideUpRetrig) || (cmd == EffectCommand.NoteSlideDownRetrig));
							break;
						}

						// PTM Reverse sample + offset (executed on every tick)
						case EffectCommand.ReverseOffset:
						{
							ReverseSampleOffset(chn, (ModCommandParam)param);
							break;
						}

						// Digi Booster sample reverse
						case EffectCommand.DigiReverseSample:
						{
							DigiBoosterSampleReverse(chn, (ModCommandParam)param);
							break;
						}

						case EffectCommand.Auto_VolumeSlide:
						{
							AutoVolumeSlide(chn, (ModCommandParam)param);
							break;
						}

						case EffectCommand.VolumeDown_Etx:
						{
							if (chn.IsFirstTick)
								VolumeDownEtx(m_PlayState, chn, (ModCommandParam)param);

							break;
						}

						case EffectCommand.TonePorta_Duration:
						{
							if (chn.RowCommand.IsNote() && triggerNote)
								TonePortamentoWithDuration(chn, (ModCommandParam)param);

							break;
						}

						case EffectCommand.VolumeDown_Duration:
						{
							if (m_PlayState.m_nTickCount == 0)
								ChannelVolumeDownWithDuration(chn, (ModCommandParam)param);

							break;
						}
					}
				}

				if (m_PlayBehaviour[PlayBehaviour.St3EffectMemory] && (cmd != EffectCommand.None) && (param != 0))
					UpdateS3MEffectMemory(chn, (ModCommandParam)param);

				if (chn.RowCommand.Instr != 0)
				{
					// Not necessarily consistent with actually playing instrument for IT compatibility
					chn.nOldIns = chn.RowCommand.Instr;
				}

				ProcessAutoSlides(m_PlayState, nChn);
			}

			// Navigation Effects
			if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
			{
				if (HandleNextRow(m_PlayState, Order.Current, true))
					m_PlayState.m_Flags.Set(PlayFlags.Song_BreakToRow);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool HandleNextRow(PlayState state, ModSequence order, bool honorPatternLoop)//XX 3883
		{
			bool doPatternLoop = state.m_PatLoopRow != Snd_Def.RowIndex_Invalid;
			bool doBreakRow = state.m_BreakRow != Snd_Def.RowIndex_Invalid;
			bool doPosJump = state.m_PosJump != Snd_Def.OrderIndex_Invalid;
			bool breakToRow = false;

			// Pattern Break / Position Jump only if no loop running
			// Exception: FastTracker 2 in all cases, Impulse Tracker in case of position jump
			// Test case for FT2 exception: PatLoop-Jumps.xm, PatLoop-Various.xm
			// Test case for IT: exception: LoopBreak.it, sbx-priority.it
			if ((doBreakRow || doPosJump) && (!doPatternLoop || m_PlayBehaviour[PlayBehaviour.Ft2PatternLoopWithJumps] || (m_PlayBehaviour[PlayBehaviour.ItPatternLoopWithJumps] && doPosJump) || (m_PlayBehaviour[PlayBehaviour.ItPatternLoopWithJumpsOld] && doPosJump)))
			{
				if (!doPosJump)
					state.m_PosJump = (uint16)(state.m_nCurrentOrder + 1);

				if (!doBreakRow)
					state.m_BreakRow = 0;

				breakToRow = true;

				if (state.m_PosJump >= order.size())
					state.m_PosJump = order.GetRestartPos();

				// IT / FT2 compatibility: don't reset loop count on pattern break.
				// Test case: gm-trippy01.it, PatLoop-Break.xm, PatLoop-Weird.xm, PatLoop-Break.mod
				if ((state.m_PosJump != state.m_nCurrentOrder) && !m_PlayBehaviour[PlayBehaviour.ItPatternLoopBreak] && !m_PlayBehaviour[PlayBehaviour.Ft2PatternLoopWithJumps] && (GetType_() != ModType.Mod))
				{
					for (ChannelIndex i = 0; i < GetNumChannels(); i++)
						state.Chn[i].nPatternLoopCount = 0;
				}

				state.m_nNextRow = state.m_BreakRow;

				if (!honorPatternLoop || !m_PlayState.m_Flags.Test(PlayFlags.Song_PatternLoop))
					state.m_nNextOrder = state.m_PosJump;
			}
			else if (doPatternLoop)
			{
				// Pattern Loop
				state.m_nNextOrder = state.m_nCurrentOrder;
				state.m_nNextRow = state.m_PatLoopRow;

				// FT2 skips the first row of the pattern loop if there's a pattern delay, ProTracker sometimes does it too (didn't quite figure it out yet).
				// But IT and ST3 don't do this.
				// Test cases: PatLoopWithDelay.it, PatLoopWithDelay.s3m
				if ((state.m_nPatternDelay != 0) && ((GetType_() != ModType.It) || !m_PlayBehaviour[PlayBehaviour.ItPatternLoopWithJumps]) && (GetType_() != ModType.S3M))
					state.m_nNextRow++;

				// IT Compatibility: If the restart row is past the end of the current pattern
				// (e.g. when continued from a previous pattern without explicit SB0 effect), continue the next pattern.
				// Test case: LoopStartAfterPatternEnd.it
				if (state.m_PatLoopRow >= Patterns[state.m_nPattern].GetNumRows())
				{
					state.m_nNextOrder++;
					state.m_nNextRow = 0;
				}
			}

			return breakToRow;
		}
		#endregion

		//XX 3953
		#region Channel effects
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResetAutoSlides(ModChannel chn)//XX 3956
		{
			EffectCommand cmd = chn.RowCommand.Command;
			VolumeCommand volCmd = chn.RowCommand.VolCmd;

			if ((cmd != EffectCommand.None) && (GetType_() == ModType._669))
			{
				chn.AutoSlide.Reset();
				return;
			}

			if (((cmd == EffectCommand.None) || (chn.RowCommand.Param == 0)) && chn.AutoSlide.IsActive(AutoSlideCommand.VolumeSlideStk))
				chn.AutoSlide.SetActive(AutoSlideCommand.VolumeSlideStk, false);

			if (((cmd == EffectCommand.ChannelVolume) || (cmd == EffectCommand.ChannelVolSlide)) && chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownWithDuration))
				chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownWithDuration, false);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoDown) || chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoDown) || chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoUp) || chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoUp))
			{
				if (!chn.RowCommand.IsTonePortamento() && chn.RowCommand.IsAnyPitchSlide())
				{
					chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoDown, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoDown, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.FinePortamentoUp, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.PortamentoUp, false);
				}
			}

			if (chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideUp) || chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideDown) || chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownEtx))
			{
				if ((cmd == EffectCommand.Volume) || (cmd == EffectCommand.Auto_VolumeSlide) || (cmd == EffectCommand.VolumeDown_Etx) || chn.RowCommand.IsNormalVolumeSlide() ||
				    (volCmd == VolumeCommand.Volume) || (volCmd == VolumeCommand.VolSlideUp) || (volCmd == VolumeCommand.VolSlideDown) || (volCmd == VolumeCommand.FineVolUp) || (volCmd == VolumeCommand.FineVolDown))
				{
					chn.AutoSlide.SetActive(AutoSlideCommand.FineVolumeSlideUp, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.FineVolumeSlideDown, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownEtx, false);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessAutoSlides(PlayState playState, ChannelIndex channel)//XX 3995
		{
			ModChannel chn = playState.Chn[channel];

			if (chn.AutoSlide.IsActive(AutoSlideCommand.TonePortamento) && !chn.RowCommand.IsTonePortamento())
				TonePortamento(channel, chn.PortamentoSlide);
			else if (chn.AutoSlide.IsActive(AutoSlideCommand.TonePortamentoWithDuration))
				TonePortamentoWithDuration(chn);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoUp))
				PortamentoUp(channel, chn.nOldPortaUp, true);
			else if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoDown))
				PortamentoDown(channel, chn.nOldPortaDown, true);
			else if (chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoUp))
				FinePortamentoUp(chn, chn.nOldFinePortaUpDown);
			else if (chn.AutoSlide.IsActive(AutoSlideCommand.FinePortamentoDown))
				FinePortamentoDown(chn, chn.nOldFinePortaUpDown);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.PortamentoFc))
				PortamentoFc(chn);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideUp) && (chn.RowCommand.Command != EffectCommand.Auto_VolumeSlide))
				FineVolumeUp(chn, 0, false);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.FineVolumeSlideDown) && (chn.RowCommand.Command != EffectCommand.Auto_VolumeSlide))
				FineVolumeDown(chn, 0, false);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownEtx))
				chn.nVolume = Math.Max(0, chn.nVolume - chn.nOldVolumeSlide);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.VolumeSlideStk))
				VolumeSlide(chn, 0);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.GlobalVolumeSlide) && (chn.RowCommand.Command != EffectCommand.GlobalVolSlide))
				GlobalVolSlide(playState, chn.nOldGlobalVolSlide, channel);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.VolumeDownWithDuration))
				ChannelVolumeDownWithDuration(chn);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.Vibrato))
				chn.dwFlags.Set(ChannelFlags.Chn_Vibrato);

			if (chn.AutoSlide.IsActive(AutoSlideCommand.Tremolo))
				chn.dwFlags.Set(ChannelFlags.Chn_Tremolo);
		}



		/********************************************************************/
		/// <summary>
		/// Update the effect memory of all S3M effects that use the last
		/// non-zero effect parameter as memory (Dxy, Exx, Fxx, Ixy, Jxy,
		/// Kxy, Lxy, Qxy, Rxy, Sxy)
		/// Test case: ParamMemory.s3m
		/// </summary>
		/********************************************************************/
		public void UpdateS3MEffectMemory(ModChannel chn, ModCommandParam param)//XX 4033
		{
			chn.nOldVolumeSlide = param;	// Dxy / Kxy / Lxy
			chn.nOldPortaUp = param;		// Exx / Fxx
			chn.nOldPortaDown = param;		// Exx / Fxx
			chn.nTremorParam = param;		// Ixy
			chn.nArpeggio = param;			// Jxy
			chn.nRetrigParam = param;		// Qxy
			chn.nTremoloDepth = (uint8)((param & 0x0f) << 2);	// Rxy
			chn.nTremoloSpeed = (uint8)((param >> 4) & 0x0f);	// Rxy
			chn.nOldCmdEx = param;			// Sxy
		}



		/********************************************************************/
		/// <summary>
		/// Calculate full parameter for effects that support parameter
		/// extension at the given pattern location.
		/// maxCommands sets the maximum number of XParam commands to look
		/// at for this effect
		/// extendedRows returns how many extended rows are used (i.e. a
		/// value of 0 means the command is not extended)
		/// </summary>
		/********************************************************************/
		public uint32 CalculateXParam(PatternIndex pat, RowIndex row, ChannelIndex chn, out uint32 extendedRows)//XX 4050
		{
			extendedRows = 0;

			if (!Patterns.IsValidPat(pat))
				return 0;

			RowIndex maxCommands = 4;
			CPointer<ModCommand> mp = Patterns[pat].GetpModCommand(row, chn);
			ModCommand m = mp[0];
			EffectCommand startCmd = m.Command;
			uint32 val = m.Param;

			switch (m.Command)
			{
				case EffectCommand.Offset:
				{
					// 24 bit command
					maxCommands = 2;
					break;
				}

				case EffectCommand.Tempo:
				case EffectCommand.PatternBreak:
				case EffectCommand.PositionJump:
				case EffectCommand.FineTune:
				case EffectCommand.FineTune_Smooth:
				{
					// 16 bit command
					maxCommands = 1;
					break;
				}

				default:
					return val;
			}

			bool xmTempoFix = (m.Command == EffectCommand.Tempo) && (GetType_() == ModType.Xm);
			RowIndex numRows = Math.Min(Patterns[pat].GetNumRows() - row - 1, maxCommands);
			uint32 extRows = 0;

			while (numRows > 0)
			{
				mp += Patterns[pat].GetNumChannels();
				m = mp[0];

				if (m.Command != EffectCommand.XParam)
					break;

				if (xmTempoFix && (val >= 0x20) && (val < 256))
				{
					// With XM, 0x20 is the lowest tempo. Anything below changes ticks per row
					val -= 0x20;
				}

				val = (val << 8) | m.Param;
				numRows--;
				extRows++;
			}

			// Always return a full-precision value for finetune
			if (((startCmd == EffectCommand.FineTune) || (startCmd == EffectCommand.FineTune_Smooth)) && (extRows == 0))
				val <<= 8;

			extendedRows = extRows;

			return val;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PositionJump(PlayState state, ChannelIndex chn)//XX 4116
		{
			state.m_NextPatStartRow = 0;	// FT2 E60 bug
			state.m_PosJump = (OrderIndex)CalculateXParam(state.m_nPattern, state.m_nRow, chn, out _);

			// see https://forum.openmpt.org/index.php?topic=2769.0 - FastTracker resets Dxx if Bxx is called _after_ Dxx
			// Test case: PatternJump.mod
			if (((GetType_() & (ModType.Mod | ModType.Xm)) != 0) && (state.m_BreakRow != Snd_Def.RowIndex_Invalid))
				state.m_BreakRow = 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public RowIndex PatternBreak(PlayState state, ChannelIndex chn, uint8 param)//XX 4130
		{
			if ((param >= 64) && ((GetType_() & ModType.S3M) != 0))
			{
				// ST3 ignores invalid pattern breaks
				return Snd_Def.RowIndex_Invalid;
			}

			state.m_NextPatStartRow = 0;	// FT2 E60 bug

			return (RowIndex)CalculateXParam(state.m_nPattern, state.m_nRow, chn, out _);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoFc(ModChannel chn)//XX 4144
		{
			chn.FcPortaTick = !chn.FcPortaTick;

			if (!chn.FcPortaTick)
				return;

			chn.nPeriod -= (int8)chn.nOldPortaUp * 4;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoUp(ChannelIndex nChn, ModCommandParam param, bool doFinePortamentoAsRegular)//XX 4153
		{
			PortamentoUp(m_PlayState, nChn, param, doFinePortamentoAsRegular);
			MidiPortamento(nChn, m_PlayState.Chn[nChn].nOldPortaUp , !doFinePortamentoAsRegular && UseCombinedPortamentoCommands());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoUp(PlayState playState, ChannelIndex nChn, ModCommandParam param, bool doFinePortamentoAsRegular)//XX 4160
		{
			ModChannel chn = playState.Chn[nChn];

			// IT compatibility: Initialize effect memory in the right order in case there are portamentos in both effect columns.
			// Test cases: DoubleSlide.it, DoubleSlideCompatGxx.it
			if ((param != 0) && !m_PlayBehaviour[PlayBehaviour.ItDoublePortamentoSlides])
			{
				// FT2 compatibility: Separate effect memory for all portamento commands
				// Test case: Porta-LinkMem.xm
				if (!m_PlayBehaviour[PlayBehaviour.Ft2PortaUpDownMemory])
					chn.nOldPortaDown = param;

				chn.nOldPortaUp = param;
			}
			else
				param = chn.nOldPortaUp;

			bool doFineSlides = !doFinePortamentoAsRegular && UseCombinedPortamentoCommands();

			if ((GetType_() == ModType.Mpt) && (chn.pModInstrument != null) && (chn.pModInstrument.pTuning != null))
			{
				// Portamento for instruments with custom tuning
				if ((param >= 0xf0) && !doFinePortamentoAsRegular)
					PortamentoFineMpt(playState, nChn, param - 0xf0);
				else if ((param >= 0xe0) && !doFinePortamentoAsRegular)
					PortamentoExtraFineMpt(chn, param - 0xe0);
				else
					PortamentoMpt(chn, param);

				return;
			}
			else if (GetType_() == ModType.Plm)
			{
				// A normal portamento up or down makes a follow-up tone portamento go the same direction
				chn.nPortamentoDest = 1;
			}

			if (doFineSlides && (param >= 0xe0))
			{
				if ((param & 0x0f) != 0)
				{
					if ((param & 0xf0) == 0xf0)
					{
						FinePortamentoUp(chn, (ModCommandParam)(param & 0x0f));
						return;
					}
					else if (((param & 0xf0) == 0xe0) && (GetType_() != ModType.Dbm))
					{
						ExtraFinePortamentoUp(chn, (ModCommandParam)(param & 0x0f));
						return;
					}
				}

				if (GetType_() != ModType.Dbm)
				{
					// DBM only has fine slides, no extra-fine slides
					return;
				}
			}

			// Regular slide
			if (!chn.IsFirstTick || ((m_PlayState.m_nMusicSpeed == 1) && m_PlayBehaviour[PlayBehaviour.SlidesAtSpeed1]) || m_SongFlags.Test(SongFlags.FastPortas))
				DoFreqSlide(chn, ref chn.nPeriod, param * 4);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoDown(ChannelIndex nChn, ModCommandParam param, bool doFinePortamentoAsRegular)//XX 4226
		{
			PortamentoDown(m_PlayState, nChn, param, doFinePortamentoAsRegular);
			MidiPortamento(nChn, -m_PlayState.Chn[nChn].nOldPortaUp , !doFinePortamentoAsRegular && UseCombinedPortamentoCommands());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoDown(PlayState playState, ChannelIndex nChn, ModCommandParam param, bool doFinePortamentoAsRegular)//XX 4233
		{
			ModChannel chn = playState.Chn[nChn];

			// IT compatibility: Initialize effect memory in the right order in case there are portamentos in both effect columns.
			// Test cases: DoubleSlide.it, DoubleSlideCompatGxx.it
			if ((param != 0) && !m_PlayBehaviour[PlayBehaviour.ItDoublePortamentoSlides])
			{
				// FT2 compatibility: Separate effect memory for all portamento commands
				// Test case: Porta-LinkMem.xm
				if (!m_PlayBehaviour[PlayBehaviour.Ft2PortaUpDownMemory])
					chn.nOldPortaUp = param;

				chn.nOldPortaDown = param;
			}
			else
				param = chn.nOldPortaDown;

			bool doFineSlides = !doFinePortamentoAsRegular && UseCombinedPortamentoCommands();

			if ((GetType_() == ModType.Mpt) && (chn.pModInstrument != null) && (chn.pModInstrument.pTuning != null))
			{
				// Portamento for instruments with custom tuning
				if ((param >= 0xf0) && !doFinePortamentoAsRegular)
					PortamentoFineMpt(playState, nChn, -(param - 0xf0));
				else if ((param >= 0xe0) && !doFinePortamentoAsRegular)
					PortamentoExtraFineMpt(chn, -(param - 0xe0));
				else
					PortamentoMpt(chn, -param);

				return;
			}
			else if (GetType_() == ModType.Plm)
			{
				// A normal portamento up or down makes a follow-up tone portamento go the same direction
				chn.nPortamentoDest = 65535;
			}

			if (doFineSlides && (param >= 0xe0))
			{
				if ((param & 0x0f) != 0)
				{
					if ((param & 0xf0) == 0xf0)
					{
						FinePortamentoDown(chn, (ModCommandParam)(param & 0x0f));
						return;
					}
					else if (((param & 0xf0) == 0xe0) && (GetType_() != ModType.Dbm))
					{
						ExtraFinePortamentoDown(chn, (ModCommandParam)(param & 0x0f));
						return;
					}
				}

				if (GetType_() != ModType.Dbm)
				{
					// DBM only has fine slides, no extra-fine slides
					return;
				}
			}

			if (!chn.IsFirstTick || ((m_PlayState.m_nMusicSpeed == 1) && m_PlayBehaviour[PlayBehaviour.SlidesAtSpeed1]) || m_SongFlags.Test(SongFlags.FastPortas))
				DoFreqSlide(chn, ref chn.nPeriod, param * -4);
		}



		/********************************************************************/
		/// <summary>
		/// Send portamento commands to plugins
		/// </summary>
		/********************************************************************/
		public void MidiPortamento(ChannelIndex nChn, c_int param, bool doFineSlides)//XX 4300
		{
			c_int actualParam = CMath.abs(param);
			c_int pitchBend = 0;

			// Old MIDI Pitch Bends:
			// - Applied on every tick
			// - No fine pitch slides (they are interpreted as normal slides)
			// New MIDI Pitch Bends:
			// - Behaviour identical to sample pitch bends if the instrument's PWD parameter corresponds to the actual VSTi setting
			if (doFineSlides && (actualParam >= 0xe0) && !m_PlayBehaviour[PlayBehaviour.OldMidiPitchBends])
			{
				if (m_PlayState.Chn[nChn].IsFirstTick)
				{
					// Extra fine slide...
					pitchBend = (actualParam & 0x0f) * Numeric.SigNum(param);

					if (actualParam >= 0xf0)
					{
						// ... or just a fine slide!
						pitchBend *= 4;
					}
				}
			}
			else if (!m_PlayState.Chn[nChn].IsFirstTick || m_PlayBehaviour[PlayBehaviour.OldMidiPitchBends])
			{
				// Regular slide
				pitchBend = param * 4;
			}

			if (pitchBend != 0)
			{
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void FinePortamentoUp(ModChannel chn, ModCommandParam param)//XX 4347
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: E1x / E2x / X1x / X2x memory is not linked
				// Test case: Porta-LinkMem.xm
				if (param != 0)
					chn.nOldFinePortaUpDown = (uint8)((chn.nOldFinePortaUpDown & 0x0f) | (param << 4));
				else
					param = (ModCommandParam)(chn.nOldFinePortaUpDown >> 4);
			}
			else if (GetType_() == ModType.Mt2)
			{
				if (param != 0)
					chn.nOldFinePortaUpDown = param;
				else
					param = chn.nOldFinePortaUpDown;
			}

			if (chn.IsFirstTick && (chn.nPeriod != 0) && (param != 0))
				DoFreqSlide(chn, ref chn.nPeriod, param * 4);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void FinePortamentoDown(ModChannel chn, ModCommandParam param)//XX 4365
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: E1x / E2x / X1x / X2x memory is not linked
				// Test case: Porta-LinkMem.xm
				if (param != 0)
					chn.nOldFinePortaUpDown = (uint8)((chn.nOldFinePortaUpDown & 0xf0) | (param & 0x0f));
				else
					param = (ModCommandParam)(chn.nOldFinePortaUpDown & 0x0f);
			}
			else if (GetType_() == ModType.Mt2)
			{
				if (param != 0)
					chn.nOldFinePortaUpDown = param;
				else
					param = chn.nOldFinePortaUpDown;
			}

			if (chn.IsFirstTick && (chn.nPeriod != 0) && (param != 0))
			{
				DoFreqSlide(chn, ref chn.nPeriod, param * -4);

				if ((chn.nPeriod > 0xffff) && !m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] && (!m_SongFlags.Test(SongFlags.LinearSlides) || (GetType_() == ModType.Xm)))
					chn.nPeriod = 0xffff;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ExtraFinePortamentoUp(ModChannel chn, ModCommandParam param)//XX 4387
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: E1x / E2x / X1x / X2x memory is not linked
				// Test case: Porta-LinkMem.xm
				if (param != 0)
					chn.nOldExtraFinePortaUpDown = (uint8)((chn.nOldExtraFinePortaUpDown & 0x0f) | (param << 4));
				else
					param = (ModCommandParam)(chn.nOldExtraFinePortaUpDown >> 4);
			}
			else if (GetType_() == ModType.Mt2)
			{
				if (param != 0)
					chn.nOldFinePortaUpDown = param;
				else
					param = chn.nOldFinePortaUpDown;
			}

			if (chn.IsFirstTick && (chn.nPeriod != 0) && (param != 0))
				DoFreqSlide(chn, ref chn.nPeriod, param);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ExtraFinePortamentoDown(ModChannel chn, ModCommandParam param)//XX 4405
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: E1x / E2x / X1x / X2x memory is not linked
				// Test case: Porta-LinkMem.xm
				if (param != 0)
					chn.nOldExtraFinePortaUpDown = (uint8)((chn.nOldExtraFinePortaUpDown & 0xf0) | (param & 0x0f));
				else
					param = (ModCommandParam)(chn.nOldExtraFinePortaUpDown & 0x0f);
			}
			else if (GetType_() == ModType.Mt2)
			{
				if (param != 0)
					chn.nOldFinePortaUpDown = param;
				else
					param = chn.nOldFinePortaUpDown;
			}

			if (chn.IsFirstTick && (chn.nPeriod != 0) && (param != 0))
			{
				DoFreqSlide(chn, ref chn.nPeriod, -param);

				if ((chn.nPeriod > 0xffff) && !m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] && (!m_SongFlags.Test(SongFlags.LinearSlides) || (GetType_() == ModType.Xm)))
					chn.nPeriod = 0xffff;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void SetFineTune(PatternIndex pattern, RowIndex row, ChannelIndex channel, PlayState playState, bool isSmooth)//XX 4440
		{
			ModChannel chn = playState.Chn[channel];
			int16 newTuning = CalculateFineTuneTarget(pattern, row, channel);

			if (isSmooth)
			{
				int32 ticksLeft = (int32)(playState.TicksOnRow() - playState.m_nTickCount);

				if (ticksLeft > 1)
				{
					int32 step = (newTuning - chn.MicroTuning) / ticksLeft;
					newTuning = int16.CreateSaturating(chn.MicroTuning + step);
				}
			}

			chn.MicroTuning = newTuning;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public int16 CalculateFineTuneTarget(PatternIndex pattern, RowIndex row, ChannelIndex channel)//XX 4463
		{
			return int16.CreateSaturating((int32)CalculateXParam(pattern, row, channel, out _) - 0x8000);
		}



		/********************************************************************/
		/// <summary>
		/// Implemented for IMF / PTM / OKT compatibility, can't actually
		/// save this in any formats
		/// Slide up / down every x ticks by y semitones
		/// Oktalyzer: Slide down on first tick only, or on every tick
		/// </summary>
		/********************************************************************/
		public void NoteSlide(ModChannel chn, uint32 param, bool slideUp, bool retrig)//XX 4472
		{
			if (chn.IsFirstTick)
			{
				if ((param & 0xf0) != 0)
					chn.NoteSlideParam = (uint8)(((uint8)param & 0xf0) | (chn.NoteSlideParam & 0x0f));

				if ((param & 0x0f) != 0)
					chn.NoteSlideParam = (uint8)((chn.NoteSlideParam & 0xf0) | ((uint8)param & 0x0f));

				chn.NoteSlideCounter = (uint8)(chn.NoteSlideParam >> 4);
			}

			bool doTrigger = false;

			if (GetType_() == ModType.Okt)
				doTrigger = ((chn.NoteSlideParam & 0xf0) == 0x10) || m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick);
			else
				doTrigger = !chn.IsFirstTick && (--chn.NoteSlideCounter == 0);

			if (doTrigger)
			{
				uint8 speed = (uint8)(chn.NoteSlideParam >> 4), steps = (uint8)(chn.NoteSlideParam & 0x0f);
				chn.NoteSlideCounter = speed;

				// Update it
				int32 delta = slideUp ? steps : -steps;

				if (chn.HasCustomTuning())
					chn.m_PortamentoFineSteps = (int32)(chn.m_PortamentoFineSteps + (delta * chn.pModInstrument.pTuning.GetFineStepCount()));
				else
					chn.nPeriod = (int32)GetPeriodFromNote((uint32)(delta + GetNoteFromPeriod((uint32)chn.nPeriod, chn.nFineTune, (uint32)chn.nC5Speed)), chn.nFineTune, (uint32)chn.nC5Speed);

				if (retrig)
					chn.Position.Set(0);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public (uint16 first, bool second) GetVolCmdTonePorta(ModCommand m, uint32 startTick)//XX 4506
		{
			if ((GetType_() & (ModType.It | ModType.Mpt | ModType.Ams | ModType.Dmf | ModType.Dbm | ModType.Imf | ModType.Psm | ModType.J2B | ModType.Ult | ModType.Okt | ModType.Mt2 | ModType.Mdl)) != 0)
				return (Tables.ImpulseTrackerPortaVolCmd[m.Vol & 0x0f], false);
			else
			{
				bool clearEffectColumn = false;
				uint16 vol = m.Vol;

				if ((m.Command == EffectCommand.TonePortamento) && (GetType_() == ModType.Xm))
				{
					// Yes, FT2 is *that* weird. If there is a Mx command in the volume column
					// and a normal 3xx command, the 3xx command is ignored but the Mx command's
					// effectiveness is doubled.
					// Test case: TonePortamentoMemory.xm
					clearEffectColumn = true;
					vol *= 2;
				}

				// FT2 compatibility: If there's a portamento and a note delay, execute the portamento, but don't update the parameter
				// Test case: PortaDelay.xm
				if (m_PlayBehaviour[PlayBehaviour.Ft2PortaDelay] && (startTick != 0))
					return (0, clearEffectColumn);
				else
					return ((uint16)(vol * 16), clearEffectColumn);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool TonePortamentoSharesEffectMemory()//XX 4535
		{
			return (!m_SongFlags.Test(SongFlags.ItCompatGxx) && m_PlayBehaviour[PlayBehaviour.ItPortaMemoryShare]) || (GetType_() == ModType.Plm);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void InitTonePortamento(ModChannel chn, uint16 param)//XX 4541
		{
			// IT compatibility 03: Share effect memory with portamento up/down
			if (TonePortamentoSharesEffectMemory())
			{
				if (param == 0)
					param = chn.nOldPortaUp;

				chn.nOldPortaUp = chn.nOldPortaDown = (uint8)param;
			}

			if (param != 0)
				chn.PortamentoSlide = param;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void TonePortamento(ChannelIndex nChn, uint16 param)//XX 4556
		{
			int32 delta = TonePortamento(m_PlayState, nChn, param);

			if (delta == 0)
				return;
		}



		/********************************************************************/
		/// <summary>
		/// Portamento slide
		/// </summary>
		/********************************************************************/
		public int32 TonePortamento(PlayState playState, ChannelIndex nChn, uint16 param)//XX 4577
		{
			ModChannel chn = playState.Chn[nChn];
			chn.dwFlags.Set(ChannelFlags.Chn_Portamento);

			if (m_SongFlags.Test(SongFlags.Auto_TonePorta))
				chn.AutoSlide.SetActive(AutoSlideCommand.TonePortamento, (param != 0) || m_SongFlags.Test(SongFlags.Auto_TonePorta_Cont));

			// IT compatibility: Initialize effect memory in the right order in case there are portamentos in both effect columns.
			// Test cases: DoubleSlide.it, DoubleSlideCompatGxx.it
			if (!m_PlayBehaviour[PlayBehaviour.ItDoublePortamentoSlides])
				InitTonePortamento(chn, param);

			int32 delta = chn.PortamentoSlide;

			if (chn.HasCustomTuning())
			{
				// Behavior: Param tells number of finesteps(or 'fullsteps'(notes) with glissando)
				// to slide per row(not per tick)
				if (delta == 0)
					return 0;

				int32 oldPortamentoTickSlide = playState.m_nTickCount != 0 ? chn.m_PortamentoTickSlide : 0;

				if (chn.nPortamentoDest < 0)
					delta = -delta;

				chn.m_PortamentoTickSlide = (int32)((playState.m_nTickCount + 1.0) * delta / playState.m_nMusicSpeed);

				if (chn.dwFlags.Test(ChannelFlags.Chn_Glissando))
				{
					chn.m_PortamentoTickSlide *= (int32)(chn.pModInstrument.pTuning.GetFineStepCount() + 1);

					// With glissando interpreting param as notes instead of finesteps
				}

				int32 slide = chn.m_PortamentoTickSlide - oldPortamentoTickSlide;

				if (CMath.abs(chn.nPortamentoDest) <= CMath.abs(slide))
				{
					if (chn.nPortamentoDest != 0)
					{
						chn.m_PortamentoFineSteps += chn.nPortamentoDest;
						chn.nPortamentoDest = 0;
						chn.m_CalculateFreq = true;
					}
				}
				else
				{
					chn.m_PortamentoFineSteps += slide;
					chn.nPortamentoDest -= slide;
					chn.m_CalculateFreq = true;
				}

				return 0;
			}

			// ST3: Adlib Note + Tone Portamento does not execute the slide, but changes to the target note instantly on the next row (unless there is another note with tone portamento)
			// Test case: TonePortamentoWithAdlibNote.s3m
			if (m_PlayBehaviour[PlayBehaviour.St3TonePortaWithAdlibNote] && chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && chn.RowCommand.IsNote())
				return 0;

			bool doPorta = !chn.IsFirstTick || (GetType_() == ModType.Dbm) || ((playState.m_nMusicSpeed == 1) && m_PlayBehaviour[PlayBehaviour.SlidesAtSpeed1]) || m_SongFlags.Test(SongFlags.FastPortas);

			if ((GetType_() == ModType.Plm) && (delta >= 0xf0))
			{
				delta -= 0xf0;
				doPorta = chn.IsFirstTick;
			}

			delta *= (GetType_() == ModType._669) ? 2 : 4;

			if ((chn.nPeriod != 0) && (chn.nPortamentoDest != 0) && doPorta)
			{
				int32 actualDelta = PeriodsAreFrequencies() ? delta : -delta;

				// IT compatibility: Command Lxx, with no tone portamento set up before, will always execute the "portamento down" branch.
				// Test cases: LxxWith0Portamento-Linear.it, LxxWith0Portamento-Amiga.it
				if (m_PlayBehaviour[PlayBehaviour.ItDoublePortamentoSlides] && (delta == 0) && (chn.RowCommand.Command == EffectCommand.TonePortaVol))
				{
					if ((chn.nPeriod > 1) && m_SongFlags.Test(SongFlags.LinearSlides))
						chn.nPeriod--;

					if (chn.nPeriod < chn.nPortamentoDest)
						chn.nPeriod = chn.nPortamentoDest;
				}
				else if ((chn.nPeriod < chn.nPortamentoDest) || chn.PortaTargetReached)
				{
					DoFreqSlide(chn, ref chn.nPeriod, actualDelta, true);

					if (chn.nPeriod > chn.nPortamentoDest)
						chn.nPeriod = chn.nPortamentoDest;
				}
				else if (chn.nPeriod > chn.nPortamentoDest)
				{
					DoFreqSlide(chn, ref chn.nPeriod, -actualDelta, true);

					if (chn.nPeriod < chn.nPortamentoDest)
						chn.nPeriod = chn.nPortamentoDest;

					// FT2 compatibility: Reaching portamento target from below forces subsequent portamentos on the same note to use the logic for reaching the note from above instead.
					// Test case: PortaResetDirection.xm
					if ((chn.nPeriod == chn.nPortamentoDest) && m_PlayBehaviour[PlayBehaviour.Ft2PortaResetDirection])
						chn.PortaTargetReached = true;
				}
			}

			// IT compatibility 23. Portamento with no note
			// ProTracker also disables portamento once the target is reached.
			// Test case: PortaTarget.mod
			if ((chn.nPeriod == chn.nPortamentoDest) && (m_PlayBehaviour[PlayBehaviour.ItPortaTargetReached] || (GetType_() == ModType.Mod)))
				chn.nPortamentoDest = 0;

			return doPorta ? delta : 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void TonePortamentoWithDuration(ModChannel chn, uint16 param = uint16.MaxValue)//XX 4685
		{
			if (param != uint16.MaxValue)
			{
				// Prepare portamento
				if (!chn.RowCommand.IsNote())
					return;

				chn.AutoSlide.SetActive(AutoSlideCommand.TonePortamentoWithDuration, param != 0);

				if (param == 0)
				{
					chn.nPeriod = chn.nPortamentoDest;
					return;
				}

				uint32 sourceNote = GetNoteFromPeriod((uint32)chn.nPeriod, chn.nFineTune, (uint32)chn.nC5Speed);
				chn.PortamentoSlide = (uint16)Util.MulDivR_Unsigned((uint32)CMath.abs((c_int)chn.RowCommand.Note - sourceNote), 64, m_PlayState.m_nMusicSpeed * param);
			}
			else if ((chn.nPeriod != 0) && (chn.nPortamentoDest != 0))
			{
				// Run portamento
				chn.dwFlags.Set(ChannelFlags.Chn_Portamento);

				int32 actualDelta = PeriodsAreFrequencies() ? chn.PortamentoSlide : -chn.PortamentoSlide;

				if (chn.nPeriod < chn.nPortamentoDest)
				{
					DoFreqSlide(chn, ref chn.nPeriod, actualDelta, true);

					if (chn.nPeriod >= chn.nPortamentoDest)
					{
						chn.nPeriod = chn.nPortamentoDest;
						chn.nPortamentoDest = 0;
					}
				}
				else if (chn.nPeriod > chn.nPortamentoDest)
				{
					DoFreqSlide(chn, ref chn.nPeriod, -actualDelta, true);

					if (chn.nPeriod <= chn.nPortamentoDest)
					{
						chn.nPeriod = chn.nPortamentoDest;
						chn.nPortamentoDest = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Vibrato(ModChannel chn, uint32 param)//XX 4726
		{
			if ((param & 0x0f) != 0)
				chn.nVibratoDepth = (uint8)((param & 0x0f) * 4);

			if ((param & 0xf0) != 0)
				chn.nVibratoSpeed = (uint8)((param >> 4) & 0x0f);

			if (m_SongFlags.Test(SongFlags.Auto_Vibrato))
				chn.AutoSlide.SetActive(AutoSlideCommand.Vibrato, param != 0);
			else
				chn.dwFlags.Set(ChannelFlags.Chn_Vibrato);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void FineVibrato(ModChannel chn, uint32 param)//XX 4737
		{
			if ((param & 0x0f) != 0)
				chn.nVibratoDepth = (uint8)(param & 0x0f);

			if ((param & 0xf0) != 0)
				chn.nVibratoSpeed = (uint8)((param >> 4) & 0x0f);

			if (m_SongFlags.Test(SongFlags.Auto_Vibrato))
				chn.AutoSlide.SetActive(AutoSlideCommand.Vibrato, param != 0);
			else
				chn.dwFlags.Set(ChannelFlags.Chn_Vibrato);

			// ST3 compatibility: Do not distinguish between vibrato types in effect memory
			// Test case: VibratoTypeChange.s3m
			if (m_PlayBehaviour[PlayBehaviour.St3VibratoMemory] && ((param & 0x0f) != 0))
				chn.nVibratoDepth *= 4;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Panbrello(ModChannel chn, uint32 param)//XX 4754
		{
			if ((param & 0x0f) != 0)
				chn.nPanbrelloDepth = (uint8)(param & 0x0f);

			if ((param & 0xf0) != 0)
				chn.nPanbrelloSpeed = (uint8)((param >> 4) & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Panning(ModChannel chn, uint32 param, PanningType panBits)//XX 4761
		{
			// No panning in ProTracker mode
			if (m_PlayBehaviour[PlayBehaviour.ModIgnorePanning])
				return;

			// IT Compatibility (and other trackers as well): panning disables surround
			// (unless panning in rear channels is enabled, which is not supported by the original trackers anyway)
			if (!m_PlayState.m_Flags.Test(PlayFlags.Song_SurroundPan) && ((panBits == PanningType.Pan8Bit) || m_PlayBehaviour[PlayBehaviour.PanOverride]))
				chn.dwFlags.Reset(ChannelFlags.Chn_Surround);

			if (panBits == PanningType.Pan4Bit)
			{
				// 0...15 panning
				chn.nPan = (int32)(((param * 256) + 8) / 15);
			}
			else if (panBits == PanningType.Pan6Bit)
			{
				// 0...64 panning
				if (param > 64)
					param = 64;

				chn.nPan = (int32)(param * 4);
			}
			else
			{
				if ((GetType_() & (ModType.S3M | ModType.Dsm | ModType.Amf0 | ModType.Amf | ModType.Mtm)) == 0)
				{
					// Real 8-bit panning
					chn.nPan = (int32)param;
				}
				else
				{
					// 7-bit panning + surround
					if (param <= 0x80)
						chn.nPan = (int32)(param << 1);
					else if (param == 0xa4)
					{
						chn.dwFlags.Set(ChannelFlags.Chn_Surround);
						chn.nPan = 0x80;
					}
				}
			}

			chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
			chn.nRestorePanOnNewNote = 0;

			// IT compatibility 20. Set pan overrides random pan
			if (m_PlayBehaviour[PlayBehaviour.PanOverride])
			{
				chn.nPanSwing = 0;
				chn.nPanbrelloOffset = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void AutoVolumeSlide(ModChannel chn, ModCommandParam param)//XX 4813
		{
			if (m_SongFlags.Test(SongFlags.Auto_VolSlide_Stk))
			{
				chn.nOldVolumeSlide = param;
				chn.AutoSlide.SetActive(AutoSlideCommand.VolumeSlideStk);
			}
			else
			{
				if ((param & 0x0f) != 0)
				{
					FineVolumeDown(chn, param, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.FineVolumeSlideDown);
				}
				else
				{
					FineVolumeUp(chn, param, false);
					chn.AutoSlide.SetActive(AutoSlideCommand.FineVolumeSlideUp);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void VolumeDownEtx(PlayState playState, ModChannel chn, ModCommandParam param)//XX 4834
		{
			chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownEtx, param != 0);

			if ((param == 0) || (playState.m_nSamplesPerTick == 0))
				return;

			uint32 slideDuration = Util.MulDivR_Unsigned(m_MixerSettings.gdwMixingFreq, 600, 1000) / param;	// 600ms at maximum volume
			uint32 neededTicks = Math.Max(1, (slideDuration + (playState.m_nSamplesPerTick / 2U)) / playState.m_nSamplesPerTick);
			chn.nOldVolumeSlide = uint8.CreateSaturating(256 / neededTicks);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void VolumeSlide(ModChannel chn, ModCommandParam param, bool volCol = false)//XX 4845
		{
			if (!volCol)
			{
				if (param != 0)
					chn.nOldVolumeSlide = param;
				else
					param = chn.nOldVolumeSlide;
			}

			if ((GetType_() & (ModType.Mod | ModType.Xm | ModType.Mt2 | ModType.Med | ModType.Digi | ModType.Stp | ModType.Dtm)) != 0)
			{
				// MOD / XM nibble priority
				if ((param & 0xf0) != 0)
					param &= 0xf0;
				else
					param &= 0x0f;
			}

			c_int newVolume = chn.nVolume;

			if ((GetType_() & (ModType.Mod | ModType.Xm | ModType.Amf0 | ModType.Med | ModType.Digi)) == 0)
			{
				if ((param & 0x0f) == 0x0f)	// Fine upslide or slide -15
				{
					if ((param & 0xf0) != 0)	// Fine upslide
					{
						FineVolumeUp(chn, (ModCommandParam)(param >> 4), false);
						return;
					}
					else	// Slide -15
					{
						if (chn.IsFirstTick && !m_SongFlags.Test(SongFlags.FastVolSlides))
							newVolume -= 0x0f * 4;
					}
				}
				else
				{
					if ((param & 0xf0) == 0xf0)	// Fine downslide or slide +15
					{
						if ((param & 0x0f) != 0)	// Fine downslide
						{
							FineVolumeDown(chn, (ModCommandParam)(param & 0x0f), false);
							return;
						}
						else	// Slide +15
						{
							if (chn.IsFirstTick && !m_SongFlags.Test(SongFlags.FastVolSlides))
								newVolume += 0x0f * 4;
						}
					}
				}
			}

			if (!chn.IsFirstTick || m_SongFlags.Test(SongFlags.FastVolSlides) || ((m_PlayState.m_nMusicSpeed == 1) && (GetType_() == ModType.Dbm)))
			{
				// IT compatibility: Ignore slide commands with both nibbles set
				if ((param & 0x0f) != 0)
				{
					if (((GetType_() & (ModType.It | ModType.Mpt)) == 0) || ((param & 0xf0) == 0))
						newVolume -= (param & 0x0f) * 4;
				}
				else
					newVolume += (param & 0xf0) >> 2;

				if (GetType_() == ModType.Mod)
					chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
			}

			newVolume = OpenMpt.Clamp(newVolume, 0, 256);

			chn.nVolume = newVolume;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PanningSlide(ModChannel chn, ModCommandParam param, bool memory = true)//XX 4919
		{
			if (memory)
			{
				// FT2 compatibility: Use effect memory (lxx and rxx in XM shouldn't use effect memory).
				// Test case: PanSlideMem.xm
				if (param != 0)
					chn.nOldPanSlide = param;
				else
					param = chn.nOldPanSlide;
			}

			if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
			{
				// XM nibble priority
				if ((param & 0xf0) != 0)
					param &= 0xf0;
				else
					param &= 0x0f;
			}

			int32 nPanSlide = 0;

			if ((GetType_() & (ModType.Xm | ModType.Mt2)) == 0)
			{
				if (((param & 0x0f) == 0x0f) && ((param & 0xf0) != 0))
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						param = (ModCommandParam)((param & 0xf0) / 4U);
						nPanSlide = -(c_int)param;
					}
				}
				else if (((param & 0xf0) == 0xf0) && ((param & 0x0f) != 0))
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						nPanSlide = (c_int)((param & 0x0f) * 4U);
				}
				else if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
				{
					if ((param & 0x0f) != 0)
					{
						// IT compatibility: Ignore slide commands with both nibbles set
						if (((GetType_() & (ModType.It | ModType.Mpt)) == 0) || ((param & 0xf0) == 0))
							nPanSlide = (c_int)((param & 0x0f) * 4U);
					}
					else
						nPanSlide = -(c_int)((param & 0xf0) / 4U);
				}
			}
			else
			{
				if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
				{
					if ((param & 0xf0) != 0)
						nPanSlide = (c_int)((param & 0xf0) / 4U);
					else
						nPanSlide = (c_int)((param & 0x0f) * 4U);

					// FT2 compatibility: FT2's panning slide is like IT's fine panning slide (not as deep)
					if (m_PlayBehaviour[PlayBehaviour.Ft2PanSlide])
						nPanSlide /= 4;
				}
			}

			if (nPanSlide != 0)
			{
				nPanSlide += chn.nPan;
				nPanSlide = OpenMpt.Clamp(nPanSlide, 0, 256);
				chn.nPan = nPanSlide;
				chn.nRestorePanOnNewNote = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void FineVolumeUp(ModChannel chn, ModCommandParam param, bool volCol)//XX 4998
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: EAx / EBx memory is not linked
				// Test case: FineVol-LinkMem.xm
				if (param != 0)
					chn.nOldFineVolUpDown = (uint8)((param << 4) | (chn.nOldFineVolUpDown & 0x0f));
				else
					param = (ModCommandParam)(chn.nOldFineVolUpDown >> 4);
			}
			else if (volCol)
			{
				if (param != 0)
					chn.nOldVolParam = param;
				else
					param = chn.nOldVolParam;
			}
			else
			{
				if (param != 0)
					chn.nOldFineVolUpDown = param;
				else
					param = chn.nOldFineVolUpDown;
			}

			if (chn.IsFirstTick)
			{
				chn.nVolume += param * 4;

				if (chn.nVolume > 256)
					chn.nVolume = 256;

				if ((GetType_() & ModType.Mod) != 0)
					chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void FineVolumeDown(ModChannel chn, ModCommandParam param, bool volCol)//XX 5022
		{
			if (GetType_() == ModType.Xm)
			{
				// FT2 compatibility: EAx / EBx memory is not linked
				// Test case: FineVol-LinkMem.xm
				if (param != 0)
					chn.nOldFineVolUpDown = (uint8)(param | (chn.nOldFineVolUpDown & 0xf0));
				else
					param = (ModCommandParam)(chn.nOldFineVolUpDown & 0x0f);
			}
			else if (volCol)
			{
				if (param != 0)
					chn.nOldVolParam = param;
				else
					param = chn.nOldVolParam;
			}
			else
			{
				if (param != 0)
					chn.nOldFineVolUpDown = param;
				else
					param = chn.nOldFineVolUpDown;
			}

			if (chn.IsFirstTick)
			{
				chn.nVolume -= param * 4;

				if (chn.nVolume < 0)
					chn.nVolume = 0;

				if ((GetType_() & ModType.Mod) != 0)
					chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Tremolo(ModChannel chn, uint32 param)//XX 5046
		{
			if ((param & 0x0f) != 0)
				chn.nTremoloDepth = (uint8)((param & 0x0f) << 2);

			if ((param & 0xf0) != 0)
				chn.nTremoloSpeed = (uint8)((param >> 4) & 0x0f);

			if (m_SongFlags.Test(SongFlags.Auto_Tremolo))
				chn.AutoSlide.SetActive(AutoSlideCommand.Tremolo, (param & 0x0f) != 0);
			else
				chn.dwFlags.Set(ChannelFlags.Chn_Tremolo);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ChannelVolSlide(ModChannel chn, ModCommandParam param)//XX 5057
		{
			int32 nChnSlide = 0;

			if (param != 0)
				chn.nOldChnVolSlide = param;
			else
				param = chn.nOldChnVolSlide;

			if (((param & 0x0f) == 0x0f) && ((param & 0xf0) != 0))
			{
				if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					nChnSlide = param >> 4;
			}
			else if (((param & 0xf0) == 0xf0) && ((param & 0x0f) != 0))
			{
				if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					nChnSlide = -(c_int)(param & 0x0f);
			}
			else
			{
				if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
				{
					if ((param & 0x0f) != 0)
					{
						if (((GetType_() & (ModType.It | ModType.Mpt | ModType.J2B | ModType.Dbm)) == 0) || ((param & 0xf0) == 0))
							nChnSlide = -(c_int)(param & 0x0f);
					}
					else
						nChnSlide = (c_int)((param & 0xf0) >> 4);
				}
			}

			if (nChnSlide != 0)
			{
				nChnSlide += chn.nGlobalVol;
				OpenMpt.Limit(ref nChnSlide, 0, 64);
				chn.nGlobalVol = (uint8)nChnSlide;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ChannelVolumeDownWithDuration(ModChannel chn, uint16 param = uint16.MaxValue)//XX 5091
		{
			if (param != uint16.MaxValue)
			{
				// Prepare slide
				chn.AutoSlide.SetActive(AutoSlideCommand.VolumeDownWithDuration, param != 0);

				if (param == 0)
				{
					chn.nGlobalVol = 0;
					return;
				}

				chn.VolSlideDownStart = chn.nGlobalVol;
				chn.VolSlideDownTotal = chn.VolSlideDownRemain = uint16.CreateSaturating(param * m_PlayState.m_nMusicSpeed);
			}
			else if (chn.VolSlideDownTotal != 0)
			{
				// Run slide
				if (chn.VolSlideDownRemain != 0)
					chn.nGlobalVol = (uint8)(Util.MulDivR(chn.VolSlideDownStart, --chn.VolSlideDownRemain, chn.VolSlideDownTotal));
				else
					chn.nGlobalVol = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ExtendedModCommands(ChannelIndex nChn, ModCommandParam param)//XX 5115
		{
			ModChannel chn = m_PlayState.Chn[nChn];
			uint8 command = (uint8)(param & 0xf0);
			param &= 0x0f;

			switch (command)
			{
				// E0x: Set Filter
				case 0x00:
				{
					for (ChannelIndex channel = 0; channel < GetNumChannels(); channel++)
						m_PlayState.Chn[channel].dwFlags.Set(ChannelFlags.Chn_AmigaFilter, (param & 1) == 0);

					break;
				}

				// E1x: Fine Portamento Up
				case 0x10:
				{
					if ((param != 0) || ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0))
					{
						FinePortamentoUp(chn, param);

						if (!m_PlayBehaviour[PlayBehaviour.PluginIgnoreTonePortamento])
							MidiPortamento(nChn, 0xf0 | param, true);
					}

					break;
				}

				// E2x: Fine Portamento Down
				case 0x20:
				{
					if ((param != 0) || ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0))
					{
						FinePortamentoDown(chn, param);

						if (!m_PlayBehaviour[PlayBehaviour.PluginIgnoreTonePortamento])
							MidiPortamento(nChn, -(0xf0 | param), true);
					}

					break;
				}

				// E3x: Set Glissando Control
				case 0x30:
				{
					chn.dwFlags.Set(ChannelFlags.Chn_Glissando, param != 0);
					break;
				}

				// E4x: Set Vibrato WaveForm
				case 0x40:
				{
					chn.nVibratoType = (uint8)(param & 0x07);
					break;
				}

				// E5x: Set FineTune
				case 0x50:
				{
					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						break;

					if ((GetType_() & (ModType.Mod | ModType.Digi | ModType.Amf0 | ModType.Med)) != 0)
					{
						chn.nFineTune = Mod2XmFineTune(param);

						if ((chn.nPeriod != 0) && chn.RowCommand.IsNote())
							chn.nPeriod = (int32)GetPeriodFromNote(chn.nNote, chn.nFineTune, (uint32)chn.nC5Speed);
					}
					else if (GetType_() == ModType.Mtm)
					{
						if (chn.RowCommand.IsNote() && (chn.pModSample != null))
						{
							// Effect is permanent in MultiTracker
							chn.pModSample.nFineTune = (int8)param;
							chn.nFineTune = param;

							if (chn.nPeriod != 0)
								chn.nPeriod = (int32)GetPeriodFromNote(chn.nNote, chn.nFineTune, (uint32)chn.nC5Speed);
						}
					}
					else if (chn.RowCommand.IsNote())
					{
						chn.nFineTune = Mod2XmFineTune(param - 8);

						if (chn.nPeriod != 0)
							chn.nPeriod = (int32)GetPeriodFromNote(chn.nNote, chn.nFineTune, (uint32)chn.nC5Speed);
					}

					break;
				}

				// E6x: Pattern Loop
				case 0x60:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						PatternLoop(m_PlayState, nChn, (ModCommandParam)(param & 0x0f));

					break;
				}

				// E7x: Set Tremolo WaveForm
				case 0x70:
				{
					chn.nTremoloType = (uint8)(param & 0x07);
					break;
				}

				// E8x: Set 4-bit Panning
				case 0x80:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						Panning(chn, param, PanningType.Pan4Bit);

					break;
				}

				// E9x: Retrig
				case 0x90:
				{
					RetrigNote(nChn, param);
					break;
				}

				// EAx: Fine Volume Up
				case 0xa0:
				{
					if ((param != 0) || ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0))
						FineVolumeUp(chn, param, false);

					break;
				}

				// EBx: Fine Volume Down
				case 0xb0:
				{
					if ((param != 0) || ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0))
						FineVolumeDown(chn, param, false);

					break;
				}

				// ECx: Note Cut
				case 0xc0:
				{
					NoteCut(nChn, param, false);
					break;
				}

				// EDx: Note Delay
				// EEx: Pattern Delay
				case 0xf0:
				{
					if (GetType_() == ModType.Mod)	// MOD: Invert Loop
					{
						chn.nEFxSpeed = param;

						if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
							InvertLoop(chn);
					}
					else	// XM: Set Active Midi Macro
					{
						chn.nActiveMacro = param;
					}

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ExtendedS3MCommands(ChannelIndex nChn, ModCommandParam param)//XX 5211
		{
			ModChannel chn = m_PlayState.Chn[nChn];
			uint8 command = (uint8)(param & 0xf0);
			param &= 0x0f;

			switch (command)
			{
				// S0x: Set Filter
				// S1x: Set Glissando Control
				case 0x10:
				{
					chn.dwFlags.Set(ChannelFlags.Chn_Glissando, param != 0);
					break;
				}

				// S2x: Set FineTune
				case 0x20:
				{
					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						break;

					if (chn.HasCustomTuning())
					{
						chn.nFineTune = (int16)(param - 8);
						chn.m_CalculateFreq = true;
					}
					else if (GetType_() != ModType._669)
					{
						chn.nC5Speed = Tables.S3MFineTuneTable[param];
						chn.nFineTune = Mod2XmFineTune(param);

						if (chn.nPeriod != 0)
							chn.nPeriod = (int32)GetPeriodFromNote(chn.nNote, chn.nFineTune, (uint32)chn.nC5Speed);
					}
					else if (chn.pModSample != null)
						chn.nC5Speed = (int32)(chn.pModSample.nC5Speed + (param * 80));

					break;
				}

				// S3x: Set Vibrato Waveform
				case 0x30:
				{
					if (GetType_() == ModType.S3M)
						chn.nVibratoType = (uint8)(param & 0x03);
					else
					{
						// IT compatibility: Ignore waveform types > 3
						if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
							chn.nVibratoType = (uint8)((param < 0x04) ? param : 0);
						else
							chn.nVibratoType = (uint8)(param & 0x07);
					}

					break;
				}

				// S4x: Set Tremolo Waveform
				case 0x40:
				{
					if (GetType_() == ModType.S3M)
						chn.nTremoloType = (uint8)(param & 0x03);
					else
					{
						// IT compatibility: Ignore waveform types > 3
						if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
							chn.nTremoloType = (uint8)((param < 0x04) ? param : 0);
						else
							chn.nTremoloType = (uint8)(param & 0x07);
					}

					break;
				}

				// S5x: Set Panbrello Waveform
				case 0x50:
				{
					// IT compatibility: Ignore waveform types > 3
					if (m_PlayBehaviour[PlayBehaviour.ItVibratoTremoloPanbrello])
					{
						chn.nPanbrelloType = (uint8)((param < 0x04) ? param : 0);
						chn.nPanbrelloPos = 0;
					}
					else
						chn.nPanbrelloType = (uint8)(param & 0x07);

					break;
				}

				// S6x: Pattern Delay for x frames
				case 0x60:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) && (m_PlayState.m_nTickCount == 0))
					{
						// Tick delays are added up.
						// Scream Tracker 3 does actually not support this command.
						// We'll use the same behaviour as for Impulse Tracker, as we can assume that
						// most S3Ms that make use of this command were made with Impulse Tracker.
						// MPT added this command to the XM format through the X6x effect, so we will use
						// the same behaviour here as well.
						// Test cases: PatternDelays.it, PatternDelays.s3m, PatternDelays.xm
						m_PlayState.m_nFrameDelay += param;
					}

					break;
				}

				// S7x: Envelope Control / Instrument Control
				case 0x70:
				{
					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						break;

					switch (param)
					{
						case 0:
						case 1:
						case 2:
						{
							for (ChannelIndex i = GetNumChannels(); i < m_PlayState.Chn.size(); i++)
							{
								ModChannel bkChn = m_PlayState.Chn[i];

								if (bkChn.nMasterChn == (nChn + 1))
								{
									if (param == 1)
									{
										KeyOff(bkChn);

//XX										if (bkChn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//											m_Opl.NoteOff(i);
									}
									else if (param == 2)
									{
										bkChn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

//XX										if (bkChn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//											m_Opl.NoteOff(i);
									}
									else
									{
										bkChn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
										bkChn.nFadeOutVol = 0;

//XX										if (bkChn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//											m_Opl.NoteCut(i);
									}
								}
							}

							break;
						}

						// S73-S7E
						default:
						{
							chn.InstrumentControl(param, this);
							break;
						}
					}

					break;
				}

				// S8x: Set 4-bit Panning
				case 0x80:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						Panning(chn, param, PanningType.Pan4Bit);

					break;
				}

				// S9x: Sound Control
				case 0x90:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						ExtendedChannelEffect(chn, param, m_PlayState);

					break;
				}

				// SAx: Set 64k Offset
				case 0xa0:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						chn.nOldHiOffset = param;

						if (!m_PlayBehaviour[PlayBehaviour.ItHighOffsetNoRetrig] && chn.RowCommand.IsNote())
						{
							SmpLength pos = (SmpLength)(param << 16);

							if (pos < chn.nLength)
								chn.Position.SetInt((int32)pos);
						}
					}

					break;
				}

				// SBx: Pattern Loop
				case 0xb0:
				{
					if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
						PatternLoop(m_PlayState, nChn, (ModCommandParam)(param & 0x0f));

					break;
				}

				// SCx: Note Cut
				case 0xc0:
				{
					if (param == 0)
					{
						// IT compatibility 22. SC0 == SC1
						if ((GetType_() & (ModType.It | ModType.Mpt)) != 0)
							param = 1;
						// ST3 doesn't cut notes with SC0
						else if (GetType_() == ModType.S3M)
							return;
					}

					// S3M/IT compatibility: Note Cut really cuts notes and does not just mute them (so that following volume commands could restore the sample)
					// Test case: scx.it
					NoteCut(nChn, param, m_PlayBehaviour[PlayBehaviour.ItSCxStopsSample] || (GetType_() == ModType.S3M));
					break;
				}

				// SDx: Note Delay
				// SEx: Pattern Delay for x rows
				// SFx: S3M: Not used, IT: Set Active Midi Macro
				case 0xf0:
				{
					if (GetType_() != ModType.S3M)
						chn.nActiveMacro = param;

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ExtendedChannelEffect(ModChannel chn, uint32 param, PlayState playState)//XX 5396
		{
			// S9x and X9x commands (S3M/XM/IT only)
			switch (param & 0x0f)
			{
				// S90: Surround off
				case 0x00:
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_Surround);
					break;
				}

				// S91: Surround on
				case 0x01:
				{
					chn.dwFlags.Set(ChannelFlags.Chn_Surround);
					chn.nPan = 128;
					break;
				}

				////////////////////////////////////////////////////////////
				// ModPlug Extensions
				// S98: Reverb Off
				case 0x08:
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_Reverb);
					chn.dwFlags.Set(ChannelFlags.Chn_NoReverb);
					break;
				}

				// S99: Reverb On
				case 0x09:
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_NoReverb);
					chn.dwFlags.Set(ChannelFlags.Chn_Reverb);
					break;
				}

				// S9A: 2-Channels surround mode
				case 0x0a:
				{
					playState.m_Flags.Reset(PlayFlags.Song_SurroundPan);
					break;
				}

				// S9B: 4-Channels surround mode
				case 0x0b:
				{
					playState.m_Flags.Set(PlayFlags.Song_SurroundPan);
					break;
				}

				// S9C: IT Filter Mode
				case 0x0c:
				{
					playState.m_Flags.Reset(PlayFlags.Song_MptFilterMode);
					break;
				}

				// S9D: MPT Filter Mode
				case 0x0d:
				{
					playState.m_Flags.Set(PlayFlags.Song_MptFilterMode);
					break;
				}

				// S9E: Go forward
				case 0x0e:
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);
					break;
				}

				// S9F: Go backward (and set playback position to the end if sample just started)
				case 0x0f:
				{
					if (chn.Position.IsZero() && (chn.nLength != 0) && (chn.RowCommand.IsNote() || !chn.dwFlags.Test(ChannelFlags.Chn_Loop)))
						chn.Position.Set((int32)(chn.nLength - 1), SamplePosition.FractMax);

					chn.dwFlags.Set(ChannelFlags.Chn_PingPongFlag);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void InvertLoop(ModChannel chn)//XX 5450
		{
			// EFx implementation for MOD files (PT 1.1A and up: Invert Loop)
			// This effect trashes samples. Thanks to 8bitbubsy for making this work. :)
			if ((GetType_() != ModType.Mod) || (chn.nEFxSpeed == 0))
				return;

			ModSample pModSample = chn.pModSample;

			if ((pModSample == null) || !pModSample.HasSampleData() || !pModSample.uFlags.Test(ChannelFlags.Chn_Loop | ChannelFlags.Chn_SustainLoop))
				return;

			chn.nEFxDelay += Tables.ModEFxTable[chn.nEFxSpeed & 0x0f];

			if (chn.nEFxDelay < 128)
				return;

			chn.nEFxDelay = 0;

			SmpLength loopStart = pModSample.uFlags.Test(ChannelFlags.Chn_Loop) ? pModSample.nLoopStart : pModSample.nSustainStart;
			SmpLength loopEnd = pModSample.uFlags.Test(ChannelFlags.Chn_Loop) ? pModSample.nLoopEnd : pModSample.nSustainEnd;

			if (++chn.nEFxOffset >= (loopEnd - loopStart))
				chn.nEFxOffset = 0;

			// TRASH IT!!! (Yes, the sample!)
			uint8 bps = pModSample.GetBytesPerSample();
			CPointer<uint8> begin = pModSample.SampleB() + ((loopStart + chn.nEFxOffset) * bps);

			for (c_int i = 0; i < bps; i++)
				begin[i] = (uint8)~begin[i];

			pModSample.PrecomputeLoops(this, false);
		}



		/********************************************************************/
		/// <summary>
		/// Process a MIDI Macro.
		/// Parameters:
		/// playState: The playback state to operate on.
		/// nChn: Mod channel to apply macro on
		/// isSmooth: If true, internal macros are interpolated between two
		/// rows
		/// macro: MIDI Macro string to process
		/// param: Parameter for parametric macros (Zxx / \xx parameter)
		/// plugin: Plugin to send MIDI message to (if not specified but
		/// needed, it is autodetected)
		/// </summary>
		/********************************************************************/
		public void ProcessMidiMacro(PlayState playState, ChannelIndex nChn, bool isSmooth, MidiMacroConfigData.Macro macro, uint8 param = 0, PlugIndex plugin = 0)//XX 5491
		{
			playState.m_MidiMacroScratchSpace.resize(macro.Length() + 1);

			MidiMacroParser parser = new MidiMacroParser(this, playState, nChn, isSmooth, macro, new MptSpan<uint8>(playState.m_MidiMacroScratchSpace), param, plugin);
			MptSpan<uint8> midiMsg;

			while (parser.NextMessage(out midiMsg))
			{
				SendMidiData(playState, nChn, isSmooth, midiMsg, plugin);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate smooth MIDI macro slide parameter for current tick
		/// </summary>
		/********************************************************************/
		public static c_float CalculateSmoothParamChange(PlayState playState, c_float currentValue, c_float param)//XX 5504
		{
			uint32 ticksLeft = playState.TicksOnRow() - playState.m_nTickCount;

			if (ticksLeft > 1)
			{
				// Slide param
				c_float step = (param - currentValue) / ticksLeft;

				return currentValue + step;
			}
			else
			{
				// On last tick, set exact value
				return param;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process exactly one MIDI message parsed by ProcessMIDIMacro.
		/// Returns bytes sent on success, 0 on (parse) failure
		/// </summary>
		/********************************************************************/
		public void SendMidiData(PlayState playState, ChannelIndex nChn, bool isSmooth, MptSpan<uint8> macro, PlugIndex plugin)//XX 5522
		{
			if (macro.Size() < 1)
				return;

			// Don't do anything that modifies state outside of the playState itself
			bool localOnly = playState.m_MidiMacroEvaluationResults != null;

			if ((macro[0] == 0xfa) || (macro[0] == 0xfc) || (macro[0] == 0xff))
			{
				// Start Song, Stop Song, MIDI Reset - both interpreted internally and sent to plugins
				for (ChannelIndex chn_ = 0; chn_ < GetNumChannels(); chn_++)
				{
					playState.Chn[chn_].nCutOff = 0x7f;
					playState.Chn[chn_].nResonance = 0x00;
				}
			}

			ModChannel chn = playState.Chn[nChn];

			if ((macro.Size() == 4) && (macro[0] == 0xf0) && ((macro[1] == 0xf0) || (macro[1] == 0xf1)))
			{
				// Internal device
				bool isExtended = macro[1] == 0xf1;
				uint8 macroCode = macro[2];
				uint8 param = macro[3];

				if ((macroCode == 0x00) && !isExtended && (param < 0x80))
				{
					// F0.F0.00.xx: Set CutOff
					if (!isSmooth)
						chn.nCutOff = param;
					else
						chn.nCutOff = uint8.CreateSaturating(CalculateSmoothParamChange(playState, chn.nCutOff, param));

					chn.nRestoreCutOffOnNewNote = 0;
					c_int cutOff = SetupChannelFilter(chn, !chn.dwFlags.Test(ChannelFlags.Chn_Filter));

/*					if ((cutOff >= 0) && chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null) && !localOnly)
					{
						// Cutoff doubles as modulator intensity for FM instruments
						m_Opl.Volume(nChn, (uint8)(cutOff / 4), true);
					}*///XX
				}
				else if ((macroCode == 0x01) && !isExtended && (param < 0x80))
				{
					// F0.F0.01.xx: Set Resonance
					if (!isSmooth)
						chn.nResonance = param;
					else
						chn.nResonance = uint8.CreateSaturating(CalculateSmoothParamChange(playState, chn.nResonance, param));

					chn.nRestoreResonanceOnNewNote = 0;
					SetupChannelFilter(chn, !chn.dwFlags.Test(ChannelFlags.Chn_Filter));
				}
				else if ((macroCode == 0x02) && !isExtended)
				{
					// F0.F0.02.xx: Set filter mode (high nibble determines filter mode)
					if (param < 0x20)
					{
						chn.nFilterMode = (FilterMode)(param >> 4);
						SetupChannelFilter(chn, !chn.dwFlags.Test(ChannelFlags.Chn_Filter));
					}
				}
			}
			else if (!localOnly)
			{
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void SendMidiNote(ChannelIndex chn, uint16 note, uint16 volume, IMixPlugin plugin = null)//XX 5648
		{
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ProcessSampleOffset(ModChannel chn, ChannelIndex nChn, PlayState playState)//XX 5675
		{
			ModCommand m = chn.RowCommand;
			uint32 extendedRows = 0;
			SmpLength offset = CalculateXParam(playState.m_nPattern, playState.m_nRow, nChn, out extendedRows), highOffset = 0;

			if (extendedRows == 0)
			{
				// No X-param (normal behaviour)
				bool isPercentageOffset = (m.VolCmd == VolumeCommand.Offset) && (m.Vol == 0);
				offset <<= 8;

				// FT2 compatibility: 9xx command without a note next to it does not update effect memory.
				// Test case: OffsetWithoutNote.xm
				if ((offset != 0) && (!m_PlayBehaviour[PlayBehaviour.Ft2OffsetMemoryRequiresNote] || m.IsNote()))
					chn.OldOffset = offset;
				else if (m.VolCmd != VolumeCommand.Offset)
					offset = chn.OldOffset;

				if (!isPercentageOffset)
					highOffset = (SmpLength)chn.nOldHiOffset << 16;
			}

			if (m.VolCmd == VolumeCommand.Offset)
			{
				if (m.Vol == 0)
					offset = Util.MulDivR_Unsigned(chn.nLength, offset, 256U << (c_int)(8 * Math.Max(1, extendedRows)));	// o00 + Oxx = Percentage Offset
				else if ((m.Vol <= new ModSample().Cues.size()) && (chn.pModSample != null) && !chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib))
					offset += chn.pModSample.Cues[m.Vol - 1];	// Offset relative to cue point

				chn.OldOffset = offset;
			}

			SampleOffset(chn, offset + highOffset);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void SampleOffset(ModChannel chn, SmpLength param)//XX 5707
		{
			OpenMpt.LimitMax(ref param, Snd_Def.Max_Sample_Length);

			// ST3 compatibility: Instrument-less note recalls previous note's offset
			// Test case: OxxMemory.s3m
			if (m_PlayBehaviour[PlayBehaviour.St3OffsetWithoutInstrument] || (GetType_() == ModType.Med))
				chn.PrevNoteOffset = 0;

			chn.PrevNoteOffset += param;

			if ((param >= chn.nLoopEnd) && ((GetType_() & (ModType.S3M | ModType.Mtm)) != 0) && chn.dwFlags.Test(ChannelFlags.Chn_Loop) && (chn.nLoopEnd > 0))
			{
				// Offset wrap-around
				// Note that ST3 only does this in GUS mode. SoundBlaster stops the sample entirely instead.
				// Test case: OffsetLoopWraparound.s3m
				param = (param - chn.nLoopStart) % (chn.nLoopEnd - chn.nLoopStart) + chn.nLoopStart;
			}

			if (((GetType_() & (ModType.Mdl | ModType.Ptm)) != 0) && chn.dwFlags.Test(ChannelFlags.Chn_16Bit))
			{
				// Digitrakker and Polytracker use byte offsets, not sample offsets
				param /= 2;
			}

			// IT compatibility: Offset with instrument number but note note recalls previous note and executes offset.
			// Test case: OffsetWithInstr.it
			uint8 note = m_PlayBehaviour[PlayBehaviour.ItOffsetWithInstrNumber] && (chn.RowCommand.Instr != 0) ? chn.nNewNote : chn.RowCommand.Note;

			if (ModCommand.IsNote(note) || m_PlayBehaviour[PlayBehaviour.ApplyOffsetWithoutNote])
			{
				// IT compatibility: If this note is not mapped to a sample, ignore it.
				// Test case: empty_sample_offset.it
				if ((chn.pModInstrument != null) && ModCommand.IsNote(note))
				{
					SampleIndex smp = chn.pModInstrument.Keyboard[note - ModCommand.Note_Min];

					if ((smp == 0) || (smp > GetNumSamples()))
						return;
				}

				if (m_SongFlags.Test(SongFlags.Pt_Mode))
				{
					// ProTracker compatibility: PT1/2-style funky 9xx offset command
					// Test case: ptoffset.mod
					chn.Position.Set((int32)chn.PrevNoteOffset);
					chn.PrevNoteOffset += param;
				}
				else
					chn.Position.Set((int32)param);

				if ((chn.Position.GetUInt() >= chn.nLength) || (chn.dwFlags.Test(ChannelFlags.Chn_Loop) && (chn.Position.GetUInt() >= chn.nLoopEnd)))
				{
					// Offset beyond sample size
					if (m_PlayBehaviour[PlayBehaviour.Ft2St3OffsetOutOfRange] || (GetType_() == ModType.Mtm))
					{
						// FT2 Compatibility: Don't play note if offset is beyond sample length
						// ST3 Compatibility: Don't play note if offset is beyond sample length (non-looped samples only)
						// Test cases: 3xx-no-old-samp.xm, OffsetPastSampleEnd.s3m
						chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
						chn.nPeriod = 0;
					}
					else if ((GetType_() & (ModType.Xm | ModType.Mt2 | ModType.Mod)) == 0)
					{
						// IT Compatibility: Offset
						if (m_PlayBehaviour[PlayBehaviour.ItOffset])
						{
							if (m_SongFlags.Test(SongFlags.ItOldEffects))
								chn.Position.Set((int32)chn.nLength);	// Old FX: Clip to end of sample
							else
								chn.Position.Set(0);	// Reset to beginning of sample
						}
						else
						{
							chn.Position.Set((int32)chn.nLoopStart);

							if (m_SongFlags.Test(SongFlags.ItOldEffects) && (chn.nLength > 4))
								chn.Position.Set((c_int)(chn.nLength - 2));
						}
					}
					else if ((GetType_() == ModType.Mod) && chn.dwFlags.Test(ChannelFlags.Chn_Loop))
						chn.Position.Set((int32)chn.nLoopStart);
				}
			}
			else if ((param < chn.nLength) && ((GetType_() & (ModType.Mtm | ModType.Dmf | ModType.Mdl | ModType.Plm)) != 0))
			{
				// Some trackers can also call offset effects without notes next to them...
				chn.Position.Set((int32)param);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ReverseSampleOffset(ModChannel chn, ModCommandParam param)//XX 5797
		{
			if ((chn.pModSample != null) && (chn.pModSample.nLength > 0))
			{
				chn.dwFlags.Set(ChannelFlags.Chn_PingPongFlag);
				chn.dwFlags.Reset(ChannelFlags.Chn_Loop);
				chn.nLength = chn.pModSample.nLength;	// If there was a loop, extend sample to whole length

				SmpLength offset = (SmpLength)(param << 8);

				if ((GetType_() == ModType.Ptm) && chn.dwFlags.Test(ChannelFlags.Chn_16Bit))
					offset /= 2;

				chn.Position.Set((int32)((chn.nLength - 1) - Math.Min(offset, chn.nLength - 1)), 0);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void DigiBoosterSampleReverse(ModChannel chn, ModCommandParam param)//XX 5812
		{
			if (chn.IsFirstTick && (chn.pModSample != null) && (chn.pModSample.nLength > 0))
			{
				chn.dwFlags.Set(ChannelFlags.Chn_PingPongFlag);
				chn.nLength = chn.pModSample.nLength;	// If there was a loop, extend sample to whole length
				chn.Position.Set((int32)chn.nLength - 1, 0);
				chn.dwFlags.Set(ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop, param > 0);

				if (param > 0)
				{
					chn.nLoopStart = 0;
					chn.nLoopEnd = chn.nLength;

					// TODO: When the sample starts playing in forward direction again, the loop should be updated to the normal sample loop
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void HandleDigiSamplePlayDirection(PlayState state, ChannelIndex chn)//XX 5830
		{
			// Digi Booster mixes two channels into one Paula channel, and when a note is triggered on one of them it resets the reverse play flag on the other
			if (GetType_() == ModType.Digi)
			{
				state.Chn[chn].dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);
				ChannelIndex otherChn = (ChannelIndex)(chn ^ 1);

				if (otherChn < GetNumChannels())
					state.Chn[otherChn].dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void RetrigNote(ChannelIndex nChn, c_int param, c_int offset = 0)//XX 5843
		{
			// Retrig: bit 8 is set if it's the new XM retrig
			ModChannel chn = m_PlayState.Chn[nChn];
			c_int retrigSpeed = param & 0x0f;
			uint8 retrigCount = chn.nRetrigCount;
			bool doRetrig = false;

			// IT compatibility 15. Retrigger
			if (m_PlayBehaviour[PlayBehaviour.ItRetrigger])
			{
				if ((m_PlayState.m_nTickCount == 0) && (chn.RowCommand.Note != 0))
					chn.nRetrigCount = (uint8)(param & 0x0f);
				else if ((chn.nRetrigCount == 0) || (--chn.nRetrigCount == 0))
				{
					chn.nRetrigCount = (uint8)(param & 0x0f);
					doRetrig = true;
				}
			}
			else if (m_PlayBehaviour[PlayBehaviour.Ft2Retrigger] && ((param & 0x100) != 0))
			{
				// Buggy-like-hell FT2 Rxy retrig!
				// Test case: retrig.xm
				if (m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick))
				{
					// Here are some really stupid things FT2 does on the first tick.
					// Test case: RetrigTick0.xm
					if ((chn.RowCommand.Instr > 0) && chn.RowCommand.IsNoteOrEmpty())
						retrigCount = 1;

					if ((chn.RowCommand.VolCmd == VolumeCommand.Volume) && (chn.RowCommand.Vol != 0))
					{
						// I guess this condition simply checked if the volume byte was != 0 in FT2
						chn.nRetrigCount = retrigCount;
						return;
					}
				}

				if (retrigCount >= retrigSpeed)
				{
					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) || !chn.RowCommand.IsNote())
					{
						doRetrig = true;
						retrigCount = 0;
					}
				}
			}
			else
			{
				// Old routines
				if ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0)
				{
					if (retrigSpeed == 0)
						retrigSpeed = 1;

					if ((retrigCount != 0) && ((retrigCount % retrigSpeed) == 0))
						doRetrig = true;

					retrigCount++;
				}
				else if (GetType_() == ModType.Mod)
				{
					// ProTracker-style retrigger
					// Test case: PTRetrigger.mod
					uint32 tick = m_PlayState.m_nTickCount % m_PlayState.m_nMusicSpeed;

					if ((tick == 0) && chn.RowCommand.IsNote())
						return;

					if ((retrigSpeed != 0) && ((tick % retrigSpeed) == 0))
						doRetrig = true;
				}
				else if (GetType_() == ModType.Mtm)
				{
					// In MultiTracker, E9x retriggers the last note at exactly the x-th tick of the row
					doRetrig = m_PlayState.m_nTickCount == (uint32)(param & 0x0f) && (retrigSpeed != 0);
				}
				else
				{
					c_int realSpeed = retrigSpeed;

					// FT2 bug: if a retrig (Rxy) occurs together with a volume command, the first retrig interval is increased by one tick
					if (((param & 0x100) != 0) && (chn.RowCommand.VolCmd == VolumeCommand.Volume) && ((chn.RowCommand.Param & 0xf0) != 0))
						realSpeed++;

					if (!m_PlayState.m_Flags.Test(PlayFlags.Song_FirstTick) || ((param & 0x100) != 0))
					{
						if (realSpeed == 0)
							realSpeed = 1;

						if (((param & 0x100) == 0) && (m_PlayState.m_nMusicSpeed != 0) && ((m_PlayState.m_nTickCount % realSpeed) == 0))
							doRetrig = true;

						retrigCount++;
					}
					else if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
						retrigCount = 0;

					if (retrigCount >= realSpeed)
					{
						if ((m_PlayState.m_nTickCount != 0) || ((param & 0x100) != 0) || (chn.RowCommand.Note == 0))
							doRetrig = true;
					}

					if (m_PlayBehaviour[PlayBehaviour.Ft2Retrigger] && (param == 0))
					{
						// E90 = Retrig instantly, and only once
						doRetrig = m_PlayState.m_nTickCount == 0;
					}
				}
			}

			// IT compatibility: If a sample is shorter than the retrig time (i.e. it stops before the retrig counter hits zero), it is not retriggered.
			// Test case: retrig-short.it
			if ((chn.nLength == 0) && m_PlayBehaviour[PlayBehaviour.ItShortSampleRetrig] && !chn.HasMidiOutput())
				return;

			// ST3 compatibility: No retrig after Note Cut
			// Test case: RetrigAfterNoteCut.s3m
			if (m_PlayBehaviour[PlayBehaviour.St3RetrigAfterNoteCut] && (chn.nFadeOutVol == 0))
				return;

			if (doRetrig)
			{
				uint32 dv = (uint32)((param >> 4) & 0x0f);
				c_int vol = chn.nVolume;

				if (dv != 0)
				{
					// FT2 compatibility: Retrig + volume will not change volume of retrigged notes
					if (!m_PlayBehaviour[PlayBehaviour.Ft2Retrigger] || (chn.RowCommand.VolCmd != VolumeCommand.Volume))
					{
						if (Tables.RetrigTable1[dv] != 0)
							vol = (vol * Tables.RetrigTable1[dv]) / 16;
						else
							vol += ((c_int)Tables.RetrigTable2[dv]) * 4;
					}

					OpenMpt.Limit(ref vol, 0, 256);

					chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);
				}

				uint32 note = chn.nNewNote;
				int32 oldPeriod = chn.nPeriod;

				// ST3 doesn't retrigger OPL notes
				// Test case: RetrigSlide.s3m
				bool oplRealRetrig = chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && m_PlayBehaviour[PlayBehaviour.OplRealRetrig];

				if ((note >= ModCommand.Note_Min) && (note <= ModCommand.Note_Max) && (chn.nLength != 0) && ((GetType_() != ModType.S3M) || oplRealRetrig))
					CheckNna(nChn, 0, (c_int)note, true);

				bool resetEnv = false;

				if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
				{
					if ((chn.RowCommand.Instr != 0) && (param < 0x100))
					{
						InstrumentChange(chn, chn.RowCommand.Instr, false, false);
						resetEnv = true;
					}

					if (param < 0x100)
						resetEnv = true;
				}

				// ProTracker Compatibility: Retrigger with lone instrument number causes instant sample change
				// Test case: InstrSwapRetrigger.mod
				if (m_PlayBehaviour[PlayBehaviour.ModSampleSwap] && (chn.RowCommand.Instr != 0))
				{
					int16 oldFineTune = chn.nFineTune;
					InstrumentChange(chn, chn.RowCommand.Instr, false, false);
					chn.nFineTune = oldFineTune;
				}

				bool fading = chn.dwFlags.Test(ChannelFlags.Chn_NoteFade);
				SmpLength oldPrevNoteOffset = chn.PrevNoteOffset;

				// Retriggered notes should not use previous offset in S3M
				// Test cases: OxxMemoryWithRetrig.s3m, PTOffsetRetrigger.mod
				if (GetType_() == ModType.S3M)
					chn.PrevNoteOffset = 0;

				// IT compatibility: Really weird combination of envelopes and retrigger (see Storlek's q.it testcase)
				// Test cases: retrig.it, RetrigSlide.s3m
				bool itS3MStyle = m_PlayBehaviour[PlayBehaviour.ItRetrigger] || ((GetType_() == ModType.S3M) && (chn.nLength != 0) && !oplRealRetrig);
				NoteChange(chn, (c_int)note, itS3MStyle, resetEnv, false, nChn);

				if (chn.RowCommand.Instr == 0)
					chn.PrevNoteOffset = oldPrevNoteOffset;

				// XM compatibility: Prevent NoteChange from resetting the fade flag in case an instrument number + note-off is present.
				// Test case: RetrigFade.xm
				if (fading && (GetType_() == ModType.Xm))
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

				chn.nVolume = vol;

				if (m_nInstruments != 0)
				{
					chn.RowCommand.Note = (ModCommandNote)note;		// No retrig without note...
				}

				if (((GetType_() & (ModType.It | ModType.Mpt)) != 0) && (chn.RowCommand.Note == ModCommand.Note_None) && (oldPeriod != 0))
					chn.nPeriod = oldPeriod;

				if ((GetType_() & (ModType.S3M | ModType.It | ModType.Mpt)) != 0)
					retrigCount = 0;

				// IT compatibility: see previous IT compatibility comment =)
				if (itS3MStyle)
					chn.Position.Set(0);

				offset--;

				if ((chn.pModSample != null) && !chn.pModSample.uFlags.Test(ChannelFlags.Chn_Adlib) && (offset >= 0) && (offset <= (c_int)chn.pModSample.Cues.size()))
				{
					if (offset == 0)
						offset = (c_int)chn.OldOffset;
					else
					{
						chn.OldOffset = chn.pModSample.Cues[offset - 1];
						offset = (c_int)chn.OldOffset;
					}

					SampleOffset(chn, (SmpLength)offset);
				}
			}

			// Buggy-like-hell FT2 Rxy retrig!
			if (m_PlayBehaviour[PlayBehaviour.Ft2Retrigger] && ((param & 0x100) != 0))
				retrigCount++;

			// Now we can also store the retrig value for IT...
			if (!m_PlayBehaviour[PlayBehaviour.ItRetrigger])
				chn.nRetrigCount = retrigCount;
		}



		/********************************************************************/
		/// <summary>
		/// Execute a frequency slide on given channel.
		/// Positive amounts increase the frequency, negative amounts
		/// decrease it.
		/// The period or frequency that is read and written is in the period
		/// variable, chn.nPeriod is not touched
		/// </summary>
		/********************************************************************/
		public void DoFreqSlide(ModChannel chn, ref int32 period, int32 amount, bool isTonePorta = false)//XX 6049
		{
			if ((period == 0) || (amount == 0))
				return;

			if (GetType_() == ModType._669)
			{
				// Like other oldskool trackers, Composer 669 doesn't have linear slides...
				// But the slides are done in Hertz rather than periods, meaning that they
				// are more effective in the lower notes (rather than the higher notes)
				period += amount * 20;
			}
			else if (GetType_() == ModType.Far)
				period += (amount * 36318 / 1024);
			else if (m_SongFlags.Test(SongFlags.LinearSlides) && ((GetType_() & (ModType.Xm | ModType.Mod)) == 0))
			{
				// IT Linear slides
				int32 oldPeriod = period;
				uint32 absAmount = (uint32)CMath.abs(amount);

				// Note: IT ignores the lower 2 bits when abs(mount) > 16 (it either uses the fine *or* the regular table, not both)
				// This means that vibratos are slightly less accurate in this range than they could be.
				// Other code paths will *either* have an amount that's a multiple of 4 *or* it's less than 16
				if (absAmount < 16)
				{
					if (amount > 0)
						period = Util.MulDivR(period, (int32)GetFineLinearSlideUpTable(this, absAmount), 65536);
					else
						period = Util.MulDivR(period, (int32)GetFineLinearSlideDownTable(this, absAmount), 65536);
				}
				else
				{
					absAmount /= 4U;

					while (absAmount > 0)
					{
						uint32 n = Math.Min(absAmount, (uint32)Tables.LinearSlideUpTable.Length - 1);

						if (amount > 0)
							period = Util.MulDivR(period, (int32)GetLinearSlideUpTable(this, n), 65536);
						else
							period = Util.MulDivR(period, (int32)GetLinearSlideDownTable(this, n), 65536);

						absAmount -= n;
					}
				}

				if (period == oldPeriod)
				{
					bool incPeriod = m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz] == (amount > 0);

					if (incPeriod && (period < int32.MaxValue))
						period++;
					else if (!incPeriod && (period > 1))
						period--;
				}
			}
			else if (!m_SongFlags.Test(SongFlags.LinearSlides) && m_PlayBehaviour[PlayBehaviour.PeriodsAreHertz])
			{
				// IT Amiga slides
				if (amount < 0)
				{
					// Go down
					period = int32.CreateSaturating(Util.Mul32To64_Unsigned(1712 * 8363, (uint32)period) / (Util.Mul32To64_Unsigned((uint32)period, (uint32)(-amount)) + (1712 * 8363)));
				}
				else if (amount > 0)
				{
					// Go up
					int64 periodDiv = (1712 * 8363) - Util.Mul32To64(period, amount);

					if (periodDiv <= 0)
					{
						if (isTonePorta)
						{
							period = int32.MaxValue;
							return;
						}
						else
						{
							period = 0;
							chn.nFadeOutVol = 0;
							chn.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);
						}

						return;
					}

					period = int32.CreateSaturating(Util.Mul32To64_Unsigned(1712 * 8363, (uint32)period) / (uint64)periodDiv);
				}
			}
			else
				period -= amount;

			if (period < 1)
			{
				period = 1;

				if ((GetType_() == ModType.S3M) && !isTonePorta)
				{
					chn.nFadeOutVol = 0;
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade | ChannelFlags.Chn_FastVolRamp);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void NoteCut(ChannelIndex nChn, uint32 nTick, bool cutSample)//XX 6144
		{
			ModChannel chn = m_PlayState.Chn[nChn];
			uint32 tickCount = m_PlayState.m_nTickCount;

			// IT compatibility: If there is a note and a tone portamento next to a Note Cut effect,
			// the Note Cut is not executed - unless there is also a row delay effect and we are on the second repetition of the row.
			// Test case: SCx-Reset.it
			if (m_PlayBehaviour[PlayBehaviour.ItNoteCutWithPorta] && chn.RowCommand.IsNote() && chn.RowCommand.IsTonePortamento())
			{
				uint32 rowLength = m_PlayState.m_nMusicSpeed + m_PlayState.m_nFrameDelay;

				if (m_PlayState.m_nTickCount < rowLength)
					return;

				if ((m_PlayState.m_nPatternDelay != 0) && (m_PlayState.m_nTickCount >= rowLength))
					tickCount %= rowLength;
			}

			if (tickCount == nTick)
			{
				if (cutSample)
				{
					// IT compatibility: Picking up a note after a Note Cut effect through a lone instrument number also restores the
					// original note pitch without any portamento slides, as if there was a note.
					// Test case: SCx-Reset.it
					if (m_PlayBehaviour[PlayBehaviour.ItNoteCutWithPorta])
						chn.nPeriod = 0;

					chn.Increment.Set(0);
					chn.nFadeOutVol = 0;
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);
				}
				else
					chn.nVolume = 0;

				chn.dwFlags.Set(ChannelFlags.Chn_FastVolRamp);

				// Instro sends to a midi chan
				SendMidiNote(nChn, ModCommand.Note_KeyOff, 0);

//XX				if (chn.dwFlags.Test(ChannelFlags.Chn_Adlib) && (m_Opl != null))
//					m_Opl.NoteCut(nChn, false);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void KeyOff(ModChannel chn)//XX 6190
		{
			bool keyIsOn = !chn.dwFlags.Test(ChannelFlags.Chn_KeyOff);
			chn.dwFlags.Set(ChannelFlags.Chn_KeyOff);

			if ((chn.pModInstrument != null) && !chn.VolEnv.Flags.Test(EnvelopeFlags.Enabled))
				chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

			if (chn.nLength == 0)
				return;

			if (chn.dwFlags.Test(ChannelFlags.Chn_SustainLoop) && (chn.pModSample != null) && keyIsOn)
			{
				ModSample pSmp = chn.pModSample;

				if (pSmp.uFlags.Test(ChannelFlags.Chn_Loop))
				{
					if (pSmp.uFlags.Test(ChannelFlags.Chn_PingPongLoop))
						chn.dwFlags.Set(ChannelFlags.Chn_PingPongLoop);
					else
						chn.dwFlags.Reset(ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_PingPongFlag);

					chn.dwFlags.Set(ChannelFlags.Chn_Loop);
					chn.nLength = pSmp.nLength;
					chn.nLoopStart = pSmp.nLoopStart;
					chn.nLoopEnd = pSmp.nLoopEnd;

					if (chn.nLength > chn.nLoopEnd)
						chn.nLength = chn.nLoopEnd;

					if (chn.Position.GetUInt() > chn.nLength)
					{
						// Test case: SusAfterLoop.it
						chn.Position.Set((int32)(chn.nLoopStart + ((chn.Position.GetInt() - chn.nLoopStart) % (chn.nLoopEnd - chn.nLoopStart))));
					}
				}
				else
				{
					chn.dwFlags.Reset(ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_PingPongFlag);
					chn.nLength = pSmp.nLength;
				}
			}

			if (chn.pModInstrument != null)
			{
				ModInstrument pIns = chn.pModInstrument;

				if ((pIns.VolEnv.dwFlags.Test(EnvelopeFlags.Loop) || ((GetType_() & (ModType.Xm | ModType.Mt2 | ModType.Mdl)) != 0)) && (pIns.nFadeOut != 0))
					chn.dwFlags.Set(ChannelFlags.Chn_NoteFade);

				if ((pIns.VolEnv.nReleaseNode != Snd_Def.Env_Release_Node_Unset) && (chn.VolEnv.nEnvValueAtReleaseJump == Snd_Def.Not_Yet_Released))
				{
					chn.VolEnv.nEnvValueAtReleaseJump = int16.CreateSaturating(pIns.VolEnv.GetValueFromPosition((c_int)chn.VolEnv.nEnvPosition, 256));
					chn.VolEnv.nEnvPosition = pIns.VolEnv[pIns.VolEnv.nReleaseNode].Tick;
				}
			}
		}
		#endregion

		//XX 6242
		#region Global effects
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetSpeed(PlayState playState, uint32 param)//XX 6246
		{
			if (param > 0)
				playState.m_nMusicSpeed = param;

			if ((GetType_() == ModType.Stm) && (param > 0))
			{
				playState.m_nMusicSpeed = Math.Max(param >> 4, 1U);
				playState.m_nMusicTempo = ConvertSt2Tempo((uint8)param);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a ST2 tempo byte to classic tempo and speed combination
		/// </summary>
		/********************************************************************/
		public Tempo ConvertSt2Tempo(uint8 tempo)//XX 6266
		{
			const uint32 st2MixingRate = 23863;		// Highest possible setting in ST2

			// This underflows at tempo 06...0F, and the resulting tick lengths depend on the mixing rate.
			// Note: ST2.3 uses the constant 50 below, earlier versions use 49, but they also play samples at a different speed
			int32 samplesPerTick = (int32)(st2MixingRate / (50 - ((st2TempoFactor[tempo >> 4] * (tempo & 0x0f)) >> 4)));

			if (samplesPerTick <= 0)
				samplesPerTick += 65536;

			Tempo result = new Tempo();
			result.SetRaw((uint32)Util.MulDivRFloor(st2MixingRate, 5 * Tempo.FractFact, (uint32)(samplesPerTick * 2)));

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempo(PlayState playState, Tempo param, bool setFromUi = false)//XX 6280
		{
			CModSpecifications specs = GetModSpecifications();

			// Anything lower than the minimum tempo is considered to be a tempo slide
			Tempo minTempo = GetMinimumTempoParam(GetType_());
			Tempo maxTempo = specs.GetTempoMax();

			// MED files may be imported with #xx parameter extension for tempos above 255, but they may be imported as either MOD or XM.
			// As regular MOD files cannot contain effect #xx, the tempo parameter cannot exceed 255 anyway, so we simply ignore their max tempo in CModSpecifications here
			if ((GetType_() & (ModType.Xm | ModType.It | ModType.Mpt)) == 0)
				maxTempo = GetModSpecifications(ModType.Mpt).GetTempoMax();

			if (m_PlayBehaviour[PlayBehaviour.TempoClamp])
				maxTempo.Set(255);

			if (setFromUi)
			{
				// Set tempo from UI - ignore slide commands and such
				playState.m_nMusicTempo = OpenMpt.Clamp(param, specs.GetTempoMin(), maxTempo);
			}
			else if ((param >= minTempo) && (playState.m_Flags.Test(PlayFlags.Song_FirstTick) == !m_PlayBehaviour[PlayBehaviour.ModTempoOnSecondTick]))
			{
				// ProTracker sets the tempo after the first tick.
				// Note: The case of one tick per row is handled in ProcessRow() instead.
				// Test case: TempoChange.mod
				playState.m_nMusicTempo = param < maxTempo ? param : maxTempo;
			}
			else if ((param < minTempo) && !playState.m_Flags.Test(PlayFlags.Song_FirstTick))
			{
				// Tempo Slide
				// Very old MPT versions (last confirmed version: 1.09.066) add/subtract the param only on the first tick.
				// Newer MPT versions (first confirmed version: 1.09.090), add/subtract the param multiplied by 2 only on the first tick.
				// In SVN r26, the behaviour was adjusted to match Impulse Tracker. This change is part of OpenMPT 1.17 RC1 but not the MPT Wild pre-beta
				Tempo tempDiff = new Tempo(param.GetInt() & 0x0f, 0);

				if ((param.GetInt() & 0xf0) == 0x10)
					playState.m_nMusicTempo += tempDiff;
				else
					playState.m_nMusicTempo -= tempDiff;

				Tempo tempoMin = specs.GetTempoMin();
				OpenMpt.Limit(ref playState.m_nMusicTempo, tempoMin, maxTempo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PatternLoop(PlayState state, ChannelIndex nChn, ModCommandParam param)//XX 6322
		{
			if (m_PlayBehaviour[PlayBehaviour.St3NoMutedChannels] && ((state.Chn[nChn].dwFlags & (ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute)) != 0))
				return;		// Not even effects are processed on muted S3M channels

			// ST3 doesn't have per-channel pattern loop memory
			ModChannel chn = state.Chn[(GetType_() == ModType.S3M) ? 0 : nChn];

			if (param == 0)
			{
				// Loop Start
				chn.nPatternLoop = state.m_nRow;
				return;
			}

			// Loop Repeat
			if (chn.nPatternLoopCount != 0)
			{
				// There's a loop left
				chn.nPatternLoopCount--;

				if (chn.nPatternLoopCount == 0)
				{
					// IT compatibility 10. Pattern loops (+ same fix for S3M files)
					// When finishing a pattern loop, the next loop without a dedicated SB0 starts on the first row after the previous loop
					if (m_PlayBehaviour[PlayBehaviour.ItPatternLoopTargetReset] || (GetType_() == ModType.S3M))
						chn.nPatternLoop = state.m_nRow + 1;

					return;
				}
			}
			else
			{
				// First time we get into the loop => Set loop count
				//
				// IT compatibility 10. Pattern loops (+ same fix for XM / MOD / S3M files)
				if (!m_PlayBehaviour[PlayBehaviour.ItFt2PatternLoop] && ((GetType_() & (ModType.Mod | ModType.S3M)) == 0))
				{
					foreach (ModChannel otherChn in state.PatternChannels(this))
					{
						// Loop on other channel
						if ((otherChn != chn) && (otherChn.nPatternLoopCount != 0))
							return;
					}
				}

				chn.nPatternLoopCount = param;
			}

			state.m_NextPatStartRow = chn.nPatternLoop;		// Nasty FT2 E60 bug emulation!

			RowIndex loopTarget = chn.nPatternLoop;

			if (loopTarget != Snd_Def.RowIndex_Invalid)
			{
				// FT2 compatibility: E6x overwrites jump targets of Dxx effects that are located left of the E6x effect.
				// Test cases: PatLoop-Jumps.xm, PatLoop-Various.xm
				if ((state.m_BreakRow != Snd_Def.RowIndex_Invalid) && m_PlayBehaviour[PlayBehaviour.Ft2PatternLoopWithJumps])
					state.m_BreakRow = loopTarget;

				state.m_PatLoopRow = loopTarget;

				// IT compatibility: SBx is prioritized over Position Jump (Bxx) effects that are located left of the SBx effect.
				// Test case: sbx-priority.it, LoopBreak.it
				if (m_PlayBehaviour[PlayBehaviour.ItPatternLoopWithJumps])
					state.m_PosJump = Snd_Def.OrderIndex_Invalid;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void GlobalVolSlide(PlayState playState, ModCommandParam param, ChannelIndex chn)//XX 6386
		{
			if (m_SongFlags.Test(SongFlags.Auto_GlobalVol))
				playState.Chn[chn].AutoSlide.SetActive(AutoSlideCommand.GlobalVolumeSlide, param != 0);

			if (param != 0)
				playState.Chn[chn].nOldGlobalVolSlide = param;
			else
				param = playState.Chn[chn].nOldGlobalVolSlide;

			if ((GetType_() & (ModType.Xm | ModType.Mt2)) != 0)
			{
				// XM nibble priority
				if ((param & 0xf0) != 0)
					param &= 0xf0;
				else
					param &= 0x0f;
			}

			int32 nGlbSlide = 0;

			if (((param & 0x0f) == 0x0f) && ((param & 0xf0) != 0))
			{
				if (playState.m_Flags.Test(PlayFlags.Song_FirstTick))
					nGlbSlide = (param >> 4) * 2;
			}
			else
			{
				if (((param & 0xf0) == 0xf0) && ((param & 0x0f) != 0))
				{
					if (playState.m_Flags.Test(PlayFlags.Song_FirstTick))
						nGlbSlide = -((param & 0x0f) * 2);

				}
				else
				{
					if (!playState.m_Flags.Test(PlayFlags.Song_FirstTick))
					{
						if ((param & 0xf0) != 0)
						{
							// IT compatibility: Ignore slide commands with both nibbles set
							if (((GetType_() & (ModType.It | ModType.Mpt | ModType.Imf | ModType.J2B | ModType.Mid | ModType.Ams | ModType.Dbm)) == 0) || ((param & 0x0f) == 0))
								nGlbSlide = ((param & 0xf0) >> 4) * 2;
						}
						else
							nGlbSlide = -((param & 0x0f) * 2);
					}
				}
			}

			if (nGlbSlide != 0)
			{
				if ((GetType_() & (ModType.It | ModType.Mpt | ModType.Imf | ModType.J2B | ModType.Mid | ModType.Ams | ModType.Dbm)) == 0)
					nGlbSlide *= 2;

				nGlbSlide += playState.m_nGlobalVolume;
				OpenMpt.Limit(ref nGlbSlide, 0, 256);
				playState.m_nGlobalVolume = nGlbSlide;
			}
		}
		#endregion

		//XX 6442
		#region Note/Period/Frequency functions
		/********************************************************************/
		/// <summary>
		/// Find lowest note which has same or lower period as a given period
		/// (i.e. the note has the same or higher frequency)
		/// </summary>
		/********************************************************************/
		public uint32 GetNoteFromPeriod(uint32 period, int32 nFineTune, uint32 nC5Speed)//XX 6445
		{
			if (period == 0)
				return 0;

			if (m_PlayBehaviour[PlayBehaviour.Ft2Periods])
			{
				// FT2's "RelocateTon" function actually rounds up and down, while GetNoteFromPeriod normally just truncates
				nFineTune += 64;
			}

			// This essentially implements std::lower_bound, with the difference that we don't need an iterable container
			uint32 minNote = ModCommand.Note_Min, maxNote = ModCommand.Note_Max, count = maxNote - minNote + 1;
			bool periodIsFreq = PeriodsAreFrequencies();

			while (count > 0)
			{
				uint32 step = count / 2, midNote = minNote + step;
				uint32 n = GetPeriodFromNote(midNote, nFineTune, nC5Speed);

				if (((n > period) && !periodIsFreq) || ((n < period) && periodIsFreq) || (n == 0))
				{
					minNote = midNote + 1;
					count -= step + 1;
				}
				else
					count = step;
			}

			return minNote;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 GetPeriodFromNote(uint32 note, int32 nFineTune, uint32 nC5Speed)//XX 6473
		{
			if ((note == ModCommand.Note_None) || (note >= ModCommand.Note_Min_Special))
				return 0;

			note -= ModCommand.Note_Min;

			if (!UseFineTuneAndTranspose())
			{
				if (GetType_() == ModType.Mdl)
				{
					// MDL uses non-linear slides, but their effectiveness does not depend on the middle-C frequency
					return (uint32)((Tables.FreqS3MTable[note % 12U] << 4) >> (int32)(note / 12));
				}
				else if (GetType_() == ModType.Dtm)
				{
					// Similar to MDL, but finetune is factored in and we don't transpose everything by an octave
					return (uint32)((Tables.ProTrackerTunedPeriods[(Xm2ModFineTune(nFineTune) * 12U) + (note % 12U)] << 5) >> (int32)(note / 12U));
				}

				if (nC5Speed == 0)
					nC5Speed = 8363;

				if (PeriodsAreFrequencies())
				{
					// Compute everything in Hertz rather than periods
					uint32 freq = Util.MulDiv_Unsigned(nC5Speed, Tables.LinearSlideUpTable[(note % 12U) * 16U] << (int32)(note / 12), 65536 << 5);
					OpenMpt.LimitMax(ref freq, (uint32)int32.MaxValue);

					return freq;
				}
				else if (m_SongFlags.Test(SongFlags.LinearSlides))
					return (uint32)((Tables.FreqS3MTable[note % 12U] << 5) >> (int32)(note / 12));
				else
				{
					OpenMpt.LimitMax(ref nC5Speed, uint32.MaxValue >> (int32)(note / 12));

					// (a*b)/c
					return Util.MulDiv_Unsigned(8363, (uint32)Tables.FreqS3MTable[note % 12U] << 5, nC5Speed << (int32)(note / 12));
					// 8363 * freq[note%12] / nC5Speed * 2^(5-note/12)
				}
			}
			else if (((GetType_() & (ModType.Xm | ModType.Mtm)) != 0) || m_SongFlags.Test(SongFlags.LinearSlides))
			{
				if (note < 12)
					note = 12;

				note -= 12;

				if (GetType_() == ModType.Mtm)
					nFineTune *= 16;
				else if (m_PlayBehaviour[PlayBehaviour.Ft2FineTunePrecision])
				{
					// FT2 Compatibility: The lower three bits of the finetune are truncated.
					// Test case: Finetune-Precision.xm
					nFineTune &= ~7;
				}

				if (m_SongFlags.Test(SongFlags.LinearSlides))
				{
					c_int l = (c_int)(((120 - note) << 6) - (nFineTune / 2));

					if (l < 1)
						l = 1;

					return (uint32)l;
				}
				else
				{
					c_int fineTune = nFineTune;
					uint32 rNote = (note % 12) << 3;
					uint32 rOct = note / 12;
					c_int rFine = fineTune / 16;
					c_int i = (c_int)(rNote + rFine + 8);

					OpenMpt.Limit(ref i, 0, 103);

					uint32 per1 = Tables.XmPeriodTable[i];

					if (fineTune < 0)
					{
						rFine--;
						fineTune = -fineTune;
					}
					else
						rFine++;

					i = (c_int)(rNote + rFine + 8);

					if (i < 0)
						i = 0;

					if (i >= 104)
						i = 103;

					uint32 per2 = Tables.XmPeriodTable[i];

					rFine = fineTune & 0x0f;
					per1 *= (uint32)(16 - rFine);
					per2 = (uint32)(per2 * rFine);

					return ((per1 + per2) << 1) >> (int32)rOct;
				}
			}
			else
			{
				nFineTune = Xm2ModFineTune(nFineTune);

				if ((nFineTune != 0) || (note < 24) || (note >= (24 + Tables.ProTrackerPeriodTable.Length)))
					return (uint32)((Tables.ProTrackerTunedPeriods[(nFineTune * 12U) + (note % 12U)] << 5) >> (int32)(note / 12));
				else
					return (uint32)(Tables.ProTrackerPeriodTable[note - 24] << 2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts period value to sample frequency. Return value is fixed
		/// point, with FREQ_FRACBITS fractional bits
		/// </summary>
		/********************************************************************/
		public uint32 GetFreqFromPeriod(uint32 period, uint32 c5Speed, int32 nPeriodFrac)//XX 6563
		{
			if (period == 0)
				return 0;

			if (((GetType_() & (ModType.Xm | ModType.Mtm)) != 0) || (m_SongFlags.Test(SongFlags.LinearSlides) && UseFineTuneAndTranspose()))
			{
				if (m_PlayBehaviour[PlayBehaviour.Ft2Periods])
				{
					// FT2 compatibility: Period is a 16-bit value in FT2, and it overflows happily.
					// Test case: FreqWraparound.xm
					period &= 0xffff;
				}

				if (m_SongFlags.Test(SongFlags.LinearSlides))
				{
					uint32 octave;

					if (m_PlayBehaviour[PlayBehaviour.Ft2Periods])
					{
						// Under normal circumstances, this calculation returns the same values as the non-compatible one.
						// However, once the 12 octaves are exceeded (through portamento slides), the octave shift goes
						// crazy in FT2, meaning that the frequency wraps around randomly...
						// The entries in FT2's conversion table are four times as big, hence we have to do an additional shift by two bits.
						// Test case: FreqWraparound.xm
						// 12 octaves * (12 * 64) LUT entries = 9216, add 767 for rounding
						uint32 div = (9216U + 767U - period) / 768;
						octave = (14 - div) & 0x1f;
					}
					else
					{
						if (period > (29 * 768))
							return 0;

						octave = (period / 768) + 2;
					}

					return (Tables.XmLinearTable[period % 768] << (c_int)(Snd_Def.Freq_FracBits + 2)) >> (c_int)octave;
				}
				else
				{
					if (period == 0)
						period = 1;

					return (uint32)(((8363 * 1712L) << Snd_Def.Freq_FracBits) / period);
				}
			}
			else if (UseFineTuneAndTranspose())
				return (uint32)(((3546895L * 4) << Snd_Def.Freq_FracBits) / period);
			else if (GetType_() == ModType._669)
			{
				// We only really use c5speed for the finetune pattern command.
				// All samples in 669 files have the same middle-C speed (imported as 8363 Hz)
				return (period + c5Speed - 8363) << Snd_Def.Freq_FracBits;
			}
			else if (GetType_() == ModType.Mdl)
			{
				OpenMpt.LimitMax(ref period, uint32.MaxValue >> 8);

				if (c5Speed == 0)
					c5Speed = 8363;

				return Util.MulDiv_Unsigned(c5Speed, (uint32)((1712L << 7) << Snd_Def.Freq_FracBits), (uint32)((period << 8) + nPeriodFrac));
			}
			else
			{
				OpenMpt.LimitMax(ref period, uint32.MaxValue >> 8);

				if (PeriodsAreFrequencies())
				{
					// Input is already a frequency in Hertz, not a period
					return (uint32)((((uint64)period << 8) + (uint64)nPeriodFrac) >> (8 - Snd_Def.Freq_FracBits));
				}
				else if (m_SongFlags.Test(SongFlags.LinearSlides) || (GetType_() == ModType.Dtm))
				{
					if (c5Speed == 0)
						c5Speed = 8363;

					return Util.MulDiv_Unsigned(c5Speed, (uint32)((1712L << 8) << Snd_Def.Freq_FracBits), (uint32)((period << 8) + nPeriodFrac));
				}
				else
					return Util.MulDiv_Unsigned(8363, (uint32)((1712L << 8) << Snd_Def.Freq_FracBits), (uint32)((period << 8) + nPeriodFrac));
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoMpt(ModChannel chn, c_int param)//XX 6811
		{
			// Behavior: Modifies portamento by param-steps on every tick.
			// Note that step meaning depends on tuning
			chn.m_PortamentoFineSteps += param;
			chn.m_CalculateFreq = true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoFineMpt(PlayState playState, ChannelIndex nChn, c_int param)//XX 6821
		{
			ModChannel chn = playState.Chn[nChn];

			// Behavior: Divides portamento change between ticks/row. For example
			// if Ticks/row == 6, and param == +-6, portamento goes up/down by one tuning-dependent
			// fine step every tick
			if (playState.m_nTickCount == 0)
				chn.nOldFinePortaUpDown = 0;

			c_int tickParam = (c_int)((playState.m_nTickCount + 1.0) * param / playState.m_nMusicSpeed);
			chn.m_PortamentoFineSteps += (param >= 0) ? tickParam - chn.nOldFinePortaUpDown : tickParam + chn.nOldFinePortaUpDown;

			if ((playState.m_nTickCount + 1) == playState.m_nMusicSpeed)
				chn.nOldFinePortaUpDown = (uint8)CMath.abs(param);
			else
				chn.nOldFinePortaUpDown = (uint8)CMath.abs(tickParam);

			chn.m_CalculateFreq = true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void PortamentoExtraFineMpt(ModChannel chn, c_int param)//XX 6842
		{
			// This kinda behaves like regular fine portamento.
			// It changes the pitch by n finetune steps on the first tick
			if (chn.IsFirstTick)
			{
				chn.m_PortamentoFineSteps += param;
				chn.m_CalculateFreq = true;
			}
		}
		#endregion
	}
}

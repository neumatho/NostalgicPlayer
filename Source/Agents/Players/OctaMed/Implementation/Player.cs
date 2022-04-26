/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Synth;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Player interface
	/// </summary>
	internal class Player : Mixer
	{
		#region SynthData class
		/// <summary>
		/// Track specific synth sound handling data
		/// </summary>
		private class SynthData
		{
			public enum SyType
			{
				None,
				Synth,
				Hybrid
			}

			public SyType SynthType;
			public int PeriodChange;
			public uint VibOffs;			// Vibrato offset
			public uint VibSpeed;			// Speed
			public int VibDep;				// Depth
			public uint VibWfNum;			// Waveform number (0 = default sine)
			public uint ArpStart;			// Arpeggio sequence start offset (in wfTable)
			public uint ArpOffs;			// Current arpeggio offset
			public uint VolCmdPos;			// Current volume execution position
			public uint WfCmdPos;			// Current waveform execution position
			public uint VolWait;			// Current volume wait
			public uint WfWait;				// Current waveform wait
			public uint VolChgSpeed;		// Volume change speed
			public uint WfChgSpeed;			// Waveform change speed
			public uint VolXSpeed;			// Execution speed
			public uint WfXSpeed;			// ...for waveform too
			public int VolXCnt;				// Execution counter
			public int WfXCnt;
			public uint EnvWfNum;			// Envelope waveform # (0 = none)
			public bool EnvLoop;			// Envelope looping?
			public ushort EnvCount;			// Envelope position counter
			public int Vol;					// Current synth volume
			public NoteNum NoteNumber;		// Note number with transpose for arpeggio
		}
		#endregion

		#region TrackData class
		/// <summary>
		/// TrackData structure for each track
		/// </summary>
		private class TrackData
		{
			public enum FxType
			{
				Normal,
				None,
				Midi,
				NoPlay = Midi
			}

			[Flags]
			public enum MiscFlag : byte
			{
				None = 0,
				Backwards = 0x01,
				NoSynthWfPtrReset = 0x02,
				StopNote = 0x04
			}

			public NoteNum TrkPrevNote;
			public InstNum TrkPrevINum;
			public byte TrkPrevVol;
			public byte TrkPrevMidiN;		// 0 = None, 1 = 0, 2 = 1, ...
			public MiscFlag TrkMiscFlags;
			public int TrkNoteOffCnt;		// -1 = None
			public uint TrkInitHold;
			public uint TrkInitDecay;
			public uint TrkDecay;
			public uint TrkFadeSpeed;		// Fading speed for decay
			public int TrkSTransp;
			public int TrkFineTune;
			public int TrkArpAdjust;
			public int TrkVibrAdjust;
			public uint TrkSOffset;
			public NoteNum TrkCurrNote;
			public FxType TrkFxType;
			public int TrkFrequency;
			public int TrkPortTargetFreq;	// Portamento (cmd 03) target frequency
			public ushort TrkPortSpeed;
			public int TrkCutOffTarget;		// Filter cutoff sweep (cmd 23) target value
			public ushort TrkCutOffSwSpeed;	// Cutoff sweep speed
			public int TrkCutOffLogPos;		// Logarithmic position of cutoff sweep
			public byte TrkVibShift;		// Depends on cmd 04 or 14
			public byte TrkVibSpeed;
			public byte TrkVibSize;
			public byte TrkTempVol;			// Temporary volume (+1; 0 = none)
			public ushort TrkVibOffs;
			public bool TrkLastNoteMidi;	// True if last note was MIDI note
			public readonly SynthData TrkSy = new SynthData();
			public VisualInfo VisualInfo = new VisualInfo();
		}
		#endregion

		private enum Break
		{
			Normal,
			PatternBreak,
			Loop,
			PositionJump
		}

		private static readonly sbyte[] sineTable =
		{
			0, 25, 49, 71, 90, 106, 117, 125, 127, 125, 117, 106, 90, 71, 49, 25,
			0, -25, -49, -71, -90, -106, -117, -125, -127, -125, -117, -106, -90, -71, -49, -25
		};

		private readonly TrackData[] td = Helpers.InitializeArray<TrackData>(Constants.MaxTracks);

		private readonly bool[] plrTrackEnabled = new bool[Constants.MaxTracks];		// Track on/off states

		private Song plrSong;
		private int plrPulseCtr;
		private uint plrBlockDelay;
		private TrackNum plrChannels;
		private LineNum plrFxLine;
		private LineNum plrNextLine;
		private BlockNum plrFxBlock;
		private LineNum plrRepeatLine;
		private uint plrRepeatCounter;
		private bool plrDelayedStop;
		public PlayPosition plrPos;
		private Break plrBreak;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Player(OctaMedWorker worker) : base(worker)
		{
			plrPos = new PlayPosition(worker);

			for (TrackNum cnt = 0; cnt < Constants.MaxTracks; cnt++)
				EnableTrack(cnt, true);

			plrSong = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player so it's ready to play the song
		/// </summary>
		/********************************************************************/
		public void PlaySong(Song song)
		{
			for (uint cnt = 0; cnt < Constants.MaxTracks; cnt++)
			{
				td[cnt] = new TrackData();
				td[cnt].TrkNoteOffCnt = -1;
			}

			for (uint cnt = 0; cnt < Constants.MaxInstr - 1; cnt++)
			{
				Instr i = song.GetInstr(cnt);
				i.Init();
			}

			SubSong css = song.CurrSS();

			plrBlockDelay = 0;
			plrSong = song;
			plrFxLine = 0;
			plrFxBlock = 0;
			plrNextLine = 0;
			plrBreak = Break.Normal;
			plrRepeatLine = 0;
			plrRepeatCounter = 0;
			plrDelayedStop = false;
			plrChannels = (TrackNum)css.GetNumChannels();
			plrPulseCtr = css.GetTempoTpl();			// To make sure that a new note is immediately triggered
			worker.SetAmigaFilter(css.GetAmigaFilter());

			PlayPosition pos = css.Pos();
			pos.SetPosition(PlayPosition.PosDef.SongStart);
			pos.SetAdvMode(PlayPosition.AdvMode.AdvSong);
			css.SetPos(pos);

			plrPos = css.Pos();							// Copy for our private use

			Start((uint)plrChannels);
		}



		/********************************************************************/
		/// <summary>
		/// Return an effect master instance if the player adds extra mixer
		/// effects to the output
		/// </summary>
		/********************************************************************/
		public IEffectMaster EffectMaster => plrSong.CurrSS().Fx();



		/********************************************************************/
		/// <summary>
		/// The main player routine
		/// </summary>
		/********************************************************************/
		public void PlrCallBack()
		{
			SubSong ss = plrSong.CurrSS();
			LineNum line = plrPos.Line();

			MedBlock blk;

			// Advance & check pulse counter
			++plrPulseCtr;
			if (plrPulseCtr >= ss.GetTempoTpl())
			{
				blk = ss.Block(plrPos.Block());
				plrPulseCtr = 0;

				if (plrDelayedStop)
				{
					// End of module
					worker.SetEndReached();
				}

				if ((plrBlockDelay != 0) && (--plrBlockDelay != 0))
				{
					// Don't get a new line
				}
				else
				{
					// Read in notes
					for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
					{
						TrackData trkD = td[trkCnt];

						if (!IsTrackEnabled(trkCnt))
						{
							// Muted, skip!
							trkD.TrkFxType = TrackData.FxType.NoPlay;
							continue;		// Next track, please!
						}

						trkD.TrkFxType = TrackData.FxType.Normal;
						MedNote currN = blk.Note(line, trkCnt);

						// Get current note of this track
						if ((currN.NoteNum >= Constants.NoteDef) && (currN.NoteNum <= Constants.Note44k))
							trkD.TrkCurrNote = currN.NoteNum;
						else
						{
							if ((trkD.TrkCurrNote = currN.NoteNum) > 0x7f)
								trkD.TrkCurrNote = 0;
						}

						// If not empty, set trkPrevNote also
						if (trkD.TrkCurrNote != 0)
							trkD.TrkPrevNote = trkD.TrkCurrNote;

						if ((currN.InstrNum != 0) && (currN.InstrNum <= Constants.MaxInstr))
						{
							trkD.TrkPrevINum = (InstNum)(currN.InstrNum - 1);

							Instr currI = plrSong.GetInstr(trkD.TrkPrevINum);
							ExtractInstrData(trkD, currI);

							// Vitale.med uses a portamento effect on a synth instrument
							// while playing a note. This causes some internal variables
							// to get out of bounds, if they are not reset.
							//
							// The note is checked, else "Traps 'n' Treasures - Jingle 4.med"
							// cut notes
							if (currN.NoteNum != 0)
							{
								Sample smp = plrSong.GetSample(trkD.TrkPrevINum);
								if ((smp != null) && smp.IsSynthSound())
								{
									// Clear parameters
									ClearSynth(trkD, trkD.TrkPrevINum);
								}
							}
						}

						// STP? (ExtractInstrData clears miscFlags)
						if (currN.NoteNum == Constants.NoteStp)
							trkD.TrkMiscFlags |= TrackData.MiscFlag.StopNote;
					}

					// Pre-fx
					for (PageNum pageCnt = 0; pageCnt < blk.Pages(); pageCnt++)
					{
						for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
						{
							TrackData trkD = td[trkCnt];

							if (trkD.TrkFxType == TrackData.FxType.None)
								continue;

							MedCmd cmd = blk.Cmd(line, trkCnt, pageCnt);
							byte data = cmd.GetDataB();
							ushort dataW = cmd.GetData();

							switch (cmd.GetCmd())
							{
								// Portamento
								case 0x03:
								{
									if (trkD.TrkCurrNote != 0)
									{
										NoteNum dest = trkD.TrkCurrNote;

										if (dest < 0x80)
										{
											int dn = dest + (ss.GetPlayTranspose() + trkD.TrkSTransp);

											while (dn >= 0x80)
												dn -= 12;

											while (dn < 1)
												dn += 12;

											dest = (NoteNum)dn;
										}

										trkD.TrkPortTargetFreq = (int)GetInstrNoteFreq(dest, plrSong.GetInstr(trkD.TrkPrevINum));
									}

									if (dataW != 0)
										trkD.TrkPortSpeed = dataW;

									trkD.TrkFxType = TrackData.FxType.NoPlay;
									break;
								}

								// Set hold/decay
								case 0x08:
								{
									if (plrSong.GetInstr(trkD.TrkPrevINum).IsMidi())
									{
										// Two digits used for hold with MIDI instruments
										trkD.TrkInitHold = data;
									}
									else
									{
										trkD.TrkInitHold = (uint)data & 0x0f;
										trkD.TrkInitDecay = (uint)data >> 4;
									}
									break;
								}

								// Set ticks per line
								case 0x09:
								{
									ss.SetTempoTpl((ushort)(data & 0x1f));
									worker.ChangePlayFreq();
									break;
								}

								// Position jump
								case 0x0b:
								{
									plrBreak = Break.PositionJump;
									plrNextLine = data;
									break;
								}

								// Volume
								case 0x0c:
								{
									if (data < 0x80)
									{
										if (data <= 64)
											trkD.TrkPrevVol = (byte)(data * 2);
									}
									else
									{
										// Set default volume
										data &= 0x7f;
										if (data <= 64)
										{
											data *= 2;

											trkD.TrkPrevVol = data;
											plrSong.GetInstr(trkD.TrkPrevINum).SetVol(data);
										}
									}
									break;
								}

								// Set synth waveform sequence position
								case 0x0e:
								{
									if (data < 128)
									{
										trkD.TrkSy.WfCmdPos = data;
										trkD.TrkMiscFlags |= TrackData.MiscFlag.NoSynthWfPtrReset;
									}
									break;
								}

								// Misc/Main tempo
								case 0x0f:
								{
									switch (data)
									{
										case 0x00:
										{
											plrBreak = Break.PatternBreak;
											plrNextLine = 0;
											break;
										}

										case 0xf2:
										case 0xf4:
										case 0xf5:
										{
											DoDelayRetrig(trkD);
											break;
										}

										case 0xf7:			// Wait until MIDI message sent
											break;

										case 0xf8:			// Amiga filter off
										{
											worker.SetAmigaFilter(false);
											break;
										}

										case 0xf9:			// Amiga filter on
										{
											worker.SetAmigaFilter(true);
											break;
										}

										case 0xfd:			// Change frequency
										{
											if (trkD.TrkCurrNote != 0)
											{
												NoteNum dest = trkD.TrkCurrNote;

												if (dest < 0x80)
												{
													int dn = dest + (ss.GetPlayTranspose() + trkD.TrkSTransp);

													while (dn >= 0x80)
														dn -= 12;

													while (dn < 1)
														dn += 12;

													dest = (NoteNum)dn;
												}

												trkD.TrkFrequency = (int)GetInstrNoteFreq(dest, plrSong.GetInstr(trkD.TrkPrevINum));
											}
											break;
										}

										case 0xfe:			// Stop
										{
											plrDelayedStop = true;
											worker.SetEndReached();
											break;
										}

										case 0xff:
										{
											MuteChannel((uint)trkCnt);
											break;
										}

										default:			// Change tempo
										{
											if (data <= 240)
											{
												ss.SetTempoBpm(data);
												worker.ChangePlayFreq();
											}
											break;
										}
									}
									break;
								}

								// Send custom MIDI/SYSX message
								case 0x10:
									break;

								// Fine tune
								case 0x15:
								{
									sbyte sData = (sbyte)data;

									if ((sData >= -8) && (sData <= 7))
										trkD.TrkFineTune = sData;

									break;
								}

								// Repeat loop
								case 0x16:
								{
									if (data != 0)
									{
										if (plrRepeatCounter == 0)
											plrRepeatCounter = data;	// Init loop
										else
										{
											if (--plrRepeatCounter == 0)
												break;					// Continue
										}

										plrNextLine = plrRepeatLine;	// Jump to beginning of loop
										plrBreak = Break.Loop;
									}
									else
										plrRepeatLine = line;			// Store line number

									break;
								}

								// Sample offset
								case 0x19:
								{
									trkD.TrkSOffset = (uint)dataW << 8;
									break;
								}

								// Change MIDI preset
								case 0x1c:
									break;

								// Next pattern
								case 0x1d:
								{
									plrBreak = Break.PatternBreak;
									plrNextLine = data;
									break;
								}

								// Block delay
								case 0x1e:
								{
									if (plrBlockDelay == 0)
										plrBlockDelay = (uint)data + 1;

									break;
								}

								// Delay/Retrig
								case 0x1f:
								{
									DoDelayRetrig(trkD);
									break;
								}

								// Sample backwards
								case 0x20:
								{
									if (dataW == 0)
										trkD.TrkMiscFlags |= TrackData.MiscFlag.Backwards;

									break;
								}

								// Filter sweep (CutOff)
								case 0x23:
								{
									if (dataW == 0)
										trkD.TrkCutOffTarget = 0;
									else
									{
										trkD.TrkCutOffTarget = (cmd.GetDataB() + 1) << 8;
										trkD.TrkCutOffSwSpeed = (ushort)(cmd.GetData2() * 20);
										trkD.TrkCutOffLogPos = 0;		// Filled by the sweep code
									}
									break;
								}

								// Set filter cutoff frequency
								case 0x24:
								{
									trkD.TrkCutOffTarget = 0;			// Stop any sweep
									break;
								}

								// Set filter resonance + type
								case 0x25:
									break;

								// ARexx trigger (only on Amiga)
								case 0x2d:
									break;

								// Panpot
								case 0x2e:
								{
									if (((sbyte)data >= -16) && ((sbyte)data <= 16))
										ss.SetTrackPan(trkCnt, (sbyte)data);

									break;
								}

								// Effect settings
								//
								// data: $Ex (x = 6 - 1) -> echo depth
								//       $Dx (x = C - 0 - 4) -> stereo separation
								case 0x2f:
								{
									byte effCmd = (byte)(data & 0xf0);
									byte effData = (byte)(data & 0x0f);

									if (effCmd == 0xe0)
									{
										// Echo depth
										effData = (byte)(7 - effData);
										if ((effData >= 1) && (effData <= 6))
											ss.Fx().GlobalGroup.SetEchoDepth(effData);
									}
									else if (effCmd == 0xd0)
									{
										// Stereo separation
										sbyte stereoSep = (sbyte)effData;

										if (effData >= 12)
											stereoSep = (sbyte)-(16 - effData);

										if ((stereoSep >= -4) && (stereoSep <= 4))
											ss.Fx().GlobalGroup.SetStereoSeparation(stereoSep);
									}
									break;
								}
							}
						}
					}

					// Play notes
					for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
					{
						TrackData trkD = td[trkCnt];

						if (trkD.TrkFxType == TrackData.FxType.NoPlay)
							continue;

						if (trkD.TrkCurrNote != 0)
						{
							if (trkD.TrkInitHold != 0)
								trkD.TrkNoteOffCnt = (int)trkD.TrkInitHold;
							else
								trkD.TrkNoteOffCnt = -1;

							PlrPlayNote(trkCnt, (NoteNum)(trkD.TrkCurrNote - 1), trkD.TrkPrevINum);
						}
					}

					// Take commands from current line/block
					plrFxLine = line;
					plrFxBlock = plrPos.Block();

					// Advance play position
					if (!plrDelayedStop)
					{
						switch (plrBreak)
						{
							case Break.Loop:
							{
								plrPos.Line(plrNextLine);
								plrBreak = Break.Normal;
								break;
							}

							case Break.PatternBreak:
							{
								plrPos.PatternBreak(plrNextLine, PSeqCmdHandler);
								plrBreak = Break.Normal;
								break;
							}

							case Break.PositionJump:
							{
								plrPos.PositionJump((uint)plrNextLine, PSeqCmdHandler);
								plrBreak = Break.Normal;
								break;
							}

							default:
							{
								plrPos.AdvancePos(PSeqCmdHandler);
								break;
							}
						}
					}

					blk = ss.Block(plrPos.Block());		// Block may have changed
				}

				// Check holding
				LineNum currLine = plrPos.Line();

				for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
				{
					if (td[trkCnt].TrkNoteOffCnt >= 0)
					{
						MedNote note = blk.Note(currLine, trkCnt);

						// Continue hold if:
						// * "hold" symbol (no note, instr. num) OR
						// * note and command 03 on page 1
						if (((note.NoteNum == 0) && (note.InstrNum != 0)) || ((note.NoteNum != 0) && (blk.Cmd(currLine, trkCnt, 0).GetCmd() == 0x03)))
							td[trkCnt].TrkNoteOffCnt += ss.GetTempoTpl();
					}
				}
			}

			// Back to processing for every tick...
			if (plrFxBlock >= ss.NumBlocks())
				plrFxBlock = ss.NumBlocks() - 1;

			blk = ss.Block(plrFxBlock);

			// Hold and fade handling
			for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
			{
				TrackData trkD = td[trkCnt];

				if ((trkD.TrkPrevMidiN > 0) && (trkD.TrkPrevMidiN <= 0x80))
				{
					trkD.TrkFxType = TrackData.FxType.Midi;

					if (((trkD.TrkMiscFlags & TrackData.MiscFlag.StopNote) != 0) || ((trkD.TrkNoteOffCnt >= 0) && (--trkD.TrkNoteOffCnt < 0)))
					{
						trkD.TrkMiscFlags &= ~TrackData.MiscFlag.StopNote;
						MuteChannel((uint)trkCnt);
					}
				}
				else
				{
					if (((trkD.TrkMiscFlags & TrackData.MiscFlag.StopNote) != 0) || ((trkD.TrkNoteOffCnt >= 0) && (--trkD.TrkNoteOffCnt < 0)))
					{
						trkD.TrkMiscFlags &= ~TrackData.MiscFlag.StopNote;

						if (trkD.TrkSy.SynthType != SynthData.SyType.None)
						{
							// A synth/hybrid sound has special way of decay (JMP)
							trkD.TrkSy.VolCmdPos = trkD.TrkDecay;
							trkD.TrkSy.VolWait = 0;
						}
						else
						{
							// A normal decay... just set fade speed, and if 0, mute immediately
							if ((trkD.TrkFadeSpeed = trkD.TrkDecay * 2) == 0)
								MuteChannel((uint)trkCnt);
						}
					}

					if (trkD.TrkFadeSpeed != 0)
					{
						if (trkD.TrkPrevVol > trkD.TrkFadeSpeed)
							trkD.TrkPrevVol -= (byte)trkD.TrkFadeSpeed;
						else
						{
							trkD.TrkPrevVol = 0;
							trkD.TrkFadeSpeed = 0;
						}
					}

					trkD.TrkFxType = TrackData.FxType.Normal;
				}
			}

			if (plrFxLine >= blk.Lines())
				plrFxLine = blk.Lines() - 1;

			// Effect handling (once per timing pulse)
			for (PageNum pageCnt = 0; pageCnt < blk.Pages(); pageCnt++)
			{
				for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
				{
					TrackData trkD = td[trkCnt];

					if (trkD.TrkFxType == TrackData.FxType.None)
						continue;

					MedCmd cmd = blk.Cmd(plrFxLine, trkCnt, pageCnt);

					// Call MIDI command handler and skip normal cmd handling if
					// MIDI command handled by this routine
					if (trkD.TrkLastNoteMidi && MidiCommand(trkD, cmd))
						continue;

					byte data = cmd.GetDataB();

					switch (cmd.GetCmd())
					{
						// Arpeggio
						case 0x00:
						{
							if (cmd.GetData2() != 0)
							{
								NoteNum bas = trkD.TrkPrevNote;
								if (bas > 0x80)
									break;

								switch (plrPulseCtr % 3)
								{
									case 0:
									{
										bas += (NoteNum)(cmd.GetData2() >> 4);
										break;
									}

									case 1:
									{
										bas += (NoteNum)(cmd.GetData2() & 0x0f);
										break;
									}
								}

								bas += (NoteNum)(ss.GetPlayTranspose() - 1 + trkD.TrkSTransp);
								int freq = (int)GetNoteFrequency(bas, trkD.TrkFineTune);
								trkD.TrkArpAdjust = freq - trkD.TrkFrequency;	// Arpeggio difference

								trkD.VisualInfo.NoteNumber = bas > 71 ? (byte)71 : bas;
								worker.VirtualChannels[trkCnt].SetVisualInfo(trkD.VisualInfo);
							}
							break;
						}

						// Slide up (once)
						case 0x11:
						{
							if (plrPulseCtr != 0)
								break;

							DoCmd1(trkD, cmd);
							break;
						}

						// Slide up
						case 0x01:
						{
							if ((plrPulseCtr == 0) && ss.GetSlide1st())
								break;

							DoCmd1(trkD, cmd);
							break;
						}

						// Slide down (once)
						case 0x12:
						{
							if (plrPulseCtr != 0)
								break;

							DoCmd2(trkD, cmd);
							break;
						}

						// Slide down
						case 0x02:
						{
							if ((plrPulseCtr == 0) && ss.GetSlide1st())
								break;

							DoCmd2(trkD, cmd);
							break;
						}

						// Portamento
						case 0x03:
						{
							if ((plrPulseCtr == 0) && ss.GetSlide1st())
								break;

							DoPortamento(trkD);
							break;
						}

						// Volume slide
						case 0x0d:
						case 0x0a:
						case 0x06:		// (with vibrato)
						case 0x05:		// (or portamento)
						{
							if ((plrPulseCtr == 0) && ss.GetSlide1st())
								break;

							if ((data & 0xf0) != 0)
							{
								trkD.TrkPrevVol += (byte)((data >> 4) * 2);
								if (trkD.TrkPrevVol > 128)
									trkD.TrkPrevVol = 128;
							}
							else
							{
								if (((data & 0x0f) * 2) > trkD.TrkPrevVol)
									trkD.TrkPrevVol = 0;
								else
									trkD.TrkPrevVol -= (byte)((data & 0x0f) * 2);
							}

							if (cmd.GetCmd() == 0x06)
								DoVibrato(trkD);
							else
							{
								if (cmd.GetCmd() == 0x05)
									DoPortamento(trkD);
							}
							break;
						}

						// Vibrato (deeper)
						case 0x04:
						{
							trkD.TrkVibShift = 5;
							VibCont(trkD, data);
							break;
						}

						// Vibrato (shallower)
						case 0x14:
						{
							trkD.TrkVibShift = 6;
							VibCont(trkD, data);
							break;
						}

						// Simple pulse vibrato
						case 0x13:
						{
							if (plrPulseCtr < 3)
								trkD.TrkVibrAdjust = -data;

							break;
						}

						// Cut note
						case 0x18:
						{
							if (plrPulseCtr == data)
								trkD.TrkPrevVol = 0;

							break;
						}

						// Volume slide up (small)
						case 0x1a:
						{
							if (plrPulseCtr == 0)
							{
								byte incr = (byte)(data + (cmd.GetData2() >= 0x80 ? 1 : 0));

								if ((trkD.TrkPrevVol + incr) < 128)
									trkD.TrkPrevVol += incr;
								else
									trkD.TrkPrevVol = 128;
							}
							break;
						}

						// Volume slide down (small)
						case 0x1b:
						{
							if (plrPulseCtr == 0)
							{
								byte decr = (byte)(data - (cmd.GetData2() >= 0x80 ? 1 : 0));

								if (trkD.TrkPrevVol > decr)
									trkD.TrkPrevVol -= decr;
								else
									trkD.TrkPrevVol = 0;
							}
							break;
						}

						// Misc retrig commands
						case 0x0f:
						{
							switch (data)
							{
								case 0xf1:
								case 0xf2:
								{
									if (plrPulseCtr == 3)
										PlayFxNote(trkCnt, trkD);

									break;
								}

								case 0xf3:
								{
									if ((plrPulseCtr == 2) || (plrPulseCtr == 4))
										PlayFxNote(trkCnt, trkD);

									break;
								}

								case 0xf4:
								{
									if ((ss.GetTempoTpl() / 3) == plrPulseCtr)
										PlayFxNote(trkCnt, trkD);

									break;
								}

								case 0xf5:
								{
									if (((ss.GetTempoTpl() * 2) / 3) == plrPulseCtr)
										PlayFxNote(trkCnt, trkD);

									break;
								}

								case 0xf7:
									break;
							}
							break;
						}

						// Note delay/retrig
						case 0x1f:
						{
							if ((data >> 4) != 0)
							{
								// There's note delay specified
								if (plrPulseCtr < (data >> 4))
									break;		// Delay still going on...

								if (plrPulseCtr == (data >> 4))
								{
									PlayFxNote(trkCnt, trkD);
									break;
								}
							}

							if (((data & 0x0f) != 0) && ((plrPulseCtr % (data & 0x0f)) == 0))
								PlayFxNote(trkCnt, trkD);

							break;
						}

						// Change sample position
						case 0x20:
						{
							if ((plrPulseCtr == 0) && (data != 0))
								ChangeSamplePosition((uint)trkCnt, cmd.GetData());

							break;
						}

						// Slide up (const. rate)
						case 0x21:
						{
							trkD.TrkFrequency += (trkD.TrkFrequency * data) >> 11;

							if (trkD.TrkFrequency > 65535)
								trkD.TrkFrequency = 65535;

							break;
						}

						// Slide down (const. rate)
						case 0x22:
						{
							if ((((trkD.TrkFrequency * data) >> 11) + 1) < trkD.TrkFrequency)
								trkD.TrkFrequency -= (trkD.TrkFrequency * data) >> 11;
							else
								trkD.TrkFrequency = 1;

							break;
						}

						// Change sample position II (relative to sample length)
						case 0x29:
						{
							if (plrPulseCtr == 0)
							{
								Sample smp = plrSong.GetSample(trkD.TrkPrevINum);
								byte div = cmd.GetData2();

								if (div == 0)
									div = 0x10;		// Default divisor is 16

								if ((smp != null) && !smp.IsSynthSound() && (data < div))
								{
									int len = (int)smp.GetLength();
									SetSamplePosition((uint)trkCnt, (data * len) / div);
								}
							}
							break;
						}
					}
				}
			}

			for (TrackNum trkCnt = 0; trkCnt < blk.Tracks(); trkCnt++)
				UpdateFreqVolPan(ss, trkCnt);
		}



		/********************************************************************/
		/// <summary>
		/// Will recalculate the volume adjustments
		/// </summary>
		/********************************************************************/
		public void RecalcVolAdjust()
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Stops the channel given
		/// </summary>
		/********************************************************************/
		protected override void MuteChannel(uint chNum)
		{
			TrackData trkd = td[chNum];
			trkd.TrkSy.SynthType = SynthData.SyType.None;
			byte prevMN = trkd.TrkPrevMidiN;

			if (prevMN != 0)
				trkd.TrkPrevMidiN = 0xff;
			else
				base.MuteChannel(chNum);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoDelayRetrig(TrackData trkD)
		{
			trkD.TrkNoteOffCnt = (int)trkD.TrkInitHold;

			if (trkD.TrkNoteOffCnt == 0)
				trkD.TrkNoteOffCnt = -1;

			trkD.TrkFxType = TrackData.FxType.NoPlay;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoCmd1(TrackData trkD, MedCmd cmd)
		{
			if (trkD.TrkFrequency > 0)
			{
				int div = 3579545 * 256 / trkD.TrkFrequency - cmd.GetData();
				if (div > 0)
					trkD.TrkFrequency = 3579545 * 256 / div;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoCmd2(TrackData trkD, MedCmd cmd)
		{
			if (trkD.TrkFrequency > 0)
			{
				int div = 3579545 * 256 / trkD.TrkFrequency + cmd.GetData();
				if (div > 0)
					trkD.TrkFrequency = 3579545 * 256 / div;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoPortamento(TrackData trkD)
		{
			if ((trkD.TrkPortTargetFreq == 0) || (trkD.TrkFrequency <= 0))
				return;

			int newFreq = trkD.TrkFrequency;
			int div = 3579545 * 256 / newFreq;

			if (trkD.TrkFrequency > trkD.TrkPortTargetFreq)
			{
				div += trkD.TrkPortSpeed;
				if (div != 0)
				{
					newFreq = 3579545 * 256 / div;

					if (newFreq <= trkD.TrkPortTargetFreq)
					{
						newFreq = trkD.TrkPortTargetFreq;
						trkD.TrkPortTargetFreq = 0;
					}
				}
			}
			else
			{
				if (div > trkD.TrkPortSpeed)
				{
					div -= trkD.TrkPortSpeed;
					newFreq = 3579545 * 256 / div;
				}
				else
					newFreq = trkD.TrkPortTargetFreq;

				if (newFreq >= trkD.TrkPortTargetFreq)
				{
					newFreq = trkD.TrkPortTargetFreq;
					trkD.TrkPortTargetFreq = 0;
				}
			}

			trkD.TrkFrequency = newFreq;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VibCont(TrackData trkD, byte data)
		{
			if ((plrPulseCtr == 0) && (data != 0))
			{
				// Check data on pulse #0 for possible new values
				if ((data & 0x0f) != 0)
					trkD.TrkVibSize = (byte)(data & 0x0f);		// New vibrato size

				if ((data >> 4) != 0)
					trkD.TrkVibSpeed = (byte)((data >> 4) * 2);	// New vibrato speed
			}

			DoVibrato(trkD);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoVibrato(TrackData trkD)
		{
			if (trkD.TrkFrequency > 0)
			{
				// Another piece of Amiga period emulation code
				int per = 3579545 / trkD.TrkFrequency;
				per += (sineTable[(trkD.TrkVibOffs >> 2) & 0x1f] * trkD.TrkVibSize) >> trkD.TrkVibShift;

				if (per > 0)
					trkD.TrkVibrAdjust = 3579545 / per - trkD.TrkFrequency;
			}

			trkD.TrkVibOffs += trkD.TrkVibSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// Will enable or disable a single track
		/// </summary>
		/********************************************************************/
		private void EnableTrack(TrackNum tNum, bool en)
		{
			if (!(plrTrackEnabled[tNum] = en))
				MuteChannel((uint)tNum);
		}



		/********************************************************************/
		/// <summary>
		/// Will tell if a specified track is enabled or disabled
		/// </summary>
		/********************************************************************/
		private bool IsTrackEnabled(TrackNum tNum)
		{
			return plrTrackEnabled[tNum];
		}



		/********************************************************************/
		/// <summary>
		/// Will trig a note and begin playing it
		/// </summary>
		/********************************************************************/
		private void PlrPlayNote(TrackNum trk, NoteNum note, InstNum iNum)
		{
			TrackData trkD = td[trk];

			if (!plrSong.InstrSlotUsed(iNum) || (trk >= Constants.MaxTracks))
				return;

			Instr currI = plrSong.GetInstr(iNum);
			if ((currI.flags & Instr.Flag.Disabled) != 0)
			{
				MuteChannel((uint)trk);
				return;
			}

			if (note < 0x80)
			{
				int nt = note;
				nt += plrSong.CurrSS().GetPlayTranspose();
				nt += currI.GetTransp();

				while (nt >= 0x80)
					nt -= 12;

				while (nt < 0)
					nt += 12;

				note = (NoteNum)nt;
			}

			if ((trkD.TrkPrevMidiN > 0) && (trkD.TrkPrevMidiN <= 0x80))
				trkD.TrkPrevMidiN = 0;

			if (currI.IsMidi())
			{
				trkD.TrkLastNoteMidi = true;
				return;
			}

			if (trk > plrChannels)
				return;

			if (note < 0x80)
			{
				while (note > 71)
					note -= 12;
			}

			trkD.TrkDecay = trkD.TrkInitDecay;
			trkD.TrkFadeSpeed = 0;
			trkD.TrkVibOffs = 0;
			trkD.TrkFrequency = (int)GetInstrNoteFreq((NoteNum)(note + 1), currI);
			trkD.TrkLastNoteMidi = false;

			SynthData sy = trkD.TrkSy;

			if (plrSong.GetSample(iNum).IsSynthSound())
			{
				// Find sample/synth type
				//
				// If next sound is synth sound, and the previous wasn't, prepare synth play
//				if (sy.SynthType != SynthData.SyType.Synth)		// TN: Uncommented, so when playing the same note several times, actual got retrigged. This can e.g. be heard in Parasol Stars in the beginning
					PrepareSynthSound((uint)trk);

				if (plrSong.GetSample(iNum).GetLength() != 0)
					sy.SynthType = SynthData.SyType.Hybrid;
				else
					sy.SynthType = SynthData.SyType.Synth;
			}
			else
				sy.SynthType = SynthData.SyType.None;

			if (sy.SynthType != SynthData.SyType.Synth)
			{
				// An ordinary sample or hybrid
				Play((uint)trk, note, iNum, plrSong.GetSample(iNum), trkD.TrkSOffset, currI.GetRepeat(), currI.GetRepeatLen(),
					(((currI.flags & Instr.Flag.Loop) != 0) ? PlayFlag.Loop : PlayFlag.None) |
					(((trkD.TrkMiscFlags & TrackData.MiscFlag.Backwards) != 0) ? PlayFlag.Backwards : PlayFlag.None) |
					(((currI.flags & Instr.Flag.PingPong) != 0) ? PlayFlag.PingPongLoop : PlayFlag.None),
					trkD.VisualInfo);
			}

			if (sy.SynthType != SynthData.SyType.None)
			{
				ClearSynth(trkD, iNum);

				sy.NoteNumber = note;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear synth variables
		/// </summary>
		/********************************************************************/
		private void ClearSynth(TrackData trkD, InstNum iNum)
		{
			SynthData sy = trkD.TrkSy;

			sy.NoteNumber = 0;
			sy.ArpOffs = 0;
			sy.ArpStart = 0;
			sy.VolXCnt = 0;
			sy.WfXCnt = 0;
			sy.VolCmdPos = 0;

			if ((trkD.TrkMiscFlags & TrackData.MiscFlag.NoSynthWfPtrReset) == 0)
				sy.WfCmdPos = 0;

			sy.VolWait = 0;
			sy.WfWait = 0;
			sy.VibSpeed = 0;
			sy.VibDep = 0;
			sy.VibWfNum = 0;
			sy.VibOffs = 0;
			sy.PeriodChange = 0;
			sy.VolChgSpeed = 0;
			sy.WfChgSpeed = 0;
			sy.VolXSpeed = plrSong.GetSynthSound(iNum).GetVolSpeed();
			sy.WfXSpeed = plrSong.GetSynthSound(iNum).GetWfSpeed();
			sy.EnvWfNum = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Extract all the instrument info from the track
		/// </summary>
		/********************************************************************/
		private void ExtractInstrData(TrackData trkD, Instr currI)
		{
			trkD.TrkPrevVol = (byte)currI.GetVol();
			trkD.TrkInitHold = currI.GetHold();
			trkD.TrkInitDecay = currI.GetDecay();
			trkD.TrkSTransp = currI.GetTransp();
			trkD.TrkFineTune = currI.GetFineTune();
			trkD.TrkSOffset = 0;
			trkD.TrkMiscFlags = TrackData.MiscFlag.None;
		}



		/********************************************************************/
		/// <summary>
		/// Will retrig the current note on the track
		/// </summary>
		/********************************************************************/
		private void PlayFxNote(TrackNum trkNum, TrackData trkD)
		{
			if (trkD.TrkCurrNote != 0)
			{
				if (trkD.TrkNoteOffCnt >= 0)
					trkD.TrkNoteOffCnt += plrPulseCtr;
				else
				{
					if (trkD.TrkInitHold != 0)
						trkD.TrkNoteOffCnt = (int)trkD.TrkInitHold;
					else
						trkD.TrkNoteOffCnt = -1;
				}

				PlrPlayNote(trkNum, (byte)(trkD.TrkCurrNote - 1), trkD.TrkPrevINum);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set/change the frequency, volume and panning
		/// </summary>
		/********************************************************************/
		private void UpdateFreqVolPan(SubSong ss, TrackNum trkNum)
		{
			TrackData trkD = td[trkNum];

			if (trkD.TrkFxType != TrackData.FxType.Normal)
				return;

			SetChannelFreq((uint)trkNum, (trkD.TrkSy.SynthType == SynthData.SyType.None ? trkD.TrkFrequency : SynthHandler((uint)trkNum, trkD.TrkPrevINum, trkD, plrSong.GetSynthSound(trkD.TrkPrevINum))) + trkD.TrkArpAdjust + trkD.TrkVibrAdjust);

			byte usedVolume;

			if (trkD.TrkTempVol != 0)
			{
				usedVolume = (byte)(trkD.TrkTempVol - 1);
				trkD.TrkTempVol = 0;
			}
			else
				usedVolume = trkD.TrkPrevVol;

			SetChannelVolPan((uint)trkNum, (ushort)((usedVolume * ss.GetTrackVol(trkNum) * ss.GetMasterVol()) / (64 * 64)), (short)ss.GetTrackPan(trkNum));

			trkD.TrkArpAdjust = 0;
			trkD.TrkVibrAdjust = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the command and see if it's a MIDI command
		/// </summary>
		/********************************************************************/
		private bool MidiCommand(TrackData trkD, MedCmd cmd)
		{
			switch (cmd.GetCmd())
			{
				case 0x00:		// Midi controller
				case 0x01:		// Pitch bend up
				case 0x02:		// Pitch bend down
				case 0x03:		// Set pitch bender
				case 0x13:		// -""-
				case 0x04:		// Modulation wheel
				case 0x05:		// Set controller
				case 0x0a:		// Polyphonic aftertouch
				case 0x0d:		// Channel aftertouch
				case 0x0e:		// Panpot
				case 0x17:		// Set volume
					return true;

				case 0x0f:		// Miscellaneous
				{
					switch (cmd.GetDataB())
					{
						case 0xfa:		// Hold pedal on
						case 0xfb:		// Hold pedal off
							return true;
					}
					break;
				}

				default:
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Handles all the synth sounds
		/// </summary>
		/********************************************************************/
		private int SynthHandler(uint chNum, InstNum instNum, TrackData trkD, SynthSound snd)
		{
			SynthData sy = trkD.TrkSy;

			// Make sure that the synth sound hasn't disappeared meanwhile...
			if ((snd == null) || !snd.IsSynthSound())
			{
				sy.SynthType = SynthData.SyType.None;
				MuteChannel(chNum);
				return 0;
			}

			if (sy.VolXCnt-- <= 1)
			{
				sy.VolXCnt = (int)sy.VolXSpeed;		// Reset volume execution counter

				if (sy.VolChgSpeed != 0)
				{
					// CHU or CHD is active
					sy.Vol += (int)sy.VolChgSpeed;

					if (sy.Vol < 0)
						sy.Vol = 0;
					else
					{
						if (sy.Vol > 64)
							sy.Vol = 64;
					}
				}

				if (sy.EnvWfNum != 0)
				{
					if ((sy.EnvWfNum == 1) && (snd.GetLength() != 0))
					{
						;
					}
					else
					{
						if (sy.EnvWfNum <= snd.Count)
						{
							// Envelope wave's been set
							sy.Vol = (snd[sy.EnvWfNum - 1].SyWfData[sy.EnvCount] + 128) >> 2;

							if (++sy.EnvCount >= 128)
							{
								sy.EnvCount = 0;

								if (!sy.EnvLoop)
									sy.EnvWfNum = 0;
							}
						}
					}
				}

				if ((sy.VolWait == 0) || (--sy.VolWait == 0))
				{
					// Check WAI
					bool endCycle = false;
					int instCount = 0;		// Instruction count prevents deadlocking

					while (!endCycle && (++instCount < 128))
					{
						byte cmd = snd.GetVolData(sy.VolCmdPos++);

						if (cmd < 0x80)
						{
							sy.Vol = cmd;

							if (sy.Vol > 64)
								sy.Vol = 64;

							break;			// Also end cycle
						}

						switch (cmd)
						{
							case SynthSound.CmdJmp:
							{
								sy.VolCmdPos = snd.GetVolData(sy.VolCmdPos);
								if (sy.VolCmdPos > 127)
									sy.VolCmdPos = 0;

								break;
							}

							case SynthSound.CmdSpd:
							{
								sy.VolXSpeed = snd.GetVolData(sy.VolCmdPos++);
								break;
							}

							case SynthSound.CmdWai:
							{
								sy.VolWait = snd.GetVolData(sy.VolCmdPos++);
								endCycle = true;
								break;
							}

							case SynthSound.CmdChu:
							{
								sy.VolChgSpeed = snd.GetVolData(sy.VolCmdPos++);
								break;
							}

							case SynthSound.CmdChd:
							{
								sy.VolChgSpeed = (uint)-snd.GetVolData(sy.VolCmdPos++);
								break;
							}

							case SynthSound.VolCmdJws:
							{
								sy.WfWait = 0;
								sy.WfCmdPos = snd.GetVolData(sy.VolCmdPos++);
								break;
							}

							case SynthSound.VolCmdEn1:
							{
								sy.EnvWfNum = (uint)(snd.GetVolData(sy.VolCmdPos++) + 1);
								sy.EnvLoop = false;			// One-shot envelope
								sy.EnvCount = 0;
								break;
							}

							case SynthSound.VolCmdEn2:
							{
								sy.EnvWfNum = (uint)(snd.GetVolData(sy.VolCmdPos++) + 1);
								sy.EnvLoop = true;			// Looped envelope
								sy.EnvCount = 0;
								break;
							}

							case SynthSound.CmdRes:
							{
								sy.EnvWfNum = 0;
								break;
							}

							case SynthSound.CmdEnd:
							case SynthSound.CmdHlt:
							{
								sy.VolCmdPos--;
								goto default;
							}

							default:
							{
								endCycle = true;
								break;
							}
						}
					}
				}
			}

			trkD.TrkTempVol = (byte)(((sy.Vol * trkD.TrkPrevVol) >> 6) + 1);

			if (sy.WfXCnt-- <= 1)
			{
				sy.WfXCnt = (int)sy.WfXSpeed;

				if (sy.WfChgSpeed != 0)
					sy.PeriodChange += (int)sy.WfChgSpeed;		// CHU or CHD active

				if ((sy.WfWait == 0) || (--sy.WfWait == 0))
				{
					// Check WAI
					bool endCycle = false;
					int instCount = 0;

					while (!endCycle && (++instCount < 128))
					{
						byte cmd = snd.GetWfData(sy.WfCmdPos++);

						if (cmd < 0x80)
						{
							// Crash prevention
							if (cmd < snd.Count)
							{
								SynthWf swf = snd[cmd];
								SetSynthWaveform(chNum, swf.SyWfData, swf.SyWfLength * 2);
							}
							break;		// Also end cycle
						}

						switch (cmd)
						{
							case SynthSound.WfCmdVwf:
							{
								sy.VibWfNum = (uint)(snd.GetWfData(sy.WfCmdPos++) + 1);
								break;
							}

							case SynthSound.CmdJmp:
							{
								sy.WfCmdPos = snd.GetWfData(sy.WfCmdPos);
								if (sy.WfCmdPos > 127)
									sy.WfCmdPos = 0;

								break;
							}

							case SynthSound.WfCmdArp:
							{
								sy.ArpOffs = sy.WfCmdPos;
								sy.ArpStart = sy.WfCmdPos;

								// Scan until next command (preferable ARE) found
								while (snd.GetWfData(sy.WfCmdPos) < 0x80)
									sy.WfCmdPos++;

								break;
							}

							case SynthSound.CmdSpd:
							{
								sy.WfXSpeed = snd.GetWfData(sy.WfCmdPos++);
								break;
							}

							case SynthSound.CmdWai:
							{
								sy.WfWait = snd.GetWfData(sy.WfCmdPos++);
								endCycle = true;
								break;
							}

							case SynthSound.WfCmdVbd:
							{
								sy.VibDep = snd.GetWfData(sy.WfCmdPos++);
								break;
							}

							case SynthSound.WfCmdVbs:
							{
								sy.VibSpeed = snd.GetWfData(sy.WfCmdPos++);
								break;
							}

							case SynthSound.CmdChd:
							{
								sy.WfChgSpeed = snd.GetWfData(sy.WfCmdPos++);
								break;
							}

							case SynthSound.CmdChu:
							{
								sy.WfChgSpeed = (uint)-snd.GetWfData(sy.WfCmdPos++);
								break;
							}

							case SynthSound.CmdRes:
							{
								sy.PeriodChange = 0;
								break;
							}

							case SynthSound.WfCmdJvs:
							{
								sy.VolCmdPos = snd.GetWfData(sy.WfCmdPos++);
								if (sy.VolCmdPos > 127)
									sy.VolCmdPos = 0;

								sy.VolWait = 0;
								break;
							}

							case SynthSound.CmdEnd:
							case SynthSound.CmdHlt:
							{
								sy.WfCmdPos--;
								goto default;
							}

							default:
							{
								endCycle = true;
								break;
							}
						}
					}
				}
			}

			int currFreq = trkD.TrkFrequency;

			trkD.VisualInfo.NoteNumber = sy.NoteNumber;
			trkD.VisualInfo.SampleNumber = (byte)instNum;
			worker.VirtualChannels[chNum].SetVisualInfo(trkD.VisualInfo);

			// Arpeggio
			if (sy.ArpOffs != 0)
			{
				trkD.VisualInfo.NoteNumber = (byte)(sy.NoteNumber + snd.GetWfData(sy.ArpOffs));

				currFreq = (int)GetNoteFrequency((byte)(sy.NoteNumber + snd.GetWfData(sy.ArpOffs)), trkD.TrkFineTune);
				if (snd.GetWfData(++sy.ArpOffs) >= 0x80)
					sy.ArpOffs = sy.ArpStart;
			}

			// Vibrato
			int currPeriodChange = sy.PeriodChange;
			if (sy.VibDep != 0)
			{
				if ((sy.VibWfNum == 1) && (snd.GetLength() != 0))
				{
					;
				}
				else
				{
					if (sy.VibWfNum <= snd.Count)
					{
						sbyte[] vibWave = ((sy.VibWfNum != 0) && (sy.VibWfNum <= snd.Count)) ? snd[sy.VibWfNum - 1].SyWfData : sineTable;
						currPeriodChange += (vibWave[(sy.VibOffs >> 4) & 0x1f] * sy.VibDep) / 256;
						sy.VibOffs += sy.VibSpeed;
					}
				}
			}

			if ((currPeriodChange != 0) && (currFreq != 0))
			{
				int newPer = 3579545 / currFreq + currPeriodChange;

				if (newPer < 113)
					newPer = 113;

				currFreq = 3579545 / newPer;
			}

			return currFreq;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool PSeqCmdHandler(OctaMedWorker worker, PlaySeqEntry pse)
		{
			if (pse.GetCmd() == PSeqCmd.Stop)
			{
				worker.plr.plrDelayedStop = true;
				return true;
			}

			return false;
		}
		#endregion
	}
}

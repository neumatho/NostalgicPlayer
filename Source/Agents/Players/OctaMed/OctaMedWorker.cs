/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Globalization;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OctaMedWorker : ModulePlayerAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ OctaMed.Agent1Id, ModuleType.Med },
			{ OctaMed.Agent2Id, ModuleType.OctaMed },
			{ OctaMed.Agent3Id, ModuleType.OctaMed_Professional4 },
			{ OctaMed.Agent4Id, ModuleType.OctaMed_Professional6 },
			{ OctaMed.Agent5Id, ModuleType.OctaMed_SoundStudio },
			{ OctaMed.Agent6Id, ModuleType.MedPacker }
		};

		#region ExtraPosInfo class
		private class ExtraPosInfo
		{
			public Tempo tempo;
			public readonly uint[] instVolumes = new uint[Constants.MaxInstr - 1];
		}
		#endregion

		private readonly ModuleType currentModuleType;

		private uint numSamples;
		private ushort numChannels;

		public Song sg;
		public Implementation.Player plr;

		private const int InfoSpeedLine = 3;
		private const int InfoTempoLine = 4;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OctaMedWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "med", "mmd0", "mmd1", "mmd2", "mmd3", "mmdc", "omed", "ocss" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleType checkType = TestModule(fileInfo);
			if (checkType == currentModuleType)
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => sg.CurrSS().GetSongName();



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => string.IsNullOrWhiteSpace(sg.GetAnnoText()) ? new string[0] : new[] { sg.GetAnnoText() };



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Song length
				case 0:
				{
					description = Resources.IDS_MED_INFODESCLINE0;
					value = SongLength.ToString();
					break;
				}

				// Used blocks
				case 1:
				{
					description = Resources.IDS_MED_INFODESCLINE1;
					value = sg.CurrSS().NumBlocks().ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_MED_INFODESCLINE2;
					value = numSamples.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_MED_INFODESCLINE3;
					value = sg.CurrSS().GetTempoTpl().ToString();
					break;
				}

				// Current tempo (Hz)
				case 4:
				{
					description = Resources.IDS_MED_INFODESCLINE4;
					value = PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture);
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;
				bool mixConv = false;		// Perform conversion from non-mix mode?
				bool eightChConv = false;	// Or eight channel mode?

				// Allocate needed objects
				sg = new Song(this);
				plr = new Implementation.Player(this);

				// Initialize member variables
				numChannels = 0;

				// Initialize the current header
				MmdHdr hdr0 = new MmdHdr();
				MmdHdr hdrX = new MmdHdr();

				MmdHdr currHdr = hdr0;
				byte markVersion;

				do
				{
					// Read the module header
					ReadMmdHeader(moduleStream, currHdr);
					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
						Cleanup();

						return AgentResult.Error;
					}

					markVersion = (byte)(hdr0.Id & 0x000000ff);

					// Seek to the song structure
					moduleStream.Seek(currHdr.SongOffs, SeekOrigin.Begin);

					// New (empty) sub-song
					sg.AppendNew(true);
					sg++;
					SubSong css = sg.CurrSS();

					for (uint cnt = 0; cnt < Constants.MaxInstr - 1; cnt++)
					{
						// Read the sample structure
						Mmd0Sample loadSmp = new Mmd0Sample();
						ReadMmd0Sample(moduleStream, loadSmp);
						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MED_ERR_LOADING_INSTRUMENTS;
							Cleanup();

							return AgentResult.Error;
						}

						// Set the instrument information
						Instr ci = sg.GetInstr(cnt);
						ci.SetRepeat((uint)loadSmp.Rep << 1);
						ci.SetRepeatLen((uint)loadSmp.RepLen << 1);
						ci.SetTransp(loadSmp.STrans);
						ci.SetInitVol((uint)loadSmp.Volume * 2);
						ci.SetMidiCh(loadSmp.MidiCh);
					}

					Mmd0SongData song0 = null;
					Mmd2SongData song2 = null;
					bool usesHexVol;

					if ((markVersion < '2') || (markVersion == 'C'))
					{
						// Type MMD0 or MMD1
						song0 = new Mmd0SongData();
						ReadMmd0Song(moduleStream, song0);

						if (song0.SongLen > 255)
						{
							errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
							Cleanup();

							return AgentResult.Error;
						}

						numSamples = Math.Min(song0.NumSamples, Constants.MaxInstr);

						if ((song0.Flags2 & MmdFlag2.Mix) == 0)
							mixConv = true;

						if ((song0.Flags & MmdFlag.VolHex) != 0)
							usesHexVol = true;
						else
							usesHexVol = false;

						if ((song0.Flags & MmdFlag.StSlide) != 0)
							css.SetSlide1st(true);

						if ((song0.Flags & MmdFlag.EightChannel) != 0)
							eightChConv = true;

						css.SetTempoBpm(song0.DefTempo);
						css.SetTempoTpl(song0.Tempo2);
						css.SetTempoLpb((ushort)((song0.Flags2 & MmdFlag2.BMask) + 1));
						css.SetTempoMode((song0.Flags2 & MmdFlag2.Bpm) != 0);
						css.SetPlayTranspose(song0.PlayTransp);
						css.SetNumChannels(4);
						css.SetAmigaFilter((song0.Flags & MmdFlag.FilterOn) != 0);

						for (int cnt = 0; cnt < 16; cnt++)
							css.SetTrackVol(cnt, song0.TrkVol[cnt]);

						css.SetMasterVol(song0.MasterVol);

						// No sections here...
						css.AppendNewSec(0);

						// Playing sequence...
						PlaySeq newPSeq = new PlaySeq();
						css.Append(newPSeq);

						for (int cnt = 0; cnt < song0.SongLen; cnt++)
							newPSeq.Add(new PlaySeqEntry(song0.PlaySeq[cnt]));
					}
					else
					{
						// MMD2 or MMD3
						song2 = new Mmd2SongData();
						ReadMmd2Song(moduleStream, song2);

						numSamples = Math.Min(song2.NumSamples, Constants.MaxInstr);

						if ((song2.Flags2 & MmdFlag2.Mix) == 0)
							mixConv = true;

						if ((song2.Flags & MmdFlag.VolHex) != 0)
							usesHexVol = true;
						else
							usesHexVol = false;

						if ((song2.Flags & MmdFlag.StSlide) != 0)
							css.SetSlide1st(true);

						if ((song2.Flags & MmdFlag.EightChannel) != 0)
							eightChConv = true;

						if ((song2.Flags3 & MmdFlag3.Stereo) != 0)
							css.SetStereo(true);

						if ((song2.Flags3 & MmdFlag3.FreePan) != 0)
							css.SetFreePan(true);

						if ((song2.Flags3 & MmdFlag3.Gm) != 0)
							css.SetGm(true);

						css.SetTempoBpm(song2.DefTempo);
						css.SetTempoTpl(song2.Tempo2);
						css.SetTempoLpb((ushort)((song2.Flags2 & MmdFlag2.BMask) + 1));
						css.SetTempoMode((song2.Flags2 & MmdFlag2.Bpm) != 0);
						css.SetPlayTranspose(song2.PlayTransp);
						css.SetNumChannels(song2.Channels != 0 ? song2.Channels : (ushort)4);
						css.SetAmigaFilter((song2.Flags & MmdFlag.FilterOn) != 0);
						css.SetVolAdjust((short)(song2.VolAdj != 0 ? song2.VolAdj : 100));

						//XX skal fjernes
						// We do not support echo and stereo separation
						if (song2.MixEchoType != 0)
							throw new NotSupportedException($"Echo not supported - {song2.MixEchoType}");

						if (song2.MixStereoSep != 0)
							throw new NotSupportedException($"Stereo separation not supported - {song2.MixStereoSep}");

						// Read track volumes
						TrackNum songTracks = Math.Min((ushort)Constants.MaxTracks, song2.NumTracks);
						moduleStream.Seek(song2.TrackVolsOffs, SeekOrigin.Begin);

						for (int cnt = 0; cnt < songTracks; cnt++)
							css.SetTrackVol(cnt, moduleStream.Read_UINT8());

						css.SetMasterVol(song2.MasterVol);

						// And track pans
						moduleStream.Seek(song2.TrackPansOffs, SeekOrigin.Begin);

						for (int cnt = 0; cnt < songTracks; cnt++)
							css.SetTrackPan(cnt, moduleStream.Read_UINT8());

						// Read the section table
						moduleStream.Seek(song2.SectionTableOffs, SeekOrigin.Begin);

						for (int cnt = 0; cnt < song2.NumSections; cnt++)
							css.AppendNewSec(moduleStream.Read_B_UINT16());

						// Read playing sequences
						moduleStream.Seek(song2.PlaySeqTableOffs, SeekOrigin.Begin);

						uint[] pSqTbl = new uint[song2.NumPlaySeqs];
						byte[] name = new byte[32];

						moduleStream.ReadArray_B_UINT32s(pSqTbl, 0, song2.NumPlaySeqs);

						for (int cnt = 0; cnt < song2.NumPlaySeqs; cnt++)
						{
							PlaySeq newSeq = new PlaySeq();

							css.Append(newSeq);
							moduleStream.Seek(pSqTbl[cnt], SeekOrigin.Begin);

							// Read PlaySeq name
							moduleStream.Read(name, 0, 32);
							name[31] = 0x00;

							// Get pointer to PlaySeq commands
							uint cmdPtr = moduleStream.Read_B_UINT32();

							// Skip reserved fields
							moduleStream.Seek(4, SeekOrigin.Current);

							newSeq.SetName(name);

							// Read PlaySeq length
							ushort seqLen = moduleStream.Read_B_UINT16();

							for (int cnt2 = 0; cnt2 < seqLen; cnt2++)
							{
								ushort seqNum = moduleStream.Read_B_UINT16();
								if (seqNum < 0x8000)
									newSeq.Add(new PlaySeqEntry(seqNum));
							}

							// Read commands, if any
							if (cmdPtr != 0)
							{
								moduleStream.Seek(cmdPtr, SeekOrigin.Begin);

								for (;;)
								{
									ushort offs = moduleStream.Read_B_UINT16();
									PSeqCmd cmdNum = (PSeqCmd)moduleStream.Read_UINT8();
									byte extraBytes = moduleStream.Read_UINT8();

									if ((offs == 0xffff) && (cmdNum == 0) && (extraBytes == 0))
										break;

									PlaySeqEntry pse = newSeq[offs];
									if (pse != null)
									{
										switch (cmdNum)
										{
											case PSeqCmd.Stop:
											{
												pse.SetCmd(PSeqCmd.Stop, 0);
												goto default;
											}

											default:
											{
												if (extraBytes != 0)
													moduleStream.Seek(extraBytes, SeekOrigin.Current);

												break;
											}

											case PSeqCmd.PosJump:
											{
												pse.SetCmd(PSeqCmd.PosJump, moduleStream.Read_B_UINT16());
												if (extraBytes > 2)
													moduleStream.Seek(extraBytes - 2, SeekOrigin.Current);

												break;
											}
										}
									}
								}
							}
						}
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
						Cleanup();

						return AgentResult.Error;
					}

					// Read blocks
					moduleStream.Seek(currHdr.BlocksOffs, SeekOrigin.Begin);

					uint numBlocks;

					if ((markVersion <= '1') || (markVersion == 'C'))
					{
						// MMD0 or MMD1
						numBlocks = song0.NumBlocks;
					}
					else
					{
						// MMD2 or MMD3
						numBlocks = song2.NumBlocks;
					}

					uint[] blkArray = new uint[numBlocks];

					// Read all the block pointers
					moduleStream.ReadArray_B_UINT32s(blkArray, 0, (int)numBlocks);

					if ((markVersion == '0') || (markVersion == 'C'))
					{
						// MMD0 or MMDC
						for (int cnt = 0; cnt < numBlocks; cnt++)
						{
							// Seek to the block data
							moduleStream.Seek(blkArray[cnt], SeekOrigin.Begin);

							// Get the block header
							TrackNum tracks = moduleStream.Read_UINT8();
							LineNum lines = moduleStream.Read_UINT8() + 1;
							numChannels = Math.Max((ushort)tracks, numChannels);

							// Allocate the block
							MedBlock blk = new MedBlock(lines, tracks);

							sg.CurrSS().Append(blk);

							// Allocate byte array holding the block data to be parsed
							int size = tracks * lines * 3;
							byte[] blockData = new byte[size];

							if (markVersion == '0')
							{
								moduleStream.Read(blockData, 0, size);

								if (moduleStream.EndOfStream)
								{
									errorMessage = Resources.IDS_MED_ERR_LOADING_PATTERNS;
									Cleanup();

									return AgentResult.Error;
								}
							}
							else
							{
								// This code is based on libxmp by Alice
								for (uint j = 0; j < size; )
								{
									uint pack = moduleStream.Read_UINT8();

									if (moduleStream.EndOfStream)
									{
										errorMessage = Resources.IDS_MED_ERR_LOADING_PATTERNS;
										Cleanup();

										return AgentResult.Error;
									}

									if ((pack & 0x80) != 0)
									{
										// Run of 0
										j += 256 - pack;
										continue;
									}

									// Uncompressed block
									pack++;
									if (pack > (size - j))
										pack = (uint)(size - j);

									if (moduleStream.Read(blockData, (int)j, (int)pack) < pack)
									{
										errorMessage = Resources.IDS_MED_ERR_LOADING_PATTERNS;
										Cleanup();

										return AgentResult.Error;
									}

									j += pack;
								}
							}

							// Parse the block data
							int offset = 0;
							for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
							{
								for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
								{
									Span<byte> mmd0Note = new Span<byte>(blockData, offset, 3);

									MedNote dn = blk.Note(lineCnt, trkCnt);
									dn.NoteNum = (byte)(mmd0Note[0] & 0x3f);
									dn.InstrNum = (byte)((mmd0Note[1] >> 4) | (((mmd0Note[0] & 0x80) != 0) ? 0x10 : 0x00) | (((mmd0Note[0] & 0x40) != 0) ? 0x20 : 0x00));

									if ((mmd0Note[1] & 0x0f) == 0)
										blk.Cmd(lineCnt, trkCnt, 0).SetCmdData(0, 0, mmd0Note[2]);
									else
										blk.Cmd(lineCnt, trkCnt, 0).SetCmdData((byte)(mmd0Note[1] & 0x0f), mmd0Note[2], 0);

									offset += 3;
								}
							}
						}
					}
					else
					{
						// MMD1, MMD2 or MMD3
						Mmd1BlockInfo blkInfo = new Mmd1BlockInfo();

						for (int cnt = 0; cnt < numBlocks; cnt++)
						{
							// Seek to the block data
							moduleStream.Seek(blkArray[cnt], SeekOrigin.Begin);

							// Read the block header
							TrackNum tracks = moduleStream.Read_B_UINT16();
							LineNum lines = moduleStream.Read_B_UINT16() + 1;
							uint blockInfoOffs = moduleStream.Read_B_UINT32();

							// More than 64 tracks.. skip the high tracks
							int skipTracks = 0;

							if (tracks > Constants.MaxTracks)
							{
								skipTracks = tracks - Constants.MaxTracks;
								tracks = Constants.MaxTracks;
							}

							numChannels = Math.Max((ushort)tracks, numChannels);

							// Read the block info
							if (blockInfoOffs != 0)
							{
								long notePos = moduleStream.Position;
								moduleStream.Seek(blockInfoOffs, SeekOrigin.Begin);
								ReadMmd1BlockInfo(moduleStream, blkInfo);

								// Back to notes
								moduleStream.Seek(notePos, SeekOrigin.Begin);
							}

							MedBlock blk = new MedBlock(lines, tracks);

							sg.CurrSS().Append(blk);

							for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
							{
								for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
								{
									byte[] mmdNote = new byte[4];
									MedNote dn = blk.Note(lineCnt, trkCnt);
									moduleStream.Read(mmdNote, 0, 4);

									if (moduleStream.EndOfStream)
									{
										errorMessage = Resources.IDS_MED_ERR_LOADING_PATTERNS;
										Cleanup();

										return AgentResult.Error;
									}

									if (mmdNote[0] <= Constants.Note44k)
										dn.NoteNum = mmdNote[0];

									dn.InstrNum = mmdNote[1];

									// Convert cmds 00 and 19 (if only one cmd byte)
									if ((mmdNote[2] == 0x19) || (mmdNote[2] == 0x00))
										blk.Cmd(lineCnt, trkCnt, 0).SetCmdData(mmdNote[2], 0, mmdNote[3]);
									else
										blk.Cmd(lineCnt, trkCnt, 0).SetCmdData(mmdNote[2], mmdNote[3], 0);
								}

								if (skipTracks != 0)
									moduleStream.Seek(skipTracks * 4, SeekOrigin.Current);
							}

							if (blockInfoOffs != 0)
							{
								// Read and set block name
								if ((blkInfo.BlockName != 0) && (blkInfo.BlockNameLen != 0))
								{
									moduleStream.Seek(blkInfo.BlockName, SeekOrigin.Begin);

									byte[] tmpTxt = new byte[blkInfo.BlockNameLen];
									moduleStream.Read(tmpTxt, 0, (int)blkInfo.BlockNameLen);
									blk.SetName(tmpTxt);
								}

								// Read extra command pages
								if (blkInfo.PageTable != 0)
								{
									moduleStream.Seek(blkInfo.PageTable, SeekOrigin.Begin);

									PageNum numPages = moduleStream.Read_B_UINT16();
									blk.SetCmdPages(numPages + 1);

									// Skip reserved fields
									moduleStream.Seek(2, SeekOrigin.Current);

									uint[] pages = new uint[numPages];
									moduleStream.ReadArray_B_UINT32s(pages, 0, numPages);

									for (PageNum pageCnt = 0; pageCnt < numPages; pageCnt++)
									{
										moduleStream.Seek(pages[pageCnt], SeekOrigin.Begin);

										for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
										{
											for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
											{
												byte cmdNum = moduleStream.Read_UINT8();
												byte cmdArg = moduleStream.Read_UINT8();

												// Convert cmds 00 and 19 (if only one cmd byte)
												if ((cmdNum == 0x19) || (cmdNum == 0x00))
													blk.Cmd(lineCnt, trkCnt, pageCnt + 1).SetCmdData(cmdNum, 0, cmdArg);
												else
													blk.Cmd(lineCnt, trkCnt, pageCnt + 1).SetCmdData(cmdNum, cmdArg, 0);
											}
										}
									}
								}

								// Read extended command values
								if (blkInfo.CmdExtTable != 0)
								{
									// This song knows about the command 0c00xx kludge
									moduleStream.Seek(blkInfo.CmdExtTable, SeekOrigin.Begin);

									uint[] cmdExt = new uint[blk.Pages()];
									moduleStream.ReadArray_B_UINT32s(cmdExt, 0, blk.Pages());

									for (PageNum pCnt = 0; pCnt < blk.Pages(); pCnt++)
									{
										moduleStream.Seek(cmdExt[pCnt], SeekOrigin.Begin);

										for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
										{
											for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
											{
												MedCmd cmd = blk.Cmd(lineCnt, trkCnt, pCnt);
												byte arg2 = moduleStream.Read_UINT8();

												if ((cmd.GetCmd() == 0x00) || (cmd.GetCmd() == 0x19))
													cmd.SetData(arg2);
												else
													cmd.SetData2(arg2);
											}
										}
									}
								}
							}

							// Convert volume command 0C
							ScanConvertOldVolToNew cVol = new ScanConvertOldVolToNew(usesHexVol);
							cVol.DoBlock(blk);
						}
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
						Cleanup();

						return AgentResult.Error;
					}

					// Read ExpData
					if (currHdr.ExpDataOffs != 0)
					{
						moduleStream.Seek(currHdr.ExpDataOffs, SeekOrigin.Begin);

						Mmd0ExpData currExp = new Mmd0ExpData();
						ReadMmd0ExpData(moduleStream, currExp);

						// Read InstExt
						for (uint cnt = 0; cnt < Math.Min(currExp.InsTextEntries, Constants.MaxInstr); cnt++)
						{
							MmdInstrExt currIe = new MmdInstrExt();

							moduleStream.Seek(currExp.InsTextOffs + cnt * currExp.InsTextEntrySize, SeekOrigin.Begin);
							Instr ci = sg.GetInstr(cnt);

							if (currExp.InsTextEntrySize >= 2)
							{
								ci.SetHold(moduleStream.Read_UINT8());
								ci.SetDecay(moduleStream.Read_UINT8());
							}

							if (currExp.InsTextEntrySize >= 4)
							{
								moduleStream.Seek(1, SeekOrigin.Current);		// Skip MIDI suppress
								ci.SetFineTune(moduleStream.Read_INT8());
							}

							if (currExp.InsTextEntrySize >= 8)
							{
								currIe.DefaultPitch = moduleStream.Read_UINT8();
								currIe.InstrFlags = (InstrFlag)moduleStream.Read_UINT8();
								currIe.LongMidiPreset = moduleStream.Read_B_UINT16();

								ci.SetDefPitch(currIe.DefaultPitch);
								ci.flags |= (((currIe.InstrFlags & InstrFlag.ExtMidiPSet) != 0) ? Instr.Flag.MidiExtPSet : 0) |
								            (((currIe.InstrFlags & InstrFlag.Disabled) != 0) ? Instr.Flag.Disabled : 0);

								if ((currIe.InstrFlags & InstrFlag.Loop) != 0)
								{
									ci.flags |= Instr.Flag.Loop;
									if ((currIe.InstrFlags & InstrFlag.PingPong) != 0)
										ci.flags |= Instr.Flag.PingPong;
								}
							}
							else
							{
								if (ci.GetRepeatLen() > 2)
									ci.flags = Instr.Flag.Loop;
							}

							if (currExp.InsTextEntrySize >= 18)
							{
								currIe.OutputDevice = moduleStream.Read_UINT8();
								currIe.Reserved = moduleStream.Read_UINT8();
								currIe.LongRepeat = moduleStream.Read_B_UINT32();
								currIe.LongRepLen = moduleStream.Read_B_UINT32();

								ci.SetRepeat(currIe.LongRepeat);
								ci.SetRepeatLen(currIe.LongRepLen);
							}

							if (currExp.InsTextEntrySize >= 19)
							{
								byte vol = moduleStream.Read_UINT8();
								ci.SetInitVol(Math.Min(vol, (ushort)128));
							}

							if (currExp.InsTextEntrySize >= 20)
								moduleStream.Seek(1, SeekOrigin.Current);		// Skip the port number

							if (currExp.InsTextEntrySize >= 24)
								moduleStream.Seek(4, SeekOrigin.Current);		// Skip the midi bank

							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
								Cleanup();

								return AgentResult.Error;
							}
						}

						// Read InstInfo
						for (uint cnt = 0; cnt < Math.Min(currExp.InstInfoEntries, Constants.MaxInstr); cnt++)
						{
							moduleStream.Seek(currExp.InstInfoOffs + cnt * currExp.InstInfoEntrySize, SeekOrigin.Begin);

							if (currExp.InstInfoEntrySize >= 40)
							{
								byte[] name = new byte[42];
								moduleStream.Read(name, 0, 40);
								sg.GetInstr(cnt).SetName(name);
							}

							// apocalypse intro.mmd0 has an invalid value in InstInfoOffs, so the above
							// name will not be read and we will get an error here. This is not a critical
							// error, so I uncommented the check below
/*							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
								Cleanup();

								return AgentResult.Error;
							}*/
						}

						// Read song's name
						if (currExp.SongNameOffs != 0)
						{
							moduleStream.Seek(currExp.SongNameOffs, SeekOrigin.Begin);

							byte[] tmpTxt = new byte[currExp.SongNameLen];
							moduleStream.Read(tmpTxt, 0, (int)currExp.SongNameLen);
							css.SetSongName(tmpTxt);

							// unlimited yellow-grey.mmd1 has an invalid value in SongNameOffs, so the above
							// name will not be read and we will get an error here. This is not a critical
							// error, so I uncommented the check below
/*							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
								Cleanup();

								return AgentResult.Error;
							}
*/						}

						// Channel split in 5-8 channel modules
						if (eightChConv)
						{
							for (int cnt = 0; cnt < 4; cnt++)
							{
								if (currExp.ChannelSplit[cnt] == 0)
									css.SetNumChannels(css.GetNumChannels() + 1);
							}
						}

						// Read annotation text
						if (currExp.AnnoTextOffs != 0)
						{
							moduleStream.Seek(currExp.AnnoTextOffs, SeekOrigin.Begin);

							byte[] loadText = new byte[currExp.AnnoTextLength];
							moduleStream.Read(loadText, 0, (int)currExp.AnnoTextLength);
							sg.SetAnnoText(loadText);

/*							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_MED_ERR_LOADING_HEADER;
								Cleanup();

								return AgentResult.Error;
							}
*/						}

						moduleStream.Seek(currExp.NextHdr, SeekOrigin.Begin);
						currHdr = hdrX;		// Don't overwrite the first MMD header
					}
					else
					{
						if (sg.CurrInstr().GetRepeatLen() > 2)
							sg.CurrInstr().flags = Instr.Flag.Loop;
					}
				}
				while (hdr0.ExtraSongs-- != 0);

				// Read samples etc.
				if ((hdr0.SamplesOffs != 0) && (numSamples != 0))
				{
					moduleStream.Seek(hdr0.SamplesOffs, SeekOrigin.Begin);

					uint[] smpArray = new uint[numSamples];
					moduleStream.ReadArray_B_UINT32s(smpArray, 0, (int)numSamples);

					for (uint cnt = 0; cnt < numSamples; cnt++)
					{
						if (smpArray[cnt] == 0)
							continue;

						uint where = smpArray[cnt];
						moduleStream.Seek(where, SeekOrigin.Begin);

						MmdSampleHdr sHdr = new MmdSampleHdr(this, moduleStream, cnt, out errorMessage);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							Cleanup();

							return AgentResult.Error;
						}

						if (sHdr.IsSample())
						{
							Sample dest = sHdr.AllocSample();
							sHdr.ReadSampleData(moduleStream, dest);
							sg.SetSample(cnt, dest);
						}
						else
						{
							if (sHdr.IsSynth() || (sHdr.IsHybrid()))
							{
								bool result = sg.ReadSynthSound(cnt, moduleStream, sHdr.IsHybrid(), out errorMessage);
								if (!result)
								{
									if (!string.IsNullOrEmpty(errorMessage))
									{
										Cleanup();

										return AgentResult.Error;
									}

									sg.SetSample(cnt, null);
								}
								else
								{
									// TN: Bug fix. It seems like synth/hybrid sounds always have
									// full volume instead of using the value from the saved instrument.
									// This make sure that the Parasol Stars synth module can play correctly
									sg.GetInstr(cnt).SetInitVol(128);
								}
							}
						}

						if ((sHdr.GetNumBytes() != 0) && moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MED_ERR_LOADING_SAMPLES;
							Cleanup();

							return AgentResult.Error;
						}
					}
				}

				// Do the conversion for old 4-8 channel modules
				if (mixConv)
				{
					ScanSongConvertToMixMode cmm = new ScanSongConvertToMixMode();
					cmm.Do(sg, (markVersion == '0') || (markVersion == 'C'));
				}

				// And special 5-8 channel tempo conversion
				if (eightChConv)
				{
					ScanSongConvertTempo ct = new ScanSongConvertTempo();
					ct.DoSong(sg);
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Everything is loaded alright
			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			// Get the number of sub-songs
			uint numSongs = sg.NumSubSongs();

			// Now loop all the songs
			for (uint i = 0; i < numSongs; i++)
			{
				// Get the sub-song pointer
				SubSong ss = sg.GetSubSong(i);

				// Initialize the start tempo
				Tempo tempo = ss.GetTempo();
				ss.SetStartTempo(tempo);
			}

			return base.InitPlayer(out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			InitializeSound(songNumber);

			return base.InitSound(songNumber, durationInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			DurationInfo[] durations = CalculateDurationBySubSongs();

			return durations;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			plr.PlrCallBack();
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => numChannels;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo((int)sg.NumSubSongs(), 0);



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength
		{
			get
			{
				SubSong ss = sg.CurrSS();
				uint numSec = ss.NumSections();
				int len = 0;

				for (uint i = 0; i < numSec; i++)
				{
					PSeqNum seq = ss.Sect(i).Value;
					len += ss.PSeq(seq).Count;
				}

				return len;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			SubSong ss = sg.CurrSS();
			uint secNum = plr.plrPos.PSectPos();
			int pos = 0;

			for (uint i = 0; i < secNum; i++)
			{
				PSeqNum seq = ss.Sect(i).Value;
				pos += ss.PSeq(seq).Count;
			}

			return (int)(pos + plr.plrPos.PSeqPos());
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			SubSong ss = sg.CurrSS();
			uint numSec = ss.NumSections();
			ExtraPosInfo extraPosInfo = (ExtraPosInfo)positionInfo.ExtraInfo;

			uint i;
			PSeqNum seq;

			// Calculate the different sequence, section etc. positions
			for (i = 0; i < numSec; i++)
			{
				seq = ss.Sect(i).Value;
				int seqNum = ss.PSeq(seq).Count;

				if (position < seqNum)
					break;

				position -= seqNum;
			}

			// Now set the player position
			plr.plrPos.PSectPos(i);
			plr.plrPos.PSeqPos((uint)position);

			seq = ss.Sect(i).Value;
			PlaySeqEntry pse = ss.PSeq(seq)[position];
			plr.plrPos.Block(pse.Value);

			// Set the tempo
			ss.SetTempo(extraPosInfo.tempo);
			ChangePlayFreq();

			// Set instrument volumes (can be changed with Cxx effect)
			for (i = 0; i < Constants.MaxInstr - 1; i++)
				sg.GetInstr(i).SetVol(extraPosInfo.instVolumes[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override SampleInfo[] Samples
		{
			get
			{
				List<SampleInfo> result = new List<SampleInfo>();

				for (uint i = 0; i < numSamples; i++)
				{
					Instr inst = sg.GetInstr(i);
					Sample sample = inst.GetSample();
					SampleInfo sampleInfo;

					if (sample == null)
					{
						// Well, fill out an empty sample
						sampleInfo = new SampleInfo
						{
							Flags = SampleInfo.SampleFlags.None,
							Type = SampleInfo.SampleType.Sample,
							BitSize = 8,
							MiddleC = (int)plr.GetNoteFrequency(36, inst.GetFineTune()),
							Volume = (int)inst.GetInitVol() * 2,
							Panning = -1,
							Sample = null,
							Length = 0,
							LoopStart = (int)inst.GetRepeat(),
							LoopLength = (int)inst.GetRepeatLen()
						};
					}
					else
					{
						sampleInfo = new SampleInfo
						{
							BitSize = sample.Is16Bit() ? 16 : 8,
							MiddleC = (int)plr.GetNoteFrequency(36, inst.GetFineTune()),
							Volume = (int)inst.GetInitVol() * 2,
							Panning = -1,
							Length = (int)sample.GetLength(),
							LoopStart = (int)inst.GetRepeat(),
							LoopLength = (int)inst.GetRepeatLen()
						};

						if (sample.IsMultiOctave())
						{
							int oct;

							sampleInfo.MultiOctaveSamples = new SampleInfo.MultiOctaveInfo[8];

							for (oct = 0; oct < 6; oct++)
							{
								uint repeat = (uint)sampleInfo.LoopStart;
								uint repLen = (uint)sampleInfo.LoopLength;
								NoteNum note = (NoteNum)(oct * 12);

								sampleInfo.MultiOctaveSamples[oct + 1].Sample = new sbyte[2][];

								sampleInfo.MultiOctaveSamples[oct + 1].Sample[0] = sample.GetPlayBuffer(0, note, ref repeat, ref repLen);
								sampleInfo.MultiOctaveSamples[oct + 1].LoopStart = (int)repeat;
								sampleInfo.MultiOctaveSamples[oct + 1].LoopLength = (int)repLen;
								sampleInfo.MultiOctaveSamples[oct + 1].NoteAdd = sample.GetNoteDifference(note);

								if (sample.IsStereo())
									sampleInfo.MultiOctaveSamples[oct + 1].Sample[1] = sample.GetPlayBuffer(1, note, ref repeat, ref repLen);
							}

							// OctaMed only have 6 octaves, so the first the last will use the same buffer
							sampleInfo.MultiOctaveSamples[0] = sampleInfo.MultiOctaveSamples[1];
							sampleInfo.MultiOctaveSamples[7] = sampleInfo.MultiOctaveSamples[6];

							// Remember all samples
							sampleInfo.MultiOctaveAllSamples = new sbyte[2][][];

							List<(sbyte[] left, sbyte[] right)> samples = new List<(sbyte[] left, sbyte[] right)>();
							oct = 0;

							sbyte[] leftBuf;
							while ((leftBuf = sample.GetSampleBuffer(0, oct)) != null)
							{
								sbyte[] rightBuf = sample.GetSampleBuffer(1, oct);
								samples.Add((leftBuf, rightBuf));

								oct++;
							}

							sampleInfo.MultiOctaveAllSamples[0] = samples.Select(s => s.left).ToArray();
							if (sample.IsStereo())
								sampleInfo.MultiOctaveAllSamples[1] = samples.Select(s => s.right).ToArray();

							sampleInfo.Flags = SampleInfo.SampleFlags.MultiOctave;
						}
						else
						{
							sampleInfo.Sample = sample.GetSampleBuffer(0, 0);
							sampleInfo.SecondSample = sample.GetSampleBuffer(1, 0);
							sampleInfo.Flags = SampleInfo.SampleFlags.None;
						}

						// Find out the type of the sample
						if (sample.IsSynthSound())
						{
							if (sample.GetLength() == 0)
								sampleInfo.Type = SampleInfo.SampleType.Synth;
							else
								sampleInfo.Type = SampleInfo.SampleType.Hybrid;
						}
						else
							sampleInfo.Type = SampleInfo.SampleType.Sample;

						// Find out the loop information
						sampleInfo.Flags |= (inst.flags & Instr.Flag.Loop) != 0 ? SampleInfo.SampleFlags.Loop : SampleInfo.SampleFlags.None;
						if ((inst.flags & Instr.Flag.PingPong) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlags.PingPong;

						if (sample.IsStereo())
							sampleInfo.Flags |= SampleInfo.SampleFlags.Stereo;
					}

					sampleInfo.Name = inst.GetName();

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Duration calculation methods
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDurationCalculationBySubSong(int subSong)
		{
			InitializeSound(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Return extra information for the current position
		/// </summary>
		/********************************************************************/
		protected override object GetExtraPositionInfo()
		{
			ExtraPosInfo extraPosInfo = new ExtraPosInfo
			{
				tempo = new Tempo(sg.CurrSS().GetTempo())
			};

			for (uint cnt = 0; cnt < Constants.MaxInstr - 1; cnt++)
				extraPosInfo.instVolumes[cnt] = sg.GetInstr(cnt).GetVol();

			return extraPosInfo;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Set the playing frequency
		/// </summary>
		/********************************************************************/
		public void SetPlayingFrequency(float freq)
		{
			PlayingFrequency = freq;
		}



		/********************************************************************/
		/// <summary>
		/// Set the Amiga filter
		/// </summary>
		/********************************************************************/
		public void SetAmigaFilter(bool filter)
		{
			AmigaFilter = filter;
		}



		/********************************************************************/
		/// <summary>
		/// Tell that the position has changed
		/// </summary>
		/********************************************************************/
		public void ChangePosition()
		{
			OnPositionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Set the end reached flag
		/// </summary>
		/********************************************************************/
		public void SetEndReached()
		{
			OnEndReached();
		}



		/********************************************************************/
		/// <summary>
		/// Will tell NostalgicPlayer about the play frequency change
		/// </summary>
		/********************************************************************/
		public void ChangePlayFreq()
		{
			// Change the module info
			OnModuleInfoChanged(InfoSpeedLine, sg.CurrSS().GetTempoTpl().ToString());
			OnModuleInfoChanged(InfoTempoLine, PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			if (moduleStream.Length < 840)
				return ModuleType.Unknown;

			// Now check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if ((mark & 0xffffff00) != 0x4d4d4400)	// MMD\0
				return ModuleType.Unknown;

			// Mark out the mark and leave the version
			byte markVersion = (byte)(mark & 0x000000ff);

			if (((markVersion < '0') || (markVersion > '3')) && (markVersion != 'C'))
				return ModuleType.Unknown;

			if (markVersion == '0')
			{
				// Well, it's either a MED or OctaMED module, find out which one
				//
				// Skip module length
				moduleStream.Seek(4, SeekOrigin.Current);

				// Seek to the song structure + skip until the flags argument
				uint temp = moduleStream.Read_B_UINT32();
				if (temp == 0)
					return ModuleType.Unknown;

				moduleStream.Seek(temp + 767, SeekOrigin.Begin);

				if (((MmdFlag)moduleStream.Read_UINT8() & MmdFlag.EightChannel) != 0)
					return ModuleType.OctaMed;

				return ModuleType.Med;
			}

			if (markVersion == '1')
				return ModuleType.OctaMed_Professional4;

			if (markVersion == '2')
				return ModuleType.OctaMed_Professional6;

			if (markVersion == '3')
				return ModuleType.OctaMed_SoundStudio;

			if (markVersion == 'C')
				return ModuleType.MedPacker;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			// Set the current song
			sg.SetSSNum(songNumber);

			// Now set the tempo in the player
			SubSong ss = sg.CurrSS();
			Tempo tempo = ss.GetStartTempo();
			ss.SetTempo(tempo);
			plr.SetMixTempo(tempo);

			// Initialize the player
			plr.PlaySong(sg);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			plr = null;
			sg = null;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD header
		/// </summary>
		/********************************************************************/
		private void ReadMmdHeader(ModuleStream moduleStream, MmdHdr header)
		{
			header.Id = moduleStream.Read_B_UINT32();
			header.ModLen = moduleStream.Read_B_UINT32();
			header.SongOffs = moduleStream.Read_B_UINT32();
			header.PSecNum = moduleStream.Read_B_UINT16();
			header.PSeq = moduleStream.Read_B_UINT16();
			header.BlocksOffs = moduleStream.Read_B_UINT32();
			header.MmdFlags = moduleStream.Read_UINT8();
			moduleStream.Seek(3, SeekOrigin.Current);
			header.SamplesOffs = moduleStream.Read_B_UINT32();
			moduleStream.Seek(4, SeekOrigin.Current);
			header.ExpDataOffs = moduleStream.Read_B_UINT32();
			moduleStream.Seek(4, SeekOrigin.Current);
			header.PState = moduleStream.Read_B_UINT16();
			header.PBlock = moduleStream.Read_B_UINT16();
			header.PLine = moduleStream.Read_B_UINT16();
			header.PSeqNum = moduleStream.Read_B_UINT16();
			header.ActPlayLine = moduleStream.Read_B_INT16();
			header.Counter = moduleStream.Read_UINT8();
			header.ExtraSongs = moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD0 sample structure
		/// </summary>
		/********************************************************************/
		private void ReadMmd0Sample(ModuleStream moduleStream, Mmd0Sample sample)
		{
			sample.Rep = moduleStream.Read_B_UINT16();
			sample.RepLen = moduleStream.Read_B_UINT16();
			sample.MidiCh = moduleStream.Read_UINT8();
			sample.MidiPreset = moduleStream.Read_UINT8();
			sample.Volume = moduleStream.Read_UINT8();
			sample.STrans = moduleStream.Read_INT8();
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD0 song structure
		/// </summary>
		/********************************************************************/
		private void ReadMmd0Song(ModuleStream moduleStream, Mmd0SongData song)
		{
			song.NumBlocks = moduleStream.Read_B_UINT16();
			song.SongLen = moduleStream.Read_B_UINT16();
			moduleStream.Read(song.PlaySeq, 0, 256);
			song.DefTempo = moduleStream.Read_B_UINT16();
			song.PlayTransp = moduleStream.Read_INT8();
			song.Flags = (MmdFlag)moduleStream.Read_UINT8();
			song.Flags2 = (MmdFlag2)moduleStream.Read_UINT8();
			song.Tempo2 = moduleStream.Read_UINT8();
			moduleStream.Read(song.TrkVol, 0, 16);
			song.MasterVol = moduleStream.Read_UINT8();
			song.NumSamples = moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD0 expansion structure
		/// </summary>
		/********************************************************************/
		private void ReadMmd0ExpData(ModuleStream moduleStream, Mmd0ExpData expData)
		{
			expData.NextHdr = moduleStream.Read_B_UINT32();
			expData.InsTextOffs = moduleStream.Read_B_UINT32();
			expData.InsTextEntries = moduleStream.Read_B_UINT16();
			expData.InsTextEntrySize = moduleStream.Read_B_UINT16();
			expData.AnnoTextOffs = moduleStream.Read_B_UINT32();
			expData.AnnoTextLength = moduleStream.Read_B_UINT32();
			expData.InstInfoOffs = moduleStream.Read_B_UINT32();
			expData.InstInfoEntries = moduleStream.Read_B_UINT16();
			expData.InstInfoEntrySize = moduleStream.Read_B_UINT16();
			expData.Obsolete0 = moduleStream.Read_B_UINT32();
			expData.Obsolete1 = moduleStream.Read_B_UINT32();
			moduleStream.Read(expData.ChannelSplit, 0, 4);
			expData.NotInfoOffs = moduleStream.Read_B_UINT32();
			expData.SongNameOffs = moduleStream.Read_B_UINT32();
			expData.SongNameLen = moduleStream.Read_B_UINT32();
			expData.DumpsOffs = moduleStream.Read_B_UINT32();
			expData.MmdInfoOffs = moduleStream.Read_B_UINT32();
			expData.MmdARexxOffs = moduleStream.Read_B_UINT32();
			expData.MmdCmd3xOffs = moduleStream.Read_B_UINT32();
			expData.TrackInfoOffs = moduleStream.Read_B_UINT32();
			expData.EffectInfoOffs = moduleStream.Read_B_UINT32();
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD1 block info structure
		/// </summary>
		/********************************************************************/
		private void ReadMmd1BlockInfo(ModuleStream moduleStream, Mmd1BlockInfo blockInfo)
		{
			blockInfo.HlMask = moduleStream.Read_B_UINT32();
			blockInfo.BlockName = moduleStream.Read_B_UINT32();
			blockInfo.BlockNameLen = moduleStream.Read_B_UINT32();
			blockInfo.PageTable = moduleStream.Read_B_UINT32();
			blockInfo.CmdExtTable = moduleStream.Read_B_UINT32();
		}



		/********************************************************************/
		/// <summary>
		/// Read all the fields in the MMD2 song structure
		/// </summary>
		/********************************************************************/
		private void ReadMmd2Song(ModuleStream moduleStream, Mmd2SongData song)
		{
			song.NumBlocks = moduleStream.Read_B_UINT16();
			song.NumSections = moduleStream.Read_B_UINT16();
			song.PlaySeqTableOffs = moduleStream.Read_B_UINT32();
			song.SectionTableOffs = moduleStream.Read_B_UINT32();
			song.TrackVolsOffs = moduleStream.Read_B_UINT32();
			song.NumTracks = moduleStream.Read_B_UINT16();
			song.NumPlaySeqs = moduleStream.Read_B_UINT16();
			song.TrackPansOffs = moduleStream.Read_B_UINT32();
			song.Flags3 = (MmdFlag3)moduleStream.Read_B_UINT32();
			song.VolAdj = moduleStream.Read_B_UINT16();
			song.Channels = moduleStream.Read_B_UINT16();
			song.MixEchoType = moduleStream.Read_UINT8();
			song.MixEchoDepth = moduleStream.Read_UINT8();
			song.MixEchoLen = moduleStream.Read_B_UINT16();
			song.MixStereoSep = moduleStream.Read_INT8();
			moduleStream.Seek(223, SeekOrigin.Current);
			song.DefTempo = moduleStream.Read_B_UINT16();
			song.PlayTransp = moduleStream.Read_INT8();
			song.Flags = (MmdFlag)moduleStream.Read_UINT8();
			song.Flags2 = (MmdFlag2)moduleStream.Read_UINT8();
			song.Tempo2 = moduleStream.Read_UINT8();
			moduleStream.Seek(16, SeekOrigin.Current);
			song.MasterVol = moduleStream.Read_UINT8();
			song.NumSamples = moduleStream.Read_UINT8();
		}
		#endregion
	}
}

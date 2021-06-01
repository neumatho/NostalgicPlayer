/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for AMF (DSMI) format
	/// </summary>
	internal class MikModConverterWorker_Amf : MikModConverterWorkerBase
	{
		#region AmfHeader class
		private class AmfHeader
		{
			public readonly byte[] Id = new byte[3];					// AMF file marker
			public byte Version;										// Upper major, lower nibble minor version number
			public readonly byte[] SongName = new byte[33];				// Song name
			public byte NumSamples;										// Number of samples saved
			public byte NumOrders;
			public ushort NumTracks;									// Number of tracks saved
			public byte NumChannels;									// Number of channels used
			public sbyte[] PanPos = new sbyte[32];						// Voice pan positions
			public byte SongBpm;
			public byte SongSpd;
		}
		#endregion

		#region AmfSample class
		private class AmfSample
		{
			public byte Type;
			public readonly byte[] SampleName = new byte[33];
			public readonly byte[] FileName = new byte[14];
			public uint Offset;
			public uint Length;
			public ushort C2Spd;
			public byte Volume;
			public uint RepPos;
			public uint RepEnd;
		}
		#endregion

		#region AmfNote class
		private class AmfNote
		{
			public byte Note;
			public byte Instr;
			public byte Volume;
			public byte FxCnt;
			public readonly byte[] Effect = new byte[3];
			public readonly sbyte[] Parameter = new sbyte[3];

			public void Copy(AmfNote destination)
			{
				destination.Note = Note;
				destination.Instr = Instr;
				destination.Volume = Volume;
				destination.FxCnt = FxCnt;

				Array.Copy(Effect, destination.Effect, 3);
				Array.Copy(Parameter, destination.Parameter, 3);
			}
		}
		#endregion

		private AmfHeader mh;
		private AmfNote[] track;

		#region MikModConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			long fileSize = moduleStream.Length;
			if (fileSize < 75)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[3];
			moduleStream.Read(buf, 0, 3);

			if ((buf[0] == 'A') && (buf[1] == 'M') && (buf[2] == 'F'))
			{
				byte ver = moduleStream.Read_UINT8();

				if ((ver >= 8) && (ver <= 14))
					return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		protected override bool LoadModule(ModuleStream moduleStream, MUniTrk uniTrk, out string errorMessage)
		{
			errorMessage = string.Empty;

			mh = new AmfHeader();
			track = Helpers.InitializeArray<AmfNote>(64);

			try
			{
				Encoding encoder = EncoderCollection.Ibm850;

				// Try to read the module header
				moduleStream.Read(mh.Id, 0, 3);

				mh.Version = moduleStream.Read_UINT8();

				// For version 8, the song name is only 20 characters long and then come
				// some data, which I do not know what is. The original code by Otto Chrons
				// load the song name as 20 characters long and then it is overwritten again
				// it another function, where it loads 32 characters, no matter which version
				// it is. So we do the same here
				moduleStream.ReadString(mh.SongName, 32);

				mh.NumSamples = moduleStream.Read_UINT8();
				mh.NumOrders = moduleStream.Read_UINT8();
				mh.NumTracks = moduleStream.Read_L_UINT16();

				if (mh.Version >= 9)
					mh.NumChannels = moduleStream.Read_UINT8();
				else
					mh.NumChannels = 4;

				if ((mh.NumChannels == 0) || (mh.NumChannels > (mh.Version >= 12 ? 32 : 16)))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				byte[] channelRemap = new byte[16];

				if (mh.Version >= 11)
				{
					Array.Clear(mh.PanPos, 0, 32);
					moduleStream.ReadSigned(mh.PanPos, 0, (mh.Version >= 13) ? 32 : 16);
				}
				else if (mh.Version >= 9)
					moduleStream.Read(channelRemap, 0, 16);

				if (mh.Version >= 13)
				{
					mh.SongBpm = moduleStream.Read_UINT8();

					if (mh.SongBpm < 32)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
						return false;
					}

					mh.SongSpd = moduleStream.Read_UINT8();

					if (mh.SongSpd > 32)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
						return false;
					}
				}
				else
				{
					mh.SongBpm = 125;
					mh.SongSpd = 6;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Set module variables
				of.InitSpeed = mh.SongSpd;
				of.InitTempo = mh.SongBpm;

				originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_DSMI, mh.Version / 10, mh.Version % 10);

				of.NumChn = mh.NumChannels;
				of.NumTrk = (byte)(mh.NumOrders * mh.NumChannels);

				if (mh.NumTracks > of.NumTrk)
					of.NumTrk = mh.NumTracks;

				of.NumTrk++;			// Add room for extra, empty track

				of.SongName = encoder.GetString(mh.SongName);
				of.NumPos = mh.NumOrders;
				of.NumPat = mh.NumOrders;
				of.RepPos = 0;
				of.Flags |= ModuleFlag.S3MSlides;

				// Whenever possible, we should try to determine the original format.
				// Here we assume it was S3M-style wrt bpm limit...
				of.BpmLimit = 32;

				// Play with the panning table. Although the AMF format embeds a
				// panning table, if the module was a MOD or an S3M with default
				// panning and didn't use any panning commands, don't flag
				// UF_PANNING, to use our preferred panning table for this case
				bool defaultPanning = true;

				if (mh.Version >= 11)
				{
					for (int t = 0; t < 32; t++)
					{
						if (mh.PanPos[t] > 64)
						{
							of.Panning[t] = SharedConstant.Pan_Surround;
							defaultPanning = false;
						}
						else
						{
							if (mh.PanPos[t] == 64)
								of.Panning[t] = SharedConstant.Pan_Right;
							else
								of.Panning[t] = (ushort)((mh.PanPos[t] + 64) << 1);
						}
					}
				}
				else
					defaultPanning = false;

				if (defaultPanning)
				{
					for (int t = 0; t < of.NumChn; t++)
					{
						if (of.Panning[t] == (((t + 1) & 2) != 0 ? SharedConstant.Pan_Right : SharedConstant.Pan_Left))
						{
							// Not MOD canonical panning
							defaultPanning = false;
							break;
						}
					}
				}

				if (defaultPanning)
					of.Flags |= ModuleFlag.Panning;

				of.NumIns = of.NumSmp = mh.NumSamples;

				if (!MLoader.AllocPositions(of, of.NumPos))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (ushort t = 0; t < of.NumPos; t++)
					of.Positions[t] = t;

				if (!MLoader.AllocTracks(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if (!MLoader.AllocPatterns(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Read AMF order table
				for (int t = 0; t < of.NumPat; t++)
				{
					if (mh.Version >= 14)
					{
						// Track size
						of.PattRows[t] = moduleStream.Read_L_UINT16();
					}

					if ((mh.Version == 9) || (mh.Version == 10))
					{
						// Only version 9 and 10 uses channel remap
						for (int u = 0; u < of.NumChn; u++)
							of.Patterns[t * of.NumChn + channelRemap[u]] = moduleStream.Read_L_UINT16();
					}
					else
						moduleStream.ReadArray_L_UINT16s(of.Patterns, t * of.NumChn, of.NumChn);
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Read sample information
				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				for (int t = 0; t < of.NumIns; t++)
				{
					Sample q = of.Samples[t];
					AmfSample s = new AmfSample();

					// Try to read sample info
					s.Type = moduleStream.Read_UINT8();

					moduleStream.ReadString(s.SampleName, 32);
					moduleStream.ReadString(s.FileName, 13);

					s.Offset = moduleStream.Read_L_UINT32();

					if (mh.Version >= 10)
						s.Length = moduleStream.Read_L_UINT32();
					else
						s.Length = moduleStream.Read_L_UINT16();

					s.C2Spd = moduleStream.Read_L_UINT16();

					if (s.C2Spd == 8368)
						s.C2Spd = 8363;

					s.Volume = moduleStream.Read_UINT8();

					// "the tribal zone.amf" and "the way its gonna b.amf" by Maelcum
					// are the only version 10 files I can find, and they have 32 bit
					// reppos and repend, not 16
					if (mh.Version >= 10)	// was 11
					{
						s.RepPos = moduleStream.Read_L_UINT32();
						s.RepEnd = moduleStream.Read_L_UINT32();
					}
					else
					{
						s.RepPos = moduleStream.Read_L_UINT16();
						s.RepEnd = moduleStream.Read_L_UINT16();

						if (s.RepEnd == 0xffff)
							s.RepEnd = 0;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					q.SampleName = encoder.GetString(s.SampleName);
					q.Speed = s.C2Spd;
					q.Volume = s.Volume;

					if (s.Type != 0)
					{
						q.SeekPos = s.Offset;
						q.Length = s.Length;
						q.LoopStart = s.RepPos;
						q.LoopEnd = s.RepEnd;

						if ((s.RepEnd - s.RepPos) > 2)
							q.Flags |= SampleFlag.Loop;
					}
				}

				// Read track table
				ushort[] trackRemap = new ushort[mh.NumTracks + 1];

				moduleStream.ReadArray_L_UINT16s(trackRemap, 1, mh.NumTracks);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}

				int realTrackCnt = 0;
				for (int t = 0; t <= mh.NumTracks; t++)
				{
					if (realTrackCnt < trackRemap[t])
						realTrackCnt = trackRemap[t];
				}

				if (realTrackCnt > mh.NumTracks)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}

				for (int t = 0; t < of.NumPat * of.NumChn; t++)
					of.Patterns[t] = (ushort)((of.Patterns[t] <= mh.NumTracks) ? trackRemap[of.Patterns[t]] - 1 : realTrackCnt);

				// Unpack tracks
				for (int t = 0; t < realTrackCnt; t++)
				{
					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
						return false;
					}

					if (!UnpackTrack(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
						return false;
					}

					if ((of.Tracks[t] = ConvertTrack(uniTrk)) == null)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
						return false;
					}
				}

				// Add an extra void track
				uniTrk.UniReset();

				for (int t = 0; t < 64; t++)
					uniTrk.UniNewLine();

				of.Tracks[realTrackCnt++] = uniTrk.UniDup();

				for (int t = realTrackCnt; t < of.NumTrk; t++)
					of.Tracks[t] = null;

				// Compute sample offsets
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				long samplePos = moduleStream.Position;
				long fileEnd = moduleStream.Length;

				int realSmpCnt = 0;
				for (int t = 0; t < of.NumSmp; t++)
				{
					if (realSmpCnt < of.Samples[t].SeekPos)
						realSmpCnt = (int)of.Samples[t].SeekPos;
				}

				for (int t = 1; t <= realSmpCnt; t++)
				{
					int q = 0;
					int u = 0;

					while (of.Samples[q].SeekPos != t)
					{
						if (++u == of.NumSmp)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
							return false;
						}

						q++;
					}

					of.Samples[q].SeekPos = (uint)samplePos;
					samplePos += of.Samples[q].Length;
				}

				if (samplePos > fileEnd)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}
			}
			finally
			{
				track = null;
				mh = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will load a single track
		/// </summary>
		/********************************************************************/
		private bool UnpackTrack(ModuleStream moduleStream)
		{
			// Empty track
			track = Helpers.InitializeArray<AmfNote>(64);

			// Read packed track
			if (moduleStream != null)
			{
				uint trackSize = moduleStream.Read_L_UINT16();

				// The original code in DSMI library read the byte,
				// but it is not used, so we won't either
//				trackSize += ((uint)moduleStream.Read_UINT8()) << 16;
				moduleStream.Read_UINT8();

				if (trackSize != 0)
				{
					while (trackSize-- > 0)
					{
						byte row = moduleStream.Read_UINT8();
						byte cmd = moduleStream.Read_UINT8();
						sbyte arg = moduleStream.Read_INT8();

						// Unexpected end of track
						if (trackSize == 0)
						{
							if ((row == 0xff) && (cmd == 0xff) && (arg == -1))
								break;
							/* The last triplet should be FF FF FF, but this is not
							   always the case.. maybe a bug in m2amf?
							else
								return false;
							*/
						}

						// Invalid row (probably unexpected end of row)
						if (row >= 64)
						{
							moduleStream.Seek(trackSize * 3, SeekOrigin.Current);
							return true;
						}

						if (cmd < 0x7f)
						{
							// Note, vol
							track[row].Note = cmd;
							track[row].Volume = (byte)((byte)arg + 1);
						}
						else
						{
							if (cmd == 0x7f)
							{
								// Duplicate row
								if ((arg < 0) && (row + arg >= 0))
									track[row + arg].Copy(track[row]);
							}
							else
							{
								if (cmd == 0x80)
								{
									// Instr
									track[row].Instr = (byte)(arg + 1);
								}
								else
								{
									if (cmd == 0x83)
									{
										// Volume without note
										track[row].Volume = (byte)((byte)arg + 1);
									}
									else
									{
										if (cmd == 0xff)
										{
											// Apparently, some M2AMF version fail to estimate the
											// size of the compressed patterns correctly, and end
											// up with blanks, i.e. dead triplets. Those are marked
											// with cmd == 0xff. Let's ignore them
										}
										else
										{
											if (track[row].FxCnt < 3)
											{
												// Effect, param
												if (cmd > 0x97)
												{
													// Instead of failing, we just ignore unknown effects.
													// This will load the "escape from dulce base" module
													continue;
//													return false;
												}

												track[row].Effect[track[row].FxCnt] = (byte)(cmd & 0x7f);
												track[row].Parameter[track[row].FxCnt] = arg;
												track[row].FxCnt++;
											}
											else
												return false;
										}
									}
								}
							}
						}
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a track to uni format
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(MUniTrk uniTrk)
		{
			Command fx4Memory = 0;

			// Convert track
			uniTrk.UniReset();

			for (int row = 0; row < 64; row++)
			{
				if (track[row].Instr != 0)
					uniTrk.UniInstrument((ushort)(track[row].Instr - 1));

				if (track[row].Note > SharedConstant.Octave)
					uniTrk.UniNote((ushort)(track[row].Note - SharedConstant.Octave));

				// AMF effects
				while (track[row].FxCnt-- > 0)
				{
					sbyte inf = track[row].Parameter[track[row].FxCnt];

					switch (track[row].Effect[track[row].FxCnt])
					{
						// Set speed
						case 1:
						{
							uniTrk.UniEffect(Command.UniS3MEffectA, (ushort)inf);
							break;
						}

						// Volume slide
						case 2:
						{
							if (inf != 0)
							{
								uniTrk.UniWriteByte((byte)Command.UniS3MEffectD);

								if (inf >= 0)
									uniTrk.UniWriteByte((byte)((inf & 0xf) << 4));
								else
									uniTrk.UniWriteByte((byte)((-inf) & 0xf));
							}
							break;
						}

						// Effect 3 -> Set channel volume, done in UnpackTrack()

						// Porta up/down
						case 4:
						{
							if (inf != 0)
							{
								if (inf > 0)
								{
									uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)inf);
									fx4Memory = Command.UniS3MEffectE;
								}
								else
								{
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)-inf);
									fx4Memory = Command.UniS3MEffectF;
								}
							}
							else
							{
								if (fx4Memory != 0)
									uniTrk.UniEffect(fx4Memory, 0);
							}
							break;
						}

						// Effect 5 -> Porta abs, not supported

						// Porta to note
						case 6:
						{
							uniTrk.UniEffect(Command.UniItEffectG, (ushort)inf);
							break;
						}

						// Tremor
						case 7:
						{
							uniTrk.UniEffect(Command.UniS3MEffectI, (ushort)inf);
							break;
						}

						// Arpeggio
						case 8:
						{
							uniTrk.UniPtEffect(0x0, (byte)inf, of.Flags);
							break;
						}

						// Vibrato
						case 9:
						{
							uniTrk.UniPtEffect(0x4, (byte)inf, of.Flags);
							break;
						}

						// Porta + volume slide
						case 0xa:
						{
							uniTrk.UniPtEffect(0x3, 0, of.Flags);

							if (inf != 0)
							{
								uniTrk.UniWriteByte((byte)Command.UniS3MEffectD);

								if (inf >= 0)
									uniTrk.UniWriteByte((byte)((inf & 0xf) << 4));
								else
									uniTrk.UniWriteByte((byte)((-inf) & 0xf));
							}
							break;
						}

						// Vibrato + volume slide
						case 0xb:
						{
							uniTrk.UniPtEffect(0x4, 0, of.Flags);

							if (inf != 0)
							{
								uniTrk.UniWriteByte((byte)Command.UniS3MEffectD);

								if (inf >= 0)
									uniTrk.UniWriteByte((byte)((inf & 0xf) << 4));
								else
									uniTrk.UniWriteByte((byte)((-inf) & 0xf));
							}
							break;
						}

						// Pattern break (in hex)
						case 0xc:
						{
							uniTrk.UniPtEffect(0xd, (byte)inf, of.Flags);
							break;
						}

						// Pattern jump
						case 0xd:
						{
							uniTrk.UniPtEffect(0xb, (byte)inf, of.Flags);
							break;
						}

						// Effect 0xe -> Sync, not supported

						// Retrig
						case 0xf:
						{
							uniTrk.UniEffect(Command.UniS3MEffectQ, (ushort)(inf & 0xf));
							break;
						}

						// Sample offset
						case 0x10:
						{
							uniTrk.UniPtEffect(0x9, (byte)inf, of.Flags);
							break;
						}

						// Fine volume slide
						case 0x11:
						{
							if (inf != 0)
							{
								uniTrk.UniWriteByte((byte)Command.UniS3MEffectD);

								if (inf >= 0)
									uniTrk.UniWriteByte((byte)((inf & 0xf) << 4 | 0xf));
								else
									uniTrk.UniWriteByte((byte)(0xf0 | ((-inf) & 0xf)));
							}
							break;
						}

						// Fine portamento
						case 0x12:
						{
							if (inf != 0)
							{
								if (inf >= 0)
								{
									uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xf0 | (inf & 0xf)));
									fx4Memory = Command.UniS3MEffectE;
								}
								else
								{
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xf0 | ((-inf) & 0xf)));
									fx4Memory = Command.UniS3MEffectF;
								}
							}
							else
							{
								if (fx4Memory != 0)
									uniTrk.UniEffect(fx4Memory, 0);
							}
							break;
						}

						// Delay note
						case 0x13:
						{
							uniTrk.UniPtEffect(0xe, (byte)(0xd0 | (inf & 0xf)), of.Flags);
							break;
						}

						// Note cut
						case 0x14:
						{
							uniTrk.UniPtEffect(0xc, 0, of.Flags);
							track[row].Volume = 0;
							break;
						}

						// Set tempo
						case 0x15:
						{
							uniTrk.UniEffect(Command.UniS3MEffectT, (ushort)inf);
							break;
						}

						// Extra fine portamento
						case 0x16:
						{
							if (inf != 0)
							{
								if (inf >= 0)
								{
									uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xe0 | ((inf >> 2) & 0xf)));
									fx4Memory = Command.UniS3MEffectE;
								}
								else
								{
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xe0 | (((-inf) >> 2) & 0xf)));
									fx4Memory = Command.UniS3MEffectF;
								}
							}
							else
							{
								if (fx4Memory != 0)
									uniTrk.UniEffect(fx4Memory, 0);
							}
							break;
						}

						// Panning
						case 0x17:
						{
							if (inf > 64)
							{
								// Surround
								uniTrk.UniEffect(Command.UniItEffectS0, 0x91);
							}
							else
								uniTrk.UniPtEffect(0x8, (byte)((inf == 64) ? 255 : (inf + 64) << 1), of.Flags);

							of.Flags |= ModuleFlag.Panning;
							break;
						}
					}
				}

				if (track[row].Volume != 0)
					uniTrk.UniVolEffect(VolEffect.Volume, (byte)(track[row].Volume - 1));

				uniTrk.UniNewLine();
			}

			return uniTrk.UniDup();
		}
		#endregion
	}
}

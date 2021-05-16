/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
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
	/// MikMod loader for GDM (General DigiMusic) format
	/// </summary>
	internal class MikModConverterWorker_Gdm : MikModConverterWorkerBase
	{
		#region GdmEffect struct
		private struct GdmEffect
		{
			public byte Effect;
			public byte Param;
		}
		#endregion

		#region GdmNote class
		private class GdmNote
		{
			public byte Note;
			public byte Samp;
			public GdmEffect[] Effect = new GdmEffect[4];
		}
		#endregion

		#region GdmHeader class
		private class GdmHeader
		{
			public byte[] Id1 = new byte[4];
			public byte[] SongName = new byte[33];
			public byte[] Author = new byte[33];
			public byte[] EofMarker = new byte[3];
			public byte[] Id2 = new byte[4];

			public byte MajorVer;
			public byte MinorVer;
			public ushort TrackerId;
			public byte T_MajorVer;
			public byte T_MinorVer;
			public byte[] PanTable = new byte[32];
			public byte MasterVol;
			public byte MasterTempo;
			public byte MasterBpm;
			public ushort Flags;

			public uint OrderLoc;
			public byte OrderNum;
			public uint PatternLoc;
			public byte PatternNum;
			public uint SamHead;
			public uint SamData;
			public byte SamNum;
			public uint MessageLoc;
			public uint MessageLen;
			public uint ScrollyLoc;
			public ushort ScrollyLen;
			public uint GraphicLoc;
			public ushort GraphicLen;
		}
		#endregion

		#region GdmSample class
		private class GdmSample
		{
			public byte[] SampName = new byte[33];
			public byte[] FileName = new byte[13];
			public byte Ems;
			public uint Length;
			public uint LoopBeg;
			public uint LoopEnd;
			public byte Flags;
			public ushort C4Spd;
			public byte Vol;
			public byte Pan;
		}
		#endregion

		private GdmHeader mh;
		private GdmNote[] gdmBuf;

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
			if (fileSize < 157)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] id = new byte[4];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(id, 0, 4);

			if ((id[0] == 'G') && (id[1] == 'D') && (id[2] == 'M') && (id[3] == 0xfe))
			{
				moduleStream.Seek(71, SeekOrigin.Begin);
				moduleStream.Read(id, 0, 4);

				if ((id[0] == 'G') && (id[1] == 'M') && (id[2] == 'F') && (id[3] == 'S'))
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

			mh = new GdmHeader();
			gdmBuf = Helpers.InitializeArray<GdmNote>(32 * 64);

			try
			{
				Encoding encoder = EncoderCollection.Ibm850;

				// Read the header
				moduleStream.Read(mh.Id1, 0, 4);
				moduleStream.ReadString(mh.SongName, 32);
				moduleStream.ReadString(mh.Author, 32);
				moduleStream.Read(mh.EofMarker, 0, 3);
				moduleStream.Read(mh.Id2, 0, 4);

				mh.MajorVer = moduleStream.Read_UINT8();
				mh.MinorVer = moduleStream.Read_UINT8();
				mh.TrackerId = moduleStream.Read_L_UINT16();
				mh.T_MajorVer = moduleStream.Read_UINT8();
				mh.T_MinorVer = moduleStream.Read_UINT8();

				moduleStream.Read(mh.PanTable, 0, 32);

				mh.MasterVol = moduleStream.Read_UINT8();
				mh.MasterTempo = moduleStream.Read_UINT8();
				mh.MasterBpm = moduleStream.Read_UINT8();
				mh.Flags = moduleStream.Read_L_UINT16();

				mh.OrderLoc = moduleStream.Read_L_UINT32();
				mh.OrderNum = moduleStream.Read_UINT8();
				mh.PatternLoc = moduleStream.Read_L_UINT32();
				mh.PatternNum = moduleStream.Read_UINT8();
				mh.SamHead = moduleStream.Read_L_UINT32();
				mh.SamData = moduleStream.Read_L_UINT32();
				mh.SamNum = moduleStream.Read_UINT8();
				mh.MessageLoc = moduleStream.Read_L_UINT32();
				mh.MessageLen = moduleStream.Read_L_UINT32();
				mh.ScrollyLoc = moduleStream.Read_L_UINT32();
				mh.ScrollyLen = moduleStream.Read_L_UINT16();
				mh.GraphicLoc = moduleStream.Read_L_UINT32();
				mh.GraphicLen = moduleStream.Read_L_UINT16();

				// Have we ended abruptly?
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Any orders?
				if (mh.OrderNum == 255)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				// Now we fill
				of.SongName = encoder.GetString(mh.SongName);
				of.NumPat = (ushort)(mh.PatternNum + 1);
				of.RepPos = 0;
				of.NumIns = of.NumSmp = (ushort)(mh.SamNum + 1);
				of.InitSpeed = mh.MasterTempo;
				of.InitTempo = mh.MasterBpm;
				of.InitVolume = (byte)(mh.MasterVol << 1);
				of.Flags |= ModuleFlag.S3MSlides | ModuleFlag.Panning;

				// Whenever possible, we should try to determine the original format.
				// Here we assume it was S3M-style wrt bpm limit...
				of.BpmLimit  = 32;

				// Read the order data
				if (!MLoader.AllocPositions(of, mh.OrderNum + 1))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				moduleStream.Seek(mh.OrderLoc, SeekOrigin.Begin);
				for (int i = 0; i < mh.OrderNum + 1; i++)
					of.Positions[i] = moduleStream.Read_UINT8();

				of.NumPos = 0;
				for (int i = 0; i < mh.OrderNum + 1; i++)
				{
					int order = of.Positions[i];
					if (order == 255)
						order = SharedConstant.Last_Pattern;

					of.Positions[of.NumPos] = (ushort)order;

					if (of.Positions[i] < 254)
						of.NumPos++;
				}

				// Have we ended abruptly yet?
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Time to load the samples
				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				uint position = mh.SamData;

				// Seek to instrument position
				moduleStream.Seek(mh.SamHead, SeekOrigin.Begin);

				for (int i = 0; i < of.NumIns; i++)
				{
					GdmSample s = new GdmSample();
					Sample q = of.Samples[i];

					// Load sample info
					moduleStream.ReadString(s.SampName, 32);
					moduleStream.ReadString(s.FileName, 12);

					s.Ems = moduleStream.Read_UINT8();
					s.Length = moduleStream.Read_L_UINT32();
					s.LoopBeg = moduleStream.Read_L_UINT32();
					s.LoopEnd = moduleStream.Read_L_UINT32();
					s.Flags = moduleStream.Read_UINT8();
					s.C4Spd = moduleStream.Read_L_UINT16();
					s.Vol = moduleStream.Read_UINT8();
					s.Pan = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					q.SampleName = encoder.GetString(s.SampName);
					q.Speed = s.C4Spd;
					q.Length = s.Length;
					q.LoopStart = s.LoopBeg;
					q.LoopEnd = s.LoopEnd;
					q.Volume = 64;
					q.SeekPos = position;

					position += s.Length;

					// Only use the sample volume byte if bit 2 is set. When bit 3 is set,
					// the sample panning is supposed to be used, but 2GDM isn't capable
					// of making a GDM using this feature; the panning byte is always 0xFF
					// or junk. Likewise, bit 5 is unused (supposed to be LZW compression)
					if ((s.Flags & 1) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 2) != 0)
						q.Flags |= SampleFlag._16Bits;

					if (((s.Flags & 4) != 0) && (s.Vol <= 64))
						q.Volume = s.Vol;

					if ((s.Flags & 16) != 0)
						q.Flags |= SampleFlag.Stereo;
				}

				// Set the panning
				int x = 0;
				for (int i = 0; i < 32; i++)
				{
					of.Panning[i] = mh.PanTable[i];

					if (of.Panning[i] == 0)
						of.Panning[i] = SharedConstant.Pan_Left;
					else if (of.Panning[i] == 8)
						of.Panning[i] = SharedConstant.Pan_Center;
					else if (of.Panning[i] == 15)
						of.Panning[i] = SharedConstant.Pan_Right;
					else if (of.Panning[i] == 16)
						of.Panning[i] = SharedConstant.Pan_Surround;
					else if (of.Panning[i] == 255)
						of.Panning[i] = 128;
					else
						of.Panning[i] <<= 3;

					if (mh.PanTable[i] != 255)
						x = i;
				}

				of.NumChn = (byte)(x + 1);
				if (of.NumChn < 1)
					of.NumChn = 1;			// For broken counts

				// Load the pattern info
				of.NumTrk = (ushort)(of.NumPat * of.NumChn);

				// Jump to patterns
				moduleStream.Seek(mh.PatternLoc, SeekOrigin.Begin);

				if (!MLoader.AllocTracks(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}

				if (!MLoader.AllocPatterns(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}

				for (int i = 0, track = 0; i < of.NumPat; i++)
				{
					if (!ReadPattern(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					for (int u = 0; u < of.NumChn; u++, track++)
					{
						of.Tracks[track] = ConvertTrack(gdmBuf, u << 6, uniTrk);
						if (of.Tracks[track] == null)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
							return false;
						}
					}
				}
			}
			finally
			{
				mh = null;
				gdmBuf = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read one pattern
		/// </summary>
		/********************************************************************/
		private bool ReadPattern(ModuleStream moduleStream)
		{
			// Get pattern length
			ushort length = moduleStream.Read_L_UINT16();
			length -= 2;

			// Clear pattern data
			for (int i = 0; i < 32 * 64; i++)
			{
				GdmNote n = gdmBuf[i];
				n.Note = n.Samp = 255;

				for (int j = 0; j < n.Effect.Length; j++)
					n.Effect[j].Effect = n.Effect[j].Param = 255;
			}

			int x = 0;
			int pos = 0;

			while (x < length)
			{
				GdmNote n = new GdmNote();
				n.Note = n.Samp = 255;

				for (int j = 0; j < n.Effect.Length; j++)
					n.Effect[j].Effect = n.Effect[j].Param = 255;

				int flag = moduleStream.Read_UINT8();
				x++;

				if (moduleStream.EndOfStream)
					return false;

				int ch = flag & 31;
				if (ch > of.NumChn)
					return false;

				if (flag == 0)
				{
					pos++;
					if (x == length)
					{
						if (pos > 64)
							return false;
					}
					else
					{
						if (pos >= 64)
							return false;
					}
					continue;
				}

				if ((flag & 0x60) != 0)
				{
					if ((flag & 0x20) != 0)
					{
						// New note
						n.Note = (byte)(moduleStream.Read_UINT8() & 127);
						n.Samp = moduleStream.Read_UINT8();
						x += 2;
					}

					if ((flag & 0x40) != 0)
					{
						int i;

						do
						{
							// Effect channel set
							i = moduleStream.Read_UINT8();
							n.Effect[i >> 6].Effect = (byte)(i & 31);
							n.Effect[i >> 6].Param = moduleStream.Read_UINT8();
							x += 2;
						}
						while ((i & 32) != 0);
					}

					gdmBuf[64 * ch + pos] = n;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert one GDM track to UniMod track
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(GdmNote[] tr, int offset, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < 64; t++)
			{
				byte note = tr[offset + t].Note;
				byte ins = tr[offset + t].Samp;

				if ((ins != 0) && (ins != 255))
					uniTrk.UniInstrument((ushort)(ins - 1));

				if ((note != 0) && (note != 255))
					uniTrk.UniNote((ushort)(((note >> 4) * SharedConstant.Octave) + (note & 0xf) - 1));

				for (int i = 0; i < 4; i++)
				{
					byte inf = tr[offset + t].Effect[i].Param;

					switch (tr[offset + t].Effect[i].Effect)
					{
						// Toneslide up
						case 1:
						{
							uniTrk.UniEffect(Command.UniS3MEffectF, inf);
							break;
						}

						// Toneslide down
						case 2:
						{
							uniTrk.UniEffect(Command.UniS3MEffectE, inf);
							break;
						}

						// Glissando to note
						case 3:
						{
							uniTrk.UniEffect(Command.UniItEffectG, inf);
							break;
						}

						// Vibrato
						case 4:
						{
							uniTrk.UniEffect(Command.UniGdmEffect4, inf);
							break;
						}

						// Portamento + volslide
						case 5:
						{
							uniTrk.UniEffect(Command.UniItEffectG, 0);
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Vibrato + volslide
						case 6:
						{
							uniTrk.UniEffect(Command.UniGdmEffect4, 0);
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Tremolo
						case 7:
						{
							uniTrk.UniEffect(Command.UniGdmEffect7, inf);
							break;
						}

						// Tremor
						case 8:
						{
							uniTrk.UniEffect(Command.UniS3MEffectI, inf);
							break;
						}

						// Offset
						case 9:
						{
							uniTrk.UniPtEffect(0x09, inf, of.Flags);
							break;
						}

						// Volslide
						case 0x0a:
						{
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Jump to order
						case 0x0b:
						{
							uniTrk.UniPtEffect(0x0b, inf, of.Flags);
							break;
						}

						// Volume set
						case 0x0c:
						{
							uniTrk.UniPtEffect(0x0c, inf, of.Flags);
							break;
						}

						// Pattern break
						case 0x0d:
						{
							uniTrk.UniPtEffect(0x0d, inf, of.Flags);
							break;
						}

						// Extended
						case 0x0e:
						{
							switch (inf & 0xf0)
							{
								// Fine portamento up
								case 0x10:
								{
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xf0 | (inf & 0x0f)));
									break;
								}

								// Fine portamento down
								case 0x20:
								{
									uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xf0 | (inf & 0x0f)));
									break;
								}

								// Glissando control
								// Vibrato waveform
								// Set c4spd
								// Loop fun
								// Tremolo waveform
								case 0x30:
								case 0x40:
								case 0x50:
								case 0x60:
								case 0x70:
								{
									uniTrk.UniPtEffect(0xe, inf, of.Flags);
									break;
								}

								// Extra fine porta up
								case 0x80:
								{
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xe0 | (inf & 0x0f)));
									break;
								}

								// Extra fine porta down
								case 0x90:
								{
									uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xe0 | (inf & 0x0f)));
									break;
								}

								// Fine volslide up
								case 0xa0:
								{
									uniTrk.UniEffect(Command.UniS3MEffectD, (ushort)(0x0f | ((inf & 0x0f) << 4)));
									break;
								}

								// Fine volslide down
								case 0xb0:
								{
									uniTrk.UniEffect(Command.UniS3MEffectD, (ushort)(0xf0 | (inf & 0x0f)));
									break;
								}

								// Note cut
								// Note delay
								// Extend row
								case 0xc0:
								case 0xd0:
								case 0xe0:
								{
									uniTrk.UniPtEffect(0xe, inf, of.Flags);
									break;
								}
							}
							break;
						}

						// Set tempo
						case 0x0f:
						{
							uniTrk.UniEffect(Command.UniS3MEffectA, inf);
							break;
						}

						// Arpeggio
						case 0x10:
						{
							uniTrk.UniPtEffect(0x0, inf, of.Flags);
							break;
						}

						// Retrigger
						case 0x12:
						{
							uniTrk.UniEffect(Command.UniS3MEffectQ, inf);
							break;
						}

						// Set global volume
						case 0x13:
						{
							uniTrk.UniEffect(Command.UniXmEffectG, inf);
							break;
						}

						// Fine vibrato
						case 0x14:
						{
							uniTrk.UniEffect(Command.UniGdmEffect14, inf);
							break;
						}

						// Special
						case 0x1e:
						{
							switch (inf & 0xf0)
							{
								// Set pan position
								case 0x80:
								{
									uniTrk.UniPtEffect(0xe, inf, of.Flags);
									break;
								}
							}
							break;
						}

						// Set bpm
						case 0x1f:
						{
							if (inf >= 0x20)
								uniTrk.UniEffect(Command.UniS3MEffectT, inf);

							break;
						}
					}
				}

				uniTrk.UniNewLine();
			}

			return uniTrk.UniDup();
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
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
	/// MikMod loader for DSM (DSIK) format
	/// </summary>
	internal class DsmFormat : MikModConverterWorkerBase
	{
		#region DsmSong class
		private class DsmSong
		{
			public readonly byte[] SongName = new byte[29];
			public ushort Version;
			public ushort Flags;
			public uint Reserved2;
			public ushort NumOrd;
			public ushort NumSmp;
			public ushort NumPat;
			public ushort NumTrk;
			public byte GlobalVol;
			public byte MasterVol;
			public byte Speed;
			public byte Bpm;
			public readonly byte[] PanPos = new byte[MaxChan];
			public readonly byte[] Orders = new byte[MaxOrders];
		}
		#endregion

		#region DsmInst class
		private class DsmInst
		{
			public readonly byte[] FileName = new byte[14];
			public ushort Flags;
			public byte Volume;
			public uint Length;
			public uint LoopStart;
			public uint LoopEnd;
			public uint Reserved1;
			public ushort C2Spd;
			public ushort Period;
			public readonly byte[] SampleName = new byte[29];
		}
		#endregion

		#region DsmNote class
		private class DsmNote
		{
			public byte Note;
			public byte Ins;
			public byte Vol;
			public byte Cmd;
			public byte Inf;
		}
		#endregion

		private const int MaxChan = 16;
		private const int MaxOrders = 128;
		private const int Surround = 0xa4;

		private const uint SongId = 0x534f4e47;		// SONG
		private const uint InstId = 0x494e5354;		// INST
		private const uint PattId = 0x50415454;		// PATT

		private DsmSong mh;
		private DsmNote[] dsmBuf;

		private uint blockId;
		private uint blockLn;
		private uint blockLp;

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
			if (fileSize < 12)
				return AgentResult.Unknown;

			// Now check the signature
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[12];
			moduleStream.Read(buf, 0, 12);

			if ((buf[0] == 'R') && (buf[1] == 'I') && (buf[2] == 'F') && (buf[3] == 'F') &&
			    (buf[8] == 'D') && (buf[9] == 'S') && (buf[10] == 'M') && (buf[11] == 'F'))
			{
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

			mh = new DsmSong();
			dsmBuf = Helpers.InitializeArray<DsmNote>(MaxChan * 64);

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				blockLp = 0;
				blockLn = 12;

				if (!GetBlockHeader(moduleStream))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if (blockId != SongId)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				moduleStream.ReadString(mh.SongName, 28);

				mh.Version = moduleStream.Read_L_UINT16();
				mh.Flags = moduleStream.Read_L_UINT16();
				mh.Reserved2 = moduleStream.Read_L_UINT32();
				mh.NumOrd = moduleStream.Read_L_UINT16();
				mh.NumSmp = moduleStream.Read_L_UINT16();
				mh.NumPat = moduleStream.Read_L_UINT16();
				mh.NumTrk = moduleStream.Read_L_UINT16();
				mh.GlobalVol = moduleStream.Read_UINT8();
				mh.MasterVol = moduleStream.Read_UINT8();
				mh.Speed = moduleStream.Read_UINT8();
				mh.Bpm = moduleStream.Read_UINT8();

				moduleStream.Read(mh.PanPos, 0, MaxChan);
				moduleStream.Read(mh.Orders, 0, MaxOrders);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Set module variables
				of.InitSpeed = mh.Speed;
				of.InitTempo = mh.Bpm;
				of.NumChn = (byte)mh.NumTrk;
				of.NumPat = mh.NumPat;
				of.NumTrk = (ushort)(of.NumChn * of.NumPat);
				of.SongName = encoder.GetString(mh.SongName);
				of.RepPos = 0;
				of.Flags |= ModuleFlag.Panning;

				// Whenever possible, we should try to determine the original format.
				// Here we assume it was S3M-style wrt bpm limit...
				of.BpmLimit  = 32;

				for (int t = 0; t < MaxChan; t++)
					of.Panning[t] = (ushort)(mh.PanPos[t] == Surround ? SharedConstant.Pan_Surround : mh.PanPos[t] < 0x80 ? (mh.PanPos[t] << 1) : 255);

				if (!MLoader.AllocPositions(of, mh.NumOrd))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				of.NumPos = 0;

				for (int t = 0; t < mh.NumOrd; t++)
				{
					int order = mh.Orders[t];
					if (order == 255)
						order = SharedConstant.Last_Pattern;
					else
					{
						// Sanity check
						if (of.Positions[t] > of.NumPat)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
							return false;
						}
					}

					of.Positions[of.NumPos] = (ushort)order;

					if (mh.Orders[t] < 254)
						of.NumPos++;
				}

				of.NumIns = of.NumSmp = mh.NumSmp;

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

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

				int curSmp = 0;
				int curPat = 0;
				int track = 0;

				while ((curSmp < of.NumIns) || (curPat < of.NumPat))
				{
					if (!GetBlockHeader(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
						return false;
					}

					if ((blockId == InstId) && (curSmp < of.NumIns))
					{
						DsmInst s = new DsmInst();
						Sample q = of.Samples[curSmp];

						// Try to read sample info
						moduleStream.Read(s.FileName, 0, 13);

						s.Flags = moduleStream.Read_L_UINT16();
						s.Volume = moduleStream.Read_UINT8();
						s.Length = moduleStream.Read_L_UINT32();
						s.LoopStart = moduleStream.Read_L_UINT32();
						s.LoopEnd = moduleStream.Read_L_UINT32();
						s.Reserved1 = moduleStream.Read_L_UINT32();
						s.C2Spd = moduleStream.Read_L_UINT16();
						s.Period = moduleStream.Read_L_UINT16();

						moduleStream.Read(s.SampleName, 0, 28);

						q.SampleName = encoder.GetString(s.SampleName);
						q.SeekPos = (uint)moduleStream.Position;
						q.Speed = s.C2Spd;
						q.Length = s.Length;
						q.LoopStart = s.LoopStart;
						q.LoopEnd = s.LoopEnd;
						q.Volume = s.Volume;

						if ((s.Flags & 1) != 0)
							q.Flags |= SampleFlag.Loop;

						if ((s.Flags & 2) != 0)
							q.Flags |= SampleFlag.Signed;

						// (s.Flags & 4) means packed samples,
						// but did they really exist in DSM?
						curSmp++;
					}
					else
					{
						if ((blockId == PattId) && (curPat < of.NumPat))
						{
							if (!ReadPattern(moduleStream))
							{
								errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
								return false;
							}

							for (int t = 0; t < of.NumChn; t++)
							{
								if ((of.Tracks[track++] = ConvertTrack(dsmBuf, t * 64, uniTrk)) == null)
								{
									errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
									return false;
								}
							}

							curPat++;
						}
					}
				}
			}
			finally
			{
				dsmBuf = null;
				mh = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will read the next block header
		/// </summary>
		/********************************************************************/
		private bool GetBlockHeader(ModuleStream moduleStream)
		{
			// Make sure we're at the right position for reading the
			// next riff block, no matter how many bytes read
			moduleStream.Seek(blockLp + blockLn, SeekOrigin.Begin);

			while (true)
			{
				blockId = moduleStream.Read_B_UINT32();
				blockLn = moduleStream.Read_L_UINT32();

				if (moduleStream.EndOfStream)
					return false;

				if ((blockId != SongId) && (blockId != InstId) && (blockId != PattId))
				{
					// Skip unknown block type
					moduleStream.Seek(blockLn, SeekOrigin.Current);
				}
				else
					break;
			}

			blockLp = (uint)moduleStream.Position;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the pattern data
		/// </summary>
		/********************************************************************/
		private bool ReadPattern(ModuleStream moduleStream)
		{
			int row = 0;

			for (int i = 0; i < MaxChan * 64; i++)
			{
				DsmNote n = dsmBuf[i];
				n.Note = n.Ins = n.Vol = n.Cmd = n.Inf = 255;
			}

			short length = (short)moduleStream.Read_L_UINT16();

			while (row < 64)
			{
				int flag = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream || (--length < 0))
					return false;

				if (flag != 0)
				{
					DsmNote n = dsmBuf[((flag & 0xf) * 64) + row];

					if ((flag & 0x80) != 0)
						n.Note = moduleStream.Read_UINT8();

					if ((flag & 0x40) != 0)
						n.Ins = moduleStream.Read_UINT8();

					if ((flag & 0x20) != 0)
						n.Vol = moduleStream.Read_UINT8();

					if ((flag & 0x10) != 0)
					{
						n.Cmd = moduleStream.Read_UINT8();
						n.Inf = moduleStream.Read_UINT8();
					}
				}
				else
					row++;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a track to uni format
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(DsmNote[] tr, int offset, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < 64; t++)
			{
				byte note = tr[offset + t].Note;
				byte ins = tr[offset + t].Ins;
				byte vol = tr[offset + t].Vol;
				byte cmd = tr[offset + t].Cmd;
				byte inf = tr[offset + t].Inf;

				if ((ins != 0) && (ins != 255))
					uniTrk.UniInstrument((ushort)(ins - 1));

				if (note != 255)
					uniTrk.UniNote((ushort)(note - 1));		// Normal note

				if (vol < 65)
					uniTrk.UniPtEffect(0xc, vol, of.Flags);

				if (cmd != 255)
				{
					if (cmd == 0x8)
					{
						if (inf == Surround)
							uniTrk.UniEffect(Command.UniItEffectS0, 0x91);
						else
						{
							if (inf <= 0x80)
							{
								inf = (byte)((inf < 0x80) ? inf << 1 : 255);
								uniTrk.UniPtEffect(cmd, inf, of.Flags);
							}
						}
					}
					else
					{
						if (cmd == 0xb)
						{
							if (inf <= 0x7f)
								uniTrk.UniPtEffect(cmd, inf, of.Flags);
						}
						else
						{
							// Convert pattern jump from dec to hex
							if (cmd == 0xd)
								inf = (byte)((((inf & 0xf0) >> 4) * 10) + (inf & 0xf));

							uniTrk.UniPtEffect(cmd, inf, of.Flags);
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

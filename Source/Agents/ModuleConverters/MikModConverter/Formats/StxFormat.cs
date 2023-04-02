/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
	/// MikMod loader for STX (STMIK) format
	/// </summary>
	internal class StxFormat : MikModConverterWorkerBase
	{
		#region StxHeader class
		private class StxHeader
		{
			public readonly byte[] SongName = new byte[21];
			public readonly byte[] TrackerName = new byte[9];
			public ushort PatSize;
			public ushort Unknown1;
			public ushort PatPtr;
			public ushort InsPtr;
			public ushort ChnPtr;			// Not sure
			public ushort Unknown2;
			public ushort Unknown3;
			public byte MasterMult;
			public byte InitSpeed;
			public ushort Unknown4;
			public ushort Unknown5;
			public ushort PatNum;
			public ushort InsNum;
			public ushort OrdNum;
			public ushort Unknown6;
			public ushort Unknown7;
			public ushort Unknown8;
			public readonly byte[] Scrm = new byte[4];
		}
		#endregion

		#region StxSample class
		private class StxSample
		{
			public byte Type;
			public readonly byte[] FileName = new byte[13];
			public byte MemSegH;
			public ushort MemSegL;
			public uint Length;
			public uint LoopBeg;
			public uint LoopEnd;
			public byte Volume;
			public byte Dsk;
			public byte Pack;
			public byte Flags;
			public uint C2Spd;
			public readonly byte[] Unused = new byte[12];
			public readonly byte[] SampName = new byte[29];
			public readonly byte[] Scrs = new byte[4];
		}
		#endregion

		#region StxNote class
		private class StxNote
		{
			public byte Note;
			public byte Ins;
			public byte Vol;
			public byte Cmd;
			public byte Inf;
		}
		#endregion

		private static readonly byte[][] signatures =
		{
			Encoding.ASCII.GetBytes("!Scream!"),
			Encoding.ASCII.GetBytes("BMOD2STM"),
			Encoding.ASCII.GetBytes("WUZAMOD!")
		};

		private MlUtil util;

		private StxHeader mh;
		private StxNote[] stxBuf;
		private ushort[] paraPtr;

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
			if (fileSize < 64)								// Size of header
				return AgentResult.Unknown;

			// Check for the S3M signature
			byte[] buf = new byte[8];

			moduleStream.Seek(0x3c, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 4);

			if ((buf[0] == 'S') && (buf[1] == 'C') && (buf[2] == 'R') && (buf[3] == 'M'))
				return AgentResult.Ok;

			// Now check the signatures
			moduleStream.Seek(0x14, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 8);

			foreach (byte[] sig in signatures)
			{
				if (ArrayHelper.ArrayCompare(buf, 0, sig, 0, 8))
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

			util = new MlUtil();

			stxBuf = ArrayHelper.InitializeArray<StxNote>(4 * 64);
			mh = new StxHeader();
			util.posLookup = new byte[256];

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				Array.Fill<byte>(util.posLookup, 255);

				// Try to read the module header
				moduleStream.ReadString(mh.SongName, 20);
				moduleStream.ReadString(mh.TrackerName, 8);

				mh.PatSize = moduleStream.Read_L_UINT16();
				mh.Unknown1 = moduleStream.Read_L_UINT16();
				mh.PatPtr = moduleStream.Read_L_UINT16();
				mh.InsPtr = moduleStream.Read_L_UINT16();
				mh.ChnPtr = moduleStream.Read_L_UINT16();
				mh.Unknown2 = moduleStream.Read_L_UINT16();
				mh.Unknown3 = moduleStream.Read_L_UINT16();
				mh.MasterMult = moduleStream.Read_UINT8();
				mh.InitSpeed = (byte)(moduleStream.Read_UINT8() >> 4);
				mh.Unknown4 = moduleStream.Read_L_UINT16();
				mh.Unknown5 = moduleStream.Read_L_UINT16();
				mh.PatNum = moduleStream.Read_L_UINT16();
				mh.InsNum = moduleStream.Read_L_UINT16();
				mh.OrdNum = moduleStream.Read_L_UINT16();
				mh.Unknown6 = moduleStream.Read_L_UINT16();
				mh.Unknown7 = moduleStream.Read_L_UINT16();
				mh.Unknown8 = moduleStream.Read_L_UINT16();
				moduleStream.Read(mh.Scrm, 0, 4);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if ((mh.OrdNum > 256) || (mh.InsNum == 0) || (mh.InsNum > 256) || (mh.PatNum > 254) || (mh.PatNum == 0))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				// Set module variables
				of.SongName = encoder.GetString(mh.SongName);
				of.NumPat = mh.PatNum;
				of.RepPos = 0;
				of.NumIns = of.NumSmp = mh.InsNum;
				of.InitSpeed = mh.InitSpeed;
				of.InitTempo = 125;
				of.NumChn = 4;
				of.Flags |= ModuleFlag.S3MSlides;
				of.BpmLimit = 32;

				paraPtr = new ushort[of.NumIns + of.NumPat];

				// Read the instrument + patterns para pointers
				moduleStream.Seek(mh.InsPtr << 4, SeekOrigin.Begin);
				moduleStream.ReadArray_L_UINT16s(paraPtr, 0, of.NumIns);

				moduleStream.Seek(mh.PatPtr << 4, SeekOrigin.Begin);
				moduleStream.ReadArray_L_UINT16s(paraPtr, of.NumIns, of.NumPat);

				// Check the module version
				moduleStream.Seek(paraPtr[of.NumIns] << 4, SeekOrigin.Begin);
				int version = moduleStream.Read_L_UINT16();

				if (version == mh.PatSize)
				{
					version = 0x10;
					originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_STX, 0);
				}
				else
				{
					version = 0x11;
					originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_STX, 1);
				}

				// Read the order data
				moduleStream.Seek((mh.ChnPtr << 4) + 32, SeekOrigin.Begin);

				if (!MLoader.AllocPositions(of, mh.OrdNum))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (int t = 0; t < mh.OrdNum; t++)
				{
					of.Positions[t] = moduleStream.Read_UINT8();
					moduleStream.Seek(4, SeekOrigin.Current);
				}

				of.NumPos = 0;
				util.posLookupCnt = mh.OrdNum;

				for (int t = 0; t < mh.OrdNum; t++)
				{
					int order = of.Positions[t];
					if (order == 255)
						order = SharedConstant.Last_Pattern;

					of.Positions[of.NumPos] = (ushort)order;
					util.posLookup[t] = (byte)of.NumPos;			// Bug fix for freaky S3Ms

					if (of.Positions[t] < 254)
						of.NumPos++;
					else
					{
						// Special end of song pattern
						if ((order == SharedConstant.Last_Pattern) && !curious)
							break;
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Load samples
				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				for (int t = 0; t < of.NumIns; t++)
				{
					StxSample s = new StxSample();
					Sample q = of.Samples[t];

					// Seek to instrument position
					moduleStream.Seek((paraPtr[t]) << 4, SeekOrigin.Begin);

					// And load sample info
					s.Type = moduleStream.Read_UINT8();

					moduleStream.ReadString(s.FileName, 12);

					s.MemSegH = moduleStream.Read_UINT8();
					s.MemSegL = moduleStream.Read_L_UINT16();
					s.Length = moduleStream.Read_L_UINT32();
					s.LoopBeg = moduleStream.Read_L_UINT32();
					s.LoopEnd = moduleStream.Read_L_UINT32();
					s.Volume = moduleStream.Read_UINT8();
					s.Dsk = moduleStream.Read_UINT8();
					s.Pack = moduleStream.Read_UINT8();
					s.Flags = moduleStream.Read_UINT8();
					s.C2Spd = moduleStream.Read_L_UINT32();

					moduleStream.Read(s.Unused, 0, 12);
					moduleStream.ReadString(s.SampName, 28);
					moduleStream.Read(s.Scrs, 0, 4);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					q.SampleName = encoder.GetString(s.SampName);
					q.Speed = (s.C2Spd * 8363) / 8448;
					q.Length = s.Length;
					q.LoopStart = s.LoopBeg;
					q.LoopEnd = s.LoopEnd;
					q.Volume = s.Volume;
					q.SeekPos = (uint)((s.MemSegH << 16 | s.MemSegL) << 4);
					q.Flags |= SampleFlag.Signed;

					if ((s.Flags & 1) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 4) != 0)
						q.Flags |= SampleFlag._16Bits;
				}

				// Load pattern info
				of.NumTrk = (ushort)(of.NumPat * of.NumChn);

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

				int track = 0;

				for (int t = 0; t < of.NumPat; t++)
				{
					// Seek to pattern position (+2 skip pattern length)
					moduleStream.Seek((paraPtr[of.NumIns + t] << 4) + (version == 0x10 ? 2 : 0), SeekOrigin.Begin);
					if (!ReadPattern(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					for (int u = 0; u < of.NumChn; u++)
					{
						if ((of.Tracks[track++] = ConvertTrack(stxBuf, u * 64, uniTrk)) == null)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
							return false;
						}
					}
				}
			}
			finally
			{
				stxBuf = null;
				paraPtr = null;
				util.posLookup = null;
				mh = null;

				util = null;
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
			int row = 0;
			StxNote dummy = new StxNote();

			// Clear pattern data
			for (int i = 0; i < 4 * 64; i++)
			{
				StxNote n = stxBuf[i];
				n.Note = n.Ins = n.Vol = n.Cmd = n.Inf = 255;
			}

			while (row < 64)
			{
				int flag = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (flag != 0)
				{
					int ch = flag & 31;

					StxNote n;
					if ((ch >= 0) && (ch < 4))
						n = stxBuf[(64 * ch) + row];
					else
						n = dummy;

					if ((flag & 32) != 0)
					{
						n.Note = moduleStream.Read_UINT8();
						n.Ins = moduleStream.Read_UINT8();
					}

					if ((flag & 64) != 0)
					{
						n.Vol = moduleStream.Read_UINT8();
						if (n.Vol > 64)
							n.Vol = 64;
					}

					if ((flag & 128) != 0)
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
		/// Convert one track in one pattern
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(StxNote[] tr, int offset, MUniTrk uniTrk)
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

				if ((note != 0) && (note != 255))
				{
					if (note == 254)
					{
						uniTrk.UniPtEffect(0xc, 0, of.Flags);		// Note cut command
						vol = 255;
					}
					else
						uniTrk.UniNote((ushort)(24 + ((note >> 4) * SharedConstant.Octave) + (note & 0xf)));		// Normal note
				}

				if (vol < 255)
					uniTrk.UniPtEffect(0xc, vol, of.Flags);

				if (cmd < 255)
				{
					switch (cmd)
					{
						// Axx set speed to xx
						case 1:
						{
							uniTrk.UniPtEffect(0xf, (byte)(inf >> 4), of.Flags);
							break;
						}

						// Bxx position jump
						case 2:
						{
							uniTrk.UniPtEffect(0xb, inf, of.Flags);
							break;
						}

						// Cxx pattern break to row xx
						case 3:
						{
							uniTrk.UniPtEffect(0xd, (byte)((((inf & 0xf0) >> 4) * 10) + (inf & 0xf)), of.Flags);
							break;
						}

						// Dxy volume slide
						case 4:
						{
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Exy tone slide down
						case 5:
						{
							uniTrk.UniEffect(Command.UniS3MEffectE, inf);
							break;
						}

						// Fxy tone slide up
						case 6:
						{
							uniTrk.UniEffect(Command.UniS3MEffectF, inf);
							break;
						}

						// Gxx tone portamento, speed xx
						case 7:
						{
							uniTrk.UniPtEffect(0x3, inf, of.Flags);
							break;
						}

						// Hxx vibrato
						case 8:
						{
							uniTrk.UniPtEffect(0x4, inf, of.Flags);
							break;
						}

						// Ixy tremor, ontime x, offtime y
						case 9:
						{
							uniTrk.UniEffect(Command.UniS3MEffectI, inf);
							break;
						}

						// Protracker arpeggio
						case 0:
						{
							if (inf == 0)
								break;

							goto case 0xa;
						}

						// Jxy arpeggio
						case 0xa:
						{
							uniTrk.UniPtEffect(0x0, inf, of.Flags);
							break;
						}

						// Kxy dual command H00 & Dxy
						case 0xb:
						{
							uniTrk.UniPtEffect(0x4, 0, of.Flags);
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Lxy dual command G00 & Dxy
						case 0xc:
						{
							uniTrk.UniPtEffect(0x3, 0, of.Flags);
							uniTrk.UniEffect(Command.UniS3MEffectD, inf);
							break;
						}

						// Support all these above, since ST2 can LOAD these values
						// but can actually only play up to J - and J is only
						// half-way implemented in ST2
						//
						// Xxx amiga command 8xx
						case 0x18:
						{
							uniTrk.UniPtEffect(0x8, inf, of.Flags);
							of.Flags |= ModuleFlag.Panning;
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

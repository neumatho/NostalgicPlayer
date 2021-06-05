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
	/// MikMod loader for STM (Stream Tracker 2) format
	/// </summary>
	internal class MikModConverterWorker_Stm : MikModConverterWorkerBase
	{
		#region StmSample class
		private class StmSample
		{
			public readonly byte[] FileName = new byte[13];
			public byte Unused;											// 0x00
			public byte InstDisk;										// Instrument disk
			public ushort Reserved;
			public ushort Length;										// Sample length
			public ushort LoopBeg;										// Loop start point
			public ushort LoopEnd;										// Loop end point
			public byte Volume;											// Volume
			public byte Reserved2;
			public ushort C2Spd;										// Good old c2spd
			public uint Reserved3;
			public ushort Isa;
		}
		#endregion

		#region StmHeader class
		private class StmHeader
		{
			public readonly byte[] SongName = new byte[21];
			public readonly byte[] TrackerName = new byte[9];			// !Scream! for ST 2.xx
			public byte Unused;
			public byte FileType;										// 1 = Song, 2 = Module
			public byte Ver_Major;
			public byte Ver_Minor;
			public byte InitTempo;										// InitSpeed = stm InitTempo >> 4
			public byte NumPat;											// Number of patterns
			public byte GlobalVol;
			public readonly byte[] Reserved = new byte[13];
			public readonly StmSample[] Sample = Helpers.InitializeArray<StmSample>(31);	// STM sample data
			public readonly byte[] PatOrder = new byte[128];			// Docs say 64 - actually 128
		}
		#endregion

		#region StmNote class
		private class StmNote
		{
			public byte Note;
			public byte InsVol;
			public byte VolCmd;
			public byte CmdInf;
		}
		#endregion

		private static readonly byte[][] signatures =
		{
			Encoding.ASCII.GetBytes("!Scream!"),
			Encoding.ASCII.GetBytes("BMOD2STM"),
			Encoding.ASCII.GetBytes("WUZAMOD!")
		};

		private static readonly string[] versions =
		{
			string.Empty,
			Resources.IDS_MIKCONV_NAME_STM_MOD2STM,
			Resources.IDS_MIKCONV_NAME_STM_WUZAMOD
		};

		private StmHeader mh;
		private StmNote[] stmBuf;

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
			if (fileSize < 176)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature and file type
			byte[] buf = new byte[44];

			moduleStream.Seek(20, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 44);

			// Check the file type
			if (buf[9] != 2)
				return AgentResult.Unknown;

			// Prevent false positives for S3M files
			if ((buf[40] == 'S') && (buf[41] == 'C') && (buf[42] == 'R') && (buf[43] == 'M'))
				return AgentResult.Unknown;

			// Check for the signatures
			foreach (byte[] sig in signatures)
			{
				if (Helpers.ArrayCompare(buf, 0, sig, 0, 8))
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

			mh = new StmHeader();
			stmBuf = Helpers.InitializeArray<StmNote>(64 * 4);

			try
			{
				int t;

				Encoding encoder = EncoderCollection.Dos;

				// Try to read the header
				moduleStream.ReadString(mh.SongName, 20);
				moduleStream.ReadString(mh.TrackerName, 8);

				mh.Unused = moduleStream.Read_UINT8();
				mh.FileType = moduleStream.Read_UINT8();
				mh.Ver_Major = moduleStream.Read_UINT8();
				mh.Ver_Minor = moduleStream.Read_UINT8();
				mh.InitTempo = moduleStream.Read_UINT8();

				if (mh.InitTempo == 0)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				mh.NumPat = moduleStream.Read_UINT8();
				mh.GlobalVol = moduleStream.Read_UINT8();

				moduleStream.Read(mh.Reserved, 0, 13);

				if (mh.NumPat > 128)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				for (t = 0; t < 31; t++)
				{
					StmSample s = mh.Sample[t];			// STM sample data

					moduleStream.ReadString(s.FileName, 12);

					s.Unused = moduleStream.Read_UINT8();
					s.InstDisk = moduleStream.Read_UINT8();
					s.Reserved = moduleStream.Read_L_UINT16();
					s.Length = moduleStream.Read_L_UINT16();
					s.LoopBeg = moduleStream.Read_L_UINT16();
					s.LoopEnd = moduleStream.Read_L_UINT16();
					s.Volume = moduleStream.Read_UINT8();
					s.Reserved2 = moduleStream.Read_UINT8();
					s.C2Spd = moduleStream.Read_L_UINT16();
					s.Reserved3 = moduleStream.Read_L_UINT32();
					s.Isa = moduleStream.Read_L_UINT16();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}
				}

				moduleStream.Read(mh.PatOrder, 0, 128);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Set module variables
				for (t = 0; t < signatures.Length; t++)
				{
					if (Helpers.ArrayCompare(mh.TrackerName, 0, signatures[t], 0, 8))
						break;
				}

				originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_STM, versions[t]);

				of.SongName = encoder.GetString(mh.SongName);
				of.NumPat = mh.NumPat;
				of.InitTempo = 125;
				of.InitSpeed = (byte)(mh.InitTempo >> 4);
				of.NumChn = 4;
				of.RepPos = 0;
				of.Flags |= ModuleFlag.S3MSlides;
				of.BpmLimit = 32;

				t = 0;

				if (!MLoader.AllocPositions(of, 0x80))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// 99 terminates the PatOrder list
				while ((mh.PatOrder[t] <= 99) && (mh.PatOrder[t] < mh.NumPat))
				{
					of.Positions[t] = mh.PatOrder[t];
					if (++t == 0x80)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
				}

				of.NumPos = (ushort)t;
				of.NumTrk = (ushort)(of.NumPat * of.NumChn);
				of.NumIns = of.NumSmp = 31;

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				if (!LoadPatterns(moduleStream, uniTrk))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}

				uint sampleStart = (uint)moduleStream.Position;
				uint sampleEnd = (uint)moduleStream.Length;

				for (t = 0; t < of.NumSmp; t++)
				{
					Sample q = of.Samples[t];

					// Load sample info
					q.SampleName = encoder.GetString(mh.Sample[t].FileName);
					q.Speed = (uint)((mh.Sample[t].C2Spd * 8363) / 8448);
					q.Volume = mh.Sample[t].Volume;
					q.Length = mh.Sample[t].Length;

					if ((mh.Sample[t].Volume == 0) || (q.Length == 1))
						q.Length = 0;

					q.LoopStart = mh.Sample[t].LoopBeg;
					q.LoopEnd = mh.Sample[t].LoopEnd;
					q.SeekPos = (uint)(mh.Sample[t].Reserved << 4);

					// Sanity check to make sure samples are bounded within the file
					if (q.Length != 0)
					{
						if (q.SeekPos < sampleStart)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
							return false;
						}

						// Some .STMs seem to rely on allowing truncated samples...
						if (q.SeekPos >= sampleEnd)
							q.SeekPos = q.Length = 0;
						else
						{
							if (q.SeekPos + q.Length > sampleEnd)
								q.Length = sampleEnd - q.SeekPos;
						}
					}
					else
						q.SeekPos = 0;

					// Contrary to the STM specs, sample data is signed
					q.Flags = SampleFlag.Signed;

					if ((q.LoopEnd != 0) && (q.LoopEnd != 0xffff) && (q.LoopStart < q.Length))
					{
						q.Flags |= SampleFlag.Loop;
						if (q.LoopEnd > q.Length)
							q.LoopEnd = q.Length;
					}
					else
						q.LoopStart = q.LoopEnd = 0;
				}
			}
			finally
			{
				mh = null;
				stmBuf = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read all the patterns
		/// </summary>
		/********************************************************************/
		private bool LoadPatterns(ModuleStream moduleStream, MUniTrk uniTrk)
		{
			if (!MLoader.AllocPatterns(of))
				return false;

			if (!MLoader.AllocTracks(of))
				return false;

			// Allocate temporary buffer for loading and converting the patterns
			int tracks = 0;

			for (int t = 0; t < of.NumPat; t++)
			{
				for (int s = 0; s < (64 * of.NumChn); s++)
				{
					stmBuf[s].Note = moduleStream.Read_UINT8();
					stmBuf[s].InsVol = moduleStream.Read_UINT8();
					stmBuf[s].VolCmd = moduleStream.Read_UINT8();
					stmBuf[s].CmdInf = moduleStream.Read_UINT8();
				}

				if (moduleStream.EndOfStream)
					return false;

				for (int s = 0; s < of.NumChn; s++)
				{
					if ((of.Tracks[tracks++] = ConvertTrack(stmBuf, s, uniTrk)) == null)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert one track in one pattern
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(StmNote[] n, int offset, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < 64; t++)
			{
				ConvertNote(n[offset], uniTrk);
				uniTrk.UniNewLine();
				offset += of.NumChn;
			}

			return uniTrk.UniDup();
		}



		/********************************************************************/
		/// <summary>
		/// Convert a note to UniMod commands
		/// </summary>
		/********************************************************************/
		private void ConvertNote(StmNote n, MUniTrk uniTrk)
		{
			// Extract the various information from the 4 bytes that make up a note
			byte note = n.Note;
			byte ins = (byte)(n.InsVol >> 3);
			byte vol = (byte)((n.InsVol & 7) + ((n.VolCmd & 0x70) >> 1));
			byte cmd = (byte)(n.VolCmd & 15);
			byte inf = n.CmdInf;

			if ((ins != 0) && (ins < 32))
				uniTrk.UniInstrument((ushort)(ins - 1));

			// Special values of [SBYTE0] are handled here.
			// We have no idea if these strange values will ever be encountered,
			// but it appears as those stms sound correct
			if ((note == 254) || (note == 252))
			{
				uniTrk.UniPtEffect(0xc, 0, of.Flags);		// Note cut
				n.VolCmd |= 0x80;
			}
			else
			{
				// If note < 251, then all three bytes are stored in the file
				if (note < 251)
					uniTrk.UniNote((ushort)((((note >> 4) + 2) * SharedConstant.Octave) + (note & 0xf)));
			}

			if (((n.VolCmd & 0x80) == 0) && (vol < 65))
				uniTrk.UniPtEffect(0xc, vol, of.Flags);

			if (cmd != 255)
			{
				switch (cmd)
				{
					// Axx: Set speed to xx
					case 1:
					{
						uniTrk.UniPtEffect(0xf, (byte)(inf >> 4), of.Flags);
						break;
					}

					// Bxx: Position jump
					case 2:
					{
						uniTrk.UniPtEffect(0xb, inf, of.Flags);
						break;
					}

					// Cxx: Pattern break to row xx
					case 3:
					{
						uniTrk.UniPtEffect(0xd, (byte)((((inf & 0xf0) >> 4) * 10) + (inf & 0xf)), of.Flags);
						break;
					}

					// Dxy: Volume slide
					case 4:
					{
						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Exy: Tone slide down
					case 5:
					{
						uniTrk.UniEffect(Command.UniS3MEffectE, inf);
						break;
					}

					// Fxy: Tone slide up
					case 6:
					{
						uniTrk.UniEffect(Command.UniS3MEffectF, inf);
						break;
					}

					// Gxx: Tone portamento, speed xx
					case 7:
					{
						uniTrk.UniPtEffect(0x3, inf, of.Flags);
						break;
					}

					// Hxy: Position jump
					case 8:
					{
						uniTrk.UniPtEffect(0x4, inf, of.Flags);
						break;
					}

					// Ixy: Tremor, ontime x, offtime y
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

					// Jxy: Arpeggio
					case 0xa:
					{
						uniTrk.UniPtEffect(0x0, inf, of.Flags);
						break;
					}

					// Kxy: Dual command H00 & Dxy
					case 0xb:
					{
						uniTrk.UniPtEffect(0x4, 0, of.Flags);
						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Lxy: Dual command G00 & Dxy
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
					// Xxx: Amiga panning command 8xx
					case 0x18:
					{
						uniTrk.UniPtEffect(0x8, inf, of.Flags);
						of.Flags |= ModuleFlag.Panning;
						break;
					}
				}
			}
		}
		#endregion
	}
}

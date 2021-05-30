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
	/// MikMod loader for S3M (Stream Tracker 3) format
	/// </summary>
	internal class MikModConverterWorker_S3M : MikModConverterWorkerBase
	{
		#region S3MHeader class
		private class S3MHeader
		{
			public readonly byte[] SongName = new byte[29];
			public byte T1A;
			public byte Type;
			public readonly byte[] Unused1 = new byte[2];
			public ushort OrdNum;
			public ushort InsNum;
			public ushort PatNum;
			public ushort Flags;
			public ushort Tracker;
			public ushort FileFormat;
			public readonly byte[] Scrm = new byte[4];
			public byte MasterVol;
			public byte InitSpeed;
			public byte InitTempo;
			public byte MasterMult;
			public byte UltraClick;
			public byte PanTable;
			public readonly byte[] Unused2 = new byte[8];
			public ushort Special;
			public readonly byte[] Channels = new byte[32];
		}
		#endregion

		#region S3MSample class
		private class S3MSample
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

		#region S3MNote class
		private class S3MNote
		{
			public byte Note;
			public byte Ins;
			public byte Vol;
			public byte Cmd;
			public byte Inf;
		}
		#endregion

		private const int NumTrackers = 4;

		private static readonly string[] versions =
		{
			Resources.IDS_MIKCONV_NAME_S3M,
			Resources.IDS_MIKCONV_NAME_S3M_IMAGO,
			Resources.IDS_MIKCONV_NAME_S3M_IT,
			Resources.IDS_MIKCONV_NAME_S3M_UNKNOWN,
			Resources.IDS_MIKCONV_NAME_S3M_IT_214P3,
			Resources.IDS_MIKCONV_NAME_S3M_IT_214P4
		};

		private MlUtil util;

		private S3MHeader mh;
		private S3MNote[] s3mBuf;
		private ushort[] paraPtr;
		private uint tracker;

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
			if (fileSize < 96)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] buf = new byte[4];

			moduleStream.Seek(0x2c, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 4);

			if ((buf[0] == 'S') && (buf[1] == 'C') && (buf[2] == 'R') && (buf[3] == 'M'))
				return AgentResult.Ok;

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

			s3mBuf = Helpers.InitializeArray<S3MNote>(32 * 64);
			mh = new S3MHeader();
			util.posLookup = new byte[256];

			try
			{
				Encoding encoder = EncoderCollection.Ibm850;

				Array.Fill<byte>(util.posLookup, 255);

				// Try to read module header
				moduleStream.ReadString(mh.SongName, 28);
				mh.T1A = moduleStream.Read_UINT8();
				mh.Type = moduleStream.Read_UINT8();
				moduleStream.Read(mh.Unused1, 0, 2);

				mh.OrdNum = moduleStream.Read_L_UINT16();
				mh.InsNum = moduleStream.Read_L_UINT16();
				mh.PatNum = moduleStream.Read_L_UINT16();
				mh.Flags = moduleStream.Read_L_UINT16();
				mh.Tracker = moduleStream.Read_L_UINT16();
				mh.FileFormat = moduleStream.Read_L_UINT16();
				moduleStream.Read(mh.Scrm, 0, 4);

				mh.MasterVol = moduleStream.Read_UINT8();
				mh.InitSpeed = moduleStream.Read_UINT8();
				mh.InitTempo = moduleStream.Read_UINT8();
				mh.MasterMult = moduleStream.Read_UINT8();
				mh.UltraClick = moduleStream.Read_UINT8();
				mh.PanTable = moduleStream.Read_UINT8();
				moduleStream.Read(mh.Unused2, 0, 8);

				mh.Special = moduleStream.Read_L_UINT16();
				moduleStream.Read(mh.Channels, 0, 32);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if ((mh.OrdNum > 255) || (mh.InsNum > 255) || (mh.PatNum > 255))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				// Then we can decide the module type
				tracker = (uint)(mh.Tracker >> 12);

				if ((tracker == 0) || (tracker >= NumTrackers))
					tracker = NumTrackers - 1;				// Unknown tracker
				else
				{
					if (mh.Tracker >= 0x3217)
						tracker = NumTrackers + 1;			// IT 2.14p4
					else
					{
						if (mh.Tracker >= 0x3216)			// IT 2.14p3
							tracker = NumTrackers;
						else
							tracker--;
					}
				}

				if (tracker < NumTrackers)
					originalFormat = string.Format(versions[tracker], (mh.Tracker >> 8) & 0xf, ((mh.Tracker >> 4) & 0xf) * 10 + (mh.Tracker & 0xf));
				else
					originalFormat = versions[tracker];

				// Set module variables
				of.SongName = encoder.GetString(mh.SongName);
				of.NumPat = mh.PatNum;
				of.RepPos = 0;
				of.NumIns = of.NumSmp = mh.InsNum;
				of.InitSpeed = mh.InitSpeed;
				of.InitTempo = mh.InitTempo;
				of.InitVolume = (byte)(mh.MasterVol << 1);
				of.Flags |= ModuleFlag.ArpMem | ModuleFlag.Panning;

				if ((mh.Tracker == 0x1300) || ((mh.Flags & 64) != 0))
					of.Flags |= ModuleFlag.S3MSlides;

				of.BpmLimit = 32;

				// Read the order data
				if (!MLoader.AllocPositions(of, mh.OrdNum))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				util.origPositions = new ushort[mh.OrdNum];

				for (int t = 0; t < mh.OrdNum; t++)
				{
					util.origPositions[t] = moduleStream.Read_UINT8();

					if ((util.origPositions[t] >= mh.PatNum) && (util.origPositions[t] < 254))
						util.origPositions[t] = 255;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				util.posLookupCnt = mh.OrdNum;
				util.CreateOrders(of, curious);

				paraPtr = new ushort[of.NumIns + of.NumPat];

				// Read the instrument + pattern parapointers
				moduleStream.ReadArray_L_UINT16s(paraPtr, 0, of.NumIns + of.NumPat);

				byte[] pan = new byte[32];

				if (mh.PanTable == 252)
				{
					// Read the panning table (ST 3.2 addition. See below for further
					// portions of channel panning [past reampper])
					moduleStream.Read(pan, 0, 32);
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
					S3MSample s = new S3MSample();
					Sample q = of.Samples[t];

					// Seek to instrument position
					moduleStream.Seek(paraPtr[t] << 4, SeekOrigin.Begin);

					// And load the sample info
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

					// ScreamTracker imposes a 64000 bytes (not 64k !) limit.
					// Enforce it, if we'll use S3MIT_SCREAM in S3M_ConvertTrack()
					if ((s.Length > 64000) && (tracker == 1))
						s.Length = 64000;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					q.SampleName = encoder.GetString(s.SampName);
					q.Speed = s.C2Spd;
					q.Length = s.Length;
					q.LoopStart = s.LoopBeg;
					q.LoopEnd = s.LoopEnd;
					q.Volume = s.Volume;
					q.SeekPos = ((((uint)s.MemSegH) << 16) | s.MemSegL) << 4;

					if ((s.Flags & 1) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 4) != 0)
						q.Flags |= SampleFlag._16Bits;

					if (mh.FileFormat == 1)
						q.Flags |= SampleFlag.Signed;

					// Don't load sample if it doesn't have the SCRS tag
					if ((s.Scrs[0] != 'S') || (s.Scrs[1] != 'C') || (s.Scrs[2] != 'R') || (s.Scrs[3] != 'S'))
						q.Length = 0;
				}

				// Determine the number of channels actually used
				of.NumChn = 0;
				Array.Fill<byte>(util.remap, 255, 0, 32);

				for (int t = 0; t < of.NumPat; t++)
				{
					// Seek to pattern position (+ 2 skip pattern length)
					moduleStream.Seek((paraPtr[of.NumIns + t] << 4) + 2, SeekOrigin.Begin);

					if (!GetNumChannels(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}
				}

				// Build the remap array
				for (int t = 0; t < 32; t++)
				{
					if (util.remap[t] == 0)
						util.remap[t] = of.NumChn++;
				}

				// Set panning positions after building remap chart!
				for (int t = 0; t < 32; t++)
				{
					if ((mh.Channels[t] < 32) && (util.remap[t] != 255))
					{
						if (mh.Channels[t] < 8)
							of.Panning[util.remap[t]] = 0x30;
						else
							of.Panning[util.remap[t]] = 0xc0;
					}
				}

				if (mh.PanTable == 252)
				{
					// Set panning positions according to panning table (new for st3.2)
					for (int t = 0; t < 32; t++)
					{
						if (((pan[t] & 0x20) != 0) && (mh.Channels[t] < 32) && (util.remap[t] != 255))
							of.Panning[util.remap[t]] = (ushort)((pan[t] & 0xf) << 4);
					}
				}

				// Load the pattern info
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
					// Seek to pattern position (+ 2 skip pattern length)
					moduleStream.Seek((paraPtr[of.NumIns + t] << 4) + 2, SeekOrigin.Begin);

					if (!ReadPattern(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					for (int u = 0; u < of.NumChn; u++)
					{
						if ((of.Tracks[track++] = ConvertTrack(s3mBuf, u * 64, uniTrk)) == null)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
							return false;
						}
					}
				}
			}
			finally
			{
				s3mBuf = null;
				paraPtr = null;
				util.posLookup = null;
				mh = null;
				util.origPositions = null;

				util = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Because so many s3m files have 16 channels as the set number
		/// used, but really only use far less (usually 8 to 12 still), I
		/// had to make this function, which determines the number of
		/// channels that are actually USED by a pattern.
		///
		/// For every channel that's used, it sets the appropriate array
		/// entry of the global variable 'remap'
		///
		/// NOTE: You must first seek to the file location of the pattern
		/// before calling this procedure
		/// </summary>
		/********************************************************************/
		private bool GetNumChannels(ModuleStream moduleStream)
		{
			int row = 0;

			while (row < 64)
			{
				int flag = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (flag != 0)
				{
					int ch = flag & 31;

					if (mh.Channels[ch] < 32)
						util.remap[ch] = 0;

					if ((flag & 32) != 0)
						moduleStream.Seek(2, SeekOrigin.Current);

					if ((flag & 64) != 0)
						moduleStream.Seek(1, SeekOrigin.Current);

					if ((flag & 128) != 0)
						moduleStream.Seek(2, SeekOrigin.Current);
				}
				else
					row++;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a single pattern
		/// </summary>
		/********************************************************************/
		private bool ReadPattern(ModuleStream moduleStream)
		{
			int row = 0;
			S3MNote dummy = new S3MNote();

			// Clear pattern data
			for (int i = 0; i < 32 * 64; i++)
			{
				S3MNote n = s3mBuf[i];
				n.Note = n.Ins = n.Vol = n.Cmd = n.Inf = 255;
			}

			while (row < 64)
			{
				int flag = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (flag != 0)
				{
					int ch = util.remap[flag & 31];

					S3MNote n;
					if (ch != 255)
						n = s3mBuf[(64 * ch) + row];
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
		/// Convert a track to uni format
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(S3MNote[] tr, int offset, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < 64; t++)
			{
				byte note = tr[offset + t].Note;
				byte ins = tr[offset + t].Ins;
				byte vol = tr[offset + t].Vol;

				if ((ins != 0) && (ins != 255))
					uniTrk.UniInstrument((ushort)(ins - 1));

				if (note != 255)
				{
					if (note == 254)
					{
						uniTrk.UniPtEffect(0xc, 0, of.Flags);		// Note cut command
						vol = 255;
					}
					else
						uniTrk.UniNote((ushort)(((note >> 4) * SharedConstant.Octave) + (note & 0xf)));		// Normal note
				}

				if (vol < 255)
					uniTrk.UniPtEffect(0xc, vol, of.Flags);

				util.ProcessCmd(of, tr[offset + t].Cmd, tr[offset + t].Inf, tracker == 1 ? ProcessFlags.OldStyle | ProcessFlags.Scream : ProcessFlags.OldStyle, uniTrk);

				uniTrk.UniNewLine();
			}

			return uniTrk.UniDup();
		}
		#endregion
	}
}

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

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for UNI (UniMod) format
	/// </summary>
	internal class MikModConverterWorker_Uni : MikModConverterWorkerBase
	{
		#region UniHeader class
		private class UniHeader
		{
			public readonly byte[] Id = new byte[4];
			public byte NumChn;
			public ushort NumPos;
			public ushort RepPos;
			public ushort NumPat;
			public ushort NumTrk;
			public ushort NumIns;
			public ushort NumSmp;
			public byte InitSpeed;
			public byte InitTempo;
			public byte InitVolume;
			public ModuleFlag Flags;
			public byte NumVoices;
			public ushort BpmLimit;

			public readonly byte[] Positions = new byte[256];
			public readonly byte[] Panning = new byte[32];
		}
		#endregion

		#region UniSmp05 class
		private struct UniSmp05
		{
			public ushort C2Spd;
			public ushort Transpose;
			public byte Volume;
			public byte Panning;
			public uint Length;
			public uint LoopStart;
			public uint LoopEnd;
			public ushort Flags;
			public string SampleName;
			public byte VibType;
			public byte VibSweep;
			public byte VibDepth;
			public byte VibRate;
		}
		#endregion

		private const int UniSmpIncr = 64;

		private UniSmp05[] wh;
		private ushort uniVersion;

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
			if (fileSize < 313)								// Size of UniHeader
				return AgentResult.Unknown;

			// Now check the signature
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] id = new byte[6];
			moduleStream.Read(id, 0, 6);

			// UniMod created by MikCvt
			if ((id[0] == 'U') && (id[1] == 'N') && (id[2] == '0'))
			{
				if ((id[3] >= '4') && (id[3] <= '6'))
					return AgentResult.Ok;
			}

			// UniMod created by APlayer
			if ((id[0] == 'A') && (id[1] == 'P') && (id[2] == 'U') && (id[3] == 'N') && (id[4] == 1))
			{
				if ((id[5] >= 1) && (id[5] <= 5))
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

			try
			{
				Encoding encoder = EncoderCollection.Ibm850;

				UniHeader mh = new UniHeader();

				// Clear the buffer pointers
				wh = null;

				// Read module header
				moduleStream.Read(mh.Id, 0, 4);
				if (mh.Id[3] != 'N')
					uniVersion = (ushort)(mh.Id[3] - '0');
				else
					uniVersion = 0x100;

				if (uniVersion >= 6)
				{
					if (uniVersion == 6)
						moduleStream.Seek(1, SeekOrigin.Current);
					else
						uniVersion = moduleStream.Read_B_UINT16();

					mh.Flags = (ModuleFlag)moduleStream.Read_B_UINT16();
					mh.NumChn = moduleStream.Read_UINT8();
					mh.NumVoices = moduleStream.Read_UINT8();
					mh.NumPos = moduleStream.Read_B_UINT16();
					mh.NumPat = moduleStream.Read_B_UINT16();
					mh.NumTrk = moduleStream.Read_B_UINT16();
					mh.NumIns = moduleStream.Read_B_UINT16();
					mh.NumSmp = moduleStream.Read_B_UINT16();
					mh.RepPos = moduleStream.Read_B_UINT16();
					mh.InitSpeed = moduleStream.Read_UINT8();
					mh.InitTempo = moduleStream.Read_UINT8();
					mh.InitVolume = moduleStream.Read_UINT8();

					if (uniVersion >= 0x106)
						mh.BpmLimit = moduleStream.Read_B_UINT16();
					else
						mh.BpmLimit = 32;

					mh.Flags &= ModuleFlag.XmPeriods | ModuleFlag.Linear | ModuleFlag.Inst | ModuleFlag.Nna;
					mh.Flags |= ModuleFlag.Panning;
				}
				else
				{
					mh.NumChn = moduleStream.Read_UINT8();
					mh.NumPos = moduleStream.Read_L_UINT16();
					mh.RepPos = (uniVersion == 5) ? moduleStream.Read_L_UINT16() : (ushort)0;
					mh.NumPat = moduleStream.Read_L_UINT16();
					mh.NumTrk = moduleStream.Read_L_UINT16();
					mh.NumIns = moduleStream.Read_L_UINT16();
					mh.InitSpeed = moduleStream.Read_UINT8();
					mh.InitTempo = moduleStream.Read_UINT8();

					moduleStream.Read(mh.Positions, 0, 256);
					moduleStream.Read(mh.Panning, 0, 32);

					mh.Flags = (ModuleFlag)moduleStream.Read_UINT8();
					mh.BpmLimit = 32;

					mh.Flags &= ModuleFlag.XmPeriods | ModuleFlag.Linear;
					mh.Flags |= ModuleFlag.Inst | ModuleFlag.NoWrap | ModuleFlag.Panning;
				}

				// Set module parameters
				of.Flags = mh.Flags;
				of.NumChn = mh.NumChn;
				of.NumPos = mh.NumPos;
				of.NumPat = mh.NumPat;
				of.NumTrk = mh.NumTrk;
				of.NumIns = mh.NumIns;
				of.RepPos = mh.RepPos;
				of.InitSpeed = mh.InitSpeed;
				of.InitTempo = mh.InitTempo;

				if (mh.BpmLimit != 0)
					of.BpmLimit = mh.BpmLimit;
				else
				{
					// Be bug-compatible with older releases
					of.BpmLimit = 32;
				}

				ushort len = moduleStream.Read_L_UINT16();
				of.SongName = moduleStream.ReadString(encoder, len);

				string oldType = null;

				if (uniVersion < 0x102)
				{
					// Read tracker used
					len = moduleStream.Read_L_UINT16();
					oldType = moduleStream.ReadString(encoder, len);
				}

				if (!string.IsNullOrEmpty(oldType))
					originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_UNI, (uniVersion >= 0x100) ? "APlayer" : "MikCvt2", oldType);
				else
					originalFormat = (uniVersion >= 0x100) ? "APlayer" : "MikCvt3";

				len = moduleStream.Read_L_UINT16();
				of.Comment = moduleStream.ReadString(encoder, len);

				if (uniVersion >= 6)
				{
					of.NumVoices = mh.NumVoices;
					of.InitVolume = mh.InitVolume;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Positions
				if (!MLoader.AllocPositions(of, of.NumPos))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if (uniVersion >= 6)
				{
					if (uniVersion >= 0x100)
						moduleStream.ReadArray_B_UINT16s(of.Positions, 0, of.NumPos);
					else
					{
						for (int t = 0; t < of.NumPos; t++)
							of.Positions[t] = moduleStream.Read_UINT8();
					}

					moduleStream.ReadArray_B_UINT16s(of.Panning, 0, of.NumChn);
					moduleStream.Read(of.ChanVol, 0, of.NumChn);
				}
				else
				{
					if ((mh.NumPos > 256) || (mh.NumChn > 32))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
						return false;
					}

					for (int t = 0; t < of.NumPos; t++)
						of.Positions[t] = mh.Positions[t];

					for (int t = 0; t < of.NumChn; t++)
						of.Panning[t] = mh.Panning[t];
				}

				// Convert the 'end of song' pattern code if necessary
				for (int t = 0; t < of.NumPos; t++)
				{
					if ((uniVersion < 0x106) && (of.Positions[t] == 255))
						of.Positions[t] = SharedConstant.Last_Pattern;
					else
					{
						// Sanity check
						if (of.Positions[t] > of.NumPat)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
							return false;
						}
					}
				}

				// Instruments and samples
				if (uniVersion >= 6)
				{
					of.NumSmp = mh.NumSmp;

					if (!MLoader.AllocSamples(of))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					if (!LoadSmp6(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					if ((of.Flags & ModuleFlag.Inst) != 0)
					{
						if (!MLoader.AllocInstruments(of))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
							return false;
						}

						if (!LoadInstr6(moduleStream))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
							return false;
						}
					}
				}
				else
				{
					if (!MLoader.AllocInstruments(of))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
						return false;
					}

					if (!LoadInstr5(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
						return false;
					}

					if (!MLoader.AllocSamples(of))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					LoadSmp5();

					// Check if the original file has no instruments
					if (of.NumSmp == of.NumIns)
					{
						int t;
						for (t = 0; t < of.NumIns; t++)
						{
							Instrument d = of.Instruments[t];

							if ((d.VolPts != 0) || (d.PanPts != 0) || (d.GlobVol != 64))
								break;

							int u;
							for (u = 0; u < 96; u++)
							{
								if ((d.SampleNumber[u] != t) || (d.SampleNote[u] != u))
									break;
							}

							if (u != 96)
								break;
						}

						if (t == of.NumIns)
						{
							of.Flags &= ~ModuleFlag.Inst;
							of.Flags &= ~ModuleFlag.NoWrap;

							for (t = 0; t < of.NumIns; t++)
							{
								of.Samples[t].SampleName = of.Instruments[t].InsName;
								of.Instruments[t].InsName = string.Empty;
							}
						}
					}
				}

				// Patterns
				if (!MLoader.AllocPatterns(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}

				if (uniVersion >= 6)
				{
					moduleStream.ReadArray_B_UINT16s(of.PattRows, 0, of.NumPat);
					moduleStream.ReadArray_B_UINT16s(of.Patterns, 0, of.NumPat * of.NumChn);
				}
				else
				{
					moduleStream.ReadArray_L_UINT16s(of.PattRows, 0, of.NumPat);
					moduleStream.ReadArray_L_UINT16s(of.Patterns, 0, of.NumPat * of.NumChn);
				}

				// Tracks
				if (!MLoader.AllocTracks(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}

				for (int t = 0; t < of.NumTrk; t++)
				{
					if ((of.Tracks[t] = ReadTrack(moduleStream)) == null)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
						return false;
					}
				}
			}
			finally
			{
				// Clean up again
				wh = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read one track and return it
		/// </summary>
		/********************************************************************/
		private byte[] ReadTrack(ModuleStream moduleStream)
		{
			int cur = 0;

			ushort len;

			if (uniVersion >= 6)
				len = moduleStream.Read_B_UINT16();
			else
				len = moduleStream.Read_L_UINT16();

			if (len == 0)
				return null;

			byte[] t = new byte[len];
			moduleStream.Read(t, 0, len);

			// Check if the track is correct
			while (true)
			{
				int chunk = t[cur++];
				if (chunk == 0)
					break;

				chunk = (chunk & 0x1f) - 1;
				while (chunk > 0)
				{
					if (cur >= len)
						return null;

					int opcode = t[cur];

					// Remap opcodes
					if (uniVersion <= 5)
					{
						if (opcode > 29)
							return null;

						switch (opcode)
						{
							// UNI_NOTE .. UNI_S3MEFFECTQ are the same
							case 25:
							{
								opcode = (int)Command.UniS3MEffectT;
								break;
							}

							case 26:
							{
								opcode = (int)Command.UniXmEffectA;
								break;
							}

							case 27:
							{
								opcode = (int)Command.UniXmEffectG;
								break;
							}

							case 28:
							{
								opcode = (int)Command.UniXmEffectH;
								break;
							}

							case 29:
							{
								opcode = (int)Command.UniXmEffectP;
								break;
							}
						}
					}
					else
					{
						// APlayer < 1.05 does not have XMEFFECT6
						if ((opcode >= (int)Command.UniXmEffect6) && (uniVersion < 0x105))
							opcode++;

						// APlayer < 1.03 does not have ITEFFECTT
						if ((opcode >= (int)Command.UniItEffectT) && (uniVersion < 0x103))
							opcode++;

						// APlayer < 1.02 does not have ITEFFECTZ
						if ((opcode >= (int)Command.UniItEffectZ) && (uniVersion < 0x102))
							opcode++;
					}

					if ((opcode == 0) || (opcode >= (int)Command.UniFormatLast))
						return null;

					t[cur] = (byte)opcode;
					int opLen = SharedLookupTables.UniOperands[opcode] + 1;
					cur += opLen;
					chunk -= opLen;
				}

				if ((chunk < 0) || (cur >= len))
					return null;
			}

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the samples in version 6 format
		/// </summary>
		/********************************************************************/
		private bool LoadSmp6(ModuleStream moduleStream)
		{
			Encoding encoder = EncoderCollection.Ibm850;

			for (int t = 0; t < of.NumSmp; t++)
			{
				Sample s = of.Samples[t];

				int flags = moduleStream.Read_B_UINT16();
				s.Flags = SampleFlag.None;

				if ((flags & 0x0004) != 0)
					s.Flags |= SampleFlag.Stereo;

				if ((flags & 0x0002) != 0)
					s.Flags |= SampleFlag.Signed;

				if ((flags & 0x0001) != 0)
					s.Flags |= SampleFlag._16Bits;

				// Convert flags
				if (uniVersion >= 0x104)
				{
					if ((flags & 0x2000) != 0)
						s.Flags |= SampleFlag.UstLoop;

					if ((flags & 0x1000) != 0)
						s.Flags |= SampleFlag.OwnPan;

					if ((flags & 0x0800) != 0)
						s.Flags |= SampleFlag.Sustain;

					if ((flags & 0x0400) != 0)
						s.Flags |= SampleFlag.Reverse;

					if ((flags & 0x0200) != 0)
						s.Flags |= SampleFlag.Bidi;

					if ((flags & 0x0100) != 0)
						s.Flags |= SampleFlag.Loop;

					if ((flags & 0x0020) != 0)
						s.Flags |= SampleFlag.ItPacked;

					if ((flags & 0x0010) != 0)
						s.Flags |= SampleFlag.Delta;

					if ((flags & 0x0008) != 0)
						s.Flags |= SampleFlag.BigEndian;
				}
				else if (uniVersion >= 0x102)
				{
					if ((flags & 0x0800) != 0)
						s.Flags |= SampleFlag.UstLoop;

					if ((flags & 0x0400) != 0)
						s.Flags |= SampleFlag.OwnPan;

					if ((flags & 0x0200) != 0)
						s.Flags |= SampleFlag.Sustain;

					if ((flags & 0x0100) != 0)
						s.Flags |= SampleFlag.Reverse;

					if ((flags & 0x0080) != 0)
						s.Flags |= SampleFlag.Bidi;

					if ((flags & 0x0040) != 0)
						s.Flags |= SampleFlag.Loop;

					if ((flags & 0x0020) != 0)
						s.Flags |= SampleFlag.ItPacked;

					if ((flags & 0x0010) != 0)
						s.Flags |= SampleFlag.Delta;

					if ((flags & 0x0008) != 0)
						s.Flags |= SampleFlag.BigEndian;
				}
				else
				{
					if ((flags & 0x0400) != 0)
						s.Flags |= SampleFlag.UstLoop;

					if ((flags & 0x0200) != 0)
						s.Flags |= SampleFlag.OwnPan;

					if ((flags & 0x0100) != 0)
						s.Flags |= SampleFlag.Reverse;

					if ((flags & 0x0080) != 0)
						s.Flags |= SampleFlag.Sustain;

					if ((flags & 0x0040) != 0)
						s.Flags |= SampleFlag.Bidi;

					if ((flags & 0x0020) != 0)
						s.Flags |= SampleFlag.Loop;

					if ((flags & 0x0010) != 0)
						s.Flags |= SampleFlag.BigEndian;

					if ((flags & 0x0008) != 0)
						s.Flags |= SampleFlag.Delta;
				}

				s.Speed = moduleStream.Read_B_UINT32();
				s.Volume = moduleStream.Read_UINT8();
				s.Panning = (short)moduleStream.Read_B_UINT16();
				s.Length = moduleStream.Read_B_UINT32();
				s.LoopStart = moduleStream.Read_B_UINT32();
				s.LoopEnd = moduleStream.Read_B_UINT32();
				s.SusBegin = moduleStream.Read_B_UINT32();
				s.SusEnd = moduleStream.Read_B_UINT32();
				s.GlobVol = moduleStream.Read_UINT8();
				s.VibFlags = (VibratoFlag)moduleStream.Read_UINT8();
				s.VibType = moduleStream.Read_UINT8();
				s.VibSweep = moduleStream.Read_UINT8();
				s.VibDepth = moduleStream.Read_UINT8();
				s.VibRate = moduleStream.Read_UINT8();

				ushort len = moduleStream.Read_L_UINT16();
				s.SampleName = moduleStream.ReadString(encoder, len);

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the instruments in version 6 format
		/// </summary>
		/********************************************************************/
		private bool LoadInstr6(ModuleStream moduleStream)
		{
			Encoding encoder = EncoderCollection.Ibm850;

			for (int t = 0; t < of.NumIns; t++)
			{
				Instrument i = of.Instruments[t];

				i.Flags = (InstrumentFlag)moduleStream.Read_UINT8();
				i.NnaType = (Nna)moduleStream.Read_UINT8();
				i.Dca = (Dca)moduleStream.Read_UINT8();
				i.Dct = (Dct)moduleStream.Read_UINT8();
				i.GlobVol = moduleStream.Read_UINT8();
				i.Panning = (short)moduleStream.Read_B_UINT16();
				i.PitPanSep = moduleStream.Read_UINT8();
				i.PitPanCenter = moduleStream.Read_UINT8();
				i.RVolVar = moduleStream.Read_UINT8();
				i.RPanVar = moduleStream.Read_UINT8();
				i.VolFade = moduleStream.Read_B_UINT16();

				i.VolFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
				i.VolPts = moduleStream.Read_UINT8();
				i.VolSusBeg = moduleStream.Read_UINT8();
				i.VolSusEnd = moduleStream.Read_UINT8();
				i.VolBeg = moduleStream.Read_UINT8();
				i.VolEnd = moduleStream.Read_UINT8();

				for (int w = 0; w < (uniVersion >= 0x100 ? 32 : i.VolPts); w++)
				{
					i.VolEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
					i.VolEnv[w].Val = (short)moduleStream.Read_B_UINT16();
				}

				i.PanFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
				i.PanPts = moduleStream.Read_UINT8();
				i.PanSusBeg = moduleStream.Read_UINT8();
				i.PanSusEnd = moduleStream.Read_UINT8();
				i.PanBeg = moduleStream.Read_UINT8();
				i.PanEnd = moduleStream.Read_UINT8();

				for (int w = 0; w < (uniVersion >= 0x100 ? 32 : i.PanPts); w++)
				{
					i.PanEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
					i.PanEnv[w].Val = (short)moduleStream.Read_B_UINT16();
				}

				i.PitFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
				i.PitPts = moduleStream.Read_UINT8();
				i.PitSusBeg = moduleStream.Read_UINT8();
				i.PitSusEnd = moduleStream.Read_UINT8();
				i.PitBeg = moduleStream.Read_UINT8();
				i.PitEnd = moduleStream.Read_UINT8();

				for (int w = 0; w < (uniVersion >= 0x100 ? 32 : i.PitPts); w++)
				{
					i.PitEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
					i.PitEnv[w].Val = (short)moduleStream.Read_B_UINT16();
				}

				if (uniVersion >= 0x103)
					moduleStream.ReadArray_B_UINT16s(i.SampleNumber, 0, 120);
				else
				{
					for (int w = 0; w < 120; w++)
						i.SampleNumber[w] = moduleStream.Read_UINT8();
				}

				moduleStream.Read(i.SampleNote, 0, 120);

				ushort len = moduleStream.Read_L_UINT16();
				i.InsName = moduleStream.ReadString(encoder, len);

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the instruments in version 5 format
		/// </summary>
		/********************************************************************/
		private bool LoadInstr5(ModuleStream moduleStream)
		{
			Encoding encoder = EncoderCollection.Ibm850;

			ushort wavCnt = 0;
			int sIndex = 0;

			of.NumSmp = 0;
			for (int t = 0; t < of.NumIns; t++)
			{
				Instrument i = of.Instruments[t];

				int numSmp = moduleStream.Read_UINT8();

				Array.Fill<ushort>(i.SampleNumber, 0xffff);
				for (int u = 0; u < 96; u++)
					i.SampleNumber[u] = (ushort)(of.NumSmp + moduleStream.Read_UINT8());

				i.VolFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
				i.VolPts = moduleStream.Read_UINT8();
				i.VolSusBeg = moduleStream.Read_UINT8();
				i.VolSusEnd = i.VolSusBeg;
				i.VolBeg = moduleStream.Read_UINT8();
				i.VolEnd = moduleStream.Read_UINT8();

				for (int u = 0; u < 12; u++)
				{
					i.VolEnv[u].Pos = (short)moduleStream.Read_L_UINT16();
					i.VolEnv[u].Val = (short)moduleStream.Read_L_UINT16();
				}

				i.PanFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
				i.PanPts = moduleStream.Read_UINT8();
				i.PanSusBeg = moduleStream.Read_UINT8();
				i.PanSusEnd = i.VolSusBeg;
				i.PanBeg = moduleStream.Read_UINT8();
				i.PanEnd = moduleStream.Read_UINT8();

				for (int u = 0; u < 12; u++)
				{
					i.PanEnv[u].Pos = (short)moduleStream.Read_L_UINT16();
					i.PanEnv[u].Val = (short)moduleStream.Read_L_UINT16();
				}

				byte vibType = moduleStream.Read_UINT8();
				byte vibSweep = moduleStream.Read_UINT8();
				byte vibDepth = moduleStream.Read_UINT8();
				byte vibRate = moduleStream.Read_UINT8();

				i.VolFade = moduleStream.Read_L_UINT16();

				ushort len = moduleStream.Read_L_UINT16();
				i.InsName = moduleStream.ReadString(encoder, len);

				for (int u = 0; u < numSmp; u++, sIndex++, of.NumSmp++)
				{
					// Allocate more room for sample information if necessary
					if (of.NumSmp + u == wavCnt)
					{
						UniSmp05[] newWh = new UniSmp05[wavCnt + UniSmpIncr];

						if (wh != null)
							Array.Copy(wh, newWh, wavCnt);

						wh = newWh;

						sIndex = wavCnt;
						wavCnt += UniSmpIncr;
					}

					ref UniSmp05 s = ref wh[sIndex];

					s.C2Spd = moduleStream.Read_L_UINT16();
					s.Transpose = (ushort)moduleStream.Read_INT8();
					s.Volume = moduleStream.Read_UINT8();
					s.Panning = moduleStream.Read_UINT8();
					s.Length = moduleStream.Read_L_UINT32();
					s.LoopStart = moduleStream.Read_L_UINT32();
					s.LoopEnd = moduleStream.Read_L_UINT32();
					s.Flags = moduleStream.Read_L_UINT16();

					len = moduleStream.Read_L_UINT16();
					s.SampleName = moduleStream.ReadString(encoder, len);

					s.VibType = vibType;
					s.VibSweep = vibSweep;
					s.VibDepth = vibDepth;
					s.VibRate = vibRate;

					if (moduleStream.EndOfStream)
						return false;
				}
			}

			// Sanity check
			if (of.NumSmp == 0)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read all the samples in version 5 format
		/// </summary>
		/********************************************************************/
		private void LoadSmp5()
		{
			int qIndex = 0;
			int sIndex = 0;

			for (int u = 0; u < of.NumSmp; u++, qIndex++, sIndex++)
			{
				Sample q = of.Samples[qIndex];
				ref UniSmp05 s = ref wh[sIndex];

				q.SampleName = s.SampleName;

				q.Length = s.Length;
				q.LoopStart = s.LoopStart;
				q.LoopEnd = s.LoopEnd;
				q.Volume = s.Volume;
				q.Speed = s.C2Spd;
				q.Panning = s.Panning;
				q.VibType = s.VibType;
				q.VibSweep = s.VibSweep;
				q.VibDepth = s.VibDepth;
				q.VibRate = s.VibRate;

				// Convert flags
				q.Flags = SampleFlag.None;

				if (uniVersion >= 5)
				{
					if ((s.Flags & 128) != 0)
						q.Flags |= SampleFlag.Reverse;

					if ((s.Flags & 64) != 0)
						q.Flags |= SampleFlag.OwnPan;

					if ((s.Flags & 32) != 0)
						q.Flags |= SampleFlag.Bidi;

					if ((s.Flags & 16) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 8) != 0)
						q.Flags |= SampleFlag.BigEndian;

					if ((s.Flags & 4) != 0)
						q.Flags |= SampleFlag.Delta;

					if ((s.Flags & 2) != 0)
						q.Flags |= SampleFlag.Signed;

					if ((s.Flags & 1) != 0)
						q.Flags |= SampleFlag._16Bits;
				}
				else
				{
					if ((s.Flags & 64) != 0)
						q.Flags |= SampleFlag.OwnPan;

					if ((s.Flags & 32) != 0)
						q.Flags |= SampleFlag.Signed;

					if ((s.Flags & 16) != 0)
						q.Flags |= SampleFlag.Bidi;

					if ((s.Flags & 8) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 4) != 0)
						q.Flags |= SampleFlag.Delta;
				}
			}

			int dIndex = 0;

			for (int u = 0; u < of.NumIns; u++, dIndex++)
			{
				for (int t = 0; t < SharedConstant.InstNotes; t++)
				{
					Instrument d = of.Instruments[dIndex];

					d.SampleNote[t] = (byte)((d.SampleNumber[t] >= of.NumSmp) ? 255 : (t + wh[d.SampleNumber[t]].Transpose));
				}
			}
		}
		#endregion
	}
}

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
	/// MikMod loader for IT (Impulse Tracker) format
	/// </summary>
	internal class MikModConverterWorker_It : MikModConverterWorkerBase
	{
		#region ItHeader class
		private class ItHeader
		{
			public readonly byte[] SongName = new byte[27];
			public readonly byte[] Blank01 = new byte[2];
			public ushort OrdNum;
			public ushort InsNum;
			public ushort SmpNum;
			public ushort PatNum;
			public ushort Cwt;											// Created with tracker (y.xx = 0x0yxx)
			public ushort Cmwt;											// Compatible with tracker ver > than val
			public ushort Flags;
			public ushort Special;										// Bit 0 set = song message attached
			public byte GlobVol;
			public byte MixVol;											// Mixing volume [ignored]
			public byte InitSpeed;
			public byte InitTempo;
			public byte PanSep;											// Panning separation between channels
			public byte ZeroByte;
			public ushort MsgLength;
			public uint MsgOffset;
			public readonly byte[] Blank02 = new byte[4];
			public readonly byte[] PanTable = new byte[64];
			public readonly byte[] VolTable = new byte[64];
		}
		#endregion

		#region ItSample class
		private class ItSample
		{
			public readonly byte[] FileName = new byte[13];
			public byte ZeroByte;
			public byte GlobVol;
			public byte Flag;
			public byte Volume;
			public byte Panning;
			public readonly byte[] SampName = new byte[29];
			public ushort Convert;										// Sample conversion flag
			public uint Length;
			public uint LoopBeg;
			public uint LoopEnd;
			public uint C5Spd;
			public uint SusBegin;
			public uint SusEnd;
			public uint SampOffset;
			public byte VibSpeed;
			public byte VibDepth;
			public byte VibRate;
			public byte VibWave;										// 0 = Sine; 1 = Rampdown; 2 = Square; 3 = Random (speed ignored)
		}
		#endregion

		#region ItInstHeader class
		private class ItInstHeader
		{
//			public uint Size;											// Instrument size
			public readonly byte[] FileName = new byte[13];				// Instrument file name
			public byte ZeroByte;										// Instrument type (always 0)
			public byte VolFlg;
			public byte VolPts;
			public byte VolBeg;											// Volume loop start (node)
			public byte VolEnd;											// Volume loop end (node)
			public byte VolSusBeg;										// Volume sustain begin (node)
			public byte VolSusEnd;										// Volume sustain end (node)
			public byte PanFlg;
			public byte PanPts;
			public byte PanBeg;											// Channel loop start (node)
			public byte PanEnd;											// Channel loop end (node)
			public byte PanSusBeg;										// Channel sustain begin (node)
			public byte PanSusEnd;										// Channel sustain end (node)
			public byte PitFlg;
			public byte PitPts;
			public byte PitBeg;											// Pitch loop start (node)
			public byte PitEnd;											// Pitch loop end (node)
			public byte PitSusBeg;										// Pitch sustain begin (node)
			public byte PitSusEnd;										// Pitch sustain end (node)
//			public ushort Blank;
			public byte GlobVol;
			public byte ChanPan;
			public ushort FadeOut;										// Envelope end / NNA volume fadeout
			public byte Dnc;											// Duplicate note check
			public byte Dca;											// Duplicate check action
			public byte Dct;											// Duplicate check type
			public byte Nna;											// New Note Action [0, 1, 2, 3]
			public ushort TrkVers;										// Tracker version used to save [files only]
			public byte PPSep;											// Pitch-Pan Separation
			public byte PPCenter;										// Pitch-Pan Center
			public byte RVolVar;										// Random volume variations
			public byte RPanVar;										// Random panning variations
			public ushort NumSmp;										// Number of samples in instrument [files only]
			public readonly byte[] Name = new byte[27];					// Instrument name
			public readonly byte[] Blank01 = new byte[6];
			public readonly ushort[] SampTable = new ushort[ItNoteCnt];	// Sample for each note [note / samp pairs]
			public readonly byte[] VolEnv = new byte[200];				// Volume envelopes (IT 1.x stuff)
			public readonly byte[] OldVolTick = new byte[ItEnvCnt];		// Volume tick position (IT 1.x stuff)
			public readonly byte[] VolNode = new byte[ItEnvCnt];		// Amplitude of volume nodes
			public readonly ushort[] VolTick = new ushort[ItEnvCnt];	// Tick value of volume nodes
			public readonly byte[] PanNode = new byte[ItEnvCnt];		// panEnv - node points
			public readonly ushort[] PanTick = new ushort[ItEnvCnt];	// Tick value of panning nodes
			public readonly byte[] PitNode = new byte[ItEnvCnt];		// pitchEnv - node points
			public readonly ushort[] PitTick = new ushort[ItEnvCnt];	// Tick value of pitch nodes
		}
		#endregion

		#region ItNote class
		private class ItNote
		{
			public byte Note;
			public byte Ins;
			public byte VolPan;
			public byte Cmd;
			public byte Inf;
		}
		#endregion

		private const int ItEnvCnt = 25;
		private const int ItNoteCnt = 120;

		private static readonly string[] versions =
		{
			Resources.IDS_MIKCONV_NAME_IT,
			Resources.IDS_MIKCONV_NAME_IT_COMPRESSED,
			Resources.IDS_MIKCONV_NAME_IT_214P3,
			Resources.IDS_MIKCONV_NAME_IT_214P3_COMPRESSED,
			Resources.IDS_MIKCONV_NAME_IT_214P4,
			Resources.IDS_MIKCONV_NAME_IT_214P4_COMPRESSED
		};

		// table for porta-to-note command within volume/panning column
		private static readonly byte[] portaTable = { 0, 1, 4, 8, 16, 32, 64, 96, 128, 255 };

		private MlUtil util;

		private ItHeader mh;
		private ItNote[] itPat;
		private byte[] mask;
		private ItNote[] last;
		private uint[] paraPtr;

		private int numTrk;
		private ProcessFlags oldEffect;

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
			if (fileSize < 188)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] buf = new byte[4];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 4);

			if ((buf[0] == 'I') && (buf[1] == 'M') && (buf[2] == 'P') && (buf[3] == 'M'))
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

			mh = new ItHeader();
			util.posLookup = new byte[256];
			itPat = Helpers.InitializeArray<ItNote>(200 * 64);
			mask = new byte[64];
			last = Helpers.InitializeArray<ItNote>(64);

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				numTrk = 0;
				util.filters = false;

				// Try to read module header
				moduleStream.Seek(4, SeekOrigin.Begin);		// Kill 4 byte header
				moduleStream.ReadString(mh.SongName, 26);
				moduleStream.Read(mh.Blank01, 0, 2);

				mh.OrdNum = moduleStream.Read_L_UINT16();
				mh.InsNum = moduleStream.Read_L_UINT16();
				mh.SmpNum = moduleStream.Read_L_UINT16();
				mh.PatNum = moduleStream.Read_L_UINT16();
				mh.Cwt = moduleStream.Read_L_UINT16();
				mh.Cmwt = moduleStream.Read_L_UINT16();
				mh.Flags = moduleStream.Read_L_UINT16();
				mh.Special = moduleStream.Read_L_UINT16();
				mh.GlobVol = moduleStream.Read_UINT8();
				mh.MixVol = moduleStream.Read_UINT8();
				mh.InitSpeed = moduleStream.Read_UINT8();
				mh.InitTempo = moduleStream.Read_UINT8();
				mh.PanSep = moduleStream.Read_UINT8();
				mh.ZeroByte = moduleStream.Read_UINT8();
				mh.MsgLength = moduleStream.Read_L_UINT16();
				mh.MsgOffset = moduleStream.Read_L_UINT32();

				moduleStream.Read(mh.Blank02, 0, 4);
				moduleStream.Read(mh.PanTable, 0, 64);
				moduleStream.Read(mh.VolTable, 0, 64);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				if ((mh.OrdNum > 256) || (mh.InsNum > 255) || (mh.SmpNum > 255) || (mh.PatNum > 255))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				// Set module variables
				of.SongName = encoder.GetString(mh.SongName);
				of.RepPos = 0;
				of.NumPat = mh.PatNum;
				of.NumIns = mh.InsNum;
				of.NumSmp = mh.SmpNum;
				of.InitSpeed = mh.InitSpeed;
				of.InitTempo = mh.InitTempo;
				of.InitVolume = mh.GlobVol;
				of.Flags |= ModuleFlag.BgSlides | ModuleFlag.ArpMem;

				if ((mh.Flags & 1) == 0)
					of.Flags |= ModuleFlag.Panning;

				of.BpmLimit = 32;

				if (mh.SongName[25] != 0)
					of.NumVoices = (byte)(1 + mh.SongName[25]);

				// Set the module type
				// 2.17 : IT 2.14p4
				// 2.16 : IT 2.14p3 with resonant filters
				// 2.15 : IT 2.14p3 (improved compression)
				if ((mh.Cwt <= 0x219) && (mh.Cwt >= 0x217))
					originalFormat = versions[mh.Cmwt < 0x214 ? 4 : 5];
				else if (mh.Cwt >= 0x215)
					originalFormat = versions[mh.Cmwt < 0x214 ? 2 : 3];
				else
					originalFormat = string.Format(versions[mh.Cmwt < 0x214 ? 0 : 1], mh.Cwt >> 8, ((mh.Cwt >> 4) & 0xf) * 10 + (mh.Cwt & 0xf));

				if ((mh.Flags & 8) != 0)
					of.Flags |= ModuleFlag.XmPeriods | ModuleFlag.Linear;

				if ((mh.Cwt >= 0x106) && ((mh.Flags & 16) != 0))
					oldEffect = ProcessFlags.OldStyle;
				else
					oldEffect = ProcessFlags.None;

				// Set panning positions
				if ((mh.Flags & 1) != 0)
				{
					for (int t = 0; t < 64; t++)
					{
						mh.PanTable[t] &= 0x7f;

						if (mh.PanTable[t] < 64)
							of.Panning[t] = (ushort)(mh.PanTable[t] << 2);
						else if (mh.PanTable[t] == 64)
							of.Panning[t] = 255;
						else if (mh.PanTable[t] == 100)
							of.Panning[t] = SharedConstant.Pan_Surround;
						else if (mh.PanTable[t] == 127)
							of.Panning[t] = SharedConstant.Pan_Center;
						else
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
							return false;
						}
					}
				}
				else
				{
					for (int t = 0; t < 64; t++)
						of.Panning[t] = SharedConstant.Pan_Center;
				}

				// Set channel volumes
				Array.Copy(mh.VolTable, of.ChanVol, 64);

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

					if ((util.origPositions[t] > mh.PatNum) && (util.origPositions[t] < 254))
						util.origPositions[t] = 255;
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				util.posLookupCnt = mh.OrdNum;
				util.CreateOrders(of, curious);

				paraPtr = new uint[mh.InsNum + mh.SmpNum + of.NumPat];

				// Read the instrument, sample and pattern parapointers
				moduleStream.ReadArray_L_UINT32s(paraPtr, 0, mh.InsNum + mh.SmpNum + of.NumPat);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Check for and load midi information for resonant filters
				if (mh.Cmwt >= 0x216)
				{
					if ((mh.Special & 8) != 0)
					{
						LoadMidiConfiguration(moduleStream);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
							return false;
						}
					}
					else
						LoadMidiConfiguration(null);

					util.filters = true;
				}

				// Check for and load song comment
				if (((mh.Special & 1) != 0) && (mh.Cwt >= 0x104) && (mh.MsgLength != 0))
				{
					moduleStream.Seek(mh.MsgOffset, SeekOrigin.Begin);
					of.Comment = moduleStream.ReadComment(mh.MsgLength, encoder);
				}

				if ((mh.Flags & 4) == 0)
					of.NumIns = of.NumSmp;

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				if (!util.AllocLinear(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				// Load all the samples
				for (int t = 0; t < mh.SmpNum; t++)
				{
					ItSample s = new ItSample();
					Sample q = of.Samples[t];

					// Seek to sample position
					moduleStream.Seek(paraPtr[mh.InsNum + t] + 4, SeekOrigin.Begin);

					// And load sample info
					moduleStream.ReadString(s.FileName, 12);

					s.ZeroByte = moduleStream.Read_UINT8();
					s.GlobVol = moduleStream.Read_UINT8();
					s.Flag = moduleStream.Read_UINT8();
					s.Volume = moduleStream.Read_UINT8();

					moduleStream.ReadString(s.SampName, 26);

					s.Convert = moduleStream.Read_UINT8();
					s.Panning = moduleStream.Read_UINT8();
					s.Length = moduleStream.Read_L_UINT32();
					s.LoopBeg = moduleStream.Read_L_UINT32();
					s.LoopEnd = moduleStream.Read_L_UINT32();
					s.C5Spd = moduleStream.Read_L_UINT32();
					s.SusBegin = moduleStream.Read_L_UINT32();
					s.SusEnd = moduleStream.Read_L_UINT32();
					s.SampOffset = moduleStream.Read_L_UINT32();
					s.VibSpeed = moduleStream.Read_UINT8();
					s.VibDepth = moduleStream.Read_UINT8();
					s.VibRate = moduleStream.Read_UINT8();
					s.VibWave = moduleStream.Read_UINT8();

					// Generate an error if c5Spd is > 512k, or sample length > 256 megs
					// (Nothing would EVER be that high)
					if (moduleStream.EndOfStream || (s.C5Spd > 0x7ffff) || (s.Length > 0xfffffff))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					// Reality check for sample loop information
					if (((s.Flag & 16) != 0) && ((s.LoopBeg > 0xfffffff) || (s.LoopEnd > 0xfffffff)))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					q.SampleName = encoder.GetString(s.SampName);
					q.Speed = s.C5Spd / 2;
					q.Panning = (short)(((s.Panning & 127) == 64) ? 255 : (s.Panning & 127) << 2);
					q.Length = s.Length;
					q.LoopStart = s.LoopBeg;
					q.LoopEnd = s.LoopEnd;
					q.Volume = s.GlobVol;
					q.SeekPos = s.SampOffset;

					// Convert speed to XM linear fine tune
					if ((of.Flags & ModuleFlag.Linear) != 0)
						q.Speed = (uint)util.SpeedToFineTune(of, s.C5Spd, t);

					if ((s.Panning & 128) != 0)
						q.Flags |= SampleFlag.OwnPan;

					if (s.VibRate != 0)
					{
						q.VibFlags |= VibratoFlag.It;
						q.VibType = s.VibWave;
						q.VibSweep = (byte)(s.VibRate * 2);
						q.VibDepth = s.VibDepth;
						q.VibRate = s.VibSpeed;
					}

					if ((s.Flag & 2) != 0)
						q.Flags |= SampleFlag._16Bits;

					if (((s.Flag & 8) != 0) && (mh.Cwt >= 0x214))
						q.Flags |= SampleFlag.ItPacked;

					if ((s.Flag & 16) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flag & 64) != 0)
						q.Flags |= SampleFlag.Bidi;

					if (mh.Cwt >= 0x200)
					{
						if ((s.Convert & 1) != 0)
							q.Flags |= SampleFlag.Signed;

						if ((s.Convert & 4) != 0)
							q.Flags |= SampleFlag.Delta;
					}
				}

				// Load instrument if instrument mode flag enabled
				if ((mh.Flags & 4) != 0)
				{
					if (!MLoader.AllocInstruments(of))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
						return false;
					}

					of.Flags |= (ModuleFlag.Nna | ModuleFlag.Inst);

					for (int t = 0; t < mh.InsNum; t++)
					{
						ItInstHeader ih = new ItInstHeader();
						Instrument d = of.Instruments[t];

						// Seek to instrument position
						moduleStream.Seek(paraPtr[t] + 4, SeekOrigin.Begin);

						// Load instrument info
						moduleStream.ReadString(ih.FileName, 12);
						ih.ZeroByte = moduleStream.Read_UINT8();

						if (mh.Cwt < 0x200)
						{
							// Load IT 1.xx inst header
							ih.VolFlg = moduleStream.Read_UINT8();
							ih.VolBeg = moduleStream.Read_UINT8();
							ih.VolEnd = moduleStream.Read_UINT8();
							ih.VolSusBeg = moduleStream.Read_UINT8();
							ih.VolSusEnd = moduleStream.Read_UINT8();

							moduleStream.Seek(2, SeekOrigin.Current);

							ih.FadeOut = moduleStream.Read_L_UINT16();
							ih.Nna = moduleStream.Read_UINT8();
							ih.Dnc = moduleStream.Read_UINT8();
						}
						else
						{
							// Read IT200+ header
							ih.Nna = moduleStream.Read_UINT8();
							ih.Dct = moduleStream.Read_UINT8();
							ih.Dca = moduleStream.Read_UINT8();
							ih.FadeOut = moduleStream.Read_L_UINT16();
							ih.PPSep = moduleStream.Read_UINT8();
							ih.PPCenter = moduleStream.Read_UINT8();
							ih.GlobVol = moduleStream.Read_UINT8();
							ih.ChanPan = moduleStream.Read_UINT8();
							ih.RVolVar = moduleStream.Read_UINT8();
							ih.RPanVar = moduleStream.Read_UINT8();
						}

						ih.TrkVers = moduleStream.Read_L_UINT16();
						ih.NumSmp = moduleStream.Read_UINT8();

						moduleStream.Seek(1, SeekOrigin.Current);

						moduleStream.ReadString(ih.Name, 26);
						moduleStream.Read(ih.Blank01, 0, 6);
						moduleStream.ReadArray_L_UINT16s(ih.SampTable, 0, ItNoteCnt);

						if (mh.Cwt < 0x200)
						{
							// Load IT 1xx volume envelope
							moduleStream.Read(ih.VolEnv, 0, 200);

							for (int lp = 0; lp < ItEnvCnt; lp++)
							{
								ih.OldVolTick[lp] = moduleStream.Read_UINT8();
								ih.VolNode[lp] = moduleStream.Read_UINT8();
							}
						}
						else
						{
							// Load IT 2xx volume, pan and pitch envelopes
							ih.VolFlg = moduleStream.Read_UINT8();
							ih.VolPts = moduleStream.Read_UINT8();

							if (ih.VolPts > ItEnvCnt)
								ih.VolPts = ItEnvCnt;

							ih.VolBeg = moduleStream.Read_UINT8();
							ih.VolEnd = moduleStream.Read_UINT8();
							ih.VolSusBeg = moduleStream.Read_UINT8();
							ih.VolSusEnd = moduleStream.Read_UINT8();

							for (int lp = 0; lp < ItEnvCnt; lp++)
							{
								ih.VolNode[lp] = moduleStream.Read_UINT8();
								ih.VolTick[lp] = moduleStream.Read_L_UINT16();
							}

							moduleStream.Seek(1, SeekOrigin.Current);

							ih.PanFlg = moduleStream.Read_UINT8();
							ih.PanPts = moduleStream.Read_UINT8();

							if (ih.PanPts > ItEnvCnt)
								ih.PanPts = ItEnvCnt;

							ih.PanBeg = moduleStream.Read_UINT8();
							ih.PanEnd = moduleStream.Read_UINT8();
							ih.PanSusBeg = moduleStream.Read_UINT8();
							ih.PanSusEnd = moduleStream.Read_UINT8();

							for (int lp = 0; lp < ItEnvCnt; lp++)
							{
								ih.PanNode[lp] = moduleStream.Read_UINT8();
								ih.PanTick[lp] = moduleStream.Read_L_UINT16();
							}

							moduleStream.Seek(1, SeekOrigin.Current);

							ih.PitFlg = moduleStream.Read_UINT8();
							ih.PitPts = moduleStream.Read_UINT8();

							if (ih.PitPts > ItEnvCnt)
								ih.PitPts = ItEnvCnt;

							ih.PitBeg = moduleStream.Read_UINT8();
							ih.PitEnd = moduleStream.Read_UINT8();
							ih.PitSusBeg = moduleStream.Read_UINT8();
							ih.PitSusEnd = moduleStream.Read_UINT8();

							for (int lp = 0; lp < ItEnvCnt; lp++)
							{
								ih.PitNode[lp] = moduleStream.Read_UINT8();
								ih.PitTick[lp] = moduleStream.Read_L_UINT16();
							}

							moduleStream.Seek(1, SeekOrigin.Current);
						}

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
							return false;
						}

						d.VolFlg |= EnvelopeFlag.VolEnv;
						d.InsName = encoder.GetString(ih.Name);
						d.NnaType = (Nna)ih.Nna & Nna.Mask;

						if (mh.Cwt < 0x200)
						{
							d.VolFade = (ushort)(ih.FadeOut << 6);

							if (ih.Dnc != 0)
							{
								d.Dct = Dct.Note;
								d.Dca = Dca.Cut;
							}

							if ((ih.VolFlg & 1) != 0)
								d.VolFlg |= EnvelopeFlag.On;

							if ((ih.VolFlg & 2) != 0)
								d.VolFlg |= EnvelopeFlag.Loop;

							if ((ih.VolFlg & 4) != 0)
								d.VolFlg |= EnvelopeFlag.Sustain;

							// XM conversion of IT envelope array
							d.VolBeg = ih.VolBeg;
							d.VolEnd = ih.VolEnd;
							d.VolSusBeg = ih.VolSusBeg;
							d.VolSusEnd = ih.VolSusEnd;

							if ((ih.VolFlg & 1) != 0)
							{
								for (int u = 0; u < ItEnvCnt; u++)
								{
									if (ih.OldVolTick[d.VolPts] != 0xff)
									{
										d.VolEnv[d.VolPts].Val = (short)(ih.VolNode[d.VolPts] << 2);
										d.VolEnv[d.VolPts].Pos = ih.OldVolTick[d.VolPts];
										d.VolPts++;
									}
									else
										break;
								}
							}
						}
						else
						{
							d.Panning = (short)(((ih.ChanPan & 127) == 64) ? 255 : (ih.ChanPan & 127) << 2);

							if ((ih.ChanPan & 128) == 0)
								d.Flags |= InstrumentFlag.OwnPan;

							if ((ih.PPSep & 128) == 0)
							{
								d.PitPanSep = (byte)(ih.PPSep << 2);
								d.PitPanCenter = ih.PPCenter;
								d.Flags |= InstrumentFlag.PitchPan;
							}

							d.GlobVol = (byte)(ih.GlobVol >> 1);
							d.VolFade = (ushort)(ih.FadeOut << 5);
							d.Dct = (Dct)ih.Dct;
							d.Dca = (Dca)ih.Dca;

							if (mh.Cwt >= 0x204)
							{
								d.RVolVar = ih.RVolVar;
								d.RPanVar = ih.RPanVar;
							}

							if ((ih.VolFlg & 1) != 0)
								d.VolFlg |= EnvelopeFlag.On;

							if ((ih.VolFlg & 2) != 0)
								d.VolFlg |= EnvelopeFlag.Loop;

							if ((ih.VolFlg & 4) != 0)
								d.VolFlg |= EnvelopeFlag.Sustain;

							d.VolPts = ih.VolPts;
							d.VolBeg = ih.VolBeg;
							d.VolEnd = ih.VolEnd;
							d.VolSusBeg = ih.VolSusBeg;
							d.VolSusEnd = ih.VolSusEnd;

							for (int u = 0; u < ih.VolPts; u++)
								d.VolEnv[u].Pos = (short)ih.VolTick[u];

							if (((d.VolFlg & EnvelopeFlag.On) != 0) && (d.VolPts < 2))
								d.VolFlg &= ~EnvelopeFlag.On;

							for (int u = 0; u < ih.VolPts; u++)
								d.VolEnv[u].Val = (short)(ih.VolNode[u] << 2);

							if ((ih.PanFlg & 1) != 0)
								d.PanFlg |= EnvelopeFlag.On;

							if ((ih.PanFlg & 2) != 0)
								d.PanFlg |= EnvelopeFlag.Loop;

							if ((ih.PanFlg & 4) != 0)
								d.PanFlg |= EnvelopeFlag.Sustain;

							d.PanPts = ih.PanPts;
							d.PanBeg = ih.PanBeg;
							d.PanEnd = ih.PanEnd;
							d.PanSusBeg = ih.PanSusBeg;
							d.PanSusEnd = ih.PanSusEnd;

							for (int u = 0; u < ih.PanPts; u++)
								d.PanEnv[u].Pos = (short)ih.PanTick[u];

							if (((d.PanFlg & EnvelopeFlag.On) != 0) && (d.PanPts < 2))
								d.PanFlg &= ~EnvelopeFlag.On;

							for (int u = 0; u < ih.PanPts; u++)
								d.PanEnv[u].Val = (short)(ih.PanNode[u] == 32 ? 255 : (ih.PanNode[u] + 32) << 2);

							if ((ih.PitFlg & 1) != 0)
								d.PitFlg |= EnvelopeFlag.On;

							if ((ih.PitFlg & 2) != 0)
								d.PitFlg |= EnvelopeFlag.Loop;

							if ((ih.PitFlg & 4) != 0)
								d.PitFlg |= EnvelopeFlag.Sustain;

							d.PitPts = ih.PitPts;
							d.PitBeg = ih.PitBeg;
							d.PitEnd = ih.PitEnd;
							d.PitSusBeg = ih.PitSusBeg;
							d.PitSusEnd = ih.PitSusEnd;

							for (int u = 0; u < ih.PitPts; u++)
								d.PitEnv[u].Pos = (short)ih.PitTick[u];

							if (((d.PitFlg & EnvelopeFlag.On) != 0) && (d.PitPts < 2))
								d.PitFlg &= ~EnvelopeFlag.On;

							for (int u = 0; u < ih.PitPts; u++)
								d.PitEnv[u].Val = (byte)(ih.PitNode[u] + 32);		// Need to case to byte to force a wrap-around for values bigger than 255

							if ((ih.PitFlg & 0x80) != 0)
							{
								// Filter envelopes not supported yet
								d.PitFlg &= ~EnvelopeFlag.On;
								ih.PitPts = ih.PitBeg = ih.PitEnd = 0;
							}
						}

						for (int u = 0; u < ItNoteCnt; u++)
						{
							d.SampleNote[u] = (byte)(ih.SampTable[u] & 255);
							d.SampleNumber[u] = (ushort)((ih.SampTable[u] >> 8) != 0 ? ((ih.SampTable[u] >> 8) - 1) : 0xffff);

							if (d.SampleNumber[u] >= of.NumSmp)
								d.SampleNumber[u] = 255;
							else
							{
								if ((of.Flags & ModuleFlag.Linear) != 0)
								{
									int note = d.SampleNote[u] + util.noteIndex[d.SampleNumber[u]];
									d.SampleNote[u] = (byte)((note < 0) ? 0 : (note > 255 ? 255 : note));
								}
							}
						}
					}
				}
				else
				{
					if ((of.Flags & ModuleFlag.Linear) != 0)
					{
						if (!MLoader.AllocInstruments(of))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
							return false;
						}

						of.Flags |= ModuleFlag.Inst;

						for (int t = 0; t < mh.SmpNum; t++)
						{
							Instrument d = of.Instruments[t];

							for (int u = 0; u < ItNoteCnt; u++)
							{
								if (d.SampleNumber[u] >= of.NumSmp)
									d.SampleNumber[u] = 255;
								else
								{
									int note = d.SampleNote[u] + util.noteIndex[d.SampleNumber[u]];
									d.SampleNote[u] = (byte)((note < 0) ? 0 : (note > 255 ? 255 : note));
								}
							}
						}
					}
				}

				// Figure out how many channels this song actually uses
				of.NumChn = 0;
				Array.Fill<byte>(util.remap, 255, 0, SharedConstant.UF_MaxChan);

				for (int t = 0; t < of.NumPat; t++)
				{
					// Seek to pattern position
					if (paraPtr[mh.InsNum + mh.SmpNum + t] != 0)		// 0 -> empty 64 row pattern
					{
						moduleStream.Seek(paraPtr[mh.InsNum + mh.SmpNum + t], SeekOrigin.Begin);
						moduleStream.Seek(2, SeekOrigin.Current);

						// Read pattern length (# of rows)
						// Impulse Tracker never creates patterns with less than 32 rows,
						// but some other trackers do, so we only check for more that 256
						// rows
						ushort packLen = moduleStream.Read_L_UINT16();

						if (packLen > 256)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
							return false;
						}

						moduleStream.Seek(4, SeekOrigin.Current);
						if (!GetNumChannels(moduleStream, packLen))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
							return false;
						}
					}
				}

				// Give each of them a different number
				for (int t = 0; t < SharedConstant.UF_MaxChan; t++)
				{
					if (util.remap[t] == 0)
						util.remap[t] = of.NumChn++;
				}

				of.NumTrk = (ushort)(of.NumPat * of.NumChn);
				if (of.NumVoices != 0)
				{
					if (of.NumVoices < of.NumChn)
						of.NumVoices = of.NumChn;
				}

				if (!MLoader.AllocPatterns(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}

				if (!MLoader.AllocTracks(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}

				for (int t = 0; t < of.NumPat; t++)
				{
					// Seek to pattern position
					if (paraPtr[mh.InsNum + mh.SmpNum + t] == 0)		// 0 -> empty 64 row pattern
					{
						of.PattRows[t] = 64;

						for (int u = 0; u < of.NumChn; u++)
						{
							uniTrk.UniReset();

							for (int k = 0; k < 64; k++)
								uniTrk.UniNewLine();

							of.Tracks[numTrk++] = uniTrk.UniDup();
						}
					}
					else
					{
						moduleStream.Seek(paraPtr[mh.InsNum + mh.SmpNum + t], SeekOrigin.Begin);

						moduleStream.Seek(2, SeekOrigin.Current);
						of.PattRows[t] = moduleStream.Read_L_UINT16();
						moduleStream.Seek(4, SeekOrigin.Current);

						if (!ReadPattern(moduleStream, of.PattRows[t], uniTrk))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
							return false;
						}
					}
				}
			}
			finally
			{
				mh = null;
				util.posLookup = null;
				itPat = null;
				mask = null;
				last = null;
				paraPtr = null;
				util.origPositions = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the number of channels used
		///
		/// Because so many IT files have 64 channels as the set number used,
		/// but really only use far less (usually from 8 to 24 still), I had
		/// to make this function, which determines the number of channels
		/// that are actually USED by a pattern.
		///
		/// NOTE: You must first seek to the file location of the pattern
		/// before calling this procedure
		/// </summary>
		/********************************************************************/
		private bool GetNumChannels(ModuleStream moduleStream, ushort patRows)
		{
			int row = 0;

			do
			{
				if (moduleStream.EndOfStream)
					return false;

				int flag = moduleStream.Read_UINT8();

				if (flag == 0)
					row++;
				else
				{
					int ch = (flag - 1) & 63;
					util.remap[ch] = 0;

					if ((flag & 128) != 0)
						mask[ch] = moduleStream.Read_UINT8();

					if ((mask[ch] & 1) != 0)
						moduleStream.Seek(1, SeekOrigin.Current);

					if ((mask[ch] & 2) != 0)
						moduleStream.Seek(1, SeekOrigin.Current);

					if ((mask[ch] & 4) != 0)
						moduleStream.Seek(1, SeekOrigin.Current);

					if ((mask[ch] & 8) != 0)
						moduleStream.Seek(2, SeekOrigin.Current);
				}
			}
			while (row < patRows);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert one IT track to UniMod track
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(ItNote[] tr, int offset, ushort numRows, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < numRows; t++)
			{
				byte note = tr[offset + t * of.NumChn].Note;
				byte ins = tr[offset + t * of.NumChn].Ins;
				byte volPan = tr[offset + t * of.NumChn].VolPan;

				if (note != 255)
				{
					if (note == 253)
						uniTrk.UniWriteByte((byte)Command.UniKeyOff);
					else if (note == 254)
					{
						uniTrk.UniPtEffect(0xc, 255, of.Flags);		// Note cut command
						volPan = 255;
					}
					else
						uniTrk.UniNote(note);
				}

				// Impulse Tracker only allows up to 99 instruments and crashes when it
				// encounters instrument >= 100. But the file format supports them just
				// fine and there are many MPT created ITs with that many instruments
				if ((ins != 0) && (ins < 253))
					uniTrk.UniInstrument((ushort)(ins - 1));
				else if (ins == 253)
					uniTrk.UniWriteByte((byte)(Command.UniKeyOff));
				else if (ins != 255)	// Crap
					return null;

				// Process volume / panning column
				// Volume / panning effects do NOT all share the same memory address yet
				if (volPan <= 64)
					uniTrk.UniVolEffect(VolEffect.Volume, volPan);
				else if (volPan == 65)				// Fine volume slide up (65-74) - A0 case
					uniTrk.UniVolEffect(VolEffect.VolSlide, 0);
				else if (volPan <= 74)				// Fine volume slide up (65-74) - General case
					uniTrk.UniVolEffect(VolEffect.VolSlide, (byte)(0x0f + ((volPan - 65) << 4)));
				else if (volPan == 75)				// Fine volume slide down (75-84) - B0 case
					uniTrk.UniVolEffect(VolEffect.VolSlide, 0);
				else if (volPan <= 84)				// Fine volume slide down (75-84) - General case
					uniTrk.UniVolEffect(VolEffect.VolSlide, (byte)(0xf0 + (volPan - 75)));
				else if (volPan <= 94)				// Volume slide up (85-94)
					uniTrk.UniVolEffect(VolEffect.VolSlide, (byte)(((volPan - 85) << 4)));
				else if (volPan <= 104)				// Volume slide down (95-104)
					uniTrk.UniVolEffect(VolEffect.VolSlide, (byte)(volPan - 95));
				else if (volPan <= 114)				// Pitch slide down (105-114)
					uniTrk.UniVolEffect(VolEffect.PitchSlideDn, (byte)(volPan - 105));
				else if (volPan <= 124)				// Pitch slide up (115-124)
					uniTrk.UniVolEffect(VolEffect.PitchSlideUp, (byte)(volPan - 115));
				else if (volPan <= 127)				// Crap
					return null;
				else if (volPan <= 192)
					uniTrk.UniVolEffect(VolEffect.Panning, (byte)(((volPan - 128) == 64) ? 255 : ((volPan - 128) << 2)));
				else if (volPan <= 202)				// Portamento to note
					uniTrk.UniVolEffect(VolEffect.Portamento, portaTable[volPan - 193]);
				else if (volPan <= 212)				// Vibrato
					uniTrk.UniVolEffect(VolEffect.Vibrato, (byte)(volPan - 203));
				else if ((volPan != 239) && (volPan != 255))	// Crap
					return null;

				util.ProcessCmd(of, tr[offset + t * of.NumChn].Cmd, tr[offset + t * of.NumChn].Inf, oldEffect | ProcessFlags.It, uniTrk);

				uniTrk.UniNewLine();
			}

			return uniTrk.UniDup();
		}



		/********************************************************************/
		/// <summary>
		/// Read one pattern
		/// </summary>
		/********************************************************************/
		private bool ReadPattern(ModuleStream moduleStream, ushort patRows, MUniTrk uniTrk)
		{
			int row = 0;
			byte[] blah = new byte[4];
			int itt = 0;
			ItNote dummy = new ItNote();
			int ite = 200 * 64 - 1;
			byte[] m;

			// Clear pattern data
			for (int i = 0; i < 200 * 64; i++)
			{
				ItNote n = itPat[i];
				n.Note = n.Ins = n.VolPan = n.Cmd = n.Inf = 255;
			}

			do
			{
				if (moduleStream.EndOfStream)
					return false;

				int flag = moduleStream.Read_UINT8();

				if (flag == 0)
				{
					itt = itt + of.NumChn;
					row++;
				}
				else
				{
					int ch = util.remap[(flag - 1) & 63];

					ItNote n, l;
					int mOffset;

					if (ch != 255)
					{
						n = itPat[itt + ch];
						l = last[ch];
						m = mask;
						mOffset = ch;

						if ((itt + ch) > ite)
						{
							// Malformed file
							return false;
						}
					}
					else
					{
						n = l = dummy;
						Array.Clear(blah, 0, 4);
						m = blah;
						mOffset = 0;
					}

					if ((flag & 128) != 0)
						m[mOffset] = moduleStream.Read_UINT8();

					if ((m[mOffset] & 1) != 0)
					{
						// Convert IT note off to internal note off
						if ((l.Note = n.Note = moduleStream.Read_UINT8()) == 255)
							l.Note = n.Note = 253;
					}

					if ((m[mOffset] & 2) != 0)
						l.Ins = n.Ins = moduleStream.Read_UINT8();

					if ((m[mOffset] & 4) != 0)
						l.VolPan = n.VolPan = moduleStream.Read_UINT8();

					if ((m[mOffset] & 8) != 0)
					{
						l.Cmd = n.Cmd = moduleStream.Read_UINT8();
						l.Inf = n.Inf = moduleStream.Read_UINT8();
					}

					if ((m[mOffset] & 16) != 0)
						n.Note = l.Note;

					if ((m[mOffset] & 32) != 0)
						n.Ins = l.Ins;

					if ((m[mOffset] & 64) != 0)
						n.VolPan = l.VolPan;

					if ((m[mOffset] & 128) != 0)
					{
						n.Cmd = l.Cmd;
						n.Inf = l.Inf;
					}
				}
			}
			while (row < patRows);

			for (int i = 0; i < of.NumChn; i++)
			{
				if ((of.Tracks[numTrk++] = ConvertTrack(itPat, i, patRows, uniTrk)) == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Loads a midi string from the module
		/// </summary>
		/********************************************************************/
		private void LoadMidiString(ModuleStream moduleStream, byte[] dest)
		{
			Array.Clear(dest, 0, 33);			// Caller sends midiLine[33]
			moduleStream.Read(dest, 0, 32);

			int cur = 0, last = 0;

			// Remove blanks and uppercase all
			while (dest[last] != 0)
			{
				if (char.IsLetterOrDigit((char)dest[last]))
					dest[cur++] = (byte)char.ToUpper((char)dest[last]);

				last++;
			}

			dest[cur] = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Loads embedded midi information for resonant filters
		/// </summary>
		/********************************************************************/
		private void LoadMidiConfiguration(ModuleStream moduleStream)
		{
			Array.Clear(util.filterMacros, 0, util.filterMacros.Length);
			Array.Clear(util.filterSettings, 0, util.filterSettings.Length);

			if (moduleStream != null)
			{
				// Information is embedded in file
				byte[] midiLine = new byte[33];

				ushort dat = moduleStream.Read_L_UINT16();
				moduleStream.Seek(8 * dat + 0x120, SeekOrigin.Current);

				// Read mini macros
				for (int i = 0; i < SharedConstant.UF_MaxMacro; i++)
				{
					LoadMidiString(moduleStream, midiLine);

					if ((midiLine[0] == 'F') && (midiLine[1] == '0') && (midiLine[2] == 'F') && (midiLine[3] == '0') && (midiLine[4] == '0') && ((midiLine[5] == '0') || (midiLine[5] == '1')))
						util.filterMacros[i] = (byte)((midiLine[5] - '0') | 0x80);
				}

				// Read standalone filters
				for (int i = 0x80; i < 0x100; i++)
				{
					LoadMidiString(moduleStream, midiLine);

					if ((midiLine[0] == 'F') && (midiLine[1] == '0') && (midiLine[2] == 'F') && (midiLine[3] == '0') && (midiLine[4] == '0') && ((midiLine[5] == '0') || (midiLine[5] == '1')))
					{
						util.filterSettings[i].FilterVal = (byte)((midiLine[5] - '0') | 0x80);
						dat = (ushort)((midiLine[6] != 0) ? (midiLine[6] - '0') : 0);

						if (midiLine[7] != 0)
							dat = (ushort)((dat << 4) | (midiLine[7] - '0'));

						util.filterSettings[i].Inf = (byte)dat;
					}
				}
			}
			else
			{
				// Use default information
				util.filterMacros[0] = SharedConstant.Filt_Cut;

				for (int i = 0x80; i < 0x90; i++)
				{
					util.filterSettings[i].FilterVal = SharedConstant.Filt_Resonant;
					util.filterSettings[i].Inf = (byte)((i & 0x7f) << 3);
				}
			}

			util.activeMacro = 0;

			for (int i = 0; i < 0x80; i++)
			{
				util.filterSettings[i].FilterVal = util.filterMacros[0];
				util.filterSettings[i].Inf = (byte)i;
			}
		}
		#endregion
	}
}

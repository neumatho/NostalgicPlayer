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
	/// MikMod loader for FastTracker 2 format
	/// </summary>
	internal class MikModConverterWorker_Xm : MikModConverterWorkerBase
	{
		#region XmHeader class
		private class XmHeader
		{
			public byte[] Id = new byte[18];				// ID text: 'Extended Module: '
			public byte[] SongName = new byte[21];			// Module name, padded with zeros
			public byte[] TrackerName = new byte[21];		// Tracker name
			public ushort Version;							// Version number
			public uint HeaderSize;							// Header size
			public ushort SongLength;						// Song length (in pattern order table)
			public ushort Restart;							// Restart position
			public ushort NumChn;							// Number of channels (2, 4, 6, 8, 10, ..., 32)
			public ushort NumPat;							// Number of patterns (max 256)
			public ushort NumIns;							// Number of instruments (max 128)
			public ushort Flags;
			public ushort Tempo;							// Default tempo
			public ushort Bpm;								// Default BPM
			public byte[] Orders = new byte[256];			// Pattern order table
		}
		#endregion

		#region XmInstHeader class
		private class XmInstHeader
		{
			public uint Size;								// Instrument size
			public byte[] Name = new byte[23];				// Instrument name
			public byte Type;								// Instrument type (always 0)
			public ushort NumSmp;							// Number of samples in instrument
			public uint SSize;
		}
		#endregion

		#region XmPatchHeader
		private class XmPatchHeader
		{
			public byte[] What = new byte[XmNoteCnt];		// Sample number for all notes
			public ushort[] VolEnv = new ushort[XmEnvCnt];	// Points for volume envelope
			public ushort[] PanEnv = new ushort[XmEnvCnt];	// Points for panning envelope
			public byte VolPts;								// Number of volume points
			public byte PanPts;								// Number of panning points
			public byte VolSus;								// Volume sustain point
			public byte VolBeg;								// Volume loop start point
			public byte VolEnd;								// Volume loop end point
			public byte PanSus;								// Panning sustain point
			public byte PanBeg;								// Panning loop start point
			public byte PanEnd;								// Panning loop end point
			public byte VolFlg;								// Volume type: Bit 0: On, 1: Sustain, 2: Loop
			public byte PanFlg;								// Panning type: Bit 0: On, 1: Sustain, 2: Loop
			public byte VibFlg;								// Vibrato type
			public byte VibSweep;							// Vibrato sweep
			public byte VibDepth;							// Vibrato depth
			public byte VibRate;							// Vibrato rate
			public ushort VolFade;							// Volume fadeout
		}
		#endregion

		#region XmWavHeader class
		private class XmWavHeader
		{
			public uint Length;								// Sample length
			public uint LoopStart;							// Sample loop start
			public uint LoopLength;							// Sample loop length;
			public byte Volume;								// Volume
			public sbyte FineTune;							// Fine tune (signed byte -128..+127)
			public byte Type;								// Loop type
			public byte Panning;							// Panning (0-255)
			public sbyte RelNote;							// Relative note number (signed byte)
			public byte Reserved;
			public byte[] SampleName = new byte[23];		// Sample name
			public byte VibType;							// Vibrato type
			public byte VibSweep;							// Vibrato sweep
			public byte VibDepth;							// Vibrato depth
			public byte VibRate;							// Vibrato rate
		}
		#endregion

		#region XmPatHeader class
		private class XmPatHeader
		{
			public uint Size;								// Pattern header length
			public byte Packing;							// Packing type (always 0)
			public ushort NumRows;							// Number of rows in pattern (1..256)
			public short PackSize;							// Packed pattern data size
		}
		#endregion

		#region XmNote class
		private class XmNote
		{
			public byte Note;
			public byte Ins;
			public byte Vol;
			public byte Eff;
			public byte Dat;
		}
		#endregion

		private const int XmSmpIncr = 64;

		private const int XmEnvCnt = 12 * 2;
		private const int XmNoteCnt = 8 * SharedConstant.Octave;

		private XmHeader mh;
		private XmWavHeader[] wh;
		private long[] nextWav;

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
			if (fileSize < 337)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[38];
			moduleStream.Read(buf, 0, 38);

			if (Encoding.ASCII.GetString(buf, 0, 17) != "Extended Module: ")
				return AgentResult.Unknown;

			if (buf[37] != 0x1a)
				return AgentResult.Unknown;

			// Check the version
			moduleStream.Seek(58, SeekOrigin.Begin);
			ushort ver = moduleStream.Read_L_UINT16();

			if ((ver < 0x102) || (ver > 0x104))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		protected override bool LoadModule(ModuleStream moduleStream, MUniTrk uniTrk, out string errorMessage)
		{
			errorMessage = string.Empty;

			mh = new XmHeader();

			try
			{
				Encoding encoder = EncoderCollection.Ibm850;

				// Try to read module header
				moduleStream.ReadString(mh.Id, 17);
				moduleStream.ReadString(mh.SongName, 20);
				moduleStream.Seek(1, SeekOrigin.Current);		// Skip 0x1a
				moduleStream.ReadString(mh.TrackerName, 20);

				mh.Version = moduleStream.Read_L_UINT16();
				mh.HeaderSize = moduleStream.Read_L_UINT32();
				mh.SongLength = moduleStream.Read_L_UINT16();
				mh.Restart = moduleStream.Read_L_UINT16();
				mh.NumChn = moduleStream.Read_L_UINT16();
				mh.NumPat = moduleStream.Read_L_UINT16();
				mh.NumIns = moduleStream.Read_L_UINT16();
				mh.Flags = moduleStream.Read_L_UINT16();
				mh.Tempo = moduleStream.Read_L_UINT16();
				mh.Bpm = moduleStream.Read_L_UINT16();

				if (mh.NumChn > 64)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				if ((mh.Tempo > 32) || (mh.Bpm < 32) || (mh.Bpm > 255))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				if ((mh.SongLength > 256) || (mh.HeaderSize < 20) || (mh.HeaderSize > 20 +256))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				if ((mh.NumPat > 256) || (mh.NumIns > 255) || (mh.Restart > 255))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}

				moduleStream.Read(mh.Orders, 0, mh.SongLength);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				moduleStream.Seek(mh.HeaderSize + 60, SeekOrigin.Begin);

				// Set module variables
				of.InitSpeed = (byte)mh.Tempo;
				of.InitTempo = mh.Bpm;

				string tracker = encoder.GetString(mh.TrackerName);
				tracker = tracker.TrimEnd();

				// Some modules have the tracker name empty
				if (string.IsNullOrEmpty(tracker))
					tracker = Resources.IDS_MIKCONV_NAME_XM_UNKNOWN;

				originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_XM, tracker, mh.Version >> 8, mh.Version & 0xff);

				of.NumChn = (byte)mh.NumChn;
				of.NumPat = mh.NumPat;
				of.NumTrk = (ushort)(of.NumPat * of.NumChn);						// Get number of channels
				of.SongName = encoder.GetString(mh.SongName);
				of.NumPos = mh.SongLength;											// Copy the song length
				of.RepPos = mh.Restart < mh.SongLength ? mh.Restart : (ushort)0;
				of.NumIns = mh.NumIns;
				of.Flags |= ModuleFlag.XmPeriods | ModuleFlag.Inst | ModuleFlag.NoWrap | ModuleFlag.Ft2Quirks | ModuleFlag.Panning;

				if ((mh.Flags & 1) != 0)
					of.Flags |= ModuleFlag.Linear;

				of.BpmLimit = 32;

				Array.Fill<byte>(of.ChanVol, 64, 0, of.NumChn);	// Store channel volumes

				if (!MLoader.AllocPositions(of, of.NumPos + 1))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (int t = 0; t < of.NumPos; t++)
					of.Positions[t] = mh.Orders[t];

				// We have to check for any pattern numbers in the order list greater than
				// the number of patterns total. If one or more is found, we set it equal to
				// the pattern total and make a dummy pattern to workaround the problem
				bool dummyPat = false;

				for (int t = 0; t < of.NumPos; t++)
				{
					if (of.Positions[t] >= of.NumPat)
					{
						of.Positions[t] = of.NumPat;
						dummyPat = true;
					}
				}

				if (dummyPat)
				{
					of.NumPat++;
					of.NumTrk += of.NumChn;
				}

				if (mh.Version < 0x0104)
				{
					if (!LoadInstruments(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
						return false;
					}

					if (!LoadPatterns(moduleStream, uniTrk, dummyPat))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					for (int t = 0; t < of.NumSmp; t++)
						nextWav[t] += moduleStream.Position;
				}
				else
				{
					if (!LoadPatterns(moduleStream, uniTrk, dummyPat))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					if (!LoadInstruments(moduleStream))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
						return false;
					}
				}

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLES;
					return false;
				}

				for (int u = 0; u < of.NumSmp; u++)
				{
					Sample q = of.Samples[u];
					XmWavHeader s = wh[u];

					q.SampleName = encoder.GetString(s.SampleName);
					q.Length = s.Length;
					q.LoopStart = s.LoopStart;
					q.LoopEnd = s.LoopStart + s.LoopLength;
					q.Volume = s.Volume;
					q.Speed = (uint)(s.FineTune + 128);
					q.Panning = s.Panning;
					q.SeekPos = (uint)nextWav[u];
					q.VibType = s.VibType;
					q.VibSweep = s.VibSweep;
					q.VibDepth = s.VibDepth;
					q.VibRate = s.VibRate;

					if ((s.Type & 0x10) != 0)
					{
						q.Length >>= 1;
						q.LoopStart >>= 1;
						q.LoopEnd >>= 1;
					}

					q.Flags |= SampleFlag.OwnPan | SampleFlag.Delta | SampleFlag.Signed;

					if ((s.Type & 0x03) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Type & 0x02) != 0)
						q.Flags |= SampleFlag.Bidi;

					if ((s.Type & 0x10) != 0)
						q.Flags |= SampleFlag._16Bits;
				}

				for (int u = 0; u < of.NumIns; u++)
				{
					Instrument d = of.Instruments[u];

					for (int t = 0; t < XmNoteCnt; t++)
					{
						if (d.SampleNumber[t] >= of.NumSmp)
							d.SampleNote[t] = 255;
						else
						{
							int note = t + wh[d.SampleNumber[t]].RelNote;
							d.SampleNote[t] = (byte)((note < 0) ? 0 : note);
						}
					}
				}
			}
			finally
			{
				wh = null;
				nextWav = null;
				mh = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will read one note from the pattern
		/// </summary>
		/********************************************************************/
		private int Xm_ReadNote(ModuleStream moduleStream, XmNote n)
		{
			byte result = 1;

			byte cmp = moduleStream.Read_UINT8();

			if ((cmp & 0x80) != 0)
			{
				if ((cmp & 1) != 0)
				{
					result++;
					n.Note = moduleStream.Read_UINT8();
				}

				if ((cmp & 2) != 0)
				{
					result++;
					n.Ins = moduleStream.Read_UINT8();
				}

				if ((cmp & 4) != 0)
				{
					result++;
					n.Vol = moduleStream.Read_UINT8();
				}

				if ((cmp & 8) != 0)
				{
					result++;
					n.Eff = moduleStream.Read_UINT8();
				}

				if ((cmp & 16) != 0)
				{
					result++;
					n.Dat = moduleStream.Read_UINT8();
				}
			}
			else
			{
				n.Note = cmp;
				n.Ins = moduleStream.Read_UINT8();
				n.Vol = moduleStream.Read_UINT8();
				n.Eff = moduleStream.Read_UINT8();
				n.Dat = moduleStream.Read_UINT8();

				result += 4;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Convert one track in one pattern
		/// </summary>
		/********************************************************************/
		private byte[] Xm_Convert(MUniTrk uniTrk, XmNote[] tracks, int trackOffset, ushort rows)
		{
			uniTrk.UniReset();

			for (int t = 0; t < rows; t++)
			{
				XmNote xmTrack = tracks[trackOffset];

				byte note = xmTrack.Note;
				byte ins = xmTrack.Ins;
				byte vol = xmTrack.Vol;
				byte eff = xmTrack.Eff;
				byte dat = xmTrack.Dat;

				if (note != 0)
				{
					if (note > XmNoteCnt)
						uniTrk.UniEffect(Command.UniKeyFade, 0);
					else
						uniTrk.UniNote((ushort)(note - 1));
				}

				if (ins != 0)
					uniTrk.UniInstrument((ushort)(ins - 1));

				switch (vol >> 4)
				{
					// Volslide down
					case 0x6:
					{
						if ((vol & 0xf) != 0)
							uniTrk.UniEffect(Command.UniXmEffectA, (ushort)(vol & 0xf));

						break;
					}

					// Volslide up
					case 0x7:
					{
						if ((vol & 0xf) != 0)
							uniTrk.UniEffect(Command.UniXmEffectA, (ushort)(vol << 4));

						break;
					}

					// Volume-row fine volume slide is compatible with ProTracker
					// EBx and EAx effects, i.e. a zero nibble means DO NOT SLIDE, as
					// opposed to 'take the last sliding value'
					//
					// Finevol down
					case 0x8:
					{
						uniTrk.UniPtEffect(0xe, (byte)(0xb0 | (vol & 0xf)), of.Flags);
						break;
					}

					// Finevol up
					case 0x9:
					{
						uniTrk.UniPtEffect(0xe, (byte)(0xa0 | (vol & 0xf)), of.Flags);
						break;
					}

					// Set vibrato speed
					case 0xa:
					{
						uniTrk.UniEffect(Command.UniXmEffect4, (ushort)(vol << 4));
						break;
					}

					// Vibrato
					case 0xb:
					{
						uniTrk.UniEffect(Command.UniXmEffect4, (ushort)(vol & 0xf));
						break;
					}

					// Set panning
					case 0xc:
					{
						uniTrk.UniPtEffect(0x8, (byte)(vol << 4), of.Flags);
						break;
					}

					// Panning slide left (only slide when data not zero)
					case 0xd:
					{
						if ((vol & 0xf) != 0)
							uniTrk.UniEffect(Command.UniXmEffectP, (ushort)(vol & 0xf));

						break;
					}

					// Panning slide right (only slide when data not zero)
					case 0xe:
					{
						if ((vol & 0xf) != 0)
							uniTrk.UniEffect(Command.UniXmEffectP, (ushort)(vol << 4));

						break;
					}

					// Tone portamento
					case 0xf:
					{
						uniTrk.UniPtEffect(0x3, (byte)(vol << 4), of.Flags);
						break;
					}

					default:
					{
						if ((vol >= 0x10) && (vol <= 0x50))
							uniTrk.UniPtEffect(0xc, (byte)(vol - 0x10), of.Flags);

						break;
					}
				}

				switch (eff)
				{
					// Vibrato
					case 0x4:
					{
						uniTrk.UniEffect(Command.UniXmEffect4, dat);
						break;
					}

					// Vibrato + volume slide
					case 0x6:
					{
						uniTrk.UniEffect(Command.UniXmEffect6, dat);
						break;
					}

					// Volume slide
					case 0xa:
					{
						uniTrk.UniEffect(Command.UniXmEffectA, dat);
						break;
					}

					// Fine porta/volume slide + extra effects
					case 0xe:
					{
						switch (dat >> 4)
						{
							// XM fine porta up
							case 0x1:
							{
								uniTrk.UniEffect(Command.UniXmEffectE1, (ushort)(dat & 0xf));
								break;
							}

							// XM fine porta down
							case 0x2:
							{
								uniTrk.UniEffect(Command.UniXmEffectE2, (ushort)(dat & 0xf));
								break;
							}

							// XM fine volume up
							case 0xa:
							{
								uniTrk.UniEffect(Command.UniXmEffectEA, (ushort)(dat & 0xf));
								break;
							}

							// XM fine volume down
							case 0xb:
							{
								uniTrk.UniEffect(Command.UniXmEffectEB, (ushort)(dat & 0xf));
								break;
							}

							default:
							{
								uniTrk.UniPtEffect(eff, dat, of.Flags);
								break;
							}
						}
						break;
					}

					// Set global volume
					case 'G' - 55:
					{
						uniTrk.UniEffect(Command.UniXmEffectG, (ushort)(dat > 64 ? 128 : dat << 1));
						break;
					}

					// Global volume slide
					case 'H' - 55:
					{
						uniTrk.UniEffect(Command.UniXmEffectH, dat);
						break;
					}

					// Keyoff and key fade
					case 'K' - 55:
					{
						uniTrk.UniEffect(Command.UniKeyFade, dat);
						break;
					}

					// Set envelope position
					case 'L' - 55:
					{
						uniTrk.UniEffect(Command.UniXmEffectL, dat);
						break;
					}

					// Panning slide
					case 'P' - 55:
					{
						uniTrk.UniEffect(Command.UniXmEffectP, dat);
						break;
					}

					// Multi retrig note
					case 'R' - 55:
					{
						uniTrk.UniEffect(Command.UniS3MEffectQ, dat);
						break;
					}

					// Tremor
					case 'T' - 55:
					{
						uniTrk.UniEffect(Command.UniS3MEffectI, dat);
						break;
					}

					// Extra fine porta
					case 'X' - 55:
					{
						switch (dat >> 4)
						{
							// Extra fine portamento up
							case 1:
							{
								uniTrk.UniEffect(Command.UniXmEffectX1, (ushort)(dat & 0xf));
								break;
							}

							// Extra fine portamento down
							case 2:
							{
								uniTrk.UniEffect(Command.UniXmEffectX2, (ushort)(dat & 0xf));
								break;
							}
						}
						break;
					}

					default:
					{
						if (eff <= 0xf)
						{
							// The pattern jump destination is written in decimal,
							// but it seems some poor tracker software writes them
							// in hexadecimal... (sigh)
							if (eff == 0xd)
							{
								// Don't change anything if we're sure it's in hexa
								if ((((dat & 0xf0) >> 4) <= 9) && ((dat & 0xf) <= 9))
								{
									// Otherwise, convert them from dec to hex
									dat = (byte)((((dat & 0xf0) >> 4) * 10) + (dat & 0xf));
								}
							}

							uniTrk.UniPtEffect(eff, dat, of.Flags);
						}
						break;
					}
				}

				uniTrk.UniNewLine();
				trackOffset++;
			}

			return uniTrk.UniDup();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the patterns
		/// </summary>
		/********************************************************************/
		private bool LoadPatterns(ModuleStream moduleStream, MUniTrk uniTrk, bool dummyPat)
		{
			if (!MLoader.AllocTracks(of))
				return false;

			if (!MLoader.AllocPatterns(of))
				return false;

			int numTrk = 0;
			int t;
			for (t = 0; t < mh.NumPat; t++)
			{
				XmPatHeader ph = new XmPatHeader();

				ph.Size = moduleStream.Read_L_UINT32();
				if (ph.Size < (mh.Version == 0x0102 ? 8 : 9))
					return false;

				ph.Packing = moduleStream.Read_UINT8();
				if (ph.Packing != 0)
					return false;

				if (mh.Version == 0x0102)
					ph.NumRows = (ushort)(moduleStream.Read_UINT8() + 1);
				else
					ph.NumRows = moduleStream.Read_L_UINT16();

				ph.PackSize = (short)moduleStream.Read_L_UINT16();

				ph.Size -= (mh.Version == 0x0102 ? (uint)8 : 9);
				if (ph.Size != 0)
					moduleStream.Seek(ph.Size, SeekOrigin.Current);

				of.PattRows[t] = ph.NumRows;

				if (ph.NumRows != 0)
				{
					XmNote[] xmPat = Helpers.InitializeArray<XmNote>(ph.NumRows * of.NumChn);

					// When PackSize is 0, don't try to load a pattern.. it's empty
					if (ph.PackSize != 0)
					{
						for (int u = 0; u < ph.NumRows; u++)
						{
							for (int v = 0; v < of.NumChn; v++)
							{
								if (ph.PackSize == 0)
									break;

								ph.PackSize -= (short)Xm_ReadNote(moduleStream, xmPat[(v * ph.NumRows) + u]);
								if (ph.PackSize < 0)
									return false;
							}
						}
					}

					if (ph.PackSize != 0)
						moduleStream.Seek(ph.PackSize, SeekOrigin.Current);

					if (moduleStream.EndOfStream)
						return false;

					for (int v = 0; v < of.NumChn; v++)
						of.Tracks[numTrk++] = Xm_Convert(uniTrk, xmPat, v * ph.NumRows, ph.NumRows);
				}
				else
				{
					for (int v = 0; v < of.NumChn; v++)
						of.Tracks[numTrk++] = Xm_Convert(uniTrk,null, 0, ph.NumRows);
				}
			}

			if (dummyPat)
			{
				of.PattRows[t] = 64;

				XmNote[] xmPat = Helpers.InitializeArray<XmNote>(64 * of.NumChn);

				for (int v = 0; v < of.NumChn; v++)
					of.Tracks[numTrk++] = Xm_Convert(uniTrk, xmPat, v * 64, 64);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to detect and fix corrupted envelopes
		/// </summary>
		/********************************************************************/
		private void FixEnvelope(EnvPt[] env, int pts)
		{
			// Some broken XM editing program will only save the low byte
			// of the position value. Try to compensate by adding the
			// missing high byte
			int curIndex = 0;
			int prevIndex = curIndex++;

			int old = env[prevIndex].Pos;

			for (int u = 1; u < pts; u++, prevIndex++, curIndex++)
			{
				ref EnvPt prev = ref env[prevIndex];
				ref EnvPt cur = ref env[curIndex];

				if (cur.Pos < prev.Pos)
				{
					if (cur.Pos < 0x100)
					{
						int tmp;

						if (cur.Pos > old)		// Same hex century
							tmp = cur.Pos + (prev.Pos - old);
						else
							tmp = (ushort)cur.Pos | ((prev.Pos + 0x100) & 0xff00);

						old = cur.Pos;
						cur.Pos = (short)tmp;
					}
					else
					{
						// Different brokenness style... fix unknown
						old = cur.Pos;
					}
				}
				else
					old = cur.Pos;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream)
		{
			Encoding encoder = EncoderCollection.Ibm850;

			long filEnd = moduleStream.Length;

			if (!MLoader.AllocInstruments(of))
				return false;

			long next = 0;
			ushort wavCnt = 0;
			int sIndex = 0;

			for (int t = 0; t < of.NumIns; t++)
			{
				Instrument d = of.Instruments[t];

				XmInstHeader ih = new XmInstHeader();

				Array.Fill<ushort>(d.SampleNumber, 0xffff);

				// Read instrument header
				long headEnd = moduleStream.Position;
				ih.Size = moduleStream.Read_L_UINT32();
				headEnd += ih.Size;

				long ck = moduleStream.Position;
				if ((headEnd < 0) || (filEnd < headEnd) || (headEnd < ck))
					break;

				moduleStream.ReadString(ih.Name, 22);

				ih.Type = moduleStream.Read_UINT8();
				ih.NumSmp = moduleStream.Read_L_UINT16();

				d.InsName = encoder.GetString(ih.Name);

				if ((short)ih.Size > 29)
				{
					ih.SSize = moduleStream.Read_L_UINT32();

					if (((short)ih.NumSmp > 0) && (ih.NumSmp <= XmNoteCnt))
					{
						XmPatchHeader pth = new XmPatchHeader();

						moduleStream.Read(pth.What, 0, XmNoteCnt);
						moduleStream.ReadArray_L_UINT16s(pth.VolEnv, XmEnvCnt);
						moduleStream.ReadArray_L_UINT16s(pth.PanEnv, XmEnvCnt);

						pth.VolPts = moduleStream.Read_UINT8();
						pth.PanPts = moduleStream.Read_UINT8();
						pth.VolSus = moduleStream.Read_UINT8();
						pth.VolBeg = moduleStream.Read_UINT8();
						pth.VolEnd = moduleStream.Read_UINT8();
						pth.PanSus = moduleStream.Read_UINT8();
						pth.PanBeg = moduleStream.Read_UINT8();
						pth.PanEnd = moduleStream.Read_UINT8();
						pth.VolFlg = moduleStream.Read_UINT8();
						pth.PanFlg = moduleStream.Read_UINT8();
						pth.VibFlg = moduleStream.Read_UINT8();
						pth.VibSweep = moduleStream.Read_UINT8();
						pth.VibDepth = moduleStream.Read_UINT8();
						pth.VibRate = moduleStream.Read_UINT8();
						pth.VolFade = moduleStream.Read_L_UINT16();

						// Read the remainder of the header
						// (2 bytes for 1.03, 22 for 1.04)
						if (headEnd >= moduleStream.Position)
						{
							for (int u = (int)(headEnd - moduleStream.Position); u > 0; u--)
								moduleStream.Seek(1, SeekOrigin.Current);
						}

						// We can't trust the envelope point count here, as some
						// modules have incorrect values (K_OSPACE.XM reports 32 volume
						// points, for example)
						if (pth.VolPts > XmEnvCnt / 2)
							pth.VolPts = XmEnvCnt / 2;

						if (pth.PanPts > XmEnvCnt / 2)
							pth.PanPts = XmEnvCnt / 2;

						if (moduleStream.EndOfStream || (pth.VolPts > XmEnvCnt / 2) || (pth.PanPts > XmEnvCnt / 2))
							return false;

						for (int u = 0; u < XmNoteCnt; u++)
							d.SampleNumber[u] = (ushort)(pth.What[u] + of.NumSmp);

						d.VolFade = pth.VolFade;

						for (int u = 0; u < (XmEnvCnt >> 1); u++)
						{
							d.VolEnv[u].Pos = (short)pth.VolEnv[u << 1];
							d.VolEnv[u].Val = (short)pth.VolEnv[(u << 1) + 1];
						}

						if ((pth.VolFlg & 1) != 0)
							d.VolFlg |= EnvelopeFlag.On;

						if ((pth.VolFlg & 2) != 0)
							d.VolFlg |= EnvelopeFlag.Sustain;

						if ((pth.VolFlg & 4) != 0)
							d.VolFlg |= EnvelopeFlag.Loop;

						d.VolSusBeg = d.VolSusEnd = pth.VolSus;
						d.VolBeg = pth.VolBeg;
						d.VolEnd = pth.VolEnd;
						d.VolPts = pth.VolPts;

						// Scale envelope
						for (int p = 0; p < XmEnvCnt / 2; p++)
							d.VolEnv[p].Val <<= 2;

						if (((d.VolFlg & EnvelopeFlag.On) != 0) && (d.VolPts < 2))
							d.VolFlg &= ~EnvelopeFlag.On;

						for (int u = 0; u < (XmEnvCnt >> 1); u++)
						{
							d.PanEnv[u].Pos = (short)pth.PanEnv[u << 1];
							d.PanEnv[u].Val = (short)pth.PanEnv[(u << 1) + 1];
						}

						if ((pth.PanFlg & 1) != 0)
							d.PanFlg |= EnvelopeFlag.On;

						if ((pth.PanFlg & 2) != 0)
							d.PanFlg |= EnvelopeFlag.Sustain;

						if ((pth.PanFlg & 4) != 0)
							d.PanFlg |= EnvelopeFlag.Loop;

						d.PanSusBeg = d.PanSusEnd = pth.PanSus;
						d.PanBeg = pth.PanBeg;
						d.PanEnd = pth.PanEnd;
						d.PanPts = pth.PanPts;

						// Scale envelope
						for (int p = 0; p < XmEnvCnt / 2; p++)
							d.PanEnv[p].Val <<= 2;

						if (((d.PanFlg & EnvelopeFlag.On) != 0) && (d.PanPts < 2))
							d.PanFlg &= ~EnvelopeFlag.On;

						if ((d.VolFlg & EnvelopeFlag.On) != 0)
							FixEnvelope(d.VolEnv, d.VolPts);

						if ((d.PanFlg & EnvelopeFlag.On) != 0)
							FixEnvelope(d.PanEnv, d.PanPts);

						// Samples are stored outside the instrument struct now, so we
						// have to load them all into a temp area, count the of.numSmp
						// along the way and then do an AllocSamples() and move
						// everything over
						if (mh.Version > 0x0103)
							next = 0;

						for (int u = 0; u < ih.NumSmp; u++, sIndex++)
						{
							// XM sample header is 40 bytes: make sure we won't hit EOF
							// Note: last instrument is at the end of file in version 0x0104
							if (moduleStream.Position + 40 > filEnd)
								return false;

							// Allocate more room for sample information if necessary
							if (of.NumSmp + u == wavCnt)
							{
								wavCnt += XmSmpIncr;

								long[] newNextWav = new long[wavCnt];

								if (nextWav != null)
									Array.Copy(nextWav, newNextWav, wavCnt - XmSmpIncr);

								nextWav = newNextWav;

								XmWavHeader[] newWave = new XmWavHeader[wavCnt];

								if (wh != null)
									Array.Copy(wh, newWave, wavCnt - XmSmpIncr);

								wh = newWave;

								sIndex = wavCnt - XmSmpIncr;
							}

							XmWavHeader s = new XmWavHeader();
							wh[sIndex] = s;

							s.Length = moduleStream.Read_L_UINT32();
							s.LoopStart = moduleStream.Read_L_UINT32();
							s.LoopLength = moduleStream.Read_L_UINT32();
							s.Volume = moduleStream.Read_UINT8();
							s.FineTune = moduleStream.Read_INT8();
							s.Type = moduleStream.Read_UINT8();
							s.Panning = moduleStream.Read_UINT8();
							s.RelNote = moduleStream.Read_INT8();
							s.VibType = pth.VibFlg;
							s.VibSweep = pth.VibSweep;
							s.VibDepth = (byte)(pth.VibDepth * 4);
							s.VibRate = pth.VibRate;
							s.Reserved = moduleStream.Read_UINT8();

							moduleStream.ReadString(s.SampleName, 22);

							nextWav[of.NumSmp + u] = next;
							next += s.Length;

							if (moduleStream.EndOfStream)
								return false;
						}

						if (mh.Version > 0x0103)
						{
							for (int u = 0; u < ih.NumSmp; u++)
								nextWav[of.NumSmp++] += moduleStream.Position;

							moduleStream.Seek(next, SeekOrigin.Current);
						}
						else
							of.NumSmp += ih.NumSmp;
					}
					else
					{
						// Read the remainder of the header
						ck = moduleStream.Position;
						if ((headEnd < 0) || (filEnd < headEnd) || (headEnd < ck))
							break;

						for (long u = headEnd - moduleStream.Position; u > 0; u--)
							moduleStream.Seek(1, SeekOrigin.Current);

						// Last instrument is at the end of file in version 0x0104
						if (moduleStream.EndOfStream && ((mh.Version < 0x0104) || (t < of.NumIns - 1)))
							return false;
					}
				}
			}

			// Sanity check
			if (of.NumSmp == 0)
				return false;

			return true;
		}
		#endregion
	}
}

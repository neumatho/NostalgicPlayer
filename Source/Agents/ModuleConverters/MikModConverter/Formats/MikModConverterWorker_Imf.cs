/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
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
	/// MikMod loader for IMF (Imago Orpheus) format
	/// </summary>
	internal class MikModConverterWorker_Imf : MikModConverterWorkerBase
	{
		#region ImfHeader class
		private class ImfHeader
		{
			public readonly byte[] SongName = new byte[33];
			public ushort OrdNum;
			public ushort PatNum;
			public ushort InsNum;
			public ushort Flags;
			public byte InitSpeed;
			public byte InitTempo;
			public byte MasterVol;
			public byte MasterMult;
			public readonly byte[] Orders = new byte[256];
		}
		#endregion

		#region ImfChannel class
		private class ImfChannel
		{
			public readonly byte[] Name = new byte[13];
			public byte Chorus;
			public byte Reverb;
			public byte Pan;
			public byte Status;
		}
		#endregion

		#region ImfInstHeader class
		private class ImfInstHeader
		{
			public readonly byte[] Name = new byte[33];
			public readonly byte[] What = new byte[ImfNoteCnt];
			public readonly ushort[] VolEnv = new ushort[ImfEnvCnt];
			public readonly ushort[] PanEnv = new ushort[ImfEnvCnt];
			public readonly ushort[] PitEnv = new ushort[ImfEnvCnt];
			public byte VolPts;
			public byte VolSus;
			public byte VolBeg;
			public byte VolEnd;
			public byte VolFlg;
			public byte PanPts;
			public byte PanSus;
			public byte PanBeg;
			public byte PanEnd;
			public byte PanFlg;
			public byte PitPts;
			public byte PitSus;
			public byte PitBeg;
			public byte PitEnd;
			public byte PitFlg;
			public ushort VolFade;
			public ushort NumSmp;
//			public uint Signature;
		}
		#endregion

		#region ImfWavHeader class
		private class ImfWavHeader
		{
			public readonly byte[] SampleName = new byte[14];
			public uint Length;
			public uint LoopStart;
			public uint LoopEnd;
			public uint SampleRate;
			public byte Volume;
			public byte Pan;
			public byte Flags;
		}
		#endregion

		#region ImfNote class
		private class ImfNote
		{
			public byte Note;
			public byte Ins;
			public byte Eff1;
			public byte Dat1;
			public byte Eff2;
			public byte Dat2;
		}
		#endregion

		private const int ImfNoteCnt = 10 * SharedConstant.Octave;
		private const int ImfEnvCnt = 16 * 2;
		private const int ImfSmpIncr = 64;

		private ImfHeader mh;
		private ImfNote[] imfPat;

		private MlUtil util;

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
			if (fileSize < 300)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] buf = new byte[512];

			moduleStream.Seek(0x3c, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 4);

			if ((buf[0] != 'I') || (buf[1] != 'M') || (buf[2] != '1') || (buf[3] != '0'))
				return AgentResult.Unknown;

			// Check count values
			moduleStream.Seek(32, SeekOrigin.Begin);

			if (moduleStream.Read_L_UINT16() > 256)
				return AgentResult.Unknown;			// Bad ordnum

			if (moduleStream.Read_L_UINT16() > 256)
				return AgentResult.Unknown;			// Bad patnum

			if (moduleStream.Read_L_UINT16() > 256)
				return AgentResult.Unknown;			// Bad insnum

			// Verify channel status
			moduleStream.Seek(64, SeekOrigin.Begin);
			if (moduleStream.Read(buf, 0, 512) != 512)
				return AgentResult.Unknown;

			int chn = 0;
			for (int t = 0, p = 15; t < 512; t += 16, p += 16)
			{
				switch (buf[p])
				{
					case 0:					        // Channel enabled
					case 1:							// Channel muted
					{
						chn++;
						break;
					}

					case 2:							// Channel disabled
						break;

					default:
						return AgentResult.Unknown;	// Bad status value
				}
			}

			if (chn == 0)
				return AgentResult.Unknown;			// No channels found

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

			imfPat = Helpers.InitializeArray<ImfNote>(32 * 256);
			mh = new ImfHeader();

			util = new MlUtil();

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				// Try to read the module header
				moduleStream.ReadString(mh.SongName, 32);

				mh.OrdNum = moduleStream.Read_L_UINT16();
				mh.PatNum = moduleStream.Read_L_UINT16();
				mh.InsNum = moduleStream.Read_L_UINT16();
				mh.Flags = moduleStream.Read_L_UINT16();

				moduleStream.Seek(8, SeekOrigin.Current);

				mh.InitSpeed = moduleStream.Read_UINT8();
				mh.InitTempo = moduleStream.Read_UINT8();
				mh.MasterVol = moduleStream.Read_UINT8();
				mh.MasterMult = moduleStream.Read_UINT8();

				moduleStream.Seek(64, SeekOrigin.Begin);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Set module variables
				of.SongName = encoder.GetString(mh.SongName);
				of.NumPat = mh.PatNum;
				of.NumIns = mh.InsNum;
				of.RepPos = 0;
				of.InitSpeed = mh.InitSpeed;
				of.InitTempo = mh.InitTempo;
				of.InitVolume = (byte)(mh.MasterVol << 1);
				of.Flags |= ModuleFlag.Inst | ModuleFlag.ArpMem | ModuleFlag.Panning;

				if ((mh.Flags & 1) != 0)
					of.Flags |= ModuleFlag.Linear;

				of.BpmLimit = 32;

				// Read channel information
				of.NumChn = 0;
				Array.Fill<byte>(util.remap, 255);

				ImfChannel[] channels = Helpers.InitializeArray<ImfChannel>(32);

				for (int t = 0; t < 32; t++)
				{
					moduleStream.ReadString(channels[t].Name, 12);

					channels[t].Chorus = moduleStream.Read_UINT8();
					channels[t].Reverb = moduleStream.Read_UINT8();
					channels[t].Pan = moduleStream.Read_UINT8();
					channels[t].Status = moduleStream.Read_UINT8();
				}

				// Bug in Imago Orpheus? If only channel 1 is enabled, in fact we
				// have to enable 16 channels
				if (channels[0].Status == 0)
				{
					int t;

					for (t = 1; t < 16; t++)
					{
						if (channels[t].Status != 1)
							break;
					}

					if (t == 16)
					{
						for (t = 1; t < 16; t++)
							channels[t].Status = 0;
					}
				}

				for (int t = 0; t < 32; t++)
				{
					if (channels[t].Status != 2)
						util.remap[t]= of.NumChn++;
					else
						util.remap[t] = 255;
				}

				for (int t = 0; t < 32; t++)
				{
					if (util.remap[t] != 255)
					{
						of.Panning[util.remap[t]] = channels[t].Pan;
						of.ChanVol[util.remap[t]] = (byte)(channels[t].Status != 0 ? 0 : 64);
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Read order list
				moduleStream.Read(mh.Orders, 0, 256);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				of.NumPos = 0;
				for (int t = 0; t < mh.OrdNum; t++)
				{
					if (mh.Orders[t] != 0xff)
						of.NumPos++;
				}

				if (!MLoader.AllocPositions(of, of.NumPos))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (int t = 0, u = 0; t < mh.OrdNum; t++)
				{
					if (mh.Orders[t] != 0xff)
						of.Positions[u++] = mh.Orders[t];
				}

				for (int t = 0; t < of.NumPos; t++)
				{
					if (of.Positions[t] > of.NumPat)	// Sanity check
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
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
					int size = moduleStream.Read_L_UINT16();
					ushort rows = moduleStream.Read_L_UINT16();

					if ((rows > 256) || (size < 4))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					of.PattRows[t] = rows;
					if (!ReadPattern(moduleStream, size - 4, rows))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
						return false;
					}

					for (int u = 0; u < of.NumChn; u++)
					{
						if ((of.Tracks[track++] = ConvertTrack(imfPat, u * 256, rows, uniTrk)) == null)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
							return false;
						}
					}
				}

				// Load instruments
				if (!MLoader.AllocInstruments(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_INSTRUMENTS;
					return false;
				}

				byte[] id = new byte[4];

				uint[] nextWav = null;
				ImfWavHeader[] wh = null;
				ushort wavCnt = 0;
				int sIndex = 0;

				for (int oldNumSmp = 0, t = 0; t < of.NumIns; t++)
				{
					ImfInstHeader ih = new ImfInstHeader();
					Instrument d = of.Instruments[t];

					Array.Fill<ushort>(d.SampleNumber, 0xffff, 0, SharedConstant.InstNotes);

					// Read instrument header
					moduleStream.ReadString(ih.Name, 32);
					d.InsName = encoder.GetString(ih.Name);

					moduleStream.Read(ih.What, 0, ImfNoteCnt);
					moduleStream.Seek(8, SeekOrigin.Current);

					moduleStream.ReadArray_L_UINT16s(ih.VolEnv, 0, ImfEnvCnt);
					moduleStream.ReadArray_L_UINT16s(ih.PanEnv, 0, ImfEnvCnt);
					moduleStream.ReadArray_L_UINT16s(ih.PitEnv, 0, ImfEnvCnt);

					ih.VolPts = moduleStream.Read_UINT8();
					ih.VolSus = moduleStream.Read_UINT8();
					ih.VolBeg = moduleStream.Read_UINT8();
					ih.VolEnd = moduleStream.Read_UINT8();
					ih.VolFlg = moduleStream.Read_UINT8();
					moduleStream.Seek(3, SeekOrigin.Current);

					ih.PanPts = moduleStream.Read_UINT8();
					ih.PanSus = moduleStream.Read_UINT8();
					ih.PanBeg = moduleStream.Read_UINT8();
					ih.PanEnd = moduleStream.Read_UINT8();
					ih.PanFlg = moduleStream.Read_UINT8();
					moduleStream.Seek(3, SeekOrigin.Current);

					ih.PitPts = moduleStream.Read_UINT8();
					ih.PitSus = moduleStream.Read_UINT8();
					ih.PitBeg = moduleStream.Read_UINT8();
					ih.PitEnd = moduleStream.Read_UINT8();
					ih.PitFlg = moduleStream.Read_UINT8();
					moduleStream.Seek(3, SeekOrigin.Current);

					ih.VolFade = moduleStream.Read_L_UINT16();
					ih.NumSmp = moduleStream.Read_L_UINT16();

					moduleStream.Read(id, 0, 4);

					// Looks like Imago Orpheus forgets the signature for empty
					// instruments following a multi-sample instrument...
					if (((id[0] != 'I') || (id[1] != 'I') || (id[2] != '1') || (id[3] != '0')) && (oldNumSmp != 0) && ((id[0] != 0) || (id[1] != 0) || (id[2] != 0) || (id[3] != 0)))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					oldNumSmp = ih.NumSmp;

					if ((ih.NumSmp > 16) || (ih.VolPts > ImfEnvCnt / 2) || (ih.PanPts > ImfEnvCnt / 2) || (ih.PitPts > ImfEnvCnt / 2) || moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					for (int u = 0; u < ImfNoteCnt; u++)
						d.SampleNumber[u] = (ushort)(ih.What[u] > ih.NumSmp ? 0xffff : ih.What[u] + of.NumSmp);

					d.VolFade = ih.VolFade;

					// Volume envelope
					for (int u = 0; u < (ImfEnvCnt >> 1); u++)
					{
						d.VolEnv[u].Pos = (short)ih.VolEnv[u << 1];
						d.VolEnv[u].Val = (short)ih.VolEnv[(u << 1) + 1];
					}

					if ((ih.VolFlg & 1) != 0)
						d.VolFlg |= EnvelopeFlag.On;

					if ((ih.VolFlg & 2) != 0)
						d.VolFlg |= EnvelopeFlag.Sustain;

					if ((ih.VolFlg & 4) != 0)
						d.VolFlg |= EnvelopeFlag.Loop;

					d.VolSusBeg = d.VolSusEnd = ih.VolSus;
					d.VolBeg = ih.VolBeg;
					d.VolEnd = ih.VolEnd;
					d.VolPts = ih.VolPts;

					if (((d.VolFlg & EnvelopeFlag.On) != 0) && (d.VolPts < 2))
						d.VolFlg &= ~EnvelopeFlag.On;

					// Panning envelope
					for (int u = 0; u < (ImfEnvCnt >> 1); u++)
					{
						d.PanEnv[u].Pos = (short)ih.PanEnv[u << 1];
						d.PanEnv[u].Val = (short)ih.PanEnv[(u << 1) + 1];
					}

					if ((ih.PanFlg & 1) != 0)
						d.PanFlg |= EnvelopeFlag.On;

					if ((ih.PanFlg & 2) != 0)
						d.PanFlg |= EnvelopeFlag.Sustain;

					if ((ih.PanFlg & 4) != 0)
						d.PanFlg |= EnvelopeFlag.Loop;

					d.PanSusBeg = d.PanSusEnd = ih.PanSus;
					d.PanBeg = ih.PanBeg;
					d.PanEnd = ih.PanEnd;
					d.PanPts = ih.PanPts;

					if (((d.PanFlg & EnvelopeFlag.On) != 0) && (d.PanPts < 2))
						d.PanFlg &= ~EnvelopeFlag.On;

					// Pitch envelope
					for (int u = 0; u < (ImfEnvCnt >> 1); u++)
					{
						d.PitEnv[u].Pos = (short)ih.PitEnv[u << 1];
						d.PitEnv[u].Val = (short)ih.PitEnv[(u << 1) + 1];
					}

					if ((ih.PitFlg & 1) != 0)
						d.PitFlg |= EnvelopeFlag.On;

					if ((ih.PitFlg & 2) != 0)
						d.PitFlg |= EnvelopeFlag.Sustain;

					if ((ih.PitFlg & 4) != 0)
						d.PitFlg |= EnvelopeFlag.Loop;

					d.PitSusBeg = d.PitSusEnd = ih.PitSus;
					d.PitBeg = ih.PitBeg;
					d.PitEnd = ih.PitEnd;
					d.PitPts = ih.PitPts;

					if (((d.PitFlg & EnvelopeFlag.On) != 0) && (d.PitPts < 2))
						d.PitFlg &= ~EnvelopeFlag.On;

					if ((ih.PitFlg & 1) != 0)
						d.PitFlg &= ~EnvelopeFlag.On;

					// Gather sample information
					for (int u = 0; u < ih.NumSmp; u++, sIndex++)
					{
						// Allocate more room for sample information if necessary
						if ((of.NumSmp + u) == wavCnt)
						{
							wavCnt += ImfSmpIncr;

							uint[] newNextWav = new uint[wavCnt];

							if (nextWav != null)
								Array.Copy(nextWav, newNextWav, wavCnt - ImfSmpIncr);

							nextWav = newNextWav;

							ImfWavHeader[] newWave = new ImfWavHeader[wavCnt];

							if (wh != null)
								Array.Copy(wh, newWave, wavCnt - ImfSmpIncr);

							wh = newWave;

							sIndex = wavCnt - ImfSmpIncr;
						}

						ImfWavHeader s = new ImfWavHeader();
						wh[sIndex] = s;

						moduleStream.ReadString(s.SampleName, 13);
						moduleStream.Seek(3, SeekOrigin.Current);

						s.Length = moduleStream.Read_L_UINT32();
						s.LoopStart = moduleStream.Read_L_UINT32();
						s.LoopEnd = moduleStream.Read_L_UINT32();
						s.SampleRate = moduleStream.Read_L_UINT32();
						s.Volume = (byte)(moduleStream.Read_UINT8() & 0x7f);
						s.Pan = moduleStream.Read_UINT8();

						moduleStream.Seek(14, SeekOrigin.Current);
						s.Flags = moduleStream.Read_UINT8();

						moduleStream.Seek(11, SeekOrigin.Current);
						moduleStream.Read(id, 0, 4);

						if ((((id[0] != 'I') || (id[1] != 'S') || (id[2] != '1') || (id[3] != '0')) && ((id[0] != 'I') || (id[1] != 'W') || (id[2] != '1') || (id[3] != '0'))) || moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
							return false;
						}

						nextWav[of.NumSmp + u] = (uint)moduleStream.Position;
						moduleStream.Seek(s.Length, SeekOrigin.Current);
					}

					of.NumSmp += ih.NumSmp;
				}

				// Sanity check
				if (of.NumSmp == 0)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				// Load samples
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

				for (int u = 0; u < of.NumSmp; u++)
				{
					Sample q = of.Samples[u];
					ImfWavHeader s = wh[u];

					q.SampleName = encoder.GetString(s.SampleName);
					q.Length = s.Length;
					q.LoopStart = s.LoopStart;
					q.LoopEnd = s.LoopEnd;
					q.Volume = s.Volume;
					q.Speed = s.SampleRate;

					if ((of.Flags & ModuleFlag.Linear) != 0)
						q.Speed = (uint)util.SpeedToFineTune(of, s.SampleRate << 1, u);

					q.Panning = s.Pan;
					q.SeekPos = nextWav[u];

					q.Flags |= SampleFlag.Signed;

					if ((s.Flags & 0x01) != 0)
						q.Flags |= SampleFlag.Loop;

					if ((s.Flags & 0x02) != 0)
						q.Flags |= SampleFlag.Bidi;

					if ((s.Flags & 0x08) != 0)
						q.Flags |= SampleFlag.OwnPan;

					if ((s.Flags & 0x04) != 0)
					{
						q.Flags |= SampleFlag._16Bits;
						q.Length >>= 1;
						q.LoopStart >>= 1;
						q.LoopEnd >>= 1;
					}
				}

				for (int u = 0; u < of.NumIns; u++)
				{
					Instrument d = of.Instruments[u];

					for (int t = 0; t < ImfNoteCnt; t++)
					{
						if (d.SampleNumber[t] >= of.NumSmp)
							d.SampleNote[t] = 255;
						else
						{
							if ((of.Flags & ModuleFlag.Linear) != 0)
							{
								int note = d.SampleNote[u] + util.noteIndex[d.SampleNumber[u]];
								d.SampleNote[u] = (byte)((note < 0) ? 0 : (note > 255 ? 255 : note));
							}
							else
								d.SampleNote[t] = (byte)t;
						}
					}
				}
			}
			finally
			{
				util.FreeLinear();
				util = null;

				imfPat = null;
				mh = null;
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
		private bool ReadPattern(ModuleStream moduleStream, int size, ushort rows)
		{
			int row = 0;
			ImfNote dummy = new ImfNote();

			// Clear pattern data
			for (int i = 0; i < 32 * 256; i++)
			{
				ImfNote n = imfPat[i];
				n.Note = n.Ins = n.Eff1 = n.Dat1 = n.Eff2 = n.Dat2 = 255;
			}

			while ((size > 0) && (row < rows))
			{
				int flag = moduleStream.Read_UINT8();
				size--;

				if (moduleStream.EndOfStream)
					return false;

				if (flag != 0)
				{
					int ch = util.remap[flag & 31];

					ImfNote n;
					if (ch != 255)
						n = imfPat[256 * ch + row];
					else
						n = dummy;

					if ((flag & 32) != 0)
					{
						n.Note = moduleStream.Read_UINT8();
						if (n.Note >= 0xa0)			// Note off
							n.Note = 0xa0;

						n.Ins = moduleStream.Read_UINT8();
						size -= 2;
					}

					if ((flag & 64) != 0)
					{
						n.Eff2 = moduleStream.Read_UINT8();
						n.Dat2 = moduleStream.Read_UINT8();
						size -= 2;
					}

					if ((flag & 128) != 0)
					{
						n.Eff1 = moduleStream.Read_UINT8();
						n.Dat1 = moduleStream.Read_UINT8();
						size -= 2;
					}
				}
				else
					row++;
			}

			if ((size != 0) && (row != rows))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert one IMF track to UniMod track
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(ImfNote[] tr, int offset, ushort rows, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < rows; t++)
			{
				byte note = tr[offset + t].Note;
				byte ins = tr[offset + t].Ins;

				if ((ins != 0) && (ins != 255))
					uniTrk.UniInstrument((ushort)(ins - 1));

				if (note != 255)
				{
					if (note == 0xa0)
					{
						uniTrk.UniPtEffect(0xc, 0, of.Flags);

						if (tr[offset + t].Eff1 == 0x0c)
							tr[offset + t].Eff1 = 0;

						if (tr[offset + t].Eff2 == 0x0c)
							tr[offset + t].Eff2 = 0;
					}
					else
						uniTrk.UniNote((ushort)(((note >> 4) * SharedConstant.Octave) + (note & 0xf)));
				}

				ProcessCmd(tr[offset + t].Eff1, tr[offset + t].Dat1, uniTrk);
				ProcessCmd(tr[offset + t].Eff2, tr[offset + t].Dat2, uniTrk);

				uniTrk.UniNewLine();
			}

			return uniTrk.UniDup();
		}



		/********************************************************************/
		/// <summary>
		/// Will process an effect and store the result in the UNI stream
		/// </summary>
		/********************************************************************/
		private void ProcessCmd(byte eff, byte inf, MUniTrk uniTrk)
		{
			if ((eff != 0) && (eff != 255))
			{
				switch (eff)
				{
					// Set tempo
					case 0x01:
					{
						uniTrk.UniEffect(Command.UniS3MEffectA, inf);
						break;
					}

					// Set BPM
					case 0x02:
					{
						if (inf >= 0x20)
							uniTrk.UniEffect(Command.UniS3MEffectT, inf);

						break;
					}

					// Tone portamento
					case 0x03:
					{
						uniTrk.UniEffect(Command.UniItEffectG, inf);
						break;
					}

					// Porta + volslide
					case 0x04:
					{
						uniTrk.UniEffect(Command.UniItEffectG, inf);
						uniTrk.UniEffect(Command.UniS3MEffectD, 0);
						break;
					}

					// Vibrato
					case 0x05:
					{
						uniTrk.UniEffect(Command.UniXmEffect4, inf);
						break;
					}

					// Vibrato + volslide
					case 0x06:
					{
						uniTrk.UniEffect(Command.UniXmEffect6, inf);
						break;
					}

					// Fine vibrato
					case 0x07:
					{
						uniTrk.UniEffect(Command.UniItEffectU, inf);
						break;
					}

					// Tremolo
					case 0x08:
					{
						uniTrk.UniEffect(Command.UniS3MEffectR, inf);
						break;
					}

					// Arpeggio
					case 0x09:
					{
						uniTrk.UniPtEffect(0x0, inf, of.Flags);
						break;
					}

					// Panning
					case 0x0a:
					{
						uniTrk.UniPtEffect(0x8, (byte)((inf >= 128) ? 255 : (inf << 1)), of.Flags);
						break;
					}

					// Pan slide
					case 0x0b:
					{
						uniTrk.UniEffect(Command.UniXmEffectP, inf);
						break;
					}

					// Set channel volume
					case 0x0c:
					{
						if (inf <= 64)
							uniTrk.UniPtEffect(0xc, inf, of.Flags);

						break;
					}

					// Volume slide
					case 0x0d:
					{
						uniTrk.UniEffect(Command.UniS3MEffectD, inf);
						break;
					}

					// Fine volume slide
					case 0x0e:
					{
						if (inf != 0)
						{
							if ((inf >> 4) != 0)
								uniTrk.UniEffect(Command.UniS3MEffectD, (ushort)(0x0f | inf));
							else
								uniTrk.UniEffect(Command.UniS3MEffectD, (ushort)(0xf0 | inf));
						}
						else
							uniTrk.UniEffect(Command.UniS3MEffectD, 0);

						break;
					}

					// Set fine tune
					case 0x0f:
					{
						uniTrk.UniPtEffect(0xe, (byte)(0x50 | (inf >> 4)), of.Flags);
						break;
					}

					// Note slide up/down
					case 0x10:
					case 0x11:
						break;

					// Slide up
					case 0x12:
					{
						uniTrk.UniEffect(Command.UniS3MEffectF, inf);
						break;
					}

					// Slide down
					case 0x13:
					{
						uniTrk.UniEffect(Command.UniS3MEffectE, inf);
						break;
					}

					// Fine slide up
					case 0x14:
					{
						if (inf != 0)
						{
							if (inf < 0x40)
								uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xe0 | (inf >> 2)));
							else
								uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xf0 | (inf >> 4)));
						}
						else
							uniTrk.UniEffect(Command.UniS3MEffectF, 0);

						break;
					}

					// Fine slide down
					case 0x15:
					{
						if (inf != 0)
						{
							if (inf < 0x40)
								uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xe0 | (inf >> 2)));
							else
								uniTrk.UniEffect(Command.UniS3MEffectE, (ushort)(0xf0 | (inf >> 4)));
						}
						else
							uniTrk.UniEffect(Command.UniS3MEffectE, 0);

						break;
					}

					// Set filter cutoff (awe32)
					// Filter side + resonance (awe32)
					case 0x16:
					case 0x17:
						break;

					// Sample offset
					case 0x18:
					{
						uniTrk.UniPtEffect(0x9, inf, of.Flags);
						break;
					}

					// Set fine sample offset
					case 0x19:
						break;

					// Keyoff
					case 0x1a:
					{
						uniTrk.UniWriteByte((byte)Command.UniKeyOff);
						break;
					}

					// Retrig
					case 0x1b:
					{
						uniTrk.UniEffect(Command.UniS3MEffectQ, inf);
						break;
					}

					// Tremor
					case 0x1c:
					{
						uniTrk.UniEffect(Command.UniS3MEffectI, inf);
						break;
					}

					// Position jump
					case 0x1d:
					{
						uniTrk.UniPtEffect(0xb, inf, of.Flags);
						break;
					}

					// Pattern break
					case 0x1e:
					{
						uniTrk.UniPtEffect(0xd, (byte)((inf >> 4) * 10 + (inf & 0xf)), of.Flags);
						break;
					}

					// Set master volume
					case 0x1f:
					{
						if (inf <= 64)
							uniTrk.UniEffect(Command.UniXmEffectG, (byte)(inf << 1));

						break;
					}

					// Master volume slide
					case 0x20:
					{
						uniTrk.UniEffect(Command.UniXmEffectH, inf);
						break;
					}

					// Extended effects
					case 0x21:
					{
						switch (inf >> 4)
						{
							// Set filter
							// Vibrato waveform
							// Tremolo waveform
							case 0x1:
							case 0x5:
							case 0x8:
							{
								uniTrk.UniPtEffect(0xe, (byte)(inf - 0x10), of.Flags);
								break;
							}

							// Pattern loop
							case 0xa:
							{
								uniTrk.UniPtEffect(0xe, (byte)(0x60 | (inf & 0xf)), of.Flags);
								break;
							}

							// Pattern delay
							case 0xb:
							{
								uniTrk.UniPtEffect(0xe, (byte)(0xe0 | (inf & 0xf)), of.Flags);
								break;
							}

							// Glissando
							// Note cut
							// Note delay
							// Invert loop
							case 0x3:
							case 0xc:
							case 0xd:
							case 0xf:
							{
								uniTrk.UniPtEffect(0xe, inf, of.Flags);
								break;
							}

							// Ignore envelope
							case 0xe:
							{
								uniTrk.UniEffect(Command.UniItEffectS0, 0x77);		// Vol
								uniTrk.UniEffect(Command.UniItEffectS0, 0x79);		// Pan
								uniTrk.UniEffect(Command.UniItEffectS0, 0x7b);		// Pit
								break;
							}
						}
						break;
					}

					// Chorus (awe32)
					// Reverb (awe32)
					case 0x22:
					case 0x23:
						break;
				}
			}
		}
		#endregion
	}
}

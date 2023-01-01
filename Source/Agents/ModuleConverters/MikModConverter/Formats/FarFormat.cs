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

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for FAR (Farandole) format
	/// </summary>
	internal class FarFormat : MikModConverterWorkerBase
	{
		#region FarHeader1 class
		private class FarHeader1
		{
			public readonly byte[] Id = new byte[4];					// File magic
			public readonly byte[] SongName = new byte[41];				// Song name
			public readonly byte[] Blah = new byte[3];					// 13, 10, 26
			public ushort HeaderLen;									// Remaining length of header in bytes
			public byte Version;
			public readonly byte[] OnOff = new byte[16];
			public readonly byte[] Edit1 = new byte[9];
			public byte Speed;
			public readonly byte[] Panning = new byte[16];
			public readonly byte[] Edit2 = new byte[4];
			public ushort StLen;
		}
		#endregion

		#region FarHeader2 class
		private class FarHeader2
		{
			public readonly byte[] Orders = new byte[256];
			public byte NumPat;
			public byte SngLen;
			public byte LoopTo;
			public readonly ushort[] PatSiz = new ushort[256];
		}
		#endregion

		#region FarSample class
		private class FarSample
		{
			public readonly byte[] SampleName = new byte[33];
			public uint Length;
			public byte FineTune;
			public byte Volume;
			public uint RepPos;
			public uint RepEnd;
			public byte Type;
			public byte Loop;
		}
		#endregion

		#region FarNote struct
		private struct FarNote
		{
			public byte Note;
			public byte Ins;
			public byte Vol;
			public byte Eff;
		}
		#endregion

		private const ushort MaxPatSize = (256 * 16 * 4) + 2;

		private FarHeader1 mh1;
		private FarHeader2 mh2;
		private FarNote[] pat;

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
			if (fileSize < 98)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] buf = new byte[47];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 47);

			if ((buf[0] == 'F') && (buf[1] == 'A') && (buf[2] == 'R') && (buf[3] == 0xfe) && (buf[44] == 13) && (buf[45] == 10) && (buf[46] == 26))
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

			mh1 = new FarHeader1();
			mh2 = new FarHeader2();
			pat = new FarNote[256 * 16 * 4];

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				// Try to read the header (first part)
				moduleStream.Read(mh1.Id, 0, 4);
				moduleStream.ReadString(mh1.SongName, 40);
				moduleStream.Read(mh1.Blah, 0, 3);

				mh1.HeaderLen = moduleStream.Read_L_UINT16();
				mh1.Version = moduleStream.Read_UINT8();

				moduleStream.Read(mh1.OnOff, 0, 16);
				moduleStream.Read(mh1.Edit1, 0, 9);

				mh1.Speed = moduleStream.Read_UINT8();

				moduleStream.Read(mh1.Panning, 0, 16);
				moduleStream.Read(mh1.Edit2, 0, 4);

				mh1.StLen = moduleStream.Read_L_UINT16();

				// Init modfile data
				of.SongName = encoder.GetString(mh1.SongName);
				of.NumChn = 16;
				of.InitSpeed = mh1.Speed != 0 ? mh1.Speed : (byte)4;
				of.BpmLimit = 5;
				of.Flags |= ModuleFlag.Panning | ModuleFlag.FarTempo | ModuleFlag.HighBpm;

				for (int t = 0; t < 16; t++)
					of.Panning[t] = (ushort)(mh1.Panning[t] << 4);

				// Read song text into comment field
				if (mh1.StLen != 0)
					of.Comment = string.Join('\n', moduleStream.ReadCommentBlock(mh1.StLen, 132, encoder));

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Try to read module header (second part)
				moduleStream.Read(mh2.Orders, 0, 256);

				mh2.NumPat = moduleStream.Read_UINT8();
				mh2.SngLen = moduleStream.Read_UINT8();
				mh2.LoopTo = moduleStream.Read_UINT8();

				moduleStream.ReadArray_L_UINT16s(mh2.PatSiz, 0, 256);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				of.NumPos = mh2.SngLen;
				of.RepPos = mh2.LoopTo;

				if (!MLoader.AllocPositions(of, of.NumPos))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Count number of patterns stored in file
				of.NumPat = 0;

				for (int t = 0; t < 256; t++)
				{
					if (mh2.PatSiz[t] != 0)
					{
						if ((t + 1) > of.NumPat)
							of.NumPat = (ushort)(t + 1);
					}
				}

				bool addExtraPattern = false;
				for (int t = 0; t < of.NumPos; t++)
				{
					if (mh2.Orders[t] == 0xff)
						break;

					of.Positions[t] = mh2.Orders[t];

					if (of.Positions[t] >= of.NumPat)
					{
						of.Positions[t] = of.NumPat;
						addExtraPattern = true;
					}
				}

				if (addExtraPattern)
					of.NumPat++;

				of.NumTrk = (ushort)(of.NumPat * of.NumChn);

				// Seek across eventual new data
				moduleStream.Seek(mh1.HeaderLen - (869 + mh1.StLen), SeekOrigin.Current);

				// Alloc track and pattern structures
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

				int tracks = 0;

				for (int t = 0; t < of.NumPat; t++)
				{
					Array.Clear(pat, 0, 256 * 16 * 4);

					if (mh2.PatSiz[t] != 0)
					{
						// Break position byte is always 1 less than the final row index,
						// i.e. it is 2 less than the total row count
						ushort rows = (ushort)(moduleStream.Read_UINT8() + 2);
						moduleStream.Read_UINT8();		// Tempo

						int cRowIndex = 0;

						// File often allocates 64 rows even if there are less in pattern.
						// Also, don't allow more than 256 rows
						if ((mh2.PatSiz[t] < 2 + (rows * 16 * 4)) || (rows > 256) || (mh2.PatSiz[t] > MaxPatSize))
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
							return false;
						}

						for (int u = (mh2.PatSiz[t] - 2) / 4; u != 0; u--, cRowIndex++)
						{
							ref FarNote cRow = ref pat[cRowIndex];

							cRow.Note = moduleStream.Read_UINT8();
							cRow.Ins = moduleStream.Read_UINT8();
							cRow.Vol = moduleStream.Read_UINT8();
							cRow.Eff = moduleStream.Read_UINT8();
						}

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
							return false;
						}

						cRowIndex = 0;
						of.PattRows[t] = rows;

						for (int u = 16; u != 0; u--, cRowIndex++)
						{
							if ((of.Tracks[tracks++] = ConvertTrack(pat, cRowIndex, rows, uniTrk)) == null)
							{
								errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
								return false;
							}
						}
					}
					else
					{
						// Faradole Composer normally use a 64 rows blank track for patterns with 0 rows
						for (int u = 0; u < 16; u++)
						{
							uniTrk.UniReset();

							for (int r = 0; r < 64; r++)
								uniTrk.UniNewLine();

							of.Tracks[tracks++] = uniTrk.UniDup();
						}

						of.PattRows[t] = 64;
					}
				}

				// Read sample map
				byte[] sMap = new byte[8];
				moduleStream.Read(sMap, 0, 8);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				// Count number of samples used
				of.NumIns = 0;

				for (int t = 0; t < 64; t++)
				{
					if ((sMap[t >> 3] & (1 << (t & 7))) != 0)
						of.NumIns = (ushort)(t + 1);
				}

				of.NumSmp = of.NumIns;

				// Alloc sample structs
				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				for (int t = 0; t < of.NumSmp; t++)
				{
					FarSample s = new FarSample();
					Sample q = of.Samples[t];

					q.Speed = 8363;
					q.Flags = SampleFlag.Signed;

					if ((sMap[t >> 3] & (1 << (t & 7))) != 0)
					{
						moduleStream.ReadString(s.SampleName, 32);

						s.Length = moduleStream.Read_L_UINT32();
						s.FineTune = moduleStream.Read_UINT8();
						s.Volume = moduleStream.Read_UINT8();
						s.RepPos = moduleStream.Read_L_UINT32();
						s.RepEnd = moduleStream.Read_L_UINT32();
						s.Type = moduleStream.Read_UINT8();
						s.Loop = moduleStream.Read_UINT8();

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
							return false;
						}

						q.SampleName = encoder.GetString(s.SampleName);
						q.Length = s.Length;
						q.LoopStart = s.RepPos;
						q.LoopEnd = s.RepEnd;
						q.Volume = (byte)(s.Volume << 2);

						if ((s.Type & 1) != 0)
						{
							q.Flags |= SampleFlag._16Bits;
							q.Length >>= 1;
							q.LoopStart >>= 1;
							q.LoopEnd >>= 1;
						}

						if ((s.Loop & 8) != 0)
							q.Flags |= SampleFlag.Loop;

						q.SeekPos = (uint)moduleStream.Position;
						moduleStream.Seek(s.Length, SeekOrigin.Current);
					}
					else
						q.SampleName = string.Empty;
				}
			}
			finally
			{
				mh1 = null;
				mh2 = null;
				pat = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert a track to uni format
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(FarNote[] tr, int offset, int rows, MUniTrk uniTrk)
		{
			int vibDepth = 1;

			uniTrk.UniReset();

			for (int t = 0; t < rows; t++)
			{
				ref FarNote n = ref tr[offset];

				if (n.Note != 0)
				{
					uniTrk.UniInstrument(n.Ins);
					uniTrk.UniNote((ushort)(n.Note + 3 * SharedConstant.Octave - 1));
				}

				if ((n.Vol >= 0x01) && (n.Vol <= 0x10))
					uniTrk.UniPtEffect(0xc, (byte)((n.Vol - 1) << 2), of.Flags);

				if (n.Eff != 0)
				{
					switch (n.Eff >> 4)
					{
						// Global effects
						case 0x0:
						{
							switch (n.Eff & 0xf)
							{
								// Fulfill loop
								case 0x3:
								{
									uniTrk.UniEffect(Command.UniKeyFade, 0);
									break;
								}

								// Old tempo mode
								case 0x4:
									break;

								// New tempo mode
								case 0x5:
									break;
							}
							break;
						}

						// Pitch adjust up
						case 0x1:
						{
							uniTrk.UniEffect(Command.UniFarEffect1, (ushort)(n.Eff & 0xf));
							break;
						}

						// Pitch adjust down
						case 0x2:
						{
							uniTrk.UniEffect(Command.UniFarEffect2, (ushort)(n.Eff & 0xf));
							break;
						}

						// Portamento
						case 0x3:
						{
							uniTrk.UniEffect(Command.UniFarEffect3, (ushort)(n.Eff & 0xf));
							break;
						}

						// Retrigger
						case 0x4:
						{
							uniTrk.UniEffect(Command.UniFarEffect4, (ushort)(n.Eff & 0xf));
							break;
						}

						// Set vibrato depth
						case 0x5:
						{
							vibDepth = n.Eff & 0xf;
							break;
						}

						// Vibrato
						case 0x6:
						{
							uniTrk.UniEffect(Command.UniFarEffect6, (ushort)(((n.Eff & 0xf) << 4) | vibDepth));
							break;
						}

						// Volume slide up
						case 0x7:
						{
							uniTrk.UniPtEffect(0xa, (byte)((n.Eff & 0xf) << 4), of.Flags);
							break;
						}

						// Volume slide down
						case 0x8:
						{
							uniTrk.UniPtEffect(0xa, (byte)(n.Eff & 0xf), of.Flags);
							break;
						}

						// Sustained vibrato
						case 0x9:
							break;

						// Port to vol
						case 0xa:
							break;

						// Set panning
						case 0xb:
						{
							uniTrk.UniPtEffect(0xe, (byte)(0x80 | (n.Eff & 0xf)), of.Flags);
							break;
						}

						// Note offset
						case 0xc:
							break;

						// Fine tempo down
						case 0xd:
						{
							uniTrk.UniEffect(Command.UniFarEffectD, (ushort)(n.Eff & 0xf));
							break;
						}

						// Fine tempo up
						case 0xe:
						{
							uniTrk.UniEffect(Command.UniFarEffectE, (ushort)(n.Eff & 0xf));
							break;
						}

						// Set speed
						case 0xf:
						{
							uniTrk.UniEffect(Command.UniFarEffectF, (ushort)(n.Eff & 0xf));
							break;
						}
					}
				}

				uniTrk.UniNewLine();
				offset += 16;
			}

			return uniTrk.UniDup();
		}
		#endregion
	}
}

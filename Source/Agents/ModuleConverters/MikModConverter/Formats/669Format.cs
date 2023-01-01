/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
	/// MikMod loader for 669 format
	/// </summary>
	internal class _669Format : MikModConverterWorkerBase
	{
		#region S69Header class
		private class S69Header
		{
			public readonly byte[] Marker = new byte[2];
			public readonly byte[] Message = new byte[109];
			public byte Nos;
			public byte Nop;
			public byte LoopOrder;
			public readonly byte[] Orders = new byte[0x80];
			public readonly byte[] Tempos = new byte[0x80];
			public readonly byte[] Breaks = new byte[0x80];
		}
		#endregion

		#region S69Sample class
		/// <summary>
		/// Sample information
		/// </summary>
		private class S69Sample
		{
			public readonly byte[] FileName = new byte[14];
			public int Length;
			public int LoopBeg;
			public int LoopEnd;
		}
		#endregion

		#region S69Note class
		/// <summary>
		/// Encoded note
		/// </summary>
		private class S69Note
		{
			public byte A;
			public byte B;
			public byte C;
		}
		#endregion

		private readonly byte checkId1;
		private readonly byte checkId2;

		// Current pattern
		private S69Note[] s69Pat;

		// Module header
		private S69Header mh;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public _669Format(byte id1, byte id2)
		{
			checkId1 = id1;
			checkId2 = id2;
		}

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
			if (fileSize < 497)								// Size of header
				return AgentResult.Unknown;

			byte[] buf = new byte[0x80];

			// Look for ID
			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 2);

			if ((buf[0] == checkId1) && (buf[1] == checkId2))
			{
				// Skip the song message
				moduleStream.Seek(108, SeekOrigin.Current);

				// Sanity check
				if (moduleStream.Read_UINT8() > 64)
					return AgentResult.Unknown;

				if (moduleStream.Read_UINT8() > 128)
					return AgentResult.Unknown;

				if (moduleStream.Read_UINT8() > 127)
					return AgentResult.Unknown;

				// Check order table
				moduleStream.Read(buf, 0, 0x80);

				if (moduleStream.EndOfStream)
					return AgentResult.Unknown;

				for (int i = 0; i < 0x80; i++)
				{
					if ((buf[i] >= 0x80) && (buf[i] != 0xff))
						return AgentResult.Unknown;
				}

				// Check tempos table
				moduleStream.Read(buf, 0, 0x80);

				if (moduleStream.EndOfStream)
					return AgentResult.Unknown;

				for (int i = 0; i < 0x80; i++)
				{
					if ((buf[i] == 0x00) || (buf[i] > 32))
						return AgentResult.Unknown;
				}

				// Check pattern length table
				moduleStream.Read(buf, 0, 0x80);

				if (moduleStream.EndOfStream)
					return AgentResult.Unknown;

				for (int i = 0; i < 0x80; i++)
				{
					if (buf[i] > 0x3f)
						return AgentResult.Unknown;
				}
			}
			else
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

			// Start to allocate some buffers we need
			s69Pat = Helpers.InitializeArray<S69Note>(64 * 8);
			mh = new S69Header();

			try
			{
				int i;

				Encoding encoder = EncoderCollection.Dos;

				// Read the module header
				moduleStream.Read(mh.Marker, 0, 2);
				moduleStream.Read(mh.Message, 0, 108);

				mh.Nos = moduleStream.Read_UINT8();
				mh.Nop = moduleStream.Read_UINT8();
				mh.LoopOrder = moduleStream.Read_UINT8();

				moduleStream.Read(mh.Orders, 0, 0x80);
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (i = 0; i < 0x80; i++)
				{
					if ((mh.Orders[i] >= 0x80) && (mh.Orders[i] != 0xff))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
				}

				moduleStream.Read(mh.Tempos, 0, 0x80);
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (i = 0; i < 0x80; i++)
				{
					if ((mh.Tempos[i] == 0x00) || (mh.Tempos[i] > 32))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
				}

				moduleStream.Read(mh.Breaks, 0, 0x80);
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (i = 0; i < 0x80; i++)
				{
					if (mh.Breaks[i] > 0x3f)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
				}

				// Set module variables
				of.InitSpeed = 4;
				of.InitTempo = 78;
				of.SongName = encoder.GetString(mh.Message, 0, 36);
				of.NumChn = 8;
				of.NumPat = mh.Nop;
				of.NumIns = of.NumSmp = mh.Nos;
				of.NumTrk = (ushort)(of.NumChn * of.NumPat);
				of.Flags = ModuleFlag.XmPeriods | ModuleFlag.Linear;

				// Split the message into 3 lines
				for (i = 35; (i >= 0) && (mh.Message[i] == ' '); i--)
					mh.Message[i] = 0;

				for (i = 36 + 35; (i >= 36 + 0) && (mh.Message[i] == ' '); i--)
					mh.Message[i] = 0;

				for (i = 72 + 35; (i >= 72 + 0) && (mh.Message[i] == ' '); i--)
					mh.Message[i] = 0;

				if ((mh.Message[0] != 0) || (mh.Message[36] != 0) || (mh.Message[72] != 0))
				{
					string[] comment = new string[]
					{
						encoder.GetString(mh.Message, 0, 36),
						encoder.GetString(mh.Message, 36, 36),
						encoder.GetString(mh.Message, 72, 36)
					};

					of.Comment = string.Join('\n', comment);
				}

				if (!MLoader.AllocPositions(of, 0x80))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (i = 0; i < 0x80; i++)
				{
					if (mh.Orders[i] >= mh.Nop)
						break;

					of.Positions[i] = mh.Orders[i];
				}

				of.NumPos = (ushort)i;
				of.RepPos = (ushort)(mh.LoopOrder < of.NumPos ? mh.LoopOrder : 0);

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				for (i = 0; i < of.NumIns; i++)
				{
					Sample current = of.Samples[i];

					// Sample information
					S69Sample sample = new S69Sample();

					moduleStream.ReadString(sample.FileName, 13);

					sample.Length = (int)moduleStream.Read_L_UINT32();
					sample.LoopBeg = (int)moduleStream.Read_L_UINT32();
					sample.LoopEnd = (int)moduleStream.Read_L_UINT32();

					// Note: 'Lost in Germany' has 0xf0ffff as marker
					if (sample.LoopEnd >= 0xfffff)
						sample.LoopEnd = 0;

					if ((sample.Length < 0) || (sample.LoopBeg < -1) || (sample.LoopEnd < -1))
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					current.SampleName = encoder.GetString(sample.FileName);
					current.SeekPos = 0;
					current.Speed = 128;		// Used as fine tune when UF_XMPERIODS is enabled, 128 is centered
					current.Length = (uint)sample.Length;
					current.LoopStart = (uint)sample.LoopBeg;
					current.LoopEnd = (uint)sample.LoopEnd;
					current.Flags = (sample.LoopBeg < sample.LoopEnd) ? SampleFlag.Loop : SampleFlag.None;
					current.Volume = 64;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
						return false;
					}
				}

				if (!LoadPatterns(moduleStream, uniTrk))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}
			}
			finally
			{
				mh = null;
				s69Pat = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Wll load all the patterns
		/// </summary>
		/********************************************************************/
		private bool LoadPatterns(ModuleStream moduleStream, MUniTrk uniTrk)
		{
			if (!MLoader.AllocPatterns(of))
				return false;

			if (!MLoader.AllocTracks(of))
				return false;

			int tracks = 0;
			for (int track = 0; track < of.NumPat; track++)
			{
				// Set pattern break locations
				of.PattRows[track] = (ushort)(mh.Breaks[track] + 1);

				// Load the 669 pattern
				int cur = 0;
				for (int row = 0; row < 64; row++)
				{
					for (int channel = 0; channel < 8; channel++, cur++)
					{
						s69Pat[cur].A = moduleStream.Read_UINT8();
						s69Pat[cur].B = moduleStream.Read_UINT8();
						s69Pat[cur].C = moduleStream.Read_UINT8();
					}
				}

				if (moduleStream.EndOfStream)
					return false;

				// Translate the pattern
				for (int channel = 0; channel < 8; channel++)
				{
					uniTrk.UniReset();

					// Set the pattern tempo
					uniTrk.UniPtEffect(0xf, 78, of.Flags);
					uniTrk.UniPtEffect(0xf, mh.Tempos[track], of.Flags);

					byte lastFx = 0xff;
					byte lastVal = 0;

					for (int row = 0; row <= mh.Breaks[track]; row++)
					{
						// Fetch the encoded note
						int a = s69Pat[(row * 8) + channel].A;
						int b = s69Pat[(row * 8) + channel].B;
						int c = s69Pat[(row * 8) + channel].C;

						// Decode it
						byte note = (byte)(a >> 2);
						byte inst = (byte)(((a & 0x3) << 4) | ((b & 0xf0) >> 4));
						byte vol = (byte)(b & 0xf);

						if (a < 0xff)
						{
							if (a < 0xfe)
							{
								uniTrk.UniInstrument(inst);
								uniTrk.UniNote((ushort)(note + 2 * SharedConstant.Octave));

								lastFx = 0xff;				// Reset background effect memory
							}

							uniTrk.UniPtEffect(0xc, (byte)(vol << 2), of.Flags);
						}

						if ((c != 0xff) || (lastFx != 0xff))
						{
							byte effect;

							if (c == 0xff)
							{
								c = lastFx;
								effect = lastVal;
							}
							else
								effect = (byte)(c & 0xf);

							switch (c >> 4)
							{
								// Portamento up
								case 0:
								{
									uniTrk.UniPtEffect(0x1, effect, of.Flags);

									lastFx = (byte)c;
									lastVal = effect;
									break;
								}

								// Portamento down
								case 1:
								{
									uniTrk.UniPtEffect(0x2, effect, of.Flags);

									lastFx = (byte)c;
									lastVal = effect;
									break;
								}

								// Portamento to note
								case 2:
								{
									uniTrk.UniPtEffect(0x3, effect, of.Flags);

									lastFx = (byte)c;
									lastVal = effect;
									break;
								}

								// Frequency adjust
								case 3:
								{
									// DMP converts this effect to S3M FFx. Why not?
									uniTrk.UniEffect(Command.UniS3MEffectF, (ushort)(0xf0 | effect));
									break;
								}

								// Vibrato
								case 4:
								{
									uniTrk.UniPtEffect(0x4, effect, of.Flags);

									lastFx = (byte)c;
									lastVal = effect;
									break;
								}

								// Set tempo
								case 5:
								{
									if (effect != 0)
										uniTrk.UniPtEffect(0xf, effect, of.Flags);
									else
									{
										if (mh.Marker[0] != 'i')
										{
											// Super fast tempo not supported
										}
									}
									break;
								}
							}
						}

						uniTrk.UniNewLine();
					}

					if ((of.Tracks[tracks++] = uniTrk.UniDup()) == null)
						return false;
				}
			}

			return true;
		}
		#endregion
	}
}

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

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for ULT (UltraTracker) format
	/// </summary>
	internal class UltFormat : MikModConverterWorkerBase
	{
		#region UltHeader class
		private class UltHeader
		{
			public readonly byte[] Id = new byte[16];
			public readonly byte[] SongTitle = new byte[33];
			public byte Reserved;
		}
		#endregion

		#region UltSample class
		private class UltSample
		{
			public readonly byte[] SampleName = new byte[33];
			public readonly byte[] DosName = new byte[13];
			public int LoopStart;
			public int LoopEnd;
			public int SizeStart;
			public int SizeEnd;
			public byte Volume;
			public byte Flags;
			public ushort Speed;
			public short FineTune;
		}
		#endregion

		#region UltEvent class
		private class UltEvent
		{
			public byte Note;
			public byte Sample;
			public byte Eff;
			public byte Dat1;
			public byte Dat2;
		}
		#endregion

		[Flags]
		private enum Flags
		{
			_16Bits = 4,
			Loop = 8,
			Reverse = 16
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
			if (fileSize < 49)								// Size of header
				return AgentResult.Unknown;

			// Now check the signature
			byte[] buf = new byte[16];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 15);

			if (Encoding.ASCII.GetString(buf, 0, 14) != "MAS_UTrack_V00")
				return AgentResult.Unknown;

			if ((buf[14] < '1') || (buf[15] > '4'))
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
			int t;

			errorMessage = string.Empty;

			UltHeader mh = new UltHeader();

			Encoding encoder = EncoderCollection.Dos;

			// Try to read the module header
			moduleStream.ReadString(mh.Id, 15);
			moduleStream.ReadString(mh.SongTitle, 32);

			mh.Reserved = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
				return false;
			}

			// Initialize some of the structure with default values
			originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_ULT, (char)('3' + (mh.Id[14] - '1')));

			of.InitSpeed = 6;
			of.InitTempo = 125;
			of.RepPos = 0;

			// Read song text
			if ((mh.Id[14] > '1') && (mh.Reserved != 0))
				of.Comment = string.Join('\n', moduleStream.ReadCommentBlock(mh.Reserved * 32, 32, encoder));

			byte nos = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
				return false;
			}

			of.SongName = encoder.GetString(mh.SongTitle).TrimEnd();
			of.NumIns = of.NumSmp = nos;

			if (!MLoader.AllocSamples(of))
			{
				errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
				return false;
			}

			for (t = 0; t < nos; t++)
			{
				UltSample s = new UltSample();
				Sample q = of.Samples[t];

				// Try to read sample info
				moduleStream.ReadString(s.SampleName, 32);
				moduleStream.ReadString(s.DosName, 12);

				s.LoopStart = (int)moduleStream.Read_L_UINT32();
				s.LoopEnd = (int)moduleStream.Read_L_UINT32();
				s.SizeStart = (int)moduleStream.Read_L_UINT32();
				s.SizeEnd = (int)moduleStream.Read_L_UINT32();
				s.Volume = moduleStream.Read_UINT8();
				s.Flags = moduleStream.Read_UINT8();
				s.Speed = (mh.Id[14] >= '4') ? moduleStream.Read_L_UINT16() : (ushort)8363;
				s.FineTune = (short)moduleStream.Read_L_UINT16();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				q.SampleName = encoder.GetString(s.SampleName).TrimEnd();

				// The correct formula would be
				// s.speed * pow(2, (double)s.finetune / (OCTAVE * 32768))
				// but to avoid libm, we'll use a first order approximation
				// 1/567290 == Ln(2)/OCTAVE/32768
				if (s.FineTune == 0)
					q.Speed = s.Speed;
				else
					q.Speed = (uint)(s.Speed * ((double)s.FineTune / 567290.0f + 1.0f));

				q.Length = (uint)(s.SizeEnd - s.SizeStart);
				q.Volume = (byte)(s.Volume >> 2);
				q.LoopStart = (uint)s.LoopStart;
				q.LoopEnd = (uint)s.LoopEnd;
				q.Flags |= SampleFlag.Signed;

				if (((Flags)s.Flags & Flags.Loop) != 0)
					q.Flags |= SampleFlag.Loop;
				else
					q.LoopStart = q.LoopEnd = 0;

				if (((Flags)s.Flags & Flags._16Bits) != 0)
				{
					s.SizeEnd += (s.SizeEnd - s.SizeStart);
					s.SizeStart <<= 1;
					q.Flags |= SampleFlag._16Bits;
					q.LoopStart >>= 1;
					q.LoopEnd >>= 1;
				}
			}

			if (!MLoader.AllocPositions(of, 256))
			{
				errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
				return false;
			}

			for (t = 0; t < 256; t++)
				of.Positions[t] = moduleStream.Read_UINT8();

			byte noc = moduleStream.Read_UINT8();
			byte nop = moduleStream.Read_UINT8();

			of.NumChn = ++noc;
			of.NumPat = ++nop;
			of.NumTrk = (ushort)(of.NumChn * of.NumPat);

			for (t = 0; t < 256; t++)
			{
				if (of.Positions[t] == 255)
				{
					of.Positions[t] = SharedConstant.Last_Pattern;
					break;
				}

				if (of.Positions[t] > of.NumPat)		// Sanity check
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
					return false;
				}
			}

			of.NumPos = (ushort)t;

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

			int tracks = 0;

			for (int u = 0; u < of.NumChn; u++)
			{
				for (t = 0; t < of.NumPat; t++)
					of.Patterns[(t * of.NumChn) + u] = (ushort)tracks++;
			}

			// Secunia SA37775 / CVE-2009-3996
			if (of.NumChn >= SharedConstant.UF_MaxChan)
				of.NumChn = SharedConstant.UF_MaxChan - 1;

			// Read pan position table for v1.5 and higher
			if (mh.Id[14] >= '3')
			{
				for (t = 0; t < of.NumChn; t++)
					of.Panning[t] = (ushort)(moduleStream.Read_UINT8() << 4);

				of.Flags |= ModuleFlag.Panning;
			}

			for (t = 0; t < of.NumTrk; t++)
			{
				int row = 0;
				bool continuePortaToNote = false;

				uniTrk.UniReset();

				while (row < 64)
				{
					int rep = ReadUltEvent(moduleStream, out UltEvent ev);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
						return false;
					}

					while (rep-- != 0)
					{
						if (ev.Sample != 0)
							uniTrk.UniInstrument((ushort)(ev.Sample - 1));

						if (ev.Note != 0)
						{
							uniTrk.UniNote((ushort)(ev.Note + 2 * SharedConstant.Octave - 1));
							continuePortaToNote = false;
						}

						// First effect - various fixes by Alexander Kerkhove and Thomas Neumann
						byte eff = (byte)(ev.Eff >> 4);

						if (continuePortaToNote && (eff != 0x3) && ((ev.Eff & 0xf) != 0x3))
							uniTrk.UniEffect(Command.UniItEffectG, 0);

						switch (eff)
						{
							// Tone portamento
							case 0x3:
							{
								uniTrk.UniEffect(Command.UniItEffectG, ev.Dat2);
								continuePortaToNote = true;
								break;
							}

							// Special
							case 0x5:
								break;              // Not supported yet!

							// Sample offset
							case 0x9:
							{
								int offset = (ev.Dat2 << 8) | ((ev.Eff & 0xf) == 9 ? ev.Dat1 : 0);
								uniTrk.UniEffect(Command.UniUltEffect9, (ushort)offset);
								break;
							}

							// Set panning
							case 0xb:
							{
								uniTrk.UniPtEffect(8, (byte)(ev.Dat2 * 0xf), of.Flags);
								of.Flags |= ModuleFlag.Panning;
								break;
							}

							// Set volume
							case 0xc:
							{
								uniTrk.UniPtEffect(eff, (byte)(ev.Dat2 >> 2), of.Flags);
								break;
							}

							default:
							{
								uniTrk.UniPtEffect(eff, ev.Dat2, of.Flags);
								break;
							}
						}

						// Second effect
						eff = (byte)(ev.Eff & 0xf);

						switch (eff)
						{
							// Tone portamento
							case 0x3:
							{
								uniTrk.UniEffect(Command.UniItEffectG, ev.Dat1);
								continuePortaToNote = true;
								break;
							}

							// Special
							case 0x5:
								break;              // Not supported yet!

							// Sample offset
							case 0x9:
							{
								if ((ev.Eff >> 4) != 9)
									uniTrk.UniEffect(Command.UniUltEffect9, (ushort)(ev.Dat1 << 8));

								break;
							}

							// Set panning
							case 0xb:
							{
								uniTrk.UniPtEffect(8, (byte)(ev.Dat1 * 0xf), of.Flags);
								of.Flags |= ModuleFlag.Panning;
								break;
							}

							// Set volume
							case 0xc:
							{
								uniTrk.UniPtEffect(eff, (byte)(ev.Dat1 >> 2), of.Flags);
								break;
							}

							default:
							{
								uniTrk.UniPtEffect(eff, ev.Dat1, of.Flags);
								break;
							}
						}

						uniTrk.UniNewLine();
						row++;
					}
				}

				if ((of.Tracks[t] = uniTrk.UniDup()) == null)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_TRACKS;
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will read one event
		/// </summary>
		/********************************************************************/
		private int ReadUltEvent(ModuleStream moduleStream, out UltEvent @event)
		{
			@event = new UltEvent();

			byte rep = 1;
			byte flag = moduleStream.Read_UINT8();

			if (flag == 0xfc)
			{
				rep = moduleStream.Read_UINT8();
				@event.Note = moduleStream.Read_UINT8();
			}
			else
				@event.Note = flag;

			@event.Sample = moduleStream.Read_UINT8();
			@event.Eff = moduleStream.Read_UINT8();
			@event.Dat1 = moduleStream.Read_UINT8();
			@event.Dat2 = moduleStream.Read_UINT8();

			return rep;
		}
		#endregion
	}
}

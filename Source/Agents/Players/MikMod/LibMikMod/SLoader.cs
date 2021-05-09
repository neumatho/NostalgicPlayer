/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.LibMikMod
{
	/// <summary>
	/// Sample loader
	/// </summary>
	internal static class SLoader
	{
		/********************************************************************/
		/// <summary>
		/// Will load the given sample and make sure it is signed and unpacked
		/// </summary>
		/********************************************************************/
		public static bool Load(ModuleStream moduleStream, int sampleNumber, sbyte[] buffer, SampleFlag inFmt, uint length, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Find the stream with the sample data and use that
			using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(sampleNumber, (int)length))
			{
				short sl_old = 0;
				ushort[] sl_buffer16 = (inFmt & SampleFlag._16Bits) != 0 ? new ushort[Constant.SlBufSize] : null;
				byte[] sl_buffer8 = (inFmt & SampleFlag._16Bits) == 0 ? new byte[Constant.SlBufSize] : null;

				int cBlock = 0;					// Compression bytes until next block
				ItPack status = new ItPack();
				ushort inCnt = 0;

				int outIndex = 0;

				while (length > 0)
				{
					int sTodo = (int)Math.Min(length, Constant.SlBufSize);

					if ((inFmt & SampleFlag.ItPacked) != 0)
					{
						// Decompress the sample
						if (cBlock == 0)
						{
							status.Bits = (ushort)((inFmt & SampleFlag._16Bits) != 0 ? 17 : 9);
							status.Last = 0;
							status.BufBits = 0;

							// Read the compressed length
							inCnt = moduleStream.Read_L_UINT16();

							cBlock = (inFmt & SampleFlag._16Bits) != 0 ? 0x4000 : 0x8000;

							if ((inFmt & SampleFlag.Delta) != 0)
								sl_old = 0;
						}

						int result;

						if ((inFmt & SampleFlag._16Bits) != 0)
							result = DecompressIt16(status, sampleDataStream, sl_buffer16, (ushort)sTodo, ref inCnt);
						else
							result = DecompressIt8(status, sampleDataStream, sl_buffer8, (ushort)sTodo, ref inCnt);

						if (result != sTodo)
						{
							// Well, some error occurred in the decompressing
							errorMessage = Resources.IDS_MIK_ERR_ITPACKING;
							return false;
						}

						cBlock -= sTodo;
					}
					else
					{
						// Read the sample into the memory
						if ((inFmt & SampleFlag._16Bits) != 0)
						{
							if ((inFmt & SampleFlag.BigEndian) != 0)
								sampleDataStream.ReadArray_B_UINT16s(sl_buffer16, 0, sTodo);
							else
								sampleDataStream.ReadArray_L_UINT16s(sl_buffer16, 0, sTodo);
						}
						else
							sampleDataStream.Read(sl_buffer8, 0, sTodo);
					}

					if (sampleDataStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIK_ERR_LOADING_SAMPLES;
						return false;
					}

					// Dedelta the sample
					if ((inFmt & SampleFlag.Delta) != 0)
					{
						if ((inFmt & SampleFlag._16Bits) != 0)
						{
							for (int t = 0; t < sTodo; t++)
							{
								sl_buffer16[t] += (ushort)sl_old;
								sl_old = (short)sl_buffer16[t];
							}
						}
						else
						{
							for (int t = 0; t < sTodo; t++)
							{
								sl_buffer8[t] += (byte)sl_old;
								sl_old = (sbyte)sl_buffer8[t];
							}
						}
					}

					// Convert the sample to signed
					if ((inFmt & SampleFlag.Signed) == 0)
					{
						if ((inFmt & SampleFlag._16Bits) != 0)
						{
							for (int t = 0; t < sTodo; t++)
								sl_buffer16[t] ^= 0x8000;
						}
						else
						{
							for (int t = 0; t < sTodo; t++)
								sl_buffer8[t] ^= 0x80;
						}
					}

					length -= (uint)sTodo;

					// Copy sample to output buffer
					if ((inFmt & SampleFlag._16Bits) != 0)
					{
						for (int t = 0; t < sTodo; t++)
						{
							byte[] samp = BitConverter.GetBytes(sl_buffer16[t]);
							buffer[outIndex++] = (sbyte)samp[0];
							buffer[outIndex++] = (sbyte)samp[1];
						}
					}
					else
					{
						Array.Copy(sl_buffer8, 0, buffer, outIndex, sTodo);
						outIndex += sTodo;
					}
				}
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Decompress a 8-bit IT packed sample
		/// </summary>
		/********************************************************************/
		private static int DecompressIt8(ItPack status, ModuleStream moduleStream, byte[] _out, ushort count, ref ushort inCnt)
		{
			ushort dest = 0, end = count;
			ushort newCount = 0;
			ushort bits = status.Bits;
			ushort bufBits = status.BufBits;
			sbyte last = (sbyte)status.Last;
			byte buf = status.Buf;

			while (dest < end)
			{
				ushort needBits = (ushort)(newCount != 0 ? 3 : 0);
				ushort x = 0;
				ushort haveBits = 0;

				while (needBits != 0)
				{
					// Feed buffer
					if (bufBits == 0)
					{
						if (inCnt-- != 0)
							buf = moduleStream.Read_UINT8();
						else
							buf = 0;

						bufBits = 8;
					}

					// Get as many bits as necessary
					ushort y = needBits < bufBits ? needBits : bufBits;
					x |= (ushort)((buf & ((1 << y) - 1)) << haveBits);
					buf >>= y;

					bufBits -= y;
					needBits -= y;
					haveBits += y;
				}

				if (newCount != 0)
				{
					newCount = 0;

					if (++x >= bits)
						x++;

					bits = x;
					continue;
				}

				if (bits < 7)
				{
					if (x == (1 << (bits - 1)))
					{
						newCount = 1;
						continue;
					}
				}
				else if (bits < 9)
				{
					ushort y = (ushort)((0xff >> (9 - bits)) - 4);
					if ((x > y) && (x <= y + 8))
					{
						if ((x -= y) >= bits)
							x++;

						bits = x;
						continue;
					}
				}
				else if (bits < 10)
				{
					if (x >= 0x100)
					{
						bits = (ushort)(x - 0x100 + 1);
						continue;
					}
				}
				else
				{
					// Error in compressed data
					return 0;
				}

				if (bits < 8)		// Extend sign
					x = (ushort)(((sbyte)(x << (8 - bits))) >> (8 - bits));

				_out[dest++] = (byte)(last += (sbyte)x);
			}

			status.Bits = bits;
			status.BufBits = bufBits;
			status.Last = last;
			status.Buf = buf;

			return dest;
		}



		/********************************************************************/
		/// <summary>
		/// Decompress a 16-bit IT packed sample
		/// </summary>
		/********************************************************************/
		private static int DecompressIt16(ItPack status, ModuleStream moduleStream, ushort[] _out, ushort count, ref ushort inCnt)
		{
			ushort dest = 0, end = count;
			int newCount = 0;
			ushort bits = status.Bits;
			ushort bufBits = status.BufBits;
			short last = status.Last;
			byte buf = status.Buf;

			while (dest < end)
			{
				int needBits = newCount != 0 ? 4 : 0;
				int x = 0;
				int haveBits = 0;

				while (needBits != 0)
				{
					// Feed buffer
					if (bufBits == 0)
					{
						if (inCnt-- != 0)
							buf = moduleStream.Read_UINT8();
						else
							buf = 0;

						bufBits = 8;
					}

					// Get as many bits as necessary
					int y = needBits < bufBits ? needBits : bufBits;
					x |= (buf & ((1 << y) - 1)) << haveBits;
					buf >>= y;

					bufBits -= (ushort)y;
					needBits -= y;
					haveBits += y;
				}

				if (newCount != 0)
				{
					newCount = 0;

					if (++x >= bits)
						x++;

					bits = (ushort)x;
					continue;
				}

				if (bits < 7)
				{
					if (x == (1 << (bits - 1)))
					{
						newCount = 1;
						continue;
					}
				}
				else if (bits < 17)
				{
					int y = (0xffff >> (17 - bits)) - 8;
					if ((x > y) && (x <= y + 16))
					{
						if ((x -= y) >= bits)
							x++;

						bits = (ushort)x;
						continue;
					}
				}
				else if (bits < 18)
				{
					if (x >= 0x10000)
					{
						bits = (ushort)(x - 0x10000 + 1);
						continue;
					}
				}
				else
				{
					// Error in compressed data
					return 0;
				}

				if (bits < 16)		// Extend sign
					x = ((short)(x << (16 - bits))) >> (16 - bits);

				_out[dest++] = (ushort)(last += (short)x);
			}

			status.Bits = bits;
			status.BufBits = bufBits;
			status.Last = last;
			status.Buf = buf;

			return dest;
		}
		#endregion
	}
}

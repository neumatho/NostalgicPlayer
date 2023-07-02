/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-SHRI decompressor
	/// </summary>
	internal class ShriDecompressor : XpkDecompressor
	{
		private class ShriState : State
		{
			public uint32_t vLen = 0;
			public uint32_t vNext = 0;
			public uint32_t shift = 0;
			public uint32_t[] ar = new uint32_t[999];
		}

		private static readonly uint32_t[] updates1 = { 358, 359, 386, 387, 414, 415 };
		private static readonly uint32_t[] updates2 = { 442, 456, 470, 484 };

		private readonly Buffer packedData;

		private readonly uint32_t ver;
		private readonly size_t startOffset;
		private readonly size_t rawSize;

		private readonly ShriState state;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ShriDecompressor(uint32_t hdr, Buffer packedData, ref State state)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr) || (packedData.Size() < 6))
				throw new InvalidFormatException();

			ver = packedData.Read8(0);

			// Second byte defines something that does not seem to be terribly important...
			uint8_t tmp = packedData.Read8(2);
			if (tmp < 0x80)
			{
				rawSize = packedData.ReadBe16(2);
				startOffset = 4;
			}
			else
			{
				rawSize = ~packedData.ReadBe32(2) + 1;
				startOffset = 6;
			}

			if (state == null)
			{
				if (ver == 2)
					throw new InvalidFormatException();

				state = new ShriState();
			}

			this.state = (ShriState)state;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("SHRI");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new ShriDecompressor(hdr, packedData, ref state);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			if (rawData.Size() != rawSize)
				throw new DecompressionException();

			ForwardInputStream inputStream = new ForwardInputStream(packedData, startOffset, packedData.Size());

			uint8_t ReadByte() => inputStream.ReadByte();

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawData.Size());

			// This follows quite closely Choloks pascal reference
			uint32_t[] ar = new uint32_t[999];

			void Resum()
			{
				for (uint32_t i = 498; i != 0; i--)
					ar[i] = ar[i * 2] + ar[i * 2 + 1];
			}

			void Init()
			{
				for (uint32_t i = 0; i < 499; i++)
					ar[i] = 0;

				for (uint32_t i = 0; i < 256; i++)
					ar[i + 499] = ((i < 32) || (i > 126)) ? 1U : 3U;

				for (uint32_t i = 256 + 499; i < 999; i++)
					ar[i] = 0;

				Resum();
			}

			void Update(uint32_t updateIndex, uint32_t increment)
			{
				if (updateIndex >= 499)
					return;

				updateIndex += 499;

				while (updateIndex != 0)
				{
					ar[updateIndex] += increment;
					updateIndex >>= 1;
				}

				if (ar[1] >= 0x2000)
				{
					for (uint32_t i = 499; i < 998; i++)
					{
						if (ar[i] != 0)
							ar[i] = (ar[i] >> 1) + 1;
					}

					Resum();
				}
			}

			uint32_t Scale(uint32_t a, uint32_t b, uint32_t mult)
			{
				if (b == 0)
					throw new DecompressionException();

				uint32_t tmp = (a << 16) / b;
				uint32_t tmp2 = (((a << 16) % b) << 16) / b;

				return ((mult & 0xffff) * tmp >> 16) + ((mult >> 16) * tmp2 >> 16) + (mult >> 16) * tmp;
			}

			uint32_t vLen = 0, vNext = 0;

			void Upgrade()
			{
				if (vNext >= 65532)
					vNext = ~0U;
				else if (vLen == 0)
					vNext = 1;
				else
				{
					uint32_t vValue = vNext - 1;

					if (vValue < 48)
						Update(vValue + 256, 1);

					uint32_t bits = 0, compare = 4;

					while (vValue >= compare)
					{
						vValue -= compare;
						compare <<= 1;
						bits++;
					}

					if (bits >= 14)
						vNext = ~0U;
					else
					{
						if (vValue == 0)
						{
							if (bits < 7)
							{
								for (uint32_t i = 304; i <= 307; i++)
									Update((bits << 2) + i, 1);
							}

							if (bits < 13)
							{
								for (uint32_t i = 332; i <= 333; i++)
									Update((bits << 1) + i, 1);
							}

							foreach (uint32_t it in updates1)
								Update((bits << 1) + it, 1);

							foreach (uint it in updates2)
								Update(bits + it, 1);
						}

						if (vNext < 49)
							vNext++;
						else if (vNext == 49)
							vNext = 61;
						else
							vNext = (vNext << 1) + 3;
					}
				}
			}

			uint32_t stream = 0, shift = 0;

			void RefillStream()
			{
				while (shift < 0x100_0000)
				{
					stream = (stream << 8) | ReadByte();
					shift <<= 8;
				}
			}

			uint32_t GetSymbol()
			{
				if ((shift >> 16) == 0)
					throw new DecompressionException();

				uint32_t vValue = (stream / (shift >> 16)) & 0xffff;
				uint32_t threshold = (ar[1] * vValue) >> 16;
				uint32_t arIndex = 1;
				uint32_t result = 0;

				do
				{
					arIndex <<= 1;
					uint32_t tmp = ar[arIndex] + result;

					if (threshold >= tmp)
					{
						result = tmp;
						arIndex++;
					}
				}
				while (arIndex < 499);

				uint32_t newValue = Scale(result, ar[1], shift);
				if (newValue > stream)
				{
					while (newValue > stream)
					{
						if (--arIndex < 499)
							arIndex += 499;

						result -= ar[arIndex];
						newValue = Scale(result, ar[1], shift);
					}
				}
				else
				{
					result += ar[arIndex];

					while (result < ar[1])
					{
						uint32_t compare = Scale(result, ar[1], shift);
						if (stream < compare)
							break;

						if (++arIndex >= 998)
							arIndex -= 499;

						result += ar[arIndex];
						newValue = compare;
					}
				}

				stream -= newValue;
				shift = Scale(ar[arIndex], ar[1], shift);
				uint32_t addition = (ar[1] >> 10) + 3;
				arIndex -= 499;

				Update(arIndex, addition);
				RefillStream();

				return arIndex;
			}

			uint32_t GetCode(uint32_t size)
			{
				uint32_t ret = 0;

				while (size-- != 0)
				{
					ret <<= 1;
					shift >>= 1;

					if (stream >= shift)
					{
						ret++;
						stream -= shift;
					}

					RefillStream();
				}

				return ret;
			}

			if (ver == 1)
			{
				Init();
				Update(498, 1);

				shift = 0x8000_0000;
			}
			else
			{
				vLen = state.vLen;
				vNext = state.vNext;
				shift = state.shift;
				Array.Copy(state.ar, ar, 999);
			}

			{
				Span<uint8_t> buf = inputStream.Consume(4);
				stream = (uint32_t)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
			}

			while (!outputStream.Eof)
			{
				while (vLen >= vNext)
					Upgrade();

				uint32_t code = GetSymbol();
				if (code < 256)
				{
					outputStream.WriteByte((uint8_t)code);
					vLen++;
				}
				else
				{
					uint32_t DistanceAddition(uint32_t i) => (uint32_t)(((1 << (int)(i + 2)) - 1) & ~0x3);

					uint32_t count, distance;

					if (code < 304)
					{
						count = 2;
						distance = code - 255;
					}
					else if (code < 332)
					{
						uint32_t tmp = code - 304;
						uint32_t extra = GetCode(tmp >> 2);
						distance = ((extra << 2) | (tmp & 3)) + DistanceAddition(tmp >> 2) + 1;
						count = 3;
					}
					else if (code < 358)
					{
						uint32_t tmp = code - 332;
						uint32_t extra = GetCode((tmp >> 1) + 1);
						distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
						count = 4;
					}
					else if (code < 386)
					{
						uint32_t tmp = code - 358;
						uint32_t extra = GetCode((tmp >> 1) + 1);
						distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
						count = 5;
					}
					else if (code < 414)
					{
						uint32_t tmp = code - 386;
						uint32_t extra = GetCode((tmp >> 1) + 1);
						distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
						count = 6;
					}
					else if (code < 442)
					{
						uint32_t tmp = code - 414;
						uint32_t extra = GetCode((tmp >> 1) + 1);
						distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
						count = 7;
					}
					else if (code < 498)
					{
						uint32_t tmp = code - 442;
						uint32_t d = tmp / 14;
						uint32_t m = tmp % 14;
						count = GetCode(d + 2) + DistanceAddition(d) + 8;
						distance = GetCode(m + 2) + DistanceAddition(m) + 1;
					}
					else
					{
						count = GetCode(16);
						distance = GetCode(16);
					}

					vLen += count;
					if (count == 0)
						throw new DecompressionException();

					outputStream.Copy(distance, count, previousBuffers);
				}
			}

			state.vLen = vLen;
			state.vNext = vNext;
			state.shift = shift;
			Array.Copy(ar, state.ar, 999);

			previousBuffers.Add(rawData);
		}
	}
}

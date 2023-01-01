/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (SHRI)
	/// </summary>
	internal class Xpk_ShriStream : XpkStream
	{
		private class ShriState
		{
			public uint vLen = 0;
			public uint vNext = 0;
			public uint shift = 0;
			public uint[] ar = new uint[999];
		}

		private static readonly uint[] updates1 = { 358, 359, 386, 387, 414, 415 };
		private static readonly uint[] updates2 = { 442, 456, 470, 484 };

		private ShriState state;
		private readonly List<byte[]> previousData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_ShriStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
			previousData = new List<byte[]>();
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk, byte[] rawData)
		{
			uint ver = chunk[0];
			if ((ver == 0) || (ver > 2))
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			// Second byte defines something that does not seem to be terribly important...
			uint startOffset, rawSize;

			byte tmpByte = chunk[2];
			if (tmpByte < 0x80)
			{
				rawSize = Read16(chunk, 2);
				startOffset = 4;
			}
			else
			{
				rawSize = ~Read32(chunk, 2) + 1;
				startOffset = 6;
			}

			if (rawData.Length != rawSize)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			if (state == null)
			{
				if (ver == 2)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				state = new ShriState();
			}

			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, startOffset, (uint)chunk.Length);

				byte ReadByte() => inputStream.ReadByte();

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, (uint)rawData.Length);

				// This follows quite closely Choloks pascal reference
				uint[] ar = new uint[999];

				void Resum()
				{
					for (uint i = 498; i != 0; i--)
						ar[i] = ar[i * 2] + ar[i * 2 + 1];
				}

				void Init()
				{
					for (uint i = 0; i < 499; i++)
						ar[i] = 0;

					for (uint i = 0; i < 256; i++)
						ar[i + 499] = ((i < 32) || (i > 126)) ? (uint)1 : 3;

					for (uint i = 256 + 499; i < 999; i++)
						ar[i] = 0;

					Resum();
				}

				void Update(uint updateIndex, uint increment)
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
						for (uint i = 499; i < 998; i++)
						{
							if (ar[i] != 0)
								ar[i] = (ar[i] >> 1) + 1;
						}

						Resum();
					}
				}

				uint Scale(uint a, uint b, uint mult)
				{
					if (b == 0)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					uint tmp = (a << 16) / b;
					uint tmp2 = (((a << 16) % b) << 16) / b;

					return ((mult & 0xffff) * tmp >> 16) + ((mult >> 16) * tmp2 >> 16) + (mult >> 16) * tmp;
				}

				uint vLen = 0, vNext = 0;

				void Upgrade()
				{
					if (vNext >= 65532)
						vNext = ~0U;
					else if (vLen == 0)
						vNext = 1;
					else
					{
						uint vValue = vNext - 1;
						if (vValue < 48)
							Update(vValue + 256, 1);

						uint bits = 0, compare = 4;

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
									for (uint i = 304; i <= 307; i++)
										Update((bits << 2) + i, 1);
								}

								if (bits < 13)
								{
									for (uint i = 332; i <= 333; i++)
										Update((bits << 1) + i, 1);
								}

								foreach (uint it in updates1)
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

				uint stream = 0, shift = 0;

				void RefillStream()
				{
					while (shift < 0x1000000)
					{
						stream = (stream << 8) | ReadByte();
						shift <<= 8;
					}
				}

				uint GetSymbol()
				{
					if ((shift >> 16) == 0)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					uint vValue = (stream / (shift >> 16)) & 0xffff;
					uint threshold = (ar[1] * vValue) >> 16;
					uint arIndex = 1;
					uint result = 0;

					do
					{
						arIndex <<= 1;
						uint tmp = ar[arIndex] + result;

						if (threshold >= tmp)
						{
							result = tmp;
							arIndex++;
						}
					}
					while (arIndex < 499);

					uint newValue = Scale(result, ar[1], shift);
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
							uint compare = Scale(result, ar[1], shift);
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
					uint addition = (ar[1] >> 10) + 3;
					arIndex -= 499;

					Update(arIndex, addition);
					RefillStream();

					return arIndex;
				}

				uint GetCode(uint size)
				{
					uint ret = 0;

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

					shift = 0x80000000;
				}
				else
				{
					vLen = state.vLen;
					vNext = state.vNext;
					shift = state.shift;
					Array.Copy(state.ar, ar, 999);
				}

				{
					byte[] buf = inputStream.Consume(4);
					stream = (uint)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
				}

				while (!outputStream.Eof)
				{
					while (vLen >= vNext)
						Upgrade();

					uint code = GetSymbol();
					if (code < 256)
					{
						outputStream.WriteByte(code);
						vLen++;
					}
					else
					{
						uint DistanceAddition(uint i) => (uint)(((1 << (int)(i + 2)) - 1) & ~0x3);

						uint count, distance;

						if (code < 304)
						{
							count = 2;
							distance = code - 255;
						}
						else if (code < 332)
						{
							uint tmp = code - 304;
							uint extra = GetCode(tmp >> 2);
							distance = ((extra << 2) | (tmp & 3)) + DistanceAddition(tmp >> 2) + 1;
							count = 3;
						}
						else if (code < 358)
						{
							uint tmp = code - 332;
							uint extra = GetCode((tmp >> 1) + 1);
							distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
							count = 4;
						}
						else if (code < 386)
						{
							uint tmp = code - 358;
							uint extra = GetCode((tmp >> 1) + 1);
							distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
							count = 5;
						}
						else if (code < 414)
						{
							uint tmp = code - 386;
							uint extra = GetCode((tmp >> 1) + 1);
							distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
							count = 6;
						}
						else if (code < 442)
						{
							uint tmp = code - 414;
							uint extra = GetCode((tmp >> 1) + 1);
							distance = ((extra << 1) | (tmp & 1)) + DistanceAddition(tmp >> 1) + 1;
							count = 7;
						}
						else if (code < 498)
						{
							uint tmp = code - 442;
							uint d = tmp / 14;
							uint m = tmp % 14;
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
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						outputStream.Copy(distance, count, previousData);
					}
				}

				state.vLen = vLen;
				state.vNext = vNext;
				state.shift = shift;
				Array.Copy(ar, state.ar, 999);

				previousData.Add(rawData);
			}
		}
	}
}

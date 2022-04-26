/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (BLZW)
	/// </summary>
	internal class Xpk_BlzwStream : XpkStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_BlzwStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk, byte[] rawData)
		{
			uint maxBits = Read16(chunk, 0);
			if ((maxBits < 9) || (maxBits > 20))
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			uint stackLength = (uint)(Read16(chunk, 2) + 5);

			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, 4, (uint)chunk.Length);
				MsbBitReader bitReader = new MsbBitReader(inputStream);

				uint ReadBits(uint count) => bitReader.ReadBits8(count);

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, (uint)rawData.Length);

				uint maxCode = (uint)(1 << (int)maxBits);
				uint[] prefix = new uint[maxCode - 259];
				byte[] suffix = new byte[maxCode - 259];
				byte[] stack = new byte[stackLength];

				uint freeIndex, codeBits, prevCode, newCode;

				uint SuffixLookup(uint code)
				{
					if (code >= freeIndex)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					return code < 259 ? code : suffix[code - 259];
				}

				void Insert(uint code)
				{
					uint stackPos = 0;
					newCode = SuffixLookup(code);

					while (code >= 259)
					{
						if ((stackPos + 1) >= stackLength)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						stack[stackPos++] = (byte)newCode;
						code = prefix[code - 259];
						newCode = SuffixLookup(code);
					}

					stack[stackPos++] = (byte)newCode;

					while (stackPos != 0)
						outputStream.WriteByte(stack[--stackPos]);
				}

				void Init()
				{
					codeBits = 9;
					freeIndex = 259;
					prevCode = ReadBits(codeBits);
					Insert(prevCode);
				}

				Init();

				while (!outputStream.Eof)
				{
					uint code = ReadBits(codeBits);

					switch (code)
					{
						case 256:
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						case 257:
						{
							Init();
							break;
						}

						case 258:
						{
							codeBits++;
							break;
						}

						default:
						{
							if (code >= freeIndex)
							{
								uint tmp = newCode;
								Insert(prevCode);
								outputStream.WriteByte(tmp);
							}
							else
								Insert(code);

							if (freeIndex < maxCode)
							{
								suffix[freeIndex - 259] = (byte)newCode;
								prefix[freeIndex - 259] = prevCode;
								freeIndex++;
							}

							prevCode = code;
							break;
						}
					}
				}
			}
		}
	}
}

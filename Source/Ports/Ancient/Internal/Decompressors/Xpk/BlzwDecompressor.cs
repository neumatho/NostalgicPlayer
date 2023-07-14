/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-BLZW decompressor
	/// </summary>
	internal class BlzwDecompressor : XpkDecompressor
	{
		private readonly Buffer packedData;
		private readonly uint32_t maxBits;
		private readonly uint32_t stackLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private BlzwDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr))
				throw new InvalidFormatException();

			maxBits = packedData.ReadBe16(0);
			if ((maxBits < 9) || (maxBits > 20))
				throw new InvalidFormatException();

			stackLength = packedData.ReadBe16(2) + 5U;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("BLZW");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new BlzwDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			ForwardInputStream inputStream = new ForwardInputStream(packedData, 4, packedData.Size());
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawData.Size());

			void WriteByte(uint8_t value) => outputStream.WriteByte(value);

			uint32_t codeBits = 9;

			uint32_t ReadCode() => ReadBits(codeBits);

			uint32_t firstCode = ReadCode();
			LzwDecoder decoder = new LzwDecoder(1U << (int)maxBits, 259, stackLength, firstCode);
			decoder.Write(firstCode, false, WriteByte);

			while (!outputStream.Eof)
			{
				uint32_t code = ReadBits(codeBits);

				switch (code)
				{
					case 256:
						throw new DecompressionException();

					case 257:
					{
						codeBits = 9;
						firstCode = ReadCode();

						decoder.Reset(firstCode);
						decoder.Write(firstCode, false, WriteByte);
						break;
					}

					case 258:
					{
						if (codeBits >= 24)
							throw new DecompressionException();

						codeBits++;
						break;
					}

					default:
					{
						decoder.Write(code, !decoder.IsLiteral(code), WriteByte);
						decoder.Add(code);
						break;
					}
				}
			}
		}
	}
}

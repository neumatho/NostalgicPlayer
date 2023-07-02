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
	/// XPK-SMPL decompressor
	/// </summary>
	internal class SmplDecompressor : XpkDecompressor
	{
		private readonly Buffer packedData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private SmplDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr) || (packedData.Size() < 2))
				throw new InvalidFormatException();

			if (packedData.ReadBe16(0) != 1)
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("SMPL");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new SmplDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			ForwardInputStream inputStream = new ForwardInputStream(packedData, 2, packedData.Size());
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);
			uint32_t ReadBit() => bitReader.ReadBits8(1);

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawData.Size());

			HuffmanDecoder<uint32_t> decoder = new HuffmanDecoder<uint32_t>();

			for (uint32_t i = 0; i < 256; i++)
			{
				uint32_t codeLength = ReadBits(4);

				if (codeLength == 0)
					continue;

				if (codeLength == 15)
					codeLength = ReadBits(4) + 15;

				uint32_t code = ReadBits(codeLength);
				decoder.Insert(new HuffmanCode<uint32_t>(codeLength, code, i));
			}

			uint8_t accum = 0;

			while (!outputStream.Eof)
			{
				uint32_t code = decoder.Decode(ReadBit);
				accum += (uint8_t)code;

				outputStream.WriteByte(accum);
			}
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-LHLB decompressor
	/// </summary>
	internal class LhlbDecompressor : XpkDecompressor
	{
		private readonly Buffer packedData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private LhlbDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("LHLB");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new LhlbDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			//
			// If this code is changed, remember to change LhLibraryDecompressor as well
			//
			ForwardInputStream inputStream = new ForwardInputStream(packedData, 0, packedData.Size());
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);
			uint32_t ReadBit() => bitReader.ReadBits8(1);

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawData.Size());

			// Same logic as in Choloks pascal implementation
			// Differences to LH1:
			// - LHLB does not halve probabilities at 32k
			// - 314 vs. 317 sized Huffman entry
			// - No end code
			// - Different distance/count logic
			DynamicHuffmanDecoder decoder = new DynamicHuffmanDecoder(317);
			VariableLengthCodeDecoder vlcDecoder = new VariableLengthCodeDecoder(5, 5, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10);

			while (!outputStream.Eof)
			{
				uint32_t code = decoder.Decode(ReadBit);
				if (code == 316)
					break;

				if (decoder.GetMaxFrequency() < 0x8000)
					decoder.Update(code);

				if (code < 256)
					outputStream.WriteByte((uint8_t)code);
				else
				{
					uint32_t distance = vlcDecoder.Decode(ReadBits, ReadBits(4));
					uint32_t count = code - 255;

					outputStream.Copy(distance, count);
				}
			}
		}
	}
}

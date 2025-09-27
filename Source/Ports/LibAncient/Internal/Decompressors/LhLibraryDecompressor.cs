/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// Lh.library decompressor
	/// </summary>
	internal class LhLibraryDecompressor : Decompressor
	{
		private readonly Buffer packedData;

		private readonly size_t rawSize;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private LhLibraryDecompressor(Buffer packedData, size_t rawSize) : base(DecompressorType.LhLibrary)
		{
			this.packedData = packedData;

			this.rawSize = rawSize;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Decompressor Create(Buffer packedData, size_t rawSize)
		{
			return new LhLibraryDecompressor(packedData, rawSize);
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the decompressed data
		/// </summary>
		/********************************************************************/
		public override size_t GetRawSize()
		{
			return rawSize;
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<uint8_t[]> DecompressImpl()
		{
			uint8_t[] outputData = new uint8_t[rawSize];
			WrappedArrayBuffer rawData = new WrappedArrayBuffer(outputData);

			//
			// This code has been copied from LhlbDecompressor. If the original code
			// is changed, this should also be changed
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

			yield return outputData;
		}
	}
}

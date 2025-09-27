/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// PowerPacker decompressor
	/// </summary>
	internal class PowerPackerDecompressor : Decompressor
	{
		private readonly Buffer packedData;

		private readonly size_t dataStart;
		private readonly size_t rawSize;
		private readonly uint8_t startShift;

		private readonly uint8_t[] modeTable = new uint8_t[4];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private PowerPackerDecompressor(Buffer packedData) : base(DecompressorType.PowerPacker)
		{
			this.packedData = packedData;

			// Check the file size
			if (packedData.Size() < 16)
				throw new InvalidFormatException();

			dataStart = packedData.Size() - 4;

			uint32_t hdr = packedData.ReadBe32(0);
			if (!DetectHeader(hdr))
				throw new InvalidFormatException();

			uint32_t mode = packedData.ReadBe32(4);

			if ((mode != 0x9090909) && (mode != 0x90a0a0a) && (mode != 0x90a0b0b) && (mode != 0x90a0c0c) && (mode != 0x90a0c0d))
				throw new InvalidFormatException();

			for (uint32_t i = 0; i < 4; i++)
			{
				modeTable[i] = (uint8_t)(mode >> 24);
				mode <<= 8;
			}

			uint32_t tmp = packedData.ReadBe32(dataStart);

			rawSize = tmp >> 8;
			startShift = (uint8_t)(tmp & 0xff);

			if ((rawSize == 0) || (startShift >= 0x20) || (rawSize > GetMaxRawSize()))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr)
		{
			return (hdr == Common.Common.FourCC("PP20"))
				|| (hdr == Common.Common.FourCC("CHFC"))	// Sky High Stuntman
				|| (hdr == Common.Common.FourCC("DEN!"))	// Jewels - Crossroads
				|| (hdr == Common.Common.FourCC("DXS9"))	// Hopp oder Top, Punkt Punkt Punkt
				|| (hdr == Common.Common.FourCC("H.D."))	// F1 Challenge
				|| (hdr == Common.Common.FourCC("RVV!"));	// Hoi AGA Remix
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new static Decompressor Create(Buffer packedData)
		{
			return new PowerPackerDecompressor(packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
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

			uint32_t key = 0;

			BackwardInputStream inputStream = new BackwardInputStream(packedData, 8, dataStart);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => Common.Common.RotateBits(bitReader.ReadBitsGeneric(count, () => (inputStream.ReadBE32() ^ key, 32)), count);
			uint ReadBit() => ReadBits(1);

			ReadBits(startShift);

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			for (;;)
			{
				uint32_t count;

				if (ReadBit() == 0)
				{
					count = 1;

					// This does not make much sense I know. But it is what it is...
					for (;;)
					{
						uint32_t tmp = ReadBits(2);
						count += tmp;

						if (tmp < 3)
							break;
					}

					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte((uint8_t)ReadBits(8));
				}

				if (outputStream.Eof)
					break;

				uint32_t modeIndex = ReadBits(2);
				uint32_t distance;

				if (modeIndex == 3)
				{
					distance = ReadBits((uint32_t)(ReadBit() != 0 ? modeTable[modeIndex] : 7)) + 1;

					// Ditto
					count = 5;
					for (;;)
					{
						uint32_t tmp = ReadBits(3);
						count += tmp;

						if (tmp < 7)
							break;
					}
				}
				else
				{
					count = modeIndex + 2;
					distance = ReadBits(modeTable[modeIndex]) + 1;
				}

				outputStream.Copy(distance, count);
			}

			yield return outputData;
		}
	}
}

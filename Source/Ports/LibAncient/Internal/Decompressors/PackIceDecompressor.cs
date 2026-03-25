/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// Ice decompressor
	/// </summary>
	internal class PackIceDecompressor : Decompressor
	{
		private readonly Buffer packedData;

		private readonly uint32_t rawSize;
		private readonly size_t packedSize;
		private readonly uint32_t ver;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private PackIceDecompressor(Buffer packedData, bool exactSizeKnown) : base(DecompressorType.PackIce)
		{
			this.packedData = packedData;

			if (packedData.Size() < 8)
				throw new InvalidFormatException();

			uint32_t hdr = packedData.ReadBe32(0);
			uint32_t footer = exactSizeKnown ? packedData.ReadBe32(packedData.Size() - 4) : 0;

			if (!DetectHeader(hdr, footer))
				throw new InvalidFormatException();

			// In theory a bitstream can be v1 and v2 at the same time
			// we prefer v1 in case of conflict
			if (footer == Common.Common.FourCC("Ice!"))
			{
				packedSize = packedData.Size();
				rawSize = packedData.ReadBe32(packedData.Size() - 8);
				ver = 0;
			}
			else
			{
				packedSize = packedData.ReadBe32(4);

				if ((packedSize == 0) || (packedSize > packedData.Size()) || (packedSize > GetMaxPackedSize()))
					throw new InvalidFormatException();

				rawSize = packedData.ReadBe32(8);
				ver = hdr == Common.Common.FourCC("ICE!") ? 2U : 1U;
			}

			if ((rawSize == 0) || (rawSize > GetMaxRawSize()))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr, uint32_t footer)
		{
			return
				// Ver 0
				(footer == Common.Common.FourCC("Ice!")) ||
				// Ver 1
				(hdr == Common.Common.FourCC("Ice!")) ||
				(hdr == Common.Common.FourCC("TMM!")) ||	// Demo Numb/Movement
				(hdr == Common.Common.FourCC("TSM!")) ||	// Lots of Amiga games
				(hdr == Common.Common.FourCC("SHE!")) ||	// Demo Overload2/JetSet
				// Ver 2
				(hdr == Common.Common.FourCC("ICE!"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static new Decompressor Create(Buffer packedData, bool exactSizeKnown)
		{
			return new PackIceDecompressor(packedData, exactSizeKnown);
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
			if (ver != 0)
			{
				if (ver == 1)
				{
					// There is a mix of v1 and v2 with a single id.
					// Thus we need to try both. start with v1
					try
					{
						return DecompressInternal(false).ToArray();
					}
					catch (Exception)
					{
						// Nothing needed
					}
				}

				return DecompressInternal(true);
			}
			else
				return DecompressInternal(false);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		private IEnumerable<uint8_t[]> DecompressInternal(bool useBytes)
		{
			uint8_t[] outputData = new uint8_t[rawSize];
			WrappedArrayBuffer rawData = new WrappedArrayBuffer(outputData);

			BackwardInputStream inputStream = new BackwardInputStream(packedData, ver != 0 ? 12U : 0U, packedSize - (ver != 0 ? 0U : 8U));
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count)
			{
				if (useBytes)
					return bitReader.ReadBits8(count);
				else
					return bitReader.ReadBitsBe32(count);
			}
			uint8_t ReadByte() => inputStream.ReadByte();

			// Anchor-bit handling
			{
				uint32_t value = useBytes ? inputStream.ReadByte() : inputStream.ReadBE32();
				uint32_t tmp = value;
				uint32_t count = 0;

				while (tmp != 0)
				{
					tmp <<= 1;
					count++;
				}

				if (count != 0)
					count--;

				if (count != 0)
					bitReader.Reset(value >> (c_int)(32 - count), (uint8_t)(count - (useBytes ? 24 : 0)));
			}

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			VariableLengthCodeDecoder litVlcDecoderOld = new VariableLengthCodeDecoder(1, 2, 2, 3, 10);
			VariableLengthCodeDecoder litVlcDecoderNew = new VariableLengthCodeDecoder(1, 2, 2, 3, 8, 15);
			VariableLengthCodeDecoder countBaseDecoder = new VariableLengthCodeDecoder(1, 1, 1, 1);
			VariableLengthCodeDecoder countDecoder = new VariableLengthCodeDecoder(0, 0, 1, 2, 10);
			VariableLengthCodeDecoder distanceBaseDecoder = new VariableLengthCodeDecoder(1, 1);
			VariableLengthCodeDecoder distanceDecoder = new VariableLengthCodeDecoder(5, 8, 12);

			// Early versions have pretty broken distance/length handling
			// which was later improved at the same time bitstream was changed to
			// byte-based format. This is why these 2 are bundled in here
			for (;;)
			{
				if (ReadBits(1) != 0)
				{
					uint32_t litLength = (ver != 0 ? litVlcDecoderNew.DecodeCascade(ReadBits) : litVlcDecoderOld.DecodeCascade(ReadBits)) + 1U;

					for (uint32_t i = 0; i < litLength; i++)
						outputStream.WriteByte(ReadByte());
				}

				// Exit criteria
				if (outputStream.Eof)
					break;

				uint32_t countBase = countBaseDecoder.DecodeCascade(ReadBits);
				uint32_t count = countDecoder.Decode(ReadBits, countBase) + 2;
				uint32_t distance;

				if (count == 2)
				{
					if (ReadBits(1) != 0)
						distance = ReadBits(9) + 0x40;
					else
						distance = ReadBits(6);

					distance += count - (useBytes ? 1U : 0);
				}
				else
				{
					uint32_t distanceBase = distanceBaseDecoder.DecodeCascade(ReadBits);

					if (distanceBase < 2)
						distanceBase ^= 1;

					distance = distanceDecoder.Decode(ReadBits, distanceBase);

					if (useBytes)
					{
						if (distance != 0)
							distance += count - 1;
						else
							distance = 1;
					}
					else
						distance += count;
				}

				outputStream.Copy(distance, count);
			}

			// Picture mode
			if ((ver != 0) && (bitReader.Available != 0) && (ReadBits(1) != 0))
			{
				uint32_t pictureSize = 32000;

				if (ver == 2)
				{
					// Magic: Format changes between versions. ID does not
					if ((bitReader.Available >= 17) && (ReadBits(1) != 0))
						pictureSize = (ReadBits(16) * 8) + 8;
				}

				if (rawSize < pictureSize)
					throw new DecompressionException();

				// C2P vibes here
				for (uint32_t i = rawSize - pictureSize; i < rawSize; i += 8)
				{
					uint16_t[] values = new uint16_t[4];

					for (uint32_t j = 0; j < 8; j += 2)
					{
						uint16_t tmp = rawData.ReadBe16(i + 6 - j);

						for (uint32_t k = 0; k < 16; k++)
						{
							values[k & 3] = (uint16_t)((values[k & 3] << 1) | (tmp >> 15));
							tmp <<= 1;
						}
					}

					for (uint32_t j = 0; j < 4; j++)
					{
						rawData[i + (j * 2)] = (uint8_t)(values[j] >> 8);
						rawData[i + (j * 2) + 1] = (uint8_t)values[j];
					}
				}
			}

			// Final sanity checking
			if (!inputStream.Eof)
				throw new DecompressionException();

			yield return outputData;
		}
		#endregion
	}
}

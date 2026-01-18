/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-NUKE decompressor
	/// </summary>
	internal class NukeDecompressor : XpkDecompressor
	{
		private readonly Buffer packedData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected NukeDecompressor(uint32_t hdr, Buffer packedData, bool skipDetect)
		{
			this.packedData = packedData;

			if (!skipDetect && !DetectHeaderXpk(hdr))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("NUKE");
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new NukeDecompressor(hdr, packedData, false);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			// There are 2 streams, reverse stream for bytes and
			// normal stream for bits, the bit stream is divided
			// into single bit, 2 bit, 4 bit and random accumulator
			ForwardInputStream forwardInputStream = new ForwardInputStream(packedData, 0, packedData.Size());
			BackwardInputStream backwardInputStream = new BackwardInputStream(packedData, 0, packedData.Size());

			MsbBitReader bit1Reader = new MsbBitReader(forwardInputStream);
			MsbBitReader bit2Reader = new MsbBitReader(forwardInputStream);
			LsbBitReader bit4Reader = new LsbBitReader(forwardInputStream);
			MsbBitReader bitXReader = new MsbBitReader(forwardInputStream);

			uint32_t ReadBit() => bit1Reader.ReadBitsBe16(1);
			uint32_t Read2Bits() => bit2Reader.ReadBitsBe16(2);
			uint32_t Read4Bits() => bit4Reader.ReadBitsBe32(4);
			uint32_t ReadBits(uint32_t count) => bitXReader.ReadBitsBe16(count);
			uint8_t ReadByte() => backwardInputStream.ReadByte();

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawData.Size());

			VariableLengthCodeDecoder vlcDecoder = new VariableLengthCodeDecoder(
				 4,  6,  8,  9,
				-4,  7,  9, 11, 13, 14,
				-5,  7,  9, 11, 13, 14);

			for (;;)
			{
				if (ReadBit() == 0)
				{
					uint32_t count;

					if (ReadBit() != 0)
					{
						count = 1;
					}
					else
					{
						count = 0;
						uint32_t tmp;

						do
						{
							tmp = Read2Bits();
							if (tmp != 0)
								count += 5 - tmp;
							else
								count += 3;
						}
						while (tmp == 0);
					}

					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte(ReadByte());
				}

				if (outputStream.Eof)
					break;

				uint32_t distanceIndex = Read4Bits();
				uint32_t distance = vlcDecoder.Decode(ReadBits, distanceIndex);

				uint32_t copyCount;

				if (distanceIndex < 4U)
					copyCount = 2;
				else if (distanceIndex < 10U)
					copyCount = 3;
				else
				{
					copyCount = Read2Bits();

					if (copyCount == 0)
					{
						copyCount = 3 + 3;
						uint32_t tmp;

						do
						{
							tmp = Read4Bits();
							if (tmp != 0)
								copyCount += 16 - tmp;
							else
								copyCount += 15;
						}
						while (tmp == 0);
					}
					else
						copyCount = 3 + 4 - copyCount;
				}

				outputStream.Copy(distance, copyCount);
			}
		}
	}
}

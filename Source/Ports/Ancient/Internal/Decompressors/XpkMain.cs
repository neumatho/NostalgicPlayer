/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors
{
	/// <summary>
	/// XPK detection class
	/// </summary>
	internal class XpkMain : Decompressor
	{
		private delegate XpkDecompressor CreateSubDecompressor(uint32_t hdr, Buffer packedData, ref XpkDecompressor.State state);

		private struct DecompressorPair
		{
			public Func<uint32_t, bool> First;
			public CreateSubDecompressor Second;
			public DecompressorType Type;
		}

		private static DecompressorPair[] decompressors = new DecompressorPair[]
		{
			new DecompressorPair { First = BlzwDecompressor.DetectHeaderXpk, Second = BlzwDecompressor.Create, Type = DecompressorType.Xpk_Blzw },
			new DecompressorPair { First = BZip2Decompressor.DetectHeaderXpk, Second = BZip2Decompressor.Create, Type = DecompressorType.Xpk_Bzp2 },
			new DecompressorPair { First = LhlbDecompressor.DetectHeaderXpk, Second = LhlbDecompressor.Create, Type = DecompressorType.Xpk_Lhlb },
			new DecompressorPair { First = MashDecompressor.DetectHeaderXpk, Second = MashDecompressor.Create, Type = DecompressorType.Xpk_Mash },
			new DecompressorPair { First = RakeDecompressor.DetectHeaderXpk, Second = RakeDecompressor.Create, Type = DecompressorType.Xpk_Rake },
			new DecompressorPair { First = ShriDecompressor.DetectHeaderXpk, Second = ShriDecompressor.Create, Type = DecompressorType.Xpk_Shri },
			new DecompressorPair { First = SmplDecompressor.DetectHeaderXpk, Second = SmplDecompressor.Create, Type = DecompressorType.Xpk_Smpl },
			new DecompressorPair { First = SqshDecompressor.DetectHeaderXpk, Second = SqshDecompressor.Create, Type = DecompressorType.Xpk_Sqsh },
			new DecompressorPair { First = UnimplementedDecompressor.DetectHeaderXpk, Second = UnimplementedDecompressor.Create, Type = DecompressorType.Unknown },
		};

		private readonly Buffer packedData;

		private readonly uint32_t packedSize;
		private readonly uint32_t rawSize;
		private readonly uint32_t headerSize;
		private readonly uint32_t type;
		private readonly bool longHeaders;
		private readonly bool hasPassword = false;

		private CreateSubDecompressor subDecompressor;
		private DecompressorType decompressorType;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private XpkMain(Buffer packedData) : base(DecompressorType.Unknown)
		{
			this.packedData = packedData;

			if (packedData.Size() < 44)
				throw new InvalidFormatException();

			uint32_t hdr = packedData.ReadBe32(0);
			if (!DetectHeader(hdr))
				throw new InvalidFormatException();

			packedSize = packedData.ReadBe32(4);
			type = packedData.ReadBe32(8);
			rawSize = packedData.ReadBe32(12);

			if ((rawSize == 0) || (packedSize == 0))
				throw new InvalidFormatException();

			if ((rawSize > GetMaxRawSize()) || (packedSize > GetMaxPackedSize()))
				throw new InvalidFormatException();

			uint8_t flags = packedData.Read8(32);
			longHeaders = (flags & 1) != 0;

			if ((flags & 2) != 0)		// Late failure so we can identify format
				hasPassword = true;

			if ((flags & 4) != 0)		// Extra header
				headerSize = (uint32_t)38 + packedData.ReadBe16(36);
			else
				headerSize = 36;

			if (OverflowCheck.Sum(packedSize, 8) > packedData.Size())
				throw new InvalidFormatException();

			bool found = false;

			foreach (DecompressorPair it in decompressors)
			{
				if (it.First(type))
				{
					subDecompressor = it.Second;
					decompressorType = it.Type;
					found = true;
					break;
				}
			}

			if (!found)
				throw new InvalidFormatException();

			if (!HeaderChecksum(packedData, 0, 36))
				throw new VerificationException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("XPKF");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new static Decompressor Create(Buffer packedData)
		{
			return new XpkMain(packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Return the type of the current decompressor
		/// </summary>
		/********************************************************************/
		public override DecompressorType GetDecompressorType()
		{
			return decompressorType;
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
			if (hasPassword)
				throw new DecompressionException();

			uint32_t destOffset = 0;
			XpkDecompressor.State state = null;
			List<Buffer> previousBuffers = new List<Buffer>();
			uint8_t[] firstChunk = null;
			size_t compareLength = 0;

			foreach ((Buffer header, Buffer chunk, uint32_t rawChunkSize, uint8_t chunkType) in ForEachChunk())
			{
				if (OverflowCheck.Sum(destOffset, rawChunkSize) > rawSize)
					throw new DecompressionException();

				if (rawChunkSize == 0)
					continue;

				uint8_t[] destBuffer = null;

				switch (chunkType)
				{
					// Raw chunk (not crunched)
					case 0:
					{
						if (rawChunkSize != chunk.Size())
							throw new DecompressionException();

						destBuffer = chunk.GetData(0, chunk.Size()).ToArray();
						break;
					}

					// Crunched
					case 1:
					{
						try
						{
							destBuffer = new uint8_t[rawChunkSize];

							XpkDecompressor xpkDecompressor = subDecompressor(type, chunk, ref state);
							xpkDecompressor.DecompressImpl(new WrappedArrayBuffer(destBuffer), previousBuffers);
						}
						catch (InvalidFormatException)
						{
							// We should throw a correct error
							throw new DecompressionException();
						}
						break;
					}

					// Last chunk
					case 15:
						break;

					default:
						yield break;
				}

				destOffset += rawChunkSize;

				if (firstChunk == null)
				{
					compareLength = Math.Min(rawChunkSize, 16);
					firstChunk = destBuffer.AsSpan(0, (int)compareLength).ToArray();
				}

				if (chunkType != 15)
					yield return destBuffer;
			}

			if (destOffset != rawSize)
				throw new DecompressionException();

			if ((firstChunk == null) || !packedData.GetData(16, compareLength).SequenceEqual(firstChunk))
				throw new DecompressionException();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the checksum of the header
		/// </summary>
		/********************************************************************/
		private bool HeaderChecksum(Buffer buffer, size_t offset, size_t len)
		{
			if ((len == 0) || (OverflowCheck.Sum(offset, len) > buffer.Size()))
				return false;

			uint8_t tmp = 0;
			for (size_t i = 0; i < len; i++)
				tmp ^= buffer[offset + i];

			return tmp == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the checksum of a chunk of data
		/// </summary>
		/********************************************************************/
		private bool ChunkChecksum(Buffer buffer, size_t offset, size_t len, uint16_t checkValue)
		{
			if ((len == 0) || (OverflowCheck.Sum(offset, len) > buffer.Size()))
				return false;

			uint8_t[] tmp = { 0, 0 };
			for (size_t i = 0; i < len; i++)
				tmp[i & 1] ^= buffer[offset + i];

			return (tmp[0] == (checkValue >> 8)) && (tmp[1] == (checkValue & 0xff));
		}



		/********************************************************************/
		/// <summary>
		/// Take a single chunk of data and depack it
		/// </summary>
		/********************************************************************/
		private IEnumerable<(Buffer header, Buffer chunk, uint32_t rawChunkSize, uint8_t chunkType)> ForEachChunk()
		{
			uint32_t currentOffset = 0;
			bool isLast = false;

			while ((currentOffset < (packedSize + 8)) && !isLast)
			{
				void ReadDualValue(uint32_t offsetShort, uint32_t offsetLong, out uint32_t value)
				{
					if (longHeaders)
						value = packedData.ReadBe32(currentOffset + offsetLong);
					else
						value = packedData.ReadBe16(currentOffset + offsetShort);
				}

				uint32_t chunkHeaderLen = longHeaders ? 12U : 8U;

				if (currentOffset == 0)
				{
					// Return first
					currentOffset = headerSize;
				}
				else
				{
					ReadDualValue(4, 4, out uint32_t tmp);
					tmp = (uint32_t)((tmp + 3) & ~3);

					if (OverflowCheck.Sum(tmp, currentOffset, chunkHeaderLen) > this.packedSize)
						throw new InvalidFormatException();

					currentOffset += chunkHeaderLen + tmp;
				}

				ReadDualValue(4, 4, out uint32_t packedSize);
				ReadDualValue(6, 8, out uint32_t rawSize);

				GenericSubBuffer hdr = new GenericSubBuffer(packedData, currentOffset, chunkHeaderLen);
				GenericSubBuffer chunk = new GenericSubBuffer(packedData, currentOffset + chunkHeaderLen, packedSize);

				if (!HeaderChecksum(hdr, 0, hdr.Size()))
					throw new VerificationException();

				uint16_t hdrCheck = (uint16_t)((hdr[2] << 8) | hdr[3]);
				if ((chunk.Size() != 0) && !ChunkChecksum(chunk, 0, chunk.Size(), hdrCheck))
					throw new VerificationException();

				uint8_t type = packedData.Read8(currentOffset);

				yield return (hdr, chunk, rawSize, type);

				if (type == 15)
					isLast = true;
			}

			if (!isLast)
				throw new InvalidFormatException();
		}
		#endregion
	}
}

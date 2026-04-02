/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors.Xpk;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// PowerPlayer decompressor
	/// </summary>
	internal class PowerPlayerDecompressor : Decompressor
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
		private PowerPlayerDecompressor(Buffer packedData) : base(DecompressorType.PowerPlayer)
		{
			this.packedData = packedData;

			uint32_t hdr = packedData.ReadBe32(0);
			if (!DetectHeader(hdr, 0) || (packedData.Size() < 12))
				throw new InvalidFormatException();

			ver = hdr == Common.Common.FourCC("SFHD") ? 0U : 1U;

			rawSize = packedData.ReadBe32(4);
			if ((rawSize == 0) || (rawSize > GetMaxRawSize()))
				throw new InvalidFormatException();

			packedSize = OverflowCheck.Sum(packedData.ReadBe32(8), 12);
			if ((packedSize == 0) || (packedSize > packedData.Size()) || (packedSize > GetMaxPackedSize()))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr, uint32_t footer)
		{
			return (hdr == Common.Common.FourCC("SFHD")) || (hdr == Common.Common.FourCC("SFCD"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static new Decompressor Create(Buffer packedData, bool exactSizeKnown)
		{
			return new PowerPlayerDecompressor(packedData);
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

			if (rawData.Size() < rawSize)
				throw new DecompressionException();

			GenericSubBuffer subPackedData = new GenericSubBuffer(packedData, 12, packedSize - 12);

			size_t length = LhDecompressor.DecompressLhLib(rawData, subPackedData);
			if (length == 0)
				throw new DecompressionException();

			// Thats all folks!
			if (ver != 0)
			{
				DltaDecode.Decode(rawData, rawData, 0, rawSize);

				if (length != rawSize)
					rawData.GetData(length, rawSize - length).Fill(rawData[length - 1]);
			}
			else
			{
				if (length != rawSize)
					rawData.GetData(length, rawSize - length).Clear();
			}

			yield return outputData;
		}
	}
}

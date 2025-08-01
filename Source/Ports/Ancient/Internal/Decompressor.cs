﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Exceptions;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal
{
    /// <summary>
    /// Main decompression and detection class
    /// </summary>
	internal abstract class Decompressor
	{
		private delegate bool CheckMethod(uint32_t hdr);
		private delegate Decompressor CreateMethod(Buffer packedData);
		private delegate Decompressor CreateMethod2(Buffer packedData, size_t rawSize);

		private struct DecompressorPair
		{
			public CheckMethod First { get; set; }
			public CreateMethod Second { get; set; }
		}

		private static readonly DecompressorPair[] decompressors =
		[
			new DecompressorPair { First = CrunchManiaDecompressor.DetectHeader, Second = CrunchManiaDecompressor.Create },
			new DecompressorPair { First = MmcmpDecompressor.DetectHeader, Second = MmcmpDecompressor.Create },
			new DecompressorPair { First = PowerPackerDecompressor.DetectHeader, Second = PowerPackerDecompressor.Create },
			new DecompressorPair { First = XpkMain.DetectHeader, Second = XpkMain.Create },
			// Putting StoneCracker last since detection can be accidentally be detected instead of correct format
			new DecompressorPair { First = StoneCrackerDecompressor.DetectHeader, Second = StoneCrackerDecompressor.Create }
		];

		private static readonly Dictionary<DecompressorType, CreateMethod2> typeDecompressors = new Dictionary<DecompressorType, CreateMethod2>
		{
			{ DecompressorType.LhLibrary, LhLibraryDecompressor.Create }
		};

		private readonly DecompressorType type;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Decompressor(DecompressorType type)
		{
			this.type = type;
		}



		/********************************************************************/
		/// <summary>
		/// Main entry point
		/// </summary>
		/********************************************************************/
		public static Decompressor Create(Buffer packedData)
		{
			try
			{
				uint32_t hdr = packedData.Size() >= 4 ? packedData.ReadBe32(0) : (uint32_t)packedData.ReadBe16(0) << 16;

				foreach (DecompressorPair it in decompressors)
				{
					if (it.First(hdr))
						return it.Second(packedData);
				}

				throw new InvalidFormatException();
			}
			catch (BufferException)
			{
				throw new InvalidFormatException();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Main entry point
		/// </summary>
		/********************************************************************/
		public static Decompressor Create(Buffer packedData, DecompressorType type, size_t rawSize)
		{
			if (typeDecompressors.TryGetValue(type, out CreateMethod2 method))
				return method(packedData, rawSize);

			throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// Return the type of the current decompressor
		/// </summary>
		/********************************************************************/
		public virtual DecompressorType GetDecompressorType()
		{
			return type;
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the decompressed data
		/// </summary>
		/********************************************************************/
		public abstract size_t GetRawSize();



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public IEnumerable<uint8_t[]> Decompress()
		{
			return DecompressImpl();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t GetMaxPackedSize()
		{
			// 1G should be enough for everyone (this is retro!)
			return 0x4000_0000;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t GetMaxRawSize()
		{
			return 0x4000_0000;
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		protected abstract IEnumerable<uint8_t[]> DecompressImpl();
	}
}

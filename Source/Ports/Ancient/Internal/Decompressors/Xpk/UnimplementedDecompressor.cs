/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// Only to detect missing XPK compressors
	/// </summary>
	internal class UnimplementedDecompressor : XpkDecompressor
	{
		private struct Mode
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Mode(uint32_t fourCC, string name)
			{
				FourCC = fourCC;
				Name = name;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public uint32_t FourCC
			{
				get;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public string Name
			{
				get;
			}
		}

		private readonly uint32_t modeIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private UnimplementedDecompressor(uint32_t hdr, Buffer packedData)
		{
			modeIndex = 0;

			if (!DetectHeaderXpk(hdr))
				throw new InvalidFormatException();

			Mode[] modes = GetModes();
			for (uint32_t i = 0; i < modes.Length; i++)
			{
				if (modes[(int)i].FourCC == hdr)
				{
					modeIndex = i;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			Mode[] modes = GetModes();
			foreach (Mode mode in modes)
			{
				if (mode.FourCC == hdr)
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new UnimplementedDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			// Only identify
			throw new DecompressionException();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Mode[] GetModes()
		{
			// So here are the remaining XPK-libraries info. All there is, all there ever was.
			// (And I'm sure after writing that someone points me to 7 more formats)
			// Unimplemented reasons are as follows:
			// 1. Missing - there is no compressor available anywhere
			// 2. PowerPC only - Amiga OS 4 (or 3.5+) libraries that are nothing but modern compression methods wrapped in AmigaOS
			// 3. Floating point based formats - Fragile formats that require exact 68881/68882 semantics
			// 4. Encryption formats - Encryption formats requiring a proper passphrase
			Mode[] modes =
			{
				new Mode(Common.Common.FourCC("BLFH"), "XPK-BLFH: Blowfish encryption (unimplemented)"),		// Encryption format
				new Mode(Common.Common.FourCC("BZIP"), "XPK-BZIP: Bzip v1 (unimplemented)"),					// PowerPC only
				new Mode(Common.Common.FourCC("CAST"), "XPK-CAST: CAST encryption (unimplemented)"),			// Encryption format
				new Mode(Common.Common.FourCC("ENCO"), "XPK-ENCO: Unsafe encryption (unimplemented)"),			// Encryption format
				new Mode(Common.Common.FourCC("DHUF"), "XPK-DHUF: Huffman compressor (unimplemented)"),			// Missing (All the libraries that exist are broken)
				new Mode(Common.Common.FourCC("DMCB"), "XPK-DMCB: Arithmetic compressor (unimplemented)"),		// Floating point based format
				new Mode(Common.Common.FourCC("DMCD"), "XPK-DMCD: Arithmetic compressor (unimplemented)"),		// Floating point based format
				new Mode(Common.Common.FourCC("DMCI"), "XPK-DMCI: Arithmetic compressor (unimplemented)"),		// Missing
				new Mode(Common.Common.FourCC("DMCU"), "XPK-DMCU: Arithmetic compressor (unimplemented)"),		// Floating point based format
				new Mode(Common.Common.FourCC("FEAL"), "XPK-FEAL: FEAL-N encryption (unimplemented)"),			// Encryption format
				new Mode(Common.Common.FourCC("IDEA"), "XPK-IDEA: IDEA encryption (unimplemented)"),			// Encryption format
				new Mode(Common.Common.FourCC("L2XZ"), "XPK-L2XZ: LZMA2 compressor (unimplemented)"),			// PowerPC only
				new Mode(Common.Common.FourCC("LZ40"), "XPK-LZ40: LZ4 compressor (unimplemented)"),				// PowerPC only
				new Mode(Common.Common.FourCC("LZMA"), "XPK-LZMA: LZMA2 compressor (unimplemented)"),			// PowerPC only
				new Mode(Common.Common.FourCC("NUID"), "XPK-NUID: IDEA encryption + NUKE (unimplemented)"),		// Encryption format
				new Mode(Common.Common.FourCC("SHID"), "XPK-SHID: IDEA encryption + SHRI (unimplemented)"),		// Encryption format
				new Mode(Common.Common.FourCC("TLTA"), "XPK-TLTA: TLTA encoder (unimplemented)")				// Missing, no idea what this is
			};

			return modes;
		}
		#endregion
	}
}

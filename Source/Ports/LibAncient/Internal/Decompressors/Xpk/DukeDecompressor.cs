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
	/// XPK-DUKE decompressor
	/// </summary>
	internal class DukeDecompressor : NukeDecompressor
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private DukeDecompressor(uint32_t hdr, Buffer packedData) : base(hdr, packedData, true)
		{
			if (!DetectHeaderXpk(hdr))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static new bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("DUKE");
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static new XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new DukeDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			base.DecompressImpl(rawData, previousBuffers);

			DltaDecode.Decode(rawData, rawData, 0, rawData.Size());
		}
	}
}

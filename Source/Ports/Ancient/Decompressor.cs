/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Ports.Ancient.Internal;

namespace Polycode.NostalgicPlayer.Ports.Ancient
{
    /// <summary>
    /// Main entry point to the API
    /// </summary>
    public class Decompressor
	{
		private readonly DecompressorImpl impl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Decompressor(Stream crunchedDataStream)
		{
			impl = new DecompressorImpl(crunchedDataStream);
		}



		/********************************************************************/
		/// <summary>
		/// Return the type of the current decompressor
		/// </summary>
		/********************************************************************/
		public DecompressorType GetDecompressorType()
		{
			return impl.decompressor.GetDecompressorType();
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public size_t GetRawSize()
		{
			return impl.decompressor.GetRawSize();
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public IEnumerable<uint8_t[]> Decompress()
		{
			return impl.decompressor.Decompress();
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Ports.Ancient.Internal;

namespace Polycode.NostalgicPlayer.Ports.Ancient
{
	/// <summary>
	/// Main entry point to the API
	/// </summary>
	public class Decompressor : IDisposable
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
		/// Constructor
		/// 
		/// Do only use this, if you know what you're doing and your 100%
		/// sure that the data given is of the given type
		/// </summary>
		/********************************************************************/
		public Decompressor(Stream crunchedDataStream, DecompressorType type, size_t rawSize)
		{
			impl = new DecompressorImpl(crunchedDataStream, type, rawSize);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			impl.Dispose();
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

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal
{
	/// <summary>
	/// Wrapper for decompression and detection class
	/// </summary>
	internal class DecompressorImpl : IDisposable
	{
		private readonly Buffer buffer;
		internal readonly Decompressor decompressor;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecompressorImpl(Stream crunchedDataStream)
		{
			buffer = new StreamBuffer(crunchedDataStream);
			decompressor = Decompressor.Create(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecompressorImpl(Stream crunchedDataStream, DecompressorType type, size_t rawSize)
		{
			buffer = new StreamBuffer(crunchedDataStream);
			decompressor = Decompressor.Create(buffer, type, rawSize);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			buffer.Dispose();
		}
	}
}

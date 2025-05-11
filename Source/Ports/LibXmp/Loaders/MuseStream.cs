/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.IO.Compression;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Helper stream class to load and decompress Muse files
	/// </summary>
	internal class MuseStream : Stream
	{
		private readonly uint32 decompressedLength;
		private readonly Stream wrapperStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MuseStream(Hio f)
		{
			f.Hio_Seek(20, SeekOrigin.Begin);
			decompressedLength = f.Hio_Read32L();

			HioStream hioStream = f.GetStream();
			SliceStream sliceStream = new SliceStream(hioStream, false, 24, f.Hio_Size() - 24);
			ZLibStream zlibStream = new ZLibStream(sliceStream, CompressionMode.Decompress, false);
			wrapperStream = new SeekableStream(zlibStream, false, decompressedLength);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			wrapperStream.Dispose();
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports reading
		/// </summary>
		/********************************************************************/
		public override bool CanRead => wrapperStream.CanRead;



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports writing
		/// </summary>
		/********************************************************************/
		public override bool CanWrite => false;



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => decompressedLength;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => wrapperStream.Position;

			set => wrapperStream.Position = value;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			return wrapperStream.Seek(offset, origin);
		}



		/********************************************************************/
		/// <summary>
		/// Set new length
		/// </summary>
		/********************************************************************/
		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			return wrapperStream.Read(buffer, offset, count);
		}



		/********************************************************************/
		/// <summary>
		/// Write data to the stream
		/// </summary>
		/********************************************************************/
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Flush buffers
		/// </summary>
		/********************************************************************/
		public override void Flush()
		{
			throw new NotSupportedException("Flush not supported");
		}
		#endregion
	}
}

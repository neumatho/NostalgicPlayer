/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This stream wraps another stream to make it seekable
	/// </summary>
	public class SeekableStream : Stream
	{
		private readonly Stream wrapperStream;

		private readonly MemoryStream bufferStream;
		private int bufferIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SeekableStream(Stream wrapperStream)
		{
			this.wrapperStream = wrapperStream;

			bufferStream = new MemoryStream();
			bufferIndex = 0;
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
			bufferStream.Dispose();
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
		public override long Length => wrapperStream.Length;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => bufferIndex;

			set => Seek(value, SeekOrigin.Begin);
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
				{
					bufferIndex = (int)offset;
					break;
				}

				case SeekOrigin.Current:
				{
					bufferIndex += (int)offset;
					break;
				}

				case SeekOrigin.End:
				{
					bufferIndex = (int)(Length + offset);
					break;
				}
			}

			if (bufferIndex < 0)
				bufferIndex = 0;
			else if (bufferIndex > Length)
				bufferIndex = (int)Length;

			return bufferIndex;
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
			if ((bufferIndex + count) > bufferStream.Length)
			{
				// Time to read some more data into the buffer
				int toRead = (int)(bufferIndex - bufferStream.Length + count);

				bufferStream.Seek(0, SeekOrigin.End);
				Helpers.CopyData(wrapperStream, bufferStream, toRead);

				if (bufferIndex >= bufferStream.Length)
					return 0;
			}

			int todo = Math.Min(count, (int)bufferStream.Length - bufferIndex);

			bufferStream.Seek(bufferIndex, SeekOrigin.Begin);
			bufferStream.Read(buffer, offset, todo);

			bufferIndex += todo;

			return todo;
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

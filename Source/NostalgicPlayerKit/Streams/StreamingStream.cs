/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class is used when streaming music
	/// </summary>
	public class StreamingStream : Stream
	{
		private readonly Stream wrapperStream;
		private readonly bool leaveStreamOpen;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StreamingStream(Stream wrapperStream, int icyMetaInt) : this(wrapperStream, false)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StreamingStream(Stream wrapperStream, bool leaveOpen)
		{
			this.wrapperStream = wrapperStream;
			leaveStreamOpen = leaveOpen;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!leaveStreamOpen)
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
		public override bool CanSeek => false;



		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => throw new NotSupportedException("Length not supported");



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => throw new NotSupportedException("Get position not supported");
			set => throw new NotSupportedException("Set position not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
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

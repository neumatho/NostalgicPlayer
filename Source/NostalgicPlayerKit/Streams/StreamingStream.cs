﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class is used when streaming music
	/// </summary>
	public class StreamingStream : Stream
	{
		private const int StreamingTimeout = 30000;

		private Stream wrapperStream;
		private readonly IStreamSeek seeker;

		private long currentPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StreamingStream(Stream wrapperStream, IStreamSeek streamSeek)
		{
			this.wrapperStream = wrapperStream;
			seeker = streamSeek;

			currentPosition = 0;
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
		public override bool CanSeek => (seeker != null) && seeker.CanSeek;



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
			get => currentPosition;

			set
			{
				if (!CanSeek)
					throw new NotSupportedException("Set position not supported");

				wrapperStream?.Dispose();
				wrapperStream = null;

				wrapperStream = seeker.SetPosition(value);
				currentPosition = value;
			}
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
			try
			{
				if (wrapperStream == null)
					return 0;

				int bytesRead = Task.Run(() =>
				{
					using (CancellationTokenSource cancellationToken = new CancellationTokenSource(StreamingTimeout))
					{
						return wrapperStream.ReadAsync(buffer, offset, count, cancellationToken.Token);
					}
				}).Result;

				currentPosition += bytesRead;

				return bytesRead;
			}
			catch(AggregateException ex)
			{
				if (ex.InnerException is TaskCanceledException)
					throw new TimeoutException(Resources.IDS_ERR_STREAMING_TIMEOUT);

				throw;
			}
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

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
	/// This class can be used if you need to "slice" a current stream to make it smaller
	/// </summary>
	public class SliceStream : ReaderStream
	{
		private readonly long startPosition;
		private readonly long length;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SliceStream(Stream wrapperStream, bool leaveOpen, long startPosition, long length) : base(wrapperStream, leaveOpen)
		{
			this.startPosition = startPosition;
			this.length = length;

			// Check arguments
			if ((startPosition + length) > wrapperStream.Length)
				throw new ArgumentOutOfRangeException("Either startPosition or length is outside of stream size");

			// Make sure the stream is inside the range
			wrapperStream.Seek(startPosition, SeekOrigin.Begin);
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => length;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => wrapperStream.Position - startPosition;

			set
			{
				if ((value < 0) || (value > length))
					throw new ArgumentOutOfRangeException(nameof(value), "Position is outside of stream");

				wrapperStream.Position = startPosition + value;
				EndOfStream = value > length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			long newPos = 0;

			switch (origin)
			{
				case SeekOrigin.Begin:
				{
					newPos = offset;
					break;
				}

				case SeekOrigin.Current:
				{
					newPos = Position + offset;
					break;
				}

				case SeekOrigin.End:
				{
					newPos = length + offset;
					break;
				}
			}

			if ((newPos < 0) || (newPos > length))
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset is outside of stream");

			wrapperStream.Seek(startPosition + newPos, SeekOrigin.Begin);
			EndOfStream = newPos > length;

			return newPos;
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = wrapperStream.Read(buffer, offset, (int)Math.Min(count, Length - Position));
			EndOfStream = read < count;

			return read;
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers
{
	/// <summary>
	/// A buffer like implementation based on a stream
	/// </summary>
	internal class StreamBuffer : Buffer, IDisposable
	{
		private readonly Stream stream;
		private readonly size_t startOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StreamBuffer(Stream dataStream) : this(dataStream, 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private StreamBuffer(Stream dataStream, size_t startOffset)
		{
			stream = dataStream;
			this.startOffset = startOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			stream.Dispose();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the buffer
		/// </summary>
		/********************************************************************/
		public override size_t Size()
		{
			return (size_t)stream.Length - startOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bytes at the offset given and return them
		/// </summary>
		/********************************************************************/
		public override Span<uint8_t> GetData(size_t offset, size_t count)
		{
			if ((offset + startOffset) != (size_t)stream.Position)
				stream.Seek((long)(offset + startOffset), SeekOrigin.Begin);

			size_t todo = Math.Min(count, (size_t)(stream.Length - stream.Position));

			uint8_t[] buffer = new uint8_t[todo];
			stream.ReadExactly(buffer, 0, (int)todo);

			return buffer.AsSpan();
		}



		/********************************************************************/
		/// <summary>
		/// Set a value in the internal buffer
		/// </summary>
		/********************************************************************/
		public override void SetData(size_t offset, uint8_t value)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current buffer and make it start at the given offset
		/// </summary>
		/********************************************************************/
		protected override Buffer CloneBuffer(size_t offset)
		{
			return new StreamBuffer(stream, offset);
		}
		#endregion
	}
}

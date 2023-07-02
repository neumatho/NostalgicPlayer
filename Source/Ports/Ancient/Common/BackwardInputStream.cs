/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Read from a buffer backwards
	/// </summary>
	internal class BackwardInputStream : IInputStream
	{
		private readonly Buffer buffer;

		private size_t currentOffset;
		private readonly size_t endOffset;
		private readonly bool allowOverrun;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BackwardInputStream(Buffer buffer, size_t startOffset, size_t endOffset, bool allowOverrun = false)
		{
			this.buffer = buffer;
			currentOffset = endOffset;
			this.endOffset = startOffset;
			this.allowOverrun = allowOverrun;

			if ((currentOffset < this.endOffset) || (currentOffset > buffer.Size()) || (this.endOffset > buffer.Size()))
				throw new DecompressionException();
		}

		#region IInputStream implementation
		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public size_t GetOffset()
		{
			return currentOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Read a single byte
		/// </summary>
		/********************************************************************/
		public uint8_t ReadByte()
		{
			if (currentOffset <= endOffset)
			{
				if (allowOverrun)
				{
					--currentOffset;
					return 0;
				}

				throw new DecompressionException();
			}

			uint8_t ret = buffer[--currentOffset];

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Consume the given number of bytes
		/// </summary>
		/********************************************************************/
		public Span<uint8_t> Consume(size_t bytes, uint8_t[] buffer)
		{
			if (currentOffset < OverflowCheck.Sum(endOffset, bytes))
			{
				if (allowOverrun && (buffer != null))
				{
					for (size_t i = bytes; i != 0; i--)
					{
						buffer[i - 1] = (currentOffset > endOffset) ? this.buffer[currentOffset - 1] : (uint8_t)0;
						--currentOffset;
					}

					return buffer.AsSpan();
				}

				throw new DecompressionException();
			}

			currentOffset -= bytes;

			return this.buffer.GetData(currentOffset, bytes);
		}
		#endregion
	}
}

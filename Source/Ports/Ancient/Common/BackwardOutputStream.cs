/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Write to a buffer backwards
	/// </summary>
	internal class BackwardOutputStream : IOutputStream
	{
		private readonly Buffer buffer;

		private readonly size_t startOffset;
		private size_t currentOffset;
		private readonly size_t endOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BackwardOutputStream(Buffer buffer, size_t startOffset, size_t endOffset)
		{
			this.buffer = buffer;

			this.startOffset = startOffset;
			currentOffset = endOffset;
			this.endOffset = endOffset;

			if ((this.startOffset > this.endOffset) || (currentOffset > buffer.Size()) || (this.endOffset > buffer.Size()))
				throw new DecompressionException();
		}

		#region IOutputStream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		/********************************************************************/
		public bool Eof => currentOffset == startOffset;



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
		/// Write a single byte to the output
		/// </summary>
		/********************************************************************/
		public void WriteByte(uint8_t value)
		{
			if (currentOffset <= startOffset)
				throw new DecompressionException();

			buffer[--currentOffset] = value;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public uint8_t Copy(size_t distance, size_t count)
		{
			if ((distance == 0) || (OverflowCheck.Sum(startOffset, count) > currentOffset) || (OverflowCheck.Sum(currentOffset, distance) > endOffset))
				throw new DecompressionException();

			uint8_t ret = 0;

			for (size_t i = 0; i < count; i++, --currentOffset)
				ret = buffer[currentOffset - 1] = buffer[currentOffset + distance - 1];

			return ret;
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Base class for all forward output streams
	/// </summary>
	internal abstract class ForwardOutputStreamBase : IOutputStream
	{
		protected Buffer buffer;

		protected readonly size_t startOffset;
		protected size_t currentOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ForwardOutputStreamBase(Buffer buffer, size_t startOffset)
		{
			this.buffer = buffer;

			this.startOffset = startOffset;
			currentOffset = startOffset;
		}

		#region IOutputStream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		/********************************************************************/
		public abstract bool Eof { get; }



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
			EnsureSize(currentOffset + 1);
			buffer[currentOffset++] = value;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public uint8_t Copy(size_t distance, size_t count)
		{
			EnsureSize(OverflowCheck.Sum(currentOffset, count));
			if ((distance == 0) || (OverflowCheck.Sum(startOffset, distance) > currentOffset))
				throw new DecompressionException();

			uint8_t ret = 0;

			for (size_t i = 0; i < count; i++, currentOffset++)
				ret = buffer[currentOffset] = buffer[currentOffset - distance];

			return ret;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public uint8_t Copy(size_t distance, size_t count, List<Buffer> prevBuffer)
		{
			EnsureSize(OverflowCheck.Sum(currentOffset, count));
			if (distance == 0)
				throw new DecompressionException();

			size_t prevCount = 0;
			uint8_t ret = 0;

			if (OverflowCheck.Sum(startOffset, distance) > currentOffset)
			{
				size_t prevSize = (size_t)prevBuffer.Sum(b => (uint32_t)b.Size());
				if ((startOffset + distance) > (currentOffset + prevSize))
					throw new DecompressionException();

				size_t prevDist = startOffset + distance - currentOffset;
				prevCount = Math.Min(count, prevDist);

				Buffer prev = null;
				size_t len = 0;

				int bufIndex;
				for (bufIndex = 0; bufIndex < prevBuffer.Count; bufIndex++)
				{
					Buffer b = prevBuffer[bufIndex];
					len += b.Size();

					if ((prevSize - prevDist) < len)
					{
						prev = b;
						break;
					}
				}

				if (prev == null)
					throw new DecompressionException();

				size_t total = prevCount;
				size_t index = prevSize - prevDist - (len - prev.Size());

				for (;;)
				{
					size_t todo = Math.Min(total, prev.Size() - index);

					for (size_t i = 0; i < todo; i++, currentOffset++)
						ret = buffer[currentOffset] = prev[index + i];

					total -= todo;
					if (total == 0)
						break;

					if (bufIndex == prevBuffer.Count)
						throw new DecompressionException();

					prev = prevBuffer[++bufIndex];
					index = 0;
				}
			}

			for (size_t i = prevCount; i < count; i++, currentOffset++)
				ret = buffer[currentOffset] = buffer[currentOffset - distance];

			return ret;
		}

		#region Override methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void EnsureSize(size_t offset);
		#endregion
	}
}

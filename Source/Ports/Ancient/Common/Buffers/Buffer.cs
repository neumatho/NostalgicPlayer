/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers
{
	/// <summary>
	/// Base class for all buffer implementations
	/// </summary>
	internal abstract class Buffer
	{
		/********************************************************************/
		/// <summary>
		/// Get the byte at the index given
		/// </summary>
		/********************************************************************/
		public uint8_t this[size_t i]
		{
			get
			{
				if (i >= Size())
					throw new OutOfBoundsException();

				Span<uint8_t> data = GetData(i, 1);

				return data[0];
			}

			set
			{
				if (i >= Size())
					throw new OutOfBoundsException();

				SetData(i, value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a new buffer based on the current one starting at the
		/// given offset
		/// </summary>
		/********************************************************************/
		public Buffer GetNewBuffer(size_t offset)
		{
			return CloneBuffer(offset);
		}



		/********************************************************************/
		/// <summary>
		/// Read a 32-bit integer in big-endian
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBe32(size_t offset)
		{
			if (OverflowCheck.Sum(offset, 4) > Size())
				throw new OutOfBoundsException();

			Span<uint8_t> data = GetData(offset, 4);

			return (uint32_t)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Read a 16-bit integer in big-endian
		/// </summary>
		/********************************************************************/
		public uint16_t ReadBe16(size_t offset)
		{
			if (OverflowCheck.Sum(offset, 2) > Size())
				throw new OutOfBoundsException();

			Span<uint8_t> data = GetData(offset, 2);

			return (uint16_t)((data[0] << 8) | data[1]);
		}



		/********************************************************************/
		/// <summary>
		/// Read a 32-bit integer in little-endian
		/// </summary>
		/********************************************************************/
		public uint32_t ReadLe32(size_t offset)
		{
			if (OverflowCheck.Sum(offset, 4) > Size())
				throw new OutOfBoundsException();

			Span<uint8_t> data = GetData(offset, 4);

			return (uint32_t)((data[3] << 24) | (data[2] << 16) | (data[1] << 8) | data[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Read a 16-bit integer in little-endian
		/// </summary>
		/********************************************************************/
		public uint16_t ReadLe16(size_t offset)
		{
			if (OverflowCheck.Sum(offset, 2) > Size())
				throw new OutOfBoundsException();

			Span<uint8_t> data = GetData(offset, 2);

			return (uint16_t)((data[1] << 8) | data[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Read a single byte
		/// </summary>
		/********************************************************************/
		public uint8_t Read8(size_t offset)
		{
			if (offset >= Size())
				throw new OutOfBoundsException();

			Span<uint8_t> data = GetData(offset, 1);

			return data[0];
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the buffer
		/// </summary>
		/********************************************************************/
		public abstract size_t Size();



		/********************************************************************/
		/// <summary>
		/// Resize the current buffer
		/// </summary>
		/********************************************************************/
		public virtual void Resize(size_t newSize)
		{
			throw new Exceptions.InvalidOperationException();
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bytes at the offset given and return them
		/// </summary>
		/********************************************************************/
		public abstract Span<uint8_t> GetData(size_t offset, size_t count);



		/********************************************************************/
		/// <summary>
		/// Set a value in the internal buffer
		/// </summary>
		/********************************************************************/
		public abstract void SetData(size_t offset, uint8_t value);



		/********************************************************************/
		/// <summary>
		/// Clone the current buffer and make it start at the given offset
		/// </summary>
		/********************************************************************/
		protected abstract Buffer CloneBuffer(size_t offset);
		#endregion
	}
}

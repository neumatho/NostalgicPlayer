/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers
{
	/// <summary>
	/// 
	/// </summary>
	internal class MemoryBuffer : Buffer
	{
		private uint8_t[] data;
		private size_t size;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MemoryBuffer(size_t size)
		{
			data = new uint8_t[size];
			this.size = size;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the buffer
		/// </summary>
		/********************************************************************/
		public override size_t Size()
		{
			return size;
		}



		/********************************************************************/
		/// <summary>
		/// Resize the current buffer
		/// </summary>
		/********************************************************************/
		public override void Resize(size_t newSize)
		{
			Array.Resize(ref data, (int)newSize);
			size = newSize;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bytes at the offset given and return them
		/// </summary>
		/********************************************************************/
		public override Span<uint8_t> GetData(size_t offset, size_t count)
		{
			return data.AsSpan((int)offset, (int)count);
		}



		/********************************************************************/
		/// <summary>
		/// Set a value in the internal buffer
		/// </summary>
		/********************************************************************/
		public override void SetData(size_t offset, uint8_t value)
		{
			data[(int)offset] = value;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current buffer and make it start at the given offset
		/// </summary>
		/********************************************************************/
		protected override Buffer CloneBuffer(size_t offset)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}

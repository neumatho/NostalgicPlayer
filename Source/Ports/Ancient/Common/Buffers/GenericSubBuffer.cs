/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers
{
	/// <summary>
	/// Splice buffer class
	/// </summary>
	internal class GenericSubBuffer : Buffer
	{
		private readonly Buffer baseBuffer;
		private readonly size_t start;
		private readonly size_t length;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public GenericSubBuffer(Buffer baseBuffer, size_t start, size_t length)
		{
			this.baseBuffer = baseBuffer;
			this.start = start;
			this.length = length;

			// If the sub-buffer is invalid, we set both start and length to 0
			// TODO: Check if invalid-empty buffers are still required
			if (OverflowCheck.Sum(start, length) > this.baseBuffer.Size())
			{
				this.start = 0;
				this.length = 0;
			}
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the buffer
		/// </summary>
		/********************************************************************/
		public override size_t Size()
		{
			return length;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bytes at the offset given and return them
		/// </summary>
		/********************************************************************/
		public override Span<uint8_t> GetData(size_t offset, size_t count)
		{
			return baseBuffer.GetData(start + offset, count);
		}



		/********************************************************************/
		/// <summary>
		/// Set a value in the internal buffer
		/// </summary>
		/********************************************************************/
		public override void SetData(size_t offset, uint8_t value)
		{
			baseBuffer.SetData(start + offset, value);
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

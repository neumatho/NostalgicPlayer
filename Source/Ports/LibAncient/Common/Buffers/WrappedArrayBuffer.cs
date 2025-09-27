/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers
{
	/// <summary>
	/// Wraps an array as a buffer
	/// </summary>
	internal class WrappedArrayBuffer : Buffer
	{
		private readonly uint8_t[] refData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public WrappedArrayBuffer(uint8_t[] refData)
		{
			this.refData = refData;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the buffer
		/// </summary>
		/********************************************************************/
		public override size_t Size()
		{
			return (size_t)refData.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bytes at the offset given and return them
		/// </summary>
		/********************************************************************/
		public override Span<uint8_t> GetData(size_t offset, size_t count)
		{
			return refData.AsSpan((int)offset, (int)count);
		}



		/********************************************************************/
		/// <summary>
		/// Set a value in the internal buffer
		/// </summary>
		/********************************************************************/
		public override void SetData(size_t offset, uint8_t value)
		{
			refData[offset] = value;
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

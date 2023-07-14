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
	/// Write to a buffer forward
	/// </summary>
	internal class ForwardOutputStream : ForwardOutputStreamBase
	{
		private readonly size_t endOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ForwardOutputStream(Buffer buffer, size_t startOffset, size_t endOffset) : base(buffer, startOffset)
		{
			this.endOffset = endOffset;

			if ((this.startOffset > this.endOffset) || (this.endOffset > this.buffer.Size()))
				throw new DecompressionException();
		}

		#region IOutputStream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		/********************************************************************/
		public override bool Eof => currentOffset == endOffset;
		#endregion

		#region Override methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void EnsureSize(size_t offset)
		{
			if (offset > endOffset)
				throw new DecompressionException();
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common
{
	/// <summary>
	/// Write to a buffer forward
	/// </summary>
	internal class AutoExpandingForwardOutputStream : ForwardOutputStreamBase, IDisposable
	{
		private const size_t Advance = 65536;

		private bool hasExpanded = false;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AutoExpandingForwardOutputStream(Buffer buffer) : base(buffer, 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Make sure the buffer has the right size
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			if (hasExpanded && (currentOffset != buffer.Size()))
				buffer.Resize(currentOffset);
		}

		#region IOutputStream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		/********************************************************************/
		public override bool Eof => false;
		#endregion

		#region Override methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void EnsureSize(size_t offset)
		{
			if (offset > Internal.Decompressor.GetMaxRawSize())
				throw new DecompressionException();

			if (offset > buffer.Size())
			{
				buffer.Resize(offset + Advance);
				hasExpanded = true;
			}
		}
		#endregion
	}
}

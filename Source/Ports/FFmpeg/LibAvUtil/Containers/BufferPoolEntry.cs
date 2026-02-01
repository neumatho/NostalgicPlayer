/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class BufferPoolEntry : IOpaque
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Data;

		/// <summary>
		/// Backups of the original opaque/free of the AVBuffer corresponding to
		/// data. They will be used to free the buffer when the pool is freed
		/// </summary>
		public IOpaque Opaque;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.Buffer_Free_Delegate Free;

		/// <summary>
		/// 
		/// </summary>
		public AvBufferPool Pool;

		/// <summary>
		/// 
		/// </summary>
		public BufferPoolEntry Next;

		/// <summary>
		/// An AVBuffer structure to (re)use as AVBuffer for subsequent uses
		/// of this BufferPoolEntry
		/// </summary>
		public readonly AvBuffer Buffer = new AvBuffer();
	}
}

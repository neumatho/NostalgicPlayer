/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvBufferPool
	{
		/// <summary>
		/// 
		/// </summary>
		internal AvMutex Mutex;

		/// <summary>
		/// 
		/// </summary>
		internal BufferPoolEntry Pool;

		/// <summary>
		/// This is used to track when the pool is to be freed.
		/// The pointer to the pool itself held by the caller is considered to
		/// be one reference. Each buffer requested by the caller increases refcount
		/// by one, returning the buffer to the pool decreases it by one.
		/// refcount reaches zero when the buffer has been uninited AND all the
		/// buffers have been released, then it's safe to free the pool and all
		/// the buffers in it
		/// </summary>
		internal c_uint RefCount;

		/// <summary>
		/// 
		/// </summary>
		internal size_t Size;

		/// <summary>
		/// 
		/// </summary>
		internal IOpaque Opaque;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Alloc_Delegate Alloc;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Alloc2_Delegate Alloc2;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Pool_Free_Delegate Pool_Free;
	}
}

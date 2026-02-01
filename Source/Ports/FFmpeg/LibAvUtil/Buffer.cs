/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Buffer
	{
		/********************************************************************/
		/// <summary>
		/// Create an AVBuffer from an existing array.
		///
		/// If this function is successful, data is owned by the AVBuffer.
		/// The caller may only access data through the returned AVBufferRef
		/// and references derived from it.
		/// 
		/// If this function fails, data is left untouched
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Create(CPointer<uint8_t> data, size_t size, UtilFunc.Buffer_Free_Delegate free, IOpaque opaque, AvBufferFlag flags)//XX 55
		{
			return Av_Buffer_Create(new DataBufferContext(data, size), free, opaque, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Create an AVBuffer from an existing array.
		///
		/// If this function is successful, data is owned by the AVBuffer.
		/// The caller may only access data through the returned AVBufferRef
		/// and references derived from it.
		/// 
		/// If this function fails, data is left untouched
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Create(IDataContext data, UtilFunc.Buffer_Free_Delegate free, IOpaque opaque, AvBufferFlag flags)//XX 55
		{
			AvBuffer buf = Mem.Av_MAlloczObj<AvBuffer>();

			if (buf == null)
				return null;

			AvBufferRef ret = Buffer_Create(buf, data, free, opaque, flags);

			if (ret == null)
			{
				Mem.Av_Free(buf);

				return null;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Default free callback, which calls av_free() on the buffer data.
		/// This function is meant to be passed to av_buffer_create(), not
		/// called directly
		/// </summary>
		/********************************************************************/
		public static void Av_Buffer_Default_Free(IOpaque opaque, IDataContext data)//XX 72
		{
			Mem.Av_Free(data);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate an AVBuffer of the given size using av_malloc()
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Alloc(size_t size)//XX 77
		{
			CPointer<uint8_t> data = Mem.Av_MAlloc<uint8_t>(size);

			if (data.IsNull)
				return null;

			AvBufferRef ret = Av_Buffer_Create(data, size, Av_Buffer_Default_Free, null, AvBufferFlag.None);

			if (ret == null)
				Mem.Av_FreeP(ref data);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate an AVBuffer of the given size using av_malloc()
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Alloc(IDataContext data)
		{
			AvBufferRef ret = Av_Buffer_Create(data, Av_Buffer_Default_Free, null, AvBufferFlag.None);

			if (ret == null)
				Mem.Av_FreeP(ref data);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Same as av_buffer_alloc(), except the returned buffer will be
		/// initialized to zero
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Allocz(size_t size)//XX 93
		{
			AvBufferRef ret = Av_Buffer_Alloc(size);

			if (ret == null)
				return null;

			CMemory.memset<uint8_t>(((DataBufferContext)ret.Data).Data, 0, size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new reference to an AVBuffer
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Ref(AvBufferRef buf)//XX 103
		{
			AvBufferRef ret = Mem.Av_MAlloczObj<AvBufferRef>();

			if (ret == null)
				return null;

			buf.CopyTo(ret);

			StdAtomic.Atomic_Fetch_Add(ref buf.Buffer.RefCount, 1);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Free a given reference and automatically free the buffer if there
		/// are no more references to it
		/// </summary>
		/********************************************************************/
		public static void Av_Buffer_Unref(ref AvBufferRef buf)//XX 139
		{
			if (buf == null)
				return;

			Buffer_Replace(ref buf);
		}



		/********************************************************************/
		/// <summary>
		/// Return 1 if the caller may write to the data referred to by buf
		/// (which is true if and only if buf is the only reference to the
		/// underlying AVBuffer). Return 0 otherwise.
		/// A positive answer is valid until av_buffer_ref() is called on buf
		/// </summary>
		/********************************************************************/
		public static c_int Av_Buffer_Is_Writable(AvBufferRef buf)//XX 147
		{
			if ((buf.Buffer.Flags & AvBufferFlag.ReadOnly) != 0)
				return 0;

			return StdAtomic.Atomic_Load(ref buf.Buffer.RefCount) == 1 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Create a writable reference from a given buffer reference,
		/// avoiding data copy if possible
		/// </summary>
		/********************************************************************/
		public static c_int Av_Buffer_Make_Writable(ref AvBufferRef pBuf)//XX 165
		{
			AvBufferRef buf = pBuf;

			if (Av_Buffer_Is_Writable(buf) != 0)
				return 0;

			AvBufferRef newBuf = Av_Buffer_Alloc(buf.Data.MakeDeepClone());

			if (newBuf == null)
				return Error.ENOMEM;

//			CMemory.memcpy(newBuf.Data, buf.Data, buf.Size);

			Buffer_Replace(ref pBuf, ref newBuf);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reallocate a given buffer.
		///
		/// Note the buffer is actually reallocated with av_realloc() only
		/// if it was initially allocated through av_buffer_realloc(NULL)
		/// and there is only one reference to it (i.e. the one passed to
		/// this function). In all other cases a new buffer is allocated and
		/// the data is copied
		/// </summary>
		/********************************************************************/
		public static c_int Av_Buffer_Realloc(ref AvBufferRef pBuf, size_t size)//XX 183
		{
			AvBufferRef buf = pBuf;

			if (buf == null)
			{
				// Allocate a new buffer with av_realloc(), so it will be reallocatable
				// later
				CPointer<uint8_t> data = Mem.Av_Realloc<uint8_t>(null, size);

				if (data.IsNull)
					return Error.ENOMEM;

				buf = Av_Buffer_Create(data, size, Av_Buffer_Default_Free, null, AvBufferFlag.None);

				if (buf == null)
				{
					Mem.Av_FreeP(ref data);

					return Error.ENOMEM;
				}

				buf.Buffer.Flags_Internal |= BufferFlag.Reallocatable;
				pBuf = buf;

				return 0;
			}

			DataBufferContext dataContext = buf.Data as DataBufferContext;
			DataBufferContext bufferDataContext = buf.Buffer.Data as DataBufferContext;

			if ((dataContext == null) || (bufferDataContext == null))
				return Error.InvalidData;

			if (dataContext.Size == size)
				return 0;

			if (((buf.Buffer.Flags_Internal & BufferFlag.Reallocatable) == 0) || ((Av_Buffer_Is_Writable(buf) == 0) || (dataContext.Data != bufferDataContext.Data)))
			{
				// Cannot realloc, allocate a new reallocable buffer and copy data
				AvBufferRef @new = null;

				c_int ret = Av_Buffer_Realloc(ref @new, size);

				if (ret < 0)
					return ret;

				CMemory.memcpy(((DataBufferContext)@new.Data).Data, dataContext.Data, Macros.FFMin(size, dataContext.Size));

				Buffer_Replace(ref pBuf, ref @new);

				return 0;
			}

			CPointer<uint8_t> tmp = Mem.Av_Realloc(bufferDataContext.Data, size);

			if (tmp.IsNull)
				return Error.ENOMEM;

			buf.Buffer.Data = buf.Data = new DataBufferContext(tmp, size);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Ensure dst refers to the same data as src.
		///
		/// When *dst is already equivalent to src, do nothing. Otherwise
		/// unreference dst and replace it with a new reference to src
		/// </summary>
		/********************************************************************/
		public static c_int Av_Buffer_Replace(ref AvBufferRef pDst, AvBufferRef src)//XX 233
		{
			AvBufferRef dst = pDst;

			if (src == null)
			{
				Av_Buffer_Unref(ref pDst);

				return 0;
			}

			if ((dst != null) && (dst.Buffer == src.Buffer))
			{
				// Make sure the data pointers match
				dst.Data = src.Data;

				return 0;
			}

			AvBufferRef tmp = Av_Buffer_Ref(src);

			if (tmp == null)
				return Error.ENOMEM;

			Av_Buffer_Unref(ref pDst);
			pDst = tmp;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate and initialize a buffer pool
		/// </summary>
		/********************************************************************/
		public static AvBufferPool Av_Buffer_Pool_Init(size_t size, UtilFunc.Alloc_Delegate alloc)//XX 283
		{
			AvBufferPool pool = Mem.Av_MAlloczObj<AvBufferPool>();

			if (pool == null)
				return null;

			if (CThread.pthread_mutex_init(out pool.Mutex) != 0)
			{
				Mem.Av_Free(pool);

				return null;
			}

			pool.Size = size;
			pool.Alloc = alloc != null ? alloc : Av_Buffer_Alloc;

			StdAtomic.Atomic_Init<c_uint>(ref pool.RefCount, 1);

			return pool;
		}



		/********************************************************************/
		/// <summary>
		/// Mark the pool as being available for freeing. It will actually
		/// be freed only once all the allocated buffers associated with the
		/// pool are released. Thus it is safe to call this function while
		/// some of the allocated buffers are still in use
		/// </summary>
		/********************************************************************/
		public static void Av_Buffer_Pool_Uninit(ref AvBufferPool pPool)//XX 328
		{
			if (pPool == null)
				return;

			AvBufferPool pool = pPool;
			pPool = null;

			CThread.pthread_mutex_lock(pool.Mutex);

			Buffer_Pool_Flush(pool);

			CThread.pthread_mutex_unlock(pool.Mutex);

			if (StdAtomic.Atomic_Fetch_Sub(ref pool.RefCount, 1) == 1)
				Buffer_Pool_Free(pool);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a new AVBuffer, reusing an old buffer from the pool when
		/// available. This function may be called simultaneously from
		/// multiple threads
		/// </summary>
		/********************************************************************/
		public static AvBufferRef Av_Buffer_Pool_Get(AvBufferPool pool)//XX 390
		{
			AvBufferRef ret;

			CThread.pthread_mutex_lock(pool.Mutex);

			BufferPoolEntry buf = pool.Pool;

			if (buf != null)
			{
				buf.Buffer.Clear();

				ret = Buffer_Create(buf.Buffer, new DataBufferContext(buf.Data, pool.Size), Pool_Release_Buffer, buf, AvBufferFlag.None);

				if (ret != null)
				{
					pool.Pool = buf.Next;
					buf.Next = null;
					buf.Buffer.Flags_Internal |= BufferFlag.NoFree;
				}
			}
			else
				ret = Pool_Alloc_Buffer(pool);

			CThread.pthread_mutex_unlock(pool.Mutex);

			if (ret != null)
				StdAtomic.Atomic_Fetch_Add(ref pool.RefCount, 1);

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvBufferRef Buffer_Create(AvBuffer buf, IDataContext data, UtilFunc.Buffer_Free_Delegate free, IOpaque opaque, AvBufferFlag flags)//XX 29
		{
			buf.Data = data;
			buf.Free = free != null ? free : Av_Buffer_Default_Free;
			buf.Opaque = opaque;

			StdAtomic.Atomic_Init(ref buf.RefCount, 1U);

			buf.Flags = flags;

			AvBufferRef @ref = Mem.Av_MAlloczObj<AvBufferRef>();

			if (@ref == null)
				return null;

			@ref.Buffer = buf;
			@ref.Data = data;

			return @ref;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Buffer_Replace(ref AvBufferRef dst)
		{
			AvBufferRef src = null;

			Buffer_Replace(ref dst, false, ref src);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Buffer_Replace(ref AvBufferRef dst, ref AvBufferRef src)
		{
			Buffer_Replace(ref dst, true, ref src);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Buffer_Replace(ref AvBufferRef dst, bool hasSrc, ref AvBufferRef src)//XX 117
		{
			AvBuffer b = dst.Buffer;

			if (hasSrc)
			{
				src.CopyTo(dst);
				Mem.Av_FreeP(ref src);
			}
			else
				Mem.Av_FreeP(ref dst);

			if (StdAtomic.Atomic_Fetch_Sub(ref b.RefCount, 1) == 1)
			{
				// b->free below might already free the structure containing *b,
				// so we have to read the flag now to avoid use-after-free
				bool free_AvBuffer = b.Flags_Internal.HasFlag(BufferFlag.NoFree);
				b.Free(b.Opaque, b.Data);

				if (free_AvBuffer)
					Mem.Av_Free(b);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Buffer_Pool_Flush(AvBufferPool pool)//XX 302
		{
			while (pool.Pool != null)
			{
				BufferPoolEntry buf = pool.Pool;
				pool.Pool = buf.Next;

				buf.Free(buf.Opaque, new DataBufferContext(buf.Data, (size_t)buf.Data.Length));
				Mem.Av_FreeP(ref buf);
			}
		}



		/********************************************************************/
		/// <summary>
		/// This function gets called when the pool has been uninited and
		/// all the buffers returned to it
		/// </summary>
		/********************************************************************/
		private static void Buffer_Pool_Free(AvBufferPool pool)//XX 317
		{
			Buffer_Pool_Flush(pool);
			CThread.pthread_mutex_destroy(pool.Mutex);

			if (pool.Pool_Free != null)
				pool.Pool_Free(pool.Opaque);

			Mem.Av_FreeP(ref pool);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Pool_Release_Buffer(IOpaque opaque, IDataContext data)//XX 345
		{
			BufferPoolEntry buf = (BufferPoolEntry)opaque;
			AvBufferPool pool = buf.Pool;

			CThread.pthread_mutex_lock(pool.Mutex);

			buf.Next = pool.Pool;
			pool.Pool = buf;

			CThread.pthread_mutex_unlock(pool.Mutex);

			if (StdAtomic.Atomic_Fetch_Sub(ref pool.RefCount, 1) == 1)
				Buffer_Pool_Free(pool);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a new buffer and override its free() callback so that
		/// it is returned to the pool on free
		/// </summary>
		/********************************************************************/
		private static AvBufferRef Pool_Alloc_Buffer(AvBufferPool pool)//XX 361
		{
			AvBufferRef ret = pool.Alloc2 != null ? pool.Alloc2(pool.Opaque, pool.Size) : pool.Alloc(pool.Size);

			if (ret == null)
				return null;

			BufferPoolEntry buf = Mem.Av_MAlloczObj<BufferPoolEntry>();

			if (buf == null)
			{
				Av_Buffer_Unref(ref ret);

				return null;
			}

			DataBufferContext dataBuffer = (DataBufferContext)ret.Buffer.Data;

			buf.Data = dataBuffer.Data;
			buf.Opaque = ret.Buffer.Opaque;
			buf.Free = ret.Buffer.Free;
			buf.Pool = pool;

			ret.Buffer.Opaque = buf;
			ret.Buffer.Free = Pool_Release_Buffer;

			return ret;
		}
		#endregion
	}
}

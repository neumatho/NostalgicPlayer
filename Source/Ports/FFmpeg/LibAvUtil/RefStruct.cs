/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class RefStruct
	{
		/********************************************************************/
		/// <summary>
		/// Mark the pool as being available for freeing. It will actually be
		/// freed only once all the allocated buffers associated with the
		/// pool are released. Thus it is safe to call this function while
		/// some of the allocated buffers are still in use.
		///
		/// It is illegal to try to get a new entry after this function has
		/// been called
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_RefStruct_Pool_Uninit(ref AvRefStructPool poolP)
		{
			Av_RefStruct_Unref(ref poolP);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Av_RefStruct_Alloc_Ext<T>(AvRefStructFlag flags, IOpaque opaque, UtilFunc.Free2_Cb_Delegate free_Cb) where T : class, IRefCountData, new()
		{
			return Av_RefStruct_Alloc_Ext_C<T>(flags, new AvRefStructOpaque { Nc = opaque }, free_Cb);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AvRefStructPool Av_RefStruct_Pool_Alloc_Ext<T>(AvRefStructPoolFlag flags, IOpaque opaque, UtilFunc.Init_Cb_Delegate init_Cb, UtilFunc.Reset_Cb_Delegate reset_Cb, UtilFunc.Free_Entry_Delegate free_Entry_Cb, UtilFunc.Free_Cb_Delegate free_Cb) where T : class, IRefCountData, new()
		{
			return Av_RefStruct_Pool_Alloc_Ext_C<T>(flags, new AvRefStructOpaque { Nc = opaque }, init_Cb, reset_Cb, free_Entry_Cb, free_Cb);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a refcounted object of usable size `size` managed via
		/// the RefStruct API.
		///
		/// By default (in the absence of flags to the contrary), the
		/// returned object is initially zeroed
		/// </summary>
		/********************************************************************/
		public static T Av_RefStruct_Alloc_Ext_C<T>(AvRefStructFlag flags, AvRefStructOpaque opaque, UtilFunc.Free2_Cb_Delegate free_Cb) where T : class, IRefCountData, new()//XX 102
		{
			T buf = Mem.Av_MAllocObj<T>();

			if (buf == null)
				return null;

			RefCount_Init(buf, opaque, free_Cb);
			IRefCountData obj = Get_UserData(buf);

			if ((flags & AvRefStructFlag.No_Zeroing) == 0)
				obj.Clear();

			return (T)obj;
		}



		/********************************************************************/
		/// <summary>
		/// Decrement the reference count of the underlying object and
		/// automatically free the object if there are no more references to
		/// it.
		///
		/// `*objp == NULL` is legal and a no-op
		/// </summary>
		/********************************************************************/
		public static void Av_RefStruct_Unref<T>(ref T objP) where T : IRefCount//XX 120
		{
			T obj = objP;
			objP = default;

			if (obj == null)
				return;

			RefCount @ref = Get_RefCount(obj);

			if (StdAtomic.Atomic_Fetch_Sub(ref @ref.Ref_Count, 1) == 1)
			{
				if (@ref.Free_Cb != null)
					@ref.Free_Cb(@ref.Opaque, obj);

				@ref.Free(@ref);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create a new reference to an object managed via this API, i.e.
		/// increment the reference count of the underlying object and return
		/// obj
		/// </summary>
		/********************************************************************/
		public static T Av_RefStruct_Ref<T>(T obj) where T : IRefCount//XX 140
		{
			RefCount @ref = Get_RefCount(obj);

			StdAtomic.Atomic_Fetch_Add(ref @ref.Ref_Count, 1);

			return obj;
		}



		/********************************************************************/
		/// <summary>
		/// Ensure `*dstp` refers to the same object as src.
		///
		/// If `*dstp` is already equal to src, do nothing. Otherwise
		/// unreference `*dstp` and replace it with a new reference to src in
		/// case `src != NULL` (this involves incrementing the reference
		/// count of src's underlying object) or with NULL otherwise
		/// </summary>
		/********************************************************************/
		public static void Av_RefStruct_Replace<T>(ref T dstP, T src) where T : IRefCount//XX 160
		{
			T dst = dstP;

			if (ReferenceEquals(src, dst))
				return;

			Av_RefStruct_Unref(ref dstP);

			if (src != null)
			{
				dst = Av_RefStruct_Ref_C(src);
				dstP = dst;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvRefStructPool Av_RefStruct_Pool_Alloc_Ext_C<T>(AvRefStructPoolFlag flags, AvRefStructOpaque opaque, UtilFunc.Init_Cb_Delegate init_Cb, UtilFunc.Reset_Cb_Delegate reset_Cb, UtilFunc.Free_Entry_Delegate free_Entry_Cb, UtilFunc.Free_Cb_Delegate free_Cb) where T : class, IRefCountData, new()//XX 340
		{
			AvRefStructPool pool = Av_RefStruct_Alloc_Ext<AvRefStructPool>(AvRefStructFlag.None, null, RefStruct_Pool_Uninit);

			if (pool == null)
				return null;

			Get_RefCount(pool).Free = Pool_Unref;

			pool.Type = typeof(T);
			pool.Opaque = opaque;
			pool.Init_Cb = init_Cb;
			pool.Reset_Cb = reset_Cb;
			pool.Free_Entry_Cb = free_Entry_Cb;
			pool.Free_Cb = free_Cb;
			pool.Entry_Flags = (AvRefStructFlag)(flags & AvRefStructPoolFlag.No_Zeroing);

			// Filter out nonsense combinations to avoid checks later
			if (pool.Reset_Cb == null)
				flags &= ~AvRefStructPoolFlag.Reset_On_Init_Error;

			if (pool.Free_Entry_Cb == null)
				flags &= ~AvRefStructPoolFlag.Free_On_Init_Error;

			pool.Pool_Flags = flags;

			if ((flags & AvRefStructPoolFlag.Zero_Every_Time) != 0)
			{
				// We will zero the buffer before every use, so zeroing
				// upon allocating the buffer is unnecessary
				pool.Entry_Flags |= AvRefStructFlag.No_Zeroing;
			}

			StdAtomic.Atomic_Init(ref pool.RefCount, 1U);

			c_int err = CThread.pthread_mutex_init(out pool.Mutex);

			if (err != 0)
			{
				// Don't call av_refstruct_uninit() on pool, as it hasn't been properly
				// set up and is just a POD right now
				Mem.Av_Free(Get_RefCount(pool));

				return null;
			}

			return pool;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static RefCount Get_RefCount(IRefCount obj)//XX 70
		{
			return (RefCount)obj;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IRefCountData Get_UserData(IRefCount buf)//XX 84
		{
			return (IRefCountData)buf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void RefCount_Init(IRefCount @ref, AvRefStructOpaque opaque, UtilFunc.Free2_Cb_Delegate free_Cb)//XX 89
		{
			RefCount ref2 = (RefCount)@ref;

			StdAtomic.Atomic_Init<c_uint>(ref ref2.Ref_Count, 1);

			ref2.Opaque = opaque;
			ref2.Free_Cb = free_Cb;
			ref2.Free = Mem.Av_Free;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static T Av_RefStruct_Ref_C<T>(T obj) where T : IRefCount//XX 149
		{
			// Casting const away here is fine, as it is only supposed
			// to apply to the user's data and not our bookkeeping data
			RefCount @ref = Get_RefCount(obj);

			StdAtomic.Atomic_Fetch_Add(ref @ref.Ref_Count, 1);

			return obj;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Pool_Free(AvRefStructPool pool)//XX 208
		{
			CThread.pthread_mutex_destroy(pool.Mutex);

			if (pool.Free_Cb != null)
				pool.Free_Cb(pool.Opaque);

			Mem.Av_Free(Get_RefCount(pool));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Pool_Free_Entry(AvRefStructPool pool, RefCount @ref)//XX 216
		{
			if (pool.Free_Entry_Cb != null)
				pool.Free_Entry_Cb(pool.Opaque, Get_UserData(@ref));

			Mem.Av_Free(@ref);
		}



		/********************************************************************/
		/// <summary>
		///  Hint: The content of pool_unref() and refstruct_pool_uninit()
		/// could currently be merged; they are only separate functions
		/// in case we would ever introduce weak references
		/// </summary>
		/********************************************************************/
		private static void Pool_Unref(IRefCount @ref)//XX 309
		{
			AvRefStructPool pool = (AvRefStructPool)Get_UserData(@ref);

			if (StdAtomic.Atomic_Fetch_Sub(ref pool.RefCount, 1) == 1)
				Pool_Free(pool);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void RefStruct_Pool_Uninit(AvRefStructOpaque unused, IRefCount obj)//XX 316
		{
			AvRefStructPool pool = (AvRefStructPool)obj;

			CThread.pthread_mutex_lock(pool.Mutex);

			pool.Uninited = 1;
			RefCount entry = pool.Available_Entries.IsNotNull ? pool.Available_Entries[0] : null;
			pool.Available_Entries.SetToNull();

			CThread.pthread_mutex_unlock(pool.Mutex);

			while (entry != null)
			{
				RefCount next = (RefCount)entry.Opaque.Nc;

				Pool_Free_Entry(pool, entry);

				entry = next;
			}
		}
		#endregion
	}
}

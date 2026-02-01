/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Default memory allocator for LibAvUtil
	/// </summary>
	public static class Mem
	{
		private static size_t max_Alloc_Size = c_int.MaxValue;

		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU)
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAlloc<T>(size_t size) where T : struct//XX 98
		{
			if (size > StdAtomic.Atomic_Load(ref max_Alloc_Size))
				return null;

			CPointer<T> ptr = CMemory.malloc<T>(size);

			if (ptr.IsNull && (size == 0))
			{
				size = 1;
				ptr = Av_MAlloc<T>(1);
			}

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU)
		/// </summary>
		/********************************************************************/
		public static T Av_MAllocObj<T>() where T : class, new()
		{
			return new T();
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU)
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAllocObj<T>(size_t size) where T : class, new()
		{
			CPointer<T> ptr = CMemory.mallocObj<T>(size);

			if (ptr.IsNull && (size == 0))
			{
				size = 1;
				ptr = Av_MAllocObj<T>(1);
			}

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free a block of memory.
		///
		/// If `ptr` is `NULL` and `size` > 0, allocate a new block.
		/// Otherwise, expand or shrink that block of memory according to
		/// `size`
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_Realloc<T>(CPointer<T> ptr, size_t size) where T : struct//XX 155
		{
			if (size > StdAtomic.Atomic_Load(ref max_Alloc_Size))
				return null;

			CPointer<T> ret = CMemory.realloc(ptr, size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free a block of memory.
		///
		/// If `ptr` is `NULL` and `size` > 0, allocate a new block.
		/// Otherwise, expand or shrink that block of memory according to
		/// `size`
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_ReallocObj<T>(CPointer<T> ptr, size_t size) where T : class, new()
		{
			CPointer<T> ret = CMemory.reallocObj(ptr, size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free a block of memory
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_Realloc_F<T>(CPointer<T> ptr, size_t size) where T : struct//XX 173
		{
			CPointer<T> r = Av_Realloc(ptr, size);

			if (r.IsNull)
				Av_Free(ptr);

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free a block of memory
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_Realloc_FObj<T>(CPointer<T> ptr, size_t size) where T : class, new()
		{
			CPointer<T> r = Av_ReallocObj(ptr, size);

			if (r.IsNull)
				Av_Free(ptr);

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free a block of memory through a pointer
		/// to a pointer.
		///
		/// If `*ptr` is `NULL` and `size` > 0, allocate a new block. If
		/// `size` is zero, free the memory block pointed to by `*ptr`.
		/// Otherwise, expand or shrink that block of memory according to
		/// `size`
		/// </summary>
		/********************************************************************/
		public static c_int Av_ReallocP<T>(ref CPointer<T> ptr, size_t size) where T : struct//XX 188
		{
			if (size == 0)
			{
				Av_FreeP(ref ptr);

				return 0;
			}

			CPointer<T> val = ptr;
			val = Av_Realloc(val, size);

			if (val.IsNull)
			{
				Av_FreeP(ref ptr);

				return Error.ENOMEM;
			}

			ptr = val;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block for an array with av_malloc().
		///
		/// The allocated memory will have size `size * nmemb` bytes
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAlloc_ArrayObj<T>(size_t nMemb) where T : class, new()//XX 209
		{
			return Av_MAllocObj<T>(nMemb);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block for an array with av_malloc().
		///
		/// The allocated memory will have size `size * nmemb` bytes
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAlloc_Array<T>(size_t nMemb) where T : struct//XX 209
		{
			return Av_MAlloc<T>(nMemb);
		}



		/********************************************************************/
		/// <summary>
		/// Free a memory block which has been allocated with a function of
		/// av_malloc() or av_realloc() family
		/// </summary>
		/********************************************************************/
		public static void Av_Free<T>(CPointer<T> ptr)//XX 238
		{
			CMemory.free(ptr);
		}



		/********************************************************************/
		/// <summary>
		/// Free a memory block which has been allocated with a function of
		/// av_malloc() or av_realloc() family
		/// </summary>
		/********************************************************************/
		public static void Av_Free<T>(T ptr)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Allocate, reallocate, or free an array.
		///
		/// If `ptr` is `NULL` and `nmemb` > 0, allocate a new block
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_Realloc_ArrayObj<T>(CPointer<T> ptr, size_t nMemb) where T : class, new()//XX 217
		{
			return Av_ReallocObj(ptr, nMemb);
		}



		/********************************************************************/
		/// <summary>
		/// Free a memory block which has been allocated with a function of
		/// av_malloc() or av_realloc() family, and set the pointer pointing
		/// to it to `NULL`
		/// </summary>
		/********************************************************************/
		public static void Av_FreeP<T>(ref CPointer<T> ptr)//XX 247
		{
			CPointer<T> val = ptr;
			ptr.SetToNull();

			Av_Free(val);
		}



		/********************************************************************/
		/// <summary>
		/// Free a memory block which has been allocated with a function of
		/// av_malloc() or av_realloc() family, and set the pointer pointing
		/// to it to `NULL`
		/// </summary>
		/********************************************************************/
		public static void Av_FreeP<T>(ref T ptr) where T : class
		{
			T val = ptr;
			ptr = null;

			Av_Free(val);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU) and zero
		/// all the bytes of the block
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAllocz<T>(size_t size) where T : struct//XX 256
		{
			return Av_MAlloc<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU) and zero
		/// all the bytes of the block
		/// </summary>
		/********************************************************************/
		public static T Av_MAlloczObj<T>() where T: class, new()
		{
			return Av_MAllocObj<T>();
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block with alignment suitable for all memory
		/// accesses (including vectors if available on the CPU) and zero
		/// all the bytes of the block
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MAlloczObj<T>(size_t size) where T : class, new()
		{
			return Av_MAllocObj<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block for an array with av_mallocz().
		///
		/// The allocated memory will have size `size * nmemb` bytes
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_CAlloc<T>(size_t nMemb) where T : struct//XX 264
		{
			return Av_MAllocz<T>(nMemb);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a memory block for an array with av_mallocz().
		///
		/// The allocated memory will have size `size * nmemb` bytes
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_CAllocObj<T>(size_t nMemb) where T : class, new()
		{
			return Av_MAlloczObj<T>(nMemb);
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate a string
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_StrDup(CPointer<char> s)//XX 272
		{
			CPointer<char> ptr = null;

			if (s.IsNotNull)
			{
				size_t len = CString.strlen(s) + 1;
				ptr = Av_Realloc<char>(null, len);

				if (ptr.IsNotNull)
					CMemory.memcpy(ptr, s, len);
			}

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate a buffer with av_malloc()
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MemDup<T>(CPointer<T> p, size_t size) where T : struct//XX 304
		{
			CPointer<T> ptr = null;

			if (p.IsNotNull)
			{
				ptr = Av_MAlloc<T>(size);
				if (ptr.IsNotNull)
					CMemory.memcpy(ptr, p, size);
			}

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate a buffer with av_malloc()
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_MemDupObj<T>(CPointer<T> p, size_t size) where T : class, new()
		{
			CPointer<T> ptr = null;

			if (p.IsNotNull)
			{
				ptr = Av_MAllocObj<T>(size);
				if (ptr.IsNotNull)
					CMemory.memcpy(ptr, p, size);
			}

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate a buffer with av_malloc()
		/// </summary>
		/********************************************************************/
		public static T Av_MemDupObj<T>(T p) where T : IDeepCloneable<T>
		{
			return p.MakeDeepClone();
		}



		/********************************************************************/
		/// <summary>
		/// Add an element to a dynamic array.
		///
		/// Function has the same functionality as av_dynarray_add(), but it
		/// doesn't free memory on fails. It returns error code instead and
		/// leave current buffer untouched
		/// </summary>
		/********************************************************************/
		public static c_int Av_DynArray_Add_NoFreeObj<T>(ref CPointer<T> tab_Ptr, ref c_int nb_Ptr, T elem) where T : class, new() //XX 315
		{
			CPointer<T> tab = tab_Ptr;
			size_t nb = (size_t)nb_Ptr;

			c_int ret = 0;

			DynArray.FF_DynArray_Add(c_int.MaxValue, ref tab, ref nb, (array, count) =>
			{
				array[count] = elem;
			}, (ref CPointer<T> array, ref size_t count) =>
			{
				ret = Error.ENOMEM;
			});

			tab_Ptr = tab;
			nb_Ptr = (c_int)nb;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Add an element of size `elem_size` to a dynamic array.
		///
		/// The array is reallocated when its number of elements reaches
		/// powers of 2. Therefore, the amortized cost of adding an element
		/// is constant.
		///
		/// In case of success, the pointer to the array is updated in order
		/// to point to the new grown array, and the number pointed to by
		/// `nb_ptr` is incremented.
		/// In case of failure, the array is freed, `*tab_ptr` is set to
		/// `NULL` and `*nb_ptr` is set to 0
		/// </summary>
		/********************************************************************/
		public static T Av_DynArray2_AddObj<T>(ref CPointer<T> tab_Ptr, ref c_int nb_Ptr, T elem_Data) where T : class, IDeepCloneable<T>, new() //XX 343
		{
			T tab_Elem_Data = null;

			CPointer<T> tab = tab_Ptr;
			size_t nb = (size_t)nb_Ptr;

			DynArray.FF_DynArray_Add(c_int.MaxValue, ref tab, ref nb, (array, count) =>
			{
				tab_Elem_Data = elem_Data.MakeDeepClone();
				array[count] = tab_Elem_Data;
			}, (ref CPointer<T> array, ref size_t count) =>
			{
				Av_FreeP(ref array);
				count = 0;
			});

			tab_Ptr = tab;
			nb_Ptr = (c_int)nb;

			return tab_Elem_Data;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a buffer, reusing the given one if large enough.
		///
		/// Contrary to av_fast_realloc(), the current buffer contents might
		/// not be preserved and on error the old buffer is freed, thus no
		/// special handling to avoid memleaks is necessary
		/// </summary>
		/********************************************************************/
		public static void Av_Fast_MAlloc<T>(ref CPointer<T> ptr, ref c_int size, size_t min_Size) where T : struct//XX 557
		{
			c_uint size1 = (c_uint)size;

			Fast_MAlloc(ref ptr, ref size1, min_Size, false);

			size = (c_int)size1;
		}



		/********************************************************************/
		/// <summary>
		/// Reallocate the given buffer if it is not large enough, otherwise
		/// do nothing.
		///
		/// If the given buffer is `NULL`, then a new uninitialized buffer
		/// is allocated.
		///
		/// If the given buffer is not large enough, and reallocation fails,
		/// `NULL` is returned and `*size` is set to 0, but the original
		/// buffer is not changed or freed
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Av_Fast_ReallocObj<T>(CPointer<T> ptr, ref c_uint size, size_t min_Size) where T : class, new()//XX 497
		{
			if (min_Size <= size)
				return ptr;

			size_t max_Size = StdAtomic.Atomic_Load(ref max_Alloc_Size);

			// *size is an unsigned, so the real maximum is <= UINT_MAX
			max_Size = Macros.FFMin(max_Size, c_uint.MaxValue);

			if (min_Size > max_Size)
			{
				size = 0;

				return null;
			}

			min_Size = Macros.FFMin(max_Size, Macros.FFMax(min_Size + (min_Size / 16) + 32, min_Size));

			ptr = Av_ReallocObj(ptr, min_Size);

			// We could set this to the unmodified min_size but this is safer
			// if the user lost the ptr and uses null now
			if (ptr.IsNull)
				min_Size = 0;

			size = (c_uint)min_Size;

			return ptr;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Fast_MAlloc<T>(ref CPointer<T> ptr, ref c_uint size, size_t min_Size, bool zero_Realloc) where T : struct//XX 527
		{
			CPointer<T> val = ptr;

			if (min_Size <= size)
				return;

			size_t max_Size = StdAtomic.Atomic_Load(ref max_Alloc_Size);

			// *size is an unsigned, so the real maximum is <= UINT_MAX
			max_Size = Macros.FFMin(max_Size, c_uint.MaxValue);

			if (min_Size > max_Size)
			{
				Av_FreeP(ref ptr);
				size = 0;
				return;
			}

			min_Size = Macros.FFMin(max_Size, Macros.FFMax(min_Size + (min_Size / 16) + 32, min_Size));

			Av_FreeP(ref ptr);

			val = zero_Realloc ? Av_MAllocz<T>(min_Size) : Av_MAlloc<T>(min_Size);
			ptr = val;

			if (val.IsNull)
				min_Size = 0;

			size = (c_uint)min_Size;
		}
		#endregion
	}
}

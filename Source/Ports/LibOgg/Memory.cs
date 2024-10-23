/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOgg
{
	/// <summary>
	/// Different memory methods
	/// </summary>
	public static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> Ogg_MAlloc<T>(size_t size)
		{
			return CMemory.MAlloc<T>((int)size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> Ogg_Realloc<T>(Pointer<T> ptr, size_t newSize)
		{
			return CMemory.Realloc(ptr, (int)newSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Ogg_Free<T>(Pointer<T> ptr)
		{
			CMemory.Free(ptr);
		}
	}
}

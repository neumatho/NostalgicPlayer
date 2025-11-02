/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpus
{
	/// <summary>
	/// Different memory methods
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Opus_Alloc<T>() where T : new()
		{
			return new T();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Free<T>(T ptr)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Copy<T>(CPointer<T> dst, CPointer<T> src, int n)
		{
			CMemory.memcpy(dst, src, (size_t)n);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Move<T>(CPointer<T> dst, CPointer<T> src, int n)
		{
			CMemory.memmove(dst, src, (size_t)n);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Clear<T>(CPointer<T> dst, int n)
		{
			CMemory.memset(dst, default, (size_t)n);
		}
    }
}

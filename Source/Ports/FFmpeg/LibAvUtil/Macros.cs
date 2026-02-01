/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Macros
	{
		/********************************************************************/
		/// <summary>
		/// Comparator.
		/// For two numerical expressions x and y, gives 1 if x › y, -1
		/// if x ‹ y, and 0 if x == y. This is useful for instance in a qsort
		/// comparator callback.
		/// Furthermore, compilers are able to optimize this to branchless
		/// code, and there is no risk of overflow with signed types.
		/// As with many macros, this evaluates its argument multiple times,
		/// it thus must not have a side-effect
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FFDiffSign<T>(T x, T y) where T : INumber<T>
		{
			return (x > y ? T.One : T.Zero) - (x < y ? T.One : T.Zero);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FFMax<T>(T a, T b) where T : INumber<T>
		{
			return a > b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FFMin<T>(T a, T b) where T : INumber<T>
		{
			return a > b ? b : a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FFSwap<T>(ref T a, ref T b)
		{
			T swap_Tmp = b;
			b = a;
			a = swap_Tmp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FFSwapObj<T>(T a, T b) where T : ICopyTo<T>, new()
		{
			T swap_Tmp = new T();

			b.CopyTo(swap_Tmp);
			a.CopyTo(b);
			swap_Tmp.CopyTo(a);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t FF_Array_Elems(Array a)
		{
			return (size_t)a.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t FF_Array_Elems<T>(CPointer<T> a)
		{
			return (size_t)a.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t FF_Array_Elems<T1, T2>(Dictionary<T1, T2> a)
		{
			return (size_t)a.Count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint MkTag(c_int a, c_int b, c_int c, c_int d)
		{
			return (c_uint)(a | (b << 8) | (c << 16) | (d << 24));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FFAlign<T>(T x, c_int a) where T : INumber<T>, IAdditionOperators<T, c_int, T>, ISubtractionOperators<T, c_int, T>, IBitwiseOperators<T, c_int, T>
		{
			return (x + a - 1) & ~(a - 1);
		}
	}
}

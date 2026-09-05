/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// Various useful utility functions
	/// </summary>
	internal static class OpenMpt
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear<T>(T[] a)
		{
			for (c_int i = a.Length - 1; i >= 0; i--)
			{
				T o = a[i];

				if (o is IClearable co)
					co.Clear();
				else
					a[i] = default;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Memset given object to zero
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemsetZero<T>(CPointer<T> a) where T : INumber<T>
		{
			Memory.MemClear(a);
		}



		/********************************************************************/
		/// <summary>
		/// Memset given object to zero
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemsetZero<T>(T[] a)
		{
			Array.Clear(a);
		}



		/********************************************************************/
		/// <summary>
		/// Limits 'val' to given range. If 'val' is less than 'lowerLimit',
		/// 'val' is set to value 'lowerLimit'. Similarly if 'val' is greater
		/// than 'upperLimit', 'val' is set to value 'upperLimit'.
		/// If 'lowerLimit' > 'upperLimit', 'val' won't be modified
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Limit<T, C>(ref T val, C lowerLimit, C upperLimit) where T : INumber<T>, IComparisonOperators<T, C, bool> where C : INumber<C>
		{
			if (lowerLimit > upperLimit)
				return;

			if (val < lowerLimit)
				val = T.CreateTruncating(lowerLimit);
			else if (val > upperLimit)
				val = T.CreateTruncating(upperLimit);
		}



		/********************************************************************/
		/// <summary>
		/// Limits 'val' to given range. If 'val' is less than 'lowerLimit',
		/// 'val' is set to value 'lowerLimit'. Similarly if 'val' is greater
		/// than 'upperLimit', 'val' is set to value 'upperLimit'.
		/// If 'lowerLimit' > 'upperLimit', 'val' won't be modified
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Limit(ref Tempo val, Tempo lowerLimit, Tempo upperLimit)
		{
			if (lowerLimit > upperLimit)
				return;

			if (val < lowerLimit)
				val = lowerLimit;
			else if (val > upperLimit)
				val = upperLimit;
		}



		/********************************************************************/
		/// <summary>
		/// Like Limit, but returns value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Clamp<T, C>(T val, C lowerLimit, C upperLimit) where T : INumber<T>, IComparisonOperators<T, C, bool> where C : INumber<C>
		{
			if (val < lowerLimit)
				return T.CreateTruncating(lowerLimit);
			else if (val > upperLimit)
				return T.CreateTruncating(upperLimit);
			else
				return val;
		}



		/********************************************************************/
		/// <summary>
		/// Like Limit, but returns value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Tempo Clamp(Tempo val, Tempo lowerLimit, Tempo upperLimit)
		{
			if (val < lowerLimit)
				return lowerLimit;
			else if (val > upperLimit)
				return upperLimit;
			else
				return val;
		}



		/********************************************************************/
		/// <summary>
		/// Like Limit, but with upperlimit only
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LimitMax<T, C>(ref T val, C upperLimit) where T : INumber<T>, IComparisonOperators<T, C, bool> where C : INumber<C>
		{
			if (val > upperLimit)
				val = T.CreateTruncating(upperLimit);
		}
	}
}

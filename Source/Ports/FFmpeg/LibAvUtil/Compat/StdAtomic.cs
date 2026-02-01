/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat
{
	/// <summary>
	/// Helper class to provide atomic functionality
	/// </summary>
	public static class StdAtomic
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Atomic_Init<T>(ref T @object, T value) where T : struct
		{
			@object = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Atomic_Store<T>(ref T @object, T desired) where T : Enum
		{
			Volatile.Write(ref Unsafe.As<T, c_int>(ref @object), Convert.ToInt32(desired));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Atomic_Store(ref int @object, int desired)
		{
			Volatile.Write(ref @object, desired);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Atomic_Store(ref uint @object, uint desired)
		{
			Volatile.Write(ref @object, desired);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Atomic_Load<T>(ref T @object) where T : Enum
		{
			c_int value = Volatile.Read(ref Unsafe.As<T, c_int>(ref @object));
			return (T)Enum.ToObject(typeof(T), value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Atomic_Load(ref c_int @object)
		{
			return Volatile.Read(ref @object);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t Atomic_Load(ref size_t @object)
		{
			return Volatile.Read(ref @object);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Atomic_Load(ref c_uint @object)
		{
			return Volatile.Read(ref @object);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Atomic_Exchange<T>(ref T @object, T desired)
		{
			return Interlocked.Exchange(ref @object, desired);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Atomic_Fetch_Add(ref c_uint @object, c_int operand)
		{
			return Interlocked.Add(ref @object, (c_uint)operand) - (c_uint)operand;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Atomic_Fetch_Sub(ref c_uint @object, c_int operand)
		{
			return Interlocked.Add(ref @object, (c_uint)(-operand)) + (c_uint)operand;
		}
	}
}

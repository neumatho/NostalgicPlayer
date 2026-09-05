/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using byte_span = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.MptSpan<byte>;
global using byte_span2 = System.Span<byte>;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static CPointer<T> Byte_Cast<T>(IPointer v) where T : unmanaged
		{
			return v.Cast<T>();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static MptSpan<TTo> Byte_Cast<TFrom, TTo>(MptSpan<TFrom> v) where TFrom : unmanaged where TTo : unmanaged
		{
			return new MptSpan<TTo>(v.Data().Cast<TTo>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Span<TTo> Byte_Cast<TFrom, TTo>(Span<TFrom> v) where TFrom : unmanaged where TTo : unmanaged
		{
			return MemoryMarshal.Cast<TFrom, TTo>(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static byte_span As_Raw_Memory<T>(T[] v) where T : unmanaged
		{
			return new byte_span(new CPointer<T>(v).Cast<T, c_byte>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static byte_span As_Raw_Memory<T>(array<T> v) where T : unmanaged
		{
			return new byte_span(v.data().Cast<T, c_byte>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static byte_span As_Raw_Memory<T>(CPointer<T> v) where T : unmanaged
		{
			return new byte_span(v.Cast<T, c_byte>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static byte_span2 As_Raw_Memory<T>(ref T v) where T : unmanaged
		{
			return MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref v, 1));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemClear<T>(CPointer<T> x) where T : INumber<T>
		{
			CMemory.memset(x, T.Zero, x.Size());
		}
	}
}

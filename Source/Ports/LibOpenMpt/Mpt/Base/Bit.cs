/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Bit
	{
		/********************************************************************/
		/// <summary>
		/// Reinterpret the raw bytes in "src" as a value of type "TDst".
		/// The source is a byte span, since the C++ counterpart is called
		/// with a plain "uint8 buf[sizeof(TDst)]" array
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDst Bit_Cast<TDst>(byte_span2 src)
		{
			return Unsafe.ReadUnaligned<TDst>(ref MemoryMarshal.GetReference(src));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Bit_Width<T>(T x) where T : INumber<T>, IShiftOperators<T, c_int, T>, IComparisonOperators<T, T, bool>
		{
			c_int result = 0;

			while (x > T.Zero)
			{
				x >>= 1;
				result += 1;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotl_Impl<T>(T x, c_int r) where T : IBinaryInteger<T>, IUnsignedNumber<T>
		{
			c_int n = Marshal.SizeOf<T>() * 8;

			return (x >> (n - r)) | (x << r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotr_Impl<T>(T x, c_int r) where T : IBinaryInteger<T>, IUnsignedNumber<T>
		{
			c_int n = Marshal.SizeOf<T>() * 8;

			return (x << (n - r)) | (x >> r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotl<T>(T x, c_int s) where T : IBinaryInteger<T>, IUnsignedNumber<T>
		{
			c_int n = Marshal.SizeOf<T>() * 8;
			c_int r = s % n;

			return s < 0 ? Rotr_Impl(x, -s) : (x >> (n - r)) | (x << r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotr<T>(T x, c_int s) where T : IBinaryInteger<T>, IUnsignedNumber<T>
		{
			c_int n = Marshal.SizeOf<T>() * 8;
			c_int r = s % n;

			return s < 0 ? Rotl_Impl(x, -s) : (x << (n - r)) | (x >> r);
		}
	}
}

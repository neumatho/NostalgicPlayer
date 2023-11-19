/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Helpers which are defined as macros in the original source
	/// </summary>
	internal static class Helpers
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Double_To_Real(c_double x)
		{
			return (Real)x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Double_To_Real_15(c_double x)
		{
			return (Real)x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Double_To_Real_Scale_Layer3(c_double x, c_double y)
		{
			return (Real)x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Mul(Real x, Real y)
		{
			return x * y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Mul_Synth(Real x, Real y)
		{
			return x * y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Mul_15(Real x, Real y)
		{
			return x * y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Mul_Scale_Layer12(Real x, Real y)
		{
			return x * y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Mul_Scale_Layer3(Real x, Real y)
		{
			return x * y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Scale_Layer12(Real x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Scale_Layer3(Real x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Real Real_Scale_Dct64(Real x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Sample_Adjust(Mpg123_Handle mh, int64_t x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Sample_Unadjust(Mpg123_Handle mh, int64_t x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Frame_BufferCheck(Mpg123_Handle mh)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Get size of one PCM sample with given encoding. This is included
		/// both in libmpg123 and libout123. Both offer an API function to
		/// provide the macro results from library compile-time, not that of
		/// you application. This most likely does not matter as I do not
		/// expect any fresh PCM sample encoding to appear. But who knows?
		/// Perhaps the encoding type will be abused for funny things in
		/// future, not even plain PCM. And, by the way: Thomas really likes
		/// the ?: operator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Mpg123_SampleSize(Mpg123_Enc_Enum enc)
		{
			return (c_int)enc < 1 ? 0 :
				(enc & Mpg123_Enc_Enum.Enc_8) != 0 ? 1 :
				(enc & Mpg123_Enc_Enum.Enc_16) != 0 ? 2 :
				(enc & Mpg123_Enc_Enum.Enc_24) != 0 ? 3 :
				((enc & Mpg123_Enc_Enum.Enc_32) != 0) || ((enc & Mpg123_Enc_Enum.Enc_Float_32) != 0) ? 4 :
				(enc & Mpg123_Enc_Enum.Enc_Float_64) != 0 ? 8 :
				0;
		}



		/********************************************************************/
		/// <summary>
		/// Finish 32 bit sample to unsigned 32 bit sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Conv_SU32(int32_t s)
		{
			return s >= 0 ?
				(uint32_t)s + 2147483648 :
				(s == -2147483647L-1L ? // Work around to prevent a non-conformant MSVC warning/error
				0 : // Separate because negation would overflow
				(uint32_t)2147483648UL - (uint32_t)(-s));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Drop4Byte(c_uchar[] buf, size_t w, size_t r)
		{
			buf[w + 0] = buf[r + 1];
			buf[w + 1] = buf[r + 2];
			buf[w + 2] = buf[r + 3];
		}
	}
}

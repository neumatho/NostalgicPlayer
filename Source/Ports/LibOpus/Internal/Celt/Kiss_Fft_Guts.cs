/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Kiss_Fft_Guts
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 S_MUL(kiss_fft_scalar a, kiss_twiddle_scalar b)
		{
			return a * b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C_MUL(ref Kiss_Fft_Cpx m, Kiss_Fft_Cpx a, Kiss_Twiddle_Cpx b)
		{
			m.r = a.r * b.r - a.i * b.i;
			m.i = a.r * b.i + a.i * b.r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C_MULBYSCALAR(ref Kiss_Fft_Cpx c, kiss_twiddle_scalar s)
		{
			c.r *= s;
			c.i *= s;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C_ADD(ref Kiss_Fft_Cpx res, Kiss_Fft_Cpx a, Kiss_Fft_Cpx b)
		{
			res.r = a.r + b.r;
			res.i = a.i + b.i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C_SUB(ref Kiss_Fft_Cpx res, Kiss_Fft_Cpx a, Kiss_Fft_Cpx b)
		{
			res.r = a.r - b.r;
			res.i = a.i - b.i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void C_ADDTO(ref Kiss_Fft_Cpx res, Kiss_Fft_Cpx a)
		{
			res.r += a.r;
			res.i += a.i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static kiss_fft_scalar HALF_OF(kiss_fft_scalar x)
		{
			return x * 0.5f;
		}
	}
}

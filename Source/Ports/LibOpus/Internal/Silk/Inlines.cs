/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Inlines
	{
		/********************************************************************/
		/// <summary>
		/// Get number od leading zeros and fractional part (the bits right
		/// after the leading one)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Silk_CLZ_FRAC(opus_int32 _in, out opus_int32 lz, out opus_int32 frac_Q7)
		{
			opus_int32 lzeros = Macros.Silk_CLZ32(_in);

			lz = lzeros;
			frac_Q7 = SigProc_Fix.Silk_ROR32(_in, 24 - lzeros) & 0x7f;
		}



		/********************************************************************/
		/// <summary>
		/// Approximation of square root
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SQRT_APPROX(opus_int32 x)
		{
			if (x <= 0)
				return 0;

			Silk_CLZ_FRAC(x, out opus_int32 lz, out opus_int32 frac_Q7);

			opus_int32 y;

			if ((lz & 1) != 0)
				y = 32768;
			else
				y = 46214;		// 46214 = sqrt(2) * 32768

			// Get scaling right
			y >>= SigProc_Fix.Silk_RSHIFT(lz, 1);

			// Increment using fractional part of input
			y = Macros.Silk_SMLAWB(y, y, Macros.Silk_SMULBB(213, frac_Q7));

			return y;
		}



		/********************************************************************/
		/// <summary>
		/// Divide two int32 values and return result as int32 in a given
		/// Q-domain
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_DIV32_VarQ(opus_int32 a32, opus_int32 b32, opus_int Qres)
		{
			// Compute number of bits head room and normalize input
			opus_int a_headrm = Macros.Silk_CLZ32(SigProc_Fix.Silk_Abs(a32)) - 1;
			opus_int32 a32_nrm = SigProc_Fix.Silk_LSHIFT(a32, a_headrm);
			// Q: a_headrm

			opus_int b_headrm = Macros.Silk_CLZ32(SigProc_Fix.Silk_Abs(b32)) - 1;
			opus_int32 b32_nrm = SigProc_Fix.Silk_LSHIFT(b32, b_headrm);
			// Q: b_headrm

			// Inverse of b32, with 14 bits of precision
			opus_int32 b32_inv = SigProc_Fix.Silk_DIV32_16(opus_int32.MaxValue >> 2, (opus_int16)SigProc_Fix.Silk_RSHIFT(b32_nrm, 16));
			// Q: 29 + 16 - b_headrm

			// First approximation
			opus_int32 result = Macros.Silk_SMULWB(a32_nrm, b32_inv);
			// Q: 29 + a_headrm - b_headrm

			// Compute residual by subtracting product of denominator and first approximation
			// It's OK to overflow because the final value of a32_nrm should always be small
			a32_nrm = SigProc_Fix.Silk_SUB32_ovflw((opus_uint32)a32_nrm, (opus_uint32)SigProc_Fix.Silk_LSHIFT_ovflw((opus_uint32)SigProc_Fix.Silk_SMMUL(b32_nrm, result), 3));	// Q: a_headrm

			// Refinement
			result = Macros.Silk_SMLAWB(result, a32_nrm, b32_inv);
			// Q: 29 + a_headrm - b_headrm

			// Convert to qRes domain
			opus_int lshift = 29 + a_headrm - b_headrm - Qres;

			if (lshift < 0)
				return SigProc_Fix.Silk_LSHIFT_SAT32(result, -lshift);
			else
			{
				if (lshift < 32)
					return SigProc_Fix.Silk_RSHIFT(result, lshift);
				else
				{
					// Avoid undefined result
					return 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Invert int32 value and return result as int32 in a given Q-domain
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_INVERSE32_VarQ(opus_int32 b32, opus_int Qres)
		{
			// Compute number of bits head room and normalize input
			opus_int b_headrm = Macros.Silk_CLZ32(SigProc_Fix.Silk_Abs(b32)) - 1;
			opus_int32 b32_nrm = SigProc_Fix.Silk_LSHIFT(b32, b_headrm);
			// Q: b_headrm

			// Inverse of b32, with 14 bits of precision
			opus_int32 b32_inv = SigProc_Fix.Silk_DIV32_16(opus_int32.MaxValue >> 2, (opus_int16)SigProc_Fix.Silk_RSHIFT(b32_nrm, 16));
			// Q: 29 + 16 - b_headrm

			// First approximation
			opus_int32 result = SigProc_Fix.Silk_LSHIFT(b32_inv, 16);
			// Q: 61 - b_headrm

			// Compute residual by subtracting product of denominator and first approximation from one
			opus_int32 err_Q32 = SigProc_Fix.Silk_LSHIFT((1 << 29) - Macros.Silk_SMULWB(b32_nrm, b32_inv), 3);	// Q32

			// Refinement
			result = Macros.Silk_SMLAWW(result, err_Q32, b32_inv);
			// Q: 61 - b_headrm

			// Convert to qRes domain
			opus_int lshift = 61 - b_headrm - Qres;

			if (lshift <= 0)
				return SigProc_Fix.Silk_LSHIFT_SAT32(result, -lshift);
			else
			{
				if (lshift < 32)
					return SigProc_Fix.Silk_RSHIFT(result, lshift);
				else
				{
					// Avoid undefined result
					return 0;
				}
			}
		}
	}
}

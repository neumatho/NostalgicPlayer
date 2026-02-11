/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Celt_Lpc
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void _Celt_Lpc(CPointer<opus_val16> _lpc, CPointer<opus_val32> ac, c_int p)
		{
			opus_val32 error = ac[0];
			CPointer<c_float> lpc = _lpc;

			Memory.Opus_Clear(lpc, p);

			if (ac[0] > 1e-10f)
			{
				for (c_int i = 0; i < p; i++)
				{
					// Sum up this iteration's reflection coefficient
					opus_val32 rr = 0;

					for (c_int j = 0; j < i; j++)
						rr += Arch.MULT32_32_Q31(lpc[j], ac[i - j]);

					rr += Arch.SHR32(ac[i + 1], 6);
					opus_val32 r = -MathOps.Frac_Div32(Arch.SHL32(rr, 6), error);

					// Update LPC coefficients and total error
					lpc[i] = Arch.SHR32(r, 6);

					for (c_int j = 0; j < ((i + 1) >> 1); j++)
					{
						opus_val32 tmp1 = lpc[j];
						opus_val32 tmp2 = lpc[i - 1 - j];

						lpc[j] = tmp1 + Arch.MULT32_32_Q31(r, tmp2);
						lpc[i - 1 - j] = tmp2 + Arch.MULT32_32_Q31(r, tmp1);
					}

					error = error - Arch.MULT32_32_Q31(Arch.MULT32_32_Q31(r, r), error);

					// Bail out once we get 30 dB gain
					if (error <= (0.001f * ac[0]))
						break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Celt_Fir_C(CPointer<opus_val16> x, CPointer<opus_val16> num, CPointer<opus_val16> y, c_int N, c_int ord, c_int arch)
		{
			c_int i;
			opus_val16[] rnum = new opus_val16[ord];

			for (i = 0; i < ord; i++)
				rnum[i] = num[ord - i - 1];

			for (i = 0; i < (N - 3); i += 4)
			{
				opus_val32[] sum = new opus_val32[4];

				sum[0] = Arch.SHL32(Arch.EXTEND32(x[i]), Constants.Sig_Shift);
				sum[1] = Arch.SHL32(Arch.EXTEND32(x[i + 1]), Constants.Sig_Shift);
				sum[2] = Arch.SHL32(Arch.EXTEND32(x[i + 2]), Constants.Sig_Shift);
				sum[3] = Arch.SHL32(Arch.EXTEND32(x[i + 3]), Constants.Sig_Shift);

				Pitch.Xcorr_Kernel(rnum, x + i - ord, sum, ord, arch);

				y[i] = Arch.SROUND16(sum[0], Constants.Sig_Shift);
				y[i + 1] = Arch.SROUND16(sum[1], Constants.Sig_Shift);
				y[i + 2] = Arch.SROUND16(sum[2], Constants.Sig_Shift);
				y[i + 3] = Arch.SROUND16(sum[3], Constants.Sig_Shift);
			}

			for (; i < N; i++)
			{
				opus_val32 sum = Arch.SHL32(Arch.EXTEND32(x[i]), Constants.Sig_Shift);

				for (c_int j = 0; j < ord; j++)
					sum = Arch.MAC16_16(sum, rnum[j], x[i + j - ord]);

				y[i] = Arch.SROUND16(sum, Constants.Sig_Shift);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Celt_Fir(CPointer<opus_val16> x, CPointer<opus_val16> num, CPointer<opus_val16> y, c_int N, c_int ord, c_int arch)
		{
			Celt_Fir_C(x, num, y, N, ord, arch);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Celt_Iir(CPointer<opus_val32> _x, CPointer<opus_val16> den, CPointer<opus_val32> _y, c_int N, c_int ord, CPointer<opus_val16> mem, c_int arch)
		{
			c_int i;
			opus_val16[] rden = new opus_val16[ord];
			CPointer<opus_val16> y = new CPointer<opus_val16>(N + ord);

			for (i = 0; i < ord; i++)
				rden[i] = den[ord - i - 1];

			for (i = 0; i < ord; i++)
				y[i] = -mem[ord - i - 1];

			for (; i < (N + ord); i++)
				y[i] = 0;

			for (i = 0; i < (N - 3); i += 4)
			{
				// Unroll by 4 as if it were an FIR filter
				opus_val32[] sum = new opus_val32[4];

				sum[0] = _x[i];
				sum[1] = _x[i + 1];
				sum[2] = _x[i + 2];
				sum[3] = _x[i + 3];

				Pitch.Xcorr_Kernel(rden, y + i, sum, ord, arch);

				// Patch up the result to compensate for the fact that this is an IIR
				y[i + ord] = -Arch.SROUND16(sum[0], Constants.Sig_Shift);
				_y[i] = sum[0];
				sum[1] = Arch.MAC16_16(sum[1], y[i + ord], den[0]);

				y[i + ord + 1] = -Arch.SROUND16(sum[1], Constants.Sig_Shift);
				_y[i + 1] = sum[1];
				sum[2] = Arch.MAC16_16(sum[2], y[i + ord + 1], den[0]);
				sum[2] = Arch.MAC16_16(sum[2], y[i + ord], den[1]);

				y[i + ord + 2] = -Arch.SROUND16(sum[2], Constants.Sig_Shift);
				_y[i + 2] = sum[2];

				sum[3] = Arch.MAC16_16(sum[3], y[i + ord + 2], den[0]);
				sum[3] = Arch.MAC16_16(sum[3], y[i + ord + 1], den[1]);
				sum[3] = Arch.MAC16_16(sum[3], y[i + ord], den[2]);
				y[i + ord + 3] = -Arch.SROUND16(sum[3], Constants.Sig_Shift);
				_y[i + 3] = sum[3];
			}

			for (; i < N; i++)
			{
				opus_val32 sum = _x[i];

				for (c_int j = 0; j < ord; j++)
					sum -= Arch.MULT16_16(rden[j], y[i + j]);

				y[i + ord] = Arch.SROUND16(sum, Constants.Sig_Shift);
				_y[i] = sum;
			}

			for (i = 0; i < ord; i++)
				mem[i] = _y[N - i - 1];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int _Celt_Autocorr(CPointer<opus_val16> x, CPointer<opus_val32> ac, CPointer<celt_coef> window, c_int overlap, c_int lag, c_int n, c_int arch)
		{
			c_int fastN = n - lag;
			CPointer<opus_val16> xptr;
			opus_val16[] xx = new opus_val16[n];

			if (overlap == 0)
				xptr = x;
			else
			{
				for (c_int i = 0; i < n; i++)
					xx[i] = x[i];

				for (c_int i = 0; i < overlap; i++)
				{
					opus_val16 w = Arch.COEF2VAL16(window[i]);
					xx[i] = Arch.MULT16_16_Q15(x[i], w);
					xx[n - i - 1] = Arch.MULT16_16_Q15(x[n - i - 1], w);
				}

				xptr = xx;
			}

			c_int shift = 0;

			Pitch.Celt_Pitch_Xcorr(xptr, xptr, ac, fastN, lag + 1, arch);

			for (c_int k = 0; k <= lag; k++)
			{
				c_int i;
				opus_val32 d;

				for (i = k + fastN, d = 0; i < n; i++)
					d = Arch.MAC16_16(d, xptr[i], xptr[i - k]);

				ac[k] += d;
			}

			return shift;
		}
	}
}

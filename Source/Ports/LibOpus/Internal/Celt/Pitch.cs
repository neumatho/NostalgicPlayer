/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Pitch
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Find_Best_Pitch(CPointer<opus_val32> xcorr, CPointer<opus_val16> y, c_int len, c_int max_pitch, CPointer<c_int> best_pitch)
		{
			opus_val32 Syy = 1;
			opus_val16[] best_num = new opus_val16[2];
			opus_val32[] best_den = new opus_val32[2];

			best_num[0] = -1;
			best_num[1] = -1;
			best_den[0] = 0;
			best_den[1] = 0;
			best_pitch[0] = 0;
			best_pitch[1] = 1;

			for (c_int j = 0; j < len; j++)
				Syy = Arch.ADD32(Syy, Arch.SHR32(Arch.MULT16_16(y[j], y[j]), 0/*yshift*/));

			for (c_int i = 0; i < max_pitch; i++)
			{
				if (xcorr[i] > 0)
				{
					opus_val32 xcorr16 = Arch.EXTRACT16(Arch.VSHR32(xcorr[i], 0/*xshift*/));

					// Considering the range of xcorr16, this should avoid both underflows
					// and overflows (inf) when squaring xcorr16
					xcorr16 *= 1e-12f;

					opus_val16 num = Arch.MULT16_16_Q15(xcorr16, xcorr16);

					if (Arch.MULT16_32_Q15(num, best_den[1]) > Arch.MULT16_32_Q15(best_num[1], Syy))
					{
						if (Arch.MULT16_32_Q15(num, best_den[0]) > Arch.MULT16_32_Q15(best_num[0], Syy))
						{
							best_num[1] = best_num[0];
							best_den[1] = best_den[0];
							best_pitch[1] = best_pitch[0];

							best_num[0] = num;
							best_den[0] = Syy;
							best_pitch[0] = i;
						}
						else
						{
							best_num[1] = num;
							best_den[1] = Syy;
							best_pitch[1] = i;
						}
					}
				}

				Syy += Arch.SHR32(Arch.MULT16_16(y[i + len], y[i + len]), 0/*yshift*/) - Arch.SHR32(Arch.MULT16_16(y[i], y[i]), 0/*yshift*/);
				Syy = Arch.MAX32(1, Syy);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Celt_Fir5(CPointer<opus_val16> x, CPointer<opus_val16> num, c_int N)
		{
			opus_val16 num0 = num[0];
			opus_val16 num1 = num[1];
			opus_val16 num2 = num[2];
			opus_val16 num3 = num[3];
			opus_val16 num4 = num[4];

			opus_val32 mem0 = 0;
			opus_val32 mem1 = 0;
			opus_val32 mem2 = 0;
			opus_val32 mem3 = 0;
			opus_val32 mem4 = 0;

			for (c_int i = 0; i < N; i++)
			{
				opus_val32 sum = Arch.SHL32(Arch.EXTEND32(x[i]), Constants.Sig_Shift);
				sum = Arch.MAC16_16(sum, num0, mem0);
				sum = Arch.MAC16_16(sum, num1, mem1);
				sum = Arch.MAC16_16(sum, num2, mem2);
				sum = Arch.MAC16_16(sum, num3, mem3);
				sum = Arch.MAC16_16(sum, num4, mem4);

				mem4 = mem3;
				mem3 = mem2;
				mem2 = mem1;
				mem1 = mem0;
				mem0 = x[i];

				x[i] = Arch.ROUND16(sum, Constants.Sig_Shift);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Pitch_Downsample(CPointer<CPointer<celt_sig>> x, CPointer<opus_val16> x_lp, c_int len, c_int C, c_int factor, c_int arch)
		{
			opus_val32[] ac = new opus_val32[5];
			opus_val16 tmp = Constants.Q15One;
			opus_val16[] lpc = new opus_val16[4];
			opus_val16[] lpc2 = new opus_val16[5];
			opus_val16 c1 = Arch.QCONST16(0.8f, 15);
			c_int offset = factor / 2;

			for (c_int i = 1; i < len; i++)
				x_lp[i] = (0.25f * x[0][(factor * i) - offset]) + (0.25f * x[0][(factor * i) + offset]) + (0.5f * x[0][factor * i]);

			x_lp[0] = (0.25f * x[0][offset]) + (0.5f * x[0][0]);

			if (C == 2)
			{
				for (c_int i = 1; i < len; i++)
					x_lp[i] += (0.25f * x[1][(factor * i) - offset]) + (0.25f * x[1][(factor * i) + offset]) + (0.5f * x[1][factor * i]);

				x_lp[0] += (0.25f * x[1][offset]) + (0.5f * x[1][0]);
			}

			Celt_Lpc._Celt_Autocorr(x_lp, ac, null, 0, 4, len, arch);

			// Noise floor -40 dB
			ac[0] *= 1.0001f;

			// Lag windowing
			for (c_int i = 1; i <=4; i++)
				ac[i] -= ac[i] * (0.008f * i) * (0.008f * i);

			Celt_Lpc._Celt_Lpc(lpc, ac, 4);

			for (c_int i = 0; i < 4; i++)
			{
				tmp = Arch.MULT16_16_Q15(Arch.QCONST16(0.9f, 15), tmp);
				lpc[i] = Arch.MULT16_16_Q15(lpc[i], tmp);
			}

			// Add a zero
			lpc2[0] = lpc[0] + Arch.QCONST16(0.8f, Constants.Sig_Shift);
			lpc2[1] = lpc[1] + Arch.MULT16_16_Q15(c1, lpc[0]);
			lpc2[2] = lpc[2] + Arch.MULT16_16_Q15(c1, lpc[1]);
			lpc2[3] = lpc[3] + Arch.MULT16_16_Q15(c1, lpc[2]);
			lpc2[4] = Arch.MULT16_16_Q15(c1, lpc[3]);

			Celt_Fir5(x_lp, lpc2, len);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Celt_Pitch_Xcorr_C(CPointer<opus_val16> _x, CPointer<opus_val16> _y, CPointer<opus_val32> xcorr, c_int len, c_int max_pitch, c_int arch)
		{
			c_int i;

			for (i = 0; i < (max_pitch - 3); i += 4)
			{
				opus_val32[] sum = [ 0, 0, 0, 0 ];

				Xcorr_Kernel(_x, _y + i, sum, len, arch);

				xcorr[i] = sum[0];
				xcorr[i + 1] = sum[1];
				xcorr[i + 2] = sum[2];
				xcorr[i + 3] = sum[3];
			}

			// In case max_pitch isn't a multiple of 4, do non-unrolled version
			for (; i < max_pitch; i++)
			{
				opus_val32 sum = Celt_Inner_Prod(_x, _y + i, len, arch);
				xcorr[i] = sum;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Pitch_Search(CPointer<opus_val16> x_lp, CPointer<opus_val16> y, c_int len, c_int max_pitch, out c_int pitch, c_int arch)
		{
			c_int[] best_pitch = [ 0, 0 ];

			c_int lag = len + max_pitch;

			opus_val16[] x_lp4 = new opus_val16[len >> 2];
			opus_val16[] y_lp4 = new opus_val16[lag >> 2];
			opus_val32[] xcorr = new opus_val32[max_pitch >> 1];

			// Downsample by 2 again
			for (c_int j = 0; j < (len >> 2); j++)
				x_lp4[j] = x_lp[2 * j];

			for (c_int j = 0; j < (lag >> 2); j++)
				y_lp4[j] = y[2 * j];

			// Coarse search with 4x decimation
			Celt_Pitch_Xcorr(x_lp4, y_lp4, xcorr, len >> 2, max_pitch >> 2, arch);

			Find_Best_Pitch(xcorr, y_lp4, len >> 2, max_pitch >> 2, best_pitch);

			// Finer search with 2x decimation
			for (c_int i = 0; i < (max_pitch >> 1); i++)
			{
				xcorr[i] = 0;

				if ((Math.Abs(i - 2 * best_pitch[0]) > 2) && (Math.Abs(i - 2 * best_pitch[1]) > 2))
					continue;

				opus_val32 sum = Celt_Inner_Prod(x_lp, y + i, len >> 1, arch);
				xcorr[i] = Arch.MAX32(-1, sum);
			}

			Find_Best_Pitch(xcorr, y, len >> 1, max_pitch >> 1, best_pitch);

			// Refine by pseudo-interpolation
			c_int offset;

			if ((best_pitch[0] > 0) && (best_pitch[0] < ((max_pitch >> 1) - 1)))
			{
				opus_val32 a = xcorr[best_pitch[0] - 1];
				opus_val32 b = xcorr[best_pitch[0]];
				opus_val32 c = xcorr[best_pitch[0] + 1];

				if ((c - a) > Arch.MULT16_32_Q15(Arch.QCONST16(0.7f, 15), b - a))
					offset = 1;
				else if ((a - c) > Arch.MULT16_32_Q15(Arch.QCONST16(0.7f, 15), b - c))
					offset = -1;
				else
					offset = 0;
			}
			else
				offset = 0;

			pitch = 2 * best_pitch[0] - offset;
		}



		/********************************************************************/
		/// <summary>
		/// OPT: This is the kernel you really want to optimize. It gets used
		/// a lot by the prefilter and by the PLC
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Xcorr_Kernel_C(CPointer<opus_val16> x, CPointer<opus_val16> y, CPointer<opus_val32> sum, c_int len)
		{
			c_int j;
			opus_val16 y_3 = 0;
			opus_val16 y_0 = y[0];
			opus_val16 y_1 = y[1];
			opus_val16 y_2 = y[2];
			y += 3;

			for (j = 0; j < (len - 3); j += 4)
			{
				opus_val16 tmp = x[0];
				x++;

				y_3 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_0);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_1);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_2);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_3);

				tmp = x[0];
				x++;
				y_0 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_1);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_2);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_3);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_0);

				tmp = x[0];
				x++;
				y_1 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_2);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_3);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_0);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_1);

				tmp = x[0];
				x++;
				y_2 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_3);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_0);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_1);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_2);
			}

			if (j++ < len)
			{
				opus_val16 tmp = x[0];
				x++;
				y_3 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_0);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_1);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_2);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_3);
			}

			if (j++ < len)
			{
				opus_val16 tmp = x[0];
				x++;
				y_0 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_1);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_2);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_3);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_0);
			}

			if (j < len)
			{
				opus_val16 tmp = x[0];
				x++;
				y_1 = y[0];
				y++;

				sum[0] = Arch.MAC16_16(sum[0], tmp, y_2);
				sum[1] = Arch.MAC16_16(sum[1], tmp, y_3);
				sum[2] = Arch.MAC16_16(sum[2], tmp, y_0);
				sum[3] = Arch.MAC16_16(sum[3], tmp, y_1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Xcorr_Kernel(CPointer<opus_val16> x, CPointer<opus_val16> y, CPointer<opus_val32> sum, c_int len, c_int arch)
		{
			Xcorr_Kernel_C(x, y, sum, len);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Dual_Inner_Prod_C(CPointer<opus_val16> x, CPointer<opus_val16> y01, CPointer<opus_val16> y02, c_int N, out opus_val32 xy1, out opus_val32 xy2)
		{
			opus_val32 xy01 = 0;
			opus_val32 xy02 = 0;

			for (c_int i = 0; i < N; i++)
			{
				xy01 = Arch.MAC16_16(xy01, x[i], y01[i]);
				xy02 = Arch.MAC16_16(xy02, x[i], y02[i]);
			}

			xy1 = xy01;
			xy2 = xy02;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Dual_Inner_Prod(CPointer<opus_val16> x, CPointer<opus_val16> y01, CPointer<opus_val16> y02, c_int N, out opus_val32 xy1, out opus_val32 xy2, c_int arch)
		{
			Dual_Inner_Prod_C(x, y01, y02, N, out xy1, out xy2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Inner_Prod_C(CPointer<opus_val16> x, CPointer<opus_val16> y, c_int N)
		{
			opus_val32 xy = 0;

			for (c_int i = 0; i < N; i++)
				xy = Arch.MAC16_16(xy, x[i], y[i]);

			return xy;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Inner_Prod(CPointer<opus_val16> x, CPointer<opus_val16> y, c_int N, c_int arch)
		{
			return Celt_Inner_Prod_C(x, y, N);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Celt_Pitch_Xcorr(CPointer<opus_val16> _x, CPointer<opus_val16> _y, CPointer<opus_val32> xcorr, c_int len, c_int max_pitch, c_int arch)
		{
			Celt_Pitch_Xcorr_C(_x, _y, xcorr, len, max_pitch, arch);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Comb_Filter_Const(CPointer<opus_val32> y, CPointer<opus_val32> x, c_int T, c_int N, opus_val16 g10, opus_val16 g11, opus_val16 g12, c_int arch)
		{
			Celt.Comb_Filter_Const_C(y, x, T, N, g10, g11, g12);
		}
	}
}

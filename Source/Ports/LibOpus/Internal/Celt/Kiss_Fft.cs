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
	internal static class Kiss_Fft
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Kf_Bfly2(Span<Kiss_Fft_Cpx> Fout, c_int m, c_int N)
		{
			{
				celt_coef tw = Arch.QCONST16(0.7071067812f, Constants.Coef_Shift - 1);

				for (c_int i = 0; i < N; i++)
				{
					Span<Kiss_Fft_Cpx> Fout2 = Fout.Slice(4);
					Kiss_Fft_Cpx t = Fout2[0];
					Kiss_Fft_Guts.C_SUB(ref Fout2[0], Fout[0], t);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[0], t);

					t.r = Kiss_Fft_Guts.S_MUL(Arch.ADD32_ovflw(Fout2[1].r, Fout2[1].i), tw);
					t.i = Kiss_Fft_Guts.S_MUL(Arch.SUB32_ovflw(Fout2[1].i, Fout2[1].r), tw);
					Kiss_Fft_Guts.C_SUB(ref Fout2[1], Fout[1], t);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[1], t);

					t.r = Fout2[2].i;
					t.i = Arch.NEG32_ovflw(Fout2[2].r);
					Kiss_Fft_Guts.C_SUB(ref Fout2[2], Fout[2], t);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[2], t);

					t.r = Kiss_Fft_Guts.S_MUL(Arch.SUB32_ovflw(Fout2[3].i, Fout2[3].r), tw);
					t.i = Kiss_Fft_Guts.S_MUL(Arch.NEG32_ovflw(Arch.ADD32_ovflw(Fout2[3].i, Fout2[3].r)), tw);
					Kiss_Fft_Guts.C_SUB(ref Fout2[3], Fout[3], t);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[3], t);

					Fout = Fout.Slice(8);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Kf_Bfly4(Span<Kiss_Fft_Cpx> Fout, size_t fstride, Kiss_Fft_State st, c_int m, c_int N, c_int mm)
		{
			if (m == 1)
			{
				// Degenerate case where all the twiddles are 1
				for (c_int i = 0; i < N; i++)
				{
					Kiss_Fft_Cpx scratch0 = default, scratch1 = default;

					Kiss_Fft_Guts.C_SUB(ref scratch0, Fout[0], Fout[2]);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[0], Fout[2]);
					Kiss_Fft_Guts.C_ADD(ref scratch1, Fout[1], Fout[3]);
					Kiss_Fft_Guts.C_SUB(ref Fout[2], Fout[0], scratch1);
					Kiss_Fft_Guts.C_ADDTO(ref Fout[0], scratch1);
					Kiss_Fft_Guts.C_SUB(ref scratch1, Fout[1], Fout[3]);

					Fout[1].r = Arch.ADD32_ovflw(scratch0.r, scratch1.i);
					Fout[1].i = Arch.SUB32_ovflw(scratch0.i, scratch1.r);
					Fout[3].r = Arch.SUB32_ovflw(scratch0.r, scratch1.i);
					Fout[3].i = Arch.ADD32_ovflw(scratch0.i, scratch1.r);

					Fout = Fout.Slice(4);
				}
			}
			else
			{
				Kiss_Fft_Cpx[] scratch = new Kiss_Fft_Cpx[6];
				c_int m2 = 2 * m;
				c_int m3 = 3 * m;
				Span<Kiss_Fft_Cpx> Fout_beg = Fout;

				for (c_int i = 0; i < N; i++)
				{
					Fout = Fout_beg.Slice(i * mm);

					CPointer<Kiss_Twiddle_Cpx> tw3, tw2, tw1;
					tw3 = tw2 = tw1 = st.twiddles;

					// m is guaranteed to  be a multiple of 4
					for (c_int j = 0; j < m; j++)
					{
						Kiss_Fft_Guts.C_MUL(ref scratch[0], Fout[m], tw1[0]);
						Kiss_Fft_Guts.C_MUL(ref scratch[1], Fout[m2], tw2[0]);
						Kiss_Fft_Guts.C_MUL(ref scratch[2], Fout[m3], tw3[0]);

						Kiss_Fft_Guts.C_SUB(ref scratch[5], Fout[0], scratch[1]);
						Kiss_Fft_Guts.C_ADDTO(ref Fout[0], scratch[1]);
						Kiss_Fft_Guts.C_ADD(ref scratch[3], scratch[0], scratch[2]);
						Kiss_Fft_Guts.C_SUB(ref scratch[4], scratch[0], scratch[2]);
						Kiss_Fft_Guts.C_SUB(ref Fout[m2], Fout[0], scratch[3]);

						tw1 += (int)fstride;
						tw2 += (int)fstride * 2;
						tw3 += (int)fstride * 3;

						Kiss_Fft_Guts.C_ADDTO(ref Fout[0], scratch[3]);

						Fout[m].r = Arch.ADD32_ovflw(scratch[5].r, scratch[4].i);
						Fout[m].i = Arch.SUB32_ovflw(scratch[5].i, scratch[4].r);
						Fout[m3].r = Arch.SUB32_ovflw(scratch[5].r, scratch[4].i);
						Fout[m3].i = Arch.ADD32_ovflw(scratch[5].i, scratch[4].r);

						Fout = Fout.Slice(1);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Kf_Bfly3(Span<Kiss_Fft_Cpx> Fout, size_t fstride, Kiss_Fft_State st, c_int m, c_int N, c_int mm)
		{
			size_t m2 = (size_t)(2 * m);
			Kiss_Fft_Cpx[] scratch = new Kiss_Fft_Cpx[5];

			Span<Kiss_Fft_Cpx> Fout_beg = Fout;
			Kiss_Twiddle_Cpx epi3 = st.twiddles[(int)fstride * m];

			for (c_int i = 0; i < N; i++)
			{
				Fout = Fout_beg.Slice(i * mm);

				CPointer<Kiss_Twiddle_Cpx> tw2, tw1;
				tw1 = tw2 = st.twiddles;

				// For non-custom modes, m is guaranteed to be a multiple of 4
				size_t k = (size_t)m;

				do
				{
					Kiss_Fft_Guts.C_MUL(ref scratch[1], Fout[m], tw1[0]);
					Kiss_Fft_Guts.C_MUL(ref scratch[2], Fout[(int)m2], tw2[0]);

					Kiss_Fft_Guts.C_ADD(ref scratch[3], scratch[1], scratch[2]);
					Kiss_Fft_Guts.C_SUB(ref scratch[0], scratch[1], scratch[2]);

					tw1 += (int)fstride;
					tw2 += (int)fstride * 2;

					Fout[m].r = Arch.SUB32_ovflw(Fout[0].r, Kiss_Fft_Guts.HALF_OF(scratch[3].r));
					Fout[m].i = Arch.SUB32_ovflw(Fout[0].i, Kiss_Fft_Guts.HALF_OF(scratch[3].i));

					Kiss_Fft_Guts.C_MULBYSCALAR(ref scratch[0], epi3.i);

					Kiss_Fft_Guts.C_ADDTO(ref Fout[0], scratch[3]);

					Fout[(int)m2].r = Arch.ADD32_ovflw(Fout[m].r, scratch[0].i);
					Fout[(int)m2].i = Arch.SUB32_ovflw(Fout[m].i, scratch[0].r);

					Fout[m].r = Arch.SUB32_ovflw(Fout[m].r, scratch[0].i);
					Fout[m].i = Arch.ADD32_ovflw(Fout[m].i, scratch[0].r);

					Fout = Fout.Slice(1);
				}
				while (--k != 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Kf_Bfly5(Span<Kiss_Fft_Cpx> Fout, size_t fstride, Kiss_Fft_State st, c_int m, c_int N, c_int mm)
		{
			Kiss_Fft_Cpx[] scratch = new Kiss_Fft_Cpx[13];
			Span<Kiss_Fft_Cpx> Fout_beg = Fout;

			Kiss_Twiddle_Cpx ya = st.twiddles[(int)fstride * m];
			Kiss_Twiddle_Cpx yb = st.twiddles[(int)fstride * 2 * m];
			CPointer<Kiss_Twiddle_Cpx> tw = st.twiddles;

			for (c_int i = 0; i < N; i++)
			{
				Fout = Fout_beg.Slice(i * mm);

				Span<Kiss_Fft_Cpx> Fout0 = Fout;
				Span<Kiss_Fft_Cpx> Fout1 = Fout0.Slice(m);
				Span<Kiss_Fft_Cpx> Fout2 = Fout0.Slice(2 * m);
				Span<Kiss_Fft_Cpx> Fout3 = Fout0.Slice(3 * m);
				Span<Kiss_Fft_Cpx> Fout4 = Fout0.Slice(4 * m);

				// For non-custom modes, m is guaranteed to be a multiple of 4
				for (c_int u = 0; u < m; ++u)
				{
					scratch[0] = Fout0[0];

					Kiss_Fft_Guts.C_MUL(ref scratch[1], Fout1[0], tw[u * (int)fstride]);
					Kiss_Fft_Guts.C_MUL(ref scratch[2], Fout2[0], tw[2 * u * (int)fstride]);
					Kiss_Fft_Guts.C_MUL(ref scratch[3], Fout3[0], tw[3 * u * (int)fstride]);
					Kiss_Fft_Guts.C_MUL(ref scratch[4], Fout4[0], tw[4 * u * (int)fstride]);

					Kiss_Fft_Guts.C_ADD(ref scratch[7], scratch[1], scratch[4]);
					Kiss_Fft_Guts.C_SUB(ref scratch[10], scratch[1], scratch[4]);
					Kiss_Fft_Guts.C_ADD(ref scratch[8], scratch[2], scratch[3]);
					Kiss_Fft_Guts.C_SUB(ref scratch[9], scratch[2], scratch[3]);

					Fout0[0].r = Arch.ADD32_ovflw(Fout0[0].r, Arch.ADD32_ovflw(scratch[7].r, scratch[8].r));
					Fout0[0].i = Arch.ADD32_ovflw(Fout0[0].i, Arch.ADD32_ovflw(scratch[7].i, scratch[8].i));

					scratch[5].r = Arch.ADD32_ovflw(scratch[0].r, Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[7].r, ya.r), Kiss_Fft_Guts.S_MUL(scratch[8].r, yb.r)));
					scratch[5].i = Arch.ADD32_ovflw(scratch[0].i, Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[7].i, ya.r), Kiss_Fft_Guts.S_MUL(scratch[8].i, yb.r)));

					scratch[6].r = Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[10].i, ya.i), Kiss_Fft_Guts.S_MUL(scratch[9].i, yb.i));
					scratch[6].i = Arch.NEG32_ovflw(Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[10].r, ya.i), Kiss_Fft_Guts.S_MUL(scratch[9].r, yb.i)));

					Kiss_Fft_Guts.C_SUB(ref Fout1[0], scratch[5], scratch[6]);
					Kiss_Fft_Guts.C_ADD(ref Fout4[0], scratch[5], scratch[6]);

					scratch[11].r = Arch.ADD32_ovflw(scratch[0].r, Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[7].r, yb.r), Kiss_Fft_Guts.S_MUL(scratch[8].r, ya.r)));
					scratch[11].i = Arch.ADD32_ovflw(scratch[0].i, Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[7].i, yb.r), Kiss_Fft_Guts.S_MUL(scratch[8].i, ya.r)));
					scratch[12].r = Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[9].i, ya.i), Kiss_Fft_Guts.S_MUL(scratch[10].i, yb.i));
					scratch[12].i = Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(scratch[10].r, yb.i), Kiss_Fft_Guts.S_MUL(scratch[9].r, ya.i));

					Kiss_Fft_Guts.C_ADD(ref Fout2[0], scratch[11], scratch[12]);
					Kiss_Fft_Guts.C_SUB(ref Fout3[0], scratch[11], scratch[12]);

					Fout0 = Fout0.Slice(1);
					Fout1 = Fout1.Slice(1);
					Fout2 = Fout2.Slice(1);
					Fout3 = Fout3.Slice(1);
					Fout4 = Fout4.Slice(1);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Opus_Fft_Impl(Kiss_Fft_State st, Span<Kiss_Fft_Cpx> fout, c_int downshift)
		{
			c_int[] fstride = new c_int[Constants.MaxFactors];

			// st->shift can be -1
			c_int shift = st.shift > 0 ? st.shift : 0;

			fstride[0] = 1;
			c_int L = 0;
			c_int m;

			do
			{
				c_int p = st.factors[2 * L];
				m = st.factors[(2 * L) + 1];
				fstride[L + 1] = fstride[L] * p;
				L++;
			}
			while (m != 1);

			m = st.factors[(2 * L) - 1];

			for (c_int i = L - 1; i >= 0; i--)
			{
				c_int m2;

				if (i != 0)
					m2 = st.factors[2 * i - 1];
				else
					m2 = 1;

				switch (st.factors[2 * i])
				{
					case 2:
					{
						Fft_Downshift(fout, st.nfft, ref downshift, 1);
						Kf_Bfly2(fout, m, fstride[i]);
						break;
					}

					case 4:
					{
						Fft_Downshift(fout, st.nfft, ref downshift, 2);
						Kf_Bfly4(fout, (size_t)fstride[i] << shift, st, m, fstride[i], m2);
						break;
					}

					case 3:
					{
						Fft_Downshift(fout, st.nfft, ref downshift, 2);
						Kf_Bfly3(fout, (size_t)fstride[i] << shift, st, m, fstride[i], m2);
						break;
					}

					case 5:
					{
						Fft_Downshift(fout, st.nfft, ref downshift, 3);
						Kf_Bfly5(fout, (size_t)fstride[i] << shift, st, m, fstride[i], m2);
						break;
					}
				}

				m = m2;
			}

			Fft_Downshift(fout, st.nfft, ref downshift, downshift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Fft(Kiss_Fft_State st, CPointer<Kiss_Fft_Cpx> fin, Span<Kiss_Fft_Cpx> fout, c_int arch)
		{
			Opus_Fft_C(st, fin, fout);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opus_Fft_C(Kiss_Fft_State st, CPointer<Kiss_Fft_Cpx> fin, Span<Kiss_Fft_Cpx> fout)
		{
			celt_coef scale = st.scale;

			// Bit-reverse the input
			for (c_int i = 0; i < st.nfft; i++)
			{
				Kiss_Fft_Cpx x = fin[i];
				fout[st.bitrev[i]].r = Kiss_Fft_Guts.S_MUL2(x.r, scale);
				fout[st.bitrev[i]].i = Kiss_Fft_Guts.S_MUL2(x.i, scale);
			}

			Opus_Fft_Impl(st, fout, 0/*scale_shift*/);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Opus_Ifft(Kiss_Fft_State st, CPointer<Kiss_Fft_Cpx> fin, Span<Kiss_Fft_Cpx> fout, c_int arch)
		{
			Opus_Ifft_C(st, fin, fout);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Fft_Downshift(Span<Kiss_Fft_Cpx> x, c_int N, ref c_int total, c_int step)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opus_Ifft_C(Kiss_Fft_State st, CPointer<Kiss_Fft_Cpx> fin, Span<Kiss_Fft_Cpx> fout)
		{
			// Bit-reverse the input
			for (c_int i = 0; i < st.nfft; i++)
				fout[st.bitrev[i]] = fin[i];

			for (c_int i = 0; i < st.nfft; i++)
				fout[i].i = -fout[i].i;

			Opus_Fft_Impl(st, fout, 0);

			for (c_int i = 0; i < st.nfft; i++)
				fout[i].i = -fout[i].i;
		}
	}
}

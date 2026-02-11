/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Mdct
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clt_Mdct_Forward(Mdct_Lookup l, CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, CPointer<opus_val16> window, c_int overlap, c_int shift, c_int stride, c_int arch)
		{
			Clt_Mdct_Forward_C(l, _in, _out, window, overlap, shift, stride, arch);
		}



		/********************************************************************/
		/// <summary>
		/// Forward MDCT trashes the input array
		/// </summary>
		/********************************************************************/
		private static void Clt_Mdct_Forward_C(Mdct_Lookup l, CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, CPointer<celt_coef> window, c_int overlap, c_int shift, c_int stride, c_int arch)
		{
			Kiss_Fft_State st = l.kfft[shift];
			celt_coef scale = st.scale;

			c_int N = l.n;
			CPointer<kiss_twiddle_scalar> trig = l.trig;

			for (c_int i = 0; i < shift; i++)
			{
				N >>= 1;
				trig += N;
			}

			c_int N2 = N >> 1;
			c_int N4 = N >> 2;

			kiss_fft_scalar[] f = new kiss_fft_scalar[N2];
			Kiss_Fft_Cpx[] f2 = new Kiss_Fft_Cpx[N4];

			// Consider the input to be composed of four blocks: [a, b, c, d]
			// Window, shuffle, fold
			{
				CPointer<kiss_fft_scalar> xp1 = _in + (overlap >> 1);
				CPointer<kiss_fft_scalar> xp2 = _in + N2 - 1 + (overlap >> 1);
				CPointer<kiss_fft_scalar> yp = f;
				CPointer<celt_coef> wp1 = window + (overlap >> 1);
				CPointer<celt_coef> wp2 = window + (overlap >> 1) - 1;

				c_int i;
				for (i = 0; i < ((overlap + 3) >> 2); i++)
				{
					// Real part arranged as -d-cR, Imag part arranged as -b+aR
					yp[0] = Kiss_Fft_Guts.S_MUL(xp1[N2], wp2[0]) + Kiss_Fft_Guts.S_MUL(xp2[0], wp1[0]);
					yp[1] = Kiss_Fft_Guts.S_MUL(xp1[0], wp1[0]) - Kiss_Fft_Guts.S_MUL(xp2[-N2], wp2[0]);

					yp += 2;
					xp1 += 2;
					xp2 -= 2;
					wp1 += 2;
					wp2 -= 2;
				}

				wp1 = window;
				wp2 = window + overlap - 1;

				for (; i < (N4 - ((overlap + 3) >> 2)); i++)
				{
					// Real part arranged as a-bR, Imag part arranged as -c-dR
					yp[0] = xp2[0];
					yp[1] = xp1[0];

					yp += 2;
					xp1 += 2;
					xp2 -= 2;
				}

				for (; i < N4; i++)
				{
					// Real part arranged as a-bR, Imag part arranged as -c-dR
					yp[0] = -Kiss_Fft_Guts.S_MUL(xp1[-N2], wp1[0]) + Kiss_Fft_Guts.S_MUL(xp2[0], wp2[0]);
					yp[1] = Kiss_Fft_Guts.S_MUL(xp1[0], wp2[0]) + Kiss_Fft_Guts.S_MUL(xp2[N2], wp1[0]);

					yp += 2;
					xp1 += 2;
					xp2 -= 2;
					wp1 += 2;
					wp2 -= 2;
				}
			}

			// Pre-rotation
			{
				CPointer<kiss_fft_scalar> yp = f;
				CPointer<kiss_twiddle_scalar> t = trig;

				for (c_int i = 0; i < N4; i++)
				{
					Kiss_Fft_Cpx yc;
					kiss_twiddle_scalar t0 = t[i];
					kiss_twiddle_scalar t1 = t[N4 + i];
					kiss_fft_scalar re = yp[0];
					kiss_fft_scalar im = yp[1];
					yp += 2;

					kiss_fft_scalar yr = Kiss_Fft_Guts.S_MUL(re, t0) - Kiss_Fft_Guts.S_MUL(im, t1);
					kiss_fft_scalar yi = Kiss_Fft_Guts.S_MUL(im, t0) + Kiss_Fft_Guts.S_MUL(re, t1);

					yc.r = Kiss_Fft_Guts.S_MUL2(yr, scale);
					yc.i = Kiss_Fft_Guts.S_MUL2(yi, scale);

					f2[st.bitrev[i]] = yc;
				}
			}

			// N/4 complex FFT, does not downscale anymore
			Kiss_Fft.Opus_Fft_Impl(st, f2, 0/*scale_shift - headroom*/);

			// Post-rotate
			{
				CPointer<Kiss_Fft_Cpx> fp = f2;
				CPointer<kiss_fft_scalar> yp1 = _out;
				CPointer<kiss_fft_scalar> yp2 = _out + stride * (N2 - 1);
				CPointer<kiss_twiddle_scalar> t = trig;

				for (c_int i = 0; i < N4; i++)
				{
					kiss_fft_scalar t0 = t[i];
					kiss_fft_scalar t1 = t[N4 + i];

					kiss_fft_scalar yr = Arch.PSHR32(Kiss_Fft_Guts.S_MUL(fp[0].i, t1) - Kiss_Fft_Guts.S_MUL(fp[0].r, t0), 0/*headroom*/);
					kiss_fft_scalar yi = Arch.PSHR32(Kiss_Fft_Guts.S_MUL(fp[0].r, t1) + Kiss_Fft_Guts.S_MUL(fp[0].i, t0), 0/*headroom*/);
					yp1[0] = yr;
					yp2[0] = yi;

					fp++;
					yp1 += 2 * stride;
					yp2 -= 2 * stride;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clt_Mdct_Backward(Mdct_Lookup l, CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, CPointer<opus_val16> window, c_int overlap, c_int shift, c_int stride, c_int arch)
		{
			Clt_Mdct_Backward_C(l, _in, _out, window, overlap, shift, stride, arch);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Clt_Mdct_Backward_C(Mdct_Lookup l, CPointer<kiss_fft_scalar> _in, CPointer<kiss_fft_scalar> _out, CPointer<celt_coef> window, c_int overlap, c_int shift, c_int stride, c_int arch)
		{
			c_int N = l.n;
			CPointer<kiss_twiddle_scalar> trig = l.trig;

			for (c_int i = 0; i < shift; i++)
			{
				N >>= 1;
				trig += N;
			}

			c_int N2 = N >> 1;
			c_int N4 = N >> 2;

			// Pre-rotate
			{
				// Temp pointers to make it really clear to the compiler what we're doing
				CPointer<kiss_fft_scalar> xp1 = _in;
				CPointer<kiss_fft_scalar> xp2 = _in + stride * (N2 - 1);
				CPointer<kiss_fft_scalar> yp = _out + (overlap >> 1);
				CPointer<kiss_twiddle_scalar> t = trig;
				CPointer<opus_int16> bitrev = l.kfft[shift].bitrev;

				for (c_int i = 0; i < N4; i++)
				{
					c_int rev = bitrev[0, 1];

					opus_val32 x1 = Arch.SHL32_ovflw(xp1[0], 0/*pre_shift*/);
					opus_val32 x2 = Arch.SHL32_ovflw(xp2[0], 0/*pre_shift*/);

					kiss_fft_scalar yr = Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(x2, t[i]), Kiss_Fft_Guts.S_MUL(x1, t[N4 + i]));
					kiss_fft_scalar yi = Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(x1, t[i]), Kiss_Fft_Guts.S_MUL(x2, t[N4 + i]));

					// We swap real and imag because we use an FFT instead of an IFFT
					yp[(2 * rev) + 1] = yr;
					yp[2 * rev] = yi;

					// Storing the pre-rotation directly in the bitrev order
					xp1 += 2 * stride;
					xp2 -= 2 * stride;
				}
			}

			Kiss_Fft.Opus_Fft_Impl(l.kfft[shift], MemoryMarshal.Cast<kiss_fft_scalar, Kiss_Fft_Cpx>((_out + (overlap >> 1)).AsSpan()), 0/*fft_shift*/);

			// Post-rotate and de-shuffle from both ends of the buffer at once to make
			// it in-place
			{
				CPointer<kiss_fft_scalar> yp0 = _out + (overlap >> 1);
				CPointer<kiss_fft_scalar> yp1 = _out + (overlap >> 1) + N2 - 2;
				CPointer<kiss_twiddle_scalar> t = trig;

				// Loop to (N4+1)>>1 to handle odd N4. When N4 is odd, the
				// middle pair will be computed twice
				for (c_int i = 0; i < ((N4 + 1) >> 1); i++)
				{
					// We swap real and imag because we're using an FFT instead of an IFFT
					kiss_fft_scalar re = yp0[1];
					kiss_fft_scalar im = yp0[0];
					kiss_twiddle_scalar t0 = t[i];
					kiss_twiddle_scalar t1 = t[N4 + i];

					// We'd scale up by 2 here, but instead it's done when mixing the windows
					kiss_fft_scalar yr = Arch.PSHR32_ovflw(Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(re, t0), Kiss_Fft_Guts.S_MUL(im, t1)), 0/*post_shift*/);
					kiss_fft_scalar yi = Arch.PSHR32_ovflw(Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(re, t1), Kiss_Fft_Guts.S_MUL(im, t0)), 0/*post_shift*/);

					// We swap real and imag because we're using an FFT instead of an IFFT
					re = yp1[1];
					im = yp1[0];
					yp0[0] = yr;
					yp1[1] = yi;

					t0 = t[N4 - i - 1];
					t1 = t[N2 - i - 1];

					// We'd scale up by 2 here, but instead it's done when mixing the windows
					yr = Arch.PSHR32_ovflw(Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(re, t0), Kiss_Fft_Guts.S_MUL(im, t1)), 0/*post_shift*/);
					yi = Arch.PSHR32_ovflw(Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(re, t1), Kiss_Fft_Guts.S_MUL(im, t0)), 0/*post_shift*/);

					yp1[0] = yr;
					yp0[1] = yi;

					yp0 += 2;
					yp1 -= 2;
				}
			}

			// Mirror on both sides for TDAC
			{
				CPointer<kiss_fft_scalar> xp1 = _out + overlap - 1;
				CPointer<kiss_fft_scalar> yp1 = _out;
				CPointer<celt_coef> wp1 = window;
				CPointer<celt_coef> wp2 = window + overlap - 1;

				for (c_int i = 0; i < (overlap / 2); i++)
				{
					kiss_fft_scalar x1 = xp1[0];
					kiss_fft_scalar x2 = yp1[0];

					yp1[0, 1] = Arch.SUB32_ovflw(Kiss_Fft_Guts.S_MUL(x2, wp2[0]), Kiss_Fft_Guts.S_MUL(x1, wp1[0]));
					xp1[0, -1] = Arch.ADD32_ovflw(Kiss_Fft_Guts.S_MUL(x2, wp1[0]), Kiss_Fft_Guts.S_MUL(x1, wp2[0]));

					wp1++;
					wp2--;
				}
			}
		}
	}
}

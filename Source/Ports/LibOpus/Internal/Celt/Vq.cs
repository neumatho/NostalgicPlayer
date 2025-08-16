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
	internal static class Vq
	{
		private static readonly c_int[] spread_factor = [ 15, 10, 5 ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Exp_Rotation1(CPointer<celt_norm> X, c_int len, c_int stride, opus_val16 c, opus_val16 s)
		{
			CPointer<celt_norm> Xptr = X;

			opus_val16 ms = Arch.NEG16(s);

			for (c_int i = 0; i < (len - stride); i++)
			{
				celt_norm x1 = Xptr[0];
				celt_norm x2 = Xptr[stride];

				Xptr[stride] = Arch.EXTRACT16(Arch.PSHR32(Arch.MAC16_16(Arch.MULT16_16(c, x2), s, x1), 15));
				Xptr[0, 1] = Arch.EXTRACT16(Arch.PSHR32(Arch.MAC16_16(Arch.MULT16_16(c, x1), ms, x2), 15));
			}

			Xptr = X + len - 2 * stride - 1;

			for (c_int i = len - 2 * stride - 1; i >= 0; i--)
			{
				celt_norm x1 = Xptr[0];
				celt_norm x2 = Xptr[stride];

				Xptr[stride] = Arch.EXTRACT16(Arch.PSHR32(Arch.MAC16_16(Arch.MULT16_16(c, x2), s, x1), 15));
				Xptr[0, -1] = Arch.EXTRACT16(Arch.PSHR32(Arch.MAC16_16(Arch.MULT16_16(c, x1), ms, x2), 15));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Exp_Rotation(CPointer<celt_norm> X, c_int len, c_int dir, c_int stride, c_int K, Spread spread)
		{
			c_int stride2 = 0;

			if (((2 * K) >= len) || (spread == Spread.None))
				return;

			c_int factor = spread_factor[(c_int)(spread) - 1];

			opus_val16 gain = MathOps.Celt_Div(Arch.MULT16_16(Constants.Q15One, len), len + factor * K);
			opus_val16 theta = Arch.HALF16(Arch.MULT16_16_Q15(gain, gain));

			opus_val16 c = MathOps.Celt_Cos_Norm(Arch.EXTEND32(theta));
			opus_val16 s = MathOps.Celt_Cos_Norm(Arch.EXTEND32(Arch.SUB16(Constants.Q15One, theta)));	// sin(theta)

			if (len >= (8 * stride))
			{
				stride2 = 1;

				// This is just a simple (equivalent) way of computing sqrt(len/stride) with rounding.
				// It's basically incrementing long as (stride2 + 0.5)^2 < len/stride
				while (((stride2 * stride2 + stride2) * stride + (stride >> 2)) < len)
					stride2++;
			}

			// NOTE: As a minor optimization, we could be passing around log2(B), not B, for both this and for
			// extract_collapse_mask()
			len = (c_int)EntCode.Celt_UDiv((opus_uint32)len, (opus_uint32)stride);

			for (c_int i = 0; i < stride; i++)
			{
				if (dir < 0)
				{
					if (stride2 != 0)
						Exp_Rotation1(X + i * len, len, stride2, s, c);

					Exp_Rotation1(X + i * len, len, 1, c, s);
				}
				else
				{
					Exp_Rotation1(X + i * len, len, 1, c, -s);

					if (stride2 != 0)
						Exp_Rotation1(X + i * len, len, stride2, s, -c);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Takes the pitch vector and the decoded residual vector, computes
		/// the gain that will give ||p+g*y||=1 and mixes the residual with
		/// the pitch
		/// </summary>
		/********************************************************************/
		private static void Normalise_Residual(CPointer<c_int> iy, CPointer<celt_norm> X, c_int N, opus_val32 Ryy, opus_val16 gain)
		{
			opus_val32 t = Arch.VSHR32(Ryy, 0/*2 * (k - 7)*/);
			opus_val16 g = Arch.MULT16_16_P15(MathOps.Celt_Rsqrt_Norm(t), gain);

			c_int i = 0;
			do
			{
				X[i] = Arch.EXTRACT16(Arch.PSHR32(Arch.MULT16_16(g, iy[i]), 0/*k + 1*/));
			}
			while (++i < N);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint Extract_Collapse_Mask(CPointer<c_int> iy, c_int N, c_int B)
		{
			if (B <= 1)
				return 1;

			// NOTE: As a minor optimization, we could be passing around log2(B), not B, for both this and for
			// exp_rotation()
			c_int N0 = (c_int)EntCode.Celt_UDiv((opus_uint32)N, (opus_uint32)B);
			c_uint colapse_mask = 0;
			c_int i = 0;

			do
			{
				c_uint tmp = 0;
				c_int j = 0;

				do
				{
					tmp |= (c_uint)iy[i * N0 + j];
				}
				while (++j < N0);

				colapse_mask |= (tmp != 0 ? 1U : 0U) << i;
			}
			while (++i < B);

			return colapse_mask;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static opus_val16 Op_Pvq_Search_C(CPointer<celt_norm> X, CPointer<c_int> iy, c_int K, c_int N, c_int arch)
		{
			celt_norm[] y = new celt_norm[N];
			c_int[] signx = new c_int[N];

			// Get rid of the sign
			opus_val32 sum = 0;
			c_int j = 0;

			do
			{
				signx[j] = X[j] < 0 ? 1 : 0;
				X[j] = Math.Abs(X[j]);

				iy[j] = 0;
				y[j] = 0;
			}
			while (++j < N);

			opus_val32 xy = 0, yy = 0;

			c_int pulsesLeft = K;

			// Do a pre-search by projecting on the pyramid
			if (K > (N >> 1))
			{
				j = 0;

				do
				{
					sum += X[j];
				}
				while (++j < N);

				// Prevents infinities and NaNs from causing too many pulses
				// to be allocated. 64 is an approximation of infinity here
				if (!((sum > Constants.Epsilon) && (sum < 64)))
				{
					X[0] = Arch.QCONST16(1.0f, 14);
					j = 1;

					do
					{
						X[j] = 0;
					}
					while (++j < N);

					sum = Arch.QCONST16(1.0f, 14);
				}

				// Using K+e with e < 1 guarantees we cannot get more than K pulses
				opus_val16 rcp = Arch.EXTRACT16(Arch.MULT16_32_Q16(K + 0.8f, MathOps.Celt_Rcp(sum)));
				j = 0;

				do
				{
					iy[j] = (c_int)Math.Floor(rcp * X[j]);
					y[j] = iy[j];
					yy = Arch.MAC16_16(yy, y[j], y[j]);
					xy = Arch.MAC16_16(xy, X[j], y[j]);
					y[j] *= 2;
					pulsesLeft -= iy[j];
				}
				while (++j < N);
			}

			// This should never happen, but just in case it does (e.g. on silence)
			// we fill the first bin with pulses
			if (pulsesLeft > (N + 3))
			{
				opus_val16 tmp = pulsesLeft;
				yy = Arch.MAC16_16(yy, tmp, tmp);
				yy = Arch.MAC16_16(yy, tmp, y[0]);
				iy[0] += pulsesLeft;
				pulsesLeft = 0;
			}

			for (c_int i = 0; i < pulsesLeft; i++)
			{
				c_int best_id = 0;

				// The squared magnitude term gets added anyway, so we might as well
				// add it outside the loop
				yy = Arch.ADD16(yy, 1);

				// Calculations for position 0 are out of the loop, in part to reduce
				// mispredicted branches (since the if condition is usually false)
				// in the loop.
				//
				// Temporary sums of the new pulse(s)
				opus_val16 Rxy = Arch.EXTRACT16(Arch.SHR32(Arch.ADD32(xy, Arch.EXTEND32(X[0])), 0/*rshift*/));

				// We're multiplying y[j] by two so we don't have to do it here
				opus_val16 Ryy = Arch.ADD16(yy, y[0]);

				// Approximate score: we maximise Rxy/sqrt(Ryy) (we're guaranteed that
				// Rxy is positive because the sign is pre-computed)
				Rxy = Arch.MULT16_16_Q15(Rxy, Rxy);
				opus_val16 best_den = Ryy;
				opus_val32 best_num = Rxy;
				j = 1;

				do
				{
					// Temporary sums of the new pulse(s)
					Rxy = Arch.EXTRACT16(Arch.SHR32(Arch.ADD32(xy, Arch.EXTEND32(X[j])), 0/*rshift*/));

					// We're multiplying y[j] by two so we don't have to do it here
					Ryy = Arch.ADD16(yy, y[j]);

					// Approximate score: we maximise Rxy/sqrt(Ryy) (we're guaranteed that
					// Rxy is positive because the sign is pre-computed)
					Rxy = Arch.MULT16_16_Q15(Rxy, Rxy);

					// The idea is to check for num/den >= best_num/best_den, but that way
					// we can do it without any division
					if (Arch.MULT16_16(best_den, Rxy) > Arch.MULT16_16(Ryy, best_num))
					{
						best_den = Ryy;
						best_num = Rxy;
						best_id = j;
					}
				}
				while (++j < N);

				// Updating the sums of the new pulse(s)
				xy = Arch.ADD32(xy, Arch.EXTEND32(X[best_id]));

				// We're multiplying y[j] by two so we don't have to do it here
				yy = Arch.ADD16(yy, y[best_id]);

				// Only now that we've made the final choice, update y/iy
				// Multiplying y[j] by 2 so we don't have to do it everywhere else
				y[best_id] += 2;
				iy[best_id]++;
			}

			// Put the original sign back
			j = 0;

			do
			{
				iy[j] = (iy[j] ^ -signx[j]) + signx[j];
			}
			while (++j < N);

			return yy;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val16 Op_Pvq_Search(CPointer<celt_norm> X, CPointer<c_int> iy, c_int K, c_int N, c_int arch)
		{
			return Op_Pvq_Search_C(X, iy, K, N, arch);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint Alg_Quant(CPointer<celt_norm> X, c_int N, c_int K, Spread spread, c_int B, Ec_Enc enc, opus_val16 gain, bool resynth, c_int arch)
		{
			// Covers vectorization by up to 4
			c_int[] iy = new c_int[N + 3];

			Exp_Rotation(X, N, 1, B, K, spread);

			opus_val16 yy = Op_Pvq_Search(X, iy, K, N, arch);

			Cwrs.Encode_Pulses(iy, N, K, enc);

			if (resynth)
			{
				Normalise_Residual(iy, X, N, yy, gain);
				Exp_Rotation(X, N, -1, B, K, spread);
			}

			c_uint collapse_mask = Extract_Collapse_Mask(iy, N, B);

			return collapse_mask;
		}



		/********************************************************************/
		/// <summary>
		/// Decode pulse vector and combine the result with the pitch vector
		/// to produce the final normalised signal in the current band
		/// </summary>
		/********************************************************************/
		public static c_uint Alg_Unquant(CPointer<celt_norm> X, c_int N, c_int K, Spread spread, c_int B, Ec_Dec dec, opus_val16 gain)
		{
			c_int[] iy = new c_int[N];

			opus_val32 Ryy = Cwrs.Decode_Pulses(iy, N, K, dec);
			Normalise_Residual(iy, X, N, Ryy, gain);
			Exp_Rotation(X, N, -1, B, K, spread);

			c_uint collapse_mask = Extract_Collapse_Mask(iy, N, B);

			return collapse_mask;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Renormalise_Vector(CPointer<celt_norm> X, c_int N, opus_val16 gain, c_int arch)
		{
			opus_val32 E = Constants.Epsilon + Pitch.Celt_Inner_Prod(X, X, N, arch);
			opus_val32 t = Arch.VSHR32(E, 0/*2 * (k - 7)*/);
			opus_val16 g = Arch.MULT16_16_P15(MathOps.Celt_Rsqrt_Norm(t), gain);

			CPointer<celt_norm> xptr = X;
			for (c_int i = 0; i < N; i++)
				xptr[0, 1] = Arch.EXTRACT16(Arch.PSHR32(Arch.MULT16_16(g, xptr[0]), 0/*k + 1*/));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Stereo_Itheta(CPointer<celt_norm> X, CPointer<celt_norm> Y, bool stereo, c_int N, c_int arch)
		{
			opus_val32 Emid = Constants.Epsilon;
			opus_val32 Eside = Constants.Epsilon;

			if (stereo)
			{
				for (c_int i = 0; i < N; i++)
				{
					celt_norm m = Arch.ADD16(Arch.SHR16(X[i], 1), Arch.SHR16(Y[i], 1));
					celt_norm s = Arch.SUB16(Arch.SHR16(X[i], 1), Arch.SHR16(Y[i], 1));

					Emid = Arch.MAC16_16(Emid, m, m);
					Eside = Arch.MAC16_16(Eside, s, s);
				}
			}
			else
			{
				Emid += Pitch.Celt_Inner_Prod(X, X, N, arch);
				Eside += Pitch.Celt_Inner_Prod(Y, Y, N, arch);
			}

			opus_val16 mid = MathOps.Celt_Sqrt(Emid);
			opus_val16 side = MathOps.Celt_Sqrt(Eside);

			c_int itheta = (c_int)Math.Floor(0.5f + 16384 * 0.63662f * MathOps.Fast_Atan2F(side, mid));

			return itheta;
		}
	}
}

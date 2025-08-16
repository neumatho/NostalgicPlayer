/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Bands
	{
		// Indexing table for converting from natural Hadamard to ordery Hadamard
		// This is essentially a bit-reversed Gray, on top of which we've added
		// an inversion of the order because we want the DC at the end rather than
		// the beginning. The lines are for N=2, 4, 8, 16
		private static readonly c_int[] ordery_table =
		[
			 1,  0,
			 3,  0,  2,  1,
			 7,  0,  4,  3,  6,  1,  5,  2,
			15,  0,  8,  7, 12,  3, 11,  4, 14,  1,  9,  6, 13,  2, 10,  5
		];

		private static readonly opus_int16[] exp2_table8 = [ 16384, 17866, 19483, 21247, 23170, 25267, 27554, 30048 ];

		private static readonly byte[] bit_interleave_table = [ 0, 1, 1, 1, 2, 3, 3, 3, 2, 3, 3, 3, 2, 3, 3, 3 ];
		private static readonly byte[] bit_deinterleave_table = [ 0x00, 0x03, 0x0c, 0x0f, 0x30, 0x33, 0x3c, 0x3f, 0xc0, 0xc3, 0xcc, 0xcf, 0xf0, 0xf3, 0xfc, 0xff ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint32 Celt_Lcg_Rand(opus_uint32 seed)
		{
			return 1664525 * seed + 1013904223;
		}



		/********************************************************************/
		/// <summary>
		/// This is a cos() approximation designed to be bit-exact on any
		/// platform. Bit exactness with this approximation is important
		/// because it has an impact on the bit allocation
		/// </summary>
		/********************************************************************/
		public static opus_int16 Bitexact_Cos(opus_int16 x)
		{
			opus_int32 tmp = (4096 + (x * x)) >> 13;
			opus_int16 x2 = (opus_int16)tmp;
			x2 = (opus_int16)((32767 - x2) + MathOps.Frac_MUL16(x2, (opus_int16)(-7651 + MathOps.Frac_MUL16(x2, (opus_int16)(8277 + MathOps.Frac_MUL16(-626, x2))))));

			return (opus_int16)(1 + x2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Bitexact_Log2Tan(c_int isin, c_int icos)
		{
			c_int lc = EntCode.Ec_Ilog((opus_uint32)icos);
			c_int ls = EntCode.Ec_Ilog((opus_uint32)isin);
			icos <<= 15 - lc;
			isin <<= 15 - ls;

			return (ls - lc) * (1 << 11)
				+ MathOps.Frac_MUL16((opus_int16)isin, (opus_int16)(MathOps.Frac_MUL16((opus_int16)isin, -2597) + 7932))
				- MathOps.Frac_MUL16((opus_int16)icos, (opus_int16)(MathOps.Frac_MUL16((opus_int16)icos, -2597) + 7932));
		}



		/********************************************************************/
		/// <summary>
		/// De-normalise the energy to produce the synthesis from the
		/// unit-energy bands
		/// </summary>
		/********************************************************************/
		public static void Denormalise_Bands(CeltMode m, CPointer<celt_norm> X, CPointer<celt_sig> freq, CPointer<opus_val16> bandLogE, c_int start, c_int end, c_int M, c_int downsample, bool silence)
		{
			CPointer<opus_int16> eBands = m.eBands;
			c_int N = M * m.shortMdctSize;
			c_int bound = M * eBands[end];

			if (downsample != 1)
				bound = Arch.IMIN(bound, N / downsample);

			if (silence)
			{
				bound = 0;
				start = end = 0;
			}

			CPointer<celt_sig> f = freq;
			CPointer<celt_norm> x = X + M * eBands[start];

			for (c_int i = 0; i < M * eBands[start]; i++)
				f[0, 1] = 0;

			for (c_int i = start; i < end; i++)
			{
				c_int j = M * eBands[i];
				c_int band_end = M * eBands[i + 1];
				opus_val16 lg = Arch.SATURATE16(Arch.ADD32(bandLogE[i], Arch.SHL32(Tables.EMeans[i], 6)));
				opus_val16 g = MathOps.Celt_Exp2(Arch.MIN32(32.0f, lg));

				do
				{
					f[0, 1] = Arch.SHR32(Arch.MULT16_16(x[0, 1], g), 0/*shift*/);
				}
				while (++j < band_end);
			}

			Memory.Opus_Clear(freq + bound, N - bound);
		}



		/********************************************************************/
		/// <summary>
		/// This prevents energy collapse for transients with multiple short
		/// MDCTs
		/// </summary>
		/********************************************************************/
		public static void Anti_Collapse(CeltMode m, CPointer<celt_norm> X_, CPointer<byte> collapse_masks, c_int LM, c_int C, c_int size, c_int start, c_int end, CPointer<opus_val16> logE, CPointer<opus_val16> prev1logE, CPointer<opus_val16> prev2logE, CPointer<c_int> pulses, opus_uint32 seed, c_int arch)
		{
			for (c_int i = start; i < end; i++)
			{
				c_int N0 = m.eBands[i + 1] - m.eBands[i];

				// Depth in 1/8 bits
				c_int depth = (c_int)(EntCode.Celt_UDiv((opus_uint32)(1 + pulses[i]), (opus_uint32)(m.eBands[i + 1] - m.eBands[i])) >> LM);

				opus_val16 thresh = 0.5f * MathOps.Celt_Exp2(-0.125f * depth);
				opus_val16 sqrt_1 = MathOps.Celt_Rsqrt(N0 << LM);

				c_int c = 0;

				do
				{
					bool renormalize = false;
					opus_val16 prev1 = prev1logE[c * m.nbEBands + i];
					opus_val16 prev2 = prev2logE[c * m.nbEBands + i];

					if (C == 1)
					{
						prev1 = Arch.MAX16(prev1, prev1logE[m.nbEBands + i]);
						prev2 = Arch.MAX16(prev2, prev2logE[m.nbEBands + i]);
					}

					opus_val32 Ediff = Arch.EXTEND32(logE[c * m.nbEBands + i]) - Arch.EXTEND32(Arch.MIN16(prev1, prev2));
					Ediff = Arch.MAX32(0, Ediff);

					// r needs to be multiplied by 2 or 2*sqrt(2) depending on LM because
					// short blocks don't have the same energy as long
					opus_val16 r = 2.0f * MathOps.Celt_Exp2(-Ediff);

					if (LM == 3)
						r *= 1.41421356f;

					r = Arch.MIN16(thresh, r);
					r = r * sqrt_1;

					CPointer<celt_norm> X = X_ + c * size + (m.eBands[i] << LM);

					for (c_int k = 0; k < (1 << LM); k++)
					{
						// Detect collapse
						if ((collapse_masks[i * C + c] & 1 << k) == 0)
						{
							// Fill with noise
							for (c_int j = 0; j < N0; j++)
							{
								seed = Celt_Lcg_Rand(seed);
								X[(j << LM) + k] = (seed & 0x8000) != 0 ? r : -r;
							}

							renormalize = true;
						}
					}

					// We just added some energy, so we need to renormalise
					if (renormalize)
						Vq.Renormalise_Vector(X, N0 << LM, Constants.Q15One, arch);
				}
				while (++c < C);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Compute the weights to use for optimizing normalized distortion
		/// across channels. We use the amplitude to weight square
		/// distortion, which means that we use the square root of the value
		/// we would have been using if we wanted to minimize the MSE in the
		/// non-normalized domain. This roughly corresponds to some
		/// quick-and-dirty perceptual experiments I ran to measure
		/// inter-aural masking (there doesn't seem to be any published data
		/// on the topic)
		/// </summary>
		/********************************************************************/
		private static void Compute_Channel_Weights(celt_ener Ex, celt_ener Ey, opus_val16[] w)
		{
			celt_ener minE = Arch.MIN32(Ex, Ey);

			// Adjustment to make the weights a bit more conservative
			Ex = Arch.ADD32(Ex, minE / 3);
			Ey = Arch.ADD32(Ey, minE / 3);

			w[0] = Arch.VSHR32(Ex, 0/*shift*/);
			w[1] = Arch.VSHR32(Ey, 0/*shift*/);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Intensity_Stereo(CeltMode m, CPointer<celt_norm> X, CPointer<celt_norm> Y, CPointer<celt_ener> bandE, c_int bandID, c_int N)
		{
			c_int i = bandID;
			opus_val16 left = Arch.VSHR32(bandE[i], 0/*shift*/);
			opus_val16 right = Arch.VSHR32(bandE[i + m.nbEBands], 0/*shift*/);
			opus_val16 norm = Constants.Epsilon + MathOps.Celt_Sqrt(Constants.Epsilon + Arch.MULT16_16(left, left) + Arch.MULT16_16(right, right));
			opus_val16 a1 = Arch.DIV32_16(Arch.SHL32(Arch.EXTEND32(left), 14), norm);
			opus_val16 a2 = Arch.DIV32_16(Arch.SHL32(Arch.EXTEND32(right), 14), norm);

			for (c_int j = 0; j < N; j++)
			{
				celt_norm l = X[j];
				celt_norm r = Y[j];

				X[j] = Arch.EXTRACT16(Arch.SHR32(Arch.MAC16_16(Arch.MULT16_16(a1, l), a2, r), 14));

				// Side is not encoded, no need to calculate
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Stereo_Split(CPointer<celt_norm> X, CPointer<celt_norm> Y, c_int N)
		{
			for (c_int j = 0; j < N; j++)
			{
				opus_val32 l = Arch.MULT16_16(Arch.QCONST16(0.70710678f, 15), X[j]);
				opus_val32 r = Arch.MULT16_16(Arch.QCONST16(0.70710678f, 15), Y[j]);

				X[j] = Arch.EXTRACT16(Arch.SHR32(Arch.ADD32(l, r), 15));
				Y[j] = Arch.EXTRACT16(Arch.SHR32(Arch.SUB32(r, l), 15));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Stereo_Merge(CPointer<celt_norm> X, CPointer<celt_norm> Y, opus_val16 mid, c_int N, c_int arch)
		{
			// Compute the norm of x+y and x-y as |x|^2 + |y|^2 +/- sum(xy)
			Pitch.Dual_Inner_Prod(Y, X, Y, N, out opus_val32 xp, out opus_val32 side, arch);

			// Compensating for the mid normalization
			xp = Arch.MULT16_32_Q15(mid, xp);

			// mid and side are in Q15, not Q14 like X and Y
			opus_val16 mid2 = Arch.SHR16(mid, 1);
			opus_val32 El = Arch.MULT16_16(mid2, mid2) + side - 2 * xp;
			opus_val32 Er = Arch.MULT16_16(mid2, mid2) + side + 2 * xp;

			if ((Er < Arch.QCONST32(6e-4f, 28)) || (El < Arch.QCONST32(6e-4f, 28)))
			{
				Memory.Opus_Copy(Y, X, N);
				return;
			}

			opus_val32 t = Arch.VSHR32(El, 0/*(kl - 7) << 1*/);
			opus_val32 lgain = MathOps.Celt_Rsqrt_Norm(t);
			t = Arch.VSHR32(Er, 0/*(kr - 7) << 1*/);
			opus_val32 rgain = MathOps.Celt_Rsqrt_Norm(t);

			for (c_int j = 0; j < N; j++)
			{
				// Apply mid scaling (side is already scaled)
				celt_norm l = Arch.MULT16_16_P15(mid, X[j]);
				celt_norm r = Y[j];

				X[j] = Arch.EXTRACT16(Arch.PSHR32(Arch.MULT16_16(lgain, Arch.SUB16(l, r)), 0/*kl + 1*/));
				Y[j] = Arch.EXTRACT16(Arch.PSHR32(Arch.MULT16_16(rgain, Arch.ADD16(l, r)), 0/*kr + 1*/));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Deinterleave_Hadamard(CPointer<celt_norm> X, c_int N0, c_int stride, bool hadamard)
		{
			c_int N = N0 * stride;
			celt_norm[] tmp = new celt_norm[N];

			if (hadamard)
			{
				CPointer<c_int> ordery = new CPointer<c_int>(ordery_table, stride - 2);

				for (c_int i = 0; i < stride; i++)
				{
					for (c_int j = 0; j < N0; j++)
						tmp[ordery[i] * N0 + j] = X[j * stride + i];
				}
			}
			else
			{
				for (c_int i = 0; i < stride; i++)
				{
					for (c_int j = 0; j < N0; j++)
						tmp[i * N0 + j] = X[j * stride + i];
				}
			}

			Memory.Opus_Copy(X, tmp, N);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Interleave_Hadamard(CPointer<celt_norm> X, c_int N0, c_int stride, bool hadamard)
		{
			c_int N = N0 * stride;
			celt_norm[] tmp = new celt_norm[N];

			if (hadamard)
			{
				CPointer<c_int> ordery = new CPointer<c_int>(ordery_table, stride - 2);

				for (c_int i = 0; i < stride; i++)
				{
					for (c_int j = 0; j < N0; j++)
						tmp[j * stride + i] = X[ordery[i] * N0 + j];
				}
			}
			else
			{
				for (c_int i = 0; i < stride; i++)
				{
					for (c_int j = 0; j < N0; j++)
						tmp[j * stride + i] = X[i * N0 + j];
				}
			}

			Memory.Opus_Copy(X, tmp, N);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Haar1(CPointer<celt_norm> X, c_int N0, c_int stride)
		{
			N0 >>= 1;

			for (c_int i = 0; i < stride; i++)
			{
				for (c_int j = 0; j < N0; j++)
				{
					opus_val32 tmp1 = Arch.MULT16_16(Arch.QCONST16(0.70710678f, 15), X[stride * 2 * j + i]);
					opus_val32 tmp2 = Arch.MULT16_16(Arch.QCONST16(0.70710678f, 15), X[stride * (2 * j + 1) + i]);

					X[stride * 2 * j + i] = Arch.EXTRACT16(Arch.PSHR32(Arch.ADD32(tmp1, tmp2), 15));
					X[stride * (2 * j + 1) + i] = Arch.EXTRACT16(Arch.PSHR32(Arch.SUB32(tmp1, tmp2), 15));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Compute_Qn(c_int N, c_int b, c_int offset, c_int pulse_cap, bool stereo)
		{
			c_int N2 = 2 * N - 1;

			if (stereo && (N == 2))
				N2--;

			// The upper limit ensures that in a stereo split with itheta==16384, we'll
			// always have enough bits left over to code at least one pulse in the
			// side; otherwise it would collapse, since it doesn't get folded
			c_int qb = EntCode.Celt_SUDiv(b + N2 * offset, N2);
			qb = Arch.IMIN(b - pulse_cap - (4 << Constants.BitRes), qb);

			qb = Arch.IMIN(8 << Constants.BitRes, qb);

			c_int qn;
			if (qb < (1 << Constants.BitRes >> 1))
				qn = 1;
			else
			{
				qn = exp2_table8[qb & 0x7] >> (14 - (qb >> Constants.BitRes));
				qn = (qn + 1) >> 1 << 1;
			}

			return qn;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Compute_Theta(Band_Ctx ctx, out Split_Ctx sctx, CPointer<celt_norm> X, CPointer<celt_norm> Y, c_int N, ref c_int b, c_int B, c_int B0, c_int LM, bool stereo, ref c_int fill)
		{
			c_int itheta = 0;
			c_int delta;
			c_int imid, iside;
			bool inv = false;

			bool encode = ctx.encode;
			CeltMode m = ctx.m;
			c_int i = ctx.i;
			c_int intensity = ctx.intensity;
			Ec_Ctx ec = ctx.ec;
			CPointer<celt_ener> bandE = ctx.bandE;

			// Decide on the resolution to give to the split parameter theta
			c_int pulse_cap = m.logN[i] + LM * (1 << Constants.BitRes);
			c_int offset = (pulse_cap >> 1) - (stereo && (N == 2) ? Constants.QTheta_Offset_TwoPhase : Constants.QTheta_Offset);
			c_int qn = Compute_Qn(N, b, offset, pulse_cap, stereo);

			if (stereo && (i >= intensity))
				qn = 1;

			if (encode)
			{
				// Theta is the atan() of the ratio between the (normalized)
				// side and mid. With just that parameter, we can re-scale both
				// mid and side because we know that 1) they have unit norm and
				// 2) they are orthogonal
				itheta = Vq.Stereo_Itheta(X, Y, stereo, N, ctx.arch);
			}

			opus_int32 tell = (opus_int32)EntCode.Ec_Tell_Frac(ec);

			if (qn != 1)
			{
				if (encode)
				{
					if (!stereo || (ctx.theta_round == 0))
					{
						itheta = (itheta * qn + 8192) >> 14;

						if (!stereo && ctx.avoid_split_noise && (itheta > 0) && (itheta < qn))
						{
							// Check if the selected value of theta will cause the bit allocation
							// to inject noise on one side. If so, make sure the energy of that side
							// is zero
							c_int unquantized = (c_int)EntCode.Celt_UDiv((opus_uint32)itheta * 16384, (opus_uint32)qn);
							imid = Bitexact_Cos((opus_int16)unquantized);
							iside = Bitexact_Cos((opus_int16)(16384 - unquantized));
							delta = MathOps.Frac_MUL16((opus_int16)((N - 1) << 7), (opus_int16)Bitexact_Log2Tan(iside, imid));

							if (delta > b)
								itheta = qn;
							else if (delta < -b)
								itheta = 0;
						}
					}
					else
					{
						// Bias quantization towards itheta=0 and itheta=16384
						c_int bias = itheta > 8192 ? 32767 / qn : -32767 / qn;
						c_int down = Arch.IMIN(qn - 1, Arch.IMAX(0, (itheta * qn + bias) >> 14));

						if (ctx.theta_round < 0)
							itheta = down;
						else
							itheta = down + 1;
					}
				}

				// Entropy coding of the angle. We use a uniform pdf for the
				// time split, a step for stereo, and a triangular one for the rest
				if (stereo && (N > 2))
				{
					c_int p0 = 3;
					c_int x = itheta;
					c_int x0 = qn / 2;
					c_int ft = p0 * (x0 + 1) + x0;

					// Use a probability of p0 up to itheta=8192 and then use 1 after
					if (encode)
						EntEnc.Ec_Encode(ec, (c_uint)(x <= x0 ? p0 * x : (x - 1 - x0) + (x0 + 1) * p0), (c_uint)(x <= x0 ? p0 * (x + 1) : (x - x0) + (x0 + 1) * p0), (c_uint)ft);
					else
					{
						c_int fs = (c_int)EntDec.Ec_Decode(ec, (c_uint)ft);

						if (fs < ((x0 + 1) * p0))
							x = fs / p0;
						else
							x = x0 + 1 + (fs - (x0 + 1) * p0);

						EntDec.Ec_Dec_Update(ec, (c_uint)(x <= x0 ? p0 * x : (x - 1 - x0) + (x0 + 1) * p0), (c_uint)(x <= x0 ? p0 * (x + 1) : (x - x0) + (x0 + 1) * p0), (c_uint)ft);

						itheta = x;
					}
				}
				else if ((B0 > 1) || stereo)
				{
					// Uniform pdf
					if (encode)
						EntEnc.Ec_Enc_UInt(ec, (opus_uint32)itheta, (opus_uint32)qn + 1);
					else
						itheta = (c_int)EntDec.Ec_Dec_UInt(ec, (opus_uint32)qn + 1);
				}
				else
				{
					c_int fs = 1;
					c_int ft = ((qn >> 1) + 1) * ((qn >> 1) + 1);

					if (encode)
					{
						fs = itheta <= (qn >> 1) ? itheta + 1 : qn + 1 - itheta;
						c_int fl = itheta <= (qn >> 1) ? itheta * (itheta + 1) >> 1 : ft - ((qn + 1 - itheta) * (qn + 2 - itheta) >> 1);

						EntEnc.Ec_Encode(ec, (c_uint)fl, (c_uint)(fl + fs), (c_uint)ft);
					}
					else
					{
						// Triangular pdf
						c_int fl = 0;
						c_int fm = (c_int)EntDec.Ec_Decode(ec, (c_uint)ft);

						if (fm < ((qn >> 1) * ((qn >> 1) + 1) >> 1))
						{
							itheta = (c_int)((MathOps.Isqrt32(8 * (opus_uint32)fm + 1) - 1) >> 1);
							fs = itheta + 1;
							fl = itheta * (itheta + 1) >> 1;
						}
						else
						{
							itheta = (c_int)((2 * (qn + 1) - MathOps.Isqrt32(8 * (opus_uint32)(ft - fm - 1) + 1)) >> 1);
							fs = qn + 1 - itheta;
							fl = ft - ((qn + 1 - itheta) * (qn + 2 - itheta) >> 1);
						}

						EntDec.Ec_Dec_Update(ec, (c_uint)fl, (c_uint)(fl + fs), (c_uint)ft);
					}
				}

				itheta = (c_int)EntCode.Celt_UDiv((opus_uint32)itheta * 16384, (opus_uint32)qn);

				if (encode && stereo)
				{
					if (itheta == 0)
						Intensity_Stereo(m, X, Y, bandE, i, N);
					else
						Stereo_Split(X, Y, N);
				}
				// NOTE: Renormalising X and Y *may* help fixed-point a bit at very high rate.
				//       Let's do that at higher complexity
			}
			else if (stereo)
			{
				if (encode)
				{
					inv = (itheta > 8192) && !ctx.disable_inv;
					if (inv)
					{
						for (c_int j = 0; j < N; j++)
							Y[j] = -Y[j];
					}

					Intensity_Stereo(m, X, Y, bandE, i, N);
				}

				if ((b > (2 << Constants.BitRes)) && (ctx.remaining_bits > (2 << Constants.BitRes)))
				{
					if (encode)
						EntEnc.Ec_Enc_Bit_Logp(ec, inv, 2);
					else
						inv = EntDec.Ec_Dec_Bit_Logp(ec, 2);
				}
				else
					inv = false;

				// Inv flag overide to avoid problems with downmixing
				if (ctx.disable_inv)
					inv = false;

				itheta = 0;
			}

			c_int qalloc = (c_int)(EntCode.Ec_Tell_Frac(ec) - tell);
			b -= qalloc;

			if (itheta == 0)
			{
				imid = 32767;
				iside = 0;
				fill &= (1 << B) - 1;
				delta = -16384;
			}
			else if (itheta == 16384)
			{
				imid = 0;
				iside = 32767;
				fill &= ((1 << B) - 1) << B;
				delta = 16384;
			}
			else
			{
				imid = Bitexact_Cos((opus_int16)itheta);
				iside = Bitexact_Cos((opus_int16)(16384 - itheta));

				// This is the mid vs side allocation that minimizes squared error
				// in that band
				delta = MathOps.Frac_MUL16((opus_int16)((N - 1) << 7), (opus_int16)Bitexact_Log2Tan(iside, imid));
			}

			sctx = new Split_Ctx();

			sctx.inv = inv;
			sctx.imid = imid;
			sctx.iside = iside;
			sctx.delta = delta;
			sctx.itheta = itheta;
			sctx.qalloc = qalloc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint Quant_Band_N1(Band_Ctx ctx, CPointer<celt_norm> X, CPointer<celt_norm> Y, CPointer<celt_norm> lowband_out)
		{
			CPointer<celt_norm> x = X;
			bool encode = ctx.encode;
			Ec_Ctx ec = ctx.ec;

			c_int stereo = Y.IsNotNull ? 1 : 0;
			c_int c = 0;

			do
			{
				c_int sign = 0;

				if (ctx.remaining_bits >= (1 << Constants.BitRes))
				{
					if (encode)
					{
						sign = x[0] < 0 ? 1 : 0;
						EntEnc.Ec_Enc_Bits(ec, (opus_uint32)sign, 1);
					}
					else
						sign = (c_int)EntDec.Ec_Dec_Bits(ec, 1);

					ctx.remaining_bits -= 1 << Constants.BitRes;
				}

				if (ctx.resynth)
					x[0] = sign != 0 ? -Constants.Norm_Scaling : Constants.Norm_Scaling;

				x = Y;
			}
			while (++c < (1 + stereo));

			if (lowband_out.IsNotNull)
				lowband_out[0] = Arch.SHR16(X[0], 4);

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// This function is responsible for encoding and decoding a mono
		/// partition. It can split the band in two and transmit the energy
		/// difference with the two half-bands. It can be called recursively
		/// so bands can end up being split in 8 parts
		/// </summary>
		/********************************************************************/
		private static c_uint Quant_Partition(Band_Ctx ctx, CPointer<celt_norm> X, c_int N, c_int b, c_int B, CPointer<celt_norm> lowband, c_int LM, opus_val16 gain, c_int fill)
		{
			c_int imid = 0, iside = 0;
			c_int B0 = B;
			opus_val16 mid = 0, side = 0;
			c_uint cm = 0;
			CPointer<celt_norm> Y = null;

			bool encode = ctx.encode;
			CeltMode m = ctx.m;
			c_int i = ctx.i;
			Spread spread = ctx.spread;
			Ec_Ctx ec = ctx.ec;

			// If we need 1.5 more bit than we can produce, split the band in two
			CPointer<byte> cache = m.cache.bits + m.cache.index[(LM + 1) * m.nbEBands + i];

			if ((LM != -1) && (b > (cache[cache[0]] + 12)) && (N > 2))
			{
				CPointer<celt_norm> next_lowband2 = null;

				N >>= 1;
				Y = X + N;
				LM -= 1;

				if (B == 1)
					fill = (fill & 1) | (fill << 1);

				B = (B + 1) >> 1;

				Compute_Theta(ctx, out Split_Ctx sctx, X, Y, N, ref b, B, B0, LM, false, ref fill);

				imid = sctx.imid;
				iside = sctx.iside;
				c_int delta = sctx.delta;
				c_int itheta = sctx.itheta;
				c_int qalloc = sctx.qalloc;

				mid = (1.0f / 32768) * imid;
				side = (1.0f / 32768) * iside;

				// Give more bits to low-energy MDCTs than they would otherwise deserve
				if ((B0 > 1) && ((itheta & 0x3fff) != 0))
				{
					if (itheta > 8192)
					{
						// Rough approximation for pre-echo masking
						delta -= delta >> (4 - LM);
					}
					else
					{
						// Corresponds to a forward-masking slope of 1.5 dB per 10 ms
						delta = Arch.IMIN(0, delta + (N << Constants.BitRes >> (5 - LM)));
					}
				}

				c_int mbits = Arch.IMAX(0, Arch.IMIN(b, (b - delta) / 2));
				c_int sbits = b - mbits;
				ctx.remaining_bits -= qalloc;

				if (lowband.IsNotNull)
					next_lowband2 = lowband + N;	// >32-bit split case

				opus_int32 rebalance = ctx.remaining_bits;

				if (mbits >= sbits)
				{
					cm = Quant_Partition(ctx, X, N, mbits, B, lowband, LM, Arch.MULT16_16_P15(gain, mid), fill);
					rebalance = mbits - (rebalance - ctx.remaining_bits);

					if ((rebalance > (3 << Constants.BitRes)) && (itheta != 0))
						sbits += rebalance - (3 << Constants.BitRes);

					cm |= Quant_Partition(ctx, Y, N, sbits, B, next_lowband2, LM, Arch.MULT16_16_P15(gain, side), fill >> B) << (B0 >> 1);
				}
				else
				{
					cm = Quant_Partition(ctx, Y, N, sbits, B, next_lowband2, LM, Arch.MULT16_16_P15(gain, side), fill >> B) << (B0 >> 1);
					rebalance = sbits - (rebalance - ctx.remaining_bits);

					if ((rebalance > (3 << Constants.BitRes)) && (itheta != 16384))
						mbits += rebalance - (3 << Constants.BitRes);

					cm |= Quant_Partition(ctx, X, N, mbits, B, lowband, LM, Arch.MULT16_16_P15(gain, mid), fill);
				}
			}
			else
			{
				// This is the basic no-split case
				c_int q = Rate.Bits2Pulses(m, i, LM, b);
				c_int curr_bits = Rate.Pulses2Bits(m, i, LM, q);
				ctx.remaining_bits -= curr_bits;

				// Ensures we can never bust the budget
				while ((ctx.remaining_bits < 0) && (q > 0))
				{
					ctx.remaining_bits += curr_bits;
					q--;
					curr_bits = Rate.Pulses2Bits(m, i, LM, q);
					ctx.remaining_bits -= curr_bits;
				}

				if (q != 0)
				{
					c_int K = Rate.Get_Pulses(q);

					// Finally do the actual quantization
					if (encode)
						cm = Vq.Alg_Quant(X, N, K, spread, B, ec, gain, ctx.resynth, ctx.arch);
					else
						cm = Vq.Alg_Unquant(X, N, K, spread, B, ec, gain);
				}
				else
				{
					// If there's no pulse, fill the band anyway
					if (ctx.resynth)
					{
						// B can be as large as 16, so this shift might overflow an int on a
						// 16-bit platform; use a long to get defined behaviour
						c_uint cm_mask = (c_uint)(1UL << B) - 1;
						fill &= (c_int)cm_mask;

						if (fill == 0)
							Memory.Opus_Clear(X, N);
						else
						{
							if (lowband.IsNull)
							{
								// Noise
								for (c_int j = 0; j < N; j++)
								{
									ctx.seed = Celt_Lcg_Rand(ctx.seed);
									X[j] = ((opus_int32)ctx.seed >> 20);
								}

								cm = cm_mask;
							}
							else
							{
								// Folded spectrum
								for (c_int j = 0; j < N; j++)
								{
									ctx.seed = Celt_Lcg_Rand(ctx.seed);

									// About 48 dB below the "normal" folding level
									opus_val16 tmp = Arch.QCONST16(1.0f / 256, 10);
									tmp = ((ctx.seed) & 0x8000) != 0 ? tmp : -tmp;
									X[j] = lowband[j] + tmp;
								}

								cm = (c_uint)fill;
							}

							Vq.Renormalise_Vector(X, N, gain, ctx.arch);
						}
					}
				}
			}

			return cm;
		}



		/********************************************************************/
		/// <summary>
		/// This function is responsible for encoding and decoding a band for
		/// the mono case
		/// </summary>
		/********************************************************************/
		private static c_uint Quant_Band(Band_Ctx ctx, CPointer<celt_norm> X, c_int N, c_int b, c_int B, CPointer<celt_norm> lowband, c_int LM, CPointer<celt_norm> lowband_out, opus_val16 gain, CPointer<celt_norm> lowband_scratch, c_int fill)
		{
			c_int N0 = N;
			c_int N_B = N;
			c_int B0 = B;
			c_int time_divide = 0;
			c_int recombine = 0;
			c_uint cm = 0;

			bool encode = ctx.encode;
			c_int tf_change = ctx.tf_change;

			bool longBlocks = B0 == 1;

			N_B = (c_int)EntCode.Celt_UDiv((c_uint)N_B, (c_uint)B);

			// Special case for one sample
			if (N == 1)
				return Quant_Band_N1(ctx, X, null, lowband_out);

			if (tf_change > 0)
			{
				recombine = tf_change;
				// Band recombining to increase frequency resolution
			}

			if (lowband_scratch.IsNotNull && lowband.IsNotNull && ((recombine != 0) || (((N_B & 1) == 0) && (tf_change < 0)) || (B0 > 1)))
			{
				Memory.Opus_Copy(lowband_scratch, lowband, N);
				lowband = lowband_scratch;
			}

			for (c_int k = 0; k < recombine; k++)
			{
				if (encode)
					Haar1(X, N >> k, 1 << k);

				if (lowband.IsNotNull)
					Haar1(lowband, N >> k, 1 << k);

				fill = bit_interleave_table[fill & 0x0f] | bit_interleave_table[fill >> 4] << 2;
			}

			B >>= recombine;
			N_B <<= recombine;

			// Increasing the time resolution
			while (((N_B & 1) == 0) && (tf_change < 0))
			{
				if (encode)
					Haar1(X, N_B, B);

				if (lowband.IsNotNull)
					Haar1(lowband, N_B, B);

				fill |= fill << B;
				B <<= 1;
				N_B >>= 1;
				time_divide++;
				tf_change++;
			}

			B0 = B;
			c_int N_B0 = N_B;

			// Reorganize the samples in time order instead of frequency order
			if (B0 > 1)
			{
				if (encode)
					Deinterleave_Hadamard(X, N_B >> recombine, B0 << recombine, longBlocks);

				if (lowband.IsNotNull)
					Deinterleave_Hadamard(lowband, N_B >> recombine, B0 << recombine, longBlocks);
			}

			cm = Quant_Partition(ctx, X, N, b, B, lowband, LM, gain, fill);

			// This code is used by the decoder and by the resynthesis-enabled encoder
			if (ctx.resynth)
			{
				// Undo the sample reorganization going from time order to frequency order
				if (B0 > 1)
					Interleave_Hadamard(X, N_B >> recombine, B0 << recombine, longBlocks);

				// Undo time-freq changes that we did earlier
				N_B = N_B0;
				B = B0;

				for (c_int k = 0; k < time_divide; k++)
				{
					B >>= 1;
					N_B <<= 1;
					cm |= cm >> B;
					Haar1(X, N_B, B);
				}

				for (c_int k = 0; k < recombine; k++)
				{
					cm = bit_deinterleave_table[cm];
					Haar1(X, N0 >> k, 1 << k);
				}

				B <<= recombine;

				// Scale output for later folding
				if (lowband_out.IsNotNull)
				{
					opus_val16 n = MathOps.Celt_Sqrt(Arch.SHL32(Arch.EXTEND32(N0), 22));

					for (c_int j = 0; j < N0; j++)
						lowband_out[j] = Arch.MULT16_16_Q15(n, X[j]);
				}

				cm &= (c_uint)((1 << B) - 1);
			}

			return cm;
		}



		/********************************************************************/
		/// <summary>
		/// This function is responsible for encoding and decoding a band for
		/// the stereo case
		/// </summary>
		/********************************************************************/
		private static c_uint Quant_Band_Stereo(Band_Ctx ctx, CPointer<celt_norm> X, CPointer<celt_norm> Y, c_int N, c_int b, c_int B, CPointer<celt_norm> lowband, c_int LM, CPointer<celt_norm> lowband_out, CPointer<celt_norm> lowband_scratch, c_int fill)
		{
			c_uint cm = 0;
			c_int mbits, sbits;

			bool encode = ctx.encode;
			Ec_Ctx ec = ctx.ec;

			// Special case for one sample
			if (N == 1)
				return Quant_Band_N1(ctx, X, Y, lowband_out);

			c_int orig_fill = fill;

			Compute_Theta(ctx, out Split_Ctx sctx, X, Y, N, ref b, B, B, LM, true, ref fill);

			bool inv = sctx.inv;
			c_int imid = sctx.imid;
			c_int iside = sctx.iside;
			c_int delta = sctx.delta;
			c_int itheta = sctx.itheta;
			c_int qalloc = sctx.qalloc;

			opus_val16 mid = (1.0f / 32768) * imid;
			opus_val16 side = (1.0f / 32768) * iside;

			// This is a special case for n=2 that only works for stereo and takes
			// advantage of the fact that mid and side are orthogonal to encode
			// the side with just one bit
			if (N == 2)
			{
				c_int sign = 0;
				mbits = b;
				sbits = 0;

				// Only need one bit for the side
				if ((itheta != 0) && (itheta != 16384))
					sbits = 1 << Constants.BitRes;

				mbits -= sbits;
				bool c = itheta > 8192;
				ctx.remaining_bits -= qalloc + sbits;

				CPointer<celt_norm> x2 = c ? Y : X;
				CPointer<celt_norm> y2 = c ? X : Y;

				if (sbits != 0)
				{
					if (encode)
					{
						// Here we only need to encode a sign for the side
						sign = x2[0] * y2[1] - x2[1] * y2[0] < 0 ? 1 : 0;
						EntEnc.Ec_Enc_Bits(ec, (opus_uint32)sign, 1);
					}
					else
						sign = (c_int)EntDec.Ec_Dec_Bits(ec, 1);
				}

				sign = 1 - 2 * sign;

				// We use orig_fill here because we want to fold the side, but if
				// itheta==16384, we'll have cleared the low bits of fill
				cm = Quant_Band(ctx, x2, N, mbits, B, lowband, LM, lowband_out, Constants.Q15One, lowband_scratch, orig_fill);

				// We don't split N=2 bands, so cm is either 1 or 0 (for a fold-collapse),
				// and there's no need to worry about mixing with the other channel
				y2[0] = -sign * x2[1];
				y2[1] = sign * x2[0];

				if (ctx.resynth)
				{
					X[0] = Arch.MULT16_16_Q15(mid, X[0]);
					X[1] = Arch.MULT16_16_Q15(mid, X[1]);
					Y[0] = Arch.MULT16_16_Q15(side, Y[0]);
					Y[1] = Arch.MULT16_16_Q15(side, Y[1]);

					celt_norm tmp = X[0];
					X[0] = Arch.SUB16(tmp, Y[0]);
					Y[0] = Arch.ADD16(tmp, Y[0]);

					tmp = X[1];
					X[1] = Arch.SUB16(tmp, Y[1]);
					Y[1] = Arch.ADD16(tmp, Y[1]);
				}
			}
			else
			{
				// "Normal" split code
				mbits = Arch.IMAX(0, Arch.IMIN(b, (b - delta) / 2));
				sbits = b - mbits;
				ctx.remaining_bits -= qalloc;

				opus_int32 rebalance = ctx.remaining_bits;

				if (mbits >= sbits)
				{
					// In stereo mode, we do not apply a scaling to the mid because we need the normalized
					// mid for folding later
					cm = Quant_Band(ctx, X, N, mbits, B, lowband, LM, lowband_out, Constants.Q15One, lowband_scratch, fill);
					rebalance = mbits - (rebalance - ctx.remaining_bits);

					if ((rebalance > (3 << Constants.BitRes)) && (itheta != 0))
						sbits += rebalance - (3 << Constants.BitRes);

					// For a stereo split, the high bits of fill are always zero, so no
					// folding will be done to the side
					cm |= Quant_Band(ctx, Y, N, sbits, B, null, LM, null, side, null, fill >> B);
				}
				else
				{
					// For a stereo split, the high bits of fill are always zero, so no
					// folding will be done to the side
					cm = Quant_Band(ctx, Y, N, sbits, B, null, LM, null, side, null, fill >> B);
					rebalance = sbits - (rebalance - ctx.remaining_bits);

					if ((rebalance > (3 << Constants.BitRes)) && (itheta != 16384))
						mbits += rebalance - (3 << Constants.BitRes);

					// In stereo mode, we do not apply a scaling to the mid because we need the normalized
					// mid for folding later
					cm |= Quant_Band(ctx, X, N, mbits, B, lowband, LM, lowband_out, Constants.Q15One, lowband_scratch, fill);
				}
			}

			// This code is used by the decoder and by the resynthesis-enabled encoder
			if (ctx.resynth)
			{
				if (N != 2)
					Stereo_Merge(X, Y, mid, N, ctx.arch);

				if (inv)
				{
					for (c_int j = 0; j < N; j++)
						Y[j] = -Y[j];
				}
			}

			return cm;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Special_Hybrid_Folding(CeltMode m, CPointer<celt_norm> norm, CPointer<celt_norm> norm2, c_int start, c_int M, bool dual_stereo)
		{
			CPointer<opus_int16> eBands = m.eBands;
			c_int n1 = M * (eBands[start + 1] - eBands[start]);
			c_int n2 = M * (eBands[start + 2] - eBands[start + 1]);

			// Duplicate enough of the first band folding data to be able to fold the second band.
			// Copies no data for CELT-only mode
			Memory.Opus_Copy(norm + n1, norm + 2 * n1 - n2, n2 - n1);

			if (dual_stereo)
				Memory.Opus_Copy(norm2 + n1, norm2 + 2 * n1 - n2, n2 - n1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Quant_All_Bands(bool encode, CeltMode m, c_int start, c_int end, CPointer<celt_norm> X_, CPointer<celt_norm> Y_, CPointer<byte> collapse_masks,
											CPointer<celt_ener> bandE, CPointer<c_int> pulses, c_int shortBlocks, Spread spread, bool dual_stereo, c_int intensity,
											CPointer<c_int> tf_res, opus_int32 total_bits, opus_int32 balance, ref Ec_Ctx ec, c_int LM, c_int codedBands,
											ref opus_uint32 seed, c_int complexity, c_int arch, bool disable_inv)
		{
			CPointer<opus_int16> eBands = m.eBands;
			bool update_lowband = true;
			c_int C = Y_.IsNotNull ? 2 : 1;
			bool theta_rdo = encode && Y_.IsNotNull && !dual_stereo && (complexity >= 8);
			bool resynth = !encode || theta_rdo;
			Band_Ctx ctx = new Band_Ctx();

			c_int M = 1 << LM;
			c_int B = shortBlocks != 0 ? M : 1;
			c_int norm_offset = M * eBands[start];

			// No need to allocate norm for the last band because we don't need an
			// output in that band
			celt_norm[] _norm = new celt_norm[C * (M * eBands[m.nbEBands - 1] - norm_offset)];
			CPointer<celt_norm> norm = _norm;
			CPointer<celt_norm> norm2 = norm + M * eBands[m.nbEBands - 1] - norm_offset;

			// For decoding, we can use the last band as scratch space because we don't need that
			// scratch space for the last band and we don't care about the data there until we're
			// decoding the last band
			c_int resynth_alloc;

			if (encode && resynth)
				resynth_alloc = M * (eBands[m.nbEBands] - eBands[m.nbEBands - 1]);
			else
				resynth_alloc = Constants.Alloc_None;

			celt_norm[] _lowband_scratch = new celt_norm[resynth_alloc];
			CPointer<celt_norm> lowband_scratch;

			if (encode && resynth)
				lowband_scratch = _lowband_scratch;
			else
				lowband_scratch = X_ + M * eBands[m.effEBands - 1];

			celt_norm[] X_save = new celt_norm[resynth_alloc];
			celt_norm[] Y_save = new celt_norm[resynth_alloc];
			celt_norm[] X_save2 = new celt_norm[resynth_alloc];
			celt_norm[] Y_save2 = new celt_norm[resynth_alloc];
			celt_norm[] norm_save2 = new celt_norm[resynth_alloc];

			c_int lowband_offset = 0;

			ctx.bandE = bandE;
			ctx.ec = ec;
			ctx.encode = encode;
			ctx.intensity = intensity;
			ctx.m = m;
			ctx.seed = seed;
			ctx.spread = spread;
			ctx.arch = arch;
			ctx.disable_inv = disable_inv;
			ctx.resynth = resynth;
			ctx.theta_round = 0;

			// Avoid injecting noise in the first band on transients
			ctx.avoid_split_noise = B > 1;

			for (c_int i = start; i < end; i++)
			{
				c_int effective_lowband = -1;
				c_int tf_change = 0;

				ctx.i = i;
				bool last = i == (end - 1);

				CPointer<celt_norm> X = X_ + M * eBands[i];
				CPointer<celt_norm> Y;

				if (Y_.IsNotNull)
					Y = Y_ + M * eBands[i];
				else
					Y = null;

				c_int N = M * eBands[i + 1] - M * eBands[i];
				opus_int32 tell = (opus_int32)EntCode.Ec_Tell_Frac(ec);

				// Compute how many bits we want to allocate to this band
				if (i != start)
					balance -= tell;

				opus_int32 remaining_bits = total_bits - tell - 1;
				ctx.remaining_bits = remaining_bits;

				c_int b;

				if (i <= (codedBands - 1))
				{
					opus_int32 curr_balance = EntCode.Celt_SUDiv(balance, Arch.IMIN(3, codedBands - i));
					b = Arch.IMAX(0, Arch.IMIN(16383, Arch.IMIN(remaining_bits + 1, pulses[i] + curr_balance)));
				}
				else
					b = 0;

				if (resynth && (((M * eBands[i] - N) >= (M * eBands[start])) || (i == (start + 1))) && (update_lowband || (lowband_offset == 0)))
					lowband_offset = i;

				if (i == (start + 1))
					Special_Hybrid_Folding(m, norm, norm2, start, M, dual_stereo);

				tf_change = tf_res[i];
				ctx.tf_change = tf_change;

				if (i >= m.effEBands)
				{
					X = norm;

					if (Y_.IsNotNull)
						Y = norm;

					lowband_scratch.SetToNull();
				}

				if (last && !theta_rdo)
					lowband_scratch.SetToNull();

				// Get a conservative estimate of the collapse_mask's for the bands we're
				// going to be folding from
				c_uint x_cm;
				c_uint y_cm;

				if ((lowband_offset != 0) && ((spread != Spread.Aggressive) || (B > 1) || (tf_change < 0)))
				{
					// This ensures we never repeat spectral content within one band
					effective_lowband = Arch.IMAX(0, M * eBands[lowband_offset] - norm_offset - N);
					c_int fold_start = lowband_offset;

					while ((M * eBands[--fold_start]) > (effective_lowband + norm_offset))
					{
					}

					c_int fold_end = lowband_offset - 1;

					while ((++fold_end < i) && ((M * eBands[fold_end]) < (effective_lowband + norm_offset + N)))
					{
					}

					x_cm = y_cm = 0;
					c_int fold_i = fold_start;

					do
					{
						x_cm |= collapse_masks[fold_i * C + 0];
						y_cm |= collapse_masks[fold_i * C + C - 1];
					}
					while (++fold_i < fold_end);
				}
				else
				{
					// Otherwise, we'll be using the LCG to fold, so all blocks will (almost
					// always) be non-zero
					x_cm = y_cm = (c_uint)((1 << B) - 1);
				}

				if (dual_stereo && (i == intensity))
				{
					// Switch off dual stereo to do intensity
					dual_stereo = false;

					if (resynth)
					{
						for (c_int j = 0; j < (M * eBands[i] - norm_offset); j++)
							norm[j] = Arch.HALF32(norm[j] + norm2[j]);
					}
				}

				if (dual_stereo)
				{
					x_cm = Quant_Band(ctx, X, N, b / 2, B,
									effective_lowband != -1 ? norm + effective_lowband : null, LM,
									last ? null : norm + M * eBands[i] - norm_offset, Constants.Q15One, lowband_scratch, (c_int)x_cm);
					y_cm = Quant_Band(ctx, Y, N, b / 2, B,
									effective_lowband != -1 ? norm2 + effective_lowband : null, LM,
									last ? null : norm2 + M * eBands[i] - norm_offset, Constants.Q15One, lowband_scratch, (c_int)y_cm);
				}
				else
				{
					if (Y.IsNotNull)
					{
						if (theta_rdo && (i < intensity))
						{
							byte[] bytes_save = new byte[1275];
							opus_val16[] w = new opus_val16[2];

							Compute_Channel_Weights(bandE[i], bandE[i + m.nbEBands], w);

							// Make a copy
							c_uint cm = x_cm | y_cm;
							Ec_Ctx ec_save = ec.MakeDeepClone();
							Band_Ctx ctx_save = ctx.MakeDeepClone();

							Memory.Opus_Copy(X_save, X, N);
							Memory.Opus_Copy(Y_save, Y, N);

							// Encode and round down
							ctx.theta_round = -1;

							x_cm = Quant_Band_Stereo(ctx, X, Y, N, b, B,
											effective_lowband != -1 ? norm + effective_lowband : null, LM,
											last ? null : norm + M * eBands[i] - norm_offset, lowband_scratch, (c_int)cm);

							opus_val32 dist0 = Arch.MULT16_32_Q15(w[0], Pitch.Celt_Inner_Prod(X_save, X, N, arch)) + Arch.MULT16_32_Q15(w[1], Pitch.Celt_Inner_Prod(Y_save, Y, N, arch));

							// Save first result
							c_uint cm2 = x_cm;
							Ec_Ctx ec_save2 = ec.MakeDeepClone();
							Band_Ctx ctx_save2 = ctx.MakeDeepClone();

							Memory.Opus_Copy(X_save2, X, N);
							Memory.Opus_Copy(Y_save2, Y, N);

							if (!last)
								Memory.Opus_Copy(norm_save2, norm + M * eBands[i] - norm_offset, N);

							c_int nstart_bytes = (c_int)ec_save.offs;
							c_int nend_bytes = (c_int)ec_save.storage;
							CPointer<byte> bytes_buf = ec_save.buf + nstart_bytes;
							c_int save_bytes = nend_bytes - nstart_bytes;
							Memory.Opus_Copy(bytes_save, bytes_buf, save_bytes);

							// Restore
							ec = ec_save.MakeDeepClone();
							ctx = ctx_save.MakeDeepClone();

							Memory.Opus_Copy(X, X_save, N);
							Memory.Opus_Copy(Y, Y_save, N);

							if (i == (start + 1))
								Special_Hybrid_Folding(m, norm, norm2, start, M, dual_stereo);

							// Encode and round up
							ctx.theta_round = 1;

							x_cm = Quant_Band_Stereo(ctx, X, Y, N, b, B,
											effective_lowband != -1 ? norm + effective_lowband : null, LM,
											last ? null : norm + M * eBands[i] - norm_offset, lowband_scratch, (c_int)cm);

							opus_val32 dist1 = Arch.MULT16_32_Q15(w[0], Pitch.Celt_Inner_Prod(X_save, X, N, arch)) + Arch.MULT16_32_Q15(w[1], Pitch.Celt_Inner_Prod(Y_save, Y, N, arch));

							if (dist0 >= dist1)
							{
								x_cm = cm2;
								ec = ec_save2.MakeDeepClone();
								ctx = ctx_save2.MakeDeepClone();

								Memory.Opus_Copy(X, X_save2, N);
								Memory.Opus_Copy(Y, Y_save2, N);

								if (!last)
									Memory.Opus_Copy(norm + M * eBands[i] - norm_offset, norm_save2, N);

								Memory.Opus_Copy(bytes_buf, bytes_save, save_bytes);
							}
						}
						else
						{
							ctx.theta_round = 0;

							x_cm = Quant_Band_Stereo(ctx, X, Y, N, b, B,
											effective_lowband != -1 ? norm + effective_lowband : null, LM,
											last ? null : norm + M * eBands[i] - norm_offset, lowband_scratch, (c_int)(x_cm | y_cm));
						}
					}
					else
					{
						x_cm = Quant_Band(ctx, X, N, b, B,
										effective_lowband != -1 ? norm + effective_lowband : null, LM,
										last ? null : norm + M * eBands[i] - norm_offset, Constants.Q15One, lowband_scratch, (c_int)(x_cm | y_cm));
					}

					y_cm = x_cm;
				}

				collapse_masks[i * C + 0] = (byte)x_cm;
				collapse_masks[i * C + C - 1] = (byte)y_cm;
				balance += pulses[i] + tell;

				// Update the folding position only as long as we have 1 bit/sample depth
				update_lowband = b > (N << Constants.BitRes);

				// We only need to avoid noise on a split for the first band. After that, we
				// have folding
				ctx.avoid_split_noise = false;
			}

			seed = ctx.seed;
		}
	}
}

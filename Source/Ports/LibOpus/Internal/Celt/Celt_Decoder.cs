/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Celt_Decoder
	{
		// The maximum pitch lag to allow in the pitch-based PLC. It's possible to save
		// CPU time in the PLC pitch search by making this smaller than MAX_PERIOD. The
		// current value corresponds to a pitch of 66.67 Hz
		private const int Plc_Pitch_Lag_Max = 720;

		// The minimum pitch lag to allow in the pitch-based PLC. This corresponds to a
		// pitch of 480 Hz
		private const int Plc_Pitch_Lag_Min = 100;

		public const int Decode_Buffer_Size = 2048;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Celt_Decoder_Init(CeltDecoder st, opus_int32 sampling_rate, c_int channels)
		{
			OpusError ret = Opus_Custom_Decoder_Init(st, Modes.Opus_Custom_Mode_Create(48000, 960, out _), channels);
			if (ret != OpusError.Ok)
				return ret;

			st.downsample = Celt.Resampling_Factor(sampling_rate);
			if (st.downsample == 0)
				return OpusError.Bad_Arg;

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Custom_Decoder_Init(CeltDecoder st, CeltMode mode, c_int channels)
		{
			if ((channels < 0) || (channels > 2))
				return OpusError.Bad_Arg;

			if (st == null)
				return OpusError.Alloc_Fail;

			st.Clear();

			st.mode = mode;
			st.overlap = mode.overlap;
			st.stream_channels = st.channels = channels;

			st.downsample = 1;
			st.start = 0;
			st.end = st.mode.effEBands;
			st.signalling = 1;
			st.disable_inv = channels == 1;
			st.arch = Cpu_Support.Opus_Select_Arch();

			Opus_Custom_Decoder_Ctl_Set(st, OpusControlSetRequest.Opus_Reset_State);

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Special case for stereo with no downsampling and no accumulation.
		/// This is quite common and we can make it faster by processing both
		/// channels in the same loop, reducing overhead due to the
		/// dependency loop in the IIR filter
		/// </summary>
		/********************************************************************/
		private static void Deemphasis_Stereo_Simple(Pointer<Pointer<celt_sig>> _in, Pointer<opus_val16> pcm, c_int N, opus_val16 coef0, Pointer<celt_sig> mem)
		{
			Pointer<celt_sig> x0 = _in[0];
			Pointer<celt_sig> x1 = _in[1];

			celt_sig m0 = mem[0];
			celt_sig m1 = mem[1];

			for (c_int j = 0; j < N; j++)
			{
				// Add VERY_SMALL to x[] first to reduce dependency chain
				celt_sig tmp0 = x0[j] + Constants.Very_Small + m0;
				celt_sig tmp1 = x1[j] + Constants.Very_Small + m1;

				m0 = Arch.MULT16_32_Q15(coef0, tmp0);
				m1 = Arch.MULT16_32_Q15(coef0, tmp1);

				pcm[2 * j] = Arch.SCALEOUT(Arch.SIG2WORD16(tmp0));
				pcm[2 * j + 1] = Arch.SCALEOUT(Arch.SIG2WORD16(tmp1));
			}

			mem[0] = m0;
			mem[1] = m1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Deemphasis(Pointer<Pointer<celt_sig>> _in, Pointer<opus_val16> pcm, c_int N, c_int C, c_int downsample, Pointer<opus_val16> coef, Pointer<celt_sig> mem, bool accum)
		{
			bool apply_downsampling = false;

			// Short version for common case
			if ((downsample == 1) && (C == 2) && !accum)
			{
				Deemphasis_Stereo_Simple(_in, pcm, N, coef[0], mem);
				return;
			}

			celt_sig[] scratch = new celt_sig[N];
			opus_val16 coef0 = coef[0];
			c_int Nd = N / downsample;
			c_int c = 0;

			do
			{
				celt_sig m = mem[c];
				Pointer<celt_sig> x = _in[c];
				Pointer<opus_val16> y = pcm + c;

				if (downsample > 1)
				{
					// Shortcut for the standard (non-custom modes) case
					for (c_int j = 0; j < N; j++)
					{
						celt_sig tmp = x[j] + Constants.Very_Small + m;
						m = Arch.MULT16_32_Q15(coef0, tmp);
						scratch[j] = tmp;
					}

					apply_downsampling = true;
				}
				else
				{
					// Shortcut for the standard (non-custom modes) case
					for (c_int j = 0; j < N; j++)
					{
						celt_sig tmp = x[j] + Constants.Very_Small + m;
						m = Arch.MULT16_32_Q15(coef0, tmp);
						y[j * C] = Arch.SCALEOUT(Arch.SIG2WORD16(tmp));
					}
				}

				mem[c] = m;

				if (apply_downsampling)
				{
					// Perform down-sampling
					for (c_int j = 0; j < Nd; j++)
						y[j * C] = Arch.SCALEOUT(Arch.SIG2WORD16(scratch[j * downsample]));
				}
			}
			while (++c < C);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Celt_Synthesis(CeltMode mode, Pointer<celt_norm> X, Pointer<Pointer<celt_sig>> out_syn, Pointer<opus_val16> oldBandE, c_int start, c_int effEnd, c_int C, c_int CC, bool isTransient, c_int LM, c_int downsample, bool silence, c_int arch)
		{
			c_int overlap = mode.overlap;
			c_int nbEBands = mode.nbEBands;
			c_int N = mode.shortMdctSize << LM;
			Pointer<celt_sig> freq = new Pointer<celt_sig>(N);	// < Interleaved signal MDCTs
			c_int M = 1 << LM;

			c_int c;
			c_int B;
			c_int NB;
			c_int shift;

			if (isTransient)
			{
				B = M;
				NB = mode.shortMdctSize;
				shift = mode.maxLM;
			}
			else
			{
				B = 1;
				NB = mode.shortMdctSize << LM;
				shift = mode.maxLM - LM;
			}

			if ((CC == 2) && (C == 1))
			{
				// Copying a mono streams to two channels
				Bands.Denormalise_Bands(mode, X, freq, oldBandE, start, effEnd, M, downsample, silence);

				// Store a temporary copy in the output buffer because the IMDCT destroys its input
				Pointer<celt_sig> freq2 = out_syn[1] + overlap / 2;

				Memory.Opus_Copy(freq2, freq, N);

				for (c_int b = 0; b < B; b++)
					Mdct.Clt_Mdct_Backward(mode.mdct, freq2 + b, out_syn[0] + NB * b, mode.window, overlap, shift, B, arch);

				for (c_int b = 0; b < B; b++)
					Mdct.Clt_Mdct_Backward(mode.mdct, freq + b, out_syn[1] + NB * b, mode.window, overlap, shift, B, arch);
			}
			else if ((CC == 1) && (C == 2))
			{
				// Downmixing a stereo stream to mono
				Pointer<celt_sig> freq2 = out_syn[0] + overlap / 2;

				Bands.Denormalise_Bands(mode, X, freq, oldBandE, start, effEnd, M, downsample, silence);

				// Use the output buffer as temp array before downmixing
				Bands.Denormalise_Bands(mode, X + N, freq2, oldBandE + nbEBands, start, effEnd, M, downsample, silence);

				for (c_int i = 0; i < N; i++)
					freq[i] = Arch.ADD32(Arch.HALF32(freq[i]), Arch.HALF32(freq2[i]));

				for (c_int b = 0; b < B; b++)
					Mdct.Clt_Mdct_Backward(mode.mdct, freq + b, out_syn[0] + NB * b, mode.window, overlap, shift, B, arch);
			}
			else
			{
				// Normal case (mono or stereo)
				c = 0;

				do
				{
					Bands.Denormalise_Bands(mode, X + c * N, freq, oldBandE + c * nbEBands, start, effEnd, M, downsample, silence);

					for (c_int b = 0; b < B; b++)
						Mdct.Clt_Mdct_Backward(mode.mdct, freq + b, out_syn[c] + NB * b, mode.window, overlap, shift, B, arch);
				}
				while (++c < CC);
			}

			// Saturate IMDCT output so that we can't overflow in the pitch postfilter
			// or in the
			c = 0;

			do
			{
				for (c_int i = 0; i < N; i++)
					out_syn[c][i] = Arch.SATURATE(out_syn[c][i], Constants.Sig_Sat);
			}
			while (++c < CC);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Tf_Decode(c_int start, c_int end, bool isTransient, Pointer<c_int> tf_res, c_int LM, Ec_Dec dec)
		{
			opus_uint32 budget = dec.storage * 8;
			opus_uint32 tell = (opus_uint32)EntCode.Ec_Tell(dec);
			c_int logp = isTransient ? 2 : 4;
			c_int tf_select_rsv = (LM > 0) && ((tell + logp + 1) <= budget) ? 1 : 0;
			budget = (opus_uint32)(budget - tf_select_rsv);
			c_int tf_changed = 0, curr = 0;

			for (c_int i = start; i < end; i++)
			{
				if ((tell + logp) <= budget)
				{
					curr ^= EntDec.Ec_Dec_Bit_Logp(dec, (c_uint)logp) ? 1 : 0;
					tell = (opus_uint32)EntCode.Ec_Tell(dec);
					tf_changed |= curr;
				}

				tf_res[i] = curr;
				logp = isTransient ? 4 : 5;
			}

			c_int tf_select = 0;

			if ((tf_select_rsv != 0) && (Tables.Tf_Select_Table[LM][4 * (isTransient ? 1 : 0) + 0 + tf_changed] != Tables.Tf_Select_Table[LM][4 * (isTransient ? 1 : 0) + 2 + tf_changed]))
				tf_select = EntDec.Ec_Dec_Bit_Logp(dec, 1) ? 1 : 0;

			for (c_int i = start; i < end; i++)
				tf_res[i] = Tables.Tf_Select_Table[LM][4 * (isTransient ? 1 : 0) + 2 * tf_select + tf_res[i]];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Celt_Plc_Pitch_Search(Pointer<Pointer<celt_sig>> decode_mem, c_int C, c_int arch)
		{
			Pointer<opus_val16> lp_pitch_buf = new Pointer<opus_val16>(Decode_Buffer_Size >> 1);

			Pitch.Pitch_Downsample(decode_mem, lp_pitch_buf, Decode_Buffer_Size, C, arch);
			Pitch.Pitch_Search(lp_pitch_buf + (Plc_Pitch_Lag_Max >> 1), lp_pitch_buf, Decode_Buffer_Size - Plc_Pitch_Lag_Max, Plc_Pitch_Lag_Max - Plc_Pitch_Lag_Min, out c_int pitch_index, arch);
			pitch_index = Plc_Pitch_Lag_Max - pitch_index;

			return pitch_index;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Prefilter_And_Fold(CeltDecoder st, c_int N)
		{
			Pointer<Pointer<celt_sig>> decode_mem = new Pointer<Pointer<celt_sig>>(2);
			OpusCustomMode mode = st.mode;
			c_int overlap = st.overlap;
			c_int CC = st.channels;
			opus_val32[] etmp = new opus_val32[overlap];

			c_int c = 0;

			do
			{
				decode_mem[c] = st.decode_mem + c * (Decode_Buffer_Size + overlap);
			}
			while (++c < CC);

			c = 0;

			do
			{
				// Apply the pre-filter to the MDCT overlap for the next frame because
				// the post-filter will be re-applied in the decoder after the MDCT
				// overlap
				Celt.Comb_Filter(etmp, decode_mem[c] + Decode_Buffer_Size - N, st.postfilter_period_old, st.postfilter_period, overlap, -st.postfilter_gain_old, -st.postfilter_gain, st.postfilter_tapset_old, st.postfilter_tapset, null, 0, st.arch);

				// Simulate TDAC on the concealed audio so that it blends with the
				// MDCT of the next frame
				for (c_int i = 0; i < (overlap / 2); i++)
					decode_mem[c][Decode_Buffer_Size - N + i] = Arch.MULT16_32_Q15(mode.window[i], etmp[overlap - 1 - i]) + Arch.MULT16_32_Q15(mode.window[overlap - i - 1], etmp[i]);
			}
			while (++c < CC);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Celt_Decode_Lost(CeltDecoder st, c_int N, c_int LM)
		{
			c_int C = st.channels;
			Pointer<Pointer<celt_sig>> decode_mem = new Pointer<Pointer<celt_sig>>(2);
			Pointer<Pointer<celt_sig>> out_syn = new Pointer<Pointer<celt_sig>>(2);

			OpusCustomMode mode = st.mode;
			c_int nbEBands = mode.nbEBands;
			c_int overlap = mode.overlap;
			Pointer<opus_int16> eBands = mode.eBands;

			c_int c = 0;

			do
			{
				decode_mem[c] = st.decode_mem + c * (Decode_Buffer_Size + overlap);
				out_syn[c] = st.decode_mem + Decode_Buffer_Size - N;
			}
			while (++c < C);

			Pointer<opus_val16> lpc = st.lpc;
			Pointer<opus_val16> oldBandE = st.oldEBands;
			Pointer<opus_val16> oldLogE = st.oldLogE;
			Pointer<opus_val16> oldLogE2 = st.oldLogE2;
			Pointer<opus_val16> backgroundLogE = st.backgroundLogE;

			c_int loss_duration = st.loss_duration;
			c_int start = st.start;
			bool noise_based = (loss_duration >= 40) || (start != 0) || st.skip_plc;

			if (noise_based)
			{
				// Noise-based PLC/CNG
				c_int end = st.end;
				c_int effEnd = Arch.IMAX(start, Arch.IMIN(end, mode.effEBands));

				Pointer<celt_norm> X = new Pointer<celt_norm>(C * N);	// < Interleaved normalised MDCTs
				c = 0;

				do
				{
					Memory.Opus_Move(decode_mem[c], decode_mem[c] + N, Decode_Buffer_Size - N + overlap);
				}
				while (++c < C);

				if (st.prefilter_and_fold)
					Prefilter_And_Fold(st, N);

				// Energy decay
				opus_val16 decay = loss_duration == 0 ? Arch.QCONST16(1.5f, Constants.Db_Shift) : Arch.QCONST16(0.5f, Constants.Db_Shift);
				c = 0;

				do
				{
					for (c_int i = start; i < end; i++)
						oldBandE[c * nbEBands + i] = Arch.MAX16(backgroundLogE[c * nbEBands + i], oldBandE[c * nbEBands + i] - decay);
				}
				while (++c < C);

				opus_uint32 seed = st.rng;

				for (c = 0; c < C; c++)
				{
					for (c_int i = start; i < effEnd; i++)
					{
						c_int boffs = N * c + (eBands[i] << LM);
						c_int blen = (eBands[i + 1] - eBands[i]) << LM;

						for (c_int j = 0; j < blen; j++)
						{
							seed = Bands.Celt_Lcg_Rand(seed);
							X[boffs + j] = ((opus_int32)seed >> 20);
						}

						Vq.Renormalise_Vector(X + boffs, blen, Constants.Q15One, st.arch);
					}
				}

				st.rng = seed;

				Celt_Synthesis(mode, X, out_syn, oldBandE, start, effEnd, C, C, false, LM, st.downsample, false, st.arch);
				st.prefilter_and_fold = false;

				// Skip regular PLC until we get two consecutive packets
				st.skip_plc = true;
			}
			else
			{
				// Pitch-based PLC
				opus_val16 fade = Constants.Q15One;
				c_int pitch_index;

				if (loss_duration == 0)
					st.last_pitch_index = pitch_index = Celt_Plc_Pitch_Search(decode_mem, C, st.arch);
				else
				{
					pitch_index = st.last_pitch_index;
					fade = Arch.QCONST16(0.8f, 15);
				}

				// We want the excitation for 2 pitch periods in order to look for a
				// decaying signal, but we can't get more than MAX_PERIOD
				c_int exc_length = Arch.IMIN(2 * pitch_index, Constants.Max_Period);

				opus_val16[] _exc = new opus_val16[Constants.Max_Period + Constants.Celt_Lpc_Order];
				opus_val16[] fir_tmp = new opus_val16[exc_length];
				Pointer<opus_val16> exc = new Pointer<opus_val16>(_exc, Constants.Celt_Lpc_Order);
				Pointer<opus_val16> window = mode.window;

				c = 0;

				do
				{
					opus_val32 S1 = 0;

					Pointer<celt_sig> buf = decode_mem[c];

					for (c_int i = 0; i < (Constants.Max_Period + Constants.Celt_Lpc_Order); i++)
						exc[i - Constants.Celt_Lpc_Order] = Arch.SROUND16(buf[Decode_Buffer_Size - Constants.Max_Period - Constants.Celt_Lpc_Order + i], Constants.Sig_Shift);

					if (loss_duration == 0)
					{
						opus_val32[] ac = new opus_val32[Constants.Celt_Lpc_Order + 1];

						// Compute LPC coefficients for the last MAX_PERIOD samples before
						// the first losds so we can work in the excitation-filter domain
						Celt_Lpc._Celt_Autocorr(exc, ac, window, overlap, Constants.Celt_Lpc_Order, Constants.Max_Period, st.arch);

						// Add a noise floor of -40 dB
						ac[0] *= 1.0001f;

						// Use lag windowing to stabilize the Levinson-Durbin recursion
						for (c_int i = 1; i <= Constants.Celt_Lpc_Order; i++)
							ac[i] -= ac[i] * (0.008f * 0.008f) * i * i;

						Celt_Lpc._Celt_Lpc(lpc + c * Constants.Celt_Lpc_Order, ac, Constants.Celt_Lpc_Order);
					}

					// Initialize the LPC history with the samples just before the start
					// of the region for which we're computing the excitation
					{
						// Compute the excitation for exc_length samples before the loss. We need the copy
						// because celt_fir() cannot filter in-place
						Celt_Lpc.Celt_Fir(exc + Constants.Max_Period - exc_length, lpc + c * Constants.Celt_Lpc_Order, fir_tmp, exc_length, Constants.Celt_Lpc_Order, st.arch);
						Memory.Opus_Copy(exc + Constants.Max_Period - exc_length, fir_tmp, exc_length);
					}

					// Check if the waveform is decaying, and if so how fast.
					// We do this to avoid adding energy when concealing in a segment
					// with decaying energy
					opus_val16 decay;
					{
						opus_val32 E1 = 1, E2 = 1;
						c_int decay_length = exc_length >> 1;

						for (c_int i = 0; i < decay_length; i++)
						{
							opus_val16 e = exc[Constants.Max_Period - decay_length + i];
							E1 += Arch.SHR32(Arch.MULT16_16(e, e), 0/*shift*/);
							e = exc[Constants.Max_Period - 2 * decay_length + i];
							E2 += Arch.SHR32(Arch.MULT16_16(e, e), 0/*shift*/);
						}

						E1 = Arch.MIN32(E1, E2);
						decay = MathOps.Celt_Sqrt(MathOps.Frac_Div32(Arch.SHR32(E1, 1), E2));
					}

					// Move the decoder memory one frame to the left to give us room to
					// add the data for the new frame. We ignore the overlap that extends
					// past the end of the buffer, because we aren't going to use it
					Memory.Opus_Move(buf, buf + N, Decode_Buffer_Size - N);

					// Extrapolate from the end of the excitation with a period of
					// "pitch_index", scaling down each period by an additional factor of
					// "decay"
					c_int extrapolation_offset = Constants.Max_Period - pitch_index;

					// We need to extrapolate enough samples to cover a complete MDCT
					// window (including overlap/2 samples on both sides)
					c_int extrapolation_len = N + overlap;

					// We also apply fading if this is not the first loss
					opus_val16 attenuation = Arch.MULT16_16_Q15(fade, decay);

					for (c_int i = 0, j = 0; i < extrapolation_len; i++, j++)
					{
						if (j >= pitch_index)
						{
							j -= pitch_index;
							attenuation = Arch.MULT16_16_Q15(attenuation, decay);
						}

						buf[Decode_Buffer_Size - N + i] = Arch.SHL32(Arch.EXTEND32(Arch.MULT16_16_Q15(attenuation, exc[extrapolation_offset + j])), Constants.Sig_Shift);

						// Compute the energy of the previously decoded signal whose
						// excitation we're copying
						opus_val16 tmp = Arch.SROUND16(buf[Decode_Buffer_Size - Constants.Max_Period - N + extrapolation_offset + j], Constants.Sig_Shift);
						S1 += Arch.SHR32(Arch.MULT16_16(tmp, tmp), 10);
					}

					{
						opus_val16[] lpc_mem = new opus_val16[Constants.Celt_Lpc_Order];

						// Copy the last decoded samples (prior to the overlap region) to
						// synthesis filter memory so we can have a continuous signal
						for (c_int i = 0; i < Constants.Celt_Lpc_Order; i++)
							lpc_mem[i] = Arch.SROUND16(buf[Decode_Buffer_Size - N - 1 - i], Constants.Sig_Shift);

						// Apply the synthesis filter to convert the excitation back into
						// the signal domain
						Celt_Lpc.Celt_Iir(buf + Decode_Buffer_Size - N, lpc + c * Constants.Celt_Lpc_Order, buf + Decode_Buffer_Size - N, extrapolation_len, Constants.Celt_Lpc_Order, lpc_mem, st.arch);
					}

					// Check if the synthesis energy is higher than expected, which can
					// happen with the signal changes during our window. If so,
					// attenuate
					{
						opus_val32 S2 = 0;

						for (c_int i = 0; i < extrapolation_len; i++)
						{
							opus_val16 tmp = Arch.SROUND16(buf[Decode_Buffer_Size - N + i], Constants.Sig_Shift);
							S2 += Arch.SHR32(Arch.MULT16_16(tmp, tmp), 10);
						}

						// This checks for an "explosion" in the synthesis

						// The float test is written this way to catch NaNs in the output
						// of the IIR filter at the same time
						if (!(S1 > (0.2f * S2)))
						{
							for (c_int i = 0; i < extrapolation_len; i++)
								buf[Decode_Buffer_Size - N + i] = 0;
						}
						else if (S1 < S2)
						{
							opus_val16 ratio = MathOps.Celt_Sqrt(MathOps.Frac_Div32(Arch.SHR32(S1, 1) + 1, S2 + 1));

							for (c_int i = 0; i < overlap; i++)
							{
								opus_val16 tmp_g = Constants.Q15One - Arch.MULT16_16_Q15(window[i], Constants.Q15One - ratio);
								buf[Decode_Buffer_Size - N + i] = Arch.MULT16_32_Q15(tmp_g, buf[Decode_Buffer_Size - N + i]);
							}

							for (c_int i = overlap; i < extrapolation_len; i++)
								buf[Decode_Buffer_Size - N + i] = Arch.MULT16_32_Q15(ratio, buf[Decode_Buffer_Size - N + i]);
						}
					}
				}
				while (++c < C);

				st.prefilter_and_fold = true;
			}

			// Saturate to something large to avoid wrap-around
			st.loss_duration = Arch.IMIN(10000, loss_duration + (1 << LM));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Celt_Decode_With_Ec_Dred(CeltDecoder st, Pointer<byte> data, c_int len, Pointer<opus_val16> pcm, c_int frame_size, Ec_Dec dec, bool accum)
		{
			Pointer<Pointer<celt_sig>> decode_mem = new Pointer<Pointer<celt_sig>>(2);
			Pointer<Pointer<celt_sig>> out_syn = new Pointer<Pointer<celt_sig>>(2);

			c_int CC = st.channels;
			c_int intensity = 0;
			bool dual_stereo = false;
			bool anti_collapse_on = false;
			c_int C = st.stream_channels;

			OpusCustomMode mode = st.mode;
			c_int nbEBands = mode.nbEBands;
			c_int overlap = mode.overlap;
			Pointer<opus_int16> eBands = mode.eBands;
			c_int start = st.start;
			c_int end = st.end;
			frame_size *= st.downsample;

			Pointer<opus_val16> lpc = st.lpc;
			Pointer<opus_val16> oldBandE = st.oldEBands;
			Pointer<opus_val16> oldLogE = st.oldLogE;
			Pointer<opus_val16> oldLogE2 = st.oldLogE2;
			Pointer<opus_val16> backgroundLogE = st.backgroundLogE;

			c_int LM;
			{
				for (LM = 0; LM <= mode.maxLM; LM++)
				{
					if ((mode.shortMdctSize << LM) == frame_size)
						break;
				}

				if (LM > mode.maxLM)
					return (c_int)OpusError.Bad_Arg;
			}

			c_int M = 1 << LM;

			if ((len < 0) || (len > 1275) || pcm.IsNull)
				return (c_int)OpusError.Bad_Arg;

			c_int N = M * mode.shortMdctSize;
			c_int c = 0;

			do
			{
				decode_mem[c] = st.decode_mem + c * (Decode_Buffer_Size + overlap);
				out_syn[c] = decode_mem[c] + Decode_Buffer_Size - N;
			}
			while (++c < CC);

			c_int effEnd = end;
			if (effEnd > mode.effEBands)
				effEnd = mode.effEBands;

			if (data.IsNull || (len <= 1))
			{
				Celt_Decode_Lost(st, N, LM);
				Deemphasis(out_syn, pcm, N, CC, st.downsample, mode.preemph, st.preemph_memD, accum);

				return frame_size / st.downsample;
			}

			// Check if there are at least two packets received consecutively before
			// turning on the pitch-based PLC
			if (st.loss_duration == 0)
				st.skip_plc = false;

			if (dec == null)
				EntDec.Ec_Dec_Init(out dec, data, (opus_uint32)len);

			if (C == 1)
			{
				for (c_int i = 0; i < nbEBands; i++)
					oldBandE[i] = Arch.MAX16(oldBandE[i], oldBandE[nbEBands + i]);
			}

			opus_int32 total_bits = len * 8;
			opus_int32 tell = EntCode.Ec_Tell(dec);
			bool silence;

			if (tell >= total_bits)
				silence = true;
			else if (tell == 1)
				silence = EntDec.Ec_Dec_Bit_Logp(dec, 15);
			else
				silence = false;

			if (silence)
			{
				// Pretend we've read all the remaining bits
				tell = len * 8;
				dec.nbits_total += tell - EntCode.Ec_Tell(dec);
			}

			opus_val16 postfilter_gain = 0;
			c_int postfilter_pitch = 0;
			c_int postfilter_tapset = 0;

			if ((start == 0) && ((tell + 16) <= total_bits))
			{
				if (EntDec.Ec_Dec_Bit_Logp(dec, 1))
				{
					c_int octave = (c_int)EntDec.Ec_Dec_UInt(dec, 6);
					postfilter_pitch = (c_int)((16 << octave) + EntDec.Ec_Dec_Bits(dec, (c_uint)(4 + octave)) - 1);
					c_int qg = (c_int)EntDec.Ec_Dec_Bits(dec, 3);

					if ((EntCode.Ec_Tell(dec) + 2) <= total_bits)
						postfilter_tapset = EntDec.Ec_Dec_Icdf(dec, Tables.Tapset_Icdf, 2);

					postfilter_gain = Arch.QCONST16(.09375f, 15) * (qg + 1);
				}

				tell = EntCode.Ec_Tell(dec);
			}

			bool isTransient;
			if ((LM > 0) && ((tell + 3) <= total_bits))
			{
				isTransient = EntDec.Ec_Dec_Bit_Logp(dec, 3);
				tell = EntCode.Ec_Tell(dec);
			}
			else
				isTransient = false;

			c_int shortBlocks;
			if (isTransient)
				shortBlocks = M;
			else
				shortBlocks = 0;

			// Decode the global flags (first symbols in the stream)
			bool intra_ener = (tell + 3) <= total_bits ? EntDec.Ec_Dec_Bit_Logp(dec, 3) : false;

			// If recovering from packet loss, make sure we make the energy prediction safe to reduce the
			// risk of getting loud artifacts
			if (!intra_ener && (st.loss_duration != 0))
			{
				c = 0;

				do
				{
					opus_val16 safety = 0;
					c_int missing = Arch.IMIN(10, st.loss_duration >> LM);

					if (LM == 0)
						safety = Arch.QCONST16(1.5f, Constants.Db_Shift);
					else if (LM == 1)
						safety = Arch.QCONST16(0.5f, Constants.Db_Shift);

					for (c_int i = start; i < end; i++)
					{
						if (oldBandE[c * nbEBands + i] < Arch.MAX16(oldLogE[c * nbEBands + i], oldLogE2[c * nbEBands + i]))
						{
							// If energy is going down already, continue the trend
							opus_val32 E0 = oldBandE[c * nbEBands + i];
							opus_val32 E1 = oldLogE[c * nbEBands + i];
							opus_val32 E2 = oldLogE2[c * nbEBands + i];
							opus_val32 slope = Arch.MAX32(E1 - E0, Arch.HALF32(E2 - E0));
							E0 -= Arch.MAX32(0, (1 + missing) * slope);
							oldBandE[c * nbEBands + i] = Arch.MAX32(-Arch.QCONST16(20.0f, Constants.Db_Shift), E0);
						}
						else
						{
							// Otherwise take the min of the last frames
							oldBandE[c * nbEBands + i] = Arch.MIN16(Arch.MIN16(oldBandE[c * nbEBands + i], oldLogE[c * nbEBands + i]), oldLogE2[c * nbEBands + i]);
						}

						// Shorter frames have more natural fluctuations -- play it safe
						oldBandE[c * nbEBands + i] -= safety;
					}
				}
				while (++c < 2);
			}

			// Get band energies
			Quant_Bands.Unquant_Coarse_Energy(mode, start, end, oldBandE, intra_ener, dec, C, LM);

			c_int[] tf_res = new c_int[nbEBands];
			Tf_Decode(start, end, isTransient, tf_res, LM, dec);

			tell = EntCode.Ec_Tell(dec);
			Spread spread_decision = Spread.Normal;

			if ((tell + 4) <= total_bits)
				spread_decision = (Spread)EntDec.Ec_Dec_Icdf(dec, Tables.Spread_Icdf, 5);

			c_int[] cap = new c_int[nbEBands];

			Celt.Init_Caps(mode, cap, LM, C);

			c_int[] offsets = new c_int[nbEBands];

			c_int dynalloc_logp = 6;
			total_bits <<= Constants.BitRes;
			tell = (opus_int32)EntCode.Ec_Tell_Frac(dec);

			for (c_int i = start; i < end; i++)
			{
				c_int width = C * (eBands[i + 1] - eBands[i]) << LM;

				// Quanta is 6 bits, but no more than 1 bit/sample
				// and no less than 1/8 bit/sample
				c_int quanta = Arch.IMIN(width << Constants.BitRes, Arch.IMAX(6 << Constants.BitRes, width));
				c_int dynalloc_loop_logp = dynalloc_logp;
				c_int boost = 0;

				while ((tell + (dynalloc_loop_logp << Constants.BitRes) < total_bits) && (boost < cap[i]))
				{
					bool flag = EntDec.Ec_Dec_Bit_Logp(dec, (c_uint)dynalloc_loop_logp);
					tell = (opus_int32)EntCode.Ec_Tell_Frac(dec);

					if (!flag)
						break;

					boost += quanta;
					total_bits -= quanta;
					dynalloc_loop_logp = 1;
				}

				offsets[i] = boost;

				// Making dynalloc more likely
				if (boost > 0)
					dynalloc_logp = Arch.IMAX(2, dynalloc_logp - 1);
			}

			c_int[] fine_quant = new c_int[nbEBands];
			c_int alloc_trim = tell + (6 << Constants.BitRes) <= total_bits ? EntDec.Ec_Dec_Icdf(dec, Tables.Trim_Icdf, 7) : 5;

			opus_int32 bits = ((len * 8) << Constants.BitRes) - (opus_int32)EntCode.Ec_Tell_Frac(dec) - 1;
			c_int anti_collapse_rsv = isTransient && (LM >= 2) && (bits >= ((LM + 2) << Constants.BitRes)) ? 1 << Constants.BitRes : 0;
			bits -= anti_collapse_rsv;

			c_int[] pulses = new c_int[nbEBands];
			bool[] fine_priority = new bool[nbEBands];

			c_int codedBands = Rate.Clt_Compute_Allocation(mode, start, end, offsets, cap, alloc_trim, ref intensity, ref dual_stereo, bits, out opus_int32 balance, pulses, fine_quant, fine_priority, C, LM, dec, false, 0, 0);
			Quant_Bands.Unquant_Fine_Energy(mode, start, end, oldBandE, fine_quant, dec, C);

			c = 0;

			do
			{
				Memory.Opus_Move(decode_mem[c], decode_mem[c] + N, Decode_Buffer_Size - N + overlap);
			}
			while (++c < CC);

			// Decode fixed codebook
			byte[] collapse_masks = new byte[C * nbEBands];
			Pointer<celt_norm> X = new Pointer<celt_norm>(C * N);		// < Interleaved normalised MDCTs

			Bands.Quant_All_Bands(false, mode, start, end, X, C == 2 ? X + N : null, collapse_masks, null, pulses, shortBlocks, spread_decision, dual_stereo, intensity, tf_res, len * (8 << Constants.BitRes) - anti_collapse_rsv, balance, ref dec, LM, codedBands, ref st.rng, 0, st.arch, st.disable_inv);

			if (anti_collapse_rsv > 0)
				anti_collapse_on = EntDec.Ec_Dec_Bits(dec, 1) != 0;

			Quant_Bands.Unquant_Energy_Finalise(mode, start, end, oldBandE, fine_quant, fine_priority, len * 8 - EntCode.Ec_Tell(dec), dec, C);

			if (anti_collapse_on)
				Bands.Anti_Collapse(mode, X, collapse_masks, LM, C, N, start, end, oldBandE, oldLogE, oldLogE2, pulses, st.rng, st.arch);

			if (silence)
			{
				for (c_int i = 0; i < (C * nbEBands); i++)
					oldBandE[i] = -Arch.QCONST16(28.0f, Constants.Db_Shift);
			}

			if (st.prefilter_and_fold)
				Prefilter_And_Fold(st, N);

			Celt_Synthesis(mode, X, out_syn, oldBandE, start, effEnd, C, CC, isTransient, LM, st.downsample, silence, st.arch);

			c = 0;

			do
			{
				st.postfilter_period = Arch.IMAX(st.postfilter_period, Constants.CombFilter_MinPeriod);
				st.postfilter_period_old = Arch.IMAX(st.postfilter_period_old, Constants.CombFilter_MinPeriod);

				Celt.Comb_Filter(out_syn[c], out_syn[c], st.postfilter_period_old, st.postfilter_period, mode.shortMdctSize, st.postfilter_gain_old, st.postfilter_gain, st.postfilter_tapset_old, st.postfilter_tapset, mode.window, overlap, st.arch);

				if (LM != 0)
					Celt.Comb_Filter(out_syn[c] + mode.shortMdctSize, out_syn[c] + mode.shortMdctSize, st.postfilter_period, postfilter_pitch, N - mode.shortMdctSize, st.postfilter_gain, postfilter_gain, st.postfilter_tapset, postfilter_tapset, mode.window, overlap, st.arch);
			}
			while (++c < CC);

			st.postfilter_period_old = st.postfilter_period;
			st.postfilter_gain_old = st.postfilter_gain;
			st.postfilter_tapset_old = st.postfilter_tapset;
			st.postfilter_period = postfilter_pitch;
			st.postfilter_gain = postfilter_gain;
			st.postfilter_tapset = postfilter_tapset;

			if (LM != 0)
			{
				st.postfilter_period_old = st.postfilter_period;
				st.postfilter_gain_old = st.postfilter_gain;
				st.postfilter_tapset_old = st.postfilter_tapset;
			}

			if (C == 1)
				Memory.Opus_Copy(oldBandE + nbEBands, oldBandE, nbEBands);

			if (!isTransient)
			{
				Memory.Opus_Copy(oldLogE2, oldLogE, 2 * nbEBands);
				Memory.Opus_Copy(oldLogE, oldBandE, 2 * nbEBands);
			}
			else
			{
				for (c_int i = 0; i < (2 * nbEBands); i++)
					oldLogE[i] = Arch.MIN16(oldLogE[i], oldBandE[i]);
			}

			// In normal circumstances, we only allow the noise floor to increase by
			// up to 2.4 dB/second, but when we're in DTX we give the weight of
			// all missing packets to the update packet
			opus_val16 max_background_increase = Arch.IMIN(160, st.loss_duration + M) * Arch.QCONST16(0.001f, Constants.Db_Shift);

			for (c_int i = 0; i < (2 * nbEBands); i++)
				backgroundLogE[i] = Arch.MIN16(backgroundLogE[i] + max_background_increase, oldBandE[i]);

			// In case start or end were to change
			c = 0;

			do
			{
				for (c_int i = 0; i < start; i++)
				{
					oldBandE[c * nbEBands + i] = 0;
					oldLogE[c * nbEBands + i] = oldLogE2[c * nbEBands + i] = -Arch.QCONST16(28.0f, Constants.Db_Shift);
				}

				for (c_int i = end; i < nbEBands; i++)
				{
					oldBandE[c * nbEBands + i] = 0;
					oldLogE[c * nbEBands + i] = oldLogE2[c * nbEBands + i] = -Arch.QCONST16(28.0f, Constants.Db_Shift);
				}
			}
			while (++c < 2);

			st.rng = dec.rng;

			Deemphasis(out_syn, pcm, N, CC, st.downsample, mode.preemph, st.preemph_memD, accum);

			st.loss_duration = 0;
			st.prefilter_and_fold = false;

			if (EntCode.Ec_Tell(dec) > (8 * len))
				return (c_int)OpusError.Internal_Error;

			if (EntCode.Ec_Get_Error(dec))
				st.error = true;

			return frame_size / st.downsample;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Celt_Decode_With_Ec(CeltDecoder st, Pointer<byte> data, c_int len, Pointer<opus_val16> pcm, c_int frame_size, Ec_Dec dec, bool accum)
		{
			return Celt_Decode_With_Ec_Dred(st, data, len, pcm, frame_size, dec, accum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Celt_Decoder_Ctl_Get<T>(CeltDecoder st, OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			return Opus_Custom_Decoder_Ctl_Get(st, request, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Celt_Decoder_Ctl_Get(CeltDecoder st, OpusControlGetRequest request, out CeltMode _out)
		{
			return Opus_Custom_Decoder_Ctl_Get(st, request, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Custom_Decoder_Ctl_Get<T>(CeltDecoder st, OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			// I know, the casting below in the case statements are not pretty
			if (typeof(T) == typeof(c_int))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Pitch:
					{
						_out = (T)(object)st.postfilter_period;
						return OpusError.Ok;
					}
				}
			}
			else if (typeof(T) == typeof(opus_uint32))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Final_Range:
					{
						_out = (T)(object)st.rng;
						return OpusError.Ok;
					}
				}
			}

			_out = default;
			return OpusError.Unimplemented;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Custom_Decoder_Ctl_Get(CeltDecoder st, OpusControlGetRequest request, out CeltMode _out)
		{
			switch (request)
			{
				case OpusControlGetRequest.Celt_Get_Mode:
				{
					_out = st.mode;
					return OpusError.Ok;
				}
			}

			_out = default;
			return OpusError.Unimplemented;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Celt_Decoder_Ctl_Set(CeltDecoder st, OpusControlSetRequest request, params object[] args)
		{
			return Opus_Custom_Decoder_Ctl_Set(st, request, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Custom_Decoder_Ctl_Set(CeltDecoder st, OpusControlSetRequest request, params object[] args)
		{
			switch (request)
			{
				case OpusControlSetRequest.Celt_Set_Start_Band:
				{
					opus_int32 value = (opus_int32)args[0];

					if ((value < 0) || (value > st.mode.nbEBands))
						return OpusError.Bad_Arg;

					st.start = value;
					break;
				}

				case OpusControlSetRequest.Celt_Set_End_Band:
				{
					opus_int32 value = (opus_int32)args[0];

					if ((value < 1) || (value > st.mode.nbEBands))
						return OpusError.Bad_Arg;

					st.end = value;
					break;
				}

				case OpusControlSetRequest.Celt_Set_Channels:
				{
					opus_int32 value = (opus_int32)args[0];

					if ((value < 1) || (value > 2))
						return OpusError.Bad_Arg;

					st.stream_channels = value;
					break;
				}

				case OpusControlSetRequest.Opus_Reset_State:
				{
					st.Reset();

					for (c_int i = 0; i < (2 * st.mode.nbEBands); i++)
						st.oldLogE[i] = st.oldLogE2[i] = -Arch.QCONST16(28.0f, Constants.Db_Shift);

					st.skip_plc = true;
					break;
				}

				case OpusControlSetRequest.Celt_Set_Signalling:
				{
					opus_int32 value = (opus_int32)args[0];

					st.signalling = value;
					break;
				}

				default:
					return OpusError.Unimplemented;
			}

			return OpusError.Ok;
		}
	}
}

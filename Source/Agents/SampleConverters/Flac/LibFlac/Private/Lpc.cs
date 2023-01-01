/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lpc : ILpc
	{
		/********************************************************************/
		/// <summary>
		/// Applies the given window to the data
		/// </summary>
		/********************************************************************/
		public static void Flac__Lpc_Window_Data(Flac__int32[] @in, Flac__real[] window, Flac__real[] @out, uint32_t data_Len)
		{
			for (uint32_t i = 0; i < data_Len; i++)
				@out[i] = @in[i] * window[i];
		}



		/********************************************************************/
		/// <summary>
		/// Compute the autocorrelation for lags between 0 and lag-1.
		/// Assumes data[] outside of [0,data_len-1] == 0.
		/// Asserts that lag > 0.
		/// </summary>
		/********************************************************************/
		public void Compute_Autocorrelation(Flac__real[] data, uint32_t data_Len, uint32_t lag, Flac__real[] autoc)
		{
			uint32_t limit = data_Len - lag;

			Debug.Assert(lag > 0);
			Debug.Assert(lag <= data_Len);

			for (uint32_t coeff = 0; coeff < lag; coeff++)
				autoc[coeff] = 0.0f;

			uint32_t sample;
			for (sample = 0; sample <= limit; sample++)
			{
				Flac__real d = data[sample];

				for (uint32_t coeff = 0; coeff < lag; coeff++)
					autoc[coeff] += d * data[sample + coeff];
			}

			for (; sample < data_Len; sample++)
			{
				Flac__real d = data[sample];

				for (uint32_t coeff = 0; coeff < data_Len - sample; coeff++)
					autoc[coeff] += d * data[sample + coeff];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Computes LP coefficients for orders 1..max_order.
		/// Do not call if autoc[0] == 0.0. This means the signal is zero
		/// and there is no point in calculating a predictor
		/// </summary>
		/********************************************************************/
		public static void Flac__Lpc_Compute_Lp_Coefficients(Flac__real[] autoc, ref uint32_t max_Order, Flac__real[][] lp_Coeff, double[] error)
		{
			double[] lpc = new double[Constants.Flac__Max_Lpc_Order];

			Debug.Assert(0 < max_Order);
			Debug.Assert(max_Order <= Constants.Flac__Max_Lpc_Order);
			Debug.Assert(autoc[0] != 0.0);

			double err = autoc[0];

			uint32_t j;
			for (uint32_t i = 0; i < max_Order; i++)
			{
				// Sum up this iteration's reflection coefficient
				double r = -autoc[i + 1];

				for (j = 0; j < i; j++)
					r -= lpc[j] * autoc[i - j];

				r /= err;

				// Update LPC coefficients and total error
				lpc[i] = r;

				for (j = 0; j < (i >> 1); j++)
				{
					double tmp = lpc[j];
					lpc[j] += r * lpc[i - 1 - j];
					lpc[i - 1 - j] += r * tmp;
				}

				if ((i & 1) != 0)
					lpc[j] += lpc[j] * r;

				err *= (1.0 - r * r);

				// Save this order
				for (j = 0; j <= i; j++)
					lp_Coeff[i][j] = (Flac__real)(-lpc[j]);		// Negate FIR filter coeff to get predictor coeff

				error[i] = err;

				// See SF bug https://sourceforge.net/p/flac/bugs/234/
				if (err == 0.0)
				{
					max_Order = i + 1;
					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Quantizes the LP coefficients. NOTE: precision + bits_per_sample
		/// must be less than 32 (sizeof(FLAC__int32)*8)
		/// </summary>
		/********************************************************************/
		public static int Flac__Lpc_Quantize_Coefficients(Flac__real[] lp_Coeff, uint32_t order, uint32_t precision, Flac__int32[] qlp_Coeff, out int shift)
		{
			Debug.Assert(precision > 0);
			Debug.Assert(precision >= Constants.Flac__Min_Qlp_Coeff_Precision);

			// Drop one bit for the sign; from here on out we consider only |lp_Coeff[i]|
			precision--;
			Flac__int32 qMax = 1 << (int)precision;
			Flac__int32 qMin = -qMax;
			qMax--;

			// Calc cMax = max( |lp_Coeff[i]| )
			double cMax = 0.0;
			for (uint32_t i = 0; i < order; i++)
			{
				double d = Math.Abs(lp_Coeff[i]);
				if (d > cMax)
					cMax = d;
			}

			if (cMax <= 0.0)
			{
				// Coefficients are all 0, which means out constant-detect didn't work
				shift = 0;
				return 2;
			}
			else
			{
				int max_ShiftLimit = (1 << (int)(Constants.Flac__SubFrame_Lpc_Qlp_Shift_Len - 1)) - 1;
				int min_ShiftLimit = -max_ShiftLimit - 1;

				CMath.frexp(cMax, out int log2CMax);
				log2CMax--;
				shift = (int)precision - log2CMax - 1;

				if (shift > max_ShiftLimit)
					shift = max_ShiftLimit;
				else if (shift < min_ShiftLimit)
					return 1;
			}

			if (shift >= 0)
			{
				double error = 0.0;

				for (uint32_t i = 0; i < order; i++)
				{
					error += lp_Coeff[i] * (1 << shift);
					Flac__int32 q = (Flac__int32)Math.Round(error, MidpointRounding.AwayFromZero);

					if (q > qMax)
						q = qMax;
					else if (q < qMin)
						q = qMin;

					error -= q;
					qlp_Coeff[i] = q;
				}
			}
			else
			{
				// Negative shift is very rare but due to design flaw, negative shift is
				// not allowed in the decoder, so it must be handled specially by scaling
				// down coeffs
				int nShift = -shift;
				double error = 0.0;

				for (uint32_t i = 0; i < order; i++)
				{
					error += lp_Coeff[i] / (1 << nShift);
					Flac__int32 q = (Flac__int32)Math.Round(error, MidpointRounding.AwayFromZero);

					if (q > qMax)
						q = qMax;
					else if (q < qMin)
						q = qMin;

					error -= q;
					qlp_Coeff[i] = q;
				}

				shift = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the residual signal obtained from subtracting the
		/// predicted signal from the original
		/// </summary>
		/********************************************************************/
		public void Compute_Residual_From_Qlp_Coefficients(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[11] * data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								residual[i] = data[data_Offset + i] - ((qlp_Coeff[0] * data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int32 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * data[data_Offset + i - 1];
							break;
						}
					}

					residual[i] = data[data_Offset + i] - (sum >> lp_Quantization);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generic 64-bit datapath
		/// </summary>
		/********************************************************************/
		public void Compute_Residual_From_Qlp_Coefficients_64Bit(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[11] * data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								residual[i] = data[data_Offset + i] - (Flac__int32)((qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int64 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * (Flac__int64)data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * (Flac__int64)data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * (Flac__int64)data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * (Flac__int64)data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * (Flac__int64)data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * (Flac__int64)data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * (Flac__int64)data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * (Flac__int64)data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * (Flac__int64)data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * (Flac__int64)data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * (Flac__int64)data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * (Flac__int64)data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * (Flac__int64)data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * (Flac__int64)data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * (Flac__int64)data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * (Flac__int64)data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * (Flac__int64)data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * (Flac__int64)data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * (Flac__int64)data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * (Flac__int64)data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * (Flac__int64)data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];
							break;
						}
					}

					residual[i] = data[data_Offset + i] - (Flac__int32)(sum >> lp_Quantization);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// For use when the signal is less-or-equal-to 16 bits-per-sample,
		/// or less-or-equal-to 15 bits-per-sample on a side channel (which
		/// requires 1 extra bit)
		/// </summary>
		/********************************************************************/
		public void Compute_Residual_From_Qlp_Coefficients_16Bit(Flac__int32[] data, uint32_t data_Offset, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] residual)
		{
			Compute_Residual_From_Qlp_Coefficients(data, data_Offset, data_Len, qlp_Coeff, order, lp_Quantization, residual);
		}



		/********************************************************************/
		/// <summary>
		/// Restore the original signal by summing the residual and the
		/// predictor
		/// </summary>
		/********************************************************************/
		public void Restore_Signal(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[11] * data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[10] * data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[9] * data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[8] * data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[7] * data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[6] * data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[5] * data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[4] * data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[3] * data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[2] * data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int32 sum = 0;
								sum += qlp_Coeff[1] * data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * data[data_Offset + i - 1];

								data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								data[data_Offset + i] = residual[i] + ((qlp_Coeff[0] * data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int32 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * data[data_Offset + i - 1];
							break;
						}
					}

					data[data_Offset + i] = residual[i] + (sum >> lp_Quantization);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generic 64-bit datapath
		/// </summary>
		/********************************************************************/
		public void Restore_Signal_64Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Debug.Assert(order > 0);
			Debug.Assert(order <= 32);

			// We do unique versions up to 12th order since that's the subset limit.
			// Also they are roughly ordered to match frequency of occurrence to
			// minimize branching
			if (order <= 12)
			{
				if (order > 8)
				{
					if (order > 10)
					{
						if (order == 12)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[11] * (Flac__int64)data[data_Offset + i - 12];
								sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 11
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 10)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 9
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
				}
				else if (order > 4)
				{
					if (order > 6)
					{
						if (order == 8)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 7
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 6)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 5
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
				}
				else
				{
					if (order > 2)
					{
						if (order == 4)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 3
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
					}
					else
					{
						if (order == 2)
						{
							for (int i = 0; i < data_Len; i++)
							{
								Flac__int64 sum = 0;
								sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
								sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];

								data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
							}
						}
						else	// order == 1
						{
							for (int i = 0; i < data_Len; i++)
								data[data_Offset + i] = residual[i] + (Flac__int32)((qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1]) >> lp_Quantization);
						}
					}
				}
			}
			else	// order == 12
			{
				for (int i = 0; i < data_Len; i++)
				{
					Flac__int64 sum = 0;

					switch (order)
					{
						case 32:
						{
							sum += qlp_Coeff[31] * (Flac__int64)data[data_Offset + i - 32];
							goto case 31;
						}

						case 31:
						{
							sum += qlp_Coeff[30] * (Flac__int64)data[data_Offset + i - 31];
							goto case 30;
						}

						case 30:
						{
							sum += qlp_Coeff[29] * (Flac__int64)data[data_Offset + i - 30];
							goto case 29;
						}

						case 29:
						{
							sum += qlp_Coeff[28] * (Flac__int64)data[data_Offset + i - 29];
							goto case 28;
						}

						case 28:
						{
							sum += qlp_Coeff[27] * (Flac__int64)data[data_Offset + i - 28];
							goto case 27;
						}

						case 27:
						{
							sum += qlp_Coeff[26] * (Flac__int64)data[data_Offset + i - 27];
							goto case 26;
						}

						case 26:
						{
							sum += qlp_Coeff[25] * (Flac__int64)data[data_Offset + i - 26];
							goto case 25;
						}

						case 25:
						{
							sum += qlp_Coeff[24] * (Flac__int64)data[data_Offset + i - 25];
							goto case 24;
						}

						case 24:
						{
							sum += qlp_Coeff[23] * (Flac__int64)data[data_Offset + i - 24];
							goto case 23;
						}

						case 23:
						{
							sum += qlp_Coeff[22] * (Flac__int64)data[data_Offset + i - 23];
							goto case 22;
						}

						case 22:
						{
							sum += qlp_Coeff[21] * (Flac__int64)data[data_Offset + i - 22];
							goto case 21;
						}

						case 21:
						{
							sum += qlp_Coeff[20] * (Flac__int64)data[data_Offset + i - 21];
							goto case 20;
						}

						case 20:
						{
							sum += qlp_Coeff[19] * (Flac__int64)data[data_Offset + i - 20];
							goto case 19;
						}

						case 19:
						{
							sum += qlp_Coeff[18] * (Flac__int64)data[data_Offset + i - 19];
							goto case 18;
						}

						case 18:
						{
							sum += qlp_Coeff[17] * (Flac__int64)data[data_Offset + i - 18];
							goto case 17;
						}

						case 17:
						{
							sum += qlp_Coeff[16] * (Flac__int64)data[data_Offset + i - 17];
							goto case 16;
						}

						case 16:
						{
							sum += qlp_Coeff[15] * (Flac__int64)data[data_Offset + i - 16];
							goto case 15;
						}

						case 15:
						{
							sum += qlp_Coeff[14] * (Flac__int64)data[data_Offset + i - 15];
							goto case 14;
						}

						case 14:
						{
							sum += qlp_Coeff[13] * (Flac__int64)data[data_Offset + i - 14];
							goto case 13;
						}

						case 13:
						{
							sum += qlp_Coeff[12] * (Flac__int64)data[data_Offset + i - 13];
							sum += qlp_Coeff[11] * (Flac__int64)data[data_Offset + i - 12];
							sum += qlp_Coeff[10] * (Flac__int64)data[data_Offset + i - 11];
							sum += qlp_Coeff[9] * (Flac__int64)data[data_Offset + i - 10];
							sum += qlp_Coeff[8] * (Flac__int64)data[data_Offset + i - 9];
							sum += qlp_Coeff[7] * (Flac__int64)data[data_Offset + i - 8];
							sum += qlp_Coeff[6] * (Flac__int64)data[data_Offset + i - 7];
							sum += qlp_Coeff[5] * (Flac__int64)data[data_Offset + i - 6];
							sum += qlp_Coeff[4] * (Flac__int64)data[data_Offset + i - 5];
							sum += qlp_Coeff[3] * (Flac__int64)data[data_Offset + i - 4];
							sum += qlp_Coeff[2] * (Flac__int64)data[data_Offset + i - 3];
							sum += qlp_Coeff[1] * (Flac__int64)data[data_Offset + i - 2];
							sum += qlp_Coeff[0] * (Flac__int64)data[data_Offset + i - 1];
							break;
						}
					}

					data[data_Offset + i] = (Flac__int32)(residual[i] + (sum >> lp_Quantization));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// For use when the signal is less-or-equal-to 16 bits-per-sample,
		/// or less-or-equal-to 15 bits-per-sample on a side channel (which
		/// requires 1 extra bit)
		/// </summary>
		/********************************************************************/
		public void Restore_Signal_16Bit(Flac__int32[] residual, uint32_t data_Len, Flac__int32[] qlp_Coeff, uint32_t order, int lp_Quantization, Flac__int32[] data, uint32_t data_Offset)
		{
			Restore_Signal(residual, data_Len, qlp_Coeff, order, lp_Quantization, data, data_Offset);
		}



		/********************************************************************/
		/// <summary>
		/// Compute the expected number of bits per residual signal sample
		/// based on the LP error (which is related to the residual variance)
		/// </summary>
		/********************************************************************/
		public static double Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample(double lpc_Error, uint32_t total_Samples)
		{
			Debug.Assert(total_Samples > 0);

			double error_Scale = 0.5 / total_Samples;

			return Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample_With_Error_Scale(lpc_Error, error_Scale);
		}



		/********************************************************************/
		/// <summary>
		/// Compute the best order from the array of signal errors returned
		/// during coefficient computation
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__Lpc_Compute_Best_Order(double[] lpc_Error, uint32_t max_Order, uint32_t total_Samples, uint32_t overhead_Bits_Per_Order)
		{
			Debug.Assert(max_Order > 0);
			Debug.Assert(total_Samples > 0);

			double error_Scale = 0.5 / total_Samples;

			uint32_t best_Index = 0;
			double best_Bits = uint32_t.MaxValue;

			for (uint32_t indx = 0, order = 1; indx < max_Order; indx++, order++)
			{
				double bits = Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample_With_Error_Scale(lpc_Error[indx], error_Scale) * (total_Samples - order) + (order * overhead_Bits_Per_Order);
				if (bits < best_Bits)
				{
					best_Index = indx;
					best_Bits = bits;
				}
			}

			return best_Index + 1;	// +1 since indx of lpc_Error[] is order-1
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static double Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample_With_Error_Scale(double lpc_Error, double error_Scale)
		{
			if (lpc_Error > 0.0)
			{
				double bps = 0.5 * Math.Log(error_Scale * lpc_Error) / Constants.M_LN2;
				if (bps >= 0.0)
					return bps;
				else
					return 0.0;
			}
			else if (lpc_Error < 0.0)	// Error should not be negative but can happen due to inadequate floating-point resolution
				return 1e32;
			else
				return 0.0;
		}
		#endregion
	}
}

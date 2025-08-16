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
	internal static class Celt
	{
		private static readonly opus_val16[][] gains =
		[
			[ Arch.QCONST16(0.3066406250f, 15), Arch.QCONST16(0.2170410156f, 15), Arch.QCONST16(0.1296386719f, 15) ],
			[ Arch.QCONST16(0.4638671875f, 15), Arch.QCONST16(0.2680664062f, 15), Arch.QCONST16(0.0f, 15) ],
			[ Arch.QCONST16(0.7998046875f, 15), Arch.QCONST16(0.1000976562f, 15), Arch.QCONST16(0.0f, 15) ]
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Resampling_Factor(opus_int32 rate)
		{
			c_int ret;

			switch (rate)
			{
				case 48000:
				{
					ret = 1;
					break;
				}

				case 24000:
				{
					ret = 2;
					break;
				}

				case 16000:
				{
					ret = 3;
					break;
				}

				case 12000:
				{
					ret = 4;
					break;
				}

				case 8000:
				{
					ret = 6;
					break;
				}

				default:
				{
					ret = 0;
					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Comb_Filter_Const_C(CPointer<opus_val32> y, CPointer<opus_val32> x, c_int T, c_int N, opus_val16 g10, opus_val16 g11, opus_val16 g12)
		{
			opus_val32 x4 = x[-T - 2];
			opus_val32 x3 = x[-T - 1];
			opus_val32 x2 = x[-T];
			opus_val32 x1 = x[-T + 1];

			for (c_int i = 0; i < N; i++)
			{
				opus_val32 x0 = x[i - T + 2];
				y[i] = x[i]
		                 + Arch.MULT16_32_Q15(g10, x2)
						 + Arch.MULT16_32_Q15(g11, Arch.ADD32(x1, x3))
						 + Arch.MULT16_32_Q15(g12, Arch.ADD32(x0, x4));
				y[i] = Arch.SATURATE(y[i], Constants.Sig_Sat);

				x4 = x3;
				x3 = x2;
				x2 = x1;
				x1 = x0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Comb_Filter(CPointer<opus_val32> y, CPointer<opus_val32> x, c_int T0, c_int T1, c_int N, opus_val16 g0, opus_val16 g1, c_int tapset0, c_int tapset1, CPointer<opus_val16> window, c_int overlap, c_int arch)
		{
			if ((g0 == 0) && (g1 == 0))
			{
				// OPT: Happens to work without the OPUS_MOVE(), but only because the current encoder already copies x to y
				if (x != y)
					Memory.Opus_Move(y, x, N);

				return;
			}

			// When the gain is zero, T0 and/or T1 is set to zero. We need
			// to have then be at least 2 to avoid processing garbage data
			T0 = Arch.IMAX(T0, Constants.CombFilter_MinPeriod);
			T1 = Arch.IMAX(T1, Constants.CombFilter_MinPeriod);
			opus_val16 g00 = Arch.MULT16_16_P15(g0, gains[tapset0][0]);
			opus_val16 g01 = Arch.MULT16_16_P15(g0, gains[tapset0][1]);
			opus_val16 g02 = Arch.MULT16_16_P15(g0, gains[tapset0][2]);
			opus_val16 g10 = Arch.MULT16_16_P15(g1, gains[tapset1][0]);
			opus_val16 g11 = Arch.MULT16_16_P15(g1, gains[tapset1][1]);
			opus_val16 g12 = Arch.MULT16_16_P15(g1, gains[tapset1][2]);
			opus_val32 x1 = x[-T1 + 1];
			opus_val32 x2 = x[-T1];
			opus_val32 x3 = x[-T1 - 1];
			opus_val32 x4 = x[-T1 - 2];

			// If the filter didn't change, we don't need the overlap
			if ((g0 == g1) && (T0 == T1) && (tapset0 == tapset1))
				overlap = 0;

			c_int i;
			for (i = 0; i < overlap; i++)
			{
				opus_val32 x0 = x[i - T1 + 2];
				opus_val16 f = Arch.MULT16_16_Q15(window[i], window[i]);

				y[i] = x[i]
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15((Constants.Q15One - f), g00), x[i - T0])
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15((Constants.Q15One - f), g01), Arch.ADD32(x[i - T0 + 1], x[i - T0 - 1]))
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15((Constants.Q15One - f), g02), Arch.ADD32(x[i - T0 + 2], x[i - T0 - 2]))
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15(f, g10), x2)
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15(f, g11), Arch.ADD32(x1, x3))
						+ Arch.MULT16_32_Q15(Arch.MULT16_16_Q15(f, g12), Arch.ADD32(x0, x4));
				y[i] = Arch.SATURATE(y[i], Constants.Sig_Sat);

				x4 = x3;
				x3 = x2;
				x2 = x1;
				x1 = x0;
			}

			if (g1 == 0)
			{
				// OPT: Happens to work without the OPUS_MOVE(), but only because the current encoder already copies x to y
				if (x != y)
					Memory.Opus_Move(y + overlap, x + overlap, N - overlap);

				return;
			}

			// Compute the part with the constant filter
			Pitch.Comb_Filter_Const(y + i, x + i, T1, N - i, g10, g11, g12, arch);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Init_Caps(CeltMode m, CPointer<c_int> cap, c_int LM, c_int C)
		{
			for (c_int i = 0; i < m.nbEBands; i++)
			{
				c_int N = (m.eBands[i + 1] - m.eBands[i]) << LM;
				cap[i] = (m.cache.caps[m.nbEBands * (2 * LM + C - 1) + i] + 64) * C * N >> 2;
			}
		}
	}
}

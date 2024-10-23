/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Quant_Bands
	{
		private const opus_val16 Beta_Intra = 4915 / 32768.0f;

		private static readonly opus_val16[] pred_coef = [ 29440 / 32768.0f, 26112 / 32768.0f, 21248 / 32768.0f, 16384 / 32768.0f ];
		private static readonly opus_val16[] beta_coef = [ 30147 / 32768.0f, 22282 / 32768.0f, 12124 / 32768.0f, 6554 / 32768.0f ];

		/// <summary>
		/// Parameters of the Laplace-like probability models used for the
		/// coarse energy.
		/// There is one pair of parameters for each frame size, prediction
		/// type (inter/intra), and band number.
		/// The first number of each pair is the probability of 0, and the
		/// second is the decay rate, both in Q8 precision
		/// </summary>
		private static readonly byte[][][] e_prob_model =
		[
			// 120 sample frames
			[
				// Inter
				[
					 72, 127,  65, 129,  66, 128,  65, 128,  64, 128,  62, 128,  64, 128,
					 64, 128,  92,  78,  92,  79,  92,  78,  90,  79, 116,  41, 115,  40,
					114,  40, 132,  26, 132,  26, 145,  17, 161,  12, 176,  10, 177,  11
				],
				// Intra
				[
					 24, 179,  48, 138,  54, 135,  54, 132,  53, 134,  56, 133,  55, 132,
					 55, 132,  61, 114,  70,  96,  74,  88,  75,  88,  87,  74,  89,  66,
					 91,  67, 100,  59, 108,  50, 120,  40, 122,  37,  97,  43,  78,  50
				]
			],
			// 240 sample frames
			[
				// Inter
				[
					 83,  78,  84,  81,  88,  75,  86,  74,  87,  71,  90,  73,  93,  74,
					 93,  74, 109,  40, 114,  36, 117,  34, 117,  34, 143,  17, 145,  18,
					146,  19, 162,  12, 165,  10, 178,   7, 189,   6, 190,   8, 177,   9
				],
				// Intra
				[
					 23, 178,  54, 115,  63, 102,  66,  98,  69,  99,  74,  89,  71,  91,
					 73,  91,  78,  89,  86,  80,  92,  66,  93,  64, 102,  59, 103,  60,
					104,  60, 117,  52, 123,  44, 138,  35, 133,  31,  97,  38,  77,  45
				]
			],
			// 480 sample frames
			[
				// Inter
				[
					 61,  90,  93,  60, 105,  42, 107,  41, 110,  45, 116,  38, 113,  38,
					112,  38, 124,  26, 132,  27, 136,  19, 140,  20, 155,  14, 159,  16,
					158,  18, 170,  13, 177,  10, 187,   8, 192,   6, 175,   9, 159,  10
				],
				// Intra
				[
					 21, 178,  59, 110,  71,  86,  75,  85,  84,  83,  91,  66,  88,  73,
					 87,  72,  92,  75,  98,  72, 105,  58, 107,  54, 115,  52, 114,  55,
					112,  56, 129,  51, 132,  40, 150,  33, 140,  29,  98,  35,  77,  42
				]
			],
			// 960 sample frames
			[
				// Inter
				[
					 42, 121,  96,  66, 108,  43, 111,  40, 117,  44, 123,  32, 120,  36,
					119,  33, 127,  33, 134,  34, 139,  21, 147,  23, 152,  20, 158,  25,
					154,  26, 166,  21, 173,  16, 184,  13, 184,  10, 150,  13, 139,  15
				],
				// Intra
				[
					 22, 178,  63, 114,  74,  82,  84,  83,  92,  82, 103,  62,  96,  72,
					 96,  67, 101,  73, 107,  72, 113,  55, 118,  52, 125,  52, 118,  52,
					117,  55, 135,  49, 137,  39, 157,  32, 145,  29,  97,  33,  77,  40
				]
			]
		];

		private static readonly byte[] small_energy_icdf = [ 2, 1, 0 ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Unquant_Coarse_Energy(CeltMode m, c_int start, c_int end, Pointer<opus_val16> oldEBands, bool intra, Ec_Dec dec, c_int C, c_int LM)
		{
			byte[] prob_model = e_prob_model[LM][intra ? 1 : 0];
			opus_val32[] prev = [ 0, 0 ];
			opus_val16 coef;
			opus_val16 beta;

			if (intra)
			{
				coef = 0;
				beta = Beta_Intra;
			}
			else
			{
				beta = beta_coef[LM];
				coef = pred_coef[LM];
			}

			opus_int32 budget = (opus_int32)dec.storage * 8;

			// Decode at a fixed coarse resolution
			for (c_int i = start; i < end; i++)
			{
				c_int c = 0;

				do
				{
					c_int qi;

					opus_int32 tell = EntCode.Ec_Tell(dec);

					if ((budget - tell) >= 15)
					{
						c_int pi = 2 * Arch.IMIN(i, 20);
						qi = Laplace.Ec_Laplace_Decode(dec, (c_uint)prob_model[pi] << 7, prob_model[pi + 1] << 6);
					}
					else if ((budget - tell) >= 2)
					{
						qi = EntDec.Ec_Dec_Icdf(dec, small_energy_icdf, 2);
						qi = (qi >> 1) ^ -(qi & 1);
					}
					else if ((budget - tell) >= 1)
						qi = -(EntDec.Ec_Dec_Bit_Logp(dec, 1) ? 1 : 0);
					else
						qi = -1;

					opus_val32 q = Arch.SHL32(Arch.EXTEND32(qi), Constants.Db_Shift);

					oldEBands[i + c * m.nbEBands] = Arch.MAX16(-Arch.QCONST16(9.0f, Constants.Db_Shift), oldEBands[i + c * m.nbEBands]);
					opus_val32 tmp = Arch.PSHR32(Arch.MULT16_16(coef, oldEBands[i + c * m.nbEBands]), 8) + prev[c] + Arch.SHL32(q, 7);
					oldEBands[i + c * m.nbEBands] = Arch.PSHR32(tmp, 7);
					prev[c] = prev[c] + Arch.SHL32(q, 7) - Arch.MULT16_16(beta, Arch.PSHR32(q, 8));
				}
				while (++c < C);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Unquant_Fine_Energy(CeltMode m, c_int start, c_int end, Pointer<opus_val16> oldEBands, Pointer<c_int> fine_quant, Ec_Dec dec, c_int C)
		{
			// Decode finer resolution
			for (c_int i = start; i < end; i++)
			{
				if (fine_quant[i] <= 0)
					continue;

				c_int c = 0;

				do
				{
					c_int q2 = (c_int)EntDec.Ec_Dec_Bits(dec, (c_uint)fine_quant[i]);
					opus_val16 offset = (q2 + 0.5f) * (1 << (14 - fine_quant[i])) * (1.0f / 16384) - 0.5f;
					oldEBands[i + c * m.nbEBands] += offset;
				}
				while (++c < C);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Unquant_Energy_Finalise(CeltMode m, c_int start, c_int end, Pointer<opus_val16> oldEBands, Pointer<c_int> fine_quant, Pointer<bool> fine_priority, c_int bits_left, Ec_Dec dec, c_int C)
		{
			bool[] prioList = [ false, true ];

			// Use up the remaining bits
			foreach (bool prio in prioList)
			{
				for (c_int i = start; ((i < end) && (bits_left >= C)); i++)
				{
					if ((fine_quant[i] >= Constants.Max_Fine_Bits) || (fine_priority[i] != prio))
						continue;

					c_int c = 0;

					do
					{
						c_int q2 = (c_int)EntDec.Ec_Dec_Bits(dec, 1);
						opus_val16 offset = (q2 - 0.5f) * (1 << (14 - fine_quant[i] - 1)) * (1.0f / 16384);
						oldEBands[i + c * m.nbEBands] += offset;
						bits_left--;
					}
					while (++c < C);
				}
			}
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Laplace
	{
		// The minimum probability of an energy delta (out of 32768)
		private const int Laplace_Log_MinP = 0;
		public const c_uint Laplace_MinP = 1 << Laplace_Log_MinP;

		// The minimum number of guaranteed representable energy deltas (in one direction)
		public const c_uint Laplace_NMin = 16;

		/********************************************************************/
		/// <summary>
		/// When called, decay is positive and at most 11456
		/// </summary>
		/********************************************************************/
		private static c_uint Ec_Laplace_Get_Freq1(c_uint fs0, c_int decay)
		{
			c_uint ft = 32768 - Laplace_MinP * (2 * Laplace_NMin) - fs0;

			return (c_uint)(ft * (16384 - decay) >> 15);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Laplace_Encode(Ec_Enc enc, ref c_int value, c_uint fs, c_int decay)
		{
			c_int val = value;
			c_uint fl = 0;

			if (val != 0)
			{
				c_int s = -(val < 0 ? 1 : 0);
				val = (val + s) ^ s;

				fl = fs;
				fs = Ec_Laplace_Get_Freq1(fs, decay);

				// Search the decaying part of the PDF
				c_int i;
				for (i = 1; (fs > 0) && (i < val); i++)
				{
					fs *= 2;
					fl += fs + 2 * Laplace_MinP;
					fs = (c_uint)(fs * decay) >> 15;
				}

				// Everything beyond that has probability LAPLACE_MINP
				if (fs == 0)
				{
					c_int ndi_max = (c_int)(32768 - fl + Laplace_MinP - 1) >> Laplace_Log_MinP;
					ndi_max = (ndi_max - s) >> 1;
					c_int di = Arch.IMIN(val - i, ndi_max - 1);
					fl += (c_uint)(2 * di + 1 + s) * Laplace_MinP;
					fs = Arch.IMIN(Laplace_MinP, 32768 - fl);
					value = (i + di + s) ^ s;
				}
				else
				{
					fs += Laplace_MinP;
					fl += (c_uint)(fs & ~s);
				}
			}

			EntEnc.Ec_Encode_Bin(enc, fl, fl + fs, 15);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ec_Laplace_Decode(Ec_Dec dec, c_uint fs, c_int decay)
		{
			c_int val = 0;
			c_uint fm = EntDec.Ec_Decode_Bin(dec, 15);
			c_uint fl = 0;

			if (fm >= fs)
			{
				val++;
				fl = fs;
				fs = Ec_Laplace_Get_Freq1(fs, decay) + Laplace_MinP;

				// Search the decaying part of the PDF
				while ((fs > Laplace_MinP) && (fm >= (fl + 2 * fs)))
				{
					fs *= 2;
					fl += fs;
					fs = (c_uint)(((fs - 2 * Laplace_MinP) * decay) >> 15);
					fs += Laplace_MinP;
					val++;
				}

				// Everything beyond that has probability LAPLACE_MIN
				if (fs <= Laplace_MinP)
				{
					c_int di = (c_int)((fm - fl) >> (Laplace_Log_MinP + 1));
					val += di;
					fl += (c_uint)(2 * di * Laplace_MinP);
				}

				if (fm < (fl + fs))
					val = -val;
				else
					fl += fs;
			}

			EntDec.Ec_Dec_Update(dec, fl, Arch.IMIN(fl + fs, 32768), 32768);

			return val;
		}
	}
}

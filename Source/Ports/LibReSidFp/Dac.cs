/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Dac
	{
		// Estimate DAC nonlinearity.
		// The SID DACs are built up as R-2R ladder as follows:
		//
		//         n  n-1      2   1   0    VGND
		//         |   |       |   |   |      |   Termination
		//        2R  2R      2R  2R  2R     2R   only for
		//         |   |       |   |   |      |   MOS 8580
		//     Vo -o-R-o-R-...-o-R-o-R--    --+
		//
		//
		// All MOS 6581 DACs are missing a termination resistor at bit 0. This causes
		// pronounced errors for the lower 4 - 5 bits (e.g. the output for bit 0 is
		// actually equal to the output for bit 1), resulting in DAC discontinuities
		// for the lower bits.
		// In addition to this, the 6581 DACs exhibit further severe discontinuities
		// for higher bits, which may be explained by a less than perfect match between
		// the R and 2R resistors, or by output impedance in the NMOS transistors
		// providing the bit voltages. A good approximation of the actual DAC output is
		// achieved for 2R/R ~ 2.20.
		//
		// The MOS 8580 DACs, on the other hand, do not exhibit any discontinuities.
		// These DACs include the correct termination resistor, and also seem to have
		// very accurately matched R and 2R resistors (2R/R = 2.00).
		//
		// On the 6581 the output of the waveform and envelope DACs go through
		// a voltage follower built with two NMOS:
		//
		//             Vdd
		//
		//              |
		//            |-+
		// Vin -------|    T1 (enhancement-mode)
		//            |-+
		//              |
		//              o-------- Vout
		//              |
		//            |-+
		//        +---|    T2 (depletion-mode)
		//        |   |-+
		//        |     |
		//
		//       GND   GND

		private const double R_INFINITY = 1e6;

		/// <summary>
		/// DAC leakage
		///
		/// "Even in standard transistors a small amount of current leaks even when they are technically switched off."
		/// </summary>
		private double leakage;

		/// <summary>
		/// Analog values
		/// </summary>
		private readonly double[] dac;

		/// <summary>
		/// The dac array length
		/// </summary>
		private readonly uint dacLength;

		private readonly double MOSFET_LEAKAGE_6581 = 0.0075;
		private readonly double MOSFET_LEAKAGE_8580 = 0.0035;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Dac(uint bits)
		{
			dac = new double[bits];
			dacLength = bits;
		}



		/********************************************************************/
		/// <summary>
		/// Build DAC model for specific chip
		/// </summary>
		/********************************************************************/
		public void KinkedDac(ChipModel chipModel)
		{
			// Non-linearity parameter, 8580 DACs are perfectly linear
			double _2r_div_r = chipModel == ChipModel.MOS6581 ? 2.20 : 2.00;

			// 6581 DACs are not terminated by a 2R resistor
			bool term = chipModel == ChipModel.MOS8580;

			leakage = chipModel == ChipModel.MOS6581 ? MOSFET_LEAKAGE_6581 : MOSFET_LEAKAGE_8580;

			double vSum = 0.0;

			// Calculate voltage contribution by each individual bit in the R-2R ladder
			for (uint set_bit = 0; set_bit < dacLength; set_bit++)
			{
				double vn = 1.0;				// Normalized bit voltage
				double r = 1.0;					// Normalized R
				double _2r = _2r_div_r * r;		// 2R
				double rn = term ?				// Rn = 2R for correct termination,
							_2r : R_INFINITY;	// INFINITY for missing termination

				uint bit;

				// Calculate DAC "tail" resistance by repeated parallel substitution
				for (bit = 0; bit < set_bit; bit++)
				{
					rn = rn == R_INFINITY ?
							r + _2r :
							r + (_2r * rn) / (_2r + rn);	// R + 2R || Rn
				}

				// Source transformation for bit voltage
				if (rn == R_INFINITY)
					rn = _2r;
				else
				{
					rn = (_2r * rn) / (_2r + rn);	// 2R || Rn
					vn = vn * rn / _2r;
				}

				// Calculate DAC output voltage by repeated source transformation from
				// the "tail"
				for (++bit; bit < dacLength; bit++)
				{
					rn += r;
					double i = vn / rn;
					rn = (_2r * rn) / (_2r + rn);	// 2R || Rn
					vn = rn * i;
				}

				dac[set_bit] = vn;
				vSum += vn;
			}

			// Normalize to integerish behaviour
			for (uint i = 0; i < dacLength; i++)
				dac[i] /= vSum;
		}



		/********************************************************************/
		/// <summary>
		/// Get the Vo output for a given combination of input bits
		/// </summary>
		/********************************************************************/
		public double GetOutput(uint input, bool saturate = false)
		{
			double dacValue = 0.0;

			for (int i = 0; i < dacLength; i++)
			{
				bool transistor_on = (input & (1 << i)) != 0;
				dacValue += transistor_on ? dac[i] : dac[i] * leakage;
			}

			// Rough attempt at modeling the MDAC saturation for the 6581.
			// Things are actually more complex, the saturation is likely
			// caused by the two NMOS source followers, one at the output
			// of the waveform DAC and the second at the output of the MDAC.
			// The buffers are also supposed to introduce a DC offset.
			// As a first step we use a cubic model for saturation and
			// apply it only at the waveform output, providing a decent
			// result without any runtime overhead
			if (saturate)
			{
				const double GAIN = 1.1;
				const double SAT = 1.1;

				dacValue = GAIN * dacValue + (1.0 - GAIN) * SAT * dacValue * dacValue * dacValue;
			}

			return dacValue;
		}
	}
}

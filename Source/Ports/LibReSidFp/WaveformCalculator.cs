/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Array;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class WaveformCalculator
	{
		// Combined waveform calculator for WaveformGenerator.
		// By combining waveforms, the bits of each waveform are effectively short
		// circuited, a zero bit in one waveform will result in a zero output bit,
		// thus the infamous claim that the waveforms are AND'ed.
		// However, a zero bit in one waveform may also affect the neighboring bits
		// in the output.
		//
		// Example:
		//
		//                 1 1
		//     Bit #       1 0 9 8 7 6 5 4 3 2 1 0
		//                 -----------------------
		//     Sawtooth    0 0 0 1 1 1 1 1 1 0 0 0
		//
		//     Triangle    0 0 1 1 1 1 1 1 0 0 0 0
		//
		//     AND         0 0 0 1 1 1 1 1 0 0 0 0
		//
		//     Output      0 0 0 0 1 1 1 0 0 0 0 0
		//
		//
		// Re-vectorized die photographs reveal the mechanism behind this behavior.
		// Each waveform selector bit acts as a switch, which directly connects
		// internal outputs into the waveform DAC inputs as follows:
		//
		// - Noise outputs the shift register bits to DAC inputs as described above.
		//   Each output is also used as input to the next bit when the shift register
		//   is shifted. Lower four bits are grounded.
		// - Pulse connects a single line to all DAC inputs. The line is connected to
		//   either 5V (pulse on) or 0V (pulse off) at bit 11, and ends at bit 0.
		// - Triangle connects the upper 11 bits of the (MSB EOR'ed) accumulator to the
		//   DAC inputs, so that DAC bit 0 = 0, DAC bit n = accumulator bit n - 1.
		// - Sawtooth connects the upper 12 bits of the accumulator to the DAC inputs,
		//   so that DAC bit n = accumulator bit n. Sawtooth blocks out the MSB from
		//   the EOR used to generate the triangle waveform.
		//
		// We can thus draw the following conclusions:
		//
		// - The shift register may be written to by combined waveforms.
		// - The pulse waveform interconnects all bits in combined waveforms via the
		//   pulse line.
		// - The combination of triangle and sawtooth interconnects neighboring bits
		//   of the sawtooth waveform.
		//
		// Also in the 6581 the MSB of the oscillator, used as input for the
		// triangle xor logic and the pulse adder's last bit, is connected directly
		// to the waveform selector, while in the 8580 it is latched at sid_clk2
		// before being forwarded to the selector. Thus in the 6581 if the sawtooth MSB
		// is pulled down it might affect the oscillator's adder
		// driving the top bit low.

		/// <summary>
		/// Combined waveform model parameters
		/// </summary>
		private class CombinedWaveformConfig
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public CombinedWaveformConfig(distance_t distFunc, float threshold, float topBit, float pulseStrength, float distance1, float distance2)
			{
				this.distFunc = distFunc;
				this.threshold = threshold;
				this.topBit = topBit;
				this.pulseStrength = pulseStrength;
				this.distance1 = distance1;
				this.distance2 = distance2;
			}

			public readonly distance_t distFunc;
			public readonly float threshold;
			public readonly float topBit;
			public readonly float pulseStrength;
			public readonly float distance1;
			public readonly float distance2;
		}

		// Parameters derived with the Monte Carlo method based on
		// samplings from real machines.
		// Code and data available in the project repository [1].
		// Sampling program made by Dag Lem [2]
		//
		// The score here reported is the acoustic error
		// calculated XORing the estimated and the sampled values.
		// For the combinations including saw on 6581 only
		// the first half of the wave is considered.
		// In parentheses the number of mispredicted bits.
		//
		// [1] https://github.com/libsidplayfp/combined-waveforms
		// [2] https://github.com/daglem/reDIP-SID/blob/master/research/combsample.d64
		private static readonly CombinedWaveformConfig[][] configAverage = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R3 0486S sampled by Trurl
				// TS  error   406  (764/32768) [RMS: -13.55]
				new CombinedWaveformConfig(ExponentialDistance, 0.79111582f, 1.06053483f, 0.0f, 1.97922957f, 2.67848182f),
				// PT  error  4590  (124/32768) [RMS: -11.40dB]
				new CombinedWaveformConfig(LinearDistance, 0.941692829f, 1.0f, 1.80072665f, 0.033124879f, 0.232303441f),
				// PS  error   211 (1030/32768) [RMS: -10.31]
				new CombinedWaveformConfig(LinearDistance, 1.09394681f, 1.42332006f, 3.44251633f, 0.0797301158f, 0.102444135f),
				// PTS error    57  (278/32768) [RMS: -18.51]
				new CombinedWaveformConfig(LinearDistance, 1.53609717f, 0.0391017497f, 1.67601228f, 1.44580793f, 1.42448807f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 1088 sampled by reFX-Mike
				// TS  error 10660 (353/32768) [RMS: -12.85dB]
				new CombinedWaveformConfig(ExponentialDistance, 0.853578329f, 1.09615636f, 0.0f, 1.8819375f, 6.80794907f),
				// PT  error 10635 (289/32768) [RMS: -7.43dB]
				new CombinedWaveformConfig(ExponentialDistance,  0.929835618f, 1.0f, 1.12836814f, 1.10453653f, 1.48065746f),
				// PS  error 12255 (554/32768) [RMS: -7.97dB]
				new CombinedWaveformConfig(QuadraticDistance, 0.911938608f, 0.996440411f, 1.2278074f, 0.000117214302f, 0.18948476f),
				// PTS error  6913 (127/32768) [RMS: -13.23dB]
				new CombinedWaveformConfig(ExponentialDistance, 0.938004673f, 1.04827631f, 1.21178246f, 0.915959001f, 1.42698038f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static readonly CombinedWaveformConfig[][] configWeak = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R2 4383 sampled by ltx128
				// TS  error  169 (843/32768) [RMS: -14.61]
				new CombinedWaveformConfig(ExponentialDistance, 0.946056068f, 26.9527836f, 0.0f, 6.38644743f, 3.61852479f),
				// PT  error  612  (102/32768) [RMS: -15.35dB]
				new CombinedWaveformConfig(LinearDistance, 1.01262534f, 1.0f, 2.46070528f, 0.0537485816f, 0.0986242667f),
				// PS  error    5 (1535/32768) [RMS: -15.71]
				new CombinedWaveformConfig(LinearDistance, 0.760607481f, 0.0588437207f, 0.407531887f, 0.0994859114f, 0.000200334835f),
				// PTS error    0  (138/32768) [RMS: -25.46]
				new CombinedWaveformConfig(LinearDistance, 1.10582423f, 0.578988612f, 1.94850934f, 0.0783150643f, 0.300926387f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 4887 sampled by reFX-Mike
				// TS  error  741 (76/32768) [RMS: -13.56dB]
				new CombinedWaveformConfig(ExponentialDistance, 0.812351167f, 1.1727736f, 0.0f, 1.87459648f, 2.31578159f),
				// PT  error 7199 (192/32768) [RMS: -9.23]
				new CombinedWaveformConfig(ExponentialDistance,  0.917997837f, 1.0f, 1.01248944f, 1.05761552f, 1.37529826f),
				// PS  error 9849 (333/32768) [RMS: -9.45]
				new CombinedWaveformConfig(QuadraticDistance, 0.969898582f, 1.00785899f, 1.30233467f, 0.00962228701f, 0.146903187f),
				// PTS error 4809 (60/32768) [RMS: -15.03dB]
				new CombinedWaveformConfig(ExponentialDistance, 0.941834152f, 1.06401193f, 0.991132736f, 0.995310068f, 1.41105855f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static readonly CombinedWaveformConfig[][] configStrong = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R2 0384 sampled by Trurl
				// TS  error   754 (2056/32768) [RMS: -18.87]
				new CombinedWaveformConfig(ExponentialDistance, 0.714277208f, 0.00729158986f, 0.0f, 2.12244034f, 1.66707671f),
				// PT  error  5190  (238/32768) [RMS: -9.73dB]
				new CombinedWaveformConfig(LinearDistance, 0.924780309f, 1.0f, 1.96809769f, 0.0888123438f, 0.234606609f),
				// PS  error   860 (1288/32768) [RMS: -8.28]
				new CombinedWaveformConfig(LinearDistance, 1.02248156f, 1.36571658f, 3.66920304f, 0.00203792308f, 0.0826661736f),
				// PTS error    60  (411/32768) [RMS: -17.97]
				new CombinedWaveformConfig(LinearDistance, 0.891591191f, 1.88450027f, 1.33901846f, 0.134212971f, 0.293466389f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 1489 sampled by reFX-Mike
				// TS  error  4837 (388/32768) [RMS: -10.54dB]
				new CombinedWaveformConfig(ExponentialDistance, 0.89762634f, 56.7594185f, 0.0f, 7.68995237f, 12.0754194f),
				// PT  error  9242 (504/32768) [RMS: -6.03]
				new CombinedWaveformConfig(ExponentialDistance,  0.871706188f, 1.0f, 1.44852948f, 1.05926013f, 1.43830109f),
				// PS  error 13146 (713/32768) [RMS: -6.34]
				new CombinedWaveformConfig(QuadraticDistance, 0.892224431f, 1.22416508f, 1.74952936f, 0.0251259189f, 0.13089405f),
				// PTS error  6702 (300/32768) [RMS: -11.14dB]
				new CombinedWaveformConfig(LinearDistance, 0.91124934f, 0.963609755f, 0.909965038f, 1.07445884f, 1.82399702f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static WaveformCalculator instance = null;

		private readonly rc_matrix_t wfTable;
		private readonly Dictionary<CombinedWaveformConfig[], matrix_t> pulldownCache = new Dictionary<CombinedWaveformConfig[], Matrix<short>>();
		private readonly Lock pulldownCache_Lock = new Lock();

		private delegate float distance_t(float distance, int i);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private WaveformCalculator()
		{
			wfTable = new matrix_t(4, 4096);

			// Build waveform table
			for (uint idx = 0; idx < 4096; idx++)
			{
				int16_t saw = (int16_t)idx;
				int16_t tri = (int16_t)TriXor(idx);

				wfTable[0][idx] = 0xfff;
				wfTable[1][idx] = tri;
				wfTable[2][idx] = saw;
				wfTable[3][idx] = (int16_t)(saw & (saw << 1));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the singleton instance
		/// </summary>
		/********************************************************************/
		public static WaveformCalculator GetInstance()
		{
			if (instance == null)
				instance = new WaveformCalculator();

			return instance;
		}



		/********************************************************************/
		/// <summary>
		/// Get the waveform table for use by WaveformGenerator
		/// </summary>
		/********************************************************************/
		public rc_matrix_t GetWaveTable()
		{
			return wfTable;
		}



		/********************************************************************/
		/// <summary>
		/// Build pulldown table for use by WaveformGenerator
		/// </summary>
		/********************************************************************/
		public rc_matrix_t BuildPulldownTable(ChipModel model, CombinedWaveforms cws)
		{
			lock (pulldownCache_Lock)
			{
				int modelIdx = model == ChipModel.MOS6581 ? 0 : 1;
				CombinedWaveformConfig[] cfgArray;

				switch (cws)
				{
					default:
					case CombinedWaveforms.AVERAGE:
					{
						cfgArray = configAverage[modelIdx];
						break;
					}

					case CombinedWaveforms.WEAK:
					{
						cfgArray = configWeak[modelIdx];
						break;
					}

					case CombinedWaveforms.STRONG:
					{
						cfgArray = configStrong[modelIdx];
						break;
					}
				}

				if (pulldownCache.TryGetValue(cfgArray, out matrix_t pdTable))
					return pdTable;

				pdTable = new matrix_t(5, 4096);

				for (int wav = 0; wav < 5; wav++)
				{
					CombinedWaveformConfig cfg = cfgArray[wav];

					distance_t distFunc = cfg.distFunc;

					float[] distanceTable = new float[12 * 2 + 1];
					distanceTable[12] = 1.0f;

					for (int i = 12; i > 0; i--)
					{
						distanceTable[12 - i] = distFunc(cfg.distance1, i);
						distanceTable[12 + i] = distFunc(cfg.distance2, i);
					}

					for (uint idx = 0; idx < 4096;  idx++)
						pdTable[(uint)wav][idx] = CalculatePulldown(distanceTable, cfg.topBit, cfg.pulseStrength, cfg.threshold, idx);
				}

				pulldownCache[cfgArray] = pdTable;

				return wfTable;
			}
		}

		#region Distance methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static float ExponentialDistance(float distance, int i)
		{
			return (float)Math.Pow(distance, -i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static float LinearDistance(float distance, int i)
		{
			return 1.0f / (1.0f + i * distance);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static float QuadraticDistance(float distance, int i)
		{
			return 1.0f / (1.0f + (i * i) * distance);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate triangle waveform
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint TriXor(uint val)
		{
			return (((val & 0x800) == 0) ? val : (val ^ 0xfff)) << 1;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Generate bitstate based on emulation of combined waves pulldown
		/// </summary>
		/********************************************************************/
		private int16_t CalculatePulldown(float[] distanceTable, float topBit, float pulseStrength, float threshold, uint wave)
		{
			float[] bit = new float[12];

			for (int i = 0; i < 12; i++)
				bit[i] = (wave & (1U << i)) != 0 ? 1.0f : 0.0f;

			bit[11] *= topBit;

			float[] pulldown = new float[12];

			for (int sb = 0; sb < 12; sb++)
			{
				float avg = 0.0f;
				float n = 0.0f;

				for (int cb = 0; cb < 12; cb++)
				{
					if (cb == sb)
						continue;

					float weight = distanceTable[sb - cb + 12];
					avg += (1.0f - bit[cb]) * weight;
					n += weight;
				}

				avg -= pulseStrength;

				pulldown[sb] = avg / n;
			}

			// Get the predicted value
			int16_t value = 0;

			for (int i = 0; i < 12; i++)
			{
				float bitValue = bit[i] > 0.0f ? 1.0f - pulldown[i] : 0.0f;
				if (bitValue > threshold)
					value |= (int16_t)(1U << i);
			}

			return value;
		}
		#endregion
	}
}

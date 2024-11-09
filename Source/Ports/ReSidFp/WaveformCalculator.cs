/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Array;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
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
		// In parentheses the number of mispredicted bits.
		//
		// [1] https://github.com/libsidplayfp/combined-waveforms
		// [2] https://github.com/daglem/reDIP-SID/blob/master/research/combsample.d64
		private static readonly CombinedWaveformConfig[][] configAverage = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R3 0486S sampled by Trurl
				// TS  error  3555 (324/32768) [RMS: 73.98]
				new CombinedWaveformConfig(ExponentialDistance, 0.877322257f, 1.11349654f, 0.0f, 2.14537621f, 9.08618164f),
				// PT  error  4590 (124/32768) [RMS: 68.90]
				new CombinedWaveformConfig(LinearDistance, 0.941692829f, 1.0f, 1.80072665f, 0.033124879f, 0.232303441f),
				// PS  error 19352 (763/32768) [RMS: 96.91]
				new CombinedWaveformConfig(LinearDistance, 2.20329857f, 1.04501438f, 10.5146885f, 0.277294368f, 0.143747061f),
				// PTS error  5068 ( 94/32768) [RMS: 41.69]
				new CombinedWaveformConfig(LinearDistance, 1.09762526f, 0.975265801f, 1.52196741f, 0.151528224f, 0.841949463f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 1088 sampled by reFX-Mike
				// TS  error 10788 (354/32768) [RMS: 58.31]
				new CombinedWaveformConfig(ExponentialDistance, 0.841851234f, 1.09233654f, 0.0f, 1.85262764f, 6.22224379f),
				// PT  error 10635 (289/32768) [RMS: 108.81]
				new CombinedWaveformConfig(ExponentialDistance,  0.929835618f, 1.0f, 1.12836814f, 1.10453653f, 1.48065746f),
				// PS  error 12255 (554/32768) [RMS: 102.27]
				new CombinedWaveformConfig(QuadraticDistance, 0.911938608f, 0.996440411f, 1.2278074f, 0.000117214302f, 0.18948476f),
				// PTS error  6995 (139/32768) [RMS: 55.78]
				new CombinedWaveformConfig(ExponentialDistance, 0.932317019f, 1.03892183f, 1.2068342f, 0.891974986f, 1.42451835f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static readonly CombinedWaveformConfig[][] configWeak = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R2 4383 sampled by ltx128
				// TS  error 1858 (204/32768) [RMS: 62.49]
				new CombinedWaveformConfig(ExponentialDistance, 0.886832297f, 1.0f, 0.0f, 2.14438701f, 9.51839447f),
				// PT  error  612 (102/32768) [RMS: 43.71]
				new CombinedWaveformConfig(LinearDistance, 1.01262534f, 1.0f, 2.46070528f, 0.0537485816f, 0.0986242667f),
				// PS  error 8135 (575/32768) [RMS: 75.10]
				new CombinedWaveformConfig(LinearDistance, 2.14896345f, 1.0216713f, 10.5400085f, 0.244498149f, 0.126134038f),
				// PTS error 2505 (63/32768) [RMS: 24.37]
				new CombinedWaveformConfig(LinearDistance, 1.29061747f, 0.9754318f, 3.15377498f, 0.0968349651f, 0.318573922f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 4887 sampled by reFX-Mike
				// TS  error  745 (77/32768) [RMS: 53.74]
				new CombinedWaveformConfig(ExponentialDistance, 0.816124022f, 1.31208789f, 0.0f, 1.92347884f, 2.35027933f),
				// PT  error 7199 (192/32768) [RMS: 88.43]
				new CombinedWaveformConfig(ExponentialDistance,  0.917997837f, 1.0f, 1.01248944f, 1.05761552f, 1.37529826f),
				// PS  error 9864 (333/32768) [RMS: 86.29]
				new CombinedWaveformConfig(QuadraticDistance, 0.970038712f, 1.00844693f, 1.30298805f, 0.0097996993f, 0.146854922f),
				// PTS error 4809 (60/32768) [RMS: 45.37]
				new CombinedWaveformConfig(ExponentialDistance, 0.941834152f, 1.06401193f, 0.991132736f, 0.995310068f, 1.41105855f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static readonly CombinedWaveformConfig[][] configStrong = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R2 0384 sampled by Trurl
				// TS  error 20337 (1579/32768) [RMS: 88.57]
				new CombinedWaveformConfig(ExponentialDistance, 0.000637792516f, 1.56725872f, 0.0f, 0.00036806846f, 1.51800942f),
				// PT  error  5194 (240/32768) [RMS: 83.54]
				new CombinedWaveformConfig(LinearDistance, 0.924824238f, 1.0f, 1.96749473f, 0.0891806409f, 0.234794483f),
				// PS  error 31015 (2181/32768) [RMS: 114.99]
				new CombinedWaveformConfig(LinearDistance, 1.2328074f, 0.73079139f, 3.9719491f, 0.00156516861f, 0.314677745f),
				// PTS error  9874 (201/32768) [RMS: 52.30]
				new CombinedWaveformConfig(LinearDistance, 1.08558261f, 0.857638359f, 1.52781796f, 0.152927235f, 1.02657032f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 1489 sampled by reFX-Mike
				// TS  error  4837 (388/32768) [RMS: 76.07]
				new CombinedWaveformConfig(ExponentialDistance, 0.89762634f, 56.7594185f, 0.0f, 7.68995237f, 12.0754194f),
				// PT  error  9298 (506/32768) [RMS: 128.15]
				new CombinedWaveformConfig(ExponentialDistance,  0.867885351f, 1.0f, 1.4511894f, 1.07057536f, 1.43333757f),
				// PS  error 13168 (718/32768) [RMS: 123.35]
				new CombinedWaveformConfig(QuadraticDistance, 0.89255774f, 1.2253896f, 1.75615835f, 0.0245045591f, 0.12982437f),
				// PTS error  6702 (300/32768) [RMS: 71.01]
				new CombinedWaveformConfig(LinearDistance, 0.91124934f, 0.963609755f, 0.909965038f, 1.07445884f, 1.82399702f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.95f, 1.0f, 1.15f, 1.0f, 1.45f),
			}
		};

		private static WaveformCalculator instance = null;

		private readonly matrix_t wfTable;
		private readonly Dictionary<CombinedWaveformConfig[], matrix_t> pulldownCache = new Dictionary<CombinedWaveformConfig[], Matrix<short>>();
		private readonly object pulldownCache_Lock = new object();

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
			for (uint idx = 0; idx < (1U << 12); idx++)
			{
				short saw = (short)idx;
				short tri = (short)TriXor(idx);

				wfTable[0][idx] = 0xfff;
				wfTable[1][idx] = tri;
				wfTable[2][idx] = saw;
				wfTable[3][idx] = (short)(saw & (saw << 1));
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
		public matrix_t GetWaveTable()
		{
			return wfTable;
		}



		/********************************************************************/
		/// <summary>
		/// Build pulldown table for use by WaveformGenerator
		/// </summary>
		/********************************************************************/
		public matrix_t BuildPulldownTable(ChipModel model, CombinedWaveforms cws)
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

					for (uint idx = 0; idx < (1U << 12);  idx++)
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
		private short CalculatePulldown(float[] distanceTable, float topBit, float pulseStrength, float threshold, uint accumulator)
		{
			float[] bit = new float[12];

			for (int i = 0; i < 12; i++)
				bit[i] = (accumulator & (1U << i)) != 0 ? 1.0f : 0.0f;

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
			short value = 0;

			for (int i = 0; i < 12; i++)
			{
				float bitValue = bit[i] > 0.0f ? 1.0f - pulldown[i] : 0.0f;
				if (bitValue > threshold)
					value |= (short)(1U << i);
			}

			return value;
		}
		#endregion
	}
}

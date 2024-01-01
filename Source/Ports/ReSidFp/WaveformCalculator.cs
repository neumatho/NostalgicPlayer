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
		private static readonly CombinedWaveformConfig[][] config = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // 6581 R3 4785 sampled by Trurl
				// TS  error 2298 (339/32768)
				new CombinedWaveformConfig(ExponentialDistance, 0.776678205f, 1.18439901f, 0.0f, 2.25732255f, 5.12803745f),
				// PT  error  582 (57/32768)
				new CombinedWaveformConfig(LinearDistance, 1.01866758f, 1.0f, 2.69177628f, 0.0233543925f, 0.0850229636f),
				// PS  error 9242 (679/32768)
				new CombinedWaveformConfig(LinearDistance, 2.20329857f, 1.04501438f, 10.5146885f, 0.277294368f, 0.143747061f),
				// PTS error 2799 (71/32768)
				new CombinedWaveformConfig(LinearDistance, 1.35652959f, 1.09051275f, 3.21098137f, 0.16658926f, 0.370252877f),
				// NP  guessed
				new CombinedWaveformConfig(ExponentialDistance, 0.96f, 1.0f, 2.5f, 1.1f, 1.2f),
			},
			new CombinedWaveformConfig[5]
			{ // 8580 R5 5092 25 sampled by reFX-Mike
				// TS  error 1212 (183/32768)
				new CombinedWaveformConfig(ExponentialDistance, 0.684999049f, 0.916620493f, 0.0f, 1.14715648f, 2.02339816f),
				// PT  error 6153 (295/32768)
				new CombinedWaveformConfig(ExponentialDistance,  0.940367579f, 1.0f, 1.26695442f, 0.976729453f, 1.57954705f),
				// PS  error 7620 (454/32768)
				new CombinedWaveformConfig(QuadraticDistance, 0.963866293f, 1.22095084f, 1.01380754f, 0.0110885892f, 0.381492466f),
				// PTS error 3701 (117/32768)
				new CombinedWaveformConfig(LinearDistance, 0.976761818f, 0.202727556f, 0.988633931f, 0.939373314f, 9.37139416f),
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
		public matrix_t BuildPulldownTable(ChipModel model)
		{
			lock (pulldownCache_Lock)
			{
				CombinedWaveformConfig[] cfgArray = config[model == ChipModel.MOS6581 ? 0 : 1];

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
			byte[] bit = new byte[12];

			for (int i = 0; i < 12; i++)
				bit[i] = (byte)((accumulator & (1U << i)) != 0 ? 1 : 0);

			bit[11] = (byte)(bit[11] * topBit);

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
					avg += (1 - bit[cb]) * weight;
					n += weight;
				}

				avg -= pulseStrength;

				pulldown[sb] = avg / n;
			}

			// Get the predicted value
			short value = 0;

			for (int i = 0; i < 12; i++)
			{
				float bitValue = bit[i] != 0 ? 1.0f - pulldown[i] : 0.0f;
				if (bitValue > threshold)
					value |= (short)(1U << i);
			}

			return value;
		}
		#endregion
	}
}

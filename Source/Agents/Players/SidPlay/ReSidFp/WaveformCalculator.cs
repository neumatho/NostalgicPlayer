/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Array;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class WaveformCalculator
	{
		// Combined waveform calculator for WaveformGenerator.
		// By combining waveforms, the bits of each waveform are effectively short
		// circuited. A zero bit in one waveform will result in a zero output bit
		// (thus the infamous claim that the waveforms are AND'ed).
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
			public CombinedWaveformConfig(float threshold, float pulseStrength, float distance1, float distance2)
			{
				this.threshold = threshold;
				this.pulseStrength = pulseStrength;
				this.distance1 = distance1;
				this.distance2 = distance2;
			}

			public readonly float threshold;
			public readonly float pulseStrength;
			public readonly float distance1;
			public readonly float distance2;
		}

		// Parameters derived with the Monte Carlo method based on
		// samplings by kevtris. Code and data available in the project repository [1].
		//
		// The score here reported is the acoustic error
		// calculated XORing the estimated and the sampled values.
		// In parentheses the number of mispredicted bits.
		//
		// [1] https://github.com/libsidplayfp/combined-waveforms
		private static readonly CombinedWaveformConfig[][] config = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[5]
			{ // kevtris chip G (6581 R2)
				new CombinedWaveformConfig(0.862147212f, 0.0f,         10.8962431f,    2.50848103f),		// TS  error  1941 (327/28672)
				new CombinedWaveformConfig(0.932746708f, 2.07508397f,   1.03668225f,   1.14876997f),		// PT  error  5992 (126/32768)
				new CombinedWaveformConfig(0.860927045f, 2.43506575f,   0.908603609f,  1.07907593f),		// PS  error  3693 (521/28672)
				new CombinedWaveformConfig(0.741343081f, 0.0452554375f, 1.1439606f,    1.05711341f),		// PTS error   338 ( 29/28672)
				new CombinedWaveformConfig(0.96f,        2.5f,          1.1f,          1.2f        )		// NP  guessed
			},
			new CombinedWaveformConfig[5]
			{ // kevtris chip V (8580 R5)
				new CombinedWaveformConfig(0.715788841f, 0.0f,          1.32999945f,   2.2172699f  ),		// TS  error   928 (135/32768)
				new CombinedWaveformConfig(0.93500334f,  1.05977178f,   1.08629429f,   1.43518543f ),		// PT  error  7991 (212/32768)
				new CombinedWaveformConfig(0.920648575f, 0.943601072f,  1.13034654f,   1.41881108f ),		// PS  error 12566 (394/32768)
				new CombinedWaveformConfig(0.90921098f,  0.979807794f,  0.942194462f,  1.40958893f ),		// PTS error  2092 ( 60/32768)
				new CombinedWaveformConfig(0.95f,        1.15f,         1.0f,          1.45f       )		// NP  guessed
			}
		};

		private static WaveformCalculator instance = null;

		private readonly matrix_t wfTable;
		private readonly Dictionary<CombinedWaveformConfig[], matrix_t> pulldownCache = new Dictionary<CombinedWaveformConfig[], Matrix<short>>();

		delegate float distance_t(float distance, int i);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private WaveformCalculator()
		{
			wfTable = new matrix_t(4, 4096);

			BuildWaveTable();
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
			CombinedWaveformConfig[] cfgArray = config[model == ChipModel.MOS6581 ? 0 : 1];

			if (pulldownCache.TryGetValue(cfgArray, out matrix_t pdTable))
				return pdTable;

			pdTable = new matrix_t(5, 4096);

			for (int wav = 0; wav < 5; wav++)
			{
				CombinedWaveformConfig cfg = cfgArray[wav];

				distance_t distFunc = ExponentialDistance;

				float[] distanceTable = new float[12 * 2 + 1];
				distanceTable[12] = 1.0f;

				for (int i = 12; i > 0; i--)
				{
					distanceTable[12 - i] = distFunc(cfg.distance1, i);
					distanceTable[12 + i] = distFunc(cfg.distance2, i);
				}

				for (uint idx = 0; idx < (1U << 12);  idx++)
					pdTable[(uint)wav][idx] = CalculatePulldown(distanceTable, cfg.pulseStrength, cfg.threshold, idx);
			}

			pulldownCache[cfgArray] = pdTable;

			return wfTable;
		}

		#region Distance methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private float ExponentialDistance(float distance, int i)
		{
			return (float)Math.Pow(distance, -i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private float LinearDistance(float distance, int i)
		{
			return 1.0f / (1.0f + i * distance);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private float QuadraticDistance(float distance, int i)
		{
			return 1.0f / (1.0f + (i * i) * distance);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate triangle waveform
		/// </summary>
		/********************************************************************/
		private uint TriXor(uint val)
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
		private short CalculatePulldown(float[] distanceTable, float pulseStrength, float threshold, uint accumulator)
		{
			byte[] bit = new byte[12];

			for (int i = 0; i < 12; i++)
				bit[i] = (byte)((accumulator & (1U << i)) != 0 ? 1 : 0);

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



		/********************************************************************/
		/// <summary>
		/// Build waveform table
		/// </summary>
		/********************************************************************/
		private void BuildWaveTable()
		{
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
		#endregion
	}
}

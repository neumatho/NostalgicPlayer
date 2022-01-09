/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
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
			public CombinedWaveformConfig(float bias, float pulseStrength, float topBit, float distance1, float distance2, float stMix)
			{
				this.bias = bias;
				this.pulseStrength = pulseStrength;
				this.topBit = topBit;
				this.distance1 = distance1;
				this.distance2 = distance2;
				this.stMix = stMix;
			}

			public readonly float bias;
			public readonly float pulseStrength;
			public readonly float topBit;
			public readonly float distance1;
			public readonly float distance2;
			public readonly float stMix;
		}

		// Parameters derived with the Monte Carlo method based on
		// samplings by kevtris. Code and data available in the project repository [1].
		//
		// The score here reported is the acoustic error
		// calculated XORing the estimated and the sampled values.
		// In parentheses the number of mispredicted bits
		// on a total of 32768.
		//
		// [1] https://github.com/libsidplayfp/combined-waveforms
		private static readonly CombinedWaveformConfig[][] config = new CombinedWaveformConfig[2][]
		{
			new CombinedWaveformConfig[4]
			{ // kevtris chip G (6581 R2)
				new CombinedWaveformConfig(0.90251f, 0.0f,       0.0f,       1.9147f,    1.6747f,  0.62376f),	// error  1689 (280)
				new CombinedWaveformConfig(0.93088f, 2.4843f,    0.0f,       1.0353f,    1.1484f,  0.0f    ),	// error  6128 (130)
				new CombinedWaveformConfig(0.90988f, 2.26303f,   1.13126f,   1.0035f,    1.13801f, 0.0f    ),	// error 14243 (632)
				new CombinedWaveformConfig(0.91f,    1.192f,     0.0f,       1.0169f,    1.2f,     0.637f  )	// error    64   (2)
			},
			new CombinedWaveformConfig[4]
			{ // kevtris chip V (8580 R5)
				new CombinedWaveformConfig(0.9632f,   0.0f,      0.975f,    1.7467f,    2.36132f, 0.975395f),	// error  1380 (169)
				new CombinedWaveformConfig(0.92886f,  1.67696f,  0.0f,      1.1014f,    1.4352f,  0.0f     ),	// error  8007 (218)
				new CombinedWaveformConfig(0.94043f,  1.7937f,   0.981f,    1.1213f,    1.4259f,  0.0f     ),	// error 11957 (362)
				new CombinedWaveformConfig(0.96211f,  0.98695f,  1.00387f,  1.46499f,   1.98375f, 0.77777f )	// error  2369  (89)
			}
		};

		private static WaveformCalculator instance = null;

		private readonly Dictionary<CombinedWaveformConfig[], matrix_t> cache = new Dictionary<CombinedWaveformConfig[], Matrix<short>>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private WaveformCalculator()
		{
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
		/// Build waveform tables for use by WaveformGenerator
		/// </summary>
		/********************************************************************/
		public matrix_t BuildTable(ChipModel model)
		{
			CombinedWaveformConfig[] cfgArray = config[model == ChipModel.MOS6581 ? 0 : 1];

			if (cache.TryGetValue(cfgArray, out matrix_t wfTable))
				return wfTable;

			wfTable = new matrix_t(8, 4096);

			for (int idx = 0; idx < 1 << 12; idx++)
			{
				wfTable[0][idx] = 0xfff;
				wfTable[1][idx] = (short)((idx & 0x800) == 0 ? idx << 1 : (idx ^ 0xfff) << 1);
				wfTable[2][idx] = (short)idx;
				wfTable[3][idx] = CalculateCombinedWaveform(cfgArray[0], 3, idx);
				wfTable[4][idx] = 0xfff;
				wfTable[5][idx] = CalculateCombinedWaveform(cfgArray[1], 5, idx);
				wfTable[6][idx] = CalculateCombinedWaveform(cfgArray[2], 6, idx);
				wfTable[7][idx] = CalculateCombinedWaveform(cfgArray[3], 7, idx);
			}

			cache[cfgArray] = wfTable;

			return wfTable;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Generate bitstate based on emulation of combined waves
		/// </summary>
		/********************************************************************/
		private short CalculateCombinedWaveform(CombinedWaveformConfig config, int waveform, int accumulator)
		{
			float[] o = new float[12];

			// Saw
			for (int i = 0; i < 12; i++)
				o[i] = (accumulator & (1 << i)) != 0 ? 1.0f : 0.0f;

			// Convert to Triangle
			if ((waveform & 3) == 1)
			{
				bool top = (accumulator & 0x800) != 0;

				for (int i = 11; i > 0; i--)
					o[i] = top ? 1.0f - o[i - 1] : o[i - 1];

				o[0] = 0.0f;
			}
			else if ((waveform & 3) == 3)	// or to Saw+Triangle
			{
				// Bottom bit is grounded via T waveform selector
				o[0] *= config.stMix;

				for (int i = 1; i < 12; i++)
				{
					// Enabling the S waveform pulls the XOR circuit selector transistor down
					// (which would normally make the descending ramp of the triangle waveform),
					// so ST does not actually have a sawtooth and triangle waveform combined,
					// but merely combines two sawtooths, one rising double the speed the other.
					//
					// http://www.lemon64.com/forum/viewtopic.php?t=25442&postdays=0&postorder=asc&start=165
					o[i] = o[i - 1] * (1.0f - config.stMix) + o[i] * config.stMix;
				}
			}

			// Topbit for Saw
			if ((waveform & 2) == 2)
				o[11] *= config.topBit;

			// ST, P* waveforms
			if ((waveform == 3) || (waveform > 4))
			{
				float[] distanceTable = new float[12 * 2 + 1];
				distanceTable[12] = 1.0f;

				for (int i = 12; i > 0; i--)
				{
					distanceTable[12 - i] = (float)(1.0f / Math.Pow(config.distance1, i));
					distanceTable[12 + i] = (float)(1.0f / Math.Pow(config.distance2, i));
				}

				float[] tmp = new float[12];

				for (int i = 0; i < 12; i++)
				{
					float avg = 0.0f;
					float n = 0.0f;

					for (int j = 0; j < 12; j++)
					{
						float weight = distanceTable[i - j + 12];
						avg += o[j] * weight;
						n += weight;
					}

					// Pulse control bit
					if (waveform > 4)
					{
						float weight = distanceTable[i - 12 + 12];
						avg += config.pulseStrength * weight;
						n += weight;
					}

					tmp[i] = (o[i] + avg / n) * 0.5f;
				}

				for (int i = 0; i < 12; i++)
					o[i] = tmp[i];
			}

			short value = 0;

			for (int i = 0; i < 12; i++)
			{
				if (o[i] > config.bias)
					value |= (short)(1 << i);
			}

			return value;
		}
		#endregion
	}
}

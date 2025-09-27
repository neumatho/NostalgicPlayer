/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	internal abstract class FilterModelConfig
	{
		/// <summary>
		/// The highpass summer has 2 - 6 inputs (bandpass, lowpass, and 0 - 4 voices)
		/// </summary>
		public static class Summer_Offset
		{
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static int Value(int i)
			{
				if (i == 0)
					return 0;

				return Value(i - 1) + ((2 + i - 1) << 16);
			}
		}

		/// <summary>
		/// The mixer has 0 - 7 inputs (0 - 4 voices and 0 - 3 filter outputs)
		/// </summary>
		public static class Mixer_Offset
		{
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public static int Value(int i)
			{
				if (i == 0)
					return 0;

				if (i == 1)
					return 1;

				return Value(i - 1) + ((i - 1) << 16);
			}
		}

		/// <summary>
		/// Hack to add quick dither when converting values from float to int
		/// and avoid quantization noise.
		/// Hopefully this can be removed the day we move all the analog part
		/// processing to floats.
		///
		/// Not sure about the effect of using such small buffer of numbers
		/// since the random sequence repeats every 1024 values but for
		/// now it seems to do the job
		/// </summary>
		private class RandomNoise
		{
			private double[] buffer = new double[1024];
			private int index = 0;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public RandomNoise()
			{
				for (int i = 0; i < 1024; i++)
					buffer[i] = Random.Shared.NextDouble();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public double GetNoise()
			{
				index = (index + 1) & 0x3ff;

				return buffer[index];
			}
		}

		// Capacitor value
		protected readonly double c;

		// Transistor parameters
		protected const double Ut = 26.0e-3;

		private readonly double vdd;			// Positive supply voltage
		private readonly double vth;			// Threshold voltage
		protected readonly double vddt;			// Vdd - Vth;
		protected double uCox;					// Transconductance coefficient: u*Cox

		// Derived stuff
		protected readonly double vMin, vMax;
		protected readonly double denorm, norm;

		// Fixed point scaling for 16 bit op-amp output
		protected readonly double n16;

		private readonly double voice_voltage_range;

		// Current factor coefficient for op-amp integrators
		private double currFactorCoeff;

		// Lookup tables for gain and summer op-amps in output stage / filter
		protected readonly ushort[] mixer;			// This is initialized in the derived class constructor
		protected readonly ushort[] summer;			// This is initialized in the derived class constructor
		protected readonly ushort[] volume;			// This is initialized in the derived class constructor
		protected readonly ushort[] resonance;		// This is initialized in the derived class constructor

		// Reverse op-amp transfer function
		protected readonly ushort[] opamp_rev = new ushort[1 << 16];	// This is initialized in the derived class constructor

		private RandomNoise rnd = new RandomNoise();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FilterModelConfig(double vvr, double c, double vdd, double vth, double uCox, Spline.Point[] opamp_voltage, uint opamp_size)
		{
			this.c = c;
			this.vdd = vdd;
			this.vth = vth;
			vddt = vdd - vth;
			vMin = opamp_voltage[0].x;
			vMax = Math.Max(vddt, opamp_voltage[0].y);
			denorm = vMax - vMin;
			norm = 1.0 / denorm;
			n16 = norm * UInt16.MaxValue;
			currFactorCoeff = denorm * (uCox / 2.0 * 1.0e-6 / c);
			voice_voltage_range = vvr;
			mixer = new ushort[Mixer_Offset.Value(8)];
			summer = new ushort[Summer_Offset.Value(5)];
			volume = new ushort[16 * (1 << 16)];
			resonance = new ushort[16 * (1 << 16)];

			SetUCox(uCox);

			// Convert op-amp voltage transfer to 16 bit values
			List<Spline.Point> scaled_voltage = new List<Spline.Point>((int)opamp_size);

			for (int i = 0; i < opamp_size; i++)
			{
				scaled_voltage.Add(new Spline.Point(
					(n16 * (opamp_voltage[i].x - opamp_voltage[i].y) / 2.0f
					// We add 32768 to get a positive number in the range [0-65535]
					+ (1U << 15)),
					n16 * (opamp_voltage[i].x - vMin))
				);
			}

			// Create lookup table mapping capacitor voltage to op amp input voltage
			Spline s = new Spline(scaled_voltage);

			for (int x = 0; x < (1 << 16); x++)
			{
				Spline.Point @out = s.Evaluate(x);

				// When interpolating outside range the first element may be negative
				opamp_rev[x] = (ushort)(@out.x > 0.0f ? To_UShort(@out.x) : 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[] GetVolume()
		{
			return volume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[] GetResonance()
		{
			return resonance;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[] GetSummer()
		{
			return summer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[] GetMixer()
		{
			return mixer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetOpampRev(int i)
		{
			return opamp_rev[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetVddt()
		{
			return vddt;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetVth()
		{
			return vth;
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ushort To_UShort_Dither(double x, double d_Noise)
		{
			int tmp = (int)(x + d_Noise);
			return (ushort)tmp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ushort To_UShort(double x)
		{
			return To_UShort_Dither(x, 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNormalizedValue(double value)
		{
			return To_UShort_Dither(n16 * (value - vMin), rnd.GetNoise());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNormalizedCurrentFactor(int N, double wl)
		{
			return To_UShort((1 << N) * currFactorCoeff * wl);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNVMin()
		{
			return To_UShort(n16 * vMin);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetNormalizedVoice(float value, uint env)
		{
			return GetNormalizedValue(GetVoiceVoltage(value, env));
		}



		/********************************************************************/
		/// <summary>
		/// The filter summer operates at n ~ 1, and has 5 fundamentally
		/// different input configurations (2 - 6 input "resistors")
		///
		/// Note that all "on" transistors are modeled as one. This is not
		/// entirely accurate, since the input for each transistor is
		/// different, and transistors are not linear components. However
		/// modeling all transistors separately would be extremely costly
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void BuildSummerTable(OpAmp opAmpModel)
		{
			double r_N16 = 1.0 / n16;

			int idx = 0;
			for (int i = 0; i < 5; i++)
			{
				int iDiv = 2 + i;		// 2 - 6 input "resistors"
				int size = iDiv << 16;
				double n = iDiv;
				double r_iDiv = 1.0 / iDiv;

				opAmpModel.Reset();

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16 * r_iDiv;	// vMin .. vMax
					summer[idx++] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// The audio mixer operates at n ~ 8/6 (6581) or 8/5 (8580), and has
		/// 8 fundamentally different input configurations (0 - 7 input
		/// "resistors").
		///
		/// All "on", transistors are modeled as one - see comments above
		/// for the filter summer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void BuildMixerTable(OpAmp opAmpModel, double nRatio)
		{
			double r_N16 = 1.0 / n16;

			int idx = 0;
			for (int i = 0; i < 8; i++)
			{
				int iDiv = i == 0 ? 1 : i;
				int size = i == 0 ? 1 : i << 16;
				double n = i * nRatio;
				double r_iDiv = 1.0 / iDiv;

				opAmpModel.Reset();

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16 * r_iDiv;	// vMin .. vMax
					mixer[idx++] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 4 bit "resistor" ladders in the audio output gain necessitate 16
		/// gain tables. From die photographs of the volume "resistor"
		/// ladders it follows that gain ~ vol/12 (6581) or vol/16 (8580)
		/// (assuming ideal op-amps and ideal "resistors")
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void BuildVolumeTable(OpAmp opAmpModel, double nDivisor)
		{
			double r_N16 = 1.0 / n16;

			int idx = 0;
			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;
				double n = n8 / nDivisor;

				opAmpModel.Reset();

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16;			// vMin .. vMax
					volume[idx++] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 4 bit "resistor" ladders in the bandpass resonance gain
		/// necessitate 16 gain tables. From die photographs of the bandpass
		/// "resistor" ladders it follows that 1/Q ~ ~res/8 (6581) or
		/// 2^((4 - res)/8) (8580)
		/// (assuming ideal op-amps and ideal "resistors")
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void BuildResonanceTable(OpAmp opAmpModel, double[] resonance_n)
		{
			double r_N16 = 1.0 / n16;

			int idx = 0;
			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;

				opAmpModel.Reset();

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16;			// vMin .. vMax
					resonance[idx++] = GetNormalizedValue(opAmpModel.Solve(resonance_n[n8], vIn));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void SetUCox(double new_uCox)
		{
			uCox = new_uCox;
			currFactorCoeff = denorm * (uCox / 2.0 * 1.0e-6 / c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract double GetVoiceDc(uint env);
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetVoiceVoltage(float value, uint env)
		{
			return value * voice_voltage_range + GetVoiceDc(env);
		}
		#endregion
	}
}

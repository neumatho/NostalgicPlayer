/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	internal abstract class FilterModelConfig
	{
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
		private readonly double voice_dc_voltage;

		// Current factor coefficient for op-amp integrators
		private double currFactorCoeff;

		// Lookup tables for gain and summer op-amps in output stage / filter
		protected readonly ushort[][] mixer = new ushort[8][];			// This is initialized in the derived class constructor
		protected readonly ushort[][] summer = new ushort[5][];			// This is initialized in the derived class constructor
		protected readonly ushort[][] volume = new ushort[16][];		// This is initialized in the derived class constructor
		protected readonly ushort[][] resonance = new ushort[16][];		// This is initialized in the derived class constructor

		// Reverse op-amp transfer function
		protected readonly ushort[] opamp_rev = new ushort[1 << 16];	// This is initialized in the derived class constructor

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FilterModelConfig(double vvr, double vdv, double c, double vdd, double vth, double uCox, Spline.Point[] opamp_voltage, uint opamp_size)
		{
			this.c = c;
			this.vdd = vdd;
			this.vth = vth;
			vddt = vdd - vth;
			vMin = opamp_voltage[0].x;
			vMax = Math.Max(vddt, opamp_voltage[0].y);
			denorm = vMax - vMin;
			norm = 1.0 / denorm;
			n16 = norm * ((1 << 16) - 1);
			currFactorCoeff = denorm * (uCox / 2.0 * 1.0e-6 / c);
			voice_voltage_range = vvr;
			voice_dc_voltage = vdv;

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
				double tmp = @out.x > 0 ? @out.x : 0;
				opamp_rev[x] = (ushort)(tmp + 0.5);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetVolume()
		{
			return volume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetResonance()
		{
			return resonance;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetSummer()
		{
			return summer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetMixer()
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract Integrator BuildIntegrator();
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNormalizedValue(double value)
		{
			double tmp = n16 * (value - vMin);
			return (ushort)(tmp + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNormalizedCurrentFactor(double wl)
		{
			double tmp = (1 << 13) * currFactorCoeff * wl;
			return (ushort)(tmp + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetNVMin()
		{
			double tmp = n16 * vMin;
			return (ushort)(tmp + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetNormalizedVoice(float value)
		{
			return GetNormalizedValue(GetVoiceVoltage(value));
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

			for (int i = 0; i < 5; i++)
			{
				int iDiv = 2 + i;		// 2 - 6 input "resistors"
				int size = iDiv << 16;
				double n = iDiv;
				double r_iDiv = 1.0 / iDiv;

				opAmpModel.Reset();
				summer[i] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16 * r_iDiv;	// vMin .. vMax
					summer[i][vi] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
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

			for (int i = 0; i < 8; i++)
			{
				int iDiv = i == 0 ? 1 : i;
				int size = i == 0 ? 1 : i << 16;
				double n = i * nRatio;
				double r_iDiv = 1.0 / iDiv;

				opAmpModel.Reset();
				mixer[i] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16 * r_iDiv;	// vMin .. vMax
					mixer[i][vi] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
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

			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;
				double n = n8 / nDivisor;

				opAmpModel.Reset();
				volume[n8] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16;			// vMin .. vMax
					volume[n8][vi] = GetNormalizedValue(opAmpModel.Solve(n, vIn));
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

			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;

				opAmpModel.Reset();
				resonance[n8] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi * r_N16;			// vMin .. vMax
					resonance[n8][vi] = GetNormalizedValue(opAmpModel.Solve(resonance_n[n8], vIn));
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
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetVoiceVoltage(float value)
		{
			return value * voice_voltage_range + voice_dc_voltage;
		}
		#endregion
	}
}

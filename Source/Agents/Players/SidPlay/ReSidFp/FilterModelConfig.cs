/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	internal abstract class FilterModelConfig
	{
		private readonly double voice_voltage_range;
		private readonly double voice_dc_voltage;

		// Capacitor value
		protected readonly double c;

		// Transistor parameters
		private readonly double vdd;
		private readonly double vth;			// Threshold voltage
		protected readonly double ut;			// Thermal voltage: Ut = kT/q = 8.61734315e-5*T ~ 26mV
		protected readonly double uCox;			// Transconductance coefficient: u*Cox
		protected readonly double vddt;			// Vdd - Vth;

		// Derived stuff
		protected readonly double vMin, vMax;
		protected readonly double denorm, norm;

		// Fixed point scaling for 16 bit op-amp output
		protected readonly double n16;

		// Current factor coefficient for op-amp integrators
		private readonly double currFactorCoeff;

		// Lookup tables for gain and summer op-amps in output stage / filter
		protected readonly ushort[][] mixer = new ushort[8][];			// This is initialized in the derived class constructor
		protected readonly ushort[][] summer = new ushort[5][];			// This is initialized in the derived class constructor
		protected readonly ushort[][] gain_vol = new ushort[16][];		// This is initialized in the derived class constructor
		protected readonly ushort[][] gain_res = new ushort[16][];		// This is initialized in the derived class constructor

		// Reverse op-amp transfer function
		protected readonly ushort[] opamp_rev = new ushort[1 << 16];	// This is initialized in the derived class constructor

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FilterModelConfig(double vvr, double vdv, double c, double vdd, double vth, double uCox, Spline.Point[] opamp_voltage, uint opamp_size)
		{
			voice_voltage_range = vvr;
			voice_dc_voltage = vdv;
			this.c = c;
			this.vdd = vdd;
			this.vth = vth;
			ut = 26.0e-3;
			this.uCox = uCox;
			vddt = vdd - vth;
			vMin = opamp_voltage[0].x;
			vMax = Math.Max(vddt, opamp_voltage[0].y);
			denorm = vMax - vMin;
			norm = 1.0 / denorm;
			n16 = norm * ((1 << 16) - 1);
			currFactorCoeff = denorm * (uCox / 2.0 * 1.0e-6 / c);

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
		public ushort[][] GetGainVol()
		{
			return gain_vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetGainRes()
		{
			return gain_res;
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
		/// The digital range of one voice is 20 bits; create a scaling term
		/// for multiplication which fits in 11 bits
		/// </summary>
		/********************************************************************/
		public int GetVoiceScaleS11()
		{
			return (int)((norm * ((1 << 11) - 1)) * voice_voltage_range);
		}



		/********************************************************************/
		/// <summary>
		/// The "zero" output level of the voices
		/// </summary>
		/********************************************************************/
		public int GetNormalizedVoiceDc()
		{
			return (int)(n16 * (voice_dc_voltage - vMin));
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



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetVoiceDcVoltage()
		{
			return voice_dc_voltage;
		}

		#region Helper functions
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
		#endregion
	}
}

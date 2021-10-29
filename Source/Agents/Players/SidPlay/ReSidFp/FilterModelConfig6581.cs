/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// Calculate parameters for 6581 filter emulation
	/// </summary>
	internal class FilterModelConfig6581
	{
		private const int DAC_BITS = 11;

		private const uint OPAMP_SIZE = 33;

		// This is the SID 6581 op-amp voltage transfer function, measured on
		// CAP1B/CAP1A on a chip marked MOS 6581R4AR 0687 14.
		// All measured chips have op-amps with output voltages (and thus input
		// voltages) within the range of 0.81V - 10.31V
		private static readonly Spline.Point[] opamp_voltage =
		{
			new ( 0.81, 10.31),		// Approximate start of actual range
			new ( 2.40, 10.31),
			new ( 2.60, 10.30),
			new ( 2.70, 10.29),
			new ( 2.80, 10.26),
			new ( 2.90, 10.17),
			new ( 3.00, 10.04),
			new ( 3.10,  9.83),
			new ( 3.20,  9.58),
			new ( 3.30,  9.32),
			new ( 3.50,  8.69),
			new ( 3.70,  8.00),
			new ( 4.00,  6.89),
			new ( 4.40,  5.21),
			new ( 4.54,  4.54),		// Working point (vi = vo)
			new ( 4.60,  4.19),
			new ( 4.80,  3.00),
			new ( 4.90,  2.30),		// Change of curvature
			new ( 4.95,  2.03),
			new ( 5.00,  1.88),
			new ( 5.05,  1.77),
			new ( 5.10,  1.69),
			new ( 5.20,  1.58),
			new ( 5.40,  1.44),
			new ( 5.60,  1.33),
			new ( 5.80,  1.26),
			new ( 6.00,  1.21),
			new ( 6.40,  1.12),
			new ( 7.00,  1.02),
			new ( 7.50,  0.97),
			new ( 8.50,  0.89),
			new ( 10.00,  0.81),
			new ( 10.31,  0.81)		// Approximate end of actual range
		};

		private static FilterModelConfig6581 instance = null;

		private readonly double voice_voltage_range;
		private readonly double voice_dc_voltage;

		/// <summary>
		/// Capacitor value
		/// </summary>
		private readonly double c;

		// Transistor parameters
		private readonly double vdd;
		private readonly double vth;			// Threshold voltage
		private readonly double ut;				// Thermal voltage: Ut = kT/q = 8.61734315e-5*T ~ 26mV
		private readonly double uCox;			// Transconductance coefficient: u*Cox
		private readonly double wl_vcr;			// W/L for VCR
		private readonly double wl_snake;		// W/L for "snake"
		private readonly double vddt;			// Vdd - Vth;

		// DAC parameters
		private readonly double dac_zero;
		private readonly double dac_scale;

		// Derived stuff
		private readonly double vMin, vMax;
		private readonly double denorm, norm;

		/// <summary>
		/// Fixed point scaling for 16 bit op-amp output
		/// </summary>
		private readonly double n16;

		// Lookup tables for gain and summer op-amps in output stage / filter
		private readonly ushort[][] mixer = new ushort[8][];
		private readonly ushort[][] summer = new ushort[5][];
		private readonly ushort[][] gain = new ushort[16][];

		/// <summary>
		/// DAC lookup table
		/// </summary>
		private readonly Dac dac;

		// VCR - 6581 only
		private readonly ushort[] vcr_vg = new ushort[1 << 16];
		private readonly ushort[] vcr_n_ids_term = new ushort[1 << 16];

		private readonly ushort[] opamp_rev = new ushort[1 << 16];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private FilterModelConfig6581()
		{
			voice_voltage_range = 1.5;
			voice_dc_voltage = 5.0;
			c = 470e-12;
			vdd = 12.18;
			vth = 1.31;
			ut = 26.0e-3;
			uCox = 20e-6;
			wl_vcr = 9.0 / 1.0;
			wl_snake = 1.0 / 115.0;
			vddt = vdd - vth;
			dac_zero = 6.65;
			dac_scale = 2.63;
			vMin = opamp_voltage[0].x;
			vMax = vddt < opamp_voltage[0].y ? opamp_voltage[0].y : vddt;
			denorm = vMax - vMin;
			norm = 1.0 / denorm;
			n16 = norm * ((1 << 16) - 1);
			dac = new Dac(DAC_BITS);

			dac.KinkedDac(ChipModel.MOS6581);

			// Convert op-amp voltage transfer to 16 bit values
			Spline.Point[] scaled_voltage = new Spline.Point[OPAMP_SIZE];

			for (uint i = 0; i < OPAMP_SIZE; i++)
			{
				scaled_voltage[i].x = n16 * (opamp_voltage[i].x - opamp_voltage[i].y + denorm) / 2.0;
				scaled_voltage[i].y = n16 * (opamp_voltage[i].x - vMin);
			}

			// Create lookup table mapping capacitor voltage to op-amp input voltage:
			Spline s = new Spline(scaled_voltage, OPAMP_SIZE);

			for (int x = 0; x < (1 << 16); x++)
			{
				Spline.Point @out = s.Evaluate(x);
				double tmp = @out.x;

				if (tmp < 0.0)
					tmp = 0.0;

				opamp_rev[x] = (ushort)(tmp + 0.5);
			}

			// Create lookup tables for gains / summers
			OpAmp opampModel = new OpAmp(opamp_voltage, OPAMP_SIZE, vddt);

			// The filter summer operates at n ~ 1, and has 5 fundamentally different
			// input configurations (2 - 6 input "resistors")
			//
			// Note that all "on" transistors are modeled as one. This is not
			// entirely accurate, since the input for each transistor is different,
			// and transistors are not linear components. However modeling all
			// transistors separately would be extremely costly
			for (int i = 0; i < 5; i++)
			{
				int iDiv = 2 + i;		// 2 - 6 input "resistors"
				int size = iDiv << 16;
				double n = iDiv;

				opampModel.Reset();
				summer[i] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16 / iDiv;	// vMin .. vMax
					double tmp = (opampModel.Solve(n, vIn) - vMin) * n16;
					summer[i][vi] = (ushort)(tmp + 0.5);
				}
			}

			// The audio mixer operates at n ~ 8/6, ans has 8 fundamentally different
			// input configurations (0 - 7 input "resistors").
			//
			// All "on", transistors are modeled as one - see comments above for
			// the filter summer
			for (int i = 0; i < 8; i++)
			{
				int iDiv = i == 0 ? 1 : i;
				int size = i == 0 ? 1 : i << 16;
				double n = i * 8.0 / 6.0;

				opampModel.Reset();
				mixer[i] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16 / iDiv;	// vMin .. vMax
					double tmp = (opampModel.Solve(n, vIn) - vMin) * n16;
					mixer[i][vi] = (ushort)(tmp + 0.5);
				}
			}

			// 4 bit "resistor" ladders in the bandpass resonance gain and the audio
			// output gain necessitate 16 gain tables.
			// From die photographs of the bandpass and volume "resistor" ladders
			// it follows that gain ~ vol/8 and 1/Q ~ ~res/8 (assuming ideal
			// op-amps and ideal "resistors")
			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;
				double n = n8 / 8.0;

				opampModel.Reset();
				gain[n8] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16;			// vMin .. vMax
					double tmp = (opampModel.Solve(n, vIn) - vMin) * n16;
					gain[n8][vi] = (ushort)(tmp + 0.5);
				}
			}

			double nVddt = n16 * vddt;
			double nVmin = n16 * vMin;

			for (uint i = 0; i < (1 << 16); i++)
			{
				// The table index is right-shifted 16 times in order to fit in
				// 16 bits; the argument to sqrt is thus multiplied by (1 << 16)
				double vg = nVddt - Math.Sqrt(i << 16);
				double tmp = vg - nVmin;
				vcr_vg[i] = (ushort)(tmp + 0.5);
			}

			//  EKV model:
			//
			//  Ids = Is * (if - ir)
			//  Is = (2 * u*Cox * Ut^2)/k * W/L
			//  if = ln^2(1 + e^((k*(Vg - Vt) - Vs)/(2*Ut))
			//  ir = ln^2(1 + e^((k*(Vg - Vt) - Vd)/(2*Ut))

			// Moderate inversion characteristic current
			double @is = (2.0 * uCox * ut * ut) * wl_vcr;

			// Normalize current factor for 1 cycle at 1Mhz
			double n15 = norm * ((1 << 15) - 1);
			double n_is = n15 * 1.0e-6 / c * @is;

			// kVgt_Vx = k*(Vg - Vt) - Vx
			// I.e. if k != 1.0, Vg must be scaled accordingly
			for (int kVgt_vx = 0; kVgt_vx < (1 << 16); kVgt_vx++)
			{
				double log_term = Log1p(Math.Exp((kVgt_vx / n16) / (2.0 * ut)));

				// Scaled by m*2^15
				double tmp = n_is * log_term * log_term;
				vcr_n_ids_term[kVgt_vx] = (ushort)(tmp + 0.5);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static FilterModelConfig6581 GetInstance()
		{
			if (instance == null)
				instance = new FilterModelConfig6581();

			return instance;
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
		public int GetVoiceDc()
		{
			return (int)(n16 * (voice_dc_voltage - vMin));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort[][] GetGain()
		{
			return gain;
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
		/// Construct an 11 bit cutoff frequency DAC output voltage table.
		/// Ownership is transferred to the requester which becomes
		/// responsible of freeing the object when done
		/// </summary>
		/********************************************************************/
		public ushort[] GetDac(double adjustment)
		{
			double dac_zero = GetDacZero(adjustment);

			ushort[] f0_dac = new ushort[1 << DAC_BITS];

			for (uint i = 0; i < (1 << DAC_BITS); i++)
			{
				double fcd = dac.GetOutput(i);
				double tmp = n16 * (dac_zero + fcd * dac_scale / (1 << DAC_BITS) - vMin);
				f0_dac[i] = (ushort)(tmp + 0.5);
			}

			return f0_dac;
		}



		/********************************************************************/
		/// <summary>
		/// Construct an integrator solver
		/// </summary>
		/********************************************************************/
		public Integrator6581 BuildIntegrator()
		{
			// Vdd - Vth, normalized so that translated values can be subtracted:
			// Vddt - x = (Vddt - t) - (x - t)
			double tmp = n16 * (vddt - vMin);
			ushort nVddt = (ushort)(tmp + 0.5);

			tmp = n16 * (vth - vMin);
			ushort nVt = (ushort)(tmp + 0.5);

			tmp = n16 * vMin;
			ushort nVMin = (ushort)(tmp + 0.5);

			// Normalized snake current factor, 1 cycle at 1Mhz
			// Fit in 5 bits
			tmp = denorm * (1 << 13) * (uCox / 2.0 * wl_snake * 1.0e-6 / c);
			ushort n_snake = (ushort)(tmp + 0.5);

			return new Integrator6581(vcr_vg, vcr_n_ids_term, opamp_rev, nVddt, nVt, nVMin, n_snake, n16);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Compute log(1+x) without losing precision for small values of x
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double Log1p(double x)
		{
			return Math.Log(1.0 + x) - (((1.0 + x) - 1.0) - x) / (1.0 + x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private double GetDacZero(double adjustment)
		{
			return dac_zero + (1.0 - adjustment);
		}
		#endregion
	}
}

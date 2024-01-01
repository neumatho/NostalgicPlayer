/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// Calculate parameters for 6581 filter emulation
	/// </summary>
	internal sealed class FilterModelConfig6581 : FilterModelConfig
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
		private static readonly object instance6581_Lock = new object();

		// Transistor parameters
		private readonly double wl_vcr;			// W/L for VCR
		private readonly double wl_snake;		// W/L for "snake"

		// DAC parameters
		private readonly double dac_zero;
		private readonly double dac_scale;

		/// <summary>
		/// DAC lookup table
		/// </summary>
		private readonly Dac dac;

		// VCR - 6581 only
		private readonly ushort[] vcr_nVg = new ushort[1 << 16];
		private readonly ushort[] vcr_n_ids_term = new ushort[1 << 16];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private FilterModelConfig6581() : base(
			1.5,		// Voice voltage range
			5.075,	// Voice DC voltage
			470e-12,	// Capacitor value
			12.18,	// Vdd
			1.31,	// Vth
			20e-6,	// uCox
			opamp_voltage,
			OPAMP_SIZE)
		{
			wl_vcr = 9.0 / 1.0;
			wl_snake = 1.0 / 115.0;
			dac_zero = 6.65;
			dac_scale = 2.63;
			dac = new Dac(DAC_BITS);

			dac.KinkedDac(ChipModel.MOS6581);

			// Create lookup tables for gains / summers
			void Loop1()
			{
				OpAmp opampModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);

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
						summer[i][vi] = GetNormalizedValue(opampModel.Solve(n, vIn));
					}
				}
			}

			void Loop2()
			{
				OpAmp opampModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);

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
						mixer[i][vi] = GetNormalizedValue(opampModel.Solve(n, vIn));
					}
				}
			}

			void Loop3()
			{
				OpAmp opampModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);

				// 4 bit "resistor" ladders in the audio output gain
				// necessitate 16 gain tables.
				// From die photographs of the volume "resistor" ladders
				// it follows that gain ~ vol/12 (assuming ideal
				// op-amps and ideal "resistors")
				for (int n8 = 0; n8 < 16; n8++)
				{
					int size = 1 << 16;
					double n = n8 / 12.0;

					opampModel.Reset();
					gain_vol[n8] = new ushort[size];

					for (int vi = 0; vi < size; vi++)
					{
						double vIn = vMin + vi / n16;			// vMin .. vMax
						gain_vol[n8][vi] = GetNormalizedValue(opampModel.Solve(n, vIn));
					}
				}
			}

			void Loop4()
			{
				OpAmp opampModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);

				// 4 bit "resistor" ladders in the bandpass resonance gain
				// necessitate 16 gain tables.
				// From die photographs of the bandpass "resistor" ladders
				// it follows that 1/Q ~ ~res/8 (assuming ideal
				// op-amps and ideal "resistors")
				for (int n8 = 0; n8 < 16; n8++)
				{
					int size = 1 << 16;
					double n = (~n8 & 0xf) / 8.0;

					opampModel.Reset();
					gain_res[n8] = new ushort[size];

					for (int vi = 0; vi < size; vi++)
					{
						double vIn = vMin + vi / n16;			// vMin .. vMax
						gain_res[n8][vi] = GetNormalizedValue(opampModel.Solve(n, vIn));
					}
				}
			}

			void Loop5()
			{
				double nVddt = n16 * (vddt - vMin);

				for (uint i = 0; i < (1 << 16); i++)
				{
					// The table index is right-shifted 16 times in order to fit in
					// 16 bits; the argument to sqrt is thus multiplied by (1 << 16)
					double tmp = nVddt - Math.Sqrt(i << 16);
					vcr_nVg[i] = (ushort)(tmp + 0.5);
				}
			}

			void Loop6()
			{
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
				for (int i = 0; i < (1 << 16); i++)
				{
					int kVgt_Vx = i - (1 << 15);
					double log_term = Log1p(Math.Exp((kVgt_Vx / n16) / (2.0 * ut)));

					// Scaled by m*2^15
					double tmp = n_is * log_term * log_term;
					vcr_n_ids_term[i] = (ushort)(tmp + 0.5);
				}
			}

			Task loop1Task = Task.Run(Loop1);
			Task loop2Task = Task.Run(Loop2);
			Task loop3Task = Task.Run(Loop3);
			Task loop4Task = Task.Run(Loop4);
			Task loop5Task = Task.Run(Loop5);
			Task loop6Task = Task.Run(Loop6);

			Task.WaitAll(loop1Task, loop2Task, loop3Task, loop4Task, loop5Task, loop6Task);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static FilterModelConfig6581 GetInstance()
		{
			lock (instance6581_Lock)
			{
				if (instance == null)
					instance = new FilterModelConfig6581();

				return instance;
			}
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
				f0_dac[i] = GetNormalizedValue(dac_zero + fcd * dac_scale / (1 << DAC_BITS));
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
			return new Integrator6581(this, wl_snake);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetVcr_nVg(uint i)
		{
			return vcr_nVg[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort GetVcr_n_Ids_Term(int i)
		{
			return vcr_n_ids_term[i];
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

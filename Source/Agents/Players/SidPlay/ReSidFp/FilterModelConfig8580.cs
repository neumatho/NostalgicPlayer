/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// Calculate parameters for 8580 filter emulation
	/// </summary>
	internal class FilterModelConfig8580
	{
		private const uint OPAMP_SIZE = 21;

		// R1 = 15.3*Ri
		// R2 =  7.3*Ri
		// R3 =  4.7*Ri
		// Rf =  1.4*Ri
		// R4 =  1.4*Ri
		// R8 =  2.0*Ri
		// RC =  2.8*Ri
		//
		// res  feedback  input
		// ---  --------  -----
		//  0   Rf        Ri
		//  1   Rf|R1     Ri
		//  2   Rf|R2     Ri
		//  3   Rf|R3     Ri
		//  4   Rf        R4
		//  5   Rf|R1     R4
		//  6   Rf|R2     R4
		//  7   Rf|R3     R4
		//  8   Rf        R8
		//  9   Rf|R1     R8
		//  A   Rf|R2     R8
		//  B   Rf|R3     R8
		//  C   Rf        RC
		//  D   Rf|R1     RC
		//  E   Rf|R2     RC
		//  F   Rf|R3     RC
		private static readonly double[] resGain = new double[16]
		{
			1.4/1.0,                     // Rf/Ri        1.4
			((1.4*15.3)/(1.4+15.3))/1.0, // (Rf|R1)/Ri   1.28263
			((1.4*7.3)/(1.4+7.3))/1.0,   // (Rf|R2)/Ri   1.17471
			((1.4*4.7)/(1.4+4.7))/1.0,   // (Rf|R3)/Ri   1.07869
			1.4/1.4,                     // Rf/R4        1
			((1.4*15.3)/(1.4+15.3))/1.4, // (Rf|R1)/R4   0.916168
			((1.4*7.3)/(1.4+7.3))/1.4,   // (Rf|R2)/R4   0.83908
			((1.4*4.7)/(1.4+4.7))/1.4,   // (Rf|R3)/R4   0.770492
			1.4/2.0,                     // Rf/R8        0.7
			((1.4*15.3)/(1.4+15.3))/2.0, // (Rf|R1)/R8   0.641317
			((1.4*7.3)/(1.4+7.3))/2.0,   // (Rf|R2)/R8   0.587356
			((1.4*4.7)/(1.4+4.7))/2.0,   // (Rf|R3)/R8   0.539344
			1.4/2.8,                     // Rf/RC        0.5
			((1.4*15.3)/(1.4+15.3))/2.8, // (Rf|R1)/RC   0.458084
			((1.4*7.3)/(1.4+7.3))/2.8,   // (Rf|R2)/RC   0.41954
			((1.4*4.7)/(1.4+4.7))/2.8,   // (Rf|R3)/RC   0.385246
		};

		// This is the SID 8580 op-amp voltage transfer function, measured on
		// CAP1B/CAP1A on a chip marked CSG 8580R5 1690 25
		private static readonly Spline.Point[] opamp_voltage =
		{
			new ( 1.30,  8.91),		// Approximate start of actual range
			new ( 4.76,  8.91),
			new ( 4.77,  8.90),
			new ( 4.78,  8.88),
			new ( 4.785, 8.86),
			new ( 4.79,  8.80),
			new ( 4.795, 8.60),
			new ( 4.80,  8.25),
			new ( 4.805, 7.50),
			new ( 4.81,  6.10),
			new ( 4.815, 4.05),		// Change of curvature
			new ( 4.82,  2.27),
			new ( 4.825, 1.65),
			new ( 4.83,  1.55),
			new ( 4.84,  1.47),
			new ( 4.85,  1.43),
			new ( 4.87,  1.37),
			new ( 4.90,  1.34),
			new ( 5.00,  1.30),
			new ( 5.10,  1.30),
			new ( 8.91,  1.30)		// Approximate end of actual range
		};

		private static FilterModelConfig8580 instance = null;

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
		private readonly double vddt;			// Vdd - Vth;

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
		private readonly ushort[][] gain_vol = new ushort[16][];
		private readonly ushort[][] gain_res = new ushort[16][];

		/// <summary>
		/// Reverse op-amp transfer function
		/// </summary>
		private readonly ushort[] opamp_rev = new ushort[1 << 16];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private FilterModelConfig8580()
		{
			voice_voltage_range = 0.25;		// FIXME measure
			voice_dc_voltage = 4.80;		// FIXME was 4.76
			c = 22e-9;
			vdd = 9.09;
			vth = 0.80;
			ut = 26.0e-3;
			uCox = 100e-6;
			vddt = vdd - vth;
			vMin = opamp_voltage[0].x;
			vMax = vddt < opamp_voltage[0].y ? opamp_voltage[0].y : vddt;
			denorm = vMax - vMin;
			norm = 1.0 / denorm;
			n16 = norm * ((1 << 16) - 1);

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

			// The audio mixer operates at n ~ 8/5, ans has 8 fundamentally different
			// input configurations (0 - 7 input "resistors").
			//
			// All "on", transistors are modeled as one - see comments above for
			// the filter summer
			for (int i = 0; i < 8; i++)
			{
				int iDiv = i == 0 ? 1 : i;
				int size = i == 0 ? 1 : i << 16;
				double n = i * 8.0 / 5.0;

				opampModel.Reset();
				mixer[i] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16 / iDiv;	// vMin .. vMax
					double tmp = (opampModel.Solve(n, vIn) - vMin) * n16;
					mixer[i][vi] = (ushort)(tmp + 0.5);
				}
			}

			// 4 bit "resistor" ladders in the audio output gain
			// necessitate 16 gain tables.
			// From die photographs of the volume "resistor" ladders
			// it follows that gain ~ vol/16 (assuming ideal op-amps)
			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;
				double n = n8 / 16.0;

				opampModel.Reset();
				gain_vol[n8] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16;			// vMin .. vMax
					double tmp = (opampModel.Solve(n, vIn) - vMin) * n16;
					gain_vol[n8][vi] = (ushort)(tmp + 0.5);
				}
			}

			// 4 bit "resistor" ladders in the bandpass resonance gain
			// necessitate 16 gain tables.
			// From die photographs of the bandpass and volume "resistor" ladders
			// it follows that 1/Q ~ 2^((4 - res)/8) (assuming ideal
			// op-amps and ideal "resistors")
			for (int n8 = 0; n8 < 16; n8++)
			{
				int size = 1 << 16;

				opampModel.Reset();
				gain_res[n8] = new ushort[size];

				for (int vi = 0; vi < size; vi++)
				{
					double vIn = vMin + vi / n16;			// vMin .. vMax
					double tmp = (opampModel.Solve(resGain[n8], vIn) - vMin) * n16;
					gain_res[n8][vi] = (ushort)(tmp + 0.5);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static FilterModelConfig8580 GetInstance()
		{
			if (instance == null)
				instance = new FilterModelConfig8580();

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
		/// Construct an integrator solver
		/// </summary>
		/********************************************************************/
		public Integrator8580 BuildIntegrator()
		{
			double nKp = denorm * (uCox / 2.0 * 1.0e-6 / c);

			return new Integrator8580(opamp_rev, vth, nKp, vMin, n16);
		}
	}
}

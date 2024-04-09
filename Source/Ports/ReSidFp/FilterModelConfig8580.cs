/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// Calculate parameters for 8580 filter emulation
	/// </summary>
	internal sealed class FilterModelConfig8580 : FilterModelConfig
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
		private static readonly object instance8580_Lock = new object();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private FilterModelConfig8580() : base(
			0.24,	// Voice voltage range FIXME measure
			4.84,	// Voice DC voltage FIXME measure
			22e-9,	// Capacitor value
			9.09,	// Vdd
			0.80,	// Vth
			100e-6,	// uCox
			opamp_voltage,
			OPAMP_SIZE)
		{
			// Create lookup tables for gains / summers
			void Loop1()
			{
				OpAmp opAmpModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);
				BuildSummerTable(opAmpModel);
			}

			void Loop2()
			{
				OpAmp opAmpModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);
				BuildMixerTable(opAmpModel, 8.0 / 5.0);
			}

			void Loop3()
			{
				OpAmp opAmpModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);
				BuildVolumeTable(opAmpModel, 16.0);
			}

			void Loop4()
			{
				OpAmp opAmpModel = new OpAmp(new List<Spline.Point>(opamp_voltage), vddt, vMin, vMax);
				BuildResonanceTable(opAmpModel, resGain);
			}

			Task loop1Task = Task.Run(Loop1);
			Task loop2Task = Task.Run(Loop2);
			Task loop3Task = Task.Run(Loop3);
			Task loop4Task = Task.Run(Loop4);

			Task.WaitAll(loop1Task, loop2Task, loop3Task, loop4Task);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static FilterModelConfig8580 GetInstance()
		{
			lock (instance8580_Lock)
			{
				if (instance == null)
					instance = new FilterModelConfig8580();

				return instance;
			}
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Construct an integrator solver
		/// </summary>
		/********************************************************************/
		public override Integrator BuildIntegrator()
		{
			return new Integrator8580(this);
		}
		#endregion
	}
}

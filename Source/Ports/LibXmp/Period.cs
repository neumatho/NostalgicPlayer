/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Period
	{
		// Gravis Ultrasound frequency increments in steps of Hz/1024, where Hz is the
		// current rate of the card and is dependent on the active channel count.
		// For <=14 channels, the rate is 44100. For 15 to 32 channels, the rate is
		// round(14 * 44100 / active_channels)
		private static readonly c_double[] gus_Rates = new c_double[]
		{
			/* <= 14 */ 44100.0,
			/* 15-20 */ 41160.0,  38587.5,  36317.65, 34300.0, 32494.74, 30870.0,
			/* 21-26 */ 29400.0,  28063.64, 26843.48, 25725.0, 24696.0,  23746.15,
			/* 27-32 */ 22866.67, 22050.0,  21289.66, 20580.0, 19916.13, 19294.75
		};

		private const c_double M_LN2 = 0.69314718055994530942;

		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Period(Xmp_Context ctx)
		{
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Get period from note
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Note_To_Period(c_int n, c_int f, c_double adj)
		{
			Module_Data m = ctx.M;

			c_double d = n + (c_double)f / 128;
			c_double per;

			switch (m.Period_Type)
			{
				case Containers.Common.Period.Linear:
				{
					per = (240.0 - d) * 16;		// Linear
					break;
				}

				case Containers.Common.Period.CSpd:
				{
					per = 8363.0 * Math.Pow(2, n / 12.0) / 32 + f;	// Hz
					break;
				}

				default:
				{
					per = Constants.Period_Base / Math.Pow(2, d / 12);	// Amiga
					break;
				}
			}

			if (adj > 0.1)
				per *= adj;

			return per;
		}



		/********************************************************************/
		/// <summary>
		/// For the software mixer
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Note_To_Period_Mix(c_int n, c_int b)
		{
			c_double d = n + (c_double)b / 12800;

			return Constants.Period_Base / Math.Pow(2, d / 12);
		}



		/********************************************************************/
		/// <summary>
		/// Get note from period.
		/// This function is used only by the MOD loader
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Period_To_Note(c_int p)
		{
			if (p <= 0)
				return 0;

			return (c_int)(LibXmp_Round(12.0 * Math.Log(Constants.Period_Base / p) / M_LN2) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Get pitchbend from base note and Amiga period
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Period_To_Bend(c_double p, c_int n, c_double adj)
		{
			Module_Data m = ctx.M;

			if ((n == 0) || (p < 0.1))
				return 0;

			c_double d;

			switch (m.Period_Type)
			{
				case Containers.Common.Period.Linear:
					return (c_int)(100 * (8 * (((240 - n) << 4) - p)));

				case Containers.Common.Period.CSpd:
				{
					d = LibXmp_Note_To_Period(n, 0, adj);
					return (c_int)LibXmp_Round(100.0 * (1536.0 / M_LN2) * Math.Log(p / d));
				}

				default:
				{
					// Amiga
					d = LibXmp_Note_To_Period(n, 0, adj);
					return (c_int)LibXmp_Round(100.0 * (1536.0 / M_LN2) * Math.Log(d / p));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert finetune = 1200 * log2(C2SPD/8363)
		///
		///      c = (1200.0 * log(c2spd) - 1200.0 * log(c4_rate)) / M_LN2;
		///      xpo = c/100;
		///      fin = 128 * (c%100) / 100
		/// </summary>
		/********************************************************************/
		public void LibXmp_C2Spd_To_Note(c_int c2Spd, out c_int n, out c_int f)
		{
			if (c2Spd <= 0)
			{
				n = f = 0;
				return;
			}

			c_int c = (c_int)(1536.0 * Math.Log((c_double)c2Spd / 8363) / M_LN2);
			n = c / 128;
			f = c % 128;
		}



		/********************************************************************/
		/// <summary>
		/// Get a Gravis Ultrasound frequency offset in Hz for a given number
		/// of steps
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Gus_Frequency_Steps(c_int num_Steps, c_int num_Channels_Active)
		{
			Common.Clamp(ref num_Channels_Active, 14, 32);

			return (num_Steps * gus_Rates[num_Channels_Active - 14]) / 1024.0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_double LibXmp_Round(c_double val)
		{
			return val >= 0.0 ? Math.Floor(val + 0.5) : Math.Ceiling(val - 0.5);
		}
		#endregion
	}
}

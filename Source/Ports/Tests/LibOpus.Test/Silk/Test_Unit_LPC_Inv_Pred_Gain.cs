/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Silk
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_LPC_Inv_Pred_Gain
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LPC_Inv_Pred_Gain()
		{
			c_int arch = Cpu_Support.Opus_Select_Arch();

			// Set to 10000 so all branches function are triggered
			c_int loop_num = 10000;

			Console.WriteLine("Testing silk_LPC_inverse_pred_gain() optimization ...");

			for (c_int count = 0; count < loop_num; count++)
			{
				opus_int16[] A_Q12 = new opus_int16[Constants.Silk_Max_Order_Lpc];

				for (opus_int order = 2; order <= Constants.Silk_Max_Order_Lpc; order += 2)		// Order must be even
				{
					for (c_uint shift = 0; shift < 16; shift++)		// Different dynamic range
					{
						for (c_uint i = 0; i < Constants.Silk_Max_Order_Lpc; i++)
							A_Q12[i] = (opus_int16)(((opus_int16)RandomGenerator.GetRandomNumber()) >> (int)shift);

						opus_int32 gain = LPC_Inv_Pred_Gain.Silk_LPC_Inverse_Pred_Gain(A_Q12, order, arch);

						// Look for filters that silk_LPC_inverse_pred_gain() thinks are
						// stable but definitely aren't
						if ((gain != 0) && (!Check_Stability(A_Q12, order)))
							Assert.Fail($"Loop {count} failed");
					}
				}

				if ((count % 500) == 0)
					Console.WriteLine($"Loop {count} passed");
			}

			Console.WriteLine("silk_LPC_inverse_pred_gain() optimization passed");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Computes the impulse response of the filter so we can catch
		/// filters that are definitely unstable. Some unstable filters may
		/// be classified as stable, but not the other way around
		/// </summary>
		/********************************************************************/
		private bool Check_Stability(opus_int16[] A_Q12, c_int order)
		{
			c_double[] y = new c_double[Constants.Silk_Max_Order_Lpc];
			c_int sum_a = 0, sum_abs_a = 0;

			for (c_int j = 0; j < order; j++)
			{
				sum_a += A_Q12[j];
				sum_abs_a += SigProc_Fix.Silk_Abs(A_Q12[j]);
			}

			// Check DC stability
			if (sum_a >= 4096)
				return false;

			// If the sum of absolute values is less than 1, the filter
			// has to be stable
			if (sum_abs_a < 4096)
				return true;

			y[0] = 1;

			for (c_int i = 0; i < 10000; i++)
			{
				c_double sum = 0;

				for (c_int j = 0; j < order ; j++)
					sum += y[j] * A_Q12[j];

				for (c_int j = order - 1; j > 0; j--)
					y[j] = y[j - 1];

				y[0] = sum * (1.0f / 4096);

				// If impulse response reaches +/- 10000, the filter
				// is definitely unstable
				if (!(y[0] < 10000) && (y[0] > -10000))
					return false;

				// Test every 8 sample for low amplitude
				if ((i & 0x7) == 0)
				{
					c_double amp = 0;

					for (c_int j = 0; j < order; j++)
						amp += Math.Abs(y[j]);

					if (amp < 0.00001)
						return true;
				}
			}

			return true;
		}
		#endregion
	}
}

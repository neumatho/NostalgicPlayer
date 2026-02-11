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

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Rotation
	{
		private const int Max_Size = 100;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Rotation()
		{
			Test_Rotation(15, 3);
			Test_Rotation(23, 5);
			Test_Rotation(50, 3);
			Test_Rotation(80, 1);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Rotation(c_int N, c_int K)
		{
			c_double err = 0, ener = 0;
			celt_norm[] x0 = new opus_val16[Max_Size];
			celt_norm[] x1 = new opus_val16[Max_Size];

			for (c_int i = 0; i < N; i++)
				x1[i] = x0[i] = (RandomGenerator.GetRandomNumber() % 16777215) - 8388608;

			Vq.Exp_Rotation(x1, N, 1, 1, K, Spread.Normal);

			for (c_int i = 0; i < N; i++)
			{
				err += (x0[i] - (c_double)x1[i]) * (x0[i] - (c_double)x1[i]);
				ener += x0[i] * (c_double)x0[i];
			}

			c_double snr0 = 20 * Math.Log10(ener / err);
			err = ener = 0;

			Vq.Exp_Rotation(x1, N, -1, 1, K, Spread.Normal);

			for (c_int i = 0; i < N; i++)
			{
				err += (x0[i] - (c_double)x1[i]) * (x0[i] - (c_double)x1[i]);
				ener += x0[i] * (c_double)x0[i];
			}

			c_double snr = 20 * Math.Log10(ener / err);
			Console.WriteLine($"SNR for size {N} ({K} pulses) is {snr} (was {snr0} without inverse)");

			if ((snr < 60) || (snr0 > 20))
				Assert.Fail("FAIL!");
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Cwrs32
	{
		private const int NMax = 240;

		private const int NDims = 22;

		private readonly c_int[] pn =
		[
			 2,   3,   4,   6,   8,   9,  11,  12,  16,
			18,  22,  24,  32,  36,  44,  48,  64,  72,
			88,  96, 144, 176
		];

		private readonly c_int[] pkmax =
		[
			128, 128, 128,  88,  36,  26,  18,  16,  12,
			 11,   9,   9,   7,   7,   6,   6,   5,   5,
			  5,   5,   4,   4
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Cwrs32()
		{
			for (c_int t = 0; t < NDims; t++)
			{
				c_int n = pn[t];

				for (c_int pseudo = 1; pseudo < 41; pseudo++)
				{
					c_int k = Rate.Get_Pulses(pseudo);

					if (k > pkmax[t])
						break;

					Console.WriteLine($"Testing CWRS with N={n}, K={k}...");

					opus_uint32 nc = Cwrs.Celt_Pvq_V(n, k);
					opus_uint32 inc = nc / 20000;

					if (inc < 1)
						inc = 1;

					for (opus_uint32 i = 0; i < nc; i += inc)
					{
						c_int[] y = new c_int[NMax];

						Cwrs.Cwrsi(n, k, i, y);

						c_int sy = 0;

						for (c_int j = 0; j < n; j++)
							sy += Math.Abs(y[j]);

						if (sy != k)
							Assert.Fail($"N={n} Pulse count mismatch in cwrsi ({sy}!={k})");

						opus_uint32 ii = Cwrs.Icwrs(n, y);
						opus_uint32 v = Cwrs.Celt_Pvq_V(n, k);

						if (ii != i)
							Assert.Fail($"Combination-index mismatch ({ii}!={i})");

						if (v != nc)
							Assert.Fail($"Combination count mismatch ({v}!={nc})");
					}
				}
			}
		}
	}
}

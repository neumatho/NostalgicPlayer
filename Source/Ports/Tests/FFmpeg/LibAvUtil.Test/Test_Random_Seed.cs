/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Random_Seed : TestBase
	{
		private delegate uint32_t Random_Seed_Delegate();

		private const c_int N = 256;
		private const c_int F = 2;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Random_Seed_()
		{
			RunTest("random_seed");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			uint32_t[] seeds = new uint32_t[N];
			Random_Seed_Delegate[] random_Seed = [ Random_Seed.Av_Get_Random_Seed, Random_Seed.Get_Generic_Seed ];

			for (c_int rsf = 0; rsf < F; ++rsf)
			{
				c_int retry, j = 0;

				for (retry = 0; retry < 3; retry++)
				{
					for (c_int i = 0; i < N; i++)
					{
						seeds[i] = random_Seed[rsf]();

						for (j = 0; j < i; j++)
						{
							if (seeds[j] == seeds[i])
								goto Retry;
						}
					}

					printf("seeds OK\n");
					break;

					Retry:
					;

				}

				if (retry >= 3)
				{
					printf("rsf %d: FAIL at %d with %lld\n", rsf, j, seeds[j]);

					return 1;
				}
			}

			return 0;
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;
using Algorithm = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Random()
		{
			Default_Prng prng = s_Prng;

			{
				vector<size_t> hist = new vector<size_t>(256);

				for (size_t i = 0; i < 256 * 256; ++i)
				{
					uint8 value = Random.Random_<uint8, uint64_t>(prng);
					hist[value] += 1;
				}

				for (size_t i = 0; i < 256; ++i)
					Assert.IsTrue(Algorithm.Is_In_Range<size_t, size_t>(hist[i], 16U, 65520U));
			}

			{
				vector<size_t> hist = new vector<size_t>(256);

				for (size_t i = 0; i < 256 * 256; ++i)
				{
					int8 value = Random.Random_<int8, uint64_t>(prng);
					hist[value + 0x80] += 1;
				}

				for (size_t i = 0; i < 256; ++i)
					Assert.IsTrue(Algorithm.Is_In_Range<size_t, size_t>(hist[i], 16U, 65520U));
			}

			{
				vector<size_t> hist = new vector<size_t>(256);

				for (size_t i = 0; i < 256 * 256; ++i)
				{
					uint8 value = Random.Random_<uint8, uint64_t>(prng, 1);
					hist[value] += 1;
				}

				for (size_t i = 0; i < 256; ++i)
				{
					if (i < 2)
						Assert.IsTrue(Algorithm.Is_In_Range<size_t, size_t>(hist[i], 16U, 65520U));
					else
						Assert.AreEqual(0U, hist[i]);
				}
			}
		}
	}
}

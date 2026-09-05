/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt
{
	/// <summary>
	///
	/// </summary>
	public partial class Test_Mpt
	{
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Random()
		{
			Sane_Random_Device rd = new Sane_Random_Device();
			Good_Engine prng = Seed.Make_Prng<Good_Engine, c_uint>(rd);
			Any_Engine<uint64> prng64 = new Any_Engine_Wrapper<uint64, uint64>(prng);
			Any_Engine<uint8> prng8 = new Any_Engine_Wrapper<uint8, uint64>(prng);

			bool failed = false;

			for (size_t i = 0; i < 10000; ++i)
			{
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_ulong, c_ulong>(Random.Random_<uint64, uint64>(prng, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_<int64, uint64>(prng, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng, (int8)(-42), (int8)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng, (int16)(-42), (int16)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng, -42, 69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_(prng, -42, 69), -42, 69);

				failed = failed || !Algorithm.Is_In_Range<c_float, c_float>(Random.Random_<c_float, uint64_t>(prng, 0.0f, 1.0f), 0.0f, 1.0f);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng, 0.0, 1.0), 0.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng, -1.0, 1.0), -1.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng, -1.0, 0.0), -1.0, 0.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng, 1.0, 2.0), 1.0, 2.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng, 1.0, 3.0), 1.0, 3.0);
			}

			for (size_t i = 0; i < 10000; ++i)
			{
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng64, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng64, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint64>(prng64, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_ulong, c_ulong>(Random.Random_<uint64, uint64>(prng64, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng64, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng64, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint64>(prng64, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_<int64, uint64>(prng64, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng64, (int8)(-42), (int8)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng64, (int16)(-42), (int16)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng64, -42, 69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_(prng64, -42, 69), -42, 69);

				failed = failed || !Algorithm.Is_In_Range<c_float, c_float>(Random.Random_<c_float, uint64_t>(prng64, 0.0f, 1.0f), 0.0f, 1.0f);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng64, 0.0, 1.0), 0.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng64, -1.0, 1.0), -1.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng64, -1.0, 0.0), -1.0, 0.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng64, 1.0, 2.0), 1.0, 2.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint64_t>(prng64, 1.0, 3.0), 1.0, 3.0);
			}

			for (size_t i = 0; i < 10000; ++i)
			{
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint8>(prng8, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint8>(prng8, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_uint, c_uint>(Random.Random_<uint16, uint8>(prng8, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_ulong, c_ulong>(Random.Random_<uint64, uint8>(prng8, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint8>(prng8, 7), 0, 127);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint8>(prng8, 8), 0, 255);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_<int16, uint8>(prng8, 9), 0, 511);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_<int64, uint8>(prng8, 1), 0, 1);

				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng8, (int8)(-42), (int8)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng8, (int16)(-42), (int16)69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_int, c_int>(Random.Random_(prng8, -42, 69), -42, 69);
				failed = failed || !Algorithm.Is_In_Range<c_long, c_long>(Random.Random_(prng8, -42, 69), -42, 69);

				failed = failed || !Algorithm.Is_In_Range<c_float, c_float>(Random.Random_<c_float, uint8>(prng8, 0.0f, 1.0f), 0.0f, 1.0f);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint8>(prng8, 0.0, 1.0), 0.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint8>(prng8, -1.0, 1.0), -1.0, 1.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint8>(prng8, -1.0, 0.0), -1.0, 0.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint8>(prng8, 1.0, 2.0), 1.0, 2.0);
				failed = failed || !Algorithm.Is_In_Range<c_double, c_double>(Random.Random_<c_double, uint8>(prng8, 1.0, 3.0), 1.0, 3.0);
			}

			Assert.IsFalse(failed);
		}
	}
}

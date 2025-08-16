/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_Laplace
	{
		private const int Data_Size = 40000;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Laplace()
		{
			c_int[] val = new c_int[10000], decay = new c_int[10000];
			CPointer<byte> ptr = CMemory.MAlloc<byte>(Data_Size);

			EntEnc.Ec_Enc_Init(out Ec_Enc enc, ptr, Data_Size);

			val[0] = 3; decay[0] = 6000;
			val[1] = 0; decay[1] = 5800;
			val[2] = -1; decay[2] = 5600;

			for (c_int i = 3; i < 10000; i++)
			{
				val[i] = RandomGenerator.GetRandomNumber() % 15 - 7;
				decay[i] = RandomGenerator.GetRandomNumber() % 11000 + 5000;
			}

			for (c_int i = 0; i < 10000; i++)
				Laplace.Ec_Laplace_Encode(enc, ref val[i], (c_uint)Ec_Laplace_Get_Start_Freq(decay[i]), decay[i]);

			EntEnc.Ec_Enc_Done(enc);

			EntDec.Ec_Dec_Init(out Ec_Dec dec, EntCode.Ec_Get_Buffer(enc), EntCode.Ec_Range_Bytes(enc));

			for (c_int i = 0; i < 10000; i++)
			{
				c_int d = Laplace.Ec_Laplace_Decode(dec, (c_uint)Ec_Laplace_Get_Start_Freq(decay[i]), decay[i]);

				if (d != val[i])
					Assert.Fail($"Got {d} instead of {val[i]}");
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Ec_Laplace_Get_Start_Freq(c_int decay)
		{
			opus_uint32 ft = 32768 - Laplace.Laplace_MinP * (2 * Laplace.Laplace_NMin + 1);
			c_int fs = (c_int)((ft * (16384 - decay)) / (16384 + decay));

			return (c_int)(fs + Laplace.Laplace_MinP);
		}
		#endregion
	}
}

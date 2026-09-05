/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt_Base
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Base_Saturate_Round()
		{
			Assert.AreEqual(uint32.MaxValue, SaturateRound.Saturate_Trunc<uint32, c_double>(int64.MaxValue));

			Assert.AreEqual(int32.MaxValue, SaturateRound.Saturate_Round<int32, c_double>(int32.MaxValue + 0.1));
			Assert.AreEqual(int32.MaxValue, SaturateRound.Saturate_Round<int32, c_double>(int32.MaxValue - 0.4));
			Assert.AreEqual(int32.MinValue, SaturateRound.Saturate_Round<int32, c_double>(int32.MinValue + 0.1));
			Assert.AreEqual(int32.MinValue, SaturateRound.Saturate_Round<int32, c_double>(int32.MinValue - 0.1));
			Assert.AreEqual(uint32.MaxValue, SaturateRound.Saturate_Round<uint32, c_double>(uint32.MaxValue + 0.499));
			Assert.AreEqual(110, SaturateRound.Saturate_Round<int8, c_double>(110.1));
			Assert.AreEqual(-110, SaturateRound.Saturate_Round<int8, c_double>(-110.1));

			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(-0.6));
			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(-0.5));
			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(-0.4));
			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(0.4));
			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(0.5));
			Assert.AreEqual(0, SaturateRound.Saturate_Trunc<int8, c_double>(0.6));

			Assert.AreEqual(-1, SaturateRound.Saturate_Round<int8, c_double>(-0.6));
			Assert.AreEqual(-1, SaturateRound.Saturate_Round<int8, c_double>(-0.5));
			Assert.AreEqual(0, SaturateRound.Saturate_Round<int8, c_double>(-0.4));
			Assert.AreEqual(0, SaturateRound.Saturate_Round<int8, c_double>(0.4));
			Assert.AreEqual(1, SaturateRound.Saturate_Round<int8, c_double>(0.5));
			Assert.AreEqual(1, SaturateRound.Saturate_Round<int8, c_double>(0.6));
		}
	}
}

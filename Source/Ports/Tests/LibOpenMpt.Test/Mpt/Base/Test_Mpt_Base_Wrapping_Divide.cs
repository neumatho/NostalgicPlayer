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
	public partial class Test_Mpt_Base : Test
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Base_Wrapping_Divide()
		{
			Assert.AreEqual(11, Wrapping_Divide.Wrapping_Modulo(-25, 12));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-24, 12));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(-23, 12));
			Assert.AreEqual(6, Wrapping_Divide.Wrapping_Modulo(-8, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-7, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(-6, 7));
			Assert.AreEqual(2, Wrapping_Divide.Wrapping_Modulo(-5, 7));
			Assert.AreEqual(3, Wrapping_Divide.Wrapping_Modulo(-4, 7));
			Assert.AreEqual(4, Wrapping_Divide.Wrapping_Modulo(-3, 7));
			Assert.AreEqual(5, Wrapping_Divide.Wrapping_Modulo(-2, 7));
			Assert.AreEqual(6, Wrapping_Divide.Wrapping_Modulo(-1, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(0, 12));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(0, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(1, 7));
			Assert.AreEqual(2, Wrapping_Divide.Wrapping_Modulo(2, 7));
			Assert.AreEqual(3, Wrapping_Divide.Wrapping_Modulo(3, 7));
			Assert.AreEqual(4, Wrapping_Divide.Wrapping_Modulo(4, 7));
			Assert.AreEqual(5, Wrapping_Divide.Wrapping_Modulo(5, 7));
			Assert.AreEqual(6, Wrapping_Divide.Wrapping_Modulo(6, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(7, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(8, 7));
			Assert.AreEqual(11, Wrapping_Divide.Wrapping_Modulo(23, 12));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(24, 12));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(25, 12));
			Assert.AreEqual(0x7fffffffU, Wrapping_Divide.Wrapping_Modulo(0x7fffffffU, 0x80000000U));
			Assert.AreEqual(0x7ffffffe, Wrapping_Divide.Wrapping_Modulo(0x7ffffffe, 0x7fffffff));

			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 1));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 2));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 1));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 2));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 1));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 2));

			Assert.AreEqual(0x7ffffffe, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7fffffff));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7fffffff));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7fffffff));

			Assert.AreEqual(0x7ffffffc, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7ffffffe));
			Assert.AreEqual(0x7ffffffd, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7ffffffe));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7ffffffe));

			Assert.AreEqual(0x7ffffffa, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7ffffffd));
			Assert.AreEqual(0x7ffffffb, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7ffffffd));
			Assert.AreEqual(0x7ffffffc, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7ffffffd));

			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(0, 0x7fffffff));
			Assert.AreEqual(0x7ffffffe, Wrapping_Divide.Wrapping_Modulo(-1, 0x7fffffff));
			Assert.AreEqual(0x7ffffffd, Wrapping_Divide.Wrapping_Modulo(-2, 0x7fffffff));

			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Modulo(0, 0x7ffffffe));
			Assert.AreEqual(0x7ffffffd, Wrapping_Divide.Wrapping_Modulo(-1, 0x7ffffffe));
			Assert.AreEqual(0x7ffffffc, Wrapping_Divide.Wrapping_Modulo(-2, 0x7ffffffe));

			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 1U));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 2U));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 1U));
			Assert.AreEqual(1U, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 2U));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 1U));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 2U));

			Assert.AreEqual(0xbffffffeU, Wrapping_Divide.Wrapping_Modulo(-0x40000001, 0xffffffffU));
			Assert.AreEqual(0xbfffffffU, Wrapping_Divide.Wrapping_Modulo(-0x40000000, 0xffffffffU));
			Assert.AreEqual(0xc0000000U, Wrapping_Divide.Wrapping_Modulo(-0x3fffffff, 0xffffffffU));

			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x80000000U));
			Assert.AreEqual(1U, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x80000000U));
			Assert.AreEqual(2U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x80000000U));

			Assert.AreEqual(1U, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x80000001U));
			Assert.AreEqual(2U, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x80000001U));
			Assert.AreEqual(3U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x80000001U));

			Assert.AreEqual(0x7ffffffeU, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7fffffffU));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7fffffffU));
			Assert.AreEqual(1U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7fffffffU));

			Assert.AreEqual(0x7ffffffcU, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7ffffffeU));
			Assert.AreEqual(0x7ffffffdU, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7ffffffeU));
			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7ffffffeU));

			Assert.AreEqual(0x7ffffffaU, Wrapping_Divide.Wrapping_Modulo((int32)(-0x80000000L), 0x7ffffffdU));
			Assert.AreEqual(0x7ffffffbU, Wrapping_Divide.Wrapping_Modulo(-0x7fffffff, 0x7ffffffdU));
			Assert.AreEqual(0x7ffffffcU, Wrapping_Divide.Wrapping_Modulo(-0x7ffffffe, 0x7ffffffdU));

			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(0, 0x7fffffffU));
			Assert.AreEqual(0x7ffffffeU, Wrapping_Divide.Wrapping_Modulo(-1, 0x7fffffffU));
			Assert.AreEqual(0x7ffffffdU, Wrapping_Divide.Wrapping_Modulo(-2, 0x7fffffffU));

			Assert.AreEqual(0U, Wrapping_Divide.Wrapping_Modulo(0, 0x7ffffffeU));
			Assert.AreEqual(0x7ffffffdU, Wrapping_Divide.Wrapping_Modulo(-1, 0x7ffffffeU));
			Assert.AreEqual(0x7ffffffcU, Wrapping_Divide.Wrapping_Modulo(-2, 0x7ffffffeU));

			Assert.AreEqual(-3, Wrapping_Divide.Wrapping_Divide_(-15, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-14, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-13, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-12, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-11, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-10, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-9, 7));
			Assert.AreEqual(-2, Wrapping_Divide.Wrapping_Divide_(-8, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-7, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-6, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-5, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-4, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-3, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-2, 7));
			Assert.AreEqual(-1, Wrapping_Divide.Wrapping_Divide_(-1, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(0, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(1, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(2, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(3, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(4, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(5, 7));
			Assert.AreEqual(0, Wrapping_Divide.Wrapping_Divide_(6, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(7, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(8, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(9, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(10, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(11, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(12, 7));
			Assert.AreEqual(1, Wrapping_Divide.Wrapping_Divide_(13, 7));
			Assert.AreEqual(2, Wrapping_Divide.Wrapping_Divide_(14, 7));
			Assert.AreEqual(2, Wrapping_Divide.Wrapping_Divide_(15, 7));
		}
	}
}

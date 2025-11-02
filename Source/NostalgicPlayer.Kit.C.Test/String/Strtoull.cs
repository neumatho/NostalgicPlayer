/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.String
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Strtoull : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic()
		{
			Expect_Parse_Exact("0", 10, 0, 1, false, "zero");
			Expect_Parse_Exact("123", 10, 123, 3, false, "decimal");
			Expect_Parse_Exact("   42xyz", 10, 42, 5, false, "ws stop");
			Expect_Parse_Exact("+77", 10, 77, 3, false, "plus sign");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Negative_Input_Behavior()
		{
			Expect_Parse_Exact("-42", 10, c_ulong_long.MaxValue - 41, 3, false, "negative wraps to ULONG_LONG_MAX");
			Expect_Parse_Exact("-0", 10, 0, 2, false, "-0 becomes 0");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Base0_Prefix()
		{
			Expect_Parse_Exact("0777", 0, 511, 4, false, "octal base0");
			Expect_Parse_Exact("0x2A", 0, 0x2a, 4, false, "hex base0");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Explicit_Bases()
		{
			Expect_Parse_Exact("1011", 2, 11, 4, false, "binary base2");
			Expect_Parse_Exact("77", 8, 63, 2, false, "octal base8");
			Expect_Parse_Exact("FF", 16, 255, 2, false, "hex base16");
			Expect_Parse_Exact("Zz", 36, 1295, 2, false, "base36");
			Expect_Parse_Exact("0xFF", 16, 255, 4, false, "hex with 0x, base16");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Prefix_Mismatch_Behavior()
		{
			Expect_Parse_Exact("0x10", 10, 0, 1, false, "0x in base10 stops at x");
			Expect_Parse_Exact("0x10", 16, 16, 4, false, "0x accepted in base16");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_EndPtr_Cases()
		{
			Expect_Parse_Exact("123abc", 10, 123, 3, false, "stops at alpha");
			Expect_Parse_Exact("+", 10, 0, 0, false, "lonely plus");
			Expect_Parse_Exact("-", 10, 0, 0, false, "lonely minus");
			Expect_Parse_Exact(string.Empty, 10, 0, 0, false, "empty");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Overflow()
		{
			Expect_Parse_Exact("18446744073709551616", 10, c_ulong_long.MaxValue, 20, true, "overflow just over ULONG_MAX");
			Expect_Parse_Exact("9999999999999999999999999", 10, c_ulong_long.MaxValue, 25, true, "many 9s overflow");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_Parse_Exact(string input, c_int @base, c_ulong_long expect_Val, c_long expect_End_Off, bool expect_Error, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_ulong_long got = CString.strtoull(inputPtr, out CPointer<char> end, @base, out bool error);

			Assert.AreEqual(expect_Val, got, label);
			Assert.AreEqual(expect_End_Off, end - inputPtr, label);
			Assert.AreEqual(expect_Error, error, label);
		}
		#endregion
	}
}

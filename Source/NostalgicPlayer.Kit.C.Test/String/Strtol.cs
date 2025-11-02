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
	public class Strtol : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Whitespace_Sign_EndPtr()
		{
			Expect_Parse_Exact("0", 10, 0, 1, false, "zero");
			Expect_Parse_Exact("-0", 10, 0, 2, false, "neg zero");
			Expect_Parse_Exact("   +123xyz", 10, 123, 7, false, "ws + sign + stop at alpha");
			Expect_Parse_Exact("   -42", 10, -42, 6, false, "leading ws and minus");
			Expect_No_Conv("   + 7", 10, "space between sign and digit (no conv)");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Base10_And_Partial()
		{
			Expect_Parse_Exact("123", 10, 123, 3, false, "simple dec");
			Expect_Parse_Exact("007", 10, 7, 3, false, "leading zeros");
			Expect_Parse_Exact("98bottles", 10, 98, 2, false, "partial dec");
			Expect_No_Conv("abc", 10, "letters only");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Auto_Base0_And_Prefixes()
		{
			Expect_Parse_Exact("0755", 0, 493, 4, false, "base0 octal");
			Expect_Parse_Exact("0x2A", 0, 0x2a, 4, false, "base0 hex 0x");
			Expect_Parse_Exact("0X2a", 0, 0x2a, 4, false, "base0 hex 0X");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Explicit_Bases()
		{
			Expect_Parse_Exact("0xFF", 16, 255, 4, false, "hex with 0x, base16");
			Expect_Parse_Exact("FF", 16, 255, 2, false, "hex no prefix, base16");
			Expect_Parse_Exact("0779", 8, 63, 3, false, "octal stops at 9");
			Expect_Parse_Exact("10102", 2, 10, 4, false, "binary digits only");
			Expect_Parse_Exact("Zz", 36, 1295, 2, false, "base36");
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
		public void Test_Prefix_Overflow_Underflow()
		{
			Expect_Parse_Exact("9223372036854775808", 10, c_long.MaxValue, 19, true, "overflow + (hardcoded for 64-bit long)");
			Expect_Parse_Exact("-9223372036854775809", 10, c_long.MinValue, 20, true, "underflow + (hardcoded for 64-bit long)");

			Expect_Parse_Exact("9999999999999999999999999", 10, c_long.MaxValue, 25, true, "overflow many 9s");
			Expect_Parse_Exact("-9999999999999999999999999", 10, c_long.MinValue, 26, true, "underflow many 9s");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_EndPtr_Boundary_Cases()
		{
			Expect_Parse_Exact("123abc456", 10, 123, 3, false, "stops at first alpha");
			Expect_Parse_Exact("+", 10, 0, 0, false, "lonely plus no conv");
			Expect_Parse_Exact("-", 10, 0, 0, false, "lonely minus no conv");
			Expect_Parse_Exact(string.Empty, 10, 0, 0, false, "empty no conv");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_Parse_Exact(string input, c_int @base, c_long expect_Val, c_long expect_End_Off, bool expect_Error, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_long got = CString.strtol(inputPtr, out CPointer<char> end, @base, out bool error);

			Assert.AreEqual(expect_Val, got, label);
			Assert.AreEqual(expect_End_Off, end - inputPtr, label);
			Assert.AreEqual(expect_Error, error, label);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_No_Conv(string input, c_int @base, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_long got = CString.strtol(inputPtr, out CPointer<char> end, @base, out _);

			Assert.AreEqual(0, got, label);
			Assert.IsTrue(end == inputPtr, label);
		}
		#endregion
	}
}

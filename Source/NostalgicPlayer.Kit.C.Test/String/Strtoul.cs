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
	public class Strtoul : TestStringBase
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
			Expect_Parse_Exact("-42", 10, c_ulong.MaxValue - 41, 3, false, "negative wraps to ULONG_MAX");
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
			Expect_Parse_Exact("9999999999999999999999999", 10, c_ulong.MaxValue, 25, true, "overflow +");
			Expect_Parse_Exact("18446744073709551616", 10, c_ulong.MaxValue, 20, true, "overflow just over ULONG_MAX");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_Parse_Exact(string input, c_int @base, c_ulong expect_Val, c_long expect_End_Off, bool expect_Error, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_ulong got = CString.strtoul(inputPtr, out CPointer<char> end, @base, out bool error);

			Assert.AreEqual(expect_Val, got, label);
			Assert.AreEqual(expect_End_Off, end - inputPtr, label);
			Assert.AreEqual(expect_Error, error, label);
		}
		#endregion
	}
}

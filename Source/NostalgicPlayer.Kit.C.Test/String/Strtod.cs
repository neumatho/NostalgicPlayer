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
	public class Strtod : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Decimals()
		{
			Expect_Parse_Exact("0", 0.0, 1, "zero");
			Expect_Parse_Exact("-0", -0.0, 2, "neg zero");
			Expect_Parse_Exact(" +123", 123.0, 5, "ws + int");
			Expect_Parse_Exact("42.5", 42.5, 4, "fraction");
			Expect_Parse_Exact(".75", 0.75, 3, "leading dot");
			Expect_Parse_Exact("3.", 3.0, 2, "trailing dot");
			Expect_Parse_Exact("1e3", 1000.0, 3, "exp pos");
			Expect_Parse_Exact("1e-3", 1e-3, 4, "exp neg");
			Expect_Parse_Exact("  -12.34foo", -12.34, 8, "stops at nondigit");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_EndPtr_Contract()
		{
			Expect_No_Conv("   xyz", "endptr none");

			Expect_Parse_Exact("  6.022e23mole", 6.022e23, 10, "endptr partial");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Infinity_NaN()
		{
			Expect_Parse_Exact("inf", c_double.PositiveInfinity, 3, "inf");
			Expect_Parse_Exact("+Infinity", c_double.PositiveInfinity, 9, "infinity");
			Expect_Parse_Exact("-inf", c_double.NegativeInfinity, 4, "neg inf");

			Expect_Parse_Exact("nan(abc)xyz", c_double.NaN, 8, "nan(payload)");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Hex_Floats()
		{
			Expect_Parse_Exact("0x1.8p+1", 3.0, 8, "hex 1");
			Expect_Parse_Exact("0x1p-4", 0.0625, 6, "hex 2");
			Expect_Parse_Exact("-0x1.0p+10", -1024.0, 10, "hex 3");
			Expect_Parse_Exact("0x", 0.0, 1, "incomplete hex stops after '0'");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Overflow_Underflow()
		{
			Expect_Parse_Exact("1e309", c_double.PositiveInfinity, 5, "overflow +");
			Expect_Parse_Exact("-1e309", c_double.NegativeInfinity, 6, "overflow -");

			Expect_Parse_Exact("1e-5000", 0.0, 7, "underflow +0");

			CPointer<char> input = "-1e-5000".ToCharPointer();
			c_double v = CString.strtod(input, out CPointer<char> end);
			Assert.AreEqual(end, input + 8);
			Assert.AreEqual(0.0, v);
			Assert.AreNotEqual(0, CMath.signbit(v));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Negative_Zero_Direct()
		{
			CPointer<char> input = "-0.0".ToCharPointer();
			c_double v = CString.strtod(input, out _);
			Assert.AreEqual(0.0, v);
			Assert.AreNotEqual(0, CMath.signbit(v));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Weird_Inputs()
		{
			Expect_No_Conv(string.Empty, "empty");
			Expect_No_Conv("+", "plus only");
			Expect_No_Conv("-", "minus only");
			Expect_No_Conv(".e10", "dot then exp");
			Expect_No_Conv("--1", "double minus");

			Expect_Parse_Exact("+.5.6", 0.5, 3, "stop after second dot");
			Expect_Parse_Exact("  7e", 7.0, 3, "exp with no digits");
			Expect_Parse_Exact("  .", 0.0, 0, "lonely dot stops");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Ulp_Sensitive_Regulars()
		{
			Expect_Parse_Exact("1.25", 1.25, 4, "1.25");
			Expect_Parse_Exact("0.1", 0.1, 3, "0.1");
			Expect_Parse_Exact("2.2250738585072014e-308", 2.2250738585072014e-308, 23, "DBL_MIN-ish");
			Expect_Parse_Exact("1.7976931348623157e308", 1.7976931348623157e308, 22, "DBL_MAX-ish");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Same_Sign_Zero(c_double a, c_double b)
		{
			return (a == 0.0) && (b == 0.0) && (CMath.signbit(a) == CMath.signbit(b));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Nearly_Equal(c_double a, c_double b)
		{
			if (CMath.isnan(a) && CMath.isnan(b))
				return true;

			if (CMath.isinf(a) && CMath.isinf(b))
				return true;

			if (Same_Sign_Zero(a, b))
				return true;

			c_double diff = CMath.fabs(a - b);
			c_double scale = Math.Max(CMath.fabs(a) + CMath.fabs(b), 1.0);

			return diff <= 1e-15 * scale;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_Parse_Exact(string input, c_double expect_Val, c_long expect_End_Off, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_double got = CString.strtod(inputPtr, out CPointer<char> end);

			Assert.IsTrue(Nearly_Equal(got, expect_Val), label);

			c_long off = end - inputPtr;
			Assert.AreEqual(expect_End_Off, off, label);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_No_Conv(string input, string label)
		{
			CPointer<char> inputPtr = input.ToCharPointer();

			c_double got = CString.strtod(inputPtr, out CPointer<char> end);

			Assert.AreEqual(0.0, got, label);
			Assert.IsTrue(end == inputPtr, label);
		}
		#endregion
	}
}

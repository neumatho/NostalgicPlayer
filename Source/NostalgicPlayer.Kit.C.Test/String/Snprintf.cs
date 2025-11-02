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
	public class Snprintf : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_And_Literals()
		{
			Test_Sizes(string.Empty, string.Empty);
			Test_Sizes("hello", "hello");
			Test_Sizes("100% done", "100%% done");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_String_Basic()
		{
			Test_Sizes("abc", "%s", "abc");
			Test_Sizes(string.Empty, "%.0s", "abc");
			Test_Sizes("ab", "%.2s", "abcd");
			Test_Sizes("abc       ", "%-10.3s", "abcdef");
			Test_Sizes("       abc", "%10.3s", "abcdef");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Char_And_Percent()
		{
			Test_Sizes("A", "%c", 'A');
			Test_Sizes("%", "%%");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Signed_Integers()
		{
			Test_Sizes("0", "%d", 0);
			Test_Sizes("-1", "%d", -1);
			Test_Sizes("42", "%i", 42);
			Test_Sizes("-2147483648", "%d", c_int.MinValue);
			Test_Sizes("2147483647", "%d", c_int.MaxValue);
			Test_Sizes("+7", "%+d", 7);
			Test_Sizes(" 7", "% d", 7);
			Test_Sizes("-000042", "%07d", -42);
			Test_Sizes("00042", "%05d", 42);
			Test_Sizes(string.Empty, "%.0d", 0);
			Test_Sizes("   ", "%3.0d", 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Unsigned_And_Bases()
		{
			Test_Sizes("10", "%u", 10U);
			Test_Sizes("ff", "%x", 255U);
			Test_Sizes("FF", "%X", 255);
			Test_Sizes("377", "%o", 255U);
			Test_Sizes("0xff", "%#x", 255U);
			Test_Sizes("0377", "%#o", 255U);
			Test_Sizes("0", "%#o", 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Width_Alignment()
		{
			Test_Sizes("   42", "%5u", 42U);
			Test_Sizes("42   ", "%-5u", 42U);
			Test_Sizes("00042", "%05u", 42U);
			Test_Sizes("   -7", "%5d", -7);
			Test_Sizes("-0007", "%05d", -7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Length_Modifiers_Basic()
		{
			Test_Sizes("42", "%hd", (c_short)42);
			Test_Sizes("42", "%ld", (c_long)42);
			Test_Sizes("42", "%lld", (c_long_long)42);
			Test_Sizes("65535", "%hu", (c_ushort)0xffff);
			Test_Sizes("4294967295", "%lu", 0xffffffffUL);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Floats()
		{
			CPointer<char> mine = new CPointer<char>(256);

			c_double[] vals = [ 0.0, -0.0, 1.25, 1234.5, 1.0 / 3.0, CMath.DBL_MAX / 2, CMath.DBL_MIN * 2 ];
			string[] fmts = [ "%f", "%.2f", "%e", "%.g", "%10.3f", "%-#12.0f" ];

			string[][] expected =
			{
				[ "0.000000", "0.00", "0.000000e+00", "0", "     0.000", "0.          " ],
				[ "-0.000000", "-0.00", "-0.000000e+00", "-0", "    -0.000", "-0.         " ],
				[ "1.250000", "1.25", "1.250000e+00", "1", "     1.250", "1.          " ],
				[ "1234.500000", "1234.50", "1.234500e+03", "1e+03", "  1234.500", "1234.       " ],
				[ "0.333333", "0.33", "3.333333e-01", "0.3", "     0.333", "0.          " ],
				[
					"898846567431157854072637118658521783990352837629224982994587384015786303900142693802947793163834390857702294767571912321171606634447320913842337733517687584930249552882756410381227450451946644720379342542275669711522916184516114740829042796660616741373989",
					"898846567431157854072637118658521783990352837629224982994587384015786303900142693802947793163834390857702294767571912321171606634447320913842337733517687584930249552882756410381227450451946644720379342542275669711522916184516114740829042796660616741373989",
					"8.988466e+307",
					"9e+307",
					"898846567431157854072637118658521783990352837629224982994587384015786303900142693802947793163834390857702294767571912321171606634447320913842337733517687584930249552882756410381227450451946644720379342542275669711522916184516114740829042796660616741373989",
					"898846567431157854072637118658521783990352837629224982994587384015786303900142693802947793163834390857702294767571912321171606634447320913842337733517687584930249552882756410381227450451946644720379342542275669711522916184516114740829042796660616741373989"
				],
				[ "0.000000", "0.00", "4.450148e-308", "4e-308", "     0.000", "0.          " ]
			};

			for (c_int i = 0; i < vals.Length; i++)
			{
				for (c_int j = 0; j < fmts.Length; j++)
				{
					CString.snprintf(mine, (size_t)mine.Length, fmts[j], vals[i]);
					Assert.IsTrue(Compare(mine, expected[i][j].ToCharPointer()), $"{i}, {j} - '{expected[i][j]}' - '{mine.ToString()}'");
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Size_Zero_And_One_Semantics()
		{
			// Size == 0: no writing, but return right length
			{
				CPointer<char> dummy = "X".ToCharPointer();

				c_int ret = CString.snprintf(dummy, 0, "abcdef");
				Assert.AreEqual(6, ret);
				Assert.AreEqual('X', dummy[0]);
			}

			// Size == 1: Always only '\0' written, but return right length
			{
				CPointer<char> b = new CPointer<char>(2);
				b[1] = 'X';

				c_int ret = CString.snprintf(b, 1, "hi");
				Assert.AreEqual(2, ret);
				Assert.AreEqual('\0', b[0]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Hard_Truncation_Edges()
		{
			CPointer<char> msg = "abcdefghijklmnopqrstuvwxyz".ToCharPointer();
			size_t l = CString.strlen(msg);

			for (size_t sz = 2; sz <= 8; sz++)
			{
				CPointer<char> buf = new CPointer<char>((c_int)sz);
				Fill(buf, (c_int)sz, '\xaa');

				c_int ret = CString.snprintf(buf, sz, "%s", msg);

				Assert.AreEqual(l, (size_t)ret);

				size_t keep = sz - 1;
				Assert.IsTrue(Compare(buf, msg, (c_int)keep));
				Assert.AreEqual('\0', buf[keep]);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Run same test with different buffer sizes
		/// </summary>
		/********************************************************************/
		private void Test_Sizes(string expect_Str, string fmt, params object[] args)
		{
			size_t len = (size_t)expect_Str.Length;

			Test_One_Size(0, expect_Str, fmt, args);
			Test_One_Size(1, expect_Str, fmt, args);
			Test_One_Size(len, expect_Str, fmt, args);
			Test_One_Size(len + 1, expect_Str, fmt, args);
			Test_One_Size(len + 10, expect_Str, fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_One_Size(size_t size, string expect_Str, string fmt, object[] args)
		{
			CPointer<char> expPtr = expect_Str.ToCharPointer();
			size_t expLen = CString.strlen(expPtr);
			char guard = '\xcd';
			size_t leftPad = 5, rightPad = 7;
			size_t bufCap = size;
			size_t arenaSize = leftPad + (bufCap != 0 ? bufCap : 1) + rightPad;

			CPointer<char> arena = new CPointer<char>((c_int)arenaSize);
			Fill(arena, (c_int)arenaSize, guard);

			CPointer<char> buf = arena + leftPad;

			c_int ret = CString.snprintf(buf, size, fmt, args);

			Assert.IsGreaterThanOrEqualTo(0, ret);
			Assert.AreEqual(expLen, (size_t)ret);

			if (size > 0)
			{
				size_t want = (expLen < (size - 1)) ? expLen : size - 1;

				Assert.IsTrue(Compare(buf, expPtr, (c_int)want));
				Assert.AreEqual('\0', buf[want]);

				if ((size >= 2) && ((want + 1) < size))
					Assert.AreEqual(guard, buf[want + 1]);
			}
			else
				Assert.AreEqual(guard, arena[leftPad]);

			Assert.AreEqual(guard, arena[arenaSize - 1]);
		}
		#endregion
	}
}

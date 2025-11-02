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
	public class Strncmp : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_N_Zero()
		{
			Assert.AreEqual(0, CString.strncmp("a".ToCharPointer(), "b", 0));
			Assert.AreEqual(0, CString.strncmp(string.Empty.ToCharPointer(), string.Empty, 0));
			Assert.AreEqual(0, CString.strncmp("abc".ToCharPointer(), string.Empty, 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Equal_Order_Small_N()
		{
			Assert.AreEqual(0, CString.strncmp("same".ToCharPointer(), "same", 1));
			Assert.AreEqual(0, CString.strncmp("same".ToCharPointer(), "same", 1000));

			Assert.AreEqual(0, CString.strncmp("abc".ToCharPointer(), "abd", 1));
			Assert.AreEqual(0, CString.strncmp("abc".ToCharPointer(), "abd", 2));

			Assert.IsLessThan(0, CString.strncmp("abc".ToCharPointer(), "abd", 3));
			Assert.IsGreaterThan(0, CString.strncmp("abd".ToCharPointer(), "abc", 3));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Prefix_And_Terminator_Vs_N()
		{
			Assert.AreEqual(0, CString.strncmp("abc".ToCharPointer(), "abcd", 3));
			Assert.IsLessThan(0, CString.strncmp("abc".ToCharPointer(), "abcd", 4));

			Assert.AreEqual(0, CString.strncmp("abcd".ToCharPointer(), "abc", 3));
			Assert.IsGreaterThan(0, CString.strncmp("abcd".ToCharPointer(), "abc", 4));

			Assert.AreEqual(0, CString.strncmp("abc".ToCharPointer(), "abc", 1000));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Embedded_Null()
		{
			CPointer<char> s1 = "ab\0xy\0".ToCharPointer();
			CPointer<char> s2 = "ab\0zz\0".ToCharPointer();

			Assert.AreEqual(0, CString.strncmp(s1, s2, 5));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Long_And_Late_Diff_Vs_N()
		{
			const int n = 6000;

			CPointer<char> a = new CPointer<char>(n + 1);
			CPointer<char> b = new CPointer<char>(n + 1);

			Fill(a, n, 'x');
			a[n] = '\0';

			CMemory.MemCpy(b, a, n + 1);
			a[n - 1] = 'y';

			Assert.IsGreaterThan(0, CString.strncmp(a, b, n));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Symmetry_And_Reflexive()
		{
			CPointer<char> x = "kitten".ToCharPointer();
			CPointer<char> y = "sitting".ToCharPointer();

			for (size_t n = 0; n < 8; n++)
			{
				c_int r1 = CString.strncmp(x, y, n);
				c_int r2 = CString.strncmp(y, x, n);

				Assert.AreEqual(-r2, r1);
				Assert.AreEqual(0, CString.strncmp(x, x, n));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Read_Past_End()
		{
			CPointer<char> s1 = new CPointer<char>(3);
			s1[0] = 'a';
			s1[1] = 'b';
			s1[2] = 'c';

			CPointer<char> s2 = "abc".ToCharPointer();

			Assert.AreEqual(0, CString.strncmp(s1, s2, 1000));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Randomized()
		{
			Random rand = new Random(2025);

			for (c_int t = 0; t < 4000; t++)
			{
				size_t len1 = (size_t)(rand.Next(256));
				size_t len2 = (size_t)(rand.Next(256));

				CPointer<char> a = new CPointer<char>((c_int)len1 + 1);
				CPointer<char> b = new CPointer<char>((c_int)len2 + 1);

				for (size_t i = 0; i < len1; i++)
					a[i] = (char)rand.Next(1, 254);

				for (size_t i = 0; i < len2; i++)
					b[i] = (char)rand.Next(1, 254);

				a[len1] = '\0';
				b[len2] = '\0';

				size_t maxLen = Math.Max(len1, len2);
				size_t n = (size_t)(rand.Next((int)(maxLen + 20)));

				c_int result = CString.strncmp(a, b, n);
				result = result < 0 ? -1 : (result > 0 ? 1 : 0);

				string c1 = a.ToString();
				c1 = c1.Substring(0, Math.Min((int)n, c1.Length));

				string c2 = b.ToString();
				c2 = c2.Substring(0, Math.Min((int)n, c2.Length));

				c_int expected = string.Compare(c1, c2, StringComparison.Ordinal);
				expected = expected < 0 ? -1 : (expected > 0 ? 1 : 0);

				Assert.AreEqual(expected, result);
			}
		}
	}
}

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
	public class Strcmp : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equal_And_Basic_Order()
		{
			Assert.AreEqual(0, CString.strcmp(CString.Empty, string.Empty));
			Assert.IsLessThan(0, CString.strcmp(CString.Empty, "a"));
			Assert.IsGreaterThan(0, CString.strcmp("a".ToCharPointer(), string.Empty));
			Assert.AreEqual(0, CString.strcmp("same".ToCharPointer(), "same"));
			Assert.IsLessThan(0, CString.strcmp("abc".ToCharPointer(), "abd"));
			Assert.IsGreaterThan(0, CString.strcmp("abd".ToCharPointer(), "abc"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Prefix_Cases()
		{
			Assert.IsLessThan(0, CString.strcmp("abc".ToCharPointer(), "abcd"));
			Assert.IsGreaterThan(0, CString.strcmp("abcd".ToCharPointer(), "abc"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Embedded_Null()
		{
			CPointer<char> s1 = "a\0x\0".ToCharPointer();
			CPointer<char> s2 = "a\0y\0".ToCharPointer();

			Assert.AreEqual(0, CString.strcmp(s1, s2));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Long_And_Late_Diff()
		{
			const int n = 5000;

			CPointer<char> a = new CPointer<char>(n + 1);
			CPointer<char> b = new CPointer<char>(n + 1);

			Fill(a, n, 'x');
			a[n] = '\0';

			CMemory.memcpy(b, a, n + 1);
			a[n - 1] = 'y';

			Assert.IsGreaterThan(0, CString.strcmp(a, b));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Symmetry_And_Zero()
		{
			CPointer<char> x = "kitten".ToCharPointer();
			CPointer<char> y = "sitting".ToCharPointer();

			c_int r1 = CString.strcmp(x, y);
			c_int r2 = CString.strcmp(y, x);

			Assert.AreEqual(-r2, r1);
			Assert.AreEqual(0, CString.strcmp(x, x));
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

			Assert.AreEqual(0, CString.strcmp(s1, s2));
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

			for (c_int n = 0; n < 3000; n++)
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

				if (len1 != 0 && rand.Next(3) == 0)
					a[rand.Next((int)len1)] = '\0';

				if (len2 != 0 && rand.Next(3) == 0)
					b[rand.Next((int)len2)] = '\0';

				c_int result = CString.strcmp(a, b);
				result = result < 0 ? -1 : (result > 0 ? 1 : 0);

				string c1 = Array.IndexOf(a.GetOriginalArray(), '\0') == -1 ? a.ToString() : new string(a.GetOriginalArray(), a.Offset, Array.IndexOf(a.GetOriginalArray(), '\0') - a.Offset);
				string c2 = Array.IndexOf(b.GetOriginalArray(), '\0') == -1 ? b.ToString() : new string(b.GetOriginalArray(), b.Offset, Array.IndexOf(b.GetOriginalArray(), '\0') - b.Offset);
				c_int expected = string.Compare(c1, c2, StringComparison.Ordinal);
				expected = expected < 0 ? -1 : (expected > 0 ? 1 : 0);

				Assert.AreEqual(expected, result);
			}
		}
	}
}

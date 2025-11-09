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
	public class Strstr : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Hits()
		{
			CPointer<char> s = "hello world".ToCharPointer();

			Assert.AreEqual(s, CString.strstr(s, "h"));
			Assert.AreEqual(s, CString.strstr(s, "he"));
			Assert.AreEqual(s + 6, CString.strstr(s, "world"));
			Assert.AreEqual(s + 4, CString.strstr(s, "o wo"));

			s = "bananana".ToCharPointer();

			Assert.AreEqual(s + 1, CString.strstr(s, "ana"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Absent_And_Edges()
		{
			CPointer<char> s = "abcdef".ToCharPointer();

			Assert.IsTrue(CString.strstr(s, "gh").IsNull);
			Assert.IsTrue(CString.strstr(s, "abcdefg").IsNull);
			Assert.IsTrue(CString.strstr(string.Empty.ToCharPointer(), "a").IsNull);

			Assert.AreEqual(s, CString.strstr(s, string.Empty));

			s = string.Empty.ToCharPointer();
			Assert.AreEqual(s, CString.strstr(s, string.Empty));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stops_At_First_Nul()
		{
			CPointer<char> hs = "abc\0xyz".ToCharPointer();

			Assert.AreEqual(hs + 2, CString.strstr(hs, "c"));
			Assert.IsTrue(CString.strstr(hs, "cx").IsNull);
			Assert.IsTrue(CString.strstr(hs, "y").IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_And_Boundary()
		{
			const c_int n = 100000;

			CPointer<char> h = new CPointer<char>(n + 1);
			Fill(h, n, 'a');
			h[n] = '\0';

			h[n - 3] = 'b';
			h[n - 2] = 'c';
			h[n - 1] = 'd';

			Assert.AreEqual(h + n - 3, CString.strstr(h, "bcd"));

			CPointer<char> needle = new CPointer<char>(64);
			Fill(needle, 63, 'a');
			needle[63] = '\0';

			Assert.AreEqual(h, CString.strstr(h, needle));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_Needle_Positions()
		{
			CPointer<char> s = "abc".ToCharPointer();

			Assert.AreEqual(s, CString.strstr(s, string.Empty));
			Assert.AreEqual(s + 2, CString.strstr(s + 2, string.Empty));

			s = string.Empty.ToCharPointer();
			Assert.AreEqual(s, CString.strstr(s, string.Empty));
		}
	}
}

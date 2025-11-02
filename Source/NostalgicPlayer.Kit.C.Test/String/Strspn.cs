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
	public class Strspn : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic()
		{
			Assert.AreEqual(0U, CString.strspn(string.Empty.ToCharPointer(), string.Empty), "empty s, empty accept");
			Assert.AreEqual(0U, CString.strspn(string.Empty.ToCharPointer(), "abc"), "empty s");
			Assert.AreEqual(0U, CString.strspn("abc".ToCharPointer(), string.Empty), "empty accept");

			Assert.AreEqual(4U, CString.strspn("aaaa".ToCharPointer(), "a"), "all same char");
			Assert.AreEqual(6U, CString.strspn("banana".ToCharPointer(), "abn"), "all allowed");
			Assert.AreEqual(3U, CString.strspn("foobar".ToCharPointer(), "of"), "stops at first disallow");
			Assert.AreEqual(3U, CString.strspn("abcxyz".ToCharPointer(), "abc"), "prefix only");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Accept_Duplicates()
		{
			Assert.AreEqual(6U, CString.strspn("abcabc".ToCharPointer(), "aabbbccc"), "accept duplicates");
			Assert.AreEqual(3U, CString.strspn("xyz".ToCharPointer(), "zzzyyxx"), "accept duplicates 2");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Contains_Null_Semantics()
		{
			Assert.AreEqual(3U, CString.strspn("abc".ToCharPointer(), "ab\0c\0"), "accept contains null");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stops_At_Null_In_S()
		{
			Assert.AreEqual(3U, CString.strspn("abc\0ab".ToCharPointer(), "abc"), "stop at first null in s");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Input()
		{
			const c_int n = 50000;

			CPointer<char> s = new CPointer<char>(n + 2);

			Fill(s, n, 'a');
			s[n] = 'X';
			s[n + 1] = '\0';

			Assert.AreEqual((size_t)n, CString.strspn(s, "a"), "large run of 'a'");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Read_Past_End()
		{
			CPointer<char> s = new CPointer<char>(3);
			s[0] = 'a';
			s[1] = 'b';
			s[2] = 'c';

			Assert.AreEqual(3U, CString.strspn(s, "abc"));
		}
	}
}

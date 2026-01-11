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
	public class Strcspn : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Matches()
		{
			Assert.AreEqual(5U, CString.strcspn("hello".ToCharPointer(), "x".ToCharPointer()));
			Assert.AreEqual(0U, CString.strcspn("hello".ToCharPointer(), "h".ToCharPointer()));
			Assert.AreEqual(4U, CString.strcspn("hello".ToCharPointer(), "o".ToCharPointer()));
			Assert.AreEqual(1U, CString.strcspn("hello".ToCharPointer(), "le".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Match()
		{
			Assert.AreEqual(6U, CString.strcspn("abcdef".ToCharPointer(), "xyz".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_First_Char_Match()
		{
			Assert.AreEqual(0U, CString.strcspn("abcdef".ToCharPointer(), "a".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Last_Char_Match()
		{
			Assert.AreEqual(5U, CString.strcspn("abcdef".ToCharPointer(), "f".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_Second_String()
		{
			Assert.AreEqual(6U, CString.strcspn("abcdef".ToCharPointer(), CString.Empty));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_First_String()
		{
			Assert.AreEqual(0U, CString.strcspn(CString.Empty, "abc".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Both_Empty()
		{
			Assert.AreEqual(0U, CString.strcspn(CString.Empty, CString.Empty));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Repeated_Chars()
		{
			Assert.AreEqual(0U, CString.strcspn("aaaaaa".ToCharPointer(), "a".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Special_Chars()
		{
			Assert.AreEqual(3U, CString.strcspn("abc\n123".ToCharPointer(), "\n".ToCharPointer()));
			Assert.AreEqual(3U, CString.strcspn("abc\t123".ToCharPointer(), "\t".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Numeric()
		{
			Assert.AreEqual(2U, CString.strcspn("123456".ToCharPointer(), "3".ToCharPointer()));
			Assert.AreEqual(6U, CString.strcspn("123456".ToCharPointer(), "78".ToCharPointer()));
		}
	}
}

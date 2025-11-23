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
	public class Strchr : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Hits()
		{
			CPointer<char> s = "hello".ToCharPointer();

			Assert.AreEqual(s, CString.strchr(s, 'h'));
			Assert.AreEqual(s + 1, CString.strchr(s, 'e'));
			Assert.AreEqual(s + 2, CString.strchr(s, 'l'));
			Assert.AreEqual(s + 4, CString.strchr(s, 'o'));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_First_Occurrence()
		{
			CPointer<char> s = "banana".ToCharPointer();

			Assert.AreEqual(s + 1, CString.strchr(s, 'a'));
			Assert.AreEqual(s + 2, CString.strchr(s, 'n'));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_First_Absent()
		{
			CPointer<char> s = "hello".ToCharPointer();

			CPointer<char> p = CString.strchr(s, 'x');
			Assert.IsTrue(p.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Search_Null()
		{
			CPointer<char> s = "abc".ToCharPointer();
			Assert.AreEqual(s + 3, CString.strchr(s, '\0'));

			s = CString.Empty;
			Assert.AreEqual(s, CString.strchr(s, '\0'));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stops_At_Null()
		{
			CPointer<char> s = "abc\0xyz".ToCharPointer();

			CPointer<char> p = CString.strchr(s, 'y');
			Assert.IsTrue(p.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_String()
		{
			CPointer<char> s = CString.Empty;

			CPointer<char> p = CString.strchr(s, 'A');
			Assert.IsTrue(p.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Return_Is_Inside_String()
		{
			CPointer<char> s = "abcdef".ToCharPointer();

			CPointer<char> p = CString.strchr(s, 'd');
			Assert.IsTrue(p.IsNotNull);

			size_t idx = (size_t)(p - s);
			Assert.AreEqual(3U, idx);
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

			for (c_int n = 0; n < 2000; n++)
			{
				size_t len = (size_t)(rand.Next(256));

				CPointer<char> s = new CPointer<char>((c_int)len + 1);

				for (size_t i = 0; i < len; i++)
					s[i] = (char)rand.Next(1, 254);

				s[len] = '\0';

				c_int pick = rand.Next(270);
				char ch = pick == 0 ? '\0' : (char)rand.Next(256);

				CPointer<char> result = CString.strchr(s, ch);
				c_int index = ch == '\0' ? s.Length - 1 : s.ToString().IndexOf(ch);

				if (index == -1)
					Assert.IsTrue(result.IsNull);
				else
					Assert.AreEqual(index, result - s);
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
			CPointer<char> s = new CPointer<char>(3);
			s[0] = 'a';
			s[1] = 'b';
			s[2] = 'c';

			CPointer<char> p = CString.strchr(s, 'y');
			Assert.IsTrue(p.IsNull);
		}
	}
}

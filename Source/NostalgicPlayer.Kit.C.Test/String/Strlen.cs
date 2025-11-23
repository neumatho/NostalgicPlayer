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
	public class Strlen : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic()
		{
			Assert.AreEqual(0U, CString.strlen(CString.Empty));
			Assert.AreEqual(1U, CString.strlen("a".ToCharPointer()));
			Assert.AreEqual(5U, CString.strlen("hello".ToCharPointer()));
			Assert.AreEqual(10U, CString.strlen("1234567890".ToCharPointer()));
			Assert.AreEqual(6U, CString.strlen("ÆØÅæøå".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Embedded_Nulls()
		{
			CPointer<char> buf = "abs\0def".ToCharPointer();

			Assert.AreEqual(3U, CString.strlen(buf));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Long_String()
		{
			CPointer<char> big = new CPointer<char>(100000);
			Fill(big, big.Length - 1, 'x');
			big[^1] = '\0';

			Assert.AreEqual((size_t)big.Length - 1, CString.strlen(big));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Randomized()
		{
			Random rand = new Random(1234);

			for (c_int n = 0; n < 1000; n++)
			{
				size_t len = (size_t)(rand.Next(256));

				CPointer<char> s = new CPointer<char>((c_int)len + 1);

				for (size_t i = 0; i < len; i++)
					s[i] = (char)rand.Next(32, 127);

				s[len] = '\0';

				Assert.AreEqual(len, CString.strlen(s));
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

			Assert.AreEqual(3U, CString.strlen(s));
		}
	}
}

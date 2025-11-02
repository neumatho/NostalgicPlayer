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
	public class Strcpy : TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Copy()
		{
			CPointer<char> dst = new CPointer<char>(32);
			Fill(dst, dst.Length, '\xcd');

			string src = "hello";
			CPointer<char> result = CString.strcpy(dst, src);

			Assert.AreEqual(dst, result);
			Assert.IsTrue(Compare(dst, src.ToCharPointer()));
			Assert.AreEqual('\0', dst[5]);
			Assert.AreEqual('\xcd', dst[6]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_Source()
		{
			CPointer<char> dst = new CPointer<char>(8);
			Fill(dst, dst.Length, '\xaa');

			string src = string.Empty;
			CPointer<char> result = CString.strcpy(dst, src);

			Assert.AreEqual(dst, result);
			Assert.AreEqual('\0', dst[0]);
			Assert.AreEqual('\xaa', dst[1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Single_Char()
		{
			CPointer<char> dst = new CPointer<char>(8);
			Fill(dst, dst.Length, '\xef');

			string src = "A";
			CPointer<char> result = CString.strcpy(dst, src);

			Assert.AreEqual(dst, result);
			Assert.AreEqual('A', dst[0]);
			Assert.AreEqual('\0', dst[1]);
			Assert.AreEqual('\xef', dst[2]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Long_String()
		{
			CPointer<char> dst = new CPointer<char>(256);
			Fill(dst, dst.Length, '\x7b');

			CPointer<char> src = new CPointer<char>(200);
			for (int i = 0; i < 199; i++)
				src[i] = (char)((i % 26) + 'a');

			src[199] = '\0';

			CPointer<char> result = CString.strcpy(dst, src);

			Assert.AreEqual(dst, result);
			Assert.IsTrue(Compare(dst, src));
			Assert.AreEqual('\x7b', dst[200]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Self_Copy_Same_Pointer()
		{
			CPointer<char> buf = "self".ToCharPointer();

			CPointer<char> result = CString.strcpy(buf, buf);

			Assert.AreEqual(buf, result);
			Assert.IsTrue(Compare(buf, "self".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Multiple_Overwrites()
		{
			CPointer<char> dst = new CPointer<char>(32);
			Fill(dst, dst.Length, '\x33');

			CPointer<char> result = CString.strcpy(dst, "first");
			Assert.IsTrue(Compare(result, "first".ToCharPointer()));

			result = CString.strcpy(dst, "second");
			Assert.IsTrue(Compare(result, "second".ToCharPointer()));

			result = CString.strcpy(dst, "");
			Assert.IsTrue(Compare(result, "".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Dst_Buffer_Exact_Fit()
		{
			CPointer<char> dst = new CPointer<char>(6);
			Fill(dst, dst.Length, '\x66');

			string src = "12345";
			CPointer<char> result = CString.strcpy(dst, src);

			Assert.IsTrue(Compare(result, "12345".ToCharPointer()));
		}



		/********************************************************************/
		/// <summary>
		/// This test only checks if strcpy crashes or not. The standard
		/// does not define what should happen in this case
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Overlap_Undefined_Behaviour()
		{
			CPointer<char> buf = "abcdefghijkl".ToCharPointer();

			CPointer<char> dst = buf + 2;
			CPointer<char> src = buf;

			CString.strcpy(dst, src);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Write_Past_End()
		{
			uint8_t canaryL = 0xab, canaryR = 0xcd;

			CPointer<char> src = "tenchars!".ToCharPointer();
			c_int need = src.Length;

			CPointer<char> areana = new CPointer<char>(need + 2);
			areana[0] = (char)canaryL;
			areana[need + 1] = (char)canaryR;

			CPointer<char> dst = areana + 1;

			CString.strcpy(dst, src);

			Assert.IsTrue(Compare(dst, src));
			Assert.AreEqual((char)canaryL, areana[0]);
			Assert.AreEqual((char)canaryR, areana[need + 1]);
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
				size_t len = (size_t)(rand.Next(64));

				CPointer<char> s = new CPointer<char>((c_int)len + 1);

				for (size_t i = 0; i < len; i++)
					s[i] = (char)rand.Next(32, 127);

				s[len] = '\0';

				CPointer<char> dst = new CPointer<char>((c_int)len + 8);
				Fill(dst, dst.Length, '\x99');

				CPointer<char> result = CString.strcpy(dst, s);

				Assert.AreEqual(dst, result);
				Assert.IsTrue(Compare(dst, s));
				Assert.AreEqual('\x99', dst[^1]);
			}
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;

namespace NostalgicPlayer.Kit.C.Test.Time
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Strftime : TestTimeBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Calendar()
		{
			tm t = Tm_Make(2023, 3, 14, 15, 9, 26, 2, 72, -1);

			Test_Sizes("Y-m-d", t, "%Y-%m-%d", "2023-03-14");
			Test_Sizes("H:M:S", t, "%H:%M:%S", "15:09:26");
			Test_Sizes("a,b", t, "%a %b %d", "Tue Mar 14");
			Test_Sizes("A,B", t, "%A %B %d", "Tuesday March 14");
			Test_Sizes("w/p", t, "%w %p", "2 PM");
			Test_Sizes("j", t, "%j", "073");
			Test_Sizes("F", t, "%F", "2023-03-14");
			Test_Sizes("T", t, "%T", "15:09:26");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Week_And_Julian_Edges()
		{
			tm t = Tm_Make(2023, 1, 1, 10, 0, 0, 0, 0, -1);

			Test_Sizes("%j", t, "%j", "001");
			Test_Sizes("%U", t, "%U", "01");
			Test_Sizes("%W", t, "%W", "00");
			Test_Sizes("%w", t, "%w", "0");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Percent_And_Literals()
		{
			tm t = Tm_Make(1999, 12, 31, 23, 59, 59, 5, 364, 0);

			Test_Sizes("literal %", t, "100%% OK", "100% OK");
			Test_Sizes("mix", t, "Y=%Y m=%m d=%d", "Y=1999 m=12 d=31");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Null_Termination_And_Truncation_Hints()
		{
			tm t = Tm_Make(2020, 1, 2, 3, 4, 5, 4, 1, -1);
			string fmt = "%A %B %d %Y";
			CPointer<char> exp = "Thursday January 02 2020".ToCharPointer();
			size_t l = CString.strlen(exp);

			{
				CPointer<char> buf = new CPointer<char>(l + 1);
				CMemory.memset(buf, '\xaa', l + 1);

				size_t r = CTime.strftime(buf, l + 1, fmt, t);
				Expect_Eq(buf, r, exp, "full fit NUL");

				Assert.AreEqual('\0', buf[l]);
			}

			{
				CPointer<char> buf = new CPointer<char>(l);
				CMemory.memset(buf, '\xaa', l);

				size_t r = CTime.strftime(buf, l, fmt, t);
				Assert.AreEqual(0U, r);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Sizes(string label, tm t, string fmt, string exp)
		{
			CPointer<char> expPtr = exp.ToCharPointer();
			size_t l = CString.strlen(expPtr);

			{
				CPointer<char> x = new CPointer<char>();
				size_t r = CTime.strftime(x, 0, fmt, t);
				Assert.AreEqual(0U, r);
			}

			{
				CPointer<char> b = new CPointer<char>(1);
				size_t r = CTime.strftime(b, 1, fmt, t);
				Assert.AreEqual(0U, r);
			}

			{
				CPointer<char> b = new CPointer<char>(l + 1);
				CMemory.memset(b, '\xa5', l + 1);

				size_t r = CTime.strftime(b, l + 1, fmt, t);
				Expect_Eq(b, r, expPtr, label);

				Assert.AreEqual('\0', b[l]);
			}

			{
				CPointer<char> b = new CPointer<char>(l != 0 ? l : 1);
				CMemory.memset(b, '\xcc', l != 0 ? l : 1);

				size_t r = CTime.strftime(b, l, fmt, t);
				Assert.AreEqual(0U, r);
			}

			{
				CPointer<char> b = new CPointer<char>(l + 10);
				CMemory.memset(b, '\xcc', l + 10);

				size_t r = CTime.strftime(b, l + 10, fmt, t);
				Expect_Eq(b, r, expPtr, label);

				Assert.AreEqual('\0', b[l]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Expect_Eq(CPointer<char> got, size_t ret, CPointer<char> exp, string label)
		{
			size_t l = CString.strlen(exp);

			Assert.AreEqual(ret, l, label);
			Assert.AreEqual(0, CString.strcmp(got, exp), label);
		}
		#endregion
	}
}

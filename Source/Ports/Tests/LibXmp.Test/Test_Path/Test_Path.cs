/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Path
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public partial class Test_Path : Test
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Move(Path dest, Path src, string expected)
		{
			dest.LibXmp_Path_Move(src);

			if (expected == null)
				Assert.AreEqual(null, dest.CurrentPath, "Should be null");
			else
				Assert.AreEqual(expected, dest.CurrentPath.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Set(Path dest, string value, string expected)
		{
			c_int ret = dest.LibXmp_Path_Set(value.ToCharPointer());
			Assert.AreEqual(0, ret, "Failed alloc");
			Assert.AreEqual(expected, dest.CurrentPath.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Truncate(Path dest, size_t num, string expected)
		{
			c_int ret = dest.LibXmp_Path_Truncate(num);
			Assert.AreEqual(0, ret, "Failed alloc");
			Assert.AreEqual(expected, dest.CurrentPath.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Suffix_At(Path dest, size_t ext_Pos, string ext, string expected)
		{
			c_int ret = dest.LibXmp_Path_Suffix_At(ext_Pos, ext.ToCharPointer());

			if (expected == null)
				Assert.AreEqual(-1, ret, "ext pos should cause failure");
			else
			{
				Assert.AreEqual(0, ret, "Failed alloc or check");
				Assert.AreEqual(expected, dest.CurrentPath.ToString());
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Append(Path dest, string value, string expected)
		{
			c_int ret = dest.LibXmp_Path_Append(value.ToCharPointer());

			Assert.AreEqual(0, ret, "Failed alloc or length overflowed");
			Assert.AreEqual(expected, dest.CurrentPath.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Join(Path dest, string value_a, string value_b, string expected)
		{
			c_int ret = dest.LibXmp_Path_Join(value_a.ToCharPointer(), value_b.ToCharPointer());

			Assert.AreEqual(0, ret, "Failed alloc or length overflowed");
			Assert.AreEqual(expected, dest.CurrentPath.ToString());
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std.Containers;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Monostate : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// All monostate instances must compare equal and hash to the same
		/// value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_All_Instances_Are_Equal()
		{
			monostate a = new monostate();
			monostate b = default;

			Assert.IsTrue(a == b);
			Assert.IsFalse(a != b);
			Assert.IsTrue(a.Equals(b));
			Assert.IsTrue(a.Equals((object)b));
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
		}



		/********************************************************************/
		/// <summary>
		/// A monostate must not be considered equal to an unrelated object
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Not_Equal_To_Other_Type()
		{
			monostate a = new monostate();

			Assert.IsFalse(a.Equals("not a monostate"));
		}
	}
}

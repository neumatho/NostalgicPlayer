/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt_Base
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Base_Numeric()
		{
			Assert.AreEqual(0U, Numeric.Saturate_Align_Up(0U, 4U));
			Assert.AreEqual(4U, Numeric.Saturate_Align_Up(1U, 4U));
			Assert.AreEqual(4U, Numeric.Saturate_Align_Up(2U, 4U));
			Assert.AreEqual(4U, Numeric.Saturate_Align_Up(3U, 4U));
			Assert.AreEqual(4U, Numeric.Saturate_Align_Up(4U, 4U));
			Assert.AreEqual(8U, Numeric.Saturate_Align_Up(5U, 4U));

			Assert.AreEqual(uint32.MaxValue - 3U, Numeric.Saturate_Align_Up(uint32.MaxValue - 5U, 4U));
			Assert.AreEqual(uint32.MaxValue - 3U, Numeric.Saturate_Align_Up(uint32.MaxValue - 4U, 4U));
			Assert.AreEqual(uint32.MaxValue - 3U, Numeric.Saturate_Align_Up(uint32.MaxValue - 3U, 4U));
			Assert.AreEqual(uint32.MaxValue, Numeric.Saturate_Align_Up(uint32.MaxValue - 2U, 4U));
			Assert.AreEqual(uint32.MaxValue, Numeric.Saturate_Align_Up(uint32.MaxValue - 1U, 4U));
			Assert.AreEqual(uint32.MaxValue, Numeric.Saturate_Align_Up(uint32.MaxValue - 0U, 4U));

			Assert.AreEqual(0, Numeric.Saturate_Align_Up(0, 4));
			Assert.AreEqual(4, Numeric.Saturate_Align_Up(1, 4));
			Assert.AreEqual(4, Numeric.Saturate_Align_Up(2, 4));
			Assert.AreEqual(4, Numeric.Saturate_Align_Up(3, 4));
			Assert.AreEqual(4, Numeric.Saturate_Align_Up(4, 4));
			Assert.AreEqual(8, Numeric.Saturate_Align_Up(5, 4));

			Assert.AreEqual(int32.MaxValue - 3, Numeric.Saturate_Align_Up(int32.MaxValue - 5, 4));
			Assert.AreEqual(int32.MaxValue - 3, Numeric.Saturate_Align_Up(int32.MaxValue - 4, 4));
			Assert.AreEqual(int32.MaxValue - 3, Numeric.Saturate_Align_Up(int32.MaxValue - 3, 4));
			Assert.AreEqual(int32.MaxValue, Numeric.Saturate_Align_Up(int32.MaxValue - 2, 4));
			Assert.AreEqual(int32.MaxValue, Numeric.Saturate_Align_Up(int32.MaxValue - 1, 4));
			Assert.AreEqual(int32.MaxValue, Numeric.Saturate_Align_Up(int32.MaxValue - 0, 4));
		}
	}
}

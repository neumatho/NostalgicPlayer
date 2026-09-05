/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Parse;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Parse()
		{
			Assert.IsTrue(Parse_.Parse<bool>("1"));
			Assert.IsFalse(Parse_.Parse<bool>("0"));
			Assert.IsTrue(Parse_.Parse<bool>("2"));
			Assert.IsFalse(Parse_.Parse<bool>("-0"));
			Assert.IsTrue(Parse_.Parse<bool>("-1"));

			Assert.AreEqual(586U, Parse_.Parse<uint32>("586"));
			Assert.AreEqual((uint32)int32.MaxValue, Parse_.Parse<uint32>("2147483647"));
			Assert.AreEqual(uint32.MaxValue, Parse_.Parse<uint32>("4294967295"));

			Assert.AreEqual(int64.MinValue, Parse_.Parse<int64>("-9223372036854775808"));
			Assert.AreEqual(-159, Parse_.Parse<int64>("-159"));
			Assert.AreEqual(int64.MaxValue, Parse_.Parse<int64>("9223372036854775807"));

			Assert.AreEqual(85059U, Parse_.Parse<uint64>("85059"));
			Assert.AreEqual((uint64)int64.MaxValue, Parse_.Parse<uint64>("9223372036854775807"));
			Assert.AreEqual(uint64.MaxValue, Parse_.Parse<uint64>("18446744073709551615"));

			Assert.AreEqual(-87.0f, Parse_.Parse<c_float>("-87.0"));
			Assert.AreEqual(-0.5e-6, Parse_.Parse<c_double>("-0.5e-6"));
			Assert.AreEqual(58.65403492763, Parse_.Parse<c_double>("58.65403492763"));

			Assert.AreEqual(254U, Parse_.Parse_Hex("fe"));
			Assert.AreEqual(65535U, Parse_.Parse_Hex("ffff"));
		}
	}
}

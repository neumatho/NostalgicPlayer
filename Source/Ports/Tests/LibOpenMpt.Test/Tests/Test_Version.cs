/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// Test if functions related to program version data work
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Version()
		{
			{
				// Verify that macros and functions work
				Assert.AreEqual(Version.Current(), Version.Parse(Version.Current().ToString()));
				Assert.AreEqual(Version.Current().ToString(), Version.Parse(Version.Current().ToString()).ToString());
				Assert.AreEqual("1.17.02.28", new Version(18285096).ToString());
				Assert.AreEqual(new Version(18285096), Version.Parse("1.17.02.28"));
				Assert.AreEqual(new Version(0x01fe0228), Version.Parse("1.fe.02.28"));
				Assert.AreEqual(new Version(0x01fe0228), Version.Parse("01.fe.02.28"));
				Assert.AreEqual(new Version(0x01220000), Version.Parse("1.22"));
				Assert.AreEqual(new Version(18285096), Version.LiteralParser.Parse("1.17.02.28"));
				Assert.AreEqual(new Version(0x01fe0228), Version.LiteralParser.Parse("1.fe.02.28"));
				Assert.AreEqual(new Version(0x01fe0228), Version.LiteralParser.Parse("01.fe.02.28"));
				Assert.AreEqual(new Version(0x01220000), Version.LiteralParser.Parse("1.22"));
				Assert.AreEqual(Version.LiteralParser.Parse("1.19.02.00"), Version.LiteralParser.Parse("1.19.02.00").WithoutTestNumber());
				Assert.AreEqual(Version.LiteralParser.Parse("1.18.03.00"), Version.LiteralParser.Parse("1.18.03.20").WithoutTestNumber());
				Assert.IsTrue(Version.LiteralParser.Parse("1.18.01.13").IsTestVersion());
				Assert.IsFalse(Version.LiteralParser.Parse("1.19.01.00").IsTestVersion());
				Assert.IsFalse(Version.LiteralParser.Parse("1.17.02.54").IsTestVersion());
				Assert.IsFalse(Version.LiteralParser.Parse("1.18.00.00").IsTestVersion());
				Assert.IsFalse(Version.LiteralParser.Parse("1.18.02.00").IsTestVersion());
				Assert.IsTrue(Version.LiteralParser.Parse("1.18.02.01").IsTestVersion());

				// Ensure that versions ending in .00.00 (which are ambiguous to truncated version numbers in certain file formats (e.g. S3M and IT) do not get qualified as test builds
				Assert.IsFalse(Version.LiteralParser.Parse("1.23.00.00").IsTestVersion());

				Assert.AreEqual(18285096U, Version.LiteralParser.Parse("1.17.2.28").GetRawVersion());
				Assert.AreEqual(18285128U, Version.LiteralParser.Parse("1.17.02.48").GetRawVersion());
				Assert.AreEqual(18285138U, Version.LiteralParser.Parse("01.17.02.52").GetRawVersion());

				// Ensure that bit-shifting works (used in some mod loaders for example)
				Assert.AreEqual(0x0117U << 16, Version.LiteralParser.Parse("01.17.00.00").GetRawVersion());
				Assert.AreEqual(0x011703U, Version.LiteralParser.Parse("01.17.03.00").GetRawVersion() >> 8);

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05-r13099").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsNewerThan(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05-r13099").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsOlderThan(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsNewerThan(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsNewerThan(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsOlderThan(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsOlderThan(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05-r13099").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsEqualTo(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05-r13099").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05-r13099").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05-r13099").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsFalse(VersionWithRevision.Parse("1.30.00.05").IsEqualTo(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05").IsEqualTo(VersionWithRevision.Parse("1.30.00.05")));

				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13100")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13099")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05-r13098")));
				Assert.IsTrue(VersionWithRevision.Parse("1.30.00.05").IsEquivalentTo(VersionWithRevision.Parse("1.30.00.05")));
			}
		}
	}
}

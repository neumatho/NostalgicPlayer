/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Test.Arc
{
	public partial class Test_Arc
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Arc_Method8()
		{
			ArcInfo arcInfo = GetArcInfo("Arc-Method8-RLE.arc");

			string result = Formats.Arc.Arc.Unpack(arcInfo.DecrunchedData, arcInfo.DecrunchedSize, arcInfo.CrunchedData, arcInfo.CrunchedSize, arcInfo.Method, 0);
			Assert.IsNull(result);

			Assert.AreEqual("64d67d1d5d123c6542a8099255ad8ca2", CalculateMd5Hash(arcInfo.DecrunchedData));
		}
	}
}

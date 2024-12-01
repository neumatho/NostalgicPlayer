/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.Ancient;

namespace Polycode.NostalgicPlayer.Ports.Tests.Ancient.Test
{
	/// <summary>
	/// This will test my own API I have added to Ancient to force
	/// a stream to use a specific decruncher
	/// </summary>
	[TestClass]
	public class Test_ForceType : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Test_ForceType() : base("TestForceType_Files")
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LhLibrary()
		{
			VerifyFile("Hamlet_LhLibrary.pack", "Hamlet.raw", false, DecompressorType.LhLibrary);
		}
	}
}

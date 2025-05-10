/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This input crashed the Digital Tracker loader due to duplicate
		/// PATT chunks corrupting the channel and track counts, causing heap
		/// corruption when generating empty patterns
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Dt_Duplicate_Chunk()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Dt_Duplicate_Patt.dtm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}

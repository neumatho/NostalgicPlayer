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
		/// This input caused heap corruption in the Coconizer loader due to
		/// a missing bounds check when loading the sequence table
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Coco_Invalid_Sequence()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Coco_Invalid_Sequence.coco", opaque);
			Assert.IsTrue((ret == 0) || (ret == -(c_int)Xmp_Error.Load), "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}

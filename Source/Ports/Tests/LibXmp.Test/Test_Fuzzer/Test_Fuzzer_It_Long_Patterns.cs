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
		/// This input caused slow loads and high RAM usage due to attempting
		/// to allocate a large number of patterns of excessive length
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_It_Long_Patterns()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused hangs and high RAM consumption in the IT loader
			// due to allocating large buffers for invalid compressed samples
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Long_Patterns.it", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}

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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Arch_Invalid()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input high memory consumption in the Archimedes Tracker
			// loader due to allowing negative pattern counts
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Arch_Invalid_Patterns.arch", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			// This input caused signed overflows calculating sample loop
			// lengths
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Arch_Invalid_Sample_Loops.arch", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}

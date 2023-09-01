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
		/// The IT loader allows duplicate check action values of 3 (which
		/// are found in some .ITs) but this caused an out-of-bounds read
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_It_Dca_3()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Dca_3.it", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}

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
		public void Test_Fuzzer_Gal4_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused UMRs in the Galaxy 4.0 loader due to
			// not checking the hio_read return value for the module title
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal4_Truncated.gal4", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (truncated)");

			// This input caused UMRs in the Galaxy 4.0 loader due to
			// not checking the hio_read return value for the volume envelope
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal4_Truncated_Env.gal4", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (truncated_env)");

			// Same, but the pan envelope instead
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal4_Truncated_Env2.gal4", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (truncated_env2)");

			opaque.Xmp_Free_Context();
		}
	}
}

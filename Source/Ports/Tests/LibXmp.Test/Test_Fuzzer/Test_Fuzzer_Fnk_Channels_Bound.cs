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
		public void Test_Fuzzer_Fnk_Channels_Bound()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input crashed the FNK loader due to containing a channel
			// count of 0
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Fnk_Channels_Bound.fnk", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (1)");

			// This input crashed the FNK loader due to the loader attempting to
			// put pattern break bytes in a nonexistent 2nd channel.
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Fnk_Channels_Bound_2.fnk", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (2)");

			opaque.Xmp_Free_Context();
		}
	}
}

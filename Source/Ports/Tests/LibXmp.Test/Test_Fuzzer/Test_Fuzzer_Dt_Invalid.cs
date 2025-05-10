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
		public void Test_Fuzzer_Dt_Invalid()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused hangs and massive RAM consumption in the Digital Tracker
			// loader due to a missing channel count bounds check
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Dt_Invalid_Channel_Count.dtm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (channel count)");

			// This input caused leaks in the Digital Tracker loader due to attempting
			// to load an invalid number of instruments.
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Dt_Invalid_Instrument_Count.dtm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (instrument count)");

			// This input caused signed integer overflows in the DTM loader due
			// to poor bounding of sample lengths and invalid sample loops
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Dt_Invalid_Sample_Loop.dtm", opaque);
			Assert.AreEqual(0, ret, "Module load (sample loop)");

			// This input caused signed integer overflows in the DTM test function
			// due to badly handling "negative" chunk lengths
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Dt_Invalid_Header_Size.dtm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (header size)");

			opaque.Xmp_Free_Context();
		}
	}
}

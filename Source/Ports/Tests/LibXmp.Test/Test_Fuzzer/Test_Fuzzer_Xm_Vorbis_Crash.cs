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
		/// Inputs that caused crashes in libxmp's Ogg Vorbis decoder
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Xm_Vorbis_Crash()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input OXM caused NULL dereferences in stb-vorbis
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Crash.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (1)");

			// This input OXM caused double frees in stb-vorbis due to
			// a libxmp fix conflicting with upstream
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Crash2.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (2)");

			// This input OXM caused heap corruption in stb_vorbis due to
			// the bounding of f->temp_memory_required being slightly off
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Crash3.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (3)");

			// This input OXM caused out-of-bounds reads of the multiplicands
			// array due to a bad clamp on the number of dimensions to decode
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Crash4.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (4)");

			opaque.Xmp_Free_Context();
		}
	}
}

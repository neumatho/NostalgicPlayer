/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		private const int BufferSize = 256000;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Load_Module_From_Memory()
		{
			c_int size;

			uint8[] buffer = new uint8[BufferSize];

			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			using (Stream modStream = OpenStream(dataDirectory, "Test.xm"))
			{
				size = modStream.Read(buffer, 0, BufferSize);
			}

			// Valid file
			c_int ret = ctx.Xmp_Load_Module_From_Memory(buffer, size);
			Assert.AreEqual(0, ret, "Load file");

			ctx.Xmp_Get_Frame_Info(out Xmp_Frame_Info fi);
			Assert.AreEqual(15360, fi.Total_Time, "Module duration");

			using (Stream modStream = OpenStream(dataDirectory, "Test.it"))
			{
				size = modStream.Read(buffer, 0, BufferSize);
			}

			// And reload without releasing
			ret = ctx.Xmp_Load_Module_From_Memory(buffer, size);
			Assert.AreEqual(0, ret, "Load file");

			ctx.Xmp_Get_Frame_Info(out fi);
			Assert.AreEqual(7680, fi.Total_Time, "Module duration");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}

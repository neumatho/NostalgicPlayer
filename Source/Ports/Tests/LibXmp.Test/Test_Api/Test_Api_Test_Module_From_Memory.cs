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
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Test_Module_From_Memory()
		{
			Xmp_Test_Info tInfo;

			// Sufficient to hold all file buffers
			uint8[] buf = new uint8[64 * 1024];

			// Unsupported format
			c_int ret = Test_Module_From_Memory_Helper("Storlek_01.data", out tInfo, buf);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Unsupported format fail");

			// File too small
			ret = Test_Module_From_Memory_Helper("Sample-16Bit.raw", out tInfo, buf);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Small file fail");

			// XM
			ret = Test_Module_From_Memory_Helper("Xm_Portamento_Target.xm", out tInfo, buf);
			Assert.AreEqual(0, ret, "XM test module fail");
			Assert.AreEqual("FastTracker II", tInfo.Type, "XM module type fail");

			// IT
			ret = Test_Module_From_Memory_Helper("Storlek_01.it", out tInfo, buf);
			Assert.AreEqual(0, ret, "IT test module fail");
			Assert.AreEqual("arpeggio + pitch slide", tInfo.Name, "IT module name fail");
			Assert.AreEqual("Impulse Tracker", tInfo.Type, "IT module type fail");

			// Small file (<256 bytes)
			ret = Test_Module_From_Memory_Helper("Small.gdm", out tInfo, buf);
			Assert.AreEqual(0, ret, "GDM (<256) test module fail");
			Assert.AreEqual(string.Empty, tInfo.Name, "GDM (<256) module name fail");
			Assert.AreEqual("General Digital Music", tInfo.Type, "GDM (<256) module type fail");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Test_Module_From_Memory_Helper(string fileName, out Xmp_Test_Info tInfo, uint8[] buf)
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			size_t bRead;

			using (Stream modStream = OpenStream(dataDirectory, fileName))
			{
				bRead = (size_t)modStream.Read(buf, 0, 64 * 1024);
			}

			c_int ret = ctx.Xmp_Test_Module_From_Memory(buf, (c_long)bRead, out tInfo);

			ctx.Xmp_Free_Context();

			return ret;
		}
		#endregion
	}
}

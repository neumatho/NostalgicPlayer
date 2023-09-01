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
		public void Test_Api_Test_Module_From_File()
		{
			Xmp_Test_Info tInfo;

			// Unsupported format
			c_int ret = Test_Module_From_File_Helper("Storlek_01.data", out tInfo);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Unsupported format fail");

			// File too small
			ret = Test_Module_From_File_Helper("Sample-16Bit.raw", out tInfo);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Small file fail");

			// IT
			ret = Test_Module_From_File_Helper("Storlek_01.it", out tInfo);
			Assert.AreEqual(0, ret, "IT test module fail");
			Assert.AreEqual("arpeggio + pitch slide", tInfo.Name, "IT module name fail");
			Assert.AreEqual("Impulse Tracker", tInfo.Type, "IT module type fail");

			// Small file (<256 bytes)
			ret = Test_Module_From_File_Helper("Small.gdm", out tInfo);
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
		private c_int Test_Module_From_File_Helper(string fileName, out Xmp_Test_Info tInfo)
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			c_int ret;

			using (Stream modStream = OpenStream(dataDirectory, fileName))
			{
				ret = ctx.Xmp_Test_Module_From_File(modStream, out tInfo);
			}

			ctx.Xmp_Free_Context();

			return ret;
		}
		#endregion
	}
}

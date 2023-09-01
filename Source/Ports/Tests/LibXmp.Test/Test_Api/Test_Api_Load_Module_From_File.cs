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
		public void Test_Api_Load_Module_From_File()
		{
			// The code below is a merge between "test_api_load_module" and "test_api_load_module_from_file",
			// but changed to use Xmp_Load_Module_From_File() all places, since the filename version has
			// not been ported
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Xmp_State state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Small file
			c_int ret = Load_Module_From_File_Helper("Sample-16Bit.raw", ctx);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Small file");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Invalid format
			ret = Load_Module_From_File_Helper("Storlek_01.data", ctx);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Invalid format");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Add corrupted S3M test
			ret = Load_Module_From_File_Helper("Adlib.S3M-Corrupted.s3m", ctx);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depack error fail");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Valid file
			ret = Load_Module_From_File_Helper("Test.it", ctx);
			Assert.AreEqual(0, ret, "Load file");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			// Load again without unloading
			ret = Load_Module_From_File_Helper("Test.xm", ctx);
			Assert.AreEqual(0, ret, "Reload file");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			// Valid file (<256 bytes)
			ret = Load_Module_From_File_Helper("Small.gdm", ctx);
			Assert.AreEqual(0, ret, "Load file <256 bytes");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			// Unload
			ctx.Xmp_Release_Module();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Double release should be safe
			ctx.Xmp_Release_Module();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			ctx.Xmp_Free_Context();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Module_From_File_Helper(string fileName, Ports.LibXmp.LibXmp ctx)
		{
			using (Stream modStream = OpenStream(dataDirectory, fileName))
			{
				return ctx.Xmp_Load_Module_From_File(modStream);
			}
		}
		#endregion
	}
}

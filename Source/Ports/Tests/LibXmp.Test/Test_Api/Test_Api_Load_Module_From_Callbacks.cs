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
		public void Test_Api_Load_Module_From_Callbacks()
		{
			Xmp_Callbacks file_Callbacks = GetCallbacks();

			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Stream f = OpenStream(dataDirectory, "Test.it");

			// Null data pointer
			c_int ret = ctx.Xmp_Load_Module_From_Callbacks(null, file_Callbacks);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null data fail");

			// Null callback
			Xmp_Callbacks t1 = GetCallbacks();
			Xmp_Callbacks t2 = GetCallbacks();
			Xmp_Callbacks t3 = GetCallbacks();

			t1.Read_Func = null;
			t2.Seek_Func = null;
			t3.Tell_Func = null;

			ret = ctx.Xmp_Load_Module_From_Callbacks(f, t1);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null read_func fail");
			ret = ctx.Xmp_Load_Module_From_Callbacks(f, t2);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null seek_func fail");
			ret = ctx.Xmp_Load_Module_From_Callbacks(f, t3);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null tell_func fail");

			// Load
			ret = ctx.Xmp_Load_Module_From_Callbacks(f, file_Callbacks);
			Assert.AreEqual(0, ret, "Load file");

			Xmp_State state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			// Unload
			ctx.Xmp_Release_Module();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Load with close callback should not leak fp
			f.Seek(0, SeekOrigin.Begin);

			t1 = GetCallbacks();
			t1.Close_Func = Close_Func;

			ret = ctx.Xmp_Load_Module_From_Callbacks(f, t1);
			Assert.AreEqual(0, ret, "Load file");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}

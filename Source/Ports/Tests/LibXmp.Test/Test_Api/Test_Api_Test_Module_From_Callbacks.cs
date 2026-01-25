/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
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
		public void Test_Api_Test_Module_From_Callbacks()
		{
			Xmp_Test_Info tInfo;

			Xmp_Callbacks file_Callbacks = GetCallbacks();

			// Unsupported format
			c_int ret = Test_Module_From_Callbacks_Helper("Storlek_01.data", out tInfo, file_Callbacks);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Unsupported format fail");

			// File too small
			ret = Test_Module_From_Callbacks_Helper("Sample-16Bit.raw", out tInfo, file_Callbacks);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Small file fail");

			// Null data pointer
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			ret = ctx.Xmp_Test_Module_From_Callbacks(null, file_Callbacks, out tInfo);
			ctx.Xmp_Free_Context();
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null data fail");

			Stream f = OpenStream(dataDirectory, "Storlek_05.it");

			// Null callback
			Xmp_Callbacks t1 = GetCallbacks();
			Xmp_Callbacks t2 = GetCallbacks();
			Xmp_Callbacks t3 = GetCallbacks();

			t1.Read_Func = null;
			t2.Seek_Func = null;
			t3.Tell_Func = null;

			ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			ret = ctx.Xmp_Test_Module_From_Callbacks(f, t1, out tInfo);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null read_func fail");
			ret = ctx.Xmp_Test_Module_From_Callbacks(f, t2, out tInfo);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null seek_func fail");
			ret = ctx.Xmp_Test_Module_From_Callbacks(f, t3, out tInfo);
			Assert.AreEqual(-(c_int)Xmp_Error.System, ret, "Null tell_func fail");

			// Close func should close
			t1 = GetCallbacks();
			t1.Close_Func = Close_Func;

			ret = ctx.Xmp_Test_Module_From_Callbacks(f, t1, out tInfo);
			Assert.AreEqual(0, ret, "Test with close func");

			// XM
			ret = Test_Module_From_Callbacks_Helper("Xm_Portamento_Target.xm", out tInfo, file_Callbacks);
			Assert.AreEqual(0, ret, "XM test module fail");
			Assert.AreEqual("FastTracker II", tInfo.Type, "XM module type fail");

			// IT
			ret = Test_Module_From_Callbacks_Helper("Storlek_01.it", out tInfo, file_Callbacks);
			Assert.AreEqual(0, ret, "IT test module fail");
			Assert.AreEqual("arpeggio + pitch slide", tInfo.Name, "IT module name fail");
			Assert.AreEqual("Impulse Tracker", tInfo.Type, "IT module type fail");

			// Small file (<256 bytes)
			ret = Test_Module_From_Callbacks_Helper("Small.gdm", out tInfo, file_Callbacks);
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
		private c_int Test_Module_From_Callbacks_Helper(string fileName, out Xmp_Test_Info tInfo, Xmp_Callbacks file_Callbacks)
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			c_int ret;

			using (Stream modStream = OpenStream(dataDirectory, fileName))
			{
				ret = ctx.Xmp_Test_Module_From_Callbacks(modStream, file_Callbacks, out tInfo);
			}

			ctx.Xmp_Free_Context();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Xmp_Callbacks GetCallbacks()
		{
			return new Xmp_Callbacks
			{
				Read_Func = Read_Func,
				Seek_Func = Seek_Func,
				Tell_Func = Tell_Func,
				Close_Func = null
			};
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_ulong Read_Func(CPointer<uint8> dest, c_ulong len, c_ulong nMemB, object priv)
		{
			Stream f = (Stream)priv;

			return (c_ulong)f.Read(dest.AsSpan((int)(len * nMemB))) / len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Seek_Func(object priv, c_long offset, SeekOrigin whence)
		{
			Stream f = (Stream)priv;

			f.Seek(offset, whence);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_long Tell_Func(object priv)
		{
			Stream f = (Stream)priv;

			return (c_long)f.Position;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Close_Func(object priv)
		{
			Stream f = (Stream)priv;

			f.Close();

			return 0;
		}
		#endregion
	}
}

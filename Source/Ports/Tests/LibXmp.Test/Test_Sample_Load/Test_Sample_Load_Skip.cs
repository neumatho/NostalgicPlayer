/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Sample_Load
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Sample_Load
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Sample_Load_Skip()
		{
			Xmp_Sample s = new Xmp_Sample();
			c_short[] buffer = new c_short[202];
			Module_Data m = new Module_Data();

			using (Stream stream = OpenStream(dataDirectory, "Sample-16Bit.raw"))
			{
				Hio f = Hio.Hio_Open_File(stream);
				Assert.IsNotNull(f, "Can't open sample file");

				// Read little-endian sample to native-endian buffer
				for (c_int i = 0; i < 101; i++)
					buffer[i] = (c_short)f.Hio_Read16L();

				for (c_int i = 0; i < 101; i++)
					buffer[101 + i] = buffer[101 - i - 1];

				// Load sample from file
				Set(s, 101, 0, 102, Xmp_Sample_Flag._16Bit);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNotNull, "Didn't allocate sample data");
				Assert.AreEqual(101, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(0, CMemory.memcmp(s.Data, MemoryMarshal.Cast<c_short, uint8>(buffer).ToArray(), 202), "Sample data error");
				Clear(s);

				// Disable sample load
				Set(s, 101, 0, 102, Xmp_Sample_Flag._16Bit);
				f.Hio_Seek(0, SeekOrigin.Begin);
				m.SmpCtl |= Xmp_SmpCtl_Flag.Skip;
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNull, "Didn't skip sample load");

				f.Hio_Close();
			}
		}
	}
}

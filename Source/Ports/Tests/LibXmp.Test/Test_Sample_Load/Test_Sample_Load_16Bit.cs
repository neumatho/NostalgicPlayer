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
		public void Test_Sample_Load_16Bit()
		{
			Xmp_Sample s = new Xmp_Sample();
			c_short[] buffer = new c_short[101];
			Module_Data m = new Module_Data();

			using (Stream stream = OpenStream(dataDirectory, "Sample-16Bit.raw"))
			{
				Hio f = Hio.Hio_Open_File(stream);
				Assert.IsNotNull(f, "Can't open sample file");

				// Read little-endian sample to native-endian buffer
				for (c_int i = 0; i < 101; i++)
					buffer[i] = (c_short)f.Hio_Read16L();

				// Load zero-length sample
				Set(s, 0, 0, 101, Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop);
				Sample.LibXmp_Load_Sample(m, null, Sample_Flag.None, s, null);

				// Load sample with invalid loop
				Set(s, 101, 150, 180, Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNotNull, "Didn't allocate sample data");
				Assert.AreEqual(0, s.Lps, "Didn't fix invalid loop start");
				Assert.AreEqual(0, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(Xmp_Sample_Flag._16Bit, s.Flg, "Didn't reset loop flags");
				Clear(s);

				// Load sample with invalid loop
				Set(s, 101, 50, 40, Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNotNull, "Didn't allocate sample data");
				Assert.AreEqual(0, s.Lps, "Didn't fix invalid loop start");
				Assert.AreEqual(0, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(Xmp_Sample_Flag._16Bit, s.Flg, "Didn't reset loop flags");
				Clear(s);

				// Load sample from file
				Set(s, 101, 0, 102, Xmp_Sample_Flag._16Bit);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNotNull, "Didn't allocate sample data");
				Assert.AreEqual(101, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(0, CMemory.MemCmp(s.Data, MemoryMarshal.Cast<c_short, uint8>(buffer).ToArray(), 202), "Sample data error");
				Assert.AreEqual(s.Data[0], s.Data[-2], "Sample prologue error");
				Assert.AreEqual(s.Data[1], s.Data[-1], "Sample prologue error");
				Assert.AreEqual(s.Data[200], s.Data[202], "Sample prologue error");
				Assert.AreEqual(s.Data[201], s.Data[203], "Sample prologue error");
				Assert.AreEqual(s.Data[202], s.Data[204], "Sample prologue error");
				Assert.AreEqual(s.Data[203], s.Data[205], "Sample prologue error");
				Clear(s);

				// Load sample from file w/ loop
				Set(s, 101, 20, 80, Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsTrue(s.Data.IsNotNull, "Didn't allocate sample data");
				Assert.AreEqual(0, CMemory.MemCmp(s.Data, MemoryMarshal.Cast<c_short, uint8>(buffer).ToArray(), 202), "Sample data error");
				Assert.AreEqual(s.Data[0], s.Data[-2], "Sample prologue error");
				Assert.AreEqual(s.Data[1], s.Data[-1], "Sample prologue error");
				Assert.AreEqual(s.Data[200], s.Data[202], "Sample prologue error");
				Assert.AreEqual(s.Data[201], s.Data[203], "Sample prologue error");
				Assert.AreEqual(s.Data[202], s.Data[204], "Sample prologue error");
				Assert.AreEqual(s.Data[203], s.Data[205], "Sample prologue error");
				Clear(s);

				f.Hio_Close();
			}
		}
	}
}

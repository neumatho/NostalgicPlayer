/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
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
		public void Test_Sample_Load_8Bit()
		{
			Xmp_Sample s = new Xmp_Sample();
			sbyte[] buffer = new sbyte[101];
			Module_Data m = new Module_Data();

			using (Stream stream = OpenStream(dataDirectory, "Sample-8Bit.raw"))
			{
				Hio f = Hio.Hio_Open_File(stream);
				Assert.IsNotNull(f, "Can't open sample file");
				f.Hio_Read(buffer, 1, 101);

				// Load zero-length sample
				Set(s, 0, 0, 101, Xmp_Sample_Flag.Loop);
				Sample.LibXmp_Load_Sample(m, null, Sample_Flag.None, s, null);

				// Load sample with invalid loop
				Set(s, 101, 150, 180, Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsNotNull(s.Data, "Didn't allocate sample data");
				Assert.AreEqual(0, s.Lps, "Didn't fix invalid loop start");
				Assert.AreEqual(0, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(Xmp_Sample_Flag.None, s.Flg, "Didn't reset loop flags");
				Clear(s);

				// Load sample with invalid loop
				Set(s, 101, 50, 40, Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsNotNull(s.Data, "Didn't allocate sample data");
				Assert.AreEqual(0, s.Lps, "Didn't fix invalid loop start");
				Assert.AreEqual(0, s.Lpe, "Didn't fix invalid loop end");
				Assert.AreEqual(Xmp_Sample_Flag.None, s.Flg, "Didn't reset loop flags");
				Clear(s);

				// Load sample from file
				Set(s, 101, 0, 102, Xmp_Sample_Flag.None);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsNotNull(s.Data, "Didn't allocate sample data");
				Assert.AreEqual(101, s.Lpe, "Didn't fix invalid loop end");
				Assert.IsTrue(ArrayHelper.ArrayCompare(s.Data, s.DataOffset, MemoryMarshal.Cast<int8, uint8>(buffer).ToArray(), 0, 101), "Sample data error");
				Assert.AreEqual(s.Data[s.DataOffset], s.Data[s.DataOffset - 1], "Sample prologue error");
				Assert.AreEqual(s.Data[s.DataOffset + 100], s.Data[s.DataOffset + 101], "Sample prologue error");
				Assert.AreEqual(s.Data[s.DataOffset + 101], s.Data[s.DataOffset + 102], "Sample prologue error");
				Clear(s);

				// Load sample from file w/ loop
				Set(s, 101, 20, 80, Xmp_Sample_Flag.Loop);
				f.Hio_Seek(0, SeekOrigin.Begin);
				Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, s, null);
				Assert.IsNotNull(s.Data, "Didn't allocate sample data");
				Assert.IsTrue(ArrayHelper.ArrayCompare(s.Data, s.DataOffset, MemoryMarshal.Cast<int8, uint8>(buffer).ToArray(), 0, 101), "Sample data error");
				Assert.AreEqual(s.Data[s.DataOffset], s.Data[s.DataOffset - 1], "Sample prologue error");
				Assert.AreEqual(s.Data[s.DataOffset + 100], s.Data[s.DataOffset + 101], "Sample prologue error");
				Assert.AreEqual(s.Data[s.DataOffset + 101], s.Data[s.DataOffset + 102], "Sample prologue error");
				Clear(s);

				f.Hio_Close();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set(Xmp_Sample s, c_int x, c_int y, c_int z, Xmp_Sample_Flag w)
		{
			s.Len = x;
			s.Lps = y;
			s.Lpe = z;
			s.Flg = w;
			s.Data = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Clear(Xmp_Sample s)
		{
			Sample.LibXmp_Free_Sample(s);
		}
	}
}

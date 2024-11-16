using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.Ancient;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Tests.Ancient.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_CrunchMania()
		{
			VerifyFile("test_C1.crm", "test_C1.raw");
			VerifyFile("test_C1_delta.crm", "test_C1.raw");
			VerifyFile("test_C1_lz.crm", "test_C1.raw");
			VerifyFile("test_C1_lz_delta.crm", "test_C1.raw");
			VerifyFile("test_C1_18051973.crm", "test_C1.raw");
			VerifyFile("test_C1_cd31.crm", "test_C1.raw");
			VerifyFile("test_C1_dcs.crm", "test_C1.raw");
			VerifyFile("test_C1_iron.crm", "test_C1.raw");
			VerifyFile("test_C1_mss.crm", "test_C1.raw");
			VerifyFile("test_C1_mss_delta.crm", "test_C1.raw");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mmcmp()
		{
			VerifyFile("test_C2.mmcmp122", "test_C2.xm", true);
			VerifyFile("test_C2.mmcmp130", "test_C2.xm", true);
			VerifyFile("test_C2.mmcmp132", "test_C2.xm", true);
			VerifyFile("test_C2.mmcmp134", "test_C2.xm", true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_PowerPacker()
		{
			VerifyFile("test_C1.pp", "test_C1.raw");
			VerifyFile("test_C1_chfc.pp", "test_C1.raw");
			VerifyFile("test_C1_den.pp", "test_C1.raw");
			VerifyFile("test_C1_dxs9.pp", "test_C1.raw");
			VerifyFile("test_C1_hd.pp", "test_C1.raw");
			VerifyFile("test_C1_rvv.pp", "test_C1.raw");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_StoneCracker()
		{
//			VerifyFile("test_C1.pack271_000.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_032.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_033.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_132.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_232.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_332.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_432.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_532.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_632.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_732.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack271_733.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_0.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_1.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_2.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_3.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_4.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_5.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_6.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack292_7.stc", "test_C1.raw");
//			VerifyFile("test_C1.pack299_0.stc", "test_C1.raw");
			VerifyFile("test_C1.pack300_0.stc", "test_C1.raw");
			VerifyFile("test_C1.pack310.stc", "test_C1.raw", true);
			VerifyFile("test_C1.pack401.stc", "test_C1.raw");
			VerifyFile("test_C1.pack402.stc", "test_C1.raw");
			VerifyFile("test_C1.pack410.stc", "test_C1.raw");
			VerifyFile("test_C1.pack410_0.stc", "test_C1.raw");
			VerifyFile("test_C1.packpre400.stc", "test_C1.raw");
			VerifyFile("test_C1_1am.sc300.stc", "test_C1.raw");
			VerifyFile("test_C1_2am.sc401.stc", "test_C1.raw");
			VerifyFile("test_C1_ays.sc410.stc", "test_C1.raw");
			VerifyFile("test_C1_zg.sc403.stc", "test_C1.raw");
			VerifyFile("test_C1_zulu.sc403.stc", "test_C1.raw");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Xpk()
		{
			VerifyFile("test_C1_blzw.xpkf", "test_C1.raw");
			VerifyFile("test_C1_bzp2.xpkf", "test_C1.raw");
			VerifyFile("test_C1_lhlb.xpkf", "test_C1.raw");
			VerifyFile("test_C1_mash.xpkf", "test_C1.raw");
			VerifyFile("test_C1_rake.xpkf", "test_C1.raw");
			VerifyFile("test_C1_shri.xpkf", "test_C1.raw");
			VerifyFile("test_C1_smpl.xpkf", "test_C1.raw");
			VerifyFile("test_C1_sqsh.xpkf", "test_C1.raw");
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private byte[] ReadFile(string fileName)
		{
			string location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			using (FileStream fs = File.OpenRead(Path.Combine(location, "..\\..\\..\\Test_Files", fileName)))
			{
				byte[] buffer = new byte[fs.Length];
				fs.ReadExactly(buffer);

				return buffer;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VerifyFile(string packedFile, byte[] verify, bool ignoreExpansion = false)
		{
			using (MemoryStream packed = new MemoryStream(ReadFile(packedFile)))
			{
				Decompressor decompressor;

				try
				{
					decompressor = new Decompressor(packed);
				}
				catch (InvalidFormatException)
				{
					Assert.Fail($"Unknown or invalid compression format in file {packedFile}");
					return;
				}
				catch (VerificationException)
				{
					Assert.Fail($"Verify (packed) failed for {packedFile}");
					return;
				}

				List<byte[]> raw = new List<byte[]>();

				try
				{
					raw.AddRange(decompressor.Decompress());
				}
				catch (DecompressionException)
				{
					Assert.Fail($"Decompression failed for {packedFile}");
					return;
				}
				catch (VerificationException)
				{
					Assert.Fail($"Verify (raw) failed for {packedFile}");
					return;
				}

				int actualSize = raw.Sum(x => x.Length);
				if (((verify.Length != actualSize) && !ignoreExpansion) || ((actualSize < verify.Length) && ignoreExpansion))
				{
					Assert.Fail($"Verify failed for {packedFile} - size differs from original");
					return;
				}

				int offset = 0;

				byte[] rawToCheck = null;
				int rawIndex = 0;

				for (int i = 0, j = 0; i < verify.Length; i++, j++)
				{
					if ((rawToCheck == null) || (j == rawToCheck.Length))
					{
						rawToCheck = raw[rawIndex++];
						j = 0;
					}

					if (rawToCheck[j] != verify[i + offset])
					{
						Assert.Fail($"Verify failed for {packedFile} - contents differ @ {i} from original");
						return;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VerifyFile(string packedFile, string rawFile, bool ignoreExpansion = false)
		{
			byte[] verify = ReadFile(rawFile);
			VerifyFile(packedFile, verify, ignoreExpansion);
		}
		#endregion
	}
}

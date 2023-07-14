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
				fs.Read(buffer);

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

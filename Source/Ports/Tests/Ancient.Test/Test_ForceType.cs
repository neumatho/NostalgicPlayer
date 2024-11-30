/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
	/// This will test my own API I have added to Ancient to force
	/// a stream to use a specific decruncher
	/// </summary>
	[TestClass]
	public class Test_ForceType
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LhLibrary()
		{
			VerifyFile("Hamlet_LhLibrary.pack", "Hamlet.txt", DecompressorType.LhLibrary);
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

			using (FileStream fs = File.OpenRead(Path.Combine(location, "..\\..\\..\\TestForceType_Files", fileName)))
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
		private void VerifyFile(string packedFile, byte[] verify, DecompressorType type, bool ignoreExpansion = false)
		{
			using (MemoryStream packed = new MemoryStream(ReadFile(packedFile)))
			{
				Decompressor decompressor;

				try
				{
					decompressor = new Decompressor(packed, type, (ulong)verify.Length);
				}
				catch (InvalidFormatException)
				{
					Assert.Fail($"Unknown or invalid compression format in file {packedFile}");
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
		private void VerifyFile(string packedFile, string rawFile, DecompressorType type, bool ignoreExpansion = false)
		{
			byte[] verify = ReadFile(rawFile);
			VerifyFile(packedFile, verify, type, ignoreExpansion);
		}
		#endregion
	}
}

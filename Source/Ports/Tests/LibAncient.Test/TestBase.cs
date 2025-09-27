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
using Polycode.NostalgicPlayer.Ports.LibAncient;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibAncient.Test
{
	/// <summary>
	/// Base class for test classes
	/// </summary>
	public abstract class TestBase
	{
		private readonly string folder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected TestBase(string testFolder)
		{
			string location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			folder = Path.Combine(location, $"..\\..\\..\\{testFolder}");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected byte[] ReadFile(string fileName)
		{
			using (FileStream fs = File.OpenRead(Path.Combine(folder, fileName)))
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
		protected void VerifyFile(string packedFile, byte[] verify, bool ignoreExpansion = false)
		{
			VerifyFile(packedFile, verify, null, ignoreExpansion);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void VerifyFile(string packedFile, byte[] verify, DecompressorType? type, bool ignoreExpansion = false)
		{
			using (MemoryStream packed = new MemoryStream(ReadFile(packedFile)))
			{
				Decompressor decompressor;

				try
				{
					decompressor = type.HasValue ? new Decompressor(packed, type.Value, (ulong)verify.Length) : new Decompressor(packed, true);
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
		protected void VerifyFile(string packedFile, string rawFile, bool ignoreExpansion = false, DecompressorType? type = null)
		{
			byte[] verify = ReadFile(rawFile);
			VerifyFile(packedFile, verify, type, ignoreExpansion);
		}
	}
}

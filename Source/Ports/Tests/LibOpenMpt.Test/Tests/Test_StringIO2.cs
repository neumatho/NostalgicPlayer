/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	///
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// Test if copying between different sized buffers work
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_StringIO2()
		{
			{
				uint8[] s0 = [ (uint8)'\0', (uint8)'X', (uint8)' ', (uint8)'X' ];
				uint8[] s2 = [ (uint8)'X', (uint8)' ', (uint8)'\0', (uint8)'X' ];
				uint8[] s4 = [ (uint8)'X', (uint8)'Y', (uint8)'Z', (uint8)' ' ];

				uint8[] d2 = [ 0x00, 0x00 ];
				uint8[] d3 = [ 0x00, 0x00, 0x00 ];
				uint8[] d4 = [ 0x00, 0x00, 0x00, 0x00 ];
				uint8[] d5 = [ 0x00, 0x00, 0x00, 0x00, 0x00 ];

				void CopyTest(CPointer<uint8> dst, CPointer<uint8> src, string expectedResult)
				{
					CMemory.memset<uint8>(dst, 0x7f, dst.Size());
					MptString.WriteAutoBuf(dst).Assign(MptString.ReadAutoBuf(src));
					Assert.AreEqual(0, CString.strncmp(dst, expectedResult, dst.Size()));	// Ensure that the strings are identical

					for (size_t i = CString.strlen(dst); i < dst.Size(); i++)
						Assert.AreEqual(0, dst[i]);		// Ensure that rest of the buffer is completely nulled
				}

				CopyTest(d2, s0, string.Empty);
				CopyTest(d2, s2, "X");
				CopyTest(d2, s4, "X");
				CopyTest(d3, s0, string.Empty);
				CopyTest(d3, s2, "X ");
				CopyTest(d3, s4, "XY");
				CopyTest(d4, s0, string.Empty);
				CopyTest(d4, s2, "X ");
				CopyTest(d4, s4, "XYZ");
				CopyTest(d5, s0, string.Empty);
				CopyTest(d5, s2, "X ");
				CopyTest(d5, s4, "XYZ ");

				void CopyTestN(CPointer<uint8> dst, CPointer<uint8> src, size_t len, string expectedResult)
				{
					CMemory.memset<uint8>(dst, 0x7f, dst.Size());
					MptString.WriteAutoBuf(dst).Assign(MptString.ReadAutoBuf(src, Math.Min(src.Size(), len)));
					Assert.AreEqual(0, CString.strncmp(dst, expectedResult, dst.Size()));	// Ensure that the strings are identical

					for (size_t i = CString.strlen(dst); i < dst.Size(); i++)
						Assert.AreEqual(0, dst[i]);		// Ensure that rest of the buffer is completely nulled
				}

				CopyTestN(d2, s0, 1, string.Empty);
				CopyTestN(d2, s2, 1, "X");
				CopyTestN(d2, s4, 1, "X");
				CopyTestN(d3, s0, 1, string.Empty);
				CopyTestN(d3, s2, 1, "X");
				CopyTestN(d3, s4, 1, "X");
				CopyTestN(d4, s0, 1, string.Empty);
				CopyTestN(d4, s2, 1, "X");
				CopyTestN(d4, s4, 1, "X");
				CopyTestN(d5, s0, 1, string.Empty);
				CopyTestN(d5, s2, 1, "X");
				CopyTestN(d5, s4, 1, "X");

				CopyTestN(d2, s0, 2, string.Empty);
				CopyTestN(d2, s2, 2, "X");
				CopyTestN(d2, s4, 2, "X");
				CopyTestN(d3, s0, 2, string.Empty);
				CopyTestN(d3, s2, 2, "X ");
				CopyTestN(d3, s4, 2, "XY");
				CopyTestN(d4, s0, 2, string.Empty);
				CopyTestN(d4, s2, 2, "X ");
				CopyTestN(d4, s4, 2, "XY");
				CopyTestN(d5, s0, 2, string.Empty);
				CopyTestN(d5, s2, 2, "X ");
				CopyTestN(d5, s4, 2, "XY");

				CopyTestN(d2, s0, 3, string.Empty);
				CopyTestN(d2, s2, 3, "X");
				CopyTestN(d2, s4, 3, "X");
				CopyTestN(d3, s0, 3, string.Empty);
				CopyTestN(d3, s2, 3, "X ");
				CopyTestN(d3, s4, 3, "XY");
				CopyTestN(d4, s0, 3, string.Empty);
				CopyTestN(d4, s2, 3, "X ");
				CopyTestN(d4, s4, 3, "XYZ");
				CopyTestN(d5, s0, 3, string.Empty);
				CopyTestN(d5, s2, 3, "X ");
				CopyTestN(d5, s4, 3, "XYZ");

				CopyTestN(d2, s0, 4, string.Empty);
				CopyTestN(d2, s2, 4, "X");
				CopyTestN(d2, s4, 4, "X");
				CopyTestN(d3, s0, 4, string.Empty);
				CopyTestN(d3, s2, 4, "X ");
				CopyTestN(d3, s4, 4, "XY");
				CopyTestN(d4, s0, 4, string.Empty);
				CopyTestN(d4, s2, 4, "X ");
				CopyTestN(d4, s4, 4, "XYZ");
				CopyTestN(d5, s0, 4, string.Empty);
				CopyTestN(d5, s2, 4, "X ");
				CopyTestN(d5, s4, 4, "XYZ ");

				CopyTestN(d2, s0, 5, string.Empty);
				CopyTestN(d2, s2, 5, "X");
				CopyTestN(d2, s4, 5, "X");
				CopyTestN(d3, s0, 5, string.Empty);
				CopyTestN(d3, s2, 5, "X ");
				CopyTestN(d3, s4, 5, "XY");
				CopyTestN(d4, s0, 5, string.Empty);
				CopyTestN(d4, s2, 5, "X ");
				CopyTestN(d4, s4, 5, "XYZ");
				CopyTestN(d5, s0, 5, string.Empty);
				CopyTestN(d5, s2, 5, "X ");
				CopyTestN(d5, s4, 5, "XYZ ");
			}
		}
	}
}

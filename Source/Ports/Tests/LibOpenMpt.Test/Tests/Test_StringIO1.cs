/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// Test if functions related to program version data work
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_StringIO1()
		{
			uint8[] src0 = [ (uint8)'\0', (uint8)'X', (uint8)' ', (uint8)'X' ];		// Weird empty buffer
			uint8[] src1 = [ (uint8)'X', (uint8)' ', (uint8)'\0', (uint8)'X' ];		// Weird buffer (hello Impulse Tracker)
			uint8[] src2 = [ (uint8)'X', (uint8)'Y', (uint8)'Z', (uint8)' ' ];		// Full buffer, last character space
			uint8[] src3 = [ (uint8)'X', (uint8)'Y', (uint8)'Z', (uint8)'!' ];		// Full buffer, last character non-space
			uint8[] src4 = [ (uint8)'x', (uint8)'y', (uint8)'\t', (uint8)'\n' ];    // Full buffer, containing non-space whitespace
			uint8[] dst1 = new uint8[6];											// Destination buffer, larger than source buffer
			uint8[] dst2 = new uint8[3];											// Destination buffer, smaller than source buffer

			{
				void ReadTest(ReadWriteMode mode, CPointer<uint8> dst, uint8[] src, string expectedResult)
				{
					CMemory.memset<uint8>(dst, 0x7f, dst.Size());
					MptString.WriteAutoBuf(dst).Assign(MptString.ReadBuf(mode, src));
					Assert.AreEqual(0, CString.strncmp(dst, expectedResult, dst.Size()));	// Ensure that the strings are identical

					for (size_t i = CString.strlen(dst); i < dst.Size(); i++)
						Assert.AreEqual(0, dst[i]);		// Ensure that rest of the buffer is completely nulled
				}

				void WriteTest(ReadWriteMode mode, CPointer<uint8> dst, uint8[] src, string expectedResult)
				{
					CMemory.memset<uint8>(dst, 0x7f, dst.Size());
					MptString.WriteBuf(mode, dst).Assign(MptString.ReadAutoBuf(src));
					Assert.AreEqual(0, CString.strncmp(dst, expectedResult, dst.Size()));	// Ensure that the strings are identical

					for (size_t i = strnlen(dst, dst.Size()); i < dst.Size(); i++)
						Assert.AreEqual(0, dst[i]);		// Ensure that rest of the buffer is completely nulled
				}

				// Check reading of null-terminated string into large buffer
				ReadTest(ReadWriteMode.NullTerminated, dst1, src0, string.Empty);
				ReadTest(ReadWriteMode.NullTerminated, dst1, src1, "X ");
				ReadTest(ReadWriteMode.NullTerminated, dst1, src2, "XYZ");
				ReadTest(ReadWriteMode.NullTerminated, dst1, src3, "XYZ");
				ReadTest(ReadWriteMode.NullTerminated, dst1, src4, "xy\t");

				// Check reading of string that should be null-terminated, but is maybe too long to still hold the null character
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst1, src0, string.Empty);
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst1, src1, "X ");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst1, src2, "XYZ ");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst1, src3, "XYZ!");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst1, src4, "xy\t\n");

				// Check reading of space-padded strings with ignored last character
				ReadTest(ReadWriteMode.SpacePaddedNull, dst1, src0, " X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst1, src1, "X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst1, src2, "XYZ");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst1, src3, "XYZ");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst1, src4, "xy\t");

				// Check reading of space-padded strings
				ReadTest(ReadWriteMode.SpacePadded, dst1, src0, " X X");
				ReadTest(ReadWriteMode.SpacePadded, dst1, src1, "X  X");
				ReadTest(ReadWriteMode.SpacePadded, dst1, src2, "XYZ");
				ReadTest(ReadWriteMode.SpacePadded, dst1, src3, "XYZ!");
				ReadTest(ReadWriteMode.SpacePadded, dst1, src4, "xy\t\n");

				///////////////////////////////

				// Check reading of null-terminated string into smaller buffer
				ReadTest(ReadWriteMode.NullTerminated, dst2, src0, string.Empty);
				ReadTest(ReadWriteMode.NullTerminated, dst2, src1, "X ");
				ReadTest(ReadWriteMode.NullTerminated, dst2, src2, "XY");
				ReadTest(ReadWriteMode.NullTerminated, dst2, src3, "XY");
				ReadTest(ReadWriteMode.NullTerminated, dst2, src4, "xy");

				// Check reading of string that should be null-terminated, but is maybe too long to still hold the null character
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst2, src0, string.Empty);
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst2, src1, "X ");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst2, src2, "XY");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst2, src3, "XY");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dst2, src4, "xy");

				// Check reading of space-padded strings with ignored last character
				ReadTest(ReadWriteMode.SpacePaddedNull, dst2, src0, " X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst2, src1, "X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst2, src2, "XY");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst2, src3, "XY");
				ReadTest(ReadWriteMode.SpacePaddedNull, dst2, src4, "xy");

				// Check reading of space-padded strings
				ReadTest(ReadWriteMode.SpacePadded, dst2, src0, " X");
				ReadTest(ReadWriteMode.SpacePadded, dst2, src1, "X ");
				ReadTest(ReadWriteMode.SpacePadded, dst2, src2, "XY");
				ReadTest(ReadWriteMode.SpacePadded, dst2, src3, "XY");
				ReadTest(ReadWriteMode.SpacePadded, dst2, src4, "xy");

				///////////////////////////////

				// Check writing of null-terminated string into larger buffer
				WriteTest(ReadWriteMode.NullTerminated, dst1, src0, string.Empty);
				WriteTest(ReadWriteMode.NullTerminated, dst1, src1, "X ");
				WriteTest(ReadWriteMode.NullTerminated, dst1, src2, "XYZ ");
				WriteTest(ReadWriteMode.NullTerminated, dst1, src3, "XYZ!");

				// Check writing of string that should be null-terminated, but is maybe too long to still hold the null character
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src0, string.Empty);
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src1, "X ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src2, "XYZ ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src3, "XYZ!");

				// Check writing of space-padded strings with last character set to null
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src0, "     ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src1, "X    ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src2, "XYZ  ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src3, "XYZ! ");

				// Check writing of space-padded strings
				WriteTest(ReadWriteMode.SpacePadded, dst1, src0, "      ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src1, "X     ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src2, "XYZ   ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src3, "XYZ!  ");

				///////////////////////////////

				// Check writing of null-terminated string into smaller buffer
				WriteTest(ReadWriteMode.NullTerminated, dst2, src0, string.Empty);
				WriteTest(ReadWriteMode.NullTerminated, dst2, src1, "X ");
				WriteTest(ReadWriteMode.NullTerminated, dst2, src2, "XY");
				WriteTest(ReadWriteMode.NullTerminated, dst2, src3, "XY");

				// Check writing of string that should be null-terminated, but is maybe too long to still hold the null character
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src0, string.Empty);
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src1, "X ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src2, "XYZ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src3, "XYZ");

				// Check writing of space-padded strings with last character set to null
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src0, "  ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src1, "X ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src2, "XY");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src3, "XY");

				// Check writing of space-padded strings
				WriteTest(ReadWriteMode.SpacePadded, dst2, src0, "   ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src1, "X  ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src2, "XYZ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src3, "XYZ");
			}

			{
				StdString dstString = new StdString();
				StdString src0String = new StdString(src0, src0.Size());
				StdString src1String = new StdString(src1, src1.Size());
				StdString src2String = new StdString(src2, src2.Size());
				StdString src3String = new StdString(src3, src3.Size());

				void ReadTest(ReadWriteMode mode, StdString dst, uint8[] src, string expectedResult)
				{
					dst = MptString.ReadBuf(mode, src);
					Assert.AreEqual(new StdString(expectedResult), dst);	// Ensure that the strings are identical
				}

				void WriteTest(ReadWriteMode mode, CPointer<uint8> dst, StdString src, string expectedResult)
				{
					CMemory.memset<uint8>(dst, 0x7f, dst.Size());
					MptString.WriteBuf(mode, dst).Assign(src);
					Assert.AreEqual(0, CString.strncmp(dst, expectedResult, dst.Size()));	// Ensure that the strings are identical

					for (size_t i = strnlen(dst, dst.Size()); i < dst.Size(); i++)
						Assert.AreEqual(0, dst[i]);		// Ensure that rest of the buffer is completely nulled
				}

				// Check reading of null-terminated string into std::string
				ReadTest(ReadWriteMode.NullTerminated, dstString, src0, string.Empty);
				ReadTest(ReadWriteMode.NullTerminated, dstString, src1, "X ");
				ReadTest(ReadWriteMode.NullTerminated, dstString, src2, "XYZ");
				ReadTest(ReadWriteMode.NullTerminated, dstString, src3, "XYZ");
				ReadTest(ReadWriteMode.NullTerminated, dstString, src4, "xy\t");

				// Check reading of string that should be null-terminated, but is maybe too long to still hold the null character
				ReadTest(ReadWriteMode.MaybeNullTerminated, dstString, src0, string.Empty);
				ReadTest(ReadWriteMode.MaybeNullTerminated, dstString, src1, "X ");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dstString, src2, "XYZ ");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dstString, src3, "XYZ!");
				ReadTest(ReadWriteMode.MaybeNullTerminated, dstString, src4, "xy\t\n");

				// Check reading of space-padded strings with ignored last character
				ReadTest(ReadWriteMode.SpacePaddedNull, dstString, src0, " X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dstString, src1, "X");
				ReadTest(ReadWriteMode.SpacePaddedNull, dstString, src2, "XYZ");
				ReadTest(ReadWriteMode.SpacePaddedNull, dstString, src3, "XYZ");
				ReadTest(ReadWriteMode.SpacePaddedNull, dstString, src4, "xy\t");

				// Check reading of space-padded strings
				ReadTest(ReadWriteMode.SpacePadded, dstString, src0, " X X");
				ReadTest(ReadWriteMode.SpacePadded, dstString, src1, "X  X");
				ReadTest(ReadWriteMode.SpacePadded, dstString, src2, "XYZ");
				ReadTest(ReadWriteMode.SpacePadded, dstString, src3, "XYZ!");
				ReadTest(ReadWriteMode.SpacePadded, dstString, src4, "xy\t\n");

				///////////////////////////////

				// Check writing of null-terminated string into larger buffer
				WriteTest(ReadWriteMode.NullTerminated, dst1, src0String, string.Empty);
				WriteTest(ReadWriteMode.NullTerminated, dst1, src1String, "X ");
				WriteTest(ReadWriteMode.NullTerminated, dst1, src2String, "XYZ ");
				WriteTest(ReadWriteMode.NullTerminated, dst1, src3String, "XYZ!");

				// Check writing of string that should be null-terminated, but is maybe too long to still hold the null character
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src0String, string.Empty);
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src1String, "X ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src2String, "XYZ ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst1, src3String, "XYZ!");

				// Check writing of space-padded strings with last character set to null
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src0String, "     ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src1String, "X    ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src2String, "XYZ  ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst1, src3String, "XYZ! ");

				// Check writing of space-padded strings
				WriteTest(ReadWriteMode.SpacePadded, dst1, src0String, "      ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src1String, "X     ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src2String, "XYZ   ");
				WriteTest(ReadWriteMode.SpacePadded, dst1, src3String, "XYZ!  ");

				///////////////////////////////

				// Check writing of null-terminated string into smaller buffer
				WriteTest(ReadWriteMode.NullTerminated, dst2, src0String, string.Empty);
				WriteTest(ReadWriteMode.NullTerminated, dst2, src1String, "X ");
				WriteTest(ReadWriteMode.NullTerminated, dst2, src2String, "XY");
				WriteTest(ReadWriteMode.NullTerminated, dst2, src3String, "XY");

				// Check writing of string that should be null-terminated, but is maybe too long to still hold the null character
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src0String, string.Empty);
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src1String, "X ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src2String, "XYZ");
				WriteTest(ReadWriteMode.MaybeNullTerminated, dst2, src3String, "XYZ");

				// Check writing of space-padded strings with last character set to null
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src0String, "  ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src1String, "X ");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src2String, "XY");
				WriteTest(ReadWriteMode.SpacePaddedNull, dst2, src3String, "XY");

				// Check writing of space-padded strings
				WriteTest(ReadWriteMode.SpacePadded, dst2, src0String, "   ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src1String, "X  ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src2String, "XYZ");
				WriteTest(ReadWriteMode.SpacePadded, dst2, src3String, "XYZ");
			}
		}
	}
}

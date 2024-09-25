/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Internal;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOgg.Test
{
	/// <summary>
	/// Self test of the bitwise routines; everything else is based on
	/// them, so they damned well better be solid
	/// </summary>
	[TestClass]
	public class TestBitwise
	{
		private OggPack o;
		private OggPackB ob;
		private OggPack r;
		private OggPackB rb;

		private static readonly c_ulong[] testBuffer1 =
		[
			18, 12, 103948, 4325, 543, 76, 432, 52, 3, 65, 4, 56, 32, 42, 34, 21, 1, 23, 32, 546, 456, 7,
			567, 56, 8, 8, 55, 3, 52, 342, 341, 4, 265, 7, 67, 86, 2199, 21, 7, 1, 5, 1, 4
		];
		private const c_int Test1Size = 43;

		private static readonly c_ulong[] testBuffer2 =
		[
			216531625, 1237861823, 56732452, 131, 3212421, 12325343, 34547562, 12313212,
			1233432, 534, 5, 346435231, 14436467, 7869299, 76326614, 167548585,
			85525151, 0, 12321, 1, 349528352
		];
		private const c_int Test2Size = 21;

		private static readonly c_ulong[] testBuffer3 =
		[
			1, 0, 14, 0, 1, 0, 12, 0, 1, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1,
			0, 1, 30, 1, 1, 1, 0, 0, 1, 0, 0, 0, 12, 0, 11, 0, 1, 0, 0, 1
		];
		private const c_int Test3Size = 56;

		private static readonly c_ulong[] large =
		[
			2136531625, 2137861823, 56732452, 131, 3212421, 12325343, 34547562, 12313212,
			1233432, 534, 5, 2146435231, 14436467, 7869299, 76326614, 167548585,
			85525151, 0, 12321, 1, 2146528352
		];

		private const c_int OneSize = 33;
		private static readonly c_int[] one =
		[
			146, 25, 44, 151, 195, 15, 153, 176, 233, 131, 196, 65, 85, 172, 47, 40,
			34, 242, 223, 136, 35, 222, 211, 86, 171, 50, 225, 135, 214, 75, 172,
			223, 4
		];
		private static readonly c_int[] oneB =
		[
			150, 101, 131, 33, 203, 15, 204, 216, 105, 193, 156, 65, 84, 85, 222,
			8, 139, 145, 227, 126, 34, 55, 244, 171, 85, 100, 39, 195, 173, 18,
			245, 251, 128
		];

		private const c_int TwoSize = 6;
		private static readonly c_int[] two =
		[
			61, 255, 255, 251, 231, 29
		];
		private static readonly c_int[] twoB =
		[
			247, 63, 255, 253, 249, 120
		];

		private const c_int ThreeSize = 54;
		private static readonly c_int[] three =
		[
			169, 2, 232, 252, 91, 132, 156, 36, 89, 13, 123, 176, 144, 32, 254,
			142, 224, 85, 59, 121, 144, 79, 124, 23, 67, 90, 90, 216, 79, 23, 83,
			58, 135, 196, 61, 55, 129, 183, 54, 101, 100, 170, 37, 127, 126, 10,
			100, 52, 4, 14, 18, 86, 77, 1
		];
		private static readonly c_int[] threeB =
		[
			206, 128, 42, 153, 57, 8, 183, 251, 13, 89, 36, 30, 32, 144, 183,
			130, 59, 240, 121, 59, 85, 223, 19, 228, 180, 134, 33, 107, 74, 98,
			233, 253, 196, 135, 63, 2, 110, 114, 50, 155, 90, 127, 37, 170, 104,
			200, 20, 254, 4, 58, 106, 176, 144, 0
		];

		private const c_int FourSize = 38;
		private static readonly c_int[] four =
		[
			18, 6, 163, 252, 97, 194, 104, 131, 32, 1, 7, 82, 137, 42, 129, 11, 72,
			132, 60, 220, 112, 8, 196, 109, 64, 179, 86, 9, 137, 195, 208, 122, 169,
			28, 2, 133, 0, 1
		];
		private static readonly c_int[] fourB =
		[
			36, 48, 102, 83, 243, 24, 52, 7, 4, 35, 132, 10, 145, 21, 2, 93, 2, 41,
			1, 219, 184, 16, 33, 184, 54, 149, 170, 132, 18, 30, 29, 98, 229, 67,
			129, 10, 4, 32
		];

		private const c_int FiveSize = 45;
		private static readonly c_int[] five =
		[
			169, 2, 126, 139, 144, 172, 30, 4, 80, 72, 240, 59, 130, 218, 73, 62,
			241, 24, 210, 44, 4, 20, 0, 248, 116, 49, 135, 100, 110, 130, 181, 169,
			84, 75, 159, 2, 1, 0, 132, 192, 8, 0, 0, 18, 22
		];
		private static readonly c_int[] fiveB =
		[
			1, 84, 145, 111, 245, 100, 128, 8, 56, 36, 40, 71, 126, 78, 213, 226,
			124, 105, 12, 0, 133, 128, 0, 162, 233, 242, 67, 152, 77, 205, 77,
			172, 150, 169, 129, 79, 128, 0, 6, 4, 32, 0, 27, 9, 0
		];

		private const c_int SixSize = 7;
		private static readonly c_int[] six =
		[
			17, 177, 170, 242, 169, 19, 148
		];
		private static readonly c_int[] sixB =
		[
			136, 141, 85, 79, 149, 200, 41
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bitwise_Lsb()
		{
			// Test read/write together
			// Later we test against pregenerated bitstreams
			OggPack.WriteInit(out o);

			Console.WriteLine("Small preclipped packing");
			ClipTest(testBuffer1, Test1Size, 0, one, OneSize);

			Console.WriteLine("Null bit call");
			ClipTest(testBuffer3, Test3Size, 0, two, TwoSize);

			Console.WriteLine("Large preclipped packing");
			ClipTest(testBuffer2, Test2Size, 0, three, ThreeSize);

			Console.WriteLine("32 bit preclipped packing");
			o.Reset();

			for (c_long i = 0; i < Test2Size; i++)
				o.Write(large[i], 32);

			Pointer<byte> buffer = o.GetBuffer();
			c_long bytes = o.Bytes();

			OggPack.ReadInit(out r, buffer, bytes);

			for (c_long i = 0; i < Test2Size; i++)
			{
				if (r.Look(32) == -1)
					Assert.Fail("Out of data. Failed");

				if (r.Look(32) != large[i])
					Assert.Fail("Read incorrect value");

				r.Adv(32);
			}

			if (r.Bytes() != bytes)
				Assert.Fail("Leftover bytes after read");

			Console.WriteLine("Small unclipped packing");
			ClipTest(testBuffer1, Test1Size, 7, four, FourSize);

			Console.WriteLine("Large unclipped packing");
			ClipTest(testBuffer2, Test2Size, 17, five, FiveSize);

			Console.WriteLine("Single bit unclipped packing");
			ClipTest(testBuffer3, Test3Size, 1, six, SixSize);

			Console.WriteLine("Testing read past end");
			OggPack.ReadInit(out r, new Pointer<byte>([ 0, 0, 0, 0, 0, 0, 0, 0 ]), 8);

			for (c_long i = 0; i < 64; i++)
			{
				if (r.Read(1) != 0)
					Assert.Fail("Failed; got -1 prematurely");
			}

			if ((r.Look(1) != -1) || (r.Read(1) != -1))
				Assert.Fail("Failed; read past end without -1");

			OggPack.ReadInit(out r, new Pointer<byte>([ 0, 0, 0, 0, 0, 0, 0, 0]), 8);

			if ((r.Read(30) != 0) || (r.Read(16) != 0))
				Assert.Fail("Failed 2; got -1 prematurely");

			if ((r.Look(18) != 0) || (r.Look(18) != 0))
				Assert.Fail("Failed 3; got -1 prematurely");

			if ((r.Look(19) != -1) || (r.Look(19) != -1))
				Assert.Fail("Failed; read past end without -1");

			if ((r.Look(32) != -1) || (r.Look(32) != -1))
				Assert.Fail("Failed; read past end without -1");

			o.WriteClear();

			// This is partly glassbox; we're mostly concerned about the allocation boundaries
			Console.WriteLine("Testing aligned writecopies");

			for (c_long i = 0; i < 71; i++)
			{
				for (c_long j = 0; j < 5; j++)
					CopyTest(j * 8, i);
			}

			for (c_long i = Bitwise.Buffer_Increment * 8 - 71; i < Bitwise.Buffer_Increment * 8 + 71; i++)
			{
				for (c_long j = 0; j < 5; j++)
					CopyTest(j * 8, i);
			}

			Console.WriteLine("Testing unaligned writecopies");

			for (c_long i = 0; i < 71; i++)
			{
				for (c_long j = 1; j < 40; j++)
				{
					if ((j & 0x7) != 0)
						CopyTest(j, i);
				}
			}

			for (c_long i = Bitwise.Buffer_Increment * 8 - 71; i < Bitwise.Buffer_Increment * 8 + 71; i++)
			{
				for (c_long j = 1; j < 40; j++)
				{
					if ((j & 0x7) != 0)
						CopyTest(j, i);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bitwise_Msb()
		{
			// Test read/write together
			// Later we test against pregenerated bitstreams
			OggPackB.WriteInit(out ob);

			Console.WriteLine("Small preclipped packing");
			ClipTestB(testBuffer1, Test1Size, 0, oneB, OneSize);

			Console.WriteLine("Null bit call");
			ClipTestB(testBuffer3, Test3Size, 0, twoB, TwoSize);

			Console.WriteLine("Large preclipped packing");
			ClipTestB(testBuffer2, Test2Size, 0, threeB, ThreeSize);

			Console.WriteLine("32 bit preclipped packing");
			ob.Reset();

			for (c_long i = 0; i < Test2Size; i++)
				ob.Write(large[i], 32);

			Pointer<byte> buffer = ob.GetBuffer();
			c_long bytes = ob.Bytes();

			OggPackB.ReadInit(out rb, buffer, bytes);

			for (c_long i = 0; i < Test2Size; i++)
			{
				if (rb.Look(32) == -1)
					Assert.Fail("Out of data. Failed");

				if (rb.Look(32) != large[i])
					Assert.Fail("Read incorrect value");

				rb.Adv(32);
			}

			if (rb.Bytes() != bytes)
				Assert.Fail("Leftover bytes after read");

			Console.WriteLine("Small unclipped packing");
			ClipTestB(testBuffer1, Test1Size, 7, fourB, FourSize);

			Console.WriteLine("Large unclipped packing");
			ClipTestB(testBuffer2, Test2Size, 17, fiveB, FiveSize);

			Console.WriteLine("Single bit unclipped packing");
			ClipTestB(testBuffer3, Test3Size, 1, sixB, SixSize);

			Console.WriteLine("Testing read past end");
			OggPackB.ReadInit(out rb, new Pointer<byte>([ 0, 0, 0, 0, 0, 0, 0, 0]), 8);

			for (c_long i = 0; i < 64; i++)
			{
				if (rb.Read(1) != 0)
					Assert.Fail("Failed; got -1 prematurely");
			}

			if ((rb.Look(1) != -1) || (rb.Read(1) != -1))
				Assert.Fail("Failed; read past end without -1");

			OggPackB.ReadInit(out rb, new Pointer<byte>([ 0, 0, 0, 0, 0, 0, 0, 0]), 8);

			if ((rb.Read(30) != 0) || (rb.Read(16) != 0))
				Assert.Fail("Failed 2; got -1 prematurely");

			if ((rb.Look(18) != 0) || (rb.Look(18) != 0))
				Assert.Fail("Failed 3; got -1 prematurely");

			if ((rb.Look(19) != -1) || (rb.Look(19) != -1))
				Assert.Fail("Failed; read past end without -1");

			if ((rb.Look(32) != -1) || (rb.Look(32) != -1))
				Assert.Fail("Failed; read past end without -1");

			ob.WriteClear();

			// This is partly glassbox; we're mostly concerned about the allocation boundaries
			Console.WriteLine("Testing aligned writecopies");

			for (c_long i = 0; i < 71; i++)
			{
				for (c_long j = 0; j < 5; j++)
					CopyTestB(j * 8, i);
			}

			for (c_long i = Bitwise.Buffer_Increment * 8 - 71; i < Bitwise.Buffer_Increment * 8 + 71; i++)
			{
				for (c_long j = 0; j < 5; j++)
					CopyTestB(j * 8, i);
			}

			Console.WriteLine("Testing unaligned writecopies");

			for (c_long i = 0; i < 71; i++)
			{
				for (c_long j = 1; j < 40; j++)
				{
					if ((j & 0x7) != 0)
						CopyTestB(j, i);
				}
			}

			for (c_long i = Bitwise.Buffer_Increment * 8 - 71; i < Bitwise.Buffer_Increment * 8 + 71; i++)
			{
				for (c_long j = 1; j < 40; j++)
				{
					if ((j & 0x7) != 0)
						CopyTestB(j, i);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int ILog(c_uint v)
		{
			c_int ret = 0;

			while (v != 0)
			{
				ret++;
				v >>= 1;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ClipTest(c_ulong[] b, c_int vals, c_int bits, c_int[] comp, c_int compSize)
		{
			o.Reset();

			for (c_long i = 0; i < vals; i++)
				o.Write(b[i], bits != 0 ? bits : ILog(b[i]));

			Pointer<byte> buffer = o.GetBuffer();
			c_long bytes = o.Bytes();
			Assert.AreEqual(compSize, bytes, "Wrong number of bytes");

			for (c_long i = 0; i < bytes; i++)
				Assert.AreEqual(comp[i], buffer[i], $"Wrote incorrect value af position ${i}");

			OggPack.ReadInit(out r, buffer, bytes);

			for (c_long i = 0; i < vals; i++)
			{
				c_int tBit = bits != 0 ? bits : ILog(b[i]);

				if (r.Look(tBit) == -1)
					Assert.Fail("Out of data");

				if (r.Look(tBit) != (b[i] & Tables.Mask[tBit]))
					Assert.Fail("Looked at incorrect value");

				if (tBit == 1)
				{
					if (r.Look1() != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Looked at single bit incorrect value");
				}

				if (tBit == 1)
				{
					if (r.Read1() != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Read incorrect single bit value");
				}
				else
				{
					if (r.Read(tBit) != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Read incorrect value");
				}
			}

			if (r.Bytes() != bytes)
				Assert.Fail("Leftover bytes after read");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ClipTestB(c_ulong[] b, c_int vals, c_int bits, c_int[] comp, c_int compSize)
		{
			ob.Reset();

			for (c_long i = 0; i < vals; i++)
				ob.Write(b[i], bits != 0 ? bits : ILog(b[i]));

			Pointer<byte> buffer = ob.GetBuffer();
			c_long bytes = ob.Bytes();
			Assert.AreEqual(compSize, bytes, "Wrong number of bytes");

			for (c_long i = 0; i < bytes; i++)
				Assert.AreEqual(comp[i], buffer[i], $"Wrote incorrect value af position ${i}");

			OggPackB.ReadInit(out rb, buffer, bytes);

			for (c_long i = 0; i < vals; i++)
			{
				c_int tBit = bits != 0 ? bits : ILog(b[i]);

				if (rb.Look(tBit) == -1)
					Assert.Fail("Out of data");

				if (rb.Look(tBit) != (b[i] & Tables.Mask[tBit]))
					Assert.Fail("Looked at incorrect value");

				if (tBit == 1)
				{
					if (rb.Look1() != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Looked at single bit incorrect value");
				}

				if (tBit == 1)
				{
					if (rb.Read1() != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Read incorrect single bit value");
				}
				else
				{
					if (rb.Read(tBit) != (b[i] & Tables.Mask[tBit]))
						Assert.Fail("Read incorrect value");
				}
			}

			if (rb.Bytes() != bytes)
				Assert.Fail("Leftover bytes after read");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CopyTest(c_int prefill, c_int copy)
		{
			OggPack.WriteInit(out OggPack sourceWrite);
			OggPack.WriteInit(out OggPack destWrite);

			for (c_int i = 0; i < (prefill + copy + 7) / 8; i++)
				sourceWrite.Write((c_ulong)((i ^ 0x5a) & 0xff), 8);

			Pointer<byte> source = sourceWrite.GetBuffer();
			c_long sourceBytes = sourceWrite.Bytes();

			// Prefill
			destWrite.WriteCopy(source, prefill);

			// Check buffers; verify end byte masking
			Pointer<byte> dest = destWrite.GetBuffer();
			c_long destBytes = destWrite.Bytes();
			Assert.AreEqual((prefill + 7) / 8, destBytes, "Wrong number of bytes after prefill");

			OggPack.ReadInit(out OggPack sourceRead, source, sourceBytes);
			OggPack.ReadInit(out OggPack destRead, dest, destBytes);

			for (c_int i = 0; i < prefill; i += 8)
			{
				c_int s = sourceRead.Read(prefill - i < 8 ? prefill - i : 8);
				c_int d = destRead.Read(prefill - i < 8 ? prefill - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} mismatch! byte {i / 8}");
			}

			if (prefill < destBytes)
			{
				if (destRead.Read(destBytes - prefill) != 0)
					Assert.Fail($"Prefill={prefill} mismatch! Trailing bits not zero");
			}

			// Second copy
			destWrite.WriteCopy(source, copy);

			// Check buffers: verify end byte masking
			dest = destWrite.GetBuffer();
			destBytes = destWrite.Bytes();
			Assert.AreEqual((copy + prefill + 7) / 8, destBytes, "Wrong number of bytes after prefill+copy");

			OggPack.ReadInit(out sourceRead, source, sourceBytes);
			OggPack.ReadInit(out destRead, dest, destBytes);

			for (c_int i = 0; i < prefill; i += 8)
			{
				c_int s = sourceRead.Read(prefill - i < 8 ? prefill - i : 8);
				c_int d = destRead.Read(prefill - i < 8 ? prefill - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} mismatch! byte {i / 8}");
			}

			OggPack.ReadInit(out sourceRead, source, sourceBytes);

			for (c_int i = 0; i < copy; i += 8)
			{
				c_int s = sourceRead.Read(copy - i < 8 ? copy - i : 8);
				c_int d = destRead.Read(copy - i < 8 ? copy - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} copy={copy} mismatch! byte {i / 8}");
			}

			if ((copy + prefill) < destBytes)
			{
				if (destRead.Read(destBytes - copy - prefill) != 0)
					Assert.Fail($"Prefill={prefill} copy={copy} mismatch! Trailing bits not zero");
			}

			sourceWrite.WriteClear();
			destWrite.WriteClear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CopyTestB(c_int prefill, c_int copy)
		{
			OggPackB.WriteInit(out OggPackB sourceWrite);
			OggPackB.WriteInit(out OggPackB destWrite);

			for (c_int i = 0; i < (prefill + copy + 7) / 8; i++)
				sourceWrite.Write((c_ulong)((i ^ 0x5a) & 0xff), 8);

			Pointer<byte> source = sourceWrite.GetBuffer();
			c_long sourceBytes = sourceWrite.Bytes();

			// Prefill
			destWrite.WriteCopy(source, prefill);

			// Check buffers; verify end byte masking
			Pointer<byte> dest = destWrite.GetBuffer();
			c_long destBytes = destWrite.Bytes();
			Assert.AreEqual((prefill + 7) / 8, destBytes, "Wrong number of bytes after prefill");

			OggPackB.ReadInit(out OggPackB sourceRead, source, sourceBytes);
			OggPackB.ReadInit(out OggPackB destRead, dest, destBytes);

			for (c_int i = 0; i < prefill; i += 8)
			{
				c_int s = sourceRead.Read(prefill - i < 8 ? prefill - i : 8);
				c_int d = destRead.Read(prefill - i < 8 ? prefill - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} mismatch! byte {i / 8}");
			}

			if (prefill < destBytes)
			{
				if (destRead.Read(destBytes - prefill) != 0)
					Assert.Fail($"Prefill={prefill} mismatch! Trailing bits not zero");
			}

			// Second copy
			destWrite.WriteCopy(source, copy);

			// Check buffers: verify end byte masking
			dest = destWrite.GetBuffer();
			destBytes = destWrite.Bytes();
			Assert.AreEqual((copy + prefill + 7) / 8, destBytes, "Wrong number of bytes after prefill+copy");

			OggPackB.ReadInit(out sourceRead, source, sourceBytes);
			OggPackB.ReadInit(out destRead, dest, destBytes);

			for (c_int i = 0; i < prefill; i += 8)
			{
				c_int s = sourceRead.Read(prefill - i < 8 ? prefill - i : 8);
				c_int d = destRead.Read(prefill - i < 8 ? prefill - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} mismatch! byte {i / 8}");
			}

			OggPackB.ReadInit(out sourceRead, source, sourceBytes);

			for (c_int i = 0; i < copy; i += 8)
			{
				c_int s = sourceRead.Read(copy - i < 8 ? copy - i : 8);
				c_int d = destRead.Read(copy - i < 8 ? copy - i : 8);
				Assert.AreEqual(s, d, $"Prefill={prefill} copy={copy} mismatch! byte {i / 8}");
			}

			if ((copy + prefill) < destBytes)
			{
				if (destRead.Read(destBytes - copy - prefill) != 0)
					Assert.Fail($"Prefill={prefill} copy={copy} mismatch! Trailing bits not zero");
			}

			sourceWrite.WriteClear();
			destWrite.WriteClear();
		}
	}
}

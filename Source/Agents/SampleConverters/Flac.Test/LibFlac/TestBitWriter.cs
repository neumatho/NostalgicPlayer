/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private;

namespace Polycode.NostalgicPlayer.Agent.Player.Flac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestBitWriter
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitWriter_New_Delete()
		{
			Console.WriteLine("Testing new");
			BitWriter bw = BitWriter.Flac__BitWriter_New();
			Assert.IsNotNull(bw);

			Console.WriteLine("Testing delete");
			bw.Flac__BitWriter_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitWriter_New_Init_Delete()
		{
			Console.WriteLine("Testing new");
			BitWriter bw = BitWriter.Flac__BitWriter_New();
			Assert.IsNotNull(bw);

			Console.WriteLine("Testing init");
			Assert.IsTrue(bw.Flac__BitWriter_Init());

			Console.WriteLine("Testing delete");
			bw.Flac__BitWriter_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitWriter_New_Init_Clear_Delete()
		{
			Console.WriteLine("Testing new");
			BitWriter bw = BitWriter.Flac__BitWriter_New();
			Assert.IsNotNull(bw);

			Console.WriteLine("Testing init");
			Assert.IsTrue(bw.Flac__BitWriter_Init());

			Console.WriteLine("Testing clear");
			bw.Flac__BitWriter_Clear();

			Console.WriteLine("Testing delete");
			bw.Flac__BitWriter_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitWriter_Normal_Usage()
		{
			uint64_t[] test_Pattern1 = { 0xa8aaaaaabeaaf0aa, 0xdbeaadaaaaaa0a30, 0x0000000000eeface };

			Console.WriteLine("Testing new");
			BitWriter bw = BitWriter.Flac__BitWriter_New();
			Assert.IsNotNull(bw);

			PrivateObject privateBw = new PrivateObject(new PrivateObject(bw).GetField("bw"));

			Console.WriteLine("Testing init");
			Assert.IsTrue(bw.Flac__BitWriter_Init());

			Console.WriteLine("Testing clear");
			bw.Flac__BitWriter_Clear();

			uint32_t words = 0;
			uint32_t bits = 0;

			Assert.IsTrue(bw.Flac__BitWriter_Write_Raw_UInt32(0x1, 1) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0x1, 2) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0xa, 5) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0xf0, 8) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0x2aa, 10) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0xf, 4) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0xaaaaaaaa, 32) &&
						  bw.Flac__BitWriter_Write_Zeroes(4) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0x3, 2) &&
			              bw.Flac__BitWriter_Write_Zeroes(8) &&
			              bw.Flac__BitWriter_Write_Raw_UInt64(0xaaaaaaaadeadbeef, 64) &&
			              bw.Flac__BitWriter_Write_Raw_UInt32(0xace, 12));

			// We wrote 152 bits (=19 bytes) to the bitwriter
			words = 152 / Constants.Flac__Bits_Per_Word;
			bits = 152 - words * Constants.Flac__Bits_Per_Word;

			Assert.AreEqual(words, privateBw.GetField("Words"));
			Assert.AreEqual(bits, privateBw.GetField("Bits"));
			Assert.AreEqual(test_Pattern1[0], ((uint64_t[])privateBw.GetField("Buffer"))[0]);
			Assert.AreEqual(test_Pattern1[1], ((uint64_t[])privateBw.GetField("Buffer"))[1]);
			Assert.AreEqual(test_Pattern1[2], (uint64_t)privateBw.GetField("Accum") & 0x00ffffff);

			Console.WriteLine("Testing raw_uint32 some more");
			Assert.IsTrue(bw.Flac__BitWriter_Write_Raw_UInt32(0x3d, 6));

			bits += 6;
			test_Pattern1[words] <<= 6;
			test_Pattern1[words] |= 0x3d;

			Assert.AreEqual(words, privateBw.GetField("Words"));
			Assert.AreEqual(bits, privateBw.GetField("Bits"));
			Assert.AreEqual(test_Pattern1[0], ((uint64_t[])privateBw.GetField("Buffer"))[0]);
			Assert.AreEqual(test_Pattern1[1], ((uint64_t[])privateBw.GetField("Buffer"))[1]);
			Assert.AreEqual(test_Pattern1[2], (uint64_t)privateBw.GetField("Accum") & 0x3fffffff);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			uint32_t Words_To_Bits(uint32_t words)
			{
				return words * Constants.Flac__Bits_Per_Word;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			uint32_t Total_Bits()
			{
				return Words_To_Bits((uint32_t)privateBw.GetField("Words")) + (uint32_t)privateBw.GetField("Bits");
			}

			Console.WriteLine("Testing utf8_uint32(0x00000000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x00000000);
			Assert.IsTrue((Total_Bits() == 8) && (((uint64_t)privateBw.GetField("Accum") & 0xff) == 0));

			Console.WriteLine("Testing utf8_uint32(0x0000007f)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x0000007f);
			Assert.IsTrue((Total_Bits() == 8) && (((uint64_t)privateBw.GetField("Accum") & 0xff) == 0x7f));

			Console.WriteLine("Testing utf8_uint32(0x00000080)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x00000080);
			Assert.IsTrue((Total_Bits() == 16) && (((uint64_t)privateBw.GetField("Accum") & 0xffff) == 0xc280));

			Console.WriteLine("Testing utf8_uint32(0x000007ff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x000007ff);
			Assert.IsTrue((Total_Bits() == 16) && (((uint64_t)privateBw.GetField("Accum") & 0xffff) == 0xdfbf));

			Console.WriteLine("Testing utf8_uint32(0x00000800)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x00000800);
			Assert.IsTrue((Total_Bits() == 24) && (((uint64_t)privateBw.GetField("Accum") & 0xffffff) == 0xe0a080));

			Console.WriteLine("Testing utf8_uint32(0x0000ffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x0000ffff);
			Assert.IsTrue((Total_Bits() == 24) && (((uint64_t)privateBw.GetField("Accum") & 0xffffff) == 0xefbfbf));

			Console.WriteLine("Testing utf8_uint32(0x00010000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x00010000);
			Assert.IsTrue((Total_Bits() == 32) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffff) == 0xf0908080));

			Console.WriteLine("Testing utf8_uint32(0x001fffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x001fffff);
			Assert.IsTrue((Total_Bits() == 32) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffff) == 0xf7bfbfbf));

			Console.WriteLine("Testing utf8_uint32(0x00200000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x00200000);
			Assert.IsTrue((Total_Bits() == 40) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffff) == 0xf888808080));

			Console.WriteLine("Testing utf8_uint32(0x03ffffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x03ffffff);
			Assert.IsTrue((Total_Bits() == 40) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffff) == 0xfbbfbfbfbf));

			Console.WriteLine("Testing utf8_uint32(0x04000000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x04000000);
			Assert.IsTrue((Total_Bits() == 48) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffff) == 0xfc8480808080));

			Console.WriteLine("Testing utf8_uint32(0x7fffffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt32(0x7fffffff);
			Assert.IsTrue((Total_Bits() == 48) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffff) == 0xfdbfbfbfbfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000000000000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000000000000);
			Assert.IsTrue((Total_Bits() == 8) && (((uint64_t)privateBw.GetField("Accum") & 0xff) == 0));

			Console.WriteLine("Testing utf8_uint64(0x000000000000007f)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x000000000000007f);
			Assert.IsTrue((Total_Bits() == 8) && (((uint64_t)privateBw.GetField("Accum") & 0xff) == 0x7f));

			Console.WriteLine("Testing utf8_uint64(0x0000000000000080)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000000000080);
			Assert.IsTrue((Total_Bits() == 16) && (((uint64_t)privateBw.GetField("Accum") & 0xffff) == 0xc280));

			Console.WriteLine("Testing utf8_uint64(0x00000000000007ff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x00000000000007ff);
			Assert.IsTrue((Total_Bits() == 16) && (((uint64_t)privateBw.GetField("Accum") & 0xffff) == 0xdfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000000000800)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000000000800);
			Assert.IsTrue((Total_Bits() == 24) && (((uint64_t)privateBw.GetField("Accum") & 0xffffff) == 0xe0a080));

			Console.WriteLine("Testing utf8_uint64(0x000000000000ffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x000000000000ffff);
			Assert.IsTrue((Total_Bits() == 24) && (((uint64_t)privateBw.GetField("Accum") & 0xffffff) == 0xefbfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000000010000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000000010000);
			Assert.IsTrue((Total_Bits() == 32) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffff) == 0xf0908080));

			Console.WriteLine("Testing utf8_uint64(0x00000000001fffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x00000000001fffff);
			Assert.IsTrue((Total_Bits() == 32) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffff) == 0xf7bfbfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000000200000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000000200000);
			Assert.IsTrue((Total_Bits() == 40) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffff) == 0xf888808080));

			Console.WriteLine("Testing utf8_uint64(0x0000000003ffffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000003ffffff);
			Assert.IsTrue((Total_Bits() == 40) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffff) == 0xfbbfbfbfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000004000000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000004000000);
			Assert.IsTrue((Total_Bits() == 48) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffff) == 0xfc8480808080));

			Console.WriteLine("Testing utf8_uint64(0x000000007fffffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x000000007fffffff);
			Assert.IsTrue((Total_Bits() == 48) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffff) == 0xfdbfbfbfbfbf));

			Console.WriteLine("Testing utf8_uint64(0x0000000080000000)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000080000000);
			Assert.IsTrue((Total_Bits() == 56) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffffff) == 0xfe828080808080));

			Console.WriteLine("Testing utf8_uint64(0x0000000fffffffff)");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Utf8_UInt64(0x0000000fffffffff);
			Assert.IsTrue((Total_Bits() == 56) && (((uint64_t)privateBw.GetField("Accum") & 0xffffffffffffff) == 0xfebfbfbfbfbfbf));

			Console.WriteLine("Testing grow");
			bw.Flac__BitWriter_Clear();
			bw.Flac__BitWriter_Write_Raw_UInt32(0x5, 4);

			uint32_t j = (uint32_t)privateBw.GetField("Capacity");
			for (uint32_t i = 0; i < j; i++)
				bw.Flac__BitWriter_Write_Raw_UInt32(0xaaaaaaaa, 32);

			Assert.IsTrue((Total_Bits() == j * 32 + 4) && (((uint64_t[])privateBw.GetField("Buffer"))[0] == 0xaaaaaaaaaaaaaa5a) && (((uint64_t)privateBw.GetField("Accum") & 0xf) == 0xa));

			Console.WriteLine("Testing free");
			bw.Flac__BitWriter_Free();

			Console.WriteLine("Testing delete");
			bw.Flac__BitWriter_Delete();
		}
	}
}

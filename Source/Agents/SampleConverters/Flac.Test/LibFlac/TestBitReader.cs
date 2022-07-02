/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private;

namespace Polycode.NostalgicPlayer.Agent.Player.Flac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestBitReader
	{
		private Flac__byte[] data;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestInitialize]
		public void Initialize()
		{
			data = new Flac__byte[32];

			// Initialize data reproducibly with pseudo-random values
			for (uint32_t i = 0; i < 32; i++)
				data[i] = (Flac__byte)(i * 8 + 7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitReader_New_Delete()
		{
			Console.WriteLine("Testing new");
			BitReader br = BitReader.Flac__BitReader_New();
			Assert.IsNotNull(br);

			Console.WriteLine("Testing delete");
			br.Flac__BitReader_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitReader_New_Init_Delete()
		{
			Console.WriteLine("Testing new");
			BitReader br = BitReader.Flac__BitReader_New();
			Assert.IsNotNull(br);

			Console.WriteLine("Testing init");
			Assert.IsTrue(br.Flac__BitReader_Init(Read_Callback, data));

			Console.WriteLine("Testing delete");
			br.Flac__BitReader_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitReader_New_Init_Clear_Delete()
		{
			Console.WriteLine("Testing new");
			BitReader br = BitReader.Flac__BitReader_New();
			Assert.IsNotNull(br);

			Console.WriteLine("Testing init");
			Assert.IsTrue(br.Flac__BitReader_Init(Read_Callback, data));

			Console.WriteLine("Testing clear");
			Assert.IsTrue(br.Flac__BitReader_Clear());

			Console.WriteLine("Testing delete");
			br.Flac__BitReader_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_BitReader_Normal_Usage()
		{
			Console.WriteLine("Testing new");
			BitReader br = BitReader.Flac__BitReader_New();
			Assert.IsNotNull(br);

			PrivateObject privateBr = new PrivateObject(new PrivateObject(br).GetField("br"));

			Console.WriteLine("Testing init");
			Assert.IsTrue(br.Flac__BitReader_Init(Read_Callback, data));

			Console.WriteLine("Testing clear");
			Assert.IsTrue(br.Flac__BitReader_Clear());

			// What we think br.consumed_Words and br.consumed_Bits should be
			uint32_t words = 0;
			uint32_t bits = 0;

			Flac__uint16[] expected_Crcs = { 0x5e4c, 0x7f6b, 0x2272, 0x42bf };

			Flac__uint32 val_UInt32;
			Flac__uint64 val_UInt64;

			Console.WriteLine("Testing raw reads");
			Flac__bool ok = br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 1) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 2) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 5) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 8) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 10) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 4) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 32) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 4) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 2) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 8) &&
			                br.Flac__BitReader_Read_Raw_UInt64(out val_UInt64, 64) &&
			                br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 12);
			Assert.IsTrue(ok);

			// We read 152 bits (=19 bytes) from the bitreader
			words = 152 / Constants.Flac__Bits_Per_Word;
			bits = 152 - words * Constants.Flac__Bits_Per_Word;

			Assert.AreEqual(words, privateBr.GetField("consumed_Words"));
			Assert.AreEqual(bits, privateBr.GetField("consumed_Bits"));

			Flac__uint16 crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(expected_Crcs[0], crc);

			Console.WriteLine("Testing CRC reset");
			br.Flac__BitReader_Clear();
			br.Flac__BitReader_Reset_Read_Crc16(0xffff);
			crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(0xffff, crc);

			br.Flac__BitReader_Reset_Read_Crc16(0);
			crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(0, crc);

			br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 16);
			br.Flac__BitReader_Reset_Read_Crc16(0);
			br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 32);
			crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(expected_Crcs[1], crc);

			Console.WriteLine("Testing unaligned < 32 bit reads");
			br.Flac__BitReader_Clear();
			br.Flac__BitReader_Skip_Bits_No_Crc(8);
			br.Flac__BitReader_Reset_Read_Crc16(0);
			ok = br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 1) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 2) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 5) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 8);
			Assert.IsTrue(ok);

			crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(expected_Crcs[2], crc);

			Console.WriteLine("Testing unaligned < 64 bit reads");
			br.Flac__BitReader_Clear();
			br.Flac__BitReader_Skip_Bits_No_Crc(8);
			br.Flac__BitReader_Reset_Read_Crc16(0);
			ok = br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 1) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 2) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 5) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 8) &&
			     br.Flac__BitReader_Read_Raw_UInt32(out val_UInt32, 32);
			Assert.IsTrue(ok);

			crc = br.Flac__BitReader_Get_Read_Crc16();
			Assert.AreEqual(expected_Crcs[3], crc);

			Console.WriteLine("Testing free");
			br.Flac__BitReader_Free();

			Console.WriteLine("Testing delete");
			br.Flac__BitReader_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Callback(Span<Flac__byte> buffer, ref size_t bytes, object client_Data)
		{
			if (bytes > 32)
				bytes = 32;

			new Span<Flac__byte>((Flac__byte[])client_Data, 0, (int)bytes).CopyTo(buffer);

			return true;
		}
	}
}

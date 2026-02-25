/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestRSid
	{
		private const int BufferSize = 128;

		private const int VersionLo = 5;
		private const int DataOffsetLo = 7;
		private const int LoadAddressLo = 9;
		private const int InitAddressHi = 10;
		private const int InitAddressLo = 11;
		private const int PlayAddressLo = 13;
		private const int SongsHi = 14;
		private const int SongsLo = 15;
		private const int SpeedLoLo = 21;

		private const int StartPage = 120;
		private const int PageLength = 121;
		private const int SecondSidAddress = 122;
		private const int ThirdSidAddress = 123;

		private readonly uint8_t[] bufferRSid = new uint8_t[BufferSize]
		{
			0x52, 0x53, 0x49, 0x44,		// Magic ID
			0x00, 0x02,					// Version
			0x00, 0x7c,					// Data offset
			0x00, 0x00,					// Load address
			0x00, 0x00,					// Init address
			0x00, 0x00,					// Play address
			0x00, 0x01,					// Songs
			0x00, 0x00,					// Start song
			0x00, 0x00, 0x00, 0x00,		// Speed
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,	// Name
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,	// Author
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,	// Released
			0x00, 0x00,					// Flags
			0x00,						// Start page
			0x00,						// Page length
			0x00,						// Second SID address
			0x00,						// Third SID address
			0xe8, 0x07, 0x00, 0x00		// Data
		};

		private readonly uint8_t[] data = new uint8_t[BufferSize];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TestRSid()
		{
			Array.Copy(bufferRSid, data, BufferSize);
		}



		/********************************************************************/
		/// <summary>
		/// Check that unmodified data loads ok
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestLoadOk()
		{
			SidTune tune = LoadTune();
			Assert.IsTrue(tune.GetStatus());
			Assert.AreEqual("No errors", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Version must be at least 2 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestUnsupportedVersion()
		{
			data[VersionLo] = 0x01;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("Unsupported RSID version", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Load address must always be 0 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongLoadAddress()
		{
			data[LoadAddressLo] = 0xff;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("File contains invalid data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Actual load address must NOT be less that $07e8 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongActualLoadAddress()
		{
			data[124] = 0xe7;
			data[125] = 0x07;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("Bad address data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Play address must always be 0 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongPlayAddress()
		{
			data[PlayAddressLo] = 0xff;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("File contains invalid data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Speed must always be 0 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongSpeed()
		{
			data[SpeedLoLo] = 0xff;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("File contains invalid data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Data offset must always be 0x007c for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongDataOffset()
		{
			data[DataOffsetLo] = 0x76;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("Bad address data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Init address must never point to a ROM area for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongInitAddressRom()
		{
			data[InitAddressHi] = 0xb0;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("Bad address data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// Init address must never be lower than $07e8 for RSID files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongInitAddressTooLow()
		{
			data[InitAddressHi] = 0x07;
			data[InitAddressLo] = 0xe7;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());
			Assert.AreEqual("Bad address data", tune.StatusString());
		}



		/********************************************************************/
		/// <summary>
		/// The maximum number of songs is 256
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestTooManySongs()
		{
			data[SongsHi] = 0x01;
			data[SongsLo] = 0x01;

			SidTune tune = LoadTune();
			Assert.AreEqual(256U, tune.GetInfo().Songs());
		}



		/********************************************************************/
		/// <summary>
		/// The song number to be played by default has a default of 1
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestDefaultStartSong()
		{
			SidTune tune = LoadTune();
			Assert.AreEqual(1U, tune.GetInfo().StartSong());
		}



		/********************************************************************/
		/// <summary>
		/// If 'start page' is 0 or 0xff, 'page length' must be set to 0
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongPageLength()
		{
			data[StartPage] = 0xff;
			data[PageLength] = 0x77;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().RelocPages());
		}

		/*** TEST v3 ***/

		/********************************************************************/
		/// <summary>
		/// $d420 is a valid second SID address
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestSecondSidAddressOk()
		{
			data[VersionLo] = 0x03;
			data[SecondSidAddress] = 0x42;

			SidTune tune = LoadTune();
			Assert.AreEqual(0xd420, tune.GetInfo().SidChipBase(1));
		}



		/********************************************************************/
		/// <summary>
		/// SecondSidAddress: Only even values are valid
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongSecondSidAddressOdd()
		{
			data[VersionLo] = 0x03;
			data[SecondSidAddress] = 0x43;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().SidChipBase(1));
		}



		/********************************************************************/
		/// <summary>
		/// SecondSidAddress: Ranges $00-$41 ($d000-$d410) and $80-$df
		/// ($d800-$ddf0) are invalid
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongSecondSidAddressOutOfRange()
		{
			data[VersionLo] = 0x03;
			data[SecondSidAddress] = 0x80;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().SidChipBase(1));
		}

		/*** TEST v4 ***/

		/********************************************************************/
		/// <summary>
		/// $d500 is a valid third SID address
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestThirdSidAddressOk()
		{
			data[VersionLo] = 0x04;
			data[SecondSidAddress] = 0x42;
			data[ThirdSidAddress] = 0x50;

			SidTune tune = LoadTune();
			Assert.AreEqual(0xd500, tune.GetInfo().SidChipBase(2));
		}



		/********************************************************************/
		/// <summary>
		/// ThirdSidAddress: Only even values are valid
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongThirdSidAddressOdd()
		{
			data[VersionLo] = 0x04;
			data[SecondSidAddress] = 0x42;
			data[ThirdSidAddress] = 0x43;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().SidChipBase(2));
		}



		/********************************************************************/
		/// <summary>
		/// ThirdSidAddress: Ranges $00-$41 ($d000-$d410) and $80-$df
		/// ($d800-$ddf0) are invalid
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongThirdSidAddressOutOfRange()
		{
			data[VersionLo] = 0x04;
			data[SecondSidAddress] = 0x42;
			data[ThirdSidAddress] = 0x80;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().SidChipBase(2));
		}



		/********************************************************************/
		/// <summary>
		/// The address of the third SID cannot be the same as the second SID
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongThirdSidAddressLikeSecond()
		{
			data[VersionLo] = 0x04;
			data[SecondSidAddress] = 0x42;
			data[ThirdSidAddress] = 0x42;

			SidTune tune = LoadTune();
			Assert.AreEqual(0, tune.GetInfo().SidChipBase(2));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load the tune
		/// </summary>
		/********************************************************************/
		private SidTune LoadTune()
		{
			using (ModuleStream moduleStream = new ModuleStream(new MemoryStream(data), false))
			{
				return new SidTune(new PlayerFileInfo("abc.sid", moduleStream, new NormalFileLoader("abc.sid")));
			}
		}
		#endregion
	}
}

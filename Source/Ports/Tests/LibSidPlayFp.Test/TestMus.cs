﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using NUnit.Framework;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestMus
	{
		private const int BufferSize = 26;

		private const int Voice1LenLo = 2;

		private readonly uint8_t[] bufferMus = new uint8_t[BufferSize]
		{
			0x52, 0x53,							// Load address
			0x04, 0x00,							// Length of the data for Voice 1
			0x04, 0x00,							// Length of the data for Voice 2
			0x04, 0x00,							// Length of the data for Voice 3
			0x00, 0x00, 0x01, 0x4f,				// Data for Voice 1
			0x00, 0x00, 0x01, 0x4f,				// Data for Voice 2
			0x00, 0x01, 0x01, 0x4f,				// Data for Voice 3
			0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x00	// Text description
		};

		private readonly uint8_t[] data = new uint8_t[BufferSize];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[SetUp]
		public void Initialize()
		{
			Array.Copy(bufferMus, data, BufferSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestPlayerAddress()
		{
			SidTune tune = LoadTune();

			Assert.That(tune.GetInfo().InitAddr(), Is.EqualTo(0xec60));
			Assert.That(tune.GetInfo().PlayAddr(), Is.EqualTo(0xec80));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestWrongVoiceLength()
		{
			data[Voice1LenLo] = 0x76;

			SidTune tune = LoadTune();
			Assert.That(tune.GetStatus(), Is.False);

			Assert.That(tune.StatusString(), Is.EqualTo("Could not determine file format"));
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
				return new SidTune(new PlayerFileInfo("abc.mus", moduleStream, new NormalFileLoader("abc.mus", null)));
			}
		}
		#endregion
	}
}

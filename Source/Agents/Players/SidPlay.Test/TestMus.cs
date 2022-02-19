/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
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
		[TestInitialize]
		public void Initialize()
		{
			Array.Copy(bufferMus, data, BufferSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestPlayerAddress()
		{
			SidTune tune = LoadTune();

			Assert.AreEqual(0xec60, tune.GetInfo().InitAddr());
			Assert.AreEqual(0xec80, tune.GetInfo().PlayAddr());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWrongVoiceLength()
		{
			data[Voice1LenLo] = 0x76;

			SidTune tune = LoadTune();
			Assert.IsFalse(tune.GetStatus());

			Assert.AreEqual("Could not determine file format", tune.StatusString());
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

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestWaveformGenerator
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestClockShiftRegister()
		{
			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();

			generator.shift_register = 0x35555e;
			generator.Clock_Shift_Register(0);

			Assert.AreEqual(0x9e0U, generator.noise_output);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestNoiseOutput()
		{
			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();

			generator.shift_register = 0x35555f;
			generator.Set_Noise_Output();

			Assert.AreEqual(0xe20U, generator.noise_output);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestWriteShiftRegister()
		{
			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();

			generator.waveform = 0xf;
			generator.Write_Shift_Register();

			Assert.AreEqual(0x2dd6ebU, generator.shift_register);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestSetTestBit()
		{
			matrix_t tables = WaveformCalculator.GetInstance().BuildTable(ChipModel.MOS6581);

			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();
			generator.shift_register = 0x35555e;
			generator.SetWaveformModels(tables);

			generator.WriteControl_Reg(0x08);	// Set test bit
			generator.WriteControl_Reg(0x00);	// Unset test bit

			Assert.AreEqual(0x9f0U, generator.noise_output);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestNoiseWriteBack1()
		{
			matrix_t tables = WaveformCalculator.GetInstance().BuildTable(ChipModel.MOS6581);
			float[] dac = new float[4096];

			WaveformGenerator modulator = new WaveformGenerator();

			WaveformGenerator generator = new WaveformGenerator();
			generator.SetWaveformModels(tables);
			generator.SetDac(dac);
			generator.Reset();

			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x90);
			generator.Clock();
			generator.Output(modulator);

			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xfc, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0x6c, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xd8, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xb1, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xd8, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0x6a, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xb1, generator.ReadOsc());
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.AreEqual(0xf0, generator.ReadOsc());
		}
	}
}

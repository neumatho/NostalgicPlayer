/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.ReSidFp;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
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
		public void TestShiftRegisterInitValue()
		{
			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();

			Assert.AreEqual(0x3fffffU, generator.shift_register);
		}



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

			// Shift phase 1
			generator.test_or_reset = false;
			generator.shift_latch = generator.shift_register;

			// Shift phase 2
			generator.Shift_Phase2(0, 0);

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
			matrix_t waveTables = WaveformCalculator.GetInstance().GetWaveTable();
			matrix_t tables = WaveformCalculator.GetInstance().BuildPulldownTable(ChipModel.MOS6581, CombinedWaveforms.AVERAGE);

			WaveformGenerator generator = new WaveformGenerator();
			generator.Reset();
			generator.shift_register = 0x35555e;
			generator.SetWaveformModels(waveTables);
			generator.SetPulldownModels(tables);

			generator.WriteControl_Reg(0x08);	// Set test bit
			generator.Clock();
			generator.WriteControl_Reg(0x00);	// Unset test bit
			generator.Clock();

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
			matrix_t waveTables = WaveformCalculator.GetInstance().GetWaveTable();
			matrix_t tables = WaveformCalculator.GetInstance().BuildPulldownTable(ChipModel.MOS6581, CombinedWaveforms.AVERAGE);

			WaveformGenerator modulator = new WaveformGenerator();

			WaveformGenerator generator = new WaveformGenerator();
			generator.SetModel(true);	// 6581
			generator.SetWaveformModels(waveTables);
			generator.SetPulldownModels(tables);
			generator.Reset();

			// Switch from noise to noise+triangle
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

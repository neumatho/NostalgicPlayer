/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using NUnit.Framework;
using Polycode.NostalgicPlayer.Ports.ReSidFp;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestWaveformGenerator
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
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
		[Test]
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
		[Test]
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
		[Test]
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
		[Test]
		public void TestSetTestBit()
		{
			matrix_t waveTables = WaveformCalculator.GetInstance().GetWaveTable();
			matrix_t tables = WaveformCalculator.GetInstance().BuildPulldownTable(ChipModel.MOS6581);

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
		[Test]
		public void TestNoiseWriteBack1()
		{
			matrix_t waveTables = WaveformCalculator.GetInstance().GetWaveTable();
			matrix_t tables = WaveformCalculator.GetInstance().BuildPulldownTable(ChipModel.MOS6581);

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

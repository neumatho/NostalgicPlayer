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

			Assert.That(generator.shift_register, Is.EqualTo(0x3fffffU));
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

			// Shift phase 1
			generator.test_or_reset = false;
			generator.shift_latch = generator.shift_register;

			// Shift phase 2
			generator.Shift_Phase2(0, 0);

			Assert.That(generator.noise_output, Is.EqualTo(0x9e0U));
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

			Assert.That(generator.noise_output, Is.EqualTo(0xe20U));
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

			Assert.That(generator.shift_register, Is.EqualTo(0x2dd6ebU));
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

			Assert.That(generator.noise_output, Is.EqualTo(0x9f0U));
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
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xfc));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0x6c));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xd8));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xb1));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xd8));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0x6a));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xb1));
			generator.WriteControl_Reg(0x88);
			generator.Clock();
			generator.Output(modulator);
			generator.WriteControl_Reg(0x80);
			generator.Clock();
			generator.Output(modulator);
			Assert.That(generator.ReadOsc(), Is.EqualTo(0xf0));
		}
	}
}

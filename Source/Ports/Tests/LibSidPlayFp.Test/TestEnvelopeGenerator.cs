/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using NUnit.Framework;
using Polycode.NostalgicPlayer.Ports.ReSidFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestEnvelopeGenerator
	{
		private EnvelopeGenerator generator;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[SetUp]
		public void Initialize()
		{
			generator = new EnvelopeGenerator();
			generator.Reset();
			generator.envelope_counter = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestAdsrDelayBug()
		{
			// If the rate counter comparison value is set below the current value of the
			// rate counter, the counter will continue counting up until it wraps around
			// to zero at 2^15 = 0x8000, and then count rate_period - 1 before the
			// envelope can constantly be stepped
			generator.WriteAttack_Decay(0x70);

			generator.WriteControl_Reg(0x01);

			// Wait 200 cycles
			for (int i = 0; i < 200; i++)
				generator.Clock();

			Assert.That(generator.ReadEnv(), Is.EqualTo(1));

			// Set lower attack time
			// should theoretically clock after 63 cycles
			generator.WriteAttack_Decay(0x20);

			// Wait another 200 cycles
			for (int i = 0; i < 200; i++)
				generator.Clock();

			Assert.That(generator.ReadEnv(), Is.EqualTo(1));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestFlipFFto00()
		{
			// The envelope counter can flip from 0xff to 0x00 by changing state to
			// release, then to attack. The envelope counter is then frozen at
			// zero; to unlock this situation the state must be changed to release,
			// then to attack

			generator.WriteAttack_Decay(0x77);
			generator.WriteSustain_Release(0x77);

			generator.WriteControl_Reg(0x01);

			do
			{
				generator.Clock();
			}
			while (generator.ReadEnv() != 0xff);

			generator.WriteControl_Reg(0x00);

			// Run for three clocks, accounting for state pipeline
			generator.Clock();
			generator.Clock();
			generator.Clock();
			generator.WriteControl_Reg(0x01);

			// Wait at least 313 cycles
			// so the counter is clocked once
			for (int i = 0; i < 315; i++)
				generator.Clock();

			Assert.That(generator.ReadEnv(), Is.EqualTo(0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestFlip00toFF()
		{
			// The envelope counter can flip from 0x00 to 0xff by changing state to
			// attack, then to release. The envelope counter will then continue
			// counting down in the release state

			generator.counter_enabled = false;

			generator.WriteAttack_Decay(0x77);
			generator.WriteSustain_Release(0x77);
			generator.Clock();
			Assert.That(generator.ReadEnv(), Is.EqualTo(0));

			generator.WriteControl_Reg(0x01);

			// Run for three clocks, accounting for state pipeline
			generator.Clock();
			generator.Clock();
			generator.Clock();
			generator.WriteControl_Reg(0x00);

			// Wait at least 313 cycles
			// so the counter is clocked once
			for (int i = 0; i < 315; i++)
				generator.Clock();

			Assert.That(generator.ReadEnv(), Is.EqualTo(0xff));
		}
	}
}

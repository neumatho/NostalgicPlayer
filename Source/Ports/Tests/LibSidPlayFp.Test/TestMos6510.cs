/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using NUnit.Framework;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestMos6510
	{
		#region TestCpu class
		private sealed class TestCpu : Mos6510
		{
			private readonly uint8_t[] mem = new uint8_t[65536];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public TestCpu(EventScheduler scheduler) : base(scheduler)
			{
				mem[0xfffc] = 0x00;
				mem[0xfffd] = 0x10;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void SetMem(uint8_t offset, uint8_t opcode)
			{
				mem[0x1000 + offset] = opcode;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool Check(uint8_t opcode)
			{
				return GetInstr() == opcode;
			}

			#region Overrides
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			protected override byte CpuRead(uint_least16_t addr)
			{
				return mem[addr];
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			protected override void CpuWrite(uint_least16_t addr, uint8_t data)
			{
				mem[addr] = data;
			}
			#endregion

			#region Private methods
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private uint8_t GetInstr()
			{
				return (uint8_t)(cycleCount >> 3);
			}
			#endregion
		}
		#endregion

		private EventScheduler scheduler;
		private TestCpu cpu;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[SetUp]
		public void Initialize()
		{
			scheduler = new EventScheduler();
			cpu = new TestCpu(scheduler);

			scheduler.Reset();
			cpu.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is recognized at T0 and triggered on the following T1
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=58eaeaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=4&irq1=100&logmore=rdy,irq
		/********************************************************************/
		[Test]
		public void TestNop()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 as the I flag is still set.
		/// It is recognized during the following opcode's T0
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=7858eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=4&irq1=100&logmore=rdy,irq
		/********************************************************************/
		[Test]
		public void TestCli()
		{
			cpu.SetMem(0, Opcodes.SEIn);
			cpu.SetMem(1, Opcodes.CLIn);
			cpu.SetMem(2, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.NOPn));

			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is recognized at T0 during CLI as the I flag is cleared
		/// while the CPU is stalled by RDY line.
		/// It is triggered on the following T1
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=7858eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=4&irq1=100&logmore=rdy,irq&rdy0=6&rdy1=8
		/********************************************************************/
		[Test]
		public void TestCliRdy()
		{
			cpu.SetMem(0, Opcodes.SEIn);
			cpu.SetMem(1, Opcodes.CLIn);
			cpu.SetMem(2, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			cpu.SetRdy(false);
			scheduler.Clock();	// CPU stalled but the I flag is being cleared
			cpu.SetRdy(true);
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is recognized at T0 as the I flag is still cleared.
		/// It is triggered on the following T1
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=5878eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=4&irq1=100&logmore=rdy,irq
		/********************************************************************/
		[Test]
		public void TestSei()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.SEIn);
			cpu.SetMem(2, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is recognized at T0 during SEI even if the I flag is
		/// set while the CPU is stalled by RDY line
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=5878eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=4&irq1=100&logmore=rdy,irq&rdy0=6&rdy1=8
		/********************************************************************/
		[Test]
		public void TestSeiRdy()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.SEIn);
			cpu.SetMem(2, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			cpu.SetRdy(false);
			scheduler.Clock();	// CPU stalled but the I flag is being set
			cpu.SetRdy(true);
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 during SEI even if the I flag
		/// is set while the CPU is stalled by RDY line
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=20&a=0010&d=5878eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=6&irq1=100&logmore=rdy,irq&rdy0=6&rdy1=8
		/********************************************************************/
		[Test]
		public void TestSeiRdy2()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.SEIn);
			cpu.SetMem(2, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			scheduler.Clock();	// T1
			cpu.TriggerIrq();
			cpu.SetRdy(false);
			scheduler.Clock();	// CPU stalled but the I flag is being set
			cpu.SetRdy(true);
			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.NOPn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 as the I flag is still set.
		/// It is recognized during the following opcode's T0
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=30&a=0010&d=58087828eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=14&irq1=100&logmore=rdy,irq
		/********************************************************************/
		[Test]
		public void TestPlp1()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.PHPn);
			cpu.SetMem(2, Opcodes.SEIn);
			cpu.SetMem(3, Opcodes.PLPn);
			cpu.SetMem(4, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T0

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T3
			scheduler.Clock();	// T0
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.NOPn));

			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 as the I flag is still set.
		/// It is recognized during the following opcode's T0
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=30&a=0010&d=58087828eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=14&irq1=100&logmore=rdy,irq&rdy0=20&rdy1=22
		/********************************************************************/
		[Test]
		public void TestPlp1Rdy()
		{
			cpu.SetMem(0, Opcodes.CLIn);
			cpu.SetMem(1, Opcodes.PHPn);
			cpu.SetMem(2, Opcodes.SEIn);
			cpu.SetMem(3, Opcodes.PLPn);
			cpu.SetMem(4, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T0

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T3
			cpu.SetRdy(false);
			scheduler.Clock();	// CPU stalled
			cpu.SetRdy(true);
			scheduler.Clock();	// T0
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.NOPn));

			scheduler.Clock();	// T0+T2
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 as the I flag is still set.
		/// It is recognized during the following opcode's T0
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=30&a=0010&d=78085828eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=14&irq1=100&logmore=rdy,irq
		/********************************************************************/
		[Test]
		public void TestPlp2()
		{
			cpu.SetMem(0, Opcodes.SEIn);
			cpu.SetMem(1, Opcodes.PHPn);
			cpu.SetMem(2, Opcodes.CLIn);
			cpu.SetMem(3, Opcodes.PLPn);
			cpu.SetMem(4, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T0

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T3
			scheduler.Clock();	// T0
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}



		/********************************************************************/
		/// <summary>
		/// Interrupt is not recognized at T0 as the I flag is still set.
		/// It is recognized during the following opcode's T0
		/// </summary>
		// http://visual6502.org/JSSim/expert.html?graphics=f&loglevel=2&steps=30&a=0010&d=78085828eaeaeaea&a=fffe&d=2000&a=0020&d=e840&r=0010&irq0=14&irq1=100&logmore=rdy,irq&rdy0=20&rdy1=22
		/********************************************************************/
		[Test]
		public void TestPlp2Rdy()
		{
			cpu.SetMem(0, Opcodes.SEIn);
			cpu.SetMem(1, Opcodes.PHPn);
			cpu.SetMem(2, Opcodes.CLIn);
			cpu.SetMem(3, Opcodes.PLPn);
			cpu.SetMem(4, Opcodes.NOPn);

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T0

			scheduler.Clock();	// T1
			scheduler.Clock();	// T0+T2

			cpu.TriggerIrq();
			scheduler.Clock();	// T1
			scheduler.Clock();	// T2
			scheduler.Clock();	// T3
			cpu.SetRdy(false);
			scheduler.Clock();	// CPU stalled
			cpu.SetRdy(true);
			scheduler.Clock();	// T0
			scheduler.Clock();	// T1
			Assert.IsTrue(cpu.Check(Opcodes.BRKn));
		}
	}
}

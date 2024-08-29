/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu
{
	/// <summary>
	/// Cycle-exact 6502/6510 emulation core.
	///
	/// Code is based on work by Simon A. White [sidplay2@yahoo.com].
	/// Original Java port by Ken Händel. Later on, it has been hacked to
	/// improve compatibility with Lorenz suite on VICE's test suite
	/// </summary>
	internal class Mos6510
	{
		private enum AccessMode
		{
			WRITE,
			READ
		}

		/// <summary>
		/// IRQ/NMI magic limit values.
		/// Need to be larger than about 0x103 * 8,
		/// but can't be min/max for interger type
		/// </summary>
		private const int MAX = 65536;

		/// <summary>
		/// Stack page location
		/// </summary>
		private const uint8_t SP_PAGE = 0x01;

		/// <summary>
		/// Status register interrupt bit
		/// </summary>
		public const int SR_INTERRUPT = 2;

		// Magic values for LXA and ANE undocumented instructions.
		// Magic may be EE, EF, FE or FF, but most emulators seem to use EE.
		// The constants here defined are based on VICE testsuite which
		// refers to some real case usage of the opcodes
		private const uint8_t LXA_Magic = 0xee;
		private const uint8_t ANE_Magic = 0xef;

		private const int InterruptDelay = 2;

		#region ProcessorCycle class
		/// <summary></summary>
		private class ProcessorCycle
		{
			/// <summary></summary>
			public Action Func = null;
			/// <summary></summary>
			public bool NoSteal = false;
		}
		#endregion

		/// <summary>
		/// Event scheduler
		/// </summary>
		private readonly EventScheduler eventScheduler;

		/// <summary>
		/// Data bus
		/// </summary>
		private ICpuDataBus dataBus;

		/// <summary>
		/// Current instruction and sub-cycle within instruction
		/// </summary>
		internal int cycleCount;

		/// <summary>
		/// When IRQ was triggered. -MAX means "during some previous instruction", MAX means "no IRQ"
		/// </summary>
		private int interruptCycle;

		/// <summary>
		/// IRQ asserted on CPU
		/// </summary>
		private bool irqAssertedOnPin;

		/// <summary>
		/// NMI requested?
		/// </summary>
		private bool nmiFlag;

		/// <summary>
		/// RST requested?
		/// </summary>
		private bool rstFlag;

		/// <summary>
		/// RDY pin state (stop CPU on read)
		/// </summary>
		private bool rdy;

		/// <summary>
		/// Address low summer carry
		/// </summary>
		private bool adl_carry;

		private bool d1x1;

		/// <summary>
		/// The RDY pin state during last throw away read
		/// </summary>
		private bool rdyOnThrowAwayRead;

		/// <summary>
		/// Status register
		/// </summary>
		private Flags flags = new Flags();

		/// <summary>
		/// Data regarding current instruction
		/// </summary>
		private uint_least16_t register_ProgramCounter;
		private uint_least16_t cycle_EffectiveAddress;
		private uint_least16_t cycle_Pointer;

		private uint8_t cycle_Data;
		private uint8_t register_StackPointer;
		private uint8_t register_Accumulator;
		private uint8_t register_X;
		private uint8_t register_Y;

		/// <summary>
		/// Table of CPU opcode implementations
		/// </summary>
		private readonly ProcessorCycle[] instrTable = ArrayHelper.InitializeArray<ProcessorCycle>(0x101 << 3);

		/// <summary>
		/// Represents an instruction sub-cycle that writes
		/// </summary>
		private readonly EventCallback noSteal;

		/// <summary>
		/// Represents an instruction sub-cycle that reads
		/// </summary>
		private readonly EventCallback steal;

		private readonly EventCallback clearInt;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mos6510(EventScheduler scheduler, ICpuDataBus bus)
		{
			eventScheduler = scheduler;
			dataBus = bus;
			noSteal = new EventCallback("CPU-nosteal", EventWithoutSteals);
			steal = new EventCallback("CPU-steal", EventWithSteals);
			clearInt = new EventCallback("Remove IRQ", RemoveIrq);

			BuildInstructionTable();

			// Initialize processor registers
			register_Accumulator = 0;
			register_X = 0;
			register_Y = 0;

			cycle_EffectiveAddress = 0;
			cycle_Data = 0;

			Initialize();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Mos6510(EventScheduler scheduler) : this(scheduler, null)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void SetDataBus(ICpuDataBus bus)
		{
			dataBus = bus;
		}



		/********************************************************************/
		/// <summary>
		/// Handle bus access signals. When RDY line is asserted, the CPU
		/// will pause when executing the next read operation
		/// </summary>
		/********************************************************************/
		public void SetRdy(bool newRdy)
		{
			rdy = newRdy;

			if (rdy)
			{
				eventScheduler.Cancel(steal);
				eventScheduler.Schedule(noSteal, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
			}
			else
			{
				eventScheduler.Cancel(noSteal);
				eventScheduler.Schedule(steal, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reset CPU emulation
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Internal stuff
			Initialize();

			// Set processor port to the default values
			CpuWrite(0, 0x2f);
			CpuWrite(1, 0x37);

			// Requires external bits
			// Read from reset vector for program entry point
			SidEndian.Endian_16Lo8(ref cycle_EffectiveAddress, CpuRead(0xfffc));
			SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, CpuRead(0xfffd));
			register_ProgramCounter = cycle_EffectiveAddress;
		}



		/********************************************************************/
		/// <summary>
		/// Get data from system environment
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8_t CpuRead(uint_least16_t addr)
		{
			return dataBus.CpuRead(addr);
		}



		/********************************************************************/
		/// <summary>
		/// Write data to system environment
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CpuWrite(uint_least16_t addr, uint8_t data)
		{
			dataBus.CpuWrite(addr, data);
		}

		#region Interrupt routines
		/********************************************************************/
		/// <summary>
		/// Trigger NMI interrupt on the CPU. Calling this method flags that
		/// CPU must enter the NMI routine at earliest opportunity. There is
		/// no way to cancel NMI request once given
		/// </summary>
		/********************************************************************/
		public void TriggerNmi()
		{
			nmiFlag = true;
			CalculateInterruptTriggerCycle();

			// Maybe process 1 clock of interrupt delay
			if (!rdy)
			{
				eventScheduler.Cancel(steal);
				eventScheduler.Schedule(steal, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Pull IRQ line low on CPU
		/// </summary>
		/********************************************************************/
		public void TriggerIrq()
		{
			irqAssertedOnPin = true;
			CalculateInterruptTriggerCycle();

			// Maybe process 1 clock of interrupt delay
			if (!rdy && (interruptCycle == cycleCount))
			{
				eventScheduler.Cancel(steal);
				eventScheduler.Schedule(steal, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Inform CPU that IRQ is no longer pulled low
		/// </summary>
		/********************************************************************/
		public void ClearIrq()
		{
			irqAssertedOnPin = false;
			eventScheduler.Schedule(clearInt, InterruptDelay, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InterruptsAndNextOpcode()
		{
			if (cycleCount > interruptCycle + InterruptDelay)
			{
				CpuRead(register_ProgramCounter);
				cycleCount = Opcodes.BRKn << 3;
				d1x1 = true;
				interruptCycle = MAX;
			}
			else
				FetchNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Evaluate when to execute an interrupt. Calling this method can
		/// also result in the decision that no interrupt at all needs to be
		/// scheduled
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CalculateInterruptTriggerCycle()
		{
			// Interrupt cycle not going to trigger?
			if (interruptCycle == MAX)
			{
				if (CheckInterrupts())
					interruptCycle = cycleCount;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IrqLoRequest()
		{
			SidEndian.Endian_16Lo8(ref register_ProgramCounter, CpuRead(cycle_EffectiveAddress));
			d1x1 = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IrqHiRequest()
		{
			SidEndian.Endian_16Hi8(ref register_ProgramCounter, CpuRead((uint_least16_t)(cycle_EffectiveAddress + 1)));
			flags.SetI(true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CheckInterrupts()
		{
			return rstFlag || nmiFlag || (irqAssertedOnPin && !flags.GetI());
		}
		#endregion

		#region Fetch methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchNextOpcode()
		{
			rdyOnThrowAwayRead = false;

			cycleCount = CpuRead(register_ProgramCounter) << 3;
			register_ProgramCounter++;

			if (!CheckInterrupts())
				interruptCycle = MAX;
			else if (interruptCycle != MAX)
				interruptCycle = -MAX;
		}



		/********************************************************************/
		/// <summary>
		/// Read the next opcode byte from memory (and throw it away)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowAwayFetch()
		{
			CpuRead(register_ProgramCounter);
		}



		/********************************************************************/
		/// <summary>
		/// Issue throw-away read and fix address. Some people use these to
		/// ACK CIA IRQs
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowAwayRead()
		{
			CpuRead(cycle_EffectiveAddress);
			if (adl_carry)
				cycle_EffectiveAddress += 0x100;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch value, increment PC
		/// Addressing modes: Immediate
		///	                  Relative
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchDataByte()
		{
			cycle_Data = CpuRead(register_ProgramCounter);
			if (!d1x1)
				register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch low address byte, increment PC
		/// Addressing modes: Stack manipulation
		///	                  Absolute
		///                   Zero page
		///                   Zero page indexed
		///                   Absolute indexed
		///                   Absolute indirect
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowAddr()
		{
			cycle_EffectiveAddress = CpuRead(register_ProgramCounter);
			register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Read from address, add index register X to it
		/// Addressing modes: Zero page indexed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowAddrX()
		{
			FetchLowAddr();
			cycle_EffectiveAddress = (uint_least16_t)((cycle_EffectiveAddress + register_X) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Read from address, add index register Y to it
		/// Addressing modes: Zero page indexed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowAddrY()
		{
			FetchLowAddr();
			cycle_EffectiveAddress = (uint_least16_t)((cycle_EffectiveAddress + register_Y) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high address byte, increment PC (absolute addressing)
		/// Low byte must have been obtained first!
		/// Addressing modes: Absolute
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighAddr()
		{
			// Get the high byte of an address from memory
			SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, CpuRead(register_ProgramCounter));
			register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high byte of address, add index register X to low address
		/// byte, increment PC
		/// Addressing modes: Absolute indexed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighAddrX()
		{
			cycle_EffectiveAddress += register_X;
			adl_carry = cycle_EffectiveAddress > 0xff;
			FetchHighAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Same as FetchHighAddrX except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighAddrX2()
		{
			FetchHighAddrX();
			if (!adl_carry)
				cycleCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high byte of address, add index register Y to low address
		/// byte, increment PC
		/// Addressing modes: Absolute indexed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighAddrY()
		{
			cycle_EffectiveAddress += register_Y;
			adl_carry = cycle_EffectiveAddress > 0xff;
			FetchHighAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Same as FetchHighAddrY except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighAddrY2()
		{
			FetchHighAddrY();
			if (!adl_carry)
				cycleCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch pointer address low, increment PC
		/// Addressing modes: Absolute indirect
		///                   Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowPointer()
		{
			cycle_Pointer = CpuRead(register_ProgramCounter);
			register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Add X to it
		/// Addressing modes: Indexed indirect (pre X)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowPointerX()
		{
			SidEndian.Endian_16Lo8(ref cycle_Pointer, (uint8_t)((cycle_Pointer + register_X) & 0xff));
		}



		/********************************************************************/
		/// <summary>
		/// Fetch pointer address high, increment PC
		/// Addressing modes: Absolute indirect
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighPointer()
		{
			SidEndian.Endian_16Hi8(ref cycle_Pointer, CpuRead(register_ProgramCounter));
			register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address low
		/// Addressing modes: Indirect
		///                   Indexed indirect (pre X)
		///                   Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchLowEffAddr()
		{
			cycle_EffectiveAddress = CpuRead(cycle_Pointer);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address high
		/// Addressing modes: Indirect
		///                   Indexed indirect (pre X)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighEffAddr()
		{
			SidEndian.Endian_16Lo8(ref cycle_Pointer, (uint8_t)((cycle_Pointer + 1) & 0xff));
			SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, CpuRead(cycle_Pointer));
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address high, add Y to low byte of effective
		/// address
		/// Addressing modes: Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighEffAddrY()
		{
			cycle_EffectiveAddress += register_Y;
			adl_carry = cycle_EffectiveAddress > 0xff;
			FetchHighEffAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Same as FetchHighEffAddrY except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchHighEffAddrY2()
		{
			FetchHighEffAddrY();
			if (!adl_carry)
				cycleCount++;
		}
		#endregion

		#region Common data accessing methods
		//------------------------------------------------------------------------------
		// Data accessing operations as described in 64doc by John West and Marko Makela
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FetchEffAddrDataByte()
		{
			cycle_Data = CpuRead(cycle_EffectiveAddress);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PutEffAddrDataByte()
		{
			CpuWrite(cycle_EffectiveAddress, cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Push data on the stack
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Push(uint8_t data)
		{
			uint_least16_t addr = SidEndian.Endian_16(SP_PAGE, register_StackPointer);
			CpuWrite(addr, data);
			register_StackPointer--;
		}



		/********************************************************************/
		/// <summary>
		/// Pop data from the stack
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint8_t Pop()
		{
			register_StackPointer++;
			uint_least16_t addr = SidEndian.Endian_16(SP_PAGE, register_StackPointer);

			return CpuRead(addr);
		}



		/********************************************************************/
		/// <summary>
		/// Push program counter low byte on stack, decrement S
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PushLowPc()
		{
			Push(SidEndian.Endian_16Lo8(register_ProgramCounter));
		}



		/********************************************************************/
		/// <summary>
		/// Push program counter high byte on stack, decrement S
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PushHighPc()
		{
			Push(SidEndian.Endian_16Hi8(register_ProgramCounter));
		}



		/********************************************************************/
		/// <summary>
		/// Increment stack and pull program counter low byte from stack
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PopLowPc()
		{
			SidEndian.Endian_16Lo8(ref cycle_EffectiveAddress, Pop());
		}



		/********************************************************************/
		/// <summary>
		/// Increment stack and pull program counter high byte from stack
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PopHighPc()
		{
			SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, Pop());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WasteCycle()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BRKPushLowPc()
		{
			PushLowPc();

			if (rstFlag)
			{
				// rst = %10x
				cycle_EffectiveAddress = 0xfffc;
			}
			else if (nmiFlag)
			{
				// nmi = %01x
				cycle_EffectiveAddress = 0xfffa;
			}
			else
			{
				// irq = %11x
				cycle_EffectiveAddress = 0xfffe;
			}

			rstFlag = false;
			nmiFlag = false;
			CalculateInterruptTriggerCycle();
		}
		#endregion

		#region Common instruction opcodes
		//------------------------------------------------------------------------------
		// See the 6510 assembly book for more information on these instructions
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLD_Instr()
		{
			flags.SetD(false);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLI_Instr()
		{
			flags.SetI(false);
			CalculateInterruptTriggerCycle();
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void JMP_Instr()
		{
			register_ProgramCounter = cycle_EffectiveAddress;

			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PHA_Instr()
		{
			Push(register_Accumulator);
		}



		/********************************************************************/
		/// <summary>
		/// RTI does not delay the IRQ I flag change as it is set 3 cycles
		/// before the end of the opcode, and thus the 6510 has enough time
		/// to call the interrupt routine as soon as the opcode ends, if
		/// necessary
		/// </summary>
		/********************************************************************/
		private void RTI_Instr()
		{
			register_ProgramCounter = cycle_EffectiveAddress;
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RTS_Instr()
		{
			CpuRead(cycle_EffectiveAddress);
			register_ProgramCounter = cycle_EffectiveAddress;
			register_ProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SED_Instr()
		{
			flags.SetD(true);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SEI_Instr()
		{
			flags.SetI(true);
			InterruptsAndNextOpcode();

			if (!rstFlag && !nmiFlag && (interruptCycle != MAX))
				interruptCycle = MAX;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STA_Instr()
		{
			cycle_Data = register_Accumulator;
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STX_Instr()
		{
			cycle_Data = register_X;
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STY_Instr()
		{
			cycle_Data = register_Y;
			PutEffAddrDataByte();
		}
		#endregion

		#region Common instruction undocumented opcodes
		//------------------------------------------------------------------------------
		// See documented 6502-nmo.opc by Adam Vardy for more details
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// Perform SH* instructions
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SH_Instr()
		{
			uint8_t tmp = SidEndian.Endian_16Hi8(cycle_EffectiveAddress);

			// When the addressing/indexing causes a page boundary crossing
			// the high byte of the target address is ANDed with the value stored
			if (adl_carry)
				SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, (uint8_t)(tmp & cycle_Data));
			else
				tmp++;

			// When a DMA is going on (the CPU is halted by the VIC-II)
			// while the instruction sha/shx/shy executes then the last
			// term of the ANDing (ADH+1) drops off
			//
			// http://sourceforge.net/p/vice-emu/bugs/578/
			if (!rdyOnThrowAwayRead)
				cycle_Data &= tmp;

			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode stores the result of A AND X AND ADH+1
		/// in memory
		/// </summary>
		/********************************************************************/
		private void AXA_Instr()
		{
			cycle_Data = (uint8_t)(register_X & register_Accumulator);
			SH_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ANDs the contents of the Y register
		/// with ADH+1 and stores the result in memory
		/// </summary>
		/********************************************************************/
		private void SAY_Instr()
		{
			cycle_Data = register_Y;
			SH_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ANDs the contents of the X register
		/// with ADH+1 and stores the result in memory
		/// </summary>
		/********************************************************************/
		private void XAS_Instr()
		{
			cycle_Data = register_X;
			SH_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - AXS ANDs the contents of the A and X registers
		/// (without changing the contents of either register) and stores the
		/// result in memory. AXS does not affect any flags in the processor
		/// status register
		/// </summary>
		/********************************************************************/
		private void AXS_Instr()
		{
			cycle_Data = (uint8_t)(register_Accumulator & register_X);
			PutEffAddrDataByte();
		}
		#endregion

		#region Arithmetic operations
		/********************************************************************/
		/// <summary>
		/// BCD adding
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DoADC()
		{
			uint c = flags.GetC() ? (uint)1 : 0;
			uint a = register_Accumulator;
			uint s = cycle_Data;
			uint regAC2 = a + s + c;

			if (flags.GetD())
			{
				// BCD mode
				uint lo = (a & 0x0f) + (s & 0x0f) + c;
				uint hi = (a & 0xf0) + (s & 0xf0);

				if (lo > 0x09)
					lo += 0x06;

				if (lo > 0x0f)
					hi += 0x10;

				flags.SetZ((regAC2 & 0xff) == 0);
				flags.SetN((hi & 0x80) != 0);
				flags.SetV((((hi ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) == 0));

				if (hi > 0x90)
					hi += 0x60;

				flags.SetC(hi > 0xff);
				register_Accumulator = (uint8_t)(hi | (lo & 0x0f));
			}
			else
			{
				// Binary mode
				flags.SetC(regAC2 > 0xff);
				flags.SetV((((regAC2 ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) == 0));
				flags.SetNZ(register_Accumulator = (uint8_t)(regAC2 & 0xff));
			}
		}



		/********************************************************************/
		/// <summary>
		/// BCD subtracting
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DoSBC()
		{
			uint c = flags.GetC() ? (uint)0 : 1;
			uint a = register_Accumulator;
			uint s = cycle_Data;
			uint regAC2 = a - s - c;

			flags.SetC(regAC2 < 0x100);
			flags.SetV((((regAC2 ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) != 0));
			flags.SetNZ((uint8_t)regAC2);

			if (flags.GetD())
			{
				// BCD mode
				uint lo = (a & 0x0f) - (s & 0x0f) - c;
				uint hi = (a & 0xf0) - (s & 0xf0);

				if ((lo & 0x10) != 0)
				{
					lo -= 0x06;
					hi -= 0x10;
				}

				if ((hi & 0x100) != 0)
					hi -= 0x60;

				register_Accumulator = (uint8_t)(hi | (lo & 0x0f));
			}
			else
			{
				// Binary mode
				register_Accumulator = (uint8_t)(regAC2 & 0xff);
			}
		}
		#endregion

		#region Generic instruction addressing routines

		#region Generic instruction opcodes
		//------------------------------------------------------------------------------
		// See the 6510 assembly book for more information on these instructions
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ADC_Instr()
		{
			DoADC();
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AND_Instr()
		{
			flags.SetNZ(register_Accumulator &= cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - For a detailed explanation of this opcode look at:
		/// http://visual6502.org/wiki/index.php?title=6502_Opcode_8B_(XAA,_ANE)
		/// </summary>
		/********************************************************************/
		private void ANE_Instr()
		{
			flags.SetNZ(register_Accumulator = (uint8_t)((register_Accumulator | ANE_Magic) & register_X & cycle_Data));
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ASL_Instr()
		{
			PutEffAddrDataByte();
			flags.SetC((cycle_Data & 0x80) != 0);
			flags.SetNZ(cycle_Data <<= 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ASLa_Instr()
		{
			flags.SetC((register_Accumulator & 0x80) != 0);
			flags.SetNZ(register_Accumulator <<= 1);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fix_Branch()
		{
			// Throw away read
			CpuRead(cycle_EffectiveAddress);

			// Fix address
			register_ProgramCounter += (uint_least16_t)(cycle_Data < 0x80 ? 0x0100 : 0xff00);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Branch_Instr(bool condition)
		{
			// 2 cycles spent before arriving here. spend 0 - 2 cycles here;
			// - condition false: Continue immediately to FetchNextInstr.
			//
			// Otherwise read the byte following the opcode (which is already scheduled to occur on this cycle).
			// This effort is wasted. Then calculate address of the branch target. If branch is on same page,
			// then continue at that insn on next cycle (this delays IRQs by 1 clock for some reason, allegedly).
			//
			// If the branch is in different memory page, issue a spurious read with wrong high byte before
			// continuing at the correct address
			if (condition)
			{
				// Issue the spurious read for next insn here
				CpuRead(register_ProgramCounter);

				cycle_EffectiveAddress = SidEndian.Endian_16Lo8(register_ProgramCounter);
				cycle_EffectiveAddress += cycle_Data;
				adl_carry = (cycle_EffectiveAddress > 0xff) != (cycle_Data > 0x7f);
				SidEndian.Endian_16Hi8(ref cycle_EffectiveAddress, SidEndian.Endian_16Hi8(register_ProgramCounter));

				register_ProgramCounter = cycle_EffectiveAddress;

				// Check for page boundary crossing
				if (!adl_carry)
				{
					// Skip next throw away read
					cycleCount++;

					// Hack: delay the interrupt past this instruction
					if (interruptCycle >> 3 == cycleCount >> 3)
						interruptCycle += 2;
				}
			}
			else
			{
				// Branch not taken: skip the following spurious read insn and go to FetchNextInstr immediately
				InterruptsAndNextOpcode();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BCC_Instr()
		{
			Branch_Instr(!flags.GetC());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BCS_Instr()
		{
			Branch_Instr(flags.GetC());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BEQ_Instr()
		{
			Branch_Instr(flags.GetZ());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BIT_Instr()
		{
			flags.SetZ((register_Accumulator & cycle_Data) == 0);
			flags.SetN((cycle_Data & 0x80) != 0);
			flags.SetV((cycle_Data & 0x40) != 0);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BMI_Instr()
		{
			Branch_Instr(flags.GetN());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BNE_Instr()
		{
			Branch_Instr(!flags.GetZ());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BPL_Instr()
		{
			Branch_Instr(!flags.GetN());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BVC_Instr()
		{
			Branch_Instr(!flags.GetV());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BVS_Instr()
		{
			Branch_Instr(flags.GetV());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLC_Instr()
		{
			flags.SetC(false);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLV_Instr()
		{
			flags.SetV(false);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Compare(uint8_t data)
		{
			uint_least16_t tmp = (uint_least16_t)(data - cycle_Data);
			flags.SetNZ((uint8_t)tmp);
			flags.SetC(tmp < 0x100);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CMP_Instr()
		{
			Compare(register_Accumulator);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CPX_Instr()
		{
			Compare(register_X);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CPY_Instr()
		{
			Compare(register_Y);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEC_Instr()
		{
			PutEffAddrDataByte();
			flags.SetNZ(--cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEX_Instr()
		{
			flags.SetNZ(--register_X);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEY_Instr()
		{
			flags.SetNZ(--register_Y);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EOR_Instr()
		{
			flags.SetNZ(register_Accumulator ^= cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INC_Instr()
		{
			PutEffAddrDataByte();
			flags.SetNZ(++cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INX_Instr()
		{
			flags.SetNZ(++register_X);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INY_Instr()
		{
			flags.SetNZ(++register_Y);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDA_Instr()
		{
			flags.SetNZ(register_Accumulator = cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDX_Instr()
		{
			flags.SetNZ(register_X = cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDY_Instr()
		{
			flags.SetNZ(register_Y = cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LSR_Instr()
		{
			PutEffAddrDataByte();
			flags.SetC((cycle_Data & 0x01) != 0);
			flags.SetNZ(cycle_Data >>= 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LSRa_Instr()
		{
			flags.SetC((register_Accumulator & 0x01) != 0);
			flags.SetNZ(register_Accumulator >>= 1);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ORA_Instr()
		{
			flags.SetNZ(register_Accumulator |= cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PLA_Instr()
		{
			flags.SetNZ(register_Accumulator = Pop());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROL_Instr()
		{
			bool newC = (cycle_Data & 0x80) != 0;
			PutEffAddrDataByte();
			cycle_Data <<= 1;

			if (flags.GetC())
				cycle_Data |= 0x01;

			flags.SetNZ(cycle_Data);
			flags.SetC(newC);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROLa_Instr()
		{
			bool newC = (register_Accumulator & 0x80) != 0;
			register_Accumulator <<= 1;

			if (flags.GetC())
				register_Accumulator |= 0x01;

			flags.SetNZ(register_Accumulator);
			flags.SetC(newC);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROR_Instr()
		{
			bool newC = (cycle_Data & 0x01) != 0;
			PutEffAddrDataByte();
			cycle_Data >>= 1;

			if (flags.GetC())
				cycle_Data |= 0x80;

			flags.SetNZ(cycle_Data);
			flags.SetC(newC);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RORa_Instr()
		{
			bool newC = (register_Accumulator & 0x01) != 0;
			register_Accumulator >>= 1;

			if (flags.GetC())
				register_Accumulator |= 0x80;

			flags.SetNZ(register_Accumulator);
			flags.SetC(newC);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SBX_Instr()
		{
			uint tmp = (uint)((register_X & register_Accumulator) - cycle_Data);
			flags.SetNZ(register_X = (byte)(tmp & 0xff));
			flags.SetC(tmp < 0x100);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SBC_Instr()
		{
			DoSBC();
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SEC_Instr()
		{
			flags.SetC(true);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SHS_Instr()
		{
			register_StackPointer = (uint8_t)(register_Accumulator & register_X);
			cycle_Data = register_StackPointer;
			SH_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TAX_Instr()
		{
			flags.SetNZ(register_X = register_Accumulator);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TAY_Instr()
		{
			flags.SetNZ(register_Y = register_Accumulator);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TSX_Instr()
		{
			flags.SetNZ(register_X = register_StackPointer);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TXA_Instr()
		{
			flags.SetNZ(register_Accumulator = register_X);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TXS_Instr()
		{
			register_StackPointer = register_X;
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TYA_Instr()
		{
			flags.SetNZ(register_Accumulator = register_Y);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InvalidOpcode()
		{
			throw new HaltInstructionException();
		}
		#endregion

		#region Generic instruction undocumented opcodes
		//------------------------------------------------------------------------------
		// See documented 6502-nmo.opc by Adam Vardy for more details
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ANDs the contents of the A register
		/// with an immediate value and then LSRs the result
		/// </summary>
		/********************************************************************/
		private void ALR_Instr()
		{
			register_Accumulator &= cycle_Data;
			flags.SetC((register_Accumulator & 0x01) != 0);
			flags.SetNZ(register_Accumulator >>= 1);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - ANC ANDs the contents of the A register with an
		/// immediate value and then moves bit 7 of A into the Carry flag.
		/// This opcode works basically identically to AND #immed. except
		/// that the Carry flag is set to the same state that the Negative
		/// flag is set to
		/// </summary>
		/********************************************************************/
		private void ANC_Instr()
		{
			flags.SetNZ(register_Accumulator &= cycle_Data);
			flags.SetC(flags.GetN());
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ANDs the contents of the A register
		/// with an immediate value and then RORs the result. (Implementation
		/// based on that of Frodo C64 Emulator)
		/// </summary>
		/********************************************************************/
		private void ARR_Instr()
		{
			uint8_t data = (uint8_t)(cycle_Data & register_Accumulator);
			register_Accumulator = (uint8_t)(data >> 1);

			if (flags.GetC())
				register_Accumulator |= 0x80;

			if (flags.GetD())
			{
				flags.SetN(flags.GetC());
				flags.SetZ(register_Accumulator == 0);
				flags.SetV(((data ^ register_Accumulator) & 0x40) != 0);

				if ((data & 0x0f) + (data & 0x01) > 5)
					register_Accumulator = (uint8_t)((register_Accumulator & 0xf0) | ((register_Accumulator + 6) & 0x0f));

				flags.SetC(((data + (data & 0x10)) & 0x1f0) > 0x50);
				if (flags.GetC())
					register_Accumulator += 0x60;
			}
			else
			{
				flags.SetNZ(register_Accumulator);
				flags.SetC((register_Accumulator & 0x40) != 0);
				flags.SetV(((register_Accumulator & 0x40) ^ ((register_Accumulator & 0x20) << 1)) != 0);
			}

			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ASLs the contents of a memory
		/// location and then ORs the result with the accumulator
		/// </summary>
		/********************************************************************/
		private void ASO_Instr()
		{
			PutEffAddrDataByte();
			flags.SetC((cycle_Data & 0x80) != 0);
			cycle_Data <<= 1;
			flags.SetNZ(register_Accumulator |= cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode DECs the contents of a memory location
		/// and then CMPs the result with the A register
		/// </summary>
		/********************************************************************/
		private void DCM_Instr()
		{
			PutEffAddrDataByte();
			cycle_Data--;

			uint_least16_t tmp = (uint_least16_t)(register_Accumulator - cycle_Data);
			flags.SetNZ((uint8_t)tmp);
			flags.SetC(tmp < 0x100);
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode INCs the contents of a memory location
		/// and then SBCs the result from the A register
		/// </summary>
		/********************************************************************/
		private void INS_Instr()
		{
			PutEffAddrDataByte();
			cycle_Data++;
			DoSBC();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ANDs the contents of a memory location
		/// with the contents of the stack pointer register and stores the
		/// result in the accumulator, the X register, and the stack pointer.
		/// Affected flags: N Z
		/// </summary>
		/********************************************************************/
		private void LAS_Instr()
		{
			flags.SetNZ(cycle_Data &= register_StackPointer);
			register_Accumulator = cycle_Data;
			register_X = cycle_Data;
			register_StackPointer = cycle_Data;
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode loads both the accumulator and the X
		/// register with the contents of a memory location
		/// </summary>
		/********************************************************************/
		private void LAX_Instr()
		{
			flags.SetNZ(register_Accumulator = register_X = cycle_Data);
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - LSE LSRs the contents of a memory location and
		/// then EORs the result with the accumulator
		/// </summary>
		/********************************************************************/
		private void LSE_Instr()
		{
			PutEffAddrDataByte();
			flags.SetC((cycle_Data & 0x01) != 0);
			cycle_Data >>= 1;
			flags.SetNZ(register_Accumulator ^= cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - This opcode ORs the A register with #xx (the
		/// "magic" value), ANDs the result with an immediate value, and then
		/// stores the result in both A and X
		/// </summary>
		/********************************************************************/
		private void OAL_Instr()
		{
			flags.SetNZ(register_X = register_Accumulator = (uint8_t)(cycle_Data & (register_Accumulator | LXA_Magic)));
			InterruptsAndNextOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - RLA ROLs the contents of a memory location and
		/// then ANDs the result with the accumulator
		/// </summary>
		/********************************************************************/
		private void RLA_Instr()
		{
			bool newC = (cycle_Data & 0x80) != 0;
			PutEffAddrDataByte();
			cycle_Data <<= 1;

			if (flags.GetC())
				cycle_Data |= 0x01;

			flags.SetC(newC);
			flags.SetNZ(register_Accumulator &= cycle_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Undocumented - RRA RORs the contents of a memory location and
		/// then ADCs the result with the accumulator
		/// </summary>
		/********************************************************************/
		private void RRA_Instr()
		{
			bool newC = (cycle_Data & 0x01) != 0;
			PutEffAddrDataByte();
			cycle_Data >>= 1;

			if (flags.GetC())
				cycle_Data |= 0x80;

			flags.SetC(newC);
			DoADC();
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// When AEC signal is high, no stealing is possible
		/// </summary>
		/********************************************************************/
		private void EventWithoutSteals()
		{
			ProcessorCycle instr = instrTable[cycleCount++];
			instr.Func();

			eventScheduler.Schedule(noSteal, 1);
		}



		/********************************************************************/
		/// <summary>
		/// When AEC signal is low, steals permitted
		/// </summary>
		/********************************************************************/
		private void EventWithSteals()
		{
			if (instrTable[cycleCount].NoSteal)
			{
				ProcessorCycle instr = instrTable[cycleCount++];
				instr.Func();

				eventScheduler.Schedule(steal, 1);
			}
			else
			{
				switch (cycleCount)
				{
					case Opcodes.CLIn << 3:
					{
						flags.SetI(false);
						if (irqAssertedOnPin && (interruptCycle == MAX))
							interruptCycle = -MAX;

						break;
					}

					case Opcodes.SEIn << 3:
					{
						flags.SetI(true);
						if (!rstFlag && !nmiFlag && (cycleCount <= interruptCycle + InterruptDelay))
							interruptCycle = MAX;

						break;
					}

					case (Opcodes.SHAiy << 3) + 3:
					case (Opcodes.SHSay << 3) + 2:
					case (Opcodes.SHYax << 3) + 2:
					case (Opcodes.SHXay << 3) + 2:
					case (Opcodes.SHAay << 3) + 2:
					{
						// Save rdy state for SH* instructions
						rdyOnThrowAwayRead = true;
						break;
					}
				}

				// Even while stalled, the CPU can still process first clock of
				// interrupt delay, but only the first one
				if (interruptCycle == cycleCount)
					interruptCycle--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RemoveIrq()
		{
			if (!rstFlag && !nmiFlag && (interruptCycle != MAX))
				interruptCycle = MAX;
		}



		/********************************************************************/
		/// <summary>
		/// Push P on stack, decrement S
		/// </summary>
		/********************************************************************/
		private void PushSr()
		{
			// Set the B flag, 0 for hardware interrupts
			// and 1 for BRK and PHP. Bit 5 is always 1
			// https://wiki.nesdev.org/w/index.php?title=Status_flags#The_B_flag
			Push((uint8_t)(flags.Get() | (d1x1 ? 0x20 : 0x30)));
		}



		/********************************************************************/
		/// <summary>
		/// Increment S, pop P off stack
		/// </summary>
		/********************************************************************/
		private void PopSr()
		{
			flags.Set(Pop());
			CalculateInterruptTriggerCycle();
		}



		/********************************************************************/
		/// <summary>
		/// Build up the processor instruction table
		/// </summary>
		/********************************************************************/
		private void BuildInstructionTable()
		{
			for (uint i = 0; i < 0x100; i++)
			{
				// So: what cycles are marked as stealable? Rules are:
				//
				// - CPU performs either read or write at every cycle. Reads are
				//   always stealable. Writes are rare.
				//
				// - Every instruction begins with a sequence of reads. Writes,
				//   if any, are at the end for most instructions

				uint buildCycle = i << 3;

				AccessMode access = AccessMode.WRITE;
				bool legalMode = true;
				bool legalInstr = true;

				switch (i)
				{
					// Accumulator or Implied addressing
					case Opcodes.ASLn or Opcodes.CLCn or Opcodes.CLDn or Opcodes.CLIn or Opcodes.CLVn or Opcodes.DEXn or
						 Opcodes.DEYn or Opcodes.INXn or Opcodes.INYn or Opcodes.LSRn or Opcodes.NOPn or Opcodes.NOPn1 or Opcodes.NOPn2 or Opcodes.NOPn3 or Opcodes.NOPn4 or Opcodes.NOPn5 or Opcodes.NOPn6 or Opcodes.PHAn or
						 Opcodes.PHPn or Opcodes.PLAn or Opcodes.PLPn or Opcodes.ROLn or Opcodes.RORn or
						 Opcodes.SECn or Opcodes.SEDn or Opcodes.SEIn or Opcodes.TAXn or Opcodes.TAYn or
						 Opcodes.TSXn or Opcodes.TXAn or Opcodes.TXSn or Opcodes.TYAn:
					{
						instrTable[buildCycle++].Func = ThrowAwayFetch;
						break;
					}

					// Immediate and Relative addressing mode handler
					case Opcodes.ADCb or Opcodes.ANDb or Opcodes.ANCb or Opcodes.ANCb1 or Opcodes.ANEb or Opcodes.ASRb or Opcodes.ARRb or
						 Opcodes.BCCr or Opcodes.BCSr or Opcodes.BEQr or Opcodes.BMIr or Opcodes.BNEr or Opcodes.BPLr or
						 Opcodes.BRKn or Opcodes.BVCr or Opcodes.BVSr or Opcodes.CMPb or Opcodes.CPXb or Opcodes.CPYb or
						 Opcodes.EORb or Opcodes.LDAb or Opcodes.LDXb or Opcodes.LDYb or Opcodes.LXAb or Opcodes.NOPb or Opcodes.NOPb1 or Opcodes.NOPb2 or Opcodes.NOPb3 or Opcodes.NOPb4 or
						 Opcodes.ORAb or Opcodes.SBCb or Opcodes.SBCb1 or Opcodes.SBXb or Opcodes.RTIn or Opcodes.RTSn:
					{
						instrTable[buildCycle++].Func = FetchDataByte;
						break;
					}

					// Zero page addressing mode handler - read & RMW
					case Opcodes.ADCz or Opcodes.ANDz or Opcodes.BITz or Opcodes.CMPz or Opcodes.CPXz or Opcodes.CPYz or
						 Opcodes.EORz or Opcodes.LAXz or Opcodes.LDAz or Opcodes.LDXz or Opcodes.LDYz or Opcodes.ORAz or
						 Opcodes.NOPz or Opcodes.NOPz1 or Opcodes.NOPz2 or Opcodes.SBCz or
						 Opcodes.ASLz or Opcodes.DCPz or Opcodes.DECz or Opcodes.INCz or Opcodes.ISBz or Opcodes.LSRz or
						 Opcodes.ROLz or Opcodes.RORz or Opcodes.SREz or Opcodes.SLOz or Opcodes.RLAz or Opcodes.RRAz:
					{
						access = AccessMode.READ;
						goto case Opcodes.SAXz;
					}

					case Opcodes.SAXz:
					case Opcodes.STAz or Opcodes.STXz or Opcodes.STYz:
					{
						instrTable[buildCycle++].Func = FetchLowAddr;
						break;
					}

					// Zero page with X offset addressing mode handler
					// These issue extra reads on the 0 page, but we don't care about it
					// because there are no detectable effects from them. These reads
					// occur during the "wasted" cycle
					case Opcodes.ADCzx or Opcodes.ANDzx or Opcodes.CMPzx or Opcodes.EORzx or Opcodes.LDAzx or Opcodes.LDYzx or
						 Opcodes.NOPzx or Opcodes.NOPzx1 or Opcodes.NOPzx2 or Opcodes.NOPzx3 or Opcodes.NOPzx4 or Opcodes.NOPzx5 or Opcodes.ORAzx or Opcodes.SBCzx or
						 Opcodes.ASLzx or Opcodes.DCPzx or Opcodes.DECzx or Opcodes.INCzx or Opcodes.ISBzx or Opcodes.LSRzx or
						 Opcodes.RLAzx or Opcodes.ROLzx or Opcodes.RORzx or Opcodes.RRAzx or Opcodes.SLOzx or Opcodes.SREzx:
					{
						access = AccessMode.READ;
						goto case Opcodes.STAzx;
					}

					case Opcodes.STAzx:
					case Opcodes.STYzx:
					{
						instrTable[buildCycle++].Func = FetchLowAddrX;

						// Operate on 0 page in read mode. Truly side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						break;
					}

					// Zero page with Y offset addressing mode handler
					case Opcodes.LDXzy or Opcodes.LAXzy:
					{
						access = AccessMode.READ;
						goto case Opcodes.STXzy;
					}

					case Opcodes.STXzy:
					case Opcodes.SAXzy:
					{
						instrTable[buildCycle++].Func = FetchLowAddrY;

						// Operate on 0 page in read mode. Truly side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						break;
					}

					// Absolute addressing mode handler
					case Opcodes.ADCa or Opcodes.ANDa or Opcodes.BITa or Opcodes.CMPa or Opcodes.CPXa or Opcodes.CPYa or
						 Opcodes.EORa or Opcodes.LAXa or Opcodes.LDAa or Opcodes.LDXa or Opcodes.LDYa or Opcodes.NOPa or
						 Opcodes.ORAa or Opcodes.SBCa or
						 Opcodes.ASLa or Opcodes.DCPa or Opcodes.DECa or Opcodes.INCa or Opcodes.ISBa or Opcodes.LSRa or
						 Opcodes.ROLa or Opcodes.RORa or Opcodes.SLOa or Opcodes.SREa or Opcodes.RLAa or Opcodes.RRAa:
					{
						access = AccessMode.READ;
						goto case Opcodes.JMPw;
					}

					case Opcodes.JMPw:
					case Opcodes.SAXa or Opcodes.STAa or Opcodes.STXa or Opcodes.STYa:
					{
						instrTable[buildCycle++].Func = FetchLowAddr;
						instrTable[buildCycle++].Func = FetchHighAddr;
						break;
					}

					case Opcodes.JSRw:
					{
						instrTable[buildCycle++].Func = FetchLowAddr;
						break;
					}

					// Absolute with X offset addressing mode handler (read)
					case Opcodes.ADCax or Opcodes.ANDax or Opcodes.CMPax or Opcodes.EORax or Opcodes.LDAax or
						 Opcodes.LDYax or Opcodes.NOPax or Opcodes.NOPax1 or Opcodes.NOPax2 or Opcodes.NOPax3 or Opcodes.NOPax4 or Opcodes.NOPax5 or Opcodes.ORAax or Opcodes.SBCax:
					{
						access = AccessMode.READ;
						instrTable[buildCycle++].Func = FetchLowAddr;
						instrTable[buildCycle++].Func = FetchHighAddrX2;

						// This cycle is skipped if the address is already correct.
						// Otherwise, it will be read and ignored
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					// Absolute X (RMW; no page crossing handled, always reads before writing)
					case Opcodes.ASLax or Opcodes.DCPax or Opcodes.DECax or Opcodes.INCax or Opcodes.ISBax or
						 Opcodes.LSRax or Opcodes.RLAax or Opcodes.ROLax or Opcodes.RORax or Opcodes.RRAax or
						 Opcodes.SLOax or Opcodes.SREax:
					{
						access = AccessMode.READ;
						goto case Opcodes.SHYax;
					}

					case Opcodes.SHYax:
					case Opcodes.STAax:
					{
						instrTable[buildCycle++].Func = FetchLowAddr;
						instrTable[buildCycle++].Func = FetchHighAddrX;
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					// Absolute with Y offset addressing mode handler (read)
					case Opcodes.ADCay or Opcodes.ANDay or Opcodes.CMPay or Opcodes.EORay or Opcodes.LASay or
						 Opcodes.LAXay or Opcodes.LDAay or Opcodes.LDXay or Opcodes.ORAay or Opcodes.SBCay:
					{
						access = AccessMode.READ;

						instrTable[buildCycle++].Func = FetchLowAddr;
						instrTable[buildCycle++].Func = FetchHighAddrY2;
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					// Absolute Y (no page crossing handled)
					case Opcodes.DCPay or Opcodes.ISBay or Opcodes.RLAay or Opcodes.RRAay or Opcodes.SLOay or
						 Opcodes.SREay:
					{
						access = AccessMode.READ;
						goto case Opcodes.SHAay;
					}

					case Opcodes.SHAay:
					case Opcodes.SHSay or Opcodes.SHXay or Opcodes.STAay:
					{
						instrTable[buildCycle++].Func = FetchLowAddr;
						instrTable[buildCycle++].Func = FetchHighAddrY;
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					// Absolute indirect addressing mode handler
					case Opcodes.JMPi:
					{
						instrTable[buildCycle++].Func = FetchLowPointer;
						instrTable[buildCycle++].Func = FetchHighPointer;
						instrTable[buildCycle++].Func = FetchLowEffAddr;
						instrTable[buildCycle++].Func = FetchHighEffAddr;
						break;
					}

					// Indexed with X preinc addressing mode handler
					case Opcodes.ADCix or Opcodes.ANDix or Opcodes.CMPix or Opcodes.EORix or Opcodes.LAXix or Opcodes.LDAix or
						 Opcodes.ORAix or Opcodes.SBCix or
						 Opcodes.DCPix or Opcodes.ISBix or Opcodes.SLOix or Opcodes.SREix or Opcodes.RLAix or Opcodes.RRAix:
					{
						access = AccessMode.READ;
						goto case Opcodes.SAXix;
					}

					case Opcodes.SAXix:
					case Opcodes.STAix:
					{
						instrTable[buildCycle++].Func = FetchLowPointer;
						instrTable[buildCycle++].Func = FetchLowPointerX;
						instrTable[buildCycle++].Func = FetchLowEffAddr;
						instrTable[buildCycle++].Func = FetchHighEffAddr;
						break;
					}

					// Indexed with Y postinc addressing mode handler (read)
					case Opcodes.ADCiy or Opcodes.ANDiy or Opcodes.CMPiy or Opcodes.EORiy or Opcodes.LAXiy or
						 Opcodes.LDAiy or Opcodes.ORAiy or Opcodes.SBCiy:
					{
						access = AccessMode.READ;
						instrTable[buildCycle++].Func = FetchLowPointer;
						instrTable[buildCycle++].Func = FetchLowEffAddr;
						instrTable[buildCycle++].Func = FetchHighEffAddrY2;
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					// Indexed Y (no page crossing handled)
					case Opcodes.DCPiy or Opcodes.ISBiy or Opcodes.RLAiy or Opcodes.RRAiy or Opcodes.SLOiy or
						 Opcodes.SREiy:
					{
						access = AccessMode.READ;
						goto case Opcodes.SHAiy;
					}

					case Opcodes.SHAiy:
					case Opcodes.STAiy:
					{
						instrTable[buildCycle++].Func = FetchLowPointer;
						instrTable[buildCycle++].Func = FetchLowEffAddr;
						instrTable[buildCycle++].Func = FetchHighEffAddrY;
						instrTable[buildCycle++].Func = ThrowAwayRead;
						break;
					}

					default:
					{
						legalMode = false;
						break;
					}
				}

				if (access == AccessMode.READ)
					instrTable[buildCycle++].Func = FetchEffAddrDataByte;

				//----------------------------------------------------------------------
				// Addressing modes finished, other cycles are instruction dependent
				switch (i)
				{
					case Opcodes.ADCz or Opcodes.ADCzx or Opcodes.ADCa or Opcodes.ADCax or Opcodes.ADCay or Opcodes.ADCix or
						 Opcodes.ADCiy or Opcodes.ADCb:
					{
						instrTable[buildCycle++].Func = ADC_Instr;
						break;
					}

					case Opcodes.ANCb or Opcodes.ANCb1:
					{
						instrTable[buildCycle++].Func = ANC_Instr;
						break;
					}

					case Opcodes.ANDz or Opcodes.ANDzx or Opcodes.ANDa or Opcodes.ANDax or Opcodes.ANDay or Opcodes.ANDix or
						 Opcodes.ANDiy or Opcodes.ANDb:
					{
						instrTable[buildCycle++].Func = AND_Instr;
						break;
					}

					case Opcodes.ANEb: // Also known as XAA
					{
						instrTable[buildCycle++].Func = ANE_Instr;
						break;
					}

					case Opcodes.ARRb:
					{
						instrTable[buildCycle++].Func = ARR_Instr;
						break;
					}

					case Opcodes.ASLn:
					{
						instrTable[buildCycle++].Func = ASLa_Instr;
						break;
					}

					case Opcodes.ASLz or Opcodes.ASLzx or Opcodes.ASLa or Opcodes.ASLax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = ASL_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.ASRb: // Also known as ALR
					{
						instrTable[buildCycle++].Func = ALR_Instr;
						break;
					}

					case Opcodes.BCCr:
					{
						instrTable[buildCycle++].Func = BCC_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BCSr:
					{
						instrTable[buildCycle++].Func = BCS_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BEQr:
					{
						instrTable[buildCycle++].Func = BEQ_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BITz or Opcodes.BITa:
					{
						instrTable[buildCycle++].Func = BIT_Instr;
						break;
					}

					case Opcodes.BMIr:
					{
						instrTable[buildCycle++].Func = BMI_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BNEr:
					{
						instrTable[buildCycle++].Func = BNE_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BPLr:
					{
						instrTable[buildCycle++].Func = BPL_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BRKn:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PushHighPc;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = BRKPushLowPc;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PushSr;
						instrTable[buildCycle++].Func = IrqLoRequest;
						instrTable[buildCycle++].Func = IrqHiRequest;
						instrTable[buildCycle++].Func = FetchNextOpcode;
						break;
					}

					case Opcodes.BVCr:
					{
						instrTable[buildCycle++].Func = BVC_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.BVSr:
					{
						instrTable[buildCycle++].Func = BVS_Instr;
						instrTable[buildCycle++].Func = Fix_Branch;
						break;
					}

					case Opcodes.CLCn:
					{
						instrTable[buildCycle++].Func = CLC_Instr;
						break;
					}

					case Opcodes.CLDn:
					{
						instrTable[buildCycle++].Func = CLD_Instr;
						break;
					}

					case Opcodes.CLIn:
					{
						instrTable[buildCycle++].Func = CLI_Instr;
						break;
					}

					case Opcodes.CLVn:
					{
						instrTable[buildCycle++].Func = CLV_Instr;
						break;
					}

					case Opcodes.CMPz or Opcodes.CMPzx or Opcodes.CMPa or Opcodes.CMPax or Opcodes.CMPay or Opcodes.CMPix or
						 Opcodes.CMPiy or Opcodes.CMPb:
					{
						instrTable[buildCycle++].Func = CMP_Instr;
						break;
					}

					case Opcodes.CPXz or Opcodes.CPXa or Opcodes.CPXb:
					{
						instrTable[buildCycle++].Func = CPX_Instr;
						break;
					}

					case Opcodes.CPYz or Opcodes.CPYa or Opcodes.CPYb:
					{
						instrTable[buildCycle++].Func = CPY_Instr;
						break;
					}

					case Opcodes.DCPz or Opcodes.DCPzx or Opcodes.DCPa or Opcodes.DCPax or Opcodes.DCPay or Opcodes.DCPix or
						 Opcodes.DCPiy:	// Also known as DCM
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = DCM_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.DECz or Opcodes.DECzx or Opcodes.DECa or Opcodes.DECax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = DEC_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.DEXn:
					{
						instrTable[buildCycle++].Func = DEX_Instr;
						break;
					}

					case Opcodes.DEYn:
					{
						instrTable[buildCycle++].Func = DEY_Instr;
						break;
					}

					case Opcodes.EORz or Opcodes.EORzx or Opcodes.EORa or Opcodes.EORax or Opcodes.EORay or Opcodes.EORix or
						 Opcodes.EORiy or Opcodes.EORb:
					{
						instrTable[buildCycle++].Func = EOR_Instr;
						break;
					}

					case Opcodes.INCz or Opcodes.INCzx or Opcodes.INCa or Opcodes.INCax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = INC_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.INXn:
					{
						instrTable[buildCycle++].Func = INX_Instr;
						break;
					}

					case Opcodes.INYn:
					{
						instrTable[buildCycle++].Func = INY_Instr;
						break;
					}

					case Opcodes.ISBz or Opcodes.ISBzx or Opcodes.ISBa or Opcodes.ISBax or Opcodes.ISBay or Opcodes.ISBix or
						 Opcodes.ISBiy:	// Also known as INS
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = INS_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.JSRw:
					{
						instrTable[buildCycle++].Func = WasteCycle;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PushHighPc;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PushLowPc;
						instrTable[buildCycle++].Func = FetchHighAddr;
						goto case Opcodes.JMPw;
					}

					case Opcodes.JMPw:
					case Opcodes.JMPi:
					{
						instrTable[buildCycle++].Func = JMP_Instr;
						break;
					}

					case Opcodes.LASay:
					{
						instrTable[buildCycle++].Func = LAS_Instr;
						break;
					}

					case Opcodes.LAXz or Opcodes.LAXzy or Opcodes.LAXa or Opcodes.LAXay or Opcodes.LAXix or Opcodes.LAXiy:
					{
						instrTable[buildCycle++].Func = LAX_Instr;
						break;
					}

					case Opcodes.LDAz or Opcodes.LDAzx or Opcodes.LDAa or Opcodes.LDAax or Opcodes.LDAay or Opcodes.LDAix or
						 Opcodes.LDAiy or Opcodes.LDAb:
					{
						instrTable[buildCycle++].Func = LDA_Instr;
						break;
					}

					case Opcodes.LDXz or Opcodes.LDXzy or Opcodes.LDXa or Opcodes.LDXay or Opcodes.LDXb:
					{
						instrTable[buildCycle++].Func = LDX_Instr;
						break;
					}

					case Opcodes.LDYz or Opcodes.LDYzx or Opcodes.LDYa or Opcodes.LDYax or Opcodes.LDYb:
					{
						instrTable[buildCycle++].Func = LDY_Instr;
						break;
					}

					case Opcodes.LSRn:
					{
						instrTable[buildCycle++].Func = LSRa_Instr;
						break;
					}

					case Opcodes.LSRz or Opcodes.LSRzx or Opcodes.LSRa or Opcodes.LSRax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = LSR_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.NOPn or Opcodes.NOPn1 or Opcodes.NOPn2 or Opcodes.NOPn3 or Opcodes.NOPn4 or Opcodes.NOPn5 or Opcodes.NOPn6 or
						 Opcodes.NOPb or Opcodes.NOPb1 or Opcodes.NOPb2 or Opcodes.NOPb3 or Opcodes.NOPb4 or
						 Opcodes.NOPz or Opcodes.NOPz1 or Opcodes.NOPz2 or
						 Opcodes.NOPzx or Opcodes.NOPzx1 or Opcodes.NOPzx2 or Opcodes.NOPzx3 or Opcodes.NOPzx4 or Opcodes.NOPzx5 or
						 Opcodes.NOPa or Opcodes.NOPax or Opcodes.NOPax1 or Opcodes.NOPax2 or Opcodes.NOPax3 or Opcodes.NOPax4 or Opcodes.NOPax5:
					{
						// NOPb NOPz NOPzx - Also known as SKBn
						// NOPa NOPax      - Also known as SKWn
						break;
					}

					case Opcodes.LXAb:	// Also known as OAL
					{
						instrTable[buildCycle++].Func = OAL_Instr;
						break;
					}

					case Opcodes.ORAz or Opcodes.ORAzx or Opcodes.ORAa or Opcodes.ORAax or Opcodes.ORAay or Opcodes.ORAix or
						 Opcodes.ORAiy or Opcodes.ORAb:
					{
						instrTable[buildCycle++].Func = ORA_Instr;
						break;
					}

					case Opcodes.PHAn:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PHA_Instr;
						break;
					}

					case Opcodes.PHPn:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PushSr;
						break;
					}

					case Opcodes.PLAn:
					{
						// Should read the value at current stack register.
						// Truely side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						instrTable[buildCycle++].Func = PLA_Instr;
						break;
					}

					case Opcodes.PLPn:
					{
						// Should read the value at current stack register.
						// Truely side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						instrTable[buildCycle++].Func = PopSr;
						break;
					}

					case Opcodes.RLAz or Opcodes.RLAzx or Opcodes.RLAix or Opcodes.RLAa or Opcodes.RLAax or Opcodes.RLAay or
						 Opcodes.RLAiy:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = RLA_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.ROLn:
					{
						instrTable[buildCycle++].Func = ROLa_Instr;
						break;
					}

					case Opcodes.ROLz or Opcodes.ROLzx or Opcodes.ROLa or Opcodes.ROLax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = ROL_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.RORn:
					{
						instrTable[buildCycle++].Func = RORa_Instr;
						break;
					}

					case Opcodes.RORz or Opcodes.RORzx or Opcodes.RORa or Opcodes.RORax:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = ROR_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.RRAa or Opcodes.RRAax or Opcodes.RRAay or Opcodes.RRAz or Opcodes.RRAzx or Opcodes.RRAix or
						 Opcodes.RRAiy:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = RRA_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.RTIn:
					{
						// Should read the value at current stack register.
						// Truely side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						instrTable[buildCycle++].Func = PopSr;
						instrTable[buildCycle++].Func = PopLowPc;
						instrTable[buildCycle++].Func = PopHighPc;
						instrTable[buildCycle++].Func = RTI_Instr;
						break;
					}

					case Opcodes.RTSn:
					{
						// Should read the value at current stack register.
						// Truely side-effect free
						instrTable[buildCycle++].Func = WasteCycle;
						instrTable[buildCycle++].Func = PopLowPc;
						instrTable[buildCycle++].Func = PopHighPc;
						instrTable[buildCycle++].Func = RTS_Instr;
						break;
					}

					case Opcodes.SAXz or Opcodes.SAXzy or Opcodes.SAXa or Opcodes.SAXix:	// Also known as AXS
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = AXS_Instr;
						break;
					}

					case Opcodes.SBCz or Opcodes.SBCzx or Opcodes.SBCa or Opcodes.SBCax or Opcodes.SBCay or Opcodes.SBCix or
						 Opcodes.SBCiy or Opcodes.SBCb or Opcodes.SBCb1:
					{
						instrTable[buildCycle++].Func = SBC_Instr;
						break;
					}

					case Opcodes.SBXb:
					{
						instrTable[buildCycle++].Func = SBX_Instr;
						break;
					}

					case Opcodes.SECn:
					{
						instrTable[buildCycle++].Func = SEC_Instr;
						break;
					}

					case Opcodes.SEDn:
					{
						instrTable[buildCycle++].Func = SED_Instr;
						break;
					}

					case Opcodes.SEIn:
					{
						instrTable[buildCycle++].Func = SEI_Instr;
						break;
					}

					case Opcodes.SHAay or Opcodes.SHAiy:	// Also known as AXA
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = AXA_Instr;
						break;
					}

					case Opcodes.SHSay:	// Also known as TAS
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = SHS_Instr;
						break;
					}

					case Opcodes.SHXay:	// Also known as XAS
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = XAS_Instr;
						break;
					}

					case Opcodes.SHYax:	// Also known as SAY
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = SAY_Instr;
						break;
					}

					case Opcodes.SLOz or Opcodes.SLOzx or Opcodes.SLOa or Opcodes.SLOax or Opcodes.SLOay or Opcodes.SLOix or
						 Opcodes.SLOiy:	// Also known as ASO
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = ASO_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.SREz or Opcodes.SREzx or Opcodes.SREa or Opcodes.SREax or Opcodes.SREay or Opcodes.SREix or
						 Opcodes.SREiy:	// Also known as LSE
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = LSE_Instr;
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = PutEffAddrDataByte;
						break;
					}

					case Opcodes.STAz or Opcodes.STAzx or Opcodes.STAa or Opcodes.STAax or Opcodes.STAay or Opcodes.STAix or
						 Opcodes.STAiy:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = STA_Instr;
						break;
					}

					case Opcodes.STXz or Opcodes.STXzy or Opcodes.STXa:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = STX_Instr;
						break;
					}

					case Opcodes.STYz or Opcodes.STYzx or Opcodes.STYa:
					{
						instrTable[buildCycle].NoSteal = true;
						instrTable[buildCycle++].Func = STY_Instr;
						break;
					}

					case Opcodes.TAXn:
					{
						instrTable[buildCycle++].Func = TAX_Instr;
						break;
					}

					case Opcodes.TAYn:
					{
						instrTable[buildCycle++].Func = TAY_Instr;
						break;
					}

					case Opcodes.TSXn:
					{
						instrTable[buildCycle++].Func = TSX_Instr;
						break;
					}

					case Opcodes.TXAn:
					{
						instrTable[buildCycle++].Func = TXA_Instr;
						break;
					}

					case Opcodes.TXSn:
					{
						instrTable[buildCycle++].Func = TXS_Instr;
						break;
					}

					case Opcodes.TYAn:
					{
						instrTable[buildCycle++].Func = TYA_Instr;
						break;
					}

					default:
					{
						legalInstr = false;
						break;
					}
				}

				// Missing an addressing mode or implementation makes opcode invalid.
				// These are normally called HLT instructions. In the hardware, the
				// CPU state machine locks up and will never recover
				if (!(legalMode || legalInstr))
					instrTable[buildCycle++].Func = InvalidOpcode;

				// Check for IRQ triggers or fetch next opcode...
				instrTable[buildCycle].Func = InterruptsAndNextOpcode;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize CPU emulation (registers)
		/// </summary>
		/********************************************************************/
		private void Initialize()
		{
			// Reset stack
			register_StackPointer = 0xff;

			// Reset cycle count
			cycleCount = (Opcodes.BRKn << 3) + 6;	// FetchNextOpcode

			// Reset status register
			flags.Reset();

			// Set PC to some value
			register_ProgramCounter = 0;

			// IRQ pending check
			irqAssertedOnPin = false;
			nmiFlag = false;
			rstFlag = false;
			interruptCycle = MAX;

			// Signals
			rdy = true;
			d1x1 = false;

			eventScheduler.Schedule(noSteal, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
		}
		#endregion
	}
}

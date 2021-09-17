/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6510
{
	/// <summary>
	/// Special MOS6510 to be fully compatible with SidPlay
	/// </summary>
	internal class Sid6510 : Mos6510
	{
		// SidPlay specials
		private bool sleeping;
		private Sid2Env mode;
		private uint delayClk;
		private bool frameLock;

		private readonly ProcessorCycle[] delayCycle;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sid6510(IEventContext context) : base(context)
		{
			mode = Sid2Env.EnvR;
			frameLock = false;

			// Ok, start all the hacks for SidPlay. This prevents
			// execution of code in roms. For real C64 emulation,
			// create object from base class! Also stops code
			// rom execution when bad code switches roms in over
			// itself
			for (int i = 0; i < Opcodes.OpcodeMax; i++)
			{
				procCycle = instrTable[i].Cycle;
				if (procCycle == null)
					continue;

				for (uint n = 0; n < instrTable[i].Cycles; n++)
				{
					if (procCycle[n].Func == Illegal_Instr)
						procCycle[n].Func = Sid_Illegal;
					else if (procCycle[n].Func == JMP_Instr)
						procCycle[n].Func = Sid_JMP;
					else if (procCycle[n].Func == CLI_Instr)
						procCycle[n].Func = Sid_CLI;
				}
			}

			{
				// Since no real IRQs, all RTIs mapped to RTS
				// Required for fix bad tunes in old modes
				procCycle = instrTable[Opcodes.RTIn].Cycle;
				for (uint n = 0; n < instrTable[Opcodes.RTIn].Cycles; n++)
				{
					if (procCycle[n].Func == PopSr)
					{
						procCycle[n].Func = Sid_RTI;
						break;
					}
				}

				procCycle = interruptTable[(int)Interrupt.Irq].Cycle;
				for (uint n = 0; n < interruptTable[(int)Interrupt.Irq].Cycles; n++)
				{
					if (procCycle[n].Func == IrqRequest)
					{
						procCycle[n].Func = Sid_Irq;
						break;
					}
				}
			}

			{
				// Support of SidPlays BRK functionality
				procCycle = instrTable[Opcodes.BRKn].Cycle;
				for (uint n = 0; n < instrTable[Opcodes.BRKn].Cycles; n++)
				{
					if (procCycle[n].Func == PushHighPc)
					{
						procCycle[n].Func = Sid_BRK;
						break;
					}
				}
			}

			// Used to insert busy delays into the CPU emulator
			delayCycle = Helpers.InitializeArray<ProcessorCycle>(1);
			delayCycle[0].Func = Sid_Delay;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void FetchOpcode()
		{
			if (mode == Sid2Env.EnvR)
			{
				base.FetchOpcode();
				return;
			}

			// Sid tunes end by wrapping the stack. For compatibility it
			// has to be handled
			sleeping |= Endian.Endian16Hi8(registerStackPointer) != SP_Page;
			sleeping |= Endian.Endian32Hi16(registerProgramCounter) != 0;
			if (!sleeping)
				base.FetchOpcode();

			if (!frameLock)
			{
				uint timeout = 6000000;
				frameLock = true;

				// Simulate SidPlay1 frame based execution
				while (!sleeping && (timeout != 0))
				{
					Clock();
					timeout--;
				}

				if (timeout == 0)
				{
					EnvReset();
				}

				Sleep();
				frameLock = false;
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Environment(Sid2Env mode)
		{
			this.mode = mode;
		}



		/********************************************************************/
		/// <summary>
		/// Reset CPU emulation
		/// </summary>
		/********************************************************************/
		public override void Reset()
		{
			sleeping = false;

			base.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Reset CPU emulation
		/// </summary>
		/********************************************************************/
		public void Reset(ushort pc, byte a, byte x, byte y)
		{
			// Reset the processor
			Reset();

			// Registers not touched by a reset
			registerAccumulator = a;
			registerX = x;
			registerY = y;
			registerProgramCounter = pc;
		}

		#region For SidPlay compatibility implement those instructions which don't behave properly
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_BRK()
		{
			if (mode == Sid2Env.EnvR)
			{
				PushHighPc();
				return;
			}

			SEI_Instr();
			Sid_RTS();
			FetchOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_JMP()
		{
			// For SidPlay compatibility, inherited from environment
			if (mode == Sid2Env.EnvR)
			{
				// If a busy loop then just sleep
				if (cycleEffectiveAddress == instrStartPc)
				{
					Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
					if (!InterruptPending())
						Sleep();
				}
				else
					JMP_Instr();

				return;
			}

			if (EnvCheckBankJump(cycleEffectiveAddress))
				JMP_Instr();
			else
				Sid_RTS();
		}



		/********************************************************************/
		/// <summary>
		/// Will do a full RTS in 1 cycle, to destroy current function and
		/// quit
		/// </summary>
		/********************************************************************/
		private void Sid_RTS()
		{
			PopLowPc();
			PopHighPc();
			RTS_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_CLI()
		{
			if (mode == Sid2Env.EnvR)
				CLI_Instr();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_RTI()
		{
			if (mode == Sid2Env.EnvR)
			{
				PopSr();
				return;
			}

			// Fake RTS
			Sid_RTS();
			FetchOpcode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_Irq()
		{
			IrqRequest();

			if (mode != Sid2Env.EnvR)
			{
				// RTI behaves like RTI in SidPlay1 modes
				registerStackPointer++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SidPlay suppresses illegal instructions
		/// </summary>
		/********************************************************************/
		private void Sid_Illegal()
		{
			if (mode == Sid2Env.EnvR)
			{
				Illegal_Instr();
				return;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid_Delay()
		{
			uint stolen = eventContext.GetTime(stealingClk, phase);
			uint delayed = eventContext.GetTime(delayClk, phase);

			// Check for stealing. The relative clock cycle
			// differences are compared here rather than the
			// clocks directly. This means we don't have to
			// worry about the clocks wrapping
			if (delayed > stolen)
			{
				// No longer stealing so adjust clock
				delayed -= stolen;
				delayClk += stolen;
				stealingClk = delayClk;
			}

			cycleCount--;

			// Woken from sleep just to handle the stealing release
			if (sleeping)
				Cancel();
			else
			{
				uint cycle = delayed % 3;
				if (cycle == 0)
				{
					if (InterruptPending())
						return;
				}

				Schedule(eventContext, 3 - cycle, phase);
			}
		}
		#endregion

		#region SidPlay compatibility interrupts. Basically wakes CPU if it is sleeping
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void TriggerNmi()
		{
			// Only in Real C64 mode
			if (mode == Sid2Env.EnvR)
			{
				base.TriggerNmi();

				if (sleeping)
				{
					sleeping = false;
					Schedule(eventContext, eventContext.Phase() == phase ? (uint)1 : 0, phase);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Level triggered interrupt
		/// </summary>
		/********************************************************************/
		public override void TriggerIrq()
		{
			switch (mode)
			{
				default:
					return;

				case Sid2Env.EnvR:
				{
					base.TriggerIrq();

					if (sleeping)
					{
						// Simulate busy loop
						sleeping = !(interrupts.IrqRequest || (interrupts.Pending != InterruptFlag.None));
						if (!sleeping)
							Schedule(eventContext, eventContext.Phase() == phase ? (uint)1 : 0, phase);
					}
					break;
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Send CPU is about to sleep. Only a reset or interrupt will wake
		/// up the processor
		/// </summary>
		/********************************************************************/
		private void Sleep()
		{
			// Simulate a delay for JMPw
			delayClk = stealingClk = eventContext.GetTime(phase);
			procCycle = delayCycle;
			cycleCount = 0;
			sleeping = !(interrupts.IrqRequest || (interrupts.Pending != InterruptFlag.None));
			EnvSleep();
		}
		#endregion
	}
}

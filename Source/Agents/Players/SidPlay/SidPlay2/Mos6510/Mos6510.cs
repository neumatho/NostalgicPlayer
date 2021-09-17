/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6510
{
	/// <summary>
	/// Cycle Accurate 6510 emulation
	/// </summary>
	internal class Mos6510 : C64Environment, IEvent
	{
		#region Event class implementation
		private class MyEvent : Event.Event
		{
			private readonly Mos6510 parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyEvent(Mos6510 parent) : base("CPU")
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// Handle the event
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				Schedule(parent.eventContext, 1, parent.phase);
				parent.Clock();
			}
		}
		#endregion

		#region ProcessorCycle class
		/// <summary></summary>
		protected class ProcessorCycle
		{
			/// <summary></summary>
			public Action Func = null;
			/// <summary></summary>
			public bool NoSteal = false;
		}
		#endregion

		#region ProcessorOperations class
		/// <summary></summary>
		protected class ProcessorOperations
		{
			public ProcessorCycle[] Cycle = null;
			public uint Cycles = 0;
			public byte Opcode;
		}
		#endregion

		#region Interrupts class
		/// <summary></summary>
		protected class Interrupts
		{
			public InterruptFlag Pending;
			public byte Irqs;
			public uint NmiClk;
			public uint IrqClk;
			public bool IrqRequest;
			public bool IrqLatch;
		}
		#endregion

		private const int AccessWrite = 0;
		private const int AccessRead = 1;

		private const uint InterruptDelay = 2;
		private const byte IrqsMax = 3;

		protected const byte SP_Page = 1;

		/// <summary></summary>
		protected enum Interrupt
		{
			None = -1,
			Rst,
			Nmi,
			Irq
		}

		/// <summary></summary>
		[Flags]
		protected enum InterruptFlag
		{
			None = 0,
			Rst = 1 << Interrupt.Rst,
			Nmi = 1 << Interrupt.Nmi,
			Irq = 1 << Interrupt.Irq
		}

		/// <summary>
		/// Status Register flag definitions
		/// </summary>
		[Flags]
		public enum StatusFlag
		{
			None = 0,
			Negative = 1 << 7,
			Overflow = 1 << 6,
			NotUsed = 1 << 5,
			Break = 1 << 4,
			Decimal = 1 << 3,
			Interrupt = 1 << 2,
			Zero = 1 << 1,
			Carry = 1 << 0
		}

		private readonly MyEvent myEvent;

		protected readonly IEventContext eventContext;

		/// <summary>
		/// Clock phase in use by the processor
		/// </summary>
		protected EventPhase phase;

		/// <summary>
		/// Clock phase when external events appear
		/// </summary>
		private EventPhase extPhase;

		// External signals
		private bool aec;
		private bool blocked;
		protected uint stealingClk;

		private readonly ProcessorCycle[] fetchCycle = Helpers.InitializeArray<ProcessorCycle>(1);
		protected ProcessorCycle[] procCycle;
		protected readonly ProcessorOperations[] instrTable = Helpers.InitializeArray<ProcessorOperations>(0x100);
		protected readonly ProcessorOperations[] interruptTable = Helpers.InitializeArray<ProcessorOperations>(3);
		private ProcessorOperations instrCurrent;

		protected ushort instrStartPc;
		private byte instrOpcode;
		protected sbyte cycleCount;

		/// <summary>
		/// Pointers to the current instruction cycle
		/// </summary>
		protected ushort cycleEffectiveAddress;
		private byte cycleData;
		private ushort cyclePointer;

		protected byte registerAccumulator;
		protected byte registerX;
		protected byte registerY;
		protected uint registerProgramCounter;
		private StatusFlag registerStatus;
		private byte registerCFlag;
		private byte registerNFlag;
		private byte registerVFlag;
		private byte registerZFlag;
		protected ushort registerStackPointer;
		private ushort instrOperand;

		protected Interrupts interrupts;

		#region IEvent implementation
		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		public void DoEvent()
		{
			myEvent.DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Schedule event
		/// </summary>
		/********************************************************************/
		public void Schedule(IEventContext context, uint cycles, EventPhase phase)
		{
			myEvent.Schedule(context, cycles, phase);
		}



		/********************************************************************/
		/// <summary>
		/// Cancel the event
		/// </summary>
		/********************************************************************/
		public void Cancel()
		{
			myEvent.Cancel();
		}
		#endregion

		#region Initialize and create CPU chip
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mos6510(IEventContext context)
		{
			myEvent = new MyEvent(this);
			eventContext = context;
			phase = EventPhase.ClockPhi2;
			extPhase = EventPhase.ClockPhi1;

			//----------------------------------------------------------------------
			// Build up the processor instruction table
			for (int i = 0; i < 0x100; i++)
			{
				// Pass 1 allocates the memory, pass 2 builds the instruction
				ProcessorOperations instr = instrTable[i];
				procCycle = null;

				for (int pass = 0; pass < 2; pass++)
				{
					int access = AccessWrite;
					cycleCount = -1;
					bool legalMode = true;
					bool legalInstr = true;

					switch (i)
					{
						// Accumulator or Implied addressing
						case Opcodes.ASLn or Opcodes.CLCn or Opcodes.CLDn or Opcodes.CLIn or Opcodes.CLVn or Opcodes.DEXn or
							 Opcodes.DEYn or Opcodes.INXn or Opcodes.INYn or Opcodes.LSRn or Opcodes.NOPn or Opcodes.NOPn1 or Opcodes.NOPn2 or Opcodes.NOPn3 or Opcodes.NOPn4 or Opcodes.NOPn5 or Opcodes.NOPn6 or Opcodes.PHAn or
							 Opcodes.PHPn or Opcodes.PLAn or Opcodes.PLPn or Opcodes.ROLn or Opcodes.RORn or Opcodes.RTIn or
							 Opcodes.RTSn or Opcodes.SECn or Opcodes.SEDn or Opcodes.SEIn or Opcodes.TAXn or Opcodes.TAYn or
							 Opcodes.TSXn or Opcodes.TXAn or Opcodes.TXSn or Opcodes.TYAn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						// Immediate and Relative addressing mode handler
						case Opcodes.ADCb or Opcodes.ANDb or Opcodes.ANCb or Opcodes.ANCb1 or Opcodes.ANEb or Opcodes.ASRb or Opcodes.ARRb or
							 Opcodes.BCCr or Opcodes.BCSr or Opcodes.BEQr or Opcodes.BMIr or Opcodes.BNEr or Opcodes.BPLr or
							 Opcodes.BRKn or Opcodes.BVCr or Opcodes.BVSr or Opcodes.CMPb or Opcodes.CPXb or Opcodes.CPYb or
							 Opcodes.EORb or Opcodes.LDAb or Opcodes.LDXb or Opcodes.LDYb or Opcodes.LXAb or Opcodes.NOPb or Opcodes.NOPb1 or Opcodes.NOPb2 or Opcodes.NOPb3 or Opcodes.NOPb4 or
							 Opcodes.ORAb or Opcodes.SBCb or Opcodes.SBCb1 or Opcodes.SBXb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchDataByte;

							break;
						}

						// Zero page addressing mode handler - read & RMW
						case Opcodes.ADCz or Opcodes.ANDz or Opcodes.BITz or Opcodes.CMPz or Opcodes.CPXz or Opcodes.CPYz or
							 Opcodes.EORz or Opcodes.LAXz or Opcodes.LDAz or Opcodes.LDXz or Opcodes.LDYz or Opcodes.ORAz or
							 Opcodes.NOPz or Opcodes.NOPz1 or Opcodes.NOPz2 or Opcodes.SBCz or
							 Opcodes.ASLz or Opcodes.DCPz or Opcodes.DECz or Opcodes.INCz or Opcodes.ISBz or Opcodes.LSRz or
							 Opcodes.ROLz or Opcodes.RORz or Opcodes.SREz or Opcodes.SLOz or Opcodes.RLAz or Opcodes.RRAz:
						{
							access++;
							goto case Opcodes.SAXz;
						}

						case Opcodes.SAXz:
						case Opcodes.STAz or Opcodes.STXz or Opcodes.STYz:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Zero page with X offset addressing mode handler
						case Opcodes.ADCzx or Opcodes.ANDzx or Opcodes.CMPzx or Opcodes.EORzx or Opcodes.LDAzx or Opcodes.LDYzx or
							 Opcodes.NOPzx or Opcodes.NOPzx1 or Opcodes.NOPzx2 or Opcodes.NOPzx3 or Opcodes.NOPzx4 or Opcodes.NOPzx5 or Opcodes.ORAzx or Opcodes.SBCzx or
							 Opcodes.ASLzx or Opcodes.DCPzx or Opcodes.DECzx or Opcodes.INCzx or Opcodes.ISBzx or Opcodes.LSRzx or
							 Opcodes.RLAzx or Opcodes.ROLzx or Opcodes.RORzx or Opcodes.RRAzx or Opcodes.SLOzx or Opcodes.SREzx:
						{
							access++;
							goto case Opcodes.STAzx;
						}

						case Opcodes.STAzx:
						case Opcodes.STYzx:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddrX;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Zero page with Y offset addressing mode handler
						case Opcodes.LDXzy or Opcodes.LAXzy:
						{
							access = AccessRead;
							goto case Opcodes.STXzy;
						}

						case Opcodes.STXzy:
						case Opcodes.SAXzy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddrY;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Absolute addressing mode handler
						case Opcodes.ADCa or Opcodes.ANDa or Opcodes.BITa or Opcodes.CMPa or Opcodes.CPXa or Opcodes.CPYa or
							 Opcodes.EORa or Opcodes.LAXa or Opcodes.LDAa or Opcodes.LDXa or Opcodes.LDYa or Opcodes.NOPa or
							 Opcodes.ORAa or Opcodes.SBCa or
							 Opcodes.ASLa or Opcodes.DCPa or Opcodes.DECa or Opcodes.INCa or Opcodes.ISBa or Opcodes.LSRa or
							 Opcodes.ROLa or Opcodes.RORa or Opcodes.SLOa or Opcodes.SREa or Opcodes.RLAa or Opcodes.RRAa:
						{
							access++;
							goto case Opcodes.JMPw;
						}

						case Opcodes.JMPw:
						case Opcodes.JSRw or Opcodes.SAXa or Opcodes.STAa or Opcodes.STXa or Opcodes.STYa:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighAddr;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Absolute with X offset addressing mode handler (read)
						case Opcodes.ADCax or Opcodes.ANDax or Opcodes.CMPax or Opcodes.EORax or Opcodes.LDAax or
							 Opcodes.LDYax or Opcodes.NOPax or Opcodes.NOPax1 or Opcodes.NOPax2 or Opcodes.NOPax3 or Opcodes.NOPax4 or Opcodes.NOPax5 or Opcodes.ORAax or Opcodes.SBCax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighAddrX;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchEffAddrDataByte;

							break;
						}

						// Absolute X (no page crossing handled)
						case Opcodes.ASLax or Opcodes.DCPax or Opcodes.DECax or Opcodes.INCax or Opcodes.ISBax or
							 Opcodes.LSRax or Opcodes.RLAax or Opcodes.ROLax or Opcodes.RORax or Opcodes.RRAax or
							 Opcodes.SLOax or Opcodes.SREax:
						{
							access = AccessRead;
							goto case Opcodes.SHYax;
						}

						case Opcodes.SHYax:
						case Opcodes.STAax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighAddrX2;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Absolute with Y offset addressing mode handler (read)
						case Opcodes.ADCay or Opcodes.ANDay or Opcodes.CMPay or Opcodes.EORay or Opcodes.LASay or
							 Opcodes.LAXay or Opcodes.LDAay or Opcodes.LDXay or Opcodes.ORAay or Opcodes.SBCay:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighAddrY;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchEffAddrDataByte;

							break;
						}

						// Absolute Y (no page crossing handled)
						case Opcodes.DCPay or Opcodes.ISBay or Opcodes.RLAay or Opcodes.RRAay or Opcodes.SLOay or
							 Opcodes.SREay:
						{
							access = AccessRead;
							goto case Opcodes.SHAay;
						}

						case Opcodes.SHAay:
						case Opcodes.SHSay or Opcodes.SHXay or Opcodes.STAay:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighAddrY2;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Absolute indirect addressing mode handler
						case Opcodes.JMPi:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowPointer;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighPointer;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowEffAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighEffAddr;

							break;
						}

						// Indexed with X preinc addressing mode handler
						case Opcodes.ADCix or Opcodes.ANDix or Opcodes.CMPix or Opcodes.EORix or Opcodes.LAXix or Opcodes.LDAix or
							 Opcodes.ORAix or Opcodes.SBCix or
							 Opcodes.DCPix or Opcodes.ISBix or Opcodes.SLOix or Opcodes.SREix or Opcodes.RLAix or Opcodes.RRAix:
						{
							access++;
							goto case Opcodes.SAXix;
						}

						case Opcodes.SAXix:
						case Opcodes.STAix:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowPointer;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowPointerX;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowEffAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighEffAddr;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						// Indexed with Y postinc addressing mode handler (read)
						case Opcodes.ADCiy or Opcodes.ANDiy or Opcodes.CMPiy or Opcodes.EORiy or Opcodes.LAXiy or
							 Opcodes.LDAiy or Opcodes.ORAiy or Opcodes.SBCiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowPointer;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowEffAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighEffAddrY;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchEffAddrDataByte;

							break;
						}

						// Indexed Y (no page crossing handled)
						case Opcodes.DCPiy or Opcodes.ISBiy or Opcodes.RLAiy or Opcodes.RRAiy or Opcodes.SLOiy or
							 Opcodes.SREiy:
						{
							access++;
							goto case Opcodes.SHAiy;
						}

						case Opcodes.SHAiy:
						case Opcodes.STAiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowPointer;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchLowEffAddr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchHighEffAddrY2;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							if (access == AccessRead)
							{
								cycleCount++;
								if (pass != 0)
									procCycle[cycleCount].Func = FetchEffAddrDataByte;
							}
							break;
						}

						default:
						{
							legalMode = false;
							break;
						}
					}

					if (pass != 0)
					{
						// Everything upto now is reads and can therefore
						// be blocked through cycle stealing
						for (int c = -1; c < cycleCount;)
							procCycle[++c].NoSteal = false;
					}

					//----------------------------------------------------------------------
					// Addressing modes finished, other cycles are instruction dependent
					switch (i)
					{
						case Opcodes.ADCz or Opcodes.ADCzx or Opcodes.ADCa or Opcodes.ADCax or Opcodes.ADCay or Opcodes.ADCix or
							 Opcodes.ADCiy or Opcodes.ADCb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ADC_Instr;

							break;
						}

						case Opcodes.ANCb or Opcodes.ANCb1:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ANC_Instr;

							break;
						}

						case Opcodes.ANDz or Opcodes.ANDzx or Opcodes.ANDa or Opcodes.ANDax or Opcodes.ANDay or Opcodes.ANDix or
							 Opcodes.ANDiy or Opcodes.ANDb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = AND_Instr;

							break;
						}

						case Opcodes.ANEb: // Also known as XAA
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ANE_Instr;

							break;
						}

						case Opcodes.ARRb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ARR_Instr;

							break;
						}

						case Opcodes.ASLn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ASLa_Instr;

							break;
						}

						case Opcodes.ASLz or Opcodes.ASLzx or Opcodes.ASLa or Opcodes.ASLax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ASL_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.ASRb: // Also known as ALR
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ALR_Instr;

							break;
						}

						case Opcodes.BCCr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BCC_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BCSr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BCS_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BEQr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BEQ_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BITz or Opcodes.BITa:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BIT_Instr;

							break;
						}

						case Opcodes.BMIr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BMI_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BNEr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BNE_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BPLr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BPL_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BRKn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushHighPc;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushLowPc;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BRK_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Irq1Request;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Irq2Request;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchOpcode;

							break;
						}

						case Opcodes.BVCr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BVC_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.BVSr:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = BVS_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Branch2_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							break;
						}

						case Opcodes.CLCn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CLC_Instr;

							break;
						}

						case Opcodes.CLDn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CLD_Instr;

							break;
						}

						case Opcodes.CLIn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CLI_Instr;

							break;
						}

						case Opcodes.CLVn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CLV_Instr;

							break;
						}

						case Opcodes.CMPz or Opcodes.CMPzx or Opcodes.CMPa or Opcodes.CMPax or Opcodes.CMPay or Opcodes.CMPix or
							 Opcodes.CMPiy or Opcodes.CMPb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CMP_Instr;

							break;
						}

						case Opcodes.CPXz or Opcodes.CPXa or Opcodes.CPXb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CPX_Instr;

							break;
						}

						case Opcodes.CPYz or Opcodes.CPYa or Opcodes.CPYb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = CPY_Instr;

							break;
						}

						case Opcodes.DCPz or Opcodes.DCPzx or Opcodes.DCPa or Opcodes.DCPax or Opcodes.DCPay or Opcodes.DCPix or
							 Opcodes.DCPiy:	// Also known as DCM
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = DCM_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.DECz or Opcodes.DECzx or Opcodes.DECa or Opcodes.DECax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = DEC_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.DEXn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = DEX_Instr;

							break;
						}

						case Opcodes.DEYn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = DEY_Instr;

							break;
						}

						case Opcodes.EORz or Opcodes.EORzx or Opcodes.EORa or Opcodes.EORax or Opcodes.EORay or Opcodes.EORix or
							 Opcodes.EORiy or Opcodes.EORb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = EOR_Instr;

							break;
						}

						case Opcodes.INCz or Opcodes.INCzx or Opcodes.INCa or Opcodes.INCax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = INC_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.INXn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = INX_Instr;

							break;
						}

						case Opcodes.INYn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = INY_Instr;

							break;
						}

						case Opcodes.ISBz or Opcodes.ISBzx or Opcodes.ISBa or Opcodes.ISBax or Opcodes.ISBay or Opcodes.ISBix or
							 Opcodes.ISBiy:	// Also known as INS
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = INS_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.JSRw:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = JSR_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushLowPc;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							goto case Opcodes.JMPw;
						}

						case Opcodes.JMPw:
						case Opcodes.JMPi:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = JMP_Instr;

							break;
						}

						case Opcodes.LASay:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LAS_Instr;

							break;
						}

						case Opcodes.LAXz or Opcodes.LAXzy or Opcodes.LAXa or Opcodes.LAXay or Opcodes.LAXix or Opcodes.LAXiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LAX_Instr;

							break;
						}

						case Opcodes.LDAz or Opcodes.LDAzx or Opcodes.LDAa or Opcodes.LDAax or Opcodes.LDAay or Opcodes.LDAix or
							 Opcodes.LDAiy or Opcodes.LDAb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LDA_Instr;

							break;
						}

						case Opcodes.LDXz or Opcodes.LDXzy or Opcodes.LDXa or Opcodes.LDXay or Opcodes.LDXb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LDX_Instr;

							break;
						}

						case Opcodes.LDYz or Opcodes.LDYzx or Opcodes.LDYa or Opcodes.LDYax or Opcodes.LDYb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LDY_Instr;

							break;
						}

						case Opcodes.LSRn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LSRa_Instr;

							break;
						}

						case Opcodes.LSRz or Opcodes.LSRzx or Opcodes.LSRa or Opcodes.LSRax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LSR_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

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
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = OAL_Instr;

							break;
						}

						case Opcodes.ORAz or Opcodes.ORAzx or Opcodes.ORAa or Opcodes.ORAax or Opcodes.ORAay or Opcodes.ORAix or
							 Opcodes.ORAiy or Opcodes.ORAb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ORA_Instr;

							break;
						}

						case Opcodes.PHAn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PHA_Instr;

							break;
						}

						case Opcodes.PHPn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushSr;

							break;
						}

						case Opcodes.PLAn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PLA_Instr;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							break;
						}

						case Opcodes.PLPn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopSr;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							break;
						}

						case Opcodes.RLAz or Opcodes.RLAzx or Opcodes.RLAix or Opcodes.RLAa or Opcodes.RLAax or Opcodes.RLAay or
							 Opcodes.RLAiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RLA_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.ROLn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ROLa_Instr;

							break;
						}

						case Opcodes.ROLz or Opcodes.ROLzx or Opcodes.ROLa or Opcodes.ROLax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ROL_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.RORn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RORa_Instr;

							break;
						}

						case Opcodes.RORz or Opcodes.RORzx or Opcodes.RORa or Opcodes.RORax:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ROR_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.RRAa or Opcodes.RRAax or Opcodes.RRAay or Opcodes.RRAz or Opcodes.RRAzx or Opcodes.RRAix or
							 Opcodes.RRAiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RRA_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.RTIn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopSr;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopLowPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopHighPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RTI_Instr;

							break;
						}

						case Opcodes.RTSn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopLowPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PopHighPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = false;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RTS_Instr;

							break;
						}

						case Opcodes.SAXz or Opcodes.SAXzy or Opcodes.SAXa or Opcodes.SAXix:	// Also known as AXS
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = AXS_Instr;

							break;
						}

						case Opcodes.SBCz or Opcodes.SBCzx or Opcodes.SBCa or Opcodes.SBCax or Opcodes.SBCay or Opcodes.SBCix or
							 Opcodes.SBCiy or Opcodes.SBCb or Opcodes.SBCb1:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SBC_Instr;

							break;
						}

						case Opcodes.SBXb:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SBX_Instr;

							break;
						}

						case Opcodes.SECn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SEC_Instr;

							break;
						}

						case Opcodes.SEDn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SED_Instr;

							break;
						}

						case Opcodes.SEIn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SEI_Instr;

							break;
						}

						case Opcodes.SHAay or Opcodes.SHAiy:	// Also known as AXA
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = AXA_Instr;

							break;
						}

						case Opcodes.SHSay:	// Also known as TAS
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SHS_Instr;

							break;
						}

						case Opcodes.SHXay:	// Also known as XAS
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = XAS_Instr;

							break;
						}

						case Opcodes.SHYax:	// Also known as SAY
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = SAY_Instr;

							break;
						}

						case Opcodes.SLOz or Opcodes.SLOzx or Opcodes.SLOa or Opcodes.SLOax or Opcodes.SLOay or Opcodes.SLOix or
							 Opcodes.SLOiy:	// Also known as ASO
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = ASO_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.SREz or Opcodes.SREzx or Opcodes.SREa or Opcodes.SREax or Opcodes.SREay or Opcodes.SREix or
							 Opcodes.SREiy:	// Also known as LSE
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = LSE_Instr;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PutEffAddrDataByte;

							break;
						}

						case Opcodes.STAz or Opcodes.STAzx or Opcodes.STAa or Opcodes.STAax or Opcodes.STAay or Opcodes.STAix or
							 Opcodes.STAiy:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = STA_Instr;

							break;
						}

						case Opcodes.STXz or Opcodes.STXzy or Opcodes.STXa:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = STX_Instr;

							break;
						}

						case Opcodes.STYz or Opcodes.STYzx or Opcodes.STYa:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = STY_Instr;

							break;
						}

						case Opcodes.TAXn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TAX_Instr;

							break;
						}

						case Opcodes.TAYn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TAY_Instr;

							break;
						}

						case Opcodes.TSXn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TSX_Instr;

							break;
						}

						case Opcodes.TXAn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TXA_Instr;

							break;
						}

						case Opcodes.TXSn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TXS_Instr;

							break;
						}

						case Opcodes.TYAn:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = TYA_Instr;

							break;
						}

						default:
						{
							legalInstr = false;
							break;
						}
					}

					if (!(legalMode || legalInstr))
					{
						cycleCount++;
						if (pass != 0)
							procCycle[cycleCount].Func = Illegal_Instr;
					}
					else if (!(legalMode && legalInstr))
					{
						Debug.Assert(false, $"Instruction {i}: Not built correctly");
					}

					cycleCount++;
					if (pass != 0)
						procCycle[cycleCount].Func = NextInstr;

					cycleCount++;
					if (pass == 0)
					{
						// Pass 1 - Allocate memory
						if (cycleCount != 0)
						{
							instr.Cycle = Helpers.InitializeArray<ProcessorCycle>(cycleCount);
							procCycle = instr.Cycle;

							int c = cycleCount;
							while (c > 0)
								procCycle[--c].NoSteal = true;
						}
					}
					else
						instr.Opcode = (byte)i;
				}

				instr.Cycles = (uint)cycleCount;
			}

			//----------------------------------------------------------------------
			// Build interrupts
			for (int i = 0; i < 3; i++)
			{
				// Pass 1 allocates the memory, pass 2 builds the interrupt
				ProcessorOperations instr = interruptTable[i];
				instr.Cycle = null;
				instr.Opcode = 0;

				for (int pass = 0; pass < 2; pass++)
				{
					cycleCount = -1;
					if (pass != 0)
						procCycle = instr.Cycle;

					switch ((Interrupt)i)
					{
						case Interrupt.Rst:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = RstRequest;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchOpcode;

							break;
						}

						case Interrupt.Nmi:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushHighPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushLowPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = IrqRequest;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = NmiRequest;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Nmi1Request;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchOpcode;

							break;
						}

						case Interrupt.Irq:
						{
							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = WasteCycle;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushHighPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = PushLowPc;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = IrqRequest;

							if (pass != 0)
								procCycle[cycleCount].NoSteal = true;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Irq1Request;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = Irq2Request;

							cycleCount++;
							if (pass != 0)
								procCycle[cycleCount].Func = FetchOpcode;

							break;
						}
					}

					cycleCount++;
					if (pass == 0)
					{
						// Pass 1 - Allocate memory
						if (cycleCount != 0)
						{
							instr.Cycle = Helpers.InitializeArray<ProcessorCycle>(cycleCount);
							procCycle = instr.Cycle;

							for (int c = 0; c < cycleCount; c++)
								procCycle[c].NoSteal = false;
						}
					}
				}

				instr.Cycles = (uint)cycleCount;
			}

			// Initialize processor registers
			registerAccumulator = 0;
			registerX = 0;
			registerY = 0;

			cycleEffectiveAddress = 0;
			cycleData = 0;
			fetchCycle[0].Func = FetchOpcode;

			Initialize();
		}
		#endregion

		#region Initialize CPU emulation (registers)
		/********************************************************************/
		/// <summary>
		/// Initialize
		/// </summary>
		/********************************************************************/
		private void Initialize()
		{
			// Reset stack
			registerStackPointer = Endian.Endian16(SP_Page, 0xff);

			// Reset cycle count
			cycleCount = 0;
			procCycle = fetchCycle;

			// Reset status register
			registerStatus = StatusFlag.NotUsed | StatusFlag.Break;

			// FLAGS are set from data directly and do not require
			// being calculated first before setting. E.g. if you used
			// SetFlags(0), N flag would be false and Z flag would be true
			SetFlagsNZ(1);
			SetFlagC(false);
			SetFlagV(false);

			// Set PC to some value
			registerProgramCounter = 0;

			// IRQ pending check
			interrupts = new Interrupts
			{
				Pending = InterruptFlag.None,
				Irqs = 0,
				IrqLatch = false,
				IrqRequest = false
			};

			// Signals
			aec = true;

			blocked = false;
			Schedule(eventContext, 0, phase);
		}



		/********************************************************************/
		/// <summary>
		/// Reset CPU emulation
		/// </summary>
		/********************************************************************/
		public virtual void Reset()
		{
			// Internal stuff
			Initialize();

			// Requires external bits
			// Read from reset vector for program entry point
			Endian.Endian16Lo8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xfffc));
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xfffd));

			registerProgramCounter = cycleEffectiveAddress;
		}
		#endregion

		#region Bus methods
		/********************************************************************/
		/// <summary>
		/// Handle bus access signals
		/// </summary>
		/********************************************************************/
		public void AecSignal(bool state)
		{
			if (aec != state)
			{
				uint clock = eventContext.GetTime(extPhase);

				// If the CPU blocked waiting for the bus,
				// then schedule a retry
				aec = state;

				if (state && blocked)
				{
					// Correct IRQs that appeard before the steal
					uint stolen = clock - stealingClk;
					interrupts.NmiClk += stolen;
					interrupts.IrqClk += stolen;

					// IRQs that appeared during the steal must have
					// there clocks corrected
					if (interrupts.NmiClk > clock)
						interrupts.NmiClk = clock - 1;

					if (interrupts.IrqClk > clock)
						interrupts.IrqClk = clock - 1;

					blocked = false;
				}

				Schedule(eventContext, eventContext.Phase() == phase ? (uint)1 : 0, phase);
			}
		}
		#endregion

		#region Status register methods
		/********************************************************************/
		/// <summary>
		/// Set the N and Z flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagsNZ(uint x)
		{
			registerZFlag = registerNFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the N flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagN(uint x)
		{
			registerNFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagV(int x)
		{
			registerVFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagV(bool x)
		{
			registerVFlag = (byte)(x ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// Set the D flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagD(bool x)
		{
			registerStatus &= ~StatusFlag.Decimal;
			if (x)
				registerStatus |= StatusFlag.Decimal;
		}



		/********************************************************************/
		/// <summary>
		/// Set the I flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagI(bool x)
		{
			registerStatus &= ~StatusFlag.Interrupt;
			if (x)
				registerStatus |= StatusFlag.Interrupt;
		}



		/********************************************************************/
		/// <summary>
		/// Set the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagZ(uint x)
		{
			registerZFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagZ(int x)
		{
			registerZFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagZ(bool x)
		{
			registerZFlag = (byte)(x ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// Set the C flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagC(int x)
		{
			registerCFlag = (byte)x;
		}



		/********************************************************************/
		/// <summary>
		/// Set the C flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetFlagC(bool x)
		{
			registerCFlag = (byte)(x ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// Get the N flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagN()
		{
			return (registerNFlag & (byte)StatusFlag.Negative) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the V flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagV()
		{
			return registerVFlag != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the D flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagD()
		{
			return (registerStatus & StatusFlag.Decimal) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the I flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagI()
		{
			return (registerStatus & StatusFlag.Interrupt) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the Z flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagZ()
		{
			return registerZFlag == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the C flag
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetFlagC()
		{
			return registerCFlag != 0;
		}
		#endregion

		#region Interrupts methods
		private static Interrupt[] offTable =
		{
			Interrupt.None, Interrupt.Rst, Interrupt.Nmi, Interrupt.Rst,
			Interrupt.Irq, Interrupt.Rst, Interrupt.Nmi, Interrupt.Rst
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void TriggerNmi()
		{
			interrupts.Pending |= InterruptFlag.Nmi;
			interrupts.NmiClk = eventContext.GetTime(extPhase);
		}



		/********************************************************************/
		/// <summary>
		/// Level triggered interrupt
		/// </summary>
		/********************************************************************/
		public virtual void TriggerIrq()
		{
			// IRQ suppressed
			if (!GetFlagI())
				interrupts.IrqRequest = true;

			if (interrupts.Irqs++ == 0)
				interrupts.IrqClk = eventContext.GetTime(extPhase);

			if (interrupts.Irqs > IrqsMax)
				throw new Exception("An external component is not clearing down it's IRQs");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ClearIrq()
		{
			if (interrupts.Irqs > 0)
			{
				if (--interrupts.Irqs == 0)
				{
					// Clear off the interrupts
					interrupts.IrqRequest = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool InterruptPending()
		{
			// Update IRQ pending
			if (!interrupts.IrqLatch)
			{
				interrupts.Pending &= ~InterruptFlag.Irq;
				if (interrupts.IrqRequest)
					interrupts.Pending |= InterruptFlag.Irq;
			}

			InterruptFlag pending = interrupts.Pending;

		InterruptPendingCheck:
			// Service the highest priority interrupt
			Interrupt offset = offTable[(int)pending];

			switch (offset)
			{
				case Interrupt.None:
					return false;

				case Interrupt.Nmi:
				{
					// Try to determine if we should be processing the NMI yet
					uint cycles = eventContext.GetTime(interrupts.NmiClk, phase);
					if (cycles >= InterruptDelay)
					{
						interrupts.Pending &= ~InterruptFlag.Nmi;
						break;
					}

					// NMI delayed so check for other interrupts
					pending &= ~InterruptFlag.Nmi;
					goto InterruptPendingCheck;
				}

				case Interrupt.Irq:
				{
					// Try to determine if we should be processing the IRQ yet
					uint cycles = eventContext.GetTime(interrupts.IrqClk, phase);
					if (cycles >= InterruptDelay)
						break;

					// IRQ delayed so check for other interrupts
					pending &= ~InterruptFlag.Irq;
					goto InterruptPendingCheck;
				}

				case Interrupt.Rst:
					break;
			}

			// Start the interrupt
			instrCurrent = interruptTable[(int)offset];
			procCycle = instrCurrent.Cycle;
			cycleCount = 0;
			Clock();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RstRequest()
		{
			EnvReset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void NmiRequest()
		{
			Endian.Endian16Lo8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xfffa));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Nmi1Request()
		{
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xfffb));
			Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void IrqRequest()
		{
			PushSr(false);
			SetFlagI(true);
			interrupts.IrqRequest = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Irq1Request()
		{
			Endian.Endian16Lo8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xfffe));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Irq2Request()
		{
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemDataByte(0xffff));
			Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void NextInstr()
		{
			if (!InterruptPending())
			{
				cycleCount = 0;
				procCycle = fetchCycle;
				Clock();
			}
		}
		#endregion

		#region Common instruction addressing methods
		//------------------------------------------------------------------------------
		// Addressing operations as described in 64doc by John West and Marko Makela
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// Fetch opcode, increment PC
		/// Addressing modes: All
		/// </summary>
		/********************************************************************/
		protected virtual void FetchOpcode()
		{
			// On new instruction all interrupt delays are reset
			interrupts.IrqLatch = false;

			instrStartPc = Endian.Endian32Lo16(registerProgramCounter++);
			instrOpcode = EnvReadMemByte(instrStartPc);

			// Convert opcode to pointer in instruction table
			instrCurrent = instrTable[instrOpcode];
			instrOperand = 0;
			procCycle = instrCurrent.Cycle;
			cycleCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch value, increment PC
		/// Addressing modes: Immediate
		///	                  Relative
		/// </summary>
		/********************************************************************/
		private void FetchDataByte()
		{
			// Get data byte from memory
			cycleData = EnvReadMemByte(Endian.Endian32Lo16(registerProgramCounter));
			registerProgramCounter++;
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
		private void FetchLowAddr()
		{
			cycleEffectiveAddress = EnvReadMemByte(Endian.Endian32Lo16(registerProgramCounter));
			registerProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Read from address, add index register X to it
		/// Addressing modes: Zero page indexed
		/// </summary>
		/********************************************************************/
		private void FetchLowAddrX()
		{
			FetchLowAddr();
			cycleEffectiveAddress = (ushort)((cycleEffectiveAddress + registerX) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Read from address, add index register Y to it
		/// Addressing modes: Zero page indexed
		/// </summary>
		/********************************************************************/
		private void FetchLowAddrY()
		{
			FetchLowAddr();
			cycleEffectiveAddress = (ushort)((cycleEffectiveAddress + registerY) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high address byte, increment PC (absolute addressing)
		/// Low byte must have been obtained first!
		/// Addressing modes: Absolute
		/// </summary>
		/********************************************************************/
		private void FetchHighAddr()
		{
			// Get the high byte of an address from memory
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemByte(Endian.Endian32Lo16(registerProgramCounter)));
			registerProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high byte of address, add index register X to low address
		/// byte, increment PC
		/// Addressing modes: Absolute indexed
		/// </summary>
		/********************************************************************/
		private void FetchHighAddrX()
		{
			FetchHighAddr();
			byte page = Endian.Endian16Hi8(cycleEffectiveAddress);
			cycleEffectiveAddress += registerX;

			// Handle page boundary crossing
			if (Endian.Endian16Hi8(cycleEffectiveAddress) == page)
				cycleCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Same as above, except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		private void FetchHighAddrX2()
		{
			FetchHighAddr();
			cycleEffectiveAddress += registerX;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch high byte of address, add index register Y to low address
		/// byte, increment PC
		/// Addressing modes: Absolute indexed
		/// </summary>
		/********************************************************************/
		private void FetchHighAddrY()
		{
			FetchHighAddr();
			byte page = Endian.Endian16Hi8(cycleEffectiveAddress);
			cycleEffectiveAddress += registerY;

			// Handle page boundary crossing
			if (Endian.Endian16Hi8(cycleEffectiveAddress) == page)
				cycleCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Same as above, except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		private void FetchHighAddrY2()
		{
			FetchHighAddr();
			cycleEffectiveAddress += registerY;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch pointer address low, increment PC
		/// Addressing modes: Absolute indirect
		///                   Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		private void FetchLowPointer()
		{
			cyclePointer = EnvReadMemByte(Endian.Endian32Lo16(registerProgramCounter));
			registerProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch pointer from the address and add X to it
		/// Addressing modes: Indexed indirect (pre X)
		/// </summary>
		/********************************************************************/
		private void FetchLowPointerX()
		{
			Endian.Endian16Hi8(ref cyclePointer, EnvReadMemDataByte(cyclePointer));

			// Page boundary crossing is not handled
			cyclePointer = (ushort)((cyclePointer + registerX) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch pointer address high, increment PC
		/// Addressing modes: Absolute indirect
		/// </summary>
		/********************************************************************/
		private void FetchHighPointer()
		{
			Endian.Endian16Hi8(ref cyclePointer, EnvReadMemByte(Endian.Endian32Lo16(registerProgramCounter)));
			registerProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address low
		/// Addressing modes: Indirect
		///                   Indexed indirect (pre X)
		///                   Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		private void FetchLowEffAddr()
		{
			cycleEffectiveAddress = EnvReadMemDataByte(cyclePointer);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address high
		/// Addressing modes: Indirect
		///                   Indexed indirect (pre X)
		/// </summary>
		/********************************************************************/
		private void FetchHighEffAddr()
		{
			Endian.Endian16Lo8(ref cyclePointer, (byte)((cyclePointer + 1) & 0xff));
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemDataByte(cyclePointer));
		}



		/********************************************************************/
		/// <summary>
		/// Fetch effective address high, add Y to low byte of effective
		/// address
		/// Addressing modes: Indirect indexed (post Y)
		/// </summary>
		/********************************************************************/
		private void FetchHighEffAddrY()
		{
			FetchHighEffAddr();
			byte page = Endian.Endian16Hi8(cycleEffectiveAddress);
			cycleEffectiveAddress += registerY;

			// Handle page boundary crossing
			if (Endian.Endian16Hi8(cycleEffectiveAddress) == page)
				cycleCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Same as above, except doesn't worry about page crossing
		/// </summary>
		/********************************************************************/
		private void FetchHighEffAddrY2()
		{
			FetchHighEffAddr();
			cycleEffectiveAddress += registerY;
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
		private void FetchEffAddrDataByte()
		{
			cycleData = EnvReadMemDataByte(cycleEffectiveAddress);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PutEffAddrDataByte()
		{
			EnvWriteMemByte(cycleEffectiveAddress, cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// Push program counter low byte on stack, decrement S
		/// </summary>
		/********************************************************************/
		private void PushLowPc()
		{
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			EnvWriteMemByte(addr, Endian.Endian32Lo8(registerProgramCounter));
			registerStackPointer--;
		}



		/********************************************************************/
		/// <summary>
		/// Push program counter high byte on stack, decrement S
		/// </summary>
		/********************************************************************/
		protected void PushHighPc()
		{
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			EnvWriteMemByte(addr, Endian.Endian32Hi8(registerProgramCounter));
			registerStackPointer--;
		}



		/********************************************************************/
		/// <summary>
		/// Increment stack and pull program counter low byte from stack
		/// </summary>
		/********************************************************************/
		protected void PopLowPc()
		{
			registerStackPointer++;
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			Endian.Endian16Lo8(ref cycleEffectiveAddress, EnvReadMemDataByte(addr));
		}



		/********************************************************************/
		/// <summary>
		/// Increment stack and pull program counter high byte from stack
		/// </summary>
		/********************************************************************/
		protected void PopHighPc()
		{
			registerStackPointer++;
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			Endian.Endian16Hi8(ref cycleEffectiveAddress, EnvReadMemDataByte(addr));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void WasteCycle()
		{
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
		private void BRK_Instr()
		{
			PushSr();
			SetFlagI(true);
			interrupts.IrqRequest = false;

			// Check for an NMI, and switch over if pending
			if ((interrupts.Pending & InterruptFlag.Nmi) != 0)
			{
				uint cycles = eventContext.GetTime(interrupts.NmiClk, phase);
				if (cycles >= InterruptDelay)
				{
					interrupts.Pending &= InterruptFlag.Nmi;
					instrCurrent = interruptTable[(int)Interrupt.Nmi];
					procCycle = instrCurrent.Cycle;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLD_Instr()
		{
			SetFlagD(false);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void CLI_Instr()
		{
			bool oldFlagI = GetFlagI();
			SetFlagI(false);

			// I flag change is delayed by 1 instruction
			interrupts.IrqLatch = oldFlagI ^ GetFlagI();

			// Check to see if interrupts got re-enabled
			if (interrupts.Irqs != 0)
				interrupts.IrqRequest = true;

			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void JMP_Instr()
		{
			Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void JSR_Instr()
		{
			// JSR uses absolute addressing in this emulation,
			// hence the -1. The real SID does not use this addressing mode
			registerProgramCounter--;
			PushHighPc();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PHA_Instr()
		{
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			EnvWriteMemByte(addr, registerAccumulator);
			registerStackPointer--;
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
			Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
			interrupts.IrqLatch = false;
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void RTS_Instr()
		{
			Endian.Endian32Lo16(ref registerProgramCounter, cycleEffectiveAddress);
			registerProgramCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SED_Instr()
		{
			SetFlagD(true);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void SEI_Instr()
		{
			bool oldFlagI = GetFlagI();
			SetFlagI(true);

			// I flag change is delayed by 1 instruction
			interrupts.IrqLatch = oldFlagI ^ GetFlagI();
			interrupts.IrqRequest = false;
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STA_Instr()
		{
			cycleData = registerAccumulator;
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STX_Instr()
		{
			cycleData = registerX;
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void STY_Instr()
		{
			cycleData = registerY;
			PutEffAddrDataByte();
		}
		#endregion

		#region Common instruction undocumented opcodes
		//------------------------------------------------------------------------------
		// See documented 6502-nmo.opc by Adam Vardy for more details
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// This opcode stores the result of A AND X AND the high byte of
		/// the target address of the operand +1 in memory
		/// </summary>
		/********************************************************************/
		private void AXA_Instr()
		{
			cycleData = (byte)(registerX & registerAccumulator & (Endian.Endian16Hi8(cycleEffectiveAddress) + 1));
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// AXS ANDs the contents of the A and X registers (without changing
		/// the contents of either register) and stores the result in memory.
		/// AXS does not affect any flags in the processor status register
		/// </summary>
		/********************************************************************/
		private void AXS_Instr()
		{
			cycleData = (byte)(registerAccumulator & registerX);
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ANDs the contents of the Y register with [ab+1] and
		/// stores the result in memory
		/// </summary>
		/********************************************************************/
		private void SAY_Instr()
		{
			cycleData = (byte)(registerY & (Endian.Endian16Hi8(cycleEffectiveAddress) + 1));
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ANDs the contents of the X register with [ab+1] and
		/// stores the result in memory
		/// </summary>
		/********************************************************************/
		private void XAS_Instr()
		{
			cycleData = (byte)(registerX & (Endian.Endian16Hi8(cycleEffectiveAddress) + 1));
			PutEffAddrDataByte();
		}
		#endregion

		#region Generic binary coded decimal correction
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Perform_ADC()
		{
			uint c = GetFlagC() ? (uint)1 : 0;
			uint a = registerAccumulator;
			uint s = cycleData;
			uint regAC2 = a + s + c;

			if (GetFlagD())
			{
				// BCD mode
				uint lo = (a & 0x0f) + (s & 0x0f) + c;
				uint hi = (a & 0xf0) + (s & 0xf0);

				if (lo > 0x09)
					lo += 0x06;

				if (lo > 0x0f)
					hi += 0x10;

				SetFlagZ(regAC2);
				SetFlagN(hi);
				SetFlagV((((hi ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) == 0));

				if (hi > 0x90)
					hi += 0x60;

				SetFlagC(hi > 0xff);
				registerAccumulator = (byte)(hi | (lo & 0x0f));
			}
			else
			{
				// Binary mode
				SetFlagC(regAC2 > 0xff);
				SetFlagV((((regAC2 ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) == 0));
				SetFlagsNZ(registerAccumulator = (byte)(regAC2 & 0xff));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Perform_SBC()
		{
			uint c = GetFlagC() ? (uint)0 : 1;
			uint a = registerAccumulator;
			uint s = cycleData;
			uint regAC2 = a - s - c;

			SetFlagC(regAC2 < 0x100);
			SetFlagV((((regAC2 ^ a) & 0x80) != 0) && (((a ^ s) & 0x80) != 0));
			SetFlagsNZ(regAC2);

			if (GetFlagD())
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

				registerAccumulator = (byte)(hi | (lo & 0x0f));
			}
			else
			{
				// Binary mode
				registerAccumulator = (byte)(regAC2 & 0xff);
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
			Perform_ADC();
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AND_Instr()
		{
			SetFlagsNZ(registerAccumulator &= cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ANE_Instr()
		{
			SetFlagsNZ(registerAccumulator = (byte)((registerAccumulator | 0xee) & registerX & cycleData));
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ASL_Instr()
		{
			PutEffAddrDataByte();
			SetFlagC(cycleData & 0x80);
			SetFlagsNZ(cycleData <<= 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ASLa_Instr()
		{
			SetFlagC(registerAccumulator & 0x80);
			SetFlagsNZ(registerAccumulator <<= 1);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Branch_Instr(bool condition)
		{
			if (condition)
			{
				byte page = Endian.Endian32Hi8(registerProgramCounter);
				registerProgramCounter = (uint)(registerProgramCounter + (sbyte)cycleData);

				// Handle page boundary crossing
				if (Endian.Endian32Hi8(registerProgramCounter) != page)
					cycleCount++;
			}
			else
			{
				cycleCount += 2;
				Clock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Branch2_Instr()
		{
			// This only gets processed when page boundary
			// is not crossed. This causes pending interrupts
			// to be delayed by a cycle
			interrupts.IrqClk++;
			interrupts.NmiClk++;
			cycleCount++;
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BCC_Instr()
		{
			Branch_Instr(!GetFlagC());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BCS_Instr()
		{
			Branch_Instr(GetFlagC());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BEQ_Instr()
		{
			Branch_Instr(GetFlagZ());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BIT_Instr()
		{
			SetFlagZ(registerAccumulator & cycleData);
			SetFlagN(cycleData);
			SetFlagV(cycleData & 0x40);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BMI_Instr()
		{
			Branch_Instr(GetFlagN());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BNE_Instr()
		{
			Branch_Instr(!GetFlagZ());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BPL_Instr()
		{
			Branch_Instr(!GetFlagN());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BVC_Instr()
		{
			Branch_Instr(!GetFlagV());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BVS_Instr()
		{
			Branch_Instr(GetFlagV());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLC_Instr()
		{
			SetFlagC(false);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CLV_Instr()
		{
			SetFlagV(false);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CMP_Instr()
		{
			ushort tmp = (ushort)(registerAccumulator - cycleData);
			SetFlagsNZ(tmp);
			SetFlagC(tmp < 0x100);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CPX_Instr()
		{
			ushort tmp = (ushort)(registerX - cycleData);
			SetFlagsNZ(tmp);
			SetFlagC(tmp < 0x100);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CPY_Instr()
		{
			ushort tmp = (ushort)(registerY - cycleData);
			SetFlagsNZ(tmp);
			SetFlagC(tmp < 0x100);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEC_Instr()
		{
			PutEffAddrDataByte();
			SetFlagsNZ(--cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEX_Instr()
		{
			SetFlagsNZ(--registerX);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DEY_Instr()
		{
			SetFlagsNZ(--registerY);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EOR_Instr()
		{
			SetFlagsNZ(registerAccumulator ^= cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INC_Instr()
		{
			PutEffAddrDataByte();
			SetFlagsNZ(++cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INX_Instr()
		{
			SetFlagsNZ(++registerX);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void INY_Instr()
		{
			SetFlagsNZ(++registerY);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDA_Instr()
		{
			SetFlagsNZ(registerAccumulator = cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDX_Instr()
		{
			SetFlagsNZ(registerX = cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LDY_Instr()
		{
			SetFlagsNZ(registerY = cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LSR_Instr()
		{
			PutEffAddrDataByte();
			SetFlagC(cycleData & 0x01);
			SetFlagsNZ(cycleData >>= 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LSRa_Instr()
		{
			SetFlagC(registerAccumulator & 0x01);
			SetFlagsNZ(registerAccumulator >>= 1);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ORA_Instr()
		{
			SetFlagsNZ(registerAccumulator |= cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PLA_Instr()
		{
			registerStackPointer++;
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);
			SetFlagsNZ(registerAccumulator = EnvReadMemDataByte(addr));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROL_Instr()
		{
			byte tmp = (byte)(cycleData & 0x80);
			PutEffAddrDataByte();
			cycleData <<= 1;

			if (GetFlagC())
				cycleData |= 0x01;

			SetFlagsNZ(cycleData);
			SetFlagC(tmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROLa_Instr()
		{
			byte tmp = (byte)(registerAccumulator & 0x80);
			registerAccumulator <<= 1;

			if (GetFlagC())
				registerAccumulator |= 0x01;

			SetFlagsNZ(registerAccumulator);
			SetFlagC(tmp);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ROR_Instr()
		{
			byte tmp = (byte)(cycleData & 0x01);
			PutEffAddrDataByte();
			cycleData >>= 1;

			if (GetFlagC())
				cycleData |= 0x80;

			SetFlagsNZ(cycleData);
			SetFlagC(tmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RORa_Instr()
		{
			byte tmp = (byte)(registerAccumulator & 0x01);
			registerAccumulator >>= 1;

			if (GetFlagC())
				registerAccumulator |= 0x80;

			SetFlagsNZ(registerAccumulator);
			SetFlagC(tmp);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SBX_Instr()
		{
			uint tmp = (uint)((registerX & registerAccumulator) - cycleData);
			SetFlagsNZ(registerX = (byte)(tmp & 0xff));
			SetFlagC(tmp < 0x100);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SBC_Instr()
		{
			Perform_SBC();
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SEC_Instr()
		{
			SetFlagC(true);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SHS_Instr()
		{
			Endian.Endian16Lo8(ref registerStackPointer, (byte)(registerAccumulator & registerX));
			cycleData = (byte)((Endian.Endian16Hi8(cycleEffectiveAddress) + 1) & registerStackPointer);
			PutEffAddrDataByte();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TAX_Instr()
		{
			SetFlagsNZ(registerX = registerAccumulator);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TAY_Instr()
		{
			SetFlagsNZ(registerY = registerAccumulator);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TSX_Instr()
		{
			SetFlagsNZ(registerX = Endian.Endian16Lo8(registerStackPointer));
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TXA_Instr()
		{
			SetFlagsNZ(registerAccumulator = registerX);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TXS_Instr()
		{
			Endian.Endian16Lo8(ref registerStackPointer, registerX);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TYA_Instr()
		{
			SetFlagsNZ(registerAccumulator = registerY);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Illegal_Instr()
		{
			// Perform environment reset
			EnvReset();
		}
		#endregion

		#region Generic instruction undocumented opcodes
		//------------------------------------------------------------------------------
		// See documented 6502-nmo.opc by Adam Vardy for more details
		//------------------------------------------------------------------------------

		/********************************************************************/
		/// <summary>
		/// This opcode ANDs the contents of the A register with an immediate
		/// value and then LSRs the result
		/// </summary>
		/********************************************************************/
		private void ALR_Instr()
		{
			registerAccumulator &= cycleData;
			SetFlagC(registerAccumulator & 0x01);
			SetFlagsNZ(registerAccumulator >>= 1);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// ANC ANDs the contents of the A register with an immediate value
		/// and then moves bit 7 of A into the Carry flag. This opcode works
		/// basically identically to AND #immed. except that the Carry flag
		/// is set to the same state that the Negative flag is set to
		/// </summary>
		/********************************************************************/
		private void ANC_Instr()
		{
			SetFlagsNZ(registerAccumulator &= cycleData);
			SetFlagC(GetFlagN());
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ANDs the contents of the A register with an immediate
		/// value and then RORs the result (implementation based on that of
		/// Frodo C64 Emulator)
		/// </summary>
		/********************************************************************/
		private void ARR_Instr()
		{
			byte data = (byte)(cycleData & registerAccumulator);
			registerAccumulator = (byte)(data >> 1);

			if (GetFlagC())
				registerAccumulator |= 0x80;

			if (GetFlagD())
			{
				SetFlagN(0);

				if (GetFlagC())
					SetFlagN((uint)StatusFlag.Negative);

				SetFlagZ(registerAccumulator);
				SetFlagV((data ^ registerAccumulator) & 0x40);

				if ((data & 0x0f) + (data & 0x01) > 5)
					registerAccumulator = (byte)(registerAccumulator & 0xf0 | (registerAccumulator + 6) & 0x0f);

				SetFlagC(((data + (data & 0x10)) & 0x1f0) > 0x50);
				if (GetFlagC())
					registerAccumulator += 0x60;
			}
			else
			{
				SetFlagsNZ(registerAccumulator);
				SetFlagC(registerAccumulator & 0x40);
				SetFlagV((registerAccumulator & 0x40) ^ ((registerAccumulator & 0x20) << 1));
			}

			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ASLs the contents of a memory location and then ORs
		/// the result with the accumulator
		/// </summary>
		/********************************************************************/
		private void ASO_Instr()
		{
			PutEffAddrDataByte();
			SetFlagC(cycleData & 0x80);
			cycleData <<= 1;
			SetFlagsNZ(registerAccumulator |= cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// This opcode DECs the contents of a memory location and then CMPs
		/// the result with the A register
		/// </summary>
		/********************************************************************/
		private void DCM_Instr()
		{
			PutEffAddrDataByte();
			cycleData--;

			ushort tmp = (ushort)(registerAccumulator - cycleData);
			SetFlagsNZ(tmp);
			SetFlagC(tmp < 0x100);
		}



		/********************************************************************/
		/// <summary>
		/// This opcode INCs the contents of a memory location and then SBCs
		/// the result from the A register
		/// </summary>
		/********************************************************************/
		private void INS_Instr()
		{
			PutEffAddrDataByte();
			cycleData++;
			Perform_SBC();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ANDs the contents of a memory location with the
		/// contents of the stack pointer register and stores the result in
		/// the accumulator, the X register, and the stack pointer. Affected
		/// flags: N Z
		/// </summary>
		/********************************************************************/
		private void LAS_Instr()
		{
			SetFlagsNZ(cycleData &= Endian.Endian16Lo8(registerStackPointer));
			registerAccumulator = cycleData;
			registerX = cycleData;
			registerStackPointer = cycleData;
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// This opcode loads both the accumulator and the X register with
		/// the contents of a memory location
		/// </summary>
		/********************************************************************/
		private void LAX_Instr()
		{
			SetFlagsNZ(registerAccumulator = registerX = cycleData);
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// LSE LSRs the contents of a memory location and then EORs the
		/// result with the accumulator
		/// </summary>
		/********************************************************************/
		private void LSE_Instr()
		{
			PutEffAddrDataByte();
			SetFlagC(cycleData & 0x01);
			cycleData >>= 1;
			SetFlagsNZ(registerAccumulator ^= cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// This opcode ORs the A register with #xx, ANDs the result with an
		/// immediate value, and then stores the result in both A and X.
		/// xx may be EE,EF,FE or FF, but most emulators seem to use EE
		/// </summary>
		/********************************************************************/
		private void OAL_Instr()
		{
			SetFlagsNZ(registerX = registerAccumulator = (byte)(cycleData & (registerAccumulator | 0xee)));
			Clock();
		}



		/********************************************************************/
		/// <summary>
		/// RLA ROLs the contents of a memory location and then ANDs the
		/// result with the accumulator
		/// </summary>
		/********************************************************************/
		private void RLA_Instr()
		{
			byte tmp = (byte)(cycleData & 0x80);
			PutEffAddrDataByte();
			cycleData <<= 1;

			if (GetFlagC())
				cycleData |= 0x01;

			SetFlagC(tmp);
			SetFlagsNZ(registerAccumulator &= cycleData);
		}



		/********************************************************************/
		/// <summary>
		/// RRA RORs the contents of a memory location and then ADCs the
		/// result with the accumulator
		/// </summary>
		/********************************************************************/
		private void RRA_Instr()
		{
			byte tmp = (byte)(cycleData & 0x01);
			PutEffAddrDataByte();
			cycleData >>= 1;

			if (GetFlagC())
				cycleData |= 0x80;

			SetFlagC(tmp);
			Perform_ADC();
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Emulate one complete cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Clock()
		{
			sbyte i = cycleCount++;

			if (procCycle[i].NoSteal || aec)
			{
				procCycle[i].Func();
				return;
			}

			if (!blocked)
			{
				blocked = true;
				stealingClk = eventContext.GetTime(phase);
			}

			cycleCount--;
			Cancel();
		}



		/********************************************************************/
		/// <summary>
		/// Push P on stack, decrement S
		/// </summary>
		/********************************************************************/
		private void PushSr(bool flag)
		{
			ushort addr = registerStackPointer;
			Endian.Endian16Hi8(ref addr, SP_Page);

			registerStatus &= (StatusFlag.NotUsed | StatusFlag.Interrupt | StatusFlag.Decimal | StatusFlag.Break);

			if (GetFlagN())
				registerStatus |= StatusFlag.Negative;

			if (GetFlagV())
				registerStatus |= StatusFlag.Overflow;

			if (GetFlagZ())
				registerStatus |= StatusFlag.Zero;

			if (GetFlagC())
				registerStatus |= StatusFlag.Carry;

			EnvWriteMemByte(addr, (byte)(registerStatus & ~(flag ? StatusFlag.None : StatusFlag.Break)));
			registerStackPointer--;
		}



		/********************************************************************/
		/// <summary>
		/// Push P on stack, decrement S
		/// </summary>
		/********************************************************************/
		private void PushSr()
		{
			PushSr(true);
		}



		/********************************************************************/
		/// <summary>
		/// Increment S, pop P off stack
		/// </summary>
		/********************************************************************/
		protected void PopSr()
		{
			bool oldFlagI = GetFlagI();

			// Get status register off stack
			registerStackPointer++;
			{
				ushort addr = registerStackPointer;
				Endian.Endian16Hi8(ref addr, SP_Page);
				registerStatus = (StatusFlag)EnvReadMemDataByte(addr);
			}

			registerStatus |= (StatusFlag.NotUsed | StatusFlag.Break);
			SetFlagN((uint)registerStatus);
			SetFlagV((registerStatus & StatusFlag.Overflow) != 0);
			SetFlagZ((registerStatus & StatusFlag.Zero) == 0);
			SetFlagC((registerStatus & StatusFlag.Carry) != 0);

			// I flag change is delayed by 1 instruction
			bool newFlagI = GetFlagI();
			interrupts.IrqLatch = oldFlagI ^ newFlagI;

			// Check to see if interrupts got re-enabled
			if (!newFlagI && (interrupts.Irqs != 0))
				interrupts.IrqRequest = true;
		}
		#endregion
	}
}

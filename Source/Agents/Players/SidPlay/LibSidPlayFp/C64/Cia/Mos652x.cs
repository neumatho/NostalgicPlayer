/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// This class is heavily based on the ciacore/ciatimer source code from VICE.
	/// The CIA state machine is lifted as-is. Big thanks to VICE project!
	/// The Serial Port emulation is based on Denise emu code
	/// </summary>
	internal abstract class Mos652x
	{
		/// <summary>
		/// 
		/// </summary>
		public enum model_t
		{
			/// <summary>
			/// Old CIA model, interrupts are delayed by 1 clock
			/// </summary>
			MOS6526 = 0,

			/// <summary>
			/// New CIA model
			/// </summary>
			MOS8521,

			/// <summary>
			/// A batch of old CIA model with unique serial port behaviour
			/// </summary>
			MOS6526W4485
		}

		private const int PRA = 0;
		protected const int PRB = 1;
		private const int DDRA = 2;
		protected const int DDRB = 3;
		private const int TAL = 4;
		private const int TAH = 5;
		private const int TBL = 6;
		private const int TBH = 7;
		private const int TOD_TEN = 8;
		private const int TOD_SEC = 9;
		private const int TOD_MIN = 10;
		private const int TOD_HR = 11;
		private const int SDR = 12;
		private const int ICR = 13;
		private const int IDR = 13;
		public const int CRA = 14;
		public const int CRB = 15;

		/// <summary>
		/// Event context
		/// </summary>
		private readonly EventScheduler eventScheduler;

		/// <summary>
		/// These are all CIA registers
		/// </summary>
		protected readonly uint8_t[] regs = new uint8_t[0x10];

		/// <summary>
		/// Timer A
		/// </summary>
		protected readonly TimerA timerA;

		/// <summary>
		/// Timer B
		/// </summary>
		private readonly TimerB timerB;

		/// <summary>
		/// Interrupt source
		/// </summary>
		private InterruptSource interruptSource;

		/// <summary>
		/// TOD
		/// </summary>
		private readonly Tod tod;

		/// <summary>
		/// Serial Data Registers
		/// </summary>
		private readonly SerialPort serialPort;

		/// <summary>
		/// Events
		/// </summary>
		private readonly EventCallback bTickEvent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Mos652x(EventScheduler scheduler)
		{
			eventScheduler = scheduler;
			timerA = new TimerA(scheduler, this);
			timerB = new TimerB(scheduler, this);
			interruptSource = new InterruptSource6526(scheduler, this);
			tod = new Tod(scheduler, this, regs);
			serialPort = new SerialPort(scheduler, this);
			bTickEvent = new EventCallback("CIA B counts A", BTick);

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Handle the serial port
		/// </summary>
		/********************************************************************/
		public void HandleSerialPort()
		{
			if ((regs[CRA] & 0x40) != 0)
				serialPort.Handle();
		}



		/********************************************************************/
		/// <summary>
		/// Reset CIA
		/// </summary>
		/********************************************************************/
		public virtual void Reset()
		{
			Array.Clear(regs, 0, regs.Length);

			serialPort.Reset();

			// Reset timers
			timerA.Reset();
			timerB.Reset();

			// Reset interrupt source
			interruptSource.Reset();

			// Reset tod
			tod.Reset();

			eventScheduler.Cancel(bTickEvent);
		}



		/********************************************************************/
		/// <summary>
		/// Timer A underflow
		/// </summary>
		/********************************************************************/
		public void UnderflowA()
		{
			interruptSource.Trigger(InterruptSource.INTERRUPT_UNDERFLOW_A);

			if ((regs[CRB] & 0x41) == 0x41)
			{
				if (timerB.Started())
					eventScheduler.Schedule(bTickEvent, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Timer B underflow
		/// </summary>
		/********************************************************************/
		public void UnderflowB()
		{
			interruptSource.Trigger(InterruptSource.INTERRUPT_UNDERFLOW_B);
		}



		/********************************************************************/
		/// <summary>
		/// Trigger an interrupt from TOD
		/// </summary>
		/********************************************************************/
		public void TodInterrupt()
		{
			interruptSource.Trigger(InterruptSource.INTERRUPT_ALARM);
		}



		/********************************************************************/
		/// <summary>
		/// Trigger an interrupt from Serial Port
		/// </summary>
		/********************************************************************/
		public void SpInterrupt()
		{
			interruptSource.Trigger(InterruptSource.INTERRUPT_SP);
		}



		/********************************************************************/
		/// <summary>
		/// Select chip model
		/// </summary>
		/********************************************************************/
		public void SetModel(model_t model)
		{
			switch (model)
			{
				case model_t.MOS6526W4485:
				case model_t.MOS6526:
				{
					serialPort.SetModel4485(model == model_t.MOS6526W4485);
					interruptSource = new InterruptSource6526(eventScheduler, this);
					break;
				}

				case model_t.MOS8521:
				{
					serialPort.SetModel4485(false);
					interruptSource = new InterruptSource8521(eventScheduler, this);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set day-of-time event occurrence of date
		/// </summary>
		/********************************************************************/
		public void SetDayOfTimeRate(uint clock)
		{
			tod.SetPeriod(clock);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal interrupt
		/// </summary>
		/********************************************************************/
		public abstract void Interrupt(bool state);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void PortA()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void PortB()
		{
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Read CIA register
		/// </summary>
		/********************************************************************/
		protected uint8_t Read(uint_least8_t addr)
		{
			addr &= 0x0f;

			timerA.SyncWithCpu();
			timerA.WakeUpAfterSyncWithCpu();
			timerB.SyncWithCpu();
			timerB.WakeUpAfterSyncWithCpu();

			switch (addr)
			{
				// Simulate a serial port
				case PRA:
					return (uint8_t)(regs[PRA] | ~regs[DDRA]);

				case PRB:
					return AdjustDataPort((uint8_t)(regs[PRB] | ~regs[DDRB]));

				case TAL:
					return SidEndian.Endian_16Lo8(timerA.GetTimer());

				case TAH:
					return SidEndian.Endian_16Hi8(timerA.GetTimer());

				case TBL:
					return SidEndian.Endian_16Lo8(timerB.GetTimer());

				case TBH:
					return SidEndian.Endian_16Hi8(timerB.GetTimer());

				case TOD_TEN:
				case TOD_SEC:
				case TOD_MIN:
				case TOD_HR:
					return tod.Read((uint_least8_t)(addr - TOD_TEN));

				case IDR:
					return interruptSource.Clear();

				case CRA:
					return (uint8_t)((regs[CRA] & 0xee) | (timerA.GetState() & 1));

				case CRB:
					return (uint8_t)((regs[CRB] & 0xee) | (timerB.GetState() & 1));

				default:
					return regs[addr];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write CIA register
		/// </summary>
		/********************************************************************/
		protected void Write(uint_least8_t addr, uint8_t data)
		{
			addr &= 0x0f;

			timerA.SyncWithCpu();
			timerB.SyncWithCpu();

			uint8_t oldData = regs[addr];
			regs[addr] = data;

			switch (addr)
			{
				case PRA:
				case DDRA:
				{
					PortA();
					break;
				}

				case PRB:
				case DDRB:
				{
					PortB();
					break;
				}

				case TAL:
				{
					timerA.LatchLo(data);
					break;
				}

				case TAH:
				{
					timerA.LatchHi(data);
					break;
				}

				case TBL:
				{
					timerB.LatchLo(data);
					break;
				}

				case TBH:
				{
					timerB.LatchHi(data);
					break;
				}

				case TOD_TEN:
				case TOD_SEC:
				case TOD_MIN:
				case TOD_HR:
				{
					tod.Write((uint_least8_t)(addr - TOD_TEN), data);
					break;
				}

				case SDR:
				{
					serialPort.StartSdr();
					break;
				}

				case ICR:
				{
					interruptSource.Set(data);
					break;
				}

				case CRA:
				{
					if (((data ^ oldData) & 0x40) != 0)
						serialPort.SwitchSerialDirection((data & 0x40) == 0);

					if (((data & 1) != 0) && ((oldData & 1) == 0))
					{
						// Reset the underflow flipflop for the data port
						timerA.SetPbToggle(true);
					}

					timerA.SetControlRegister(data);
					break;
				}

				case CRB:
				{
					if (((data & 1) != 0) && ((oldData & 1) == 0))
					{
						// Reset the underflow flipflop for the data port
						timerB.SetPbToggle(true);
					}

					timerB.SetControlRegister((uint8_t)(data | (data & 0x40) >> 1));
					break;
				}
			}

			timerA.WakeUpAfterSyncWithCpu();
			timerB.WakeUpAfterSyncWithCpu();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// This event exists solely to break the ambiguity of what
		/// scheduling on top of PHI1 causes, because there is no ordering
		/// between events on same phase. Thus it is scheduled in PHI2 to
		/// ensure the b.event() is run once before the value changes.
		///
		/// - PHI1 a.event() (which calls underflow())
		/// - PHI1 b.event()
		/// - PHI2 bTick.event()
		/// - PHI1 a.event()
		/// - PHI1 b.event()
		/// </summary>
		/********************************************************************/
		private void BTick()
		{
			timerB.Cascade();
		}



		/********************************************************************/
		/// <summary>
		/// Timers can appear on the port
		/// </summary>
		/********************************************************************/
		private uint8_t AdjustDataPort(uint8_t data)
		{
			if ((regs[CRA] & 0x02) != 0)
			{
				data &= 0xbf;

				if (timerA.GetPb(regs[CRA]))
					data |= 0x40;
			}

			if ((regs[CRB] & 0x02) != 0)
			{
				data &= 0x7f;

				if (timerB.GetPb(regs[CRB]))
					data |= 0x80;
			}

			return data;
		}
		#endregion
	}
}

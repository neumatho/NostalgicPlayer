/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6526
{
	/// <summary>
	/// CIA timer to produce interrupts
	/// </summary>
	internal abstract class Mos6526 : CoComponent, ISidComponent
	{
		private const int Pra = 0;
		protected const int Prb = 1;
		private const int Ddra = 2;
		protected const int Ddrb = 3;
		private const int Tal = 4;
		private const int Tah = 5;
		private const int Tbl = 6;
		private const int Tbh = 7;
		private const int TodTen = 8;
		private const int TodSec = 9;
		private const int TodMin = 10;
		private const int TodHr = 11;
		private const int Sdr = 12;
		private const int Icr = 13;
		private const int Idr = 13;
		private const int Cra = 14;
		private const int Crb = 15;

		[Flags]
		private enum InterruptType
		{
			None = 0,
			Ta = 1 << 0,
			Tb = 1 << 1,
			Alarm = 1 << 2,
			Sp = 1 << 3,
			Flag = 1 << 4,
			Request = 1 << 7
		}

		protected byte[] regs;
		private bool cntHigh;

		// Timer A
		private byte cra;
		private byte dpa;
		private ushort ta;
		private ushort taLatch;
		private bool taUnderflow;

		// Timer B
		private byte crb;
		private ushort tb;
		private ushort tbLatch;
		private bool tbUnderflow;

		// Serial data registers
		private byte sdrOut;
		private bool sdrBuffered;
		private int sdrCount;

		private InterruptType icr;					// Interrupt control registers
		private InterruptType idr;
		private uint accessClk;
		private IEventContext eventContext;
		private EventPhase phase;

		private bool todLatched;
		private bool todStopped;
		private byte[] todClock;
		private byte[] todAlarm;
		private byte[] todLatch;
		private uint todCycles;
		private uint todPeriod;

		private EventCallback taEvent;
		private EventCallback tbEvent;
		private EventCallback todEvent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Mos6526(IEventContext context) : base("MOS6526")
		{
			idr = InterruptType.None;
			eventContext = context;
			phase = EventPhase.ClockPhi1;
			todPeriod = 0xffffffff;			// Dummy

			taEvent = new EventCallback("CIA Timer A", TaEvent);
			tbEvent = new EventCallback("CIA Timer B", TbEvent);
			todEvent = new EventCallback("CIA Time of Day", TodEvent);

			Reset();
		}

		#region ISidUnknown overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IQuery(IId iid, out object implementation)
		{
			implementation = null;
			return false;
		}
		#endregion

		#region ISidComponent implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Reset()
		{
			ta = taLatch = 0xffff;
			tb = tbLatch = 0xffff;
			taUnderflow = tbUnderflow = false;
			cra = crb = sdrOut = 0;
			sdrCount = 0;
			sdrBuffered = false;

			// Clear off any IRQs
			Trigger(InterruptType.None);
			cntHigh = true;
			icr = idr = InterruptType.None;
			accessClk = 0;
			dpa = 0xf0;
			regs = new byte[0x10];

			// Reset tod
			todClock = new byte[4];
			todAlarm = new byte[4];
			todLatch = new byte[4];
			todLatched = false;
			todStopped = true;
			todClock[TodHr - TodTen] = 1;		// The most common value
			todCycles = 0;

			// Remove outstanding events
			taEvent.Cancel();
			tbEvent.Cancel();
			todEvent.Schedule(eventContext, 0, phase);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Read(byte addr)
		{
			if (addr > 0x0f)
				return 0;

			bool taPulse = false;
			bool tbPulse = false;

			uint cycles = eventContext.GetTime(accessClk, eventContext.Phase());
			accessClk += cycles;

			// Sync up timers
			if ((cra & 0x21) == 0x01)
			{
				ta -= (ushort)cycles;
				if (ta == 0)
				{
					TaEvent();
					taPulse = true;
				}
			}

			if ((crb & 0x61) == 0x01)
			{
				tb -= (ushort)cycles;
				if (tb == 0)
				{
					TbEvent();
					tbPulse = true;
				}
			}

			switch (addr)
			{
				case Pra:
				{
					// Simulate a serial port
					return (byte)(regs[Pra] | ~regs[Ddra]);
				}

				case Prb:
				{
					byte data = (byte)(regs[Prb] | ~regs[Ddrb]);

					// Timers can appear on the port
					if ((cra & 0x02) != 0)
					{
						data &= 0xbf;

						if ((cra & 0x04) != 0 ? taUnderflow : taPulse)
							data |= 0x40;
					}

					if ((crb & 0x02) != 0)
					{
						data &= 0x7f;

						if ((crb & 0x04) != 0 ? tbUnderflow : tbPulse)
							data |= 0x80;
					}

					return data;
				}

				case Tal:
					return Endian.Endian16Lo8(ta);

				case Tah:
					return Endian.Endian16Hi8(ta);

				case Tbl:
					return Endian.Endian16Lo8(tb);

				case Tbh:
					return Endian.Endian16Hi8(tb);

				// TOD implementation taken from Vice
				// TOD clock is latched by reading hours, and released
				// upon reading tenths of seconds. The counter itself
				// keeps ticking all the time.
				// Also note that this latching is different from the input one
				case TodTen:	// Time of Day clock 1/10 s
				case TodSec:	// Time of Day clock sec
				case TodMin:	// Time of Day clock min
				case TodHr:		// Time of Day clock hour
				{
					if (!todLatched)
						Array.Copy(todClock, 0, todLatch, 0, todLatch.Length);

					if (addr == TodTen)
						todLatched = false;

					if (addr == TodHr)
						todLatched = true;

					return todLatch[addr - TodTen];
				}

				case Idr:
				{
					// Clear IRQs and return interrupt
					// data register
					byte ret = (byte)idr;
					Trigger(InterruptType.None);

					return ret;
				}

				case Cra:
					return cra;

				case Crb:
					return crb;

				default:
					return regs[addr];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(byte addr, byte data)
		{
			if (addr > 0x0f)
				return;

			regs[addr] = data;
			uint cycles = eventContext.GetTime(accessClk, eventContext.Phase());

			if (cycles != 0)
			{
				accessClk += cycles;

				// Sync up timers
				if ((cra & 0x21) == 0x01)
				{
					ta -= (ushort)cycles;
					if (ta == 0)
						TaEvent();
				}

				if ((crb & 0x61) == 0x01)
				{
					tb -= (ushort)cycles;
					if (tb == 0)
						TbEvent();
				}
			}

			switch (addr)
			{
				case Pra or Ddra:
				{
					PortA();
					break;
				}

				case Prb or Ddrb:
				{
					PortB();
					break;
				}

				case Tal:
				{
					Endian.Endian16Lo8(ref taLatch, data);
					break;
				}

				case Tah:
				{
					Endian.Endian16Hi8(ref taLatch, data);

					// Reload timer if stopped
					if ((cra & 0x01) == 0)
						ta = taLatch;

					break;
				}

				case Tbl:
				{
					Endian.Endian16Lo8(ref tbLatch, data);
					break;
				}

				case Tbh:
				{
					Endian.Endian16Hi8(ref tbLatch, data);

					// Reload timer if stopped
					if ((crb & 0x01) == 0)
						tb = tbLatch;

					break;
				}

				// TOD implementation taken from Vice
				//
				// Time Of Day clock hour
				// Flip AM/PM on hour 12
				//    (Andreas Boose <viceteam@t-online.de> 1997/10/11).
				// Flip AM/PM only when writing time, not when writing alarm
				//    (Alexander Bluhm <mam96ehy@studserv.uni-leipzig.de> 2000/09/17)
				case TodHr:
				{
					data &= 0x9f;
					if (((data & 0x1f) == 0x12) && ((crb & 0x80) == 0))
						data ^= 0x80;

					goto case TodTen;
				}

				case TodTen:        // Time Of Day clock 1/10 s
				case TodSec:        // Time Of Day clock sec
				case TodMin:		// Time Of Day clock min
				{
					if ((crb & 0x80) != 0)
						todAlarm[addr - TodTen] = data;
					else
					{
						if (addr == TodTen)
							todStopped = false;

						if (addr == TodHr)
							todStopped = true;

						todClock[addr - TodTen] = data;
					}

					// Check alarm
					if (!todStopped && todAlarm.SequenceEqual(todClock))
						Trigger(InterruptType.Alarm);

					break;
				}

				case Sdr:
				{
					if ((cra & 0x40) != 0)
						sdrBuffered = true;

					break;
				}

				case Icr:
				{
					if ((data & 0x80) != 0)
						icr |= (InterruptType)(data & 0x1f);
					else
						icr &= (InterruptType)~data;

					Trigger(idr);
					break;
				}

				case Cra:
				{
					// Reset the underflow flip-flop for the data port
					if (((data & 1) != 0) && ((cra & 1) == 0))
					{
						ta = taLatch;
						taUnderflow = true;
					}

					cra = data;

					// Check for forced load
					if ((data & 0x10) != 0)
					{
						cra &= unchecked((byte)~0x10);
						ta = taLatch;
					}

					if ((data & 0x21) == 0x01)
					{
						// Active
						taEvent.Schedule(eventContext, (uint)ta + 3, phase);
					}
					else
					{
						// Inactive
						taEvent.Cancel();
					}
					break;
				}

				case Crb:
				{
					// Reset the underflow flip-flop for the data port
					if (((data & 1) != 0) && ((crb & 1) == 0))
					{
						tb = tbLatch;
						tbUnderflow = true;
					}

					crb = data;

					// Check for forced load
					if ((data & 0x10) != 0)
					{
						crb &= unchecked((byte)~0x10);
						tb = tbLatch;
					}

					if ((data & 0x61) == 0x01)
					{
						// Active
						tbEvent.Schedule(eventContext, (uint)tb + 3, phase);
					}
					else
					{
						// Inactive
						tbEvent.Cancel();
					}
					break;
				}
			}
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void Interrupt(bool state);



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
		/// 
		/// </summary>
		/********************************************************************/
		public void Clock(double clock)
		{
			// @FIXME@ This is not correct! There should be multiple schedulers
			// running at different rates that are passed into different
			// function calls. This is the same as have different clock freqs
			// connected to pins on the IC
			//
			// Fixed point 25.7
			todPeriod = (uint)(clock * (1 << 7));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Trigger(InterruptType irq)
		{
			if (irq == InterruptType.None)
			{
				// Clear any requested IRQs
				if ((idr & InterruptType.Request) != 0)
					Interrupt(false);

				idr = InterruptType.None;
				return;
			}

			idr |= irq;
			if ((icr & idr) != 0)
			{
				if ((idr & InterruptType.Request) == 0)
				{
					idr |= InterruptType.Request;
					Interrupt(true);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TaEvent()
		{
			// Timer modes
			byte mode = (byte)(cra & 0x21);

			if (mode == 0x21)
			{
				if (ta-- != 0)
					return;
			}

			uint cycles = eventContext.GetTime(accessClk, phase);
			accessClk += cycles;

			ta = taLatch;
			taUnderflow ^= true;	// Toggle flip-flop

			if ((cra & 0x08) != 0)
			{
				// One shot, stop timer A
				cra &= unchecked((byte)~0x01);
			}
			else if (mode == 0x01)
			{
				// Reset event
				taEvent.Schedule(eventContext, (uint)(ta + 1), phase);
			}

			Trigger(InterruptType.Ta);

			// Handle serial port
			if ((cra & 0x40) != 0)
			{
				if (sdrCount != 0)
				{
					if (--sdrCount == 0)
						Trigger(InterruptType.Sp);
				}

				if ((sdrCount == 0) && sdrBuffered)
				{
					sdrOut = regs[Sdr];
					sdrBuffered = false;
					sdrCount = 16;			// Output rate 8 bits at ta / 2
				}
			}

			switch (crb & 0x61)
			{
				case 0x01:
				{
					tb -= (ushort)cycles;
					break;
				}

				case 0x41:
				case 0x61:
				{
					TbEvent();
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TbEvent()
		{
			// Timer modes
			byte mode = (byte)(crb & 0x61);

			switch (mode)
			{
				case 0x01:
					break;

				case 0x21:
				case 0x41:
				{
					if (tb-- != 0)
						return;

					break;
				}

				case 0x61:
				{
					if (cntHigh)
					{
						if (tb-- != 0)
							return;
					}
					break;
				}

				default:
					return;
			}

			accessClk = eventContext.GetTime(phase);
			tb = tbLatch;
			tbUnderflow ^= true;		// Toggle flip-flop

			if ((crb & 0x08) != 0)
			{
				// One shot, stop timer B
				crb &= unchecked((byte)~0x01);
			}
			else if (mode == 0x01)
			{
				// Reset event
				tbEvent.Schedule(eventContext, (uint)(tb + 1), phase);
			}

			Trigger(InterruptType.Tb);
		}



		/********************************************************************/
		/// <summary>
		/// TOD implementation taken from Vice
		/// </summary>
		/********************************************************************/
		private void TodEvent()
		{
			// Reload divider according to 50/60 Hz flag
			// Only performed on expiry according to Frodo
			if ((cra & 0x80) != 0)
				todCycles += todPeriod * 5;
			else
				todCycles += todPeriod * 6;

			// Fixed precision 25.7
			todEvent.Schedule(eventContext, todCycles >> 7, phase);
			todCycles &= 0x7f;	// Just keep the decimal part

			if (!todStopped)
			{
				// Inc timer
				int tod = 0;
				byte t = (byte)(Bcd2Byte(todClock[tod]) + 1);
				todClock[tod++] = Byte2Bcd(t % 10);
				if (t >= 10)
				{
					t = (byte)(Bcd2Byte(todClock[tod]) + 1);
					todClock[tod++] = Byte2Bcd(t % 60);
					if (t >= 60)
					{
						t = (byte)(Bcd2Byte(todClock[tod]) + 1);
						todClock[tod++] = Byte2Bcd(t % 60);
						if (t >= 60)
						{
							byte pm = (byte)(todClock[tod] & 0x80);
							t = (byte)(todClock[tod] & 0x1f);

							if (t == 0x11)
								pm ^= 0x80;		// Toggle am/pm on 0:59->1:00 hr

							if (t == 0x12)
								t = 1;
							else if (++t == 10)
								t = 0x10;		// Increment, adjust bcd

							t &= 0x1f;
							todClock[tod] = (byte)(t | pm);
						}
					}
				}

				// Check alarm
				if (todAlarm.SequenceEqual(todClock))
					Trigger(InterruptType.Alarm);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte Bcd2Byte(byte byt)
		{
			return (byte)((((byt / 10) << 4) + (byt % 10)) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte Byte2Bcd(int bcd)
		{
			return (byte)(((10 * ((bcd & 0xf0) >> 4)) + (bcd & 0xf)) & 0xff);
		}
		#endregion
	}
}

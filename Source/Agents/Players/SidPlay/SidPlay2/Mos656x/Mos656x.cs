/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos656x
{
	/// <summary>
	/// Minimal VIC emulation
	/// </summary>
	internal abstract class Mos656x : CoComponent, ISidComponent
	{
		#region Event class implementation
		private class MyEvent : Event.Event
		{
			private readonly Mos656x parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyEvent(Mos656x parent) : base("VIC Raster")
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
				parent.DoEvent();
			}
		}
		#endregion

		private const ushort Mos6567R56A_ScreenHeight = 262;
		private const ushort Mos6567R56A_ScreenWidth = 64;
		private const ushort Mos6567R56A_FirstDmaLine = 0x30;
		private const ushort Mos6567R56A_LastDmaLine = 0xf7;

		private const ushort Mos6567R8_ScreenHeight = 263;
		private const ushort Mos6567R8_ScreenWidth = 65;
		private const ushort Mos6567R8_FirstDmaLine = 0x30;
		private const ushort Mos6567R8_LastDmaLine = 0xf7;

		private const ushort Mos6569_ScreenHeight = 312;
		private const ushort Mos6569_ScreenWidth = 63;
		private const ushort Mos6569_FirstDmaLine = 0x30;
		private const ushort Mos6569_LastDmaLine = 0xf7;

		private const int SpriteEnable = 0x15;
		private const int SpriteYExpansion = 0x17;

		[Flags]
		private enum InterruptType
		{
			None = 0,
			Rst = 1 << 0,
			Lp = 1 << 3,
			Request = 1 << 7
		}

		private MyEvent myEvent;

		private IEventContext eventContext;
		private EventPhase phase;

		private byte[] regs;
		private InterruptType icr;
		private InterruptType idr;
		private byte ctrl1;

		private ushort yRasters;
		private ushort xRasters;
		private ushort rasterIrq;
		private ushort rasterX;
		private ushort rasterY;
		private ushort firstDmaLine;
		private ushort lastDmaLine;
		private ushort yScroll;
		private bool badLinesEnabled;
		private bool badLine;
		private bool vBlanking;
		private bool lpTriggered;
		private byte lpX;
		private byte lpY;
		private byte spriteDma;
		private byte spriteExpandY;
		private byte[] spriteMcBase;

		private uint rasterClk;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Mos656x(IEventContext context) : base("MOS656x")
		{
			myEvent = new MyEvent(this);

			eventContext = context;
			phase = EventPhase.ClockPhi1;

			Chip(Mos656xModel.Mos6569);
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
		public void Reset()
		{
			icr = idr = InterruptType.None;
			ctrl1 = 0;
			rasterIrq = 0;
			yScroll = 0;
			rasterY = (ushort)(yRasters - 1);
			rasterX = 0;
			badLinesEnabled = false;
			rasterClk = 0;
			vBlanking = lpTriggered = false;
			lpX = lpY = 0;
			spriteDma = 0;
			spriteExpandY = 0xff;

			regs = new byte[0x40];
			spriteMcBase = new byte[8];

			myEvent.Schedule(eventContext, 0, phase);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Read(byte addr)
		{
			if (addr > 0x3f)
				return 0;

			if (addr > 0x2e)
				return 0xff;

			// Sync up timers
			DoEvent();

			switch (addr)
			{
				// Control register 1
				case 0x11:
					return (byte)((ctrl1 & 0x7f) | ((rasterY & 0x100) >> 1));

				// Raster counter
				case 0x12:
					return (byte)(rasterY & 0xff);

				case 0x13:
					return lpX;

				case 0x14:
					return lpY;

				// IRQ flags
				case 0x19:
					return (byte)idr;

				// IRQ mask
				case 0x1a:
					return (byte)((byte)icr | 0xf0);

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
			if (addr > 0x3f)
				return;

			regs[addr] = data;

			// Sync up timers
			myEvent.DoEvent();

			switch (addr)
			{
				case 0x11:
				{
					// Control register 1
					Endian.Endian16Hi8(ref rasterIrq, (byte)(data >> 7));
					ctrl1 = data;
					yScroll = (ushort)(data & 7);

					if (rasterX < 11)
						break;

					// In line $30, the DEN bit controls if Bad Lines can occur
					if ((rasterY == firstDmaLine) && ((data & 0x10) != 0))
						badLinesEnabled = true;

					// Bad line condition?
					badLine = (rasterY >= firstDmaLine) && (rasterY <= lastDmaLine) && ((rasterY & 7) == yScroll) && badLinesEnabled;

					// Start bad DMA line now
					if (badLine && (rasterX < 53))
						AddrCtrl(false);

					break;
				}

				case 0x12:
				{
					// Raster counter
					Endian.Endian16Lo8(ref rasterIrq, data);
					break;
				}

				case 0x17:
				{
					spriteExpandY |= (byte)~data;	// 3.8.1-1
					break;
				}

				case 0x19:
				{
					// IRQ flags
					idr &= (InterruptType)((~data & 0x0f) | 0x80);
					if (idr == InterruptType.Request)
						Trigger(InterruptType.None);

					break;
				}

				case 0x1a:
				{
					// IRQ mask
					icr = (InterruptType)(data & 0x0f);
					Trigger(icr & idr);
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
		protected abstract void AddrCtrl(bool state);
		#endregion

		/********************************************************************/
		/// <summary>
		/// Handle light pen trigger
		/// </summary>
		/********************************************************************/
		public void LightPen()
		{
			// Synchronise simulation
			myEvent.DoEvent();

			if (!lpTriggered)
			{
				// Latch current coordinates
				lpX = (byte)(rasterX << 2);
				lpY = (byte)(rasterY & 0xff);
				Trigger(InterruptType.Lp);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Chip(Mos656xModel model)
		{
			switch (model)
			{
				case Mos656xModel.Mos6567R56A:
				{
					// Seem to be an older NTSC chip
					yRasters = Mos6567R56A_ScreenHeight;
					xRasters = Mos6567R56A_ScreenWidth;
					firstDmaLine = Mos6567R56A_FirstDmaLine;
					lastDmaLine = Mos6567R56A_LastDmaLine;
					break;
				}

				case Mos656xModel.Mos6567R8:
				{
					// NTSC chip
					yRasters = Mos6567R8_ScreenHeight;
					xRasters = Mos6567R8_ScreenWidth;
					firstDmaLine = Mos6567R8_FirstDmaLine;
					lastDmaLine = Mos6567R8_LastDmaLine;
					break;
				}

				case Mos656xModel.Mos6569:
				{
					// PAL chip
					yRasters = Mos6569_ScreenHeight;
					xRasters = Mos6569_ScreenWidth;
					firstDmaLine = Mos6569_FirstDmaLine;
					lastDmaLine = Mos6569_LastDmaLine;
					break;
				}
			}

			Reset();
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
		/// Handle the event
		/// </summary>
		/********************************************************************/
		private void DoEvent()
		{
			uint cycles = eventContext.GetTime(rasterClk, eventContext.Phase());

			// Cycle already executed check
			if (cycles == 0)
				return;

			uint delay = 1;

			// Update x raster
			rasterClk += cycles;
			rasterX += (ushort)cycles;
			ushort cycle = (ushort)((rasterX + 9) % xRasters);
			rasterX %= xRasters;

			switch (cycle)
			{
				case 0:
				{
					// Calculate sprite DMA
					byte y = (byte)(rasterY & 0xff);
					byte mask = 1;
					spriteExpandY ^= regs[SpriteYExpansion];	// 3.8.1-2

					for (int i = 1; i < 0x10; i += 2, mask <<= 1)
					{
						// 3.8.1-3
						if (((SpriteEnable & mask) != 0) && (y == regs[i]))
						{
							spriteDma |= mask;
							spriteMcBase[i >> 1] = 0;
							spriteExpandY &= (byte)~(regs[SpriteYExpansion] & mask);
						}
					}

					delay = 2;

					if ((spriteDma & 0x01) != 0)
						AddrCtrl(false);
					else
					{
						AddrCtrl(true);

						// No sprite before next compulsory cycle
						if ((spriteDma & 0x1f) == 0)
							delay = 9;
					}
					break;
				}

				case 1:
					break;

				case 2:
				{
					if ((spriteDma & 0x02) != 0)
						AddrCtrl(false);

					break;
				}

				case 3:
				{
					if ((spriteDma & 0x03) == 0)
						AddrCtrl(true);

					break;
				}

				case 4:
				{
					if ((spriteDma & 0x04) != 0)
						AddrCtrl(false);

					break;
				}

				case 5:
				{
					if ((spriteDma & 0x06) == 0)
						AddrCtrl(true);

					break;
				}

				case 6:
				{
					if ((spriteDma & 0x08) != 0)
						AddrCtrl(false);

					break;
				}

				case 7:
				{
					if ((spriteDma & 0x0c) == 0)
						AddrCtrl(true);

					break;
				}

				case 8:
				{
					if ((spriteDma & 0x10) != 0)
						AddrCtrl(false);

					break;
				}

				case 9:		// IRQ occurred (xRaster != 0)
				{
					if (rasterY == (yRasters - 1))
						vBlanking = true;
					else
					{
						rasterY++;

						// Trigger raster IRQ if IRQ line reached
						if (rasterY == rasterIrq)
							Trigger(InterruptType.Rst);
					}

					if ((spriteDma & 0x18) == 0)
						AddrCtrl(true);

					break;
				}

				case 10:	// Vertical blank (line 0)
				{
					if (vBlanking)
					{
						vBlanking = lpTriggered = false;
						rasterY = 0;

						// Trigger raster IRQ if IRQ in line 0
						if (rasterIrq == 0)
							Trigger(InterruptType.Rst);
					}

					if ((spriteDma & 0x20) != 0)
						AddrCtrl(false);
					else if ((spriteDma & 0xf8) == 0)	// No sprites before next compulsory cycle
						delay = 10;

					break;
				}

				case 11:
				{
					if ((spriteDma & 0x30) == 0)
						AddrCtrl(true);

					break;
				}

				case 12:
				{
					if ((spriteDma & 0x40) != 0)
						AddrCtrl(false);

					break;
				}

				case 13:
				{
					if ((spriteDma & 0x60) == 0)
						AddrCtrl(true);

					break;
				}

				case 14:
				{
					if ((spriteDma & 0x80) != 0)
						AddrCtrl(false);

					break;
				}

				case 15:
				{
					delay = 2;

					if ((spriteDma & 0xc0) == 0)
					{
						AddrCtrl(true);
						delay = 5;
					}
					break;
				}

				case 16:
					break;

				case 17:
				{
					delay = 2;

					if ((spriteDma & 0x80) == 0)
					{
						AddrCtrl(true);
						delay = 3;
					}
					break;
				}

				case 18:
					break;

				case 19:
				{
					AddrCtrl(true);
					break;
				}

				case 20:	// Start bad line
				{
					// In line $30, the DEN bit controls if Bad Lines can occur
					if (rasterY == firstDmaLine)
						badLinesEnabled = (ctrl1 & 0x10) != 0;

					// Test for bad line condition
					badLine = (rasterY >= firstDmaLine) && (rasterY <= lastDmaLine) && ((rasterY & 7) == yScroll) && badLinesEnabled;

					if (badLine)
					{
						// DMA starts on cycle 23
						AddrCtrl(false);
					}

					delay = 3;
					break;
				}

				case 23:
				{
					// 3.8.1-7
					for (int i = 0; i < 8; i++)
					{
						if ((spriteExpandY & (1 << i)) != 0)
							spriteMcBase[i] += 2;
					}
					break;
				}

				case 24:
				{
					byte mask = 1;

					for (int i = 0; i < 8; i++, mask <<= 1)
					{
						// 3.8.1-8
						if ((spriteExpandY & mask) != 0)
							spriteMcBase[i]++;

						if ((spriteMcBase[i] & 0x3f) == 0x3f)
							spriteDma &= (byte)~mask;
					}

					delay = 39;
					break;
				}

				case 63:	// End DMA - Only get here for non PAL
				{
					AddrCtrl(true);
					delay = (uint)xRasters - cycle;
					break;
				}

				default:
				{
					if (cycle < 23)
						delay = (uint)23 - cycle;
					else if (cycle < 63)
						delay = (uint)63 - cycle;
					else
						delay = (uint)xRasters - cycle;

					break;
				}
			}

			myEvent.Schedule(eventContext, delay - (uint)eventContext.Phase(), phase);
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Vic_II
{
	/// <summary>
	/// MOS 6567/6569/6572/6573 emulation.
	/// Not cycle exact but good enough for SID playback
	/// </summary>
	internal abstract class Mos656x
	{
		public enum model_t
		{
			/// <summary>
			/// OLD NTSC CHIP
			/// </summary>
			MOS6567R56A = 0,

			/// <summary>
			/// NTSC-M
			/// </summary>
			MOS6567R8,

			/// <summary>
			/// PAL-B
			/// </summary>
			MOS6569,

			/// <summary>
			/// PAL-N
			/// </summary>
			MOS6572,

			/// <summary>
			/// PAL-M
			/// </summary>
			MOS6573
		}

		/// <summary>
		/// Cycle # at which the VIC takes the bus in a bad line (BA goes low)
		/// </summary>
		private const int VICII_FETCH_CYCLE = 11;

		private const int VICII_SCREEN_TEXTCOLS = 40;

		private delegate event_clock_t ClockFunc();

		private class model_data_t
		{
			public model_data_t(uint rasterLines, uint cyclesPerLine, ClockFunc clock)
			{
				this.rasterLines = rasterLines;
				this.cyclesPerLine = cyclesPerLine;
				this.clock = clock;
			}

			public uint rasterLines;
			public uint cyclesPerLine;
			public ClockFunc clock;
		}

		private readonly model_data_t[] modelData;

		/// <summary>
		/// Raster IRQ flag
		/// </summary>
		private const int IRQ_RASTER = 1 << 0;

		/// <summary>
		/// Lightpen IRQ flag
		/// </summary>
		private const int IRQ_LIGHTPEN = 1 << 3;

		/// <summary>
		/// First line when we can check for bad lines
		/// </summary>
		private const uint FIRST_DMA_LINE = 0x30;

		/// <summary>
		/// Last line when we check for bad lines
		/// </summary>
		private const uint LAST_DMA_LINE = 0xf7;

		#region Private implementation of Event
		private class PrivateEvent : Event
		{
			private readonly Mos656x parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PrivateEvent(string name, Mos656x parent) : base(name)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				event_clock_t cycles = parent.eventScheduler.GetTime(parent.eventScheduler.Phase()) - parent.rasterClk;

				event_clock_t delay;

				if (cycles != 0)
				{
					// Update x raster
					parent.rasterClk += cycles;
					parent.lineCycle += (uint)cycles;
					parent.lineCycle %= parent.cyclesPerLine;

					delay = parent.clock();
				}
				else
					delay = 1;

				parent.eventScheduler.Schedule(this, (uint)(delay - (event_clock_t)parent.eventScheduler.Phase()), EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
			}
		}
		#endregion

		private readonly PrivateEvent eventObject;

		/// <summary>
		/// Current model clock function
		/// </summary>
		private ClockFunc clock;

		/// <summary>
		/// Current raster clock
		/// </summary>
		private event_clock_t rasterClk;

		/// <summary>
		/// System's event scheduler
		/// </summary>
		private readonly EventScheduler eventScheduler;

		/// <summary>
		/// Number of cycles per line
		/// </summary>
		private uint cyclesPerLine;

		/// <summary>
		/// Number of raster lines
		/// </summary>
		private uint maxRasters;

		/// <summary>
		/// Current visible line
		/// </summary>
		private uint lineCycle;

		/// <summary>
		/// Current raster line
		/// </summary>
		private uint rasterY;

		/// <summary>
		/// Vertical scrolling value
		/// </summary>
		private uint yScroll;

		/// <summary>
		/// Are bad lines enabled for this frame?
		/// </summary>
		private bool areBadLinesEnabled;

		/// <summary>
		/// Is the current line a bad line
		/// </summary>
		private bool isBadLine;

		/// <summary>
		/// Is rasterYIrq condition true?
		/// </summary>
		private bool rasterYIrqCondition;

		/// <summary>
		/// Set when new frame starts
		/// </summary>
		private bool vBlanking;

		/// <summary>
		/// Is CIA asserting lightpen?
		/// </summary>
		private bool lpAsserted;

		/// <summary>
		/// Internal IRQ flags
		/// </summary>
		private uint8_t irqFlags;

		/// <summary>
		/// Masks for the IRQ flags
		/// </summary>
		private uint8_t irqMask;

		/// <summary>
		/// Light pen
		/// </summary>
		private readonly Lightpen lp = new Lightpen();

		/// <summary>
		/// The 8 sprites data
		/// </summary>
		private readonly Sprites sprites;

		/// <summary>
		/// Memory for chip registers
		/// </summary>
		private readonly uint8_t[] regs = new uint8_t[0x40];

		private readonly EventCallback badLineStateChangeEvent;

		private readonly EventCallback rasterYIrqEdgeDetectorEvent;

		private readonly EventCallback lightpenTriggerEvent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Mos656x(EventScheduler scheduler)
		{
			modelData = new model_data_t[]
			{
				new model_data_t(262, 64, ClockOldNtsc),			// Old NTSC (MOS6567R56A)
				new model_data_t(263, 65, ClockNtsc),			// NTSC-M   (MOS6567R8)
				new model_data_t(312, 63, ClockPal),				// PAL-B    (MOS6569R1, MOS6569R3)
				new model_data_t(312, 65, ClockNtsc),			// PAL-N    (MOS6572)
				new model_data_t(263, 65, ClockNtsc),			// PAL-M    (MOS6573)
			};

			eventObject = new PrivateEvent("VIC Raster", this);
			eventScheduler = scheduler;
			sprites = new Sprites(regs);
			badLineStateChangeEvent = new EventCallback("Update AEC signal", BadLineStateChange);
			rasterYIrqEdgeDetectorEvent = new EventCallback("RasterY changed", RasterYIrqEdgeDetector);
			lightpenTriggerEvent = new EventCallback("Trigger lightpen", LightpenTrigger);

			Chip(model_t.MOS6569);
		}



		/********************************************************************/
		/// <summary>
		/// Reset VIC-II
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			irqFlags = 0;
			irqMask = 0;
			yScroll = 0;
			rasterY = maxRasters - 1;
			lineCycle = 0;
			areBadLinesEnabled = false;
			isBadLine = false;
			rasterYIrqCondition = false;
			rasterClk = 0;
			vBlanking = false;
			lpAsserted = false;

			Array.Clear(regs, 0, regs.Length);

			lp.Reset();
			sprites.Reset();

			eventScheduler.Cancel(eventObject);
			eventScheduler.Schedule(eventObject, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void Chip(model_t model)
		{
			int model_Idx = (int)model;

			maxRasters = modelData[model_Idx].rasterLines;
			cyclesPerLine = modelData[model_Idx].cyclesPerLine;
			clock = modelData[model_Idx].clock;

			lp.SetScreenSize(maxRasters, cyclesPerLine);

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Read VIC register
		/// </summary>
		/********************************************************************/
		protected uint8_t Read(uint_least8_t addr)
		{
			addr &= 0x3f;

			// Sync up timers
			Sync();

			switch (addr)
			{
				// Control register 1
				case 0x11:
					return (uint8_t)((uint8_t)(regs[addr] & 0x7f) | ((rasterY & 0x100) >> 1));

				// Raster counter
				case 0x12:
					return (uint8_t)(rasterY & 0xff);

				case 0x13:
					return lp.GetX();

				case 0x14:
					return lp.GetY();

				// Interrupt pending register
				case 0x19:
					return (uint8_t)(irqFlags | 0x70);

				// Interrupt mask register
				case 0x1a:
					return (uint8_t)(irqMask | 0xf0);

				default:
				{
					// For addresses < $20 read from register directly
					if (addr < 0x20)
						return regs[addr];

					// For addresses < $2f set bits of high nibble to 1
					if (addr < 0x2f)
						return (uint8_t)(regs[addr] | 0xf0);

					// For addresses >= 0x2f return $ff
					return 0xff;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write to VIC register
		/// </summary>
		/********************************************************************/
		protected void Write(uint_least8_t addr, uint8_t data)
		{
			addr &= 0x3f;

			regs[addr] = data;

			// Sync up timers
			Sync();

			switch (addr)
			{
				// Control register 1
				case 0x11:
				{
					uint oldYScroll = yScroll;
					yScroll = (uint)(data & 7);

					// This is the funniest part... handle bad line tricks
					bool wasBadLinesEnabled = areBadLinesEnabled;

					if ((rasterY == FIRST_DMA_LINE) && (lineCycle == 0))
						areBadLinesEnabled = ReadDen();

					if ((OldRasterY() == FIRST_DMA_LINE) && ReadDen())
						areBadLinesEnabled = true;

					if (((oldYScroll != yScroll) || (areBadLinesEnabled != wasBadLinesEnabled)) && (rasterY >= FIRST_DMA_LINE) && (rasterY <= LAST_DMA_LINE))
					{
						// Check whether bad line state has changed
						bool wasBadLine = wasBadLinesEnabled && (oldYScroll == (rasterY & 7));
						bool nowBadLine = areBadLinesEnabled && (yScroll == (rasterY & 7));

						if (nowBadLine != wasBadLine)
						{
							bool oldBadLine = isBadLine;

							if (wasBadLine)
							{
								if (lineCycle < VICII_FETCH_CYCLE)
									isBadLine = false;
							}
							else
							{
								// Bad line may be generated during fetch interval
								//   (VICII_FETCH_CYCLE <= lineCycle < VICII_FETCH_CYCLE + VICII_SCREEN_TEXTCOLS + 3)
								// or outside the fetch interval but before raster yCounter is incremented
								//   (lineCycle <= VICII_FETCH_CYCLE + VICII_SCREEN_TEXTCOLS + 6)
								if (lineCycle <= VICII_FETCH_CYCLE + VICII_SCREEN_TEXTCOLS + 6)
									isBadLine = true;
							}

							if (isBadLine != oldBadLine)
								eventScheduler.Schedule(badLineStateChangeEvent, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
						}
					}

					goto case 0x12;
				}

				// Raster counter
				case 0x12:
				{
					// Check raster Y IRQ condition changes at the next PHI1
					eventScheduler.Schedule(rasterYIrqEdgeDetectorEvent, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
					break;
				}

				case 0x17:
				{
					sprites.LineCrunch(data, lineCycle);
					break;
				}

				// VIC interrupt flag register
				case 0x19:
				{
					irqFlags &= (uint8_t)((~data & 0x0f) | 0x80);
					HandleIrqState();
					break;
				}

				// IRQ mask register
				case 0x1a:
				{
					irqMask = (uint8_t)(data & 0x0f);
					HandleIrqState();
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Trigger the lightpen. Sets the lightpen usage flag
		/// </summary>
		/********************************************************************/
		public void TriggerLightpen()
		{
			lpAsserted = true;

			eventScheduler.Schedule(lightpenTriggerEvent, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Clears the lightpen usage flag
		/// </summary>
		/********************************************************************/
		public void ClearLightpen()
		{
			lpAsserted = false;
		}

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
		protected abstract void SetBa(bool state);
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Signal CPU interrupt if requested by VIC
		/// </summary>
		/********************************************************************/
		private void HandleIrqState()
		{
			// Signal an IRQ unless we already signaled it
			if ((irqFlags & irqMask & 0x0f) != 0)
			{
				if ((irqFlags & 0x80) == 0)
				{
					Interrupt(true);
					irqFlags |= 0x80;
				}
			}
			else
			{
				if ((irqFlags & 0x80) != 0)
				{
					Interrupt(false);
					irqFlags &= 0x7f;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private event_clock_t ClockPal()
		{
			event_clock_t delay = 1;

			switch (lineCycle)
			{
				case 0:
				{
					CheckVBlank();
					EndDma(2);
					break;
				}

				case 1:
				{
					VBlank();
					StartDma(5);

					// No sprites before next compulsory cycle
					if (!sprites.IsDma(0xf8))
						delay = 10;

					break;
				}

				case 2:
				{
					EndDma(3);
					break;
				}

				case 3:
				{
					StartDma(6);
					break;
				}

				case 4:
				{
					EndDma(4);
					break;
				}

				case 5:
				{
					StartDma(7);
					break;
				}

				case 6:
				{
					EndDma(5);

					delay = sprites.IsDma(0xc0) ? 2 : 5;
					break;
				}

				case 7:
					break;

				case 8:
				{
					EndDma(6);

					delay = 2;
					break;
				}

				case 9:
					break;

				case 10:
				{
					EndDma7();
					break;
				}

				case 11:
				{
					StartBadLine();

					delay = 3;
					break;
				}

				case 12:
				{
					delay = 2;
					break;
				}

				case 13:
					break;

				case 14:
				{
					sprites.UpdateMc();
					break;
				}

				case 15:
				{
					sprites.UpdateMcBase();

					delay = 39;
					break;
				}

				case 54:
				{
					sprites.CheckDma(rasterY, regs);
					StartDma0();
					break;
				}

				case 55:
				{
					sprites.CheckDma(rasterY, regs);		// Phi1
					sprites.CheckExp();						// Phi2
					StartDma0();
					break;
				}

				case 56:
				{
					StartDma(1);
					break;
				}

				case 57:
				{
					sprites.CheckDisplay();

					// No sprites before next compulsory cycle
					if (!sprites.IsDma(0x1f))
						delay = 6;

					break;
				}

				case 58:
				{
					StartDma(2);
					break;
				}

				case 59:
				{
					EndDma(0);
					break;
				}

				case 60:
				{
					StartDma(3);
					break;
				}

				case 61:
				{
					EndDma(1);
					break;
				}

				case 62:
				{
					StartDma(4);
					break;
				}

				default:
				{
					delay = 54 - lineCycle;
					break;
				}
			}

			return delay;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private event_clock_t ClockNtsc()
		{
			event_clock_t delay = 1;

			switch (lineCycle)
			{
				case 0:
				{
					CheckVBlank();
					StartDma(5);
					break;
				}

				case 1:
				{
					VBlank();
					EndDma(3);

					// No sprites before next compulsory cycle
					if (!sprites.IsDma(0xf0))
						delay = 10;

					break;
				}

				case 2:
				{
					StartDma(6);
					break;
				}

				case 3:
				{
					EndDma(4);
					break;
				}

				case 4:
				{
					StartDma(7);
					break;
				}

				case 5:
				{
					EndDma(5);

					delay = sprites.IsDma(0xc0) ? 2 : 6;
					break;
				}

				case 6:
					break;

				case 7:
				{
					EndDma(6);

					delay = 2;
					break;
				}

				case 8:
					break;

				case 9:
				{
					EndDma7();

					delay = 2;
					break;
				}

				case 10:
					break;

				case 11:
				{
					StartBadLine();

					delay = 3;
					break;
				}

				case 12:
				{
					delay = 2;
					break;
				}

				case 13:
					break;

				case 14:
				{
					sprites.UpdateMc();
					break;
				}

				case 15:
				{
					sprites.UpdateMcBase();

					delay = 39;
					break;
				}

				case 54:
				{
					SetBa(true);
					break;
				}

				case 55:
				{
					sprites.CheckDma(rasterY, regs);		// Phi1
					sprites.CheckExp();						// Phi2
					StartDma0();
					break;
				}

				case 56:
				{
					sprites.CheckDma(rasterY, regs);
					StartDma0();
					break;
				}

				case 57:
				{
					StartDma(1);
					break;
				}

				case 58:
				{
					sprites.CheckDisplay();

					// No sprites before next compulsory cycle
					if (!sprites.IsDma(0x1f))
						delay = 7;

					break;
				}

				case 59:
				{
					StartDma(2);
					break;
				}

				case 60:
				{
					EndDma(0);
					break;
				}

				case 61:
				{
					StartDma(3);
					break;
				}

				case 62:
				{
					EndDma(1);
					break;
				}

				case 63:
				{
					StartDma(4);
					break;
				}

				case 64:
				{
					EndDma(2);
					break;
				}

				default:
				{
					delay = 54 - lineCycle;
					break;
				}
			}

			return delay;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private event_clock_t ClockOldNtsc()
		{
			event_clock_t delay = 1;

			switch (lineCycle)
			{
				case 0:
				{
					CheckVBlank();
					EndDma(2);
					break;
				}

				case 1:
				{
					VBlank();
					StartDma(5);

					// No sprites before next compulsory cycle
					if (!sprites.IsDma(0xf8))
						delay = 10;

					break;
				}

				case 2:
				{
					EndDma(3);
					break;
				}

				case 3:
				{
					StartDma(6);
					break;
				}

				case 4:
				{
					EndDma(4);
					break;
				}

				case 5:
				{
					StartDma(7);
					break;
				}

				case 6:
				{
					EndDma(5);

					delay = sprites.IsDma(0xc0) ? 2 : 5;
					break;
				}

				case 7:
					break;

				case 8:
				{
					EndDma(6);

					delay = 2;
					break;
				}

				case 9:
					break;

				case 10:
				{
					EndDma7();
					break;
				}

				case 11:
				{
					StartBadLine();

					delay = 3;
					break;
				}

				case 12:
				{
					delay = 2;
					break;
				}

				case 13:
					break;

				case 14:
				{
					sprites.UpdateMc();
					break;
				}

				case 15:
				{
					sprites.UpdateMcBase();

					delay = 39;
					break;
				}

				case 54:
				{
					SetBa(true);
					break;
				}

				case 55:
				{
					sprites.CheckDma(rasterY, regs);		// Phi1
					sprites.CheckExp();						// Phi2
					StartDma0();
					break;
				}

				case 56:
				{
					sprites.CheckDma(rasterY, regs);
					StartDma0();
					break;
				}

				case 57:
				{
					sprites.CheckDisplay();
					StartDma(1);

					// No sprites before next compulsory cycle
					delay = sprites.IsDma(0x1f) ? 2 : 7;
					break;
				}

				case 58:
					break;

				case 59:
				{
					StartDma(2);
					break;
				}

				case 60:
				{
					EndDma(0);
					break;
				}

				case 61:
				{
					StartDma(3);
					break;
				}

				case 62:
				{
					EndDma(1);
					break;
				}

				case 63:
				{
					StartDma(4);
					break;
				}

				default:
				{
					delay = 54 - lineCycle;
					break;
				}
			}

			return delay;
		}



		/********************************************************************/
		/// <summary>
		/// AEC state was updated
		/// </summary>
		/********************************************************************/
		private void BadLineStateChange()
		{
			SetBa(!isBadLine);
		}



		/********************************************************************/
		/// <summary>
		/// RasterY IRQ edge detector
		/// </summary>
		/********************************************************************/
		private void RasterYIrqEdgeDetector()
		{
			bool oldRasterYIrqCondition = rasterYIrqCondition;
			rasterYIrqCondition = rasterY == ReadRasterLineIrq();

			if (!oldRasterYIrqCondition && rasterYIrqCondition)
				ActivateIrqFlag(IRQ_RASTER);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LightpenTrigger()
		{
			// Synchronise simulation
			Sync();

			if (lp.Trigger(lineCycle, rasterY))
				ActivateIrqFlag(IRQ_LIGHTPEN);
		}



		/********************************************************************/
		/// <summary>
		/// Set an IRQ flag and trigger an IRQ if the corresponding IRQ mask
		/// is set. The IRQ gets activated, i.e. flag 0x80 gets set, if it
		/// was not active before
		/// </summary>
		/********************************************************************/
		private void ActivateIrqFlag(int flag)
		{
			irqFlags |= (byte)flag;
			HandleIrqState();
		}



		/********************************************************************/
		/// <summary>
		/// Read the value of the raster line IRQ.
		///
		/// Return the raster line when to trigger an IRQ
		/// </summary>
		/********************************************************************/
		private uint ReadRasterLineIrq()
		{
			return (uint)(regs[0x12] + ((regs[0x11] & 0x80) << 1));
		}



		/********************************************************************/
		/// <summary>
		/// Read the DEN flag which tells whether the display is enabled
		/// </summary>
		/********************************************************************/
		private bool ReadDen()
		{
			return (regs[0x11] & 0x10) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool EvaluateIsBadLine()
		{
			return areBadLinesEnabled && (rasterY >= FIRST_DMA_LINE) && (rasterY <= LAST_DMA_LINE) && ((rasterY & 7) == yScroll);
		}



		/********************************************************************/
		/// <summary>
		/// Get previous value of Y raster
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OldRasterY()
		{
			return (rasterY > 0 ? rasterY : maxRasters) - 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Sync()
		{
			eventScheduler.Cancel(eventObject);
			eventObject.DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Check for vertical blanking
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckVBlank()
		{
			// IRQ occurred (xraster != 0)
			if (rasterY == (maxRasters - 1))
				vBlanking = true;

			// Check DEN bit on first cycle of the line following the first DMA line
			if ((rasterY == FIRST_DMA_LINE) && !areBadLinesEnabled && ReadDen())
				areBadLinesEnabled = true;

			// Disallow bad lines after the last possible one has passed
			if (rasterY == LAST_DMA_LINE)
				areBadLinesEnabled = false;

			isBadLine = false;

			if (!vBlanking)
			{
				rasterY++;
				RasterYIrqEdgeDetector();

				if ((rasterY == FIRST_DMA_LINE) && !areBadLinesEnabled)
					areBadLinesEnabled = ReadDen();
			}

			if (EvaluateIsBadLine())
				isBadLine = true;
		}



		/********************************************************************/
		/// <summary>
		/// Vertical blank (line 0)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void VBlank()
		{
			if (vBlanking)
			{
				vBlanking = false;
				rasterY = 0;
				RasterYIrqEdgeDetector();
				lp.Untrigger();

				if (lpAsserted && lp.Retrigger())
					ActivateIrqFlag(IRQ_LIGHTPEN);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Start DMA for sprite n
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StartDma(int n)
		{
			if (sprites.IsDma((uint)0x01 << n))
				SetBa(false);
		}



		/********************************************************************/
		/// <summary>
		/// Start DMA for sprite 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StartDma0()
		{
			SetBa(!sprites.IsDma(0x01));
		}



		/********************************************************************/
		/// <summary>
		/// End DMA for sprite n
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EndDma(int n)
		{
			if (!sprites.IsDma((uint)0x06 << n))
				SetBa(true);
		}



		/********************************************************************/
		/// <summary>
		/// End DMA for sprite 7
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EndDma7()
		{
			SetBa(true);
		}



		/********************************************************************/
		/// <summary>
		/// Start bad line
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StartBadLine()
		{
			if (isBadLine)
				SetBa(false);
		}
		#endregion
	}
}

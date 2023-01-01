/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// This is the base class for the MOS6526 interrupt sources
	/// Based on Denise emu code
	/// </summary>
	internal abstract class InterruptSource
	{
		public const int INTERRUPT_NONE = 0;					// No interrupt
		public const int INTERRUPT_UNDERFLOW_A = 1 << 0;		// Underflow Timer A
		public const int INTERRUPT_UNDERFLOW_B = 1 << 1;		// Underflow Timer B
		public const int INTERRUPT_ALARM = 1 << 2;				// Alarm clock
		public const int INTERRUPT_SP = 1 << 3;					// Serial port
		public const int INTERRUPT_FLAG = 1 << 4;				// External flag
		public const int INTERRUPT_REQUEST = 1 << 7;			// Control bit

		/// <summary>
		/// Pointer to the MOS6526 which this Interrupt belongs to
		/// </summary>
		private readonly Mos652x parent;

		/// <summary>
		/// Event scheduler
		/// </summary>
		private readonly EventScheduler eventScheduler;

		// Clock when clear was called last
		private event_clock_t last_clear;
		private event_clock_t last_set;

		/// <summary>
		/// Interrupt control register
		/// </summary>
		private uint8_t icr;

		/// <summary>
		/// Interrupt data register
		/// </summary>
		protected uint8_t idr;

		protected uint8_t idrTemp;

		/// <summary>
		/// Have we already scheduled CIA->CPU interrupt transition?
		/// </summary>
		private bool scheduled;

		/// <summary>
		/// Is the irq pin asserted?
		/// </summary>
		private bool asserted;

		private readonly EventCallback interruptEvent;
		private readonly EventCallback updateIdrEvent;
		private readonly EventCallback setIrqEvent;
		private readonly EventCallback clearIrqEvent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected InterruptSource(EventScheduler scheduler, Mos652x parent)
		{
			this.parent = parent;
			eventScheduler = scheduler;
			last_clear = 0;
			last_set = 0;
			icr = 0;
			idr = 0;
			scheduled = false;
			asserted = false;

			interruptEvent = new EventCallback("CIA Interrupt", Interrupt);
			updateIdrEvent = new EventCallback("CIA update ICR", UpdateIdr);
			setIrqEvent = new EventCallback("CIA set IRQ", SetIrq);
			clearIrqEvent = new EventCallback("CIA clear IRQ", ClearIrq);
		}



		/********************************************************************/
		/// <summary>
		/// Clear pending interrupts, but do not signal to CPU we lost them.
		/// It is assumed that all components get reset() calls in
		/// synchronous manner
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			last_clear = 0;
			last_set = 0;

			icr = 0;
			idr = 0;

			eventScheduler.Cancel(updateIdrEvent);
			eventScheduler.Cancel(setIrqEvent);
			eventScheduler.Cancel(clearIrqEvent);
			eventScheduler.Cancel(interruptEvent);
			scheduled = false;

			asserted = false;
		}



		/********************************************************************/
		/// <summary>
		/// Set interrupt control mask bits
		/// </summary>
		/********************************************************************/
		public void Set(uint8_t interruptMask)
		{
			if ((interruptMask & INTERRUPT_REQUEST) != 0)
				icr |= (uint8_t)(interruptMask & ~INTERRUPT_REQUEST);
			else
				icr &= (uint8_t)~interruptMask;

			if (!Ack0())
				Trigger(INTERRUPT_NONE);

			last_set = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
		}



		/********************************************************************/
		/// <summary>
		/// Clear interrupt state
		/// </summary>
		/********************************************************************/
		public virtual uint8_t Clear()
		{
			last_clear = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);

			eventScheduler.Schedule(clearIrqEvent, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);

			if (!eventScheduler.IsPending(updateIdrEvent))
			{
				eventScheduler.Schedule(updateIdrEvent, 1, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
				idrTemp = 0;
			}

			return idr;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void TriggerInterrupt();



		/********************************************************************/
		/// <summary>
		/// Trigger an interrupt
		/// </summary>
		/********************************************************************/
		public abstract void Trigger(uint8_t interruptMask);
		#endregion

		/********************************************************************/
		/// <summary>
		/// Check if interrupts were acknowledged during previous cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Ack0()
		{
			return eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2) == (last_clear + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Schedules an IRQ asserting state transition for next cycle
		/// </summary>
		/********************************************************************/
		protected void Schedule(int delay)
		{
			if (!scheduled)
			{
				eventScheduler.Schedule(interruptEvent, (uint)delay, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
				scheduled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void ScheduleIrq()
		{
			eventScheduler.Schedule(setIrqEvent, 1, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool IsTriggered(uint8_t interruptMask)
		{
			idr |= interruptMask;
			idrTemp |= interruptMask;

			if (InterruptMasked(interruptMask))
				return true;

			if ((interruptMask == INTERRUPT_NONE) && Write0())
			{
				// Cancel pending interrupts
				if (scheduled)
				{
					eventScheduler.Cancel(interruptEvent);
					scheduled = false;
				}
			}

			return false;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Signal interrupt to CPU
		/// </summary>
		/********************************************************************/
		private void Interrupt()
		{
			if (!InterruptTriggered())
			{
				TriggerInterrupt();
				SetIrq();
			}

			scheduled = false;
		}



		/********************************************************************/
		/// <summary>
		/// Signal interrupt to CPU
		/// </summary>
		/********************************************************************/
		private void UpdateIdr()
		{
			idr = idrTemp;

			if (Ack0())
			{
				eventScheduler.Schedule(updateIdrEvent, 1, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
				idrTemp = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Signal interrupt to CPU
		/// </summary>
		/********************************************************************/
		private void SetIrq()
		{
			if (!Ack0())
			{
				if (!asserted)
				{
					parent.Interrupt(true);
					asserted = true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Signal interrupt to CPU
		/// </summary>
		/********************************************************************/
		private void ClearIrq()
		{
			if (asserted)
			{
				parent.Interrupt(false);
				asserted = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool InterruptTriggered()
		{
			return (idr & INTERRUPT_REQUEST) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool InterruptMasked(uint8_t interruptMask)
		{
			return (((interruptMask != INTERRUPT_NONE) ? interruptMask : idr) & icr) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check if interrupts were acknowledged during previous cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Write0()
		{
			return eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2) == (last_set + 1);
		}
		#endregion
	}
}

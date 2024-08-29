/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// This is the base class for the MOS6526 timers
	/// </summary>
	internal abstract class Timer
	{
		protected const int_least32_t CIAT_CR_START = 0x01;
		protected const int_least32_t CIAT_STEP = 0x04;
		private const int_least32_t CIAT_CR_ONESHOT = 0x08;
		private const int_least32_t CIAT_CR_FLOAD = 0x10;
		private const int_least32_t CIAT_PHI2IN = 0x20;
		private const int_least32_t CIAT_CR_MASK = CIAT_CR_START | CIAT_CR_ONESHOT | CIAT_CR_FLOAD | CIAT_PHI2IN;

		private const int_least32_t CIAT_COUNT2 = 0x100;
		private const int_least32_t CIAT_COUNT3 = 0x200;

		private const int_least32_t CIAT_ONESHOT0 = 0x08 << 8;
		private const int_least32_t CIAT_ONESHOT = 0x08 << 16;
		private const int_least32_t CIAT_LOAD1 = 0x10 << 8;
		private const int_least32_t CIAT_LOAD = 0x10 << 16;

		private const int_least32_t CIAT_OUT = unchecked((int_least32_t)0x80000000);

		#region Private implementation of Event
		private class PrivateEvent : Event
		{
			private readonly Timer parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PrivateEvent(string name, Timer parent) : base(name)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// Timer ticking event
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				parent.Clock();
				parent.Reschedule();
			}
		}
		#endregion

		private readonly PrivateEvent eventObject;

		private readonly EventCallback cycleSkippingEvent;

		/// <summary>
		/// Event context
		/// </summary>
		private readonly EventScheduler eventScheduler;

		/// <summary>
		/// This is a tri-state:
		///
		/// - when -1: CIA is completely stopped
		/// - when 0: CIA 1-clock events are ticking
		/// - otherwise: cycle skip event is ticking, and the value is the first
		///   phi1 clock of skipping
		/// </summary>
		private event_clock_t ciaEventPauseTime;

		/// <summary>
		/// PB6/PB7 Flipflop to signal underflows
		/// </summary>
		private bool pbToggle = false;

		/// <summary>
		/// Current timer value
		/// </summary>
		private uint_least16_t timer = 0;

		/// <summary>
		/// Timer start value (Latch)
		/// </summary>
		private uint_least16_t latch = 0;

		/// <summary>
		/// Copy of regs[CRA/B]
		/// </summary>
		private uint8_t lastControlValue = 0;

		/// <summary>
		/// Pointer to the MOS6526 which this Timer belongs to
		/// </summary>
		protected readonly Mos652x parent;

		/// <summary>
		/// CRA/CRB control register / state
		/// </summary>
		protected int_least32_t state = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Timer(string name, EventScheduler scheduler, Mos652x parent)
		{
			eventObject = new PrivateEvent(name, this);
			cycleSkippingEvent = new EventCallback("Skip CIA clock decrement cycles", CycleSkippingEvent);
			eventScheduler = scheduler;
			this.parent = parent;
		}



		/********************************************************************/
		/// <summary>
		/// Set CRA/CRB control register
		/// </summary>
		/********************************************************************/
		public void SetControlRegister(uint8_t cr)
		{
			state &= ~CIAT_CR_MASK;
			state |= (cr & CIAT_CR_MASK) ^ CIAT_PHI2IN;
			lastControlValue = cr;
		}



		/********************************************************************/
		/// <summary>
		/// Perform cycle skipping manually
		///
		/// Clocks the CIA up to the state it should be in, and stops all
		/// events
		/// </summary>
		/********************************************************************/
		public void SyncWithCpu()
		{
			if (ciaEventPauseTime > 0)
			{
				eventScheduler.Cancel(cycleSkippingEvent);
				event_clock_t elapsed = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2) - ciaEventPauseTime;

				// It's possible for CIA to determine that it wants to go to sleep starting from the next
				// cycle, and then save its plans aborted by CPU. Thus, we must avoid modifying
				// the CIA state if the first sleep clock was still in the future
				if (elapsed >= 0)
				{
					timer -= (uint_least16_t)elapsed;
					Clock();
				}
			}

			if (ciaEventPauseTime == 0)
				eventScheduler.Cancel(eventObject);

			ciaEventPauseTime = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Counterpart of SyncWithCpu(), starts the event ticking if it is
		/// needed. No Clock() call or anything such is permissible here!
		/// </summary>
		/********************************************************************/
		public void WakeUpAfterSyncWithCpu()
		{
			ciaEventPauseTime = 0;
			eventScheduler.Schedule(eventObject, 0, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// Reset timer
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			eventScheduler.Cancel(eventObject);
			timer = latch = 0xffff;
			pbToggle = false;
			state = 0;
			lastControlValue = 0;
			ciaEventPauseTime = 0;
			eventScheduler.Schedule(eventObject, 1, EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// Set low byte of timer start value (Latch)
		/// </summary>
		/********************************************************************/
		public void LatchLo(uint8_t data)
		{
			SidEndian.Endian_16Lo8(ref latch, data);
			if ((state & CIAT_LOAD) != 0)
				timer = latch;
		}



		/********************************************************************/
		/// <summary>
		/// Set high byte of timer start value (Latch)
		/// </summary>
		/********************************************************************/
		public void LatchHi(uint8_t data)
		{
			SidEndian.Endian_16Hi8(ref latch, data);
			if ((state & CIAT_LOAD) != 0)
				timer = latch;
			else if ((state & CIAT_CR_START) == 0)
			{
				// Reload timer if stopped
				state |= CIAT_LOAD1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set PB6/PB7 flipflop state
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPbToggle(bool state)
		{
			pbToggle = state;
		}



		/********************************************************************/
		/// <summary>
		/// Get current state value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int_least32_t GetState()
		{
			return state;
		}



		/********************************************************************/
		/// <summary>
		/// Get current timer value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint_least16_t GetTimer()
		{
			return timer;
		}



		/********************************************************************/
		/// <summary>
		/// Get PB6/PB7 flipflop state
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetPb(uint8_t reg)
		{
			return (reg & 0x04) != 0 ? pbToggle : (state & CIAT_OUT) != 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal timer underflow
		/// </summary>
		/********************************************************************/
		protected abstract void Underflow();



		/********************************************************************/
		/// <summary>
		/// Handle the serial port
		/// </summary>
		/********************************************************************/
		protected virtual void SerialPort()
		{
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Perform scheduled cycle skipping, and resume
		/// </summary>
		/********************************************************************/
		private void CycleSkippingEvent()
		{
			event_clock_t elapsed = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1) - ciaEventPauseTime;
			ciaEventPauseTime = 0;
			timer -= (uint_least16_t)elapsed;
			eventObject.DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Execute one CIA state transition
		/// </summary>
		/********************************************************************/
		private void Clock()
		{
			if ((state & CIAT_COUNT3) != 0)
				timer--;

			// ciatimer.c block start
			int_least32_t adj = state & (CIAT_CR_START | CIAT_CR_ONESHOT | CIAT_PHI2IN);

			if ((state & (CIAT_CR_START | CIAT_PHI2IN)) == (CIAT_CR_START | CIAT_PHI2IN))
				adj |= CIAT_COUNT2;

			if (((state & CIAT_COUNT2) != 0) || ((state & (CIAT_STEP | CIAT_CR_START)) == (CIAT_STEP | CIAT_CR_START)))
				adj |= CIAT_COUNT3;

			// CR_FLOAD -> LOAD1, CR_ONESHOT -> ONESHOT0, LOAD1 -> LOAD, ONESHOT0 -> ONESHOT
			adj |= (state & (CIAT_CR_FLOAD | CIAT_CR_ONESHOT | CIAT_LOAD1 | CIAT_ONESHOT0)) << 8;
			state = adj;
			// ciatimer.c block end

			if ((timer == 0) && ((state & CIAT_COUNT3) != 0))
			{
				state |= CIAT_LOAD | CIAT_OUT;

				if ((state & (CIAT_ONESHOT | CIAT_ONESHOT0)) != 0)
					state &= ~(CIAT_CR_START | CIAT_COUNT2);

				// By setting bits 2&3 of the control register,
				// PB6/PB7 will be toggled between high and low at each underflow
				bool toogle = (lastControlValue & 0x06) == 6;
				pbToggle = toogle && !pbToggle;

				// Implementation of the serial port
				SerialPort();

				// Timer A signals underflow handling: IRQ/B-count
				Underflow();
			}

			if ((state & CIAT_LOAD) != 0)
			{
				timer = latch;
				state &= ~CIAT_COUNT3;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reschedule CIA event at the earliest interesting time.
		/// If CIA timer is stopped or is programmed to just count down,
		/// the events are paused
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reschedule()
		{
			// There are only two subcases to consider.
			//
			// - are we counting, and if so, are we going to
			//   continue counting?
			// - have we stopped, and are there no conditions to force a new beginning?
			//
			// Additionally, there are numerous flags that are present only in passing manner,
			// but which we need to let cycle through the CIA state machine
			int_least32_t unwanted = CIAT_OUT | CIAT_CR_FLOAD | CIAT_LOAD1 | CIAT_LOAD;
			if ((state & unwanted) != 0)
			{
				eventScheduler.Schedule(eventObject, 1);
				return;
			}

			if ((state & CIAT_COUNT3) != 0)
			{
				// Test the conditions that keep COUNT2 and thus COUNT3 alive, and also
				// ensure that all of them are set indicating steady state operation
				int_least32_t wanted = CIAT_CR_START | CIAT_PHI2IN | CIAT_COUNT2 | CIAT_COUNT3;
				if ((timer > 2) && ((state & wanted) == wanted))
				{
					// We executed this cycle, therefore the pauseTime is +1. If we are called
					// to execute on the very next clock, we need to get 0 because there's
					// another timer-- in it
					ciaEventPauseTime = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1) + 1;

					// Execute event slightly before the next underflow
					eventScheduler.Schedule(cycleSkippingEvent, (uint)timer - 1);
					return;
				}

				// Play safe, keep on ticking
				eventScheduler.Schedule(eventObject, 1);
			}
			else
			{
				// Test conditions that result in CIA activity in next clocks.
				// If none, stop
				int_least32_t unwanted1 = CIAT_CR_START | CIAT_PHI2IN;
				int_least32_t unwanted2 = CIAT_CR_START | CIAT_STEP;

				if (((state & unwanted1) == unwanted1) || ((state & unwanted2) == unwanted2))
				{
					eventScheduler.Schedule(eventObject, 1);
					return;
				}

				ciaEventPauseTime = -1;
			}
		}
		#endregion
	}
}

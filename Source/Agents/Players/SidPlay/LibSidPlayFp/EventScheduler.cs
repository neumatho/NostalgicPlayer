/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp
{
	/// <summary>
	/// Fast EventScheduler, which maintains a linked list of Events.
	/// This scheduler takes negligible time even when it is used to
	/// schedule events for nearly every clock.
	///
	/// Events occur on an internal clock which is 2x the visible clock.
	/// The visible clock is divided to two phases called phi1 and phi2.
	///
	/// The phi1 clocks are used by VIC and CIA chips, phi2 clocks by CPU.
	///
	/// Scheduling an event for a phi1 clock when system is in phi2 causes the
	/// event to be moved to the next phi1 cycle. Correspondingly, requesting
	/// a phi1 time when system is in phi2 returns the value of the next phi1
	/// </summary>
	internal class EventScheduler
	{
		/// <summary>
		/// C64 system runs actions at system clock high and low
		/// states. The PHI1 corresponds to the auxiliary chip activity
		/// and PHI2 to CPU activity. For any clock, PHI1s are before
		/// PHI2s
		/// </summary>
		public enum event_phase_t
		{
			EVENT_CLOCK_PHI1 = 0,
			EVENT_CLOCK_PHI2 = 1
		}

		/// <summary>
		/// The first event of the chain
		/// </summary>
		private Event firstEvent;

		/// <summary>
		/// EventScheduler's current clock
		/// </summary>
		private event_clock_t currentTime;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EventScheduler()
		{
			firstEvent = null;
			currentTime = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Add event to pending queue
		///
		/// At PHI2, specify cycles=0 and Phase=PHI1 to fire on the very
		/// next PHI1
		/// </summary>
		/********************************************************************/
		public void Schedule(Event @event, uint cycles, event_phase_t phase)
		{
			// This strange formulation always selects the next available slot regardless of specified phase
			@event.triggerTime = currentTime + ((currentTime & 1) ^ (int)phase) + (cycles << 1);
			Schedule(@event);
		}



		/********************************************************************/
		/// <summary>
		/// Add event to pending queue in the same phase as current event
		/// </summary>
		/********************************************************************/
		public void Schedule(Event @event, uint cycles)
		{
			@event.triggerTime = currentTime + (cycles << 1);
			Schedule(@event);
		}



		/********************************************************************/
		/// <summary>
		/// Cancel all pending events and reset time
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			firstEvent = null;
			currentTime = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Cancel event if pending
		/// </summary>
		/********************************************************************/
		public void Cancel(Event @event)
		{
			ref Event scan = ref firstEvent;

			while (scan != null)
			{
				if (@event == scan)
				{
					scan = scan.next;
					break;
				}

				scan = ref scan.next;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fire next event, advance system time to that event
		/// </summary>
		/********************************************************************/
		public void Clock()
		{
			Event @event = firstEvent;
			firstEvent = firstEvent.next;
			currentTime = @event.triggerTime;
			@event.DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Check if an event is in the queue
		/// </summary>
		/********************************************************************/
		public bool IsPending(Event @event)
		{
			ref Event scan = ref firstEvent;

			while (scan != null)
			{
				if (@event == scan)
					return true;

				scan = ref scan.next;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		/********************************************************************/
		public event_clock_t GetTime(event_phase_t phase)
		{
			return (currentTime + ((int)phase ^ 1)) >> 1;
		}



		/********************************************************************/
		/// <summary>
		/// Return current clock phase
		/// </summary>
		/********************************************************************/
		public event_phase_t Phase()
		{
			return (event_phase_t)(currentTime & 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event_clock_t Remaining(Event @event)
		{
			return @event.triggerTime - currentTime;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Scan the event queue and schedule event for execution
		/// </summary>
		/********************************************************************/
		private void Schedule(Event @event)
		{
			// Find the right spot where to tuck this new event
			ref Event scan = ref firstEvent;
			for (;;)
			{
				if ((scan == null) || (scan.triggerTime > @event.triggerTime))
				{
					@event.next = scan;
					scan = @event;
					break;
				}

				scan = ref scan.next;
			}
		}
		#endregion
	}
}

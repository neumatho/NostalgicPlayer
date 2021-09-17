/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event
{
	/// <summary>
	/// Event scheduler (based on alarm from Vice)
	/// </summary>
	internal class EventScheduler : Event, IEventContext
	{
		#region EventContext class implementation
		private class MyEventContext : EventContext
		{
			private readonly EventScheduler parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyEventContext(EventScheduler parent)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// Add event to ordered pending queue
			/// </summary>
			/********************************************************************/
			public override void Schedule(Event @event, uint cycles, EventPhase phase)
			{
				for (;;)
				{
					if (!@event.pending)
					{
						uint clk = parent.clk + (cycles << 1);
						clk += ((clk & 1) ^ (uint)phase);

						// Now put in the correct place so we don't need to keep
						// searching the list later
						Event e;
						uint count;

						if (clk >= parent.clk)
						{
							e = parent.next;
							count = parent.events++;
						}
						else
						{
							e = parent.timeWarp.next;
							count = parent.eventsFuture++;
						}

						while ((count != 0) && (e.clk <= clk))
						{
							count--;
							e = e.next;
						}

						@event.next = e;
						@event.prev = e.prev;
						e.prev.next = @event;
						e.prev = @event;
						@event.pending = true;
						@event.clk = clk;
						@event.context = this;
						break;
					}

					@event.Cancel();
				}
			}



			/********************************************************************/
			/// <summary>
			/// Cancel the event
			/// </summary>
			/********************************************************************/
			public override void Cancel(Event @event)
			{
				@event.pending = false;
				@event.prev.next = @event.next;
				@event.next.prev = @event.prev;
				parent.events--;
			}



			/********************************************************************/
			/// <summary>
			/// Get time with respect to a specific clock phase
			/// </summary>
			/********************************************************************/
			public override uint GetTime(EventPhase phase)
			{
				return (uint)((parent.clk + ((int)phase ^ 1)) >> 1);
			}



			/********************************************************************/
			/// <summary>
			/// Get time with respect to a specific clock phase
			/// </summary>
			/********************************************************************/
			public override uint GetTime(uint clock, EventPhase phase)
			{
				return ((GetTime(phase) - clock) << 1) >> 1;	// 31 bit resolution
			}



			/********************************************************************/
			/// <summary>
			/// Get time with respect to a specific clock phase
			/// </summary>
			/********************************************************************/
			public override EventPhase Phase()
			{
				return (EventPhase)(parent.clk & 1);
			}
		}
		#endregion

		private readonly MyEventContext myEventContext;

		private EventCallback timeWarp;
		private uint events;
		private uint eventsFuture;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EventScheduler(string name) : base(name)
		{
			myEventContext = new MyEventContext(this);

			timeWarp = new EventCallback("Time Warp", DoEvent);
			events = 0;
			eventsFuture = 0;

			next = this;
			prev = this;
			timeWarp.clk = 0;
			Reset();
		}

		#region IEventContext implementation
		/********************************************************************/
		/// <summary>
		/// Schedule event
		/// </summary>
		/********************************************************************/
		public void Schedule(Event @event, uint cycles, EventPhase phase)
		{
			myEventContext.Schedule(@event, cycles, phase);
		}



		/********************************************************************/
		/// <summary>
		/// Cancel the event
		/// </summary>
		/********************************************************************/
		public void Cancel(Event @event)
		{
			myEventContext.Cancel(@event);
		}



		/********************************************************************/
		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		/********************************************************************/
		public uint GetTime(EventPhase phase)
		{
			return myEventContext.GetTime(phase);
		}



		/********************************************************************/
		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		/********************************************************************/
		public uint GetTime(uint clock, EventPhase phase)
		{
			return myEventContext.GetTime(clock, phase);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public EventPhase Phase()
		{
			return myEventContext.Phase();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Reset the scheduler
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Remove all events
			Event e = next;
			uint count = events;
			pending = false;

			while (e.pending)
			{
				e.pending = false;
				e = e.next;
			}

			next = this;
			prev = this;
			clk = 0;
			events = 0;
			eventsFuture = 0;
			DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Reset the scheduler
		/// </summary>
		/********************************************************************/
		public void Clock()
		{
			Event e = next;
			clk = e.clk;
			Cancel(e);
			e.DoEvent();
		}



		/********************************************************************/
		/// <summary>
		/// Used to prevent overflowing by time warping the event clocks
		/// </summary>
		/********************************************************************/
		public override void DoEvent()
		{
			events = eventsFuture;
			eventsFuture = 0;
			timeWarp.next = this;
			timeWarp.prev = this;
			prev.next = timeWarp;
			prev = timeWarp;
			timeWarp.pending = true;
		}
	}
}

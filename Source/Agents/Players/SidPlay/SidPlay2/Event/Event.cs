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
	/// 
	/// </summary>
	internal abstract class Event : IEvent
	{
		private readonly string name;

		/// <summary></summary>
		public EventContext context;

		/// <summary></summary>
		public uint clk;

		/// <summary>
		/// This variable is set by the event context
		/// when it is scheduled
		/// </summary>
		public bool pending;

		/// <summary>
		/// Link to the next and previous events in the list
		/// </summary>
		public Event next;
		/// <summary></summary>
		public Event prev;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Event(string name)
		{
			this.name = name;
			pending = false;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		public abstract void DoEvent();



		/********************************************************************/
		/// <summary>
		/// Tells if an event is pending
		/// </summary>
		/********************************************************************/
		public bool Pending()
		{
			return pending;
		}



		/********************************************************************/
		/// <summary>
		/// Cancel the event
		/// </summary>
		/********************************************************************/
		public void Cancel()
		{
			if (pending)
				context.Cancel(this);
		}



		/********************************************************************/
		/// <summary>
		/// Schedule event
		/// </summary>
		/********************************************************************/
		public void Schedule(IEventContext context, uint cycles, EventPhase phase)
		{
			context.Schedule(this, cycles, phase);
		}
	}
}

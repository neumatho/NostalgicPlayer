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
	/// Public event context
	/// </summary>
	internal abstract class EventContext : IEventContext
	{
		/********************************************************************/
		/// <summary>
		/// Schedule event
		/// </summary>
		/********************************************************************/
		public abstract void Schedule(Event @event, uint cycles, EventPhase phase);



		/********************************************************************/
		/// <summary>
		/// Cancel the event
		/// </summary>
		/********************************************************************/
		public abstract void Cancel(Event @event);



		/********************************************************************/
		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		/********************************************************************/
		public abstract uint GetTime(EventPhase phase);



		/********************************************************************/
		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		/********************************************************************/
		public abstract uint GetTime(uint clock, EventPhase phase);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract EventPhase Phase();
	}
}

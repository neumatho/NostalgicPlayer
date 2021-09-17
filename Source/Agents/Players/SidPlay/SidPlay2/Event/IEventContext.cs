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
	/// Event context
	/// </summary>
	internal interface IEventContext
	{
		/// <summary>
		/// Schedule event
		/// </summary>
		void Schedule(Event @event, uint cycles, EventPhase phase);

		/// <summary>
		/// Cancel the event
		/// </summary>
		void Cancel(Event @event);

		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		uint GetTime(EventPhase phase);

		/// <summary>
		/// Get time with respect to a specific clock phase
		/// </summary>
		uint GetTime(uint clock, EventPhase phase);

		/// <summary>
		/// 
		/// </summary>
		EventPhase Phase();
	}
}

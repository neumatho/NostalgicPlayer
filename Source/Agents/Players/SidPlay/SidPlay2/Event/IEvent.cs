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
	/// Interface for Event class. Needed to simulate multiple inheritances
	/// </summary>
	internal interface IEvent
	{
		/// <summary>
		/// Handle the event
		/// </summary>
		void DoEvent();

		/// <summary>
		/// Schedule event
		/// </summary>
		void Schedule(IEventContext context, uint cycles, EventPhase phase);

		/// <summary>
		/// Cancel the event
		/// </summary>
		void Cancel();
	}
}

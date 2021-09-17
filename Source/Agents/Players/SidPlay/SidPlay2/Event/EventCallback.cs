/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event
{
	/// <summary>
	/// Callback event
	/// </summary>
	internal class EventCallback : Event
	{
		private Action callback;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EventCallback(string name, Action callback) : base(name)
		{
			this.callback = callback;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		public override void DoEvent()
		{
			callback();
		}
	}
}

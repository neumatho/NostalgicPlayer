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
	/// Callback event
	/// </summary>
	internal class EventCallback : Event
	{
		public delegate void Callback();

		private readonly Callback callback;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EventCallback(string name, Callback callback) : base(name)
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

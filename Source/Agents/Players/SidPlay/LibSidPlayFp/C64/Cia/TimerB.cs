/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// This is the timer B of this CIA
	///
	/// Author: Ken Händel
	/// </summary>
	internal sealed class TimerB : Timer
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TimerB(EventScheduler scheduler, Mos652x parent) : base("CIA Timer B", scheduler, parent)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Receive an underflow from Timer A
		/// </summary>
		/********************************************************************/
		public void Cascade()
		{
			// We pretend that we are CPU doing a write to ctrl register
			SyncWithCpu();
			state |= CIAT_STEP;
			WakeUpAfterSyncWithCpu();
		}



		/********************************************************************/
		/// <summary>
		/// Check if start flag is set
		/// </summary>
		/********************************************************************/
		public bool Started()
		{
			return (state & CIAT_CR_START) != 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal timer underflow
		/// </summary>
		/********************************************************************/
		protected override void Underflow()
		{
			parent.UnderflowB();
		}
		#endregion
	}
}

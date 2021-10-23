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
	/// InterruptSource that acts like new CIA
	/// </summary>
	internal sealed class InterruptSource8521 : InterruptSource
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InterruptSource8521(EventScheduler scheduler, Mos652x parent) : base(scheduler, parent)
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void TriggerInterrupt()
		{
			idr |= INTERRUPT_REQUEST;
			idrTemp |= INTERRUPT_REQUEST;

			if (Ack0())
				ScheduleIrq();
		}



		/********************************************************************/
		/// <summary>
		/// Trigger an interrupt
		/// </summary>
		/********************************************************************/
		public override void Trigger(uint8_t interruptMask)
		{
			if (IsTriggered(interruptMask))
				Schedule(0);
		}
		#endregion
	}
}

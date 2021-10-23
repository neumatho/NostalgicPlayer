/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// An implementation of this class can be created to perform the C64
	/// specifics. A pointer to this child class can then be passed to
	/// each of the components so they can interact with it
	/// </summary>
	internal abstract class C64Env
	{
		private readonly EventScheduler eventScheduler;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected C64Env(EventScheduler scheduler)
		{
			eventScheduler = scheduler;
		}



		/********************************************************************/
		/// <summary>
		/// Return the event scheduler
		/// </summary>
		/********************************************************************/
		public EventScheduler Scheduler()
		{
			return eventScheduler;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract uint8_t CpuRead(uint_least16_t addr);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void CpuWrite(uint_least16_t addr, uint8_t data);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void InterruptIrq(bool state);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void InterruptNmi();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void SetBa(bool state);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Lightpen(bool state);
		#endregion
	}
}

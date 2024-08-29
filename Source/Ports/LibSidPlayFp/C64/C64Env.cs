/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64
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

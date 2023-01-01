/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp
{
	/// <summary>
	/// An Event object that can be inserted in the Event Scheduler
	/// </summary>
	internal abstract class Event
	{
		/// <summary>
		/// The next event in sequence
		/// </summary>
		public Event next;

		/// <summary>
		/// The clock this event fires
		/// </summary>
		public event_clock_t triggerTime;

		/// <summary>
		/// Describe event for humans
		/// </summary>
		private readonly string name;

		/********************************************************************/
		/// <summary>
		/// Constructor
		///
		/// Events are used for delayed execution. Name is not used by code,
		/// but is useful for debugging
		/// </summary>
		/********************************************************************/
		protected Event(string name)
		{
			this.name = name;
		}



		/********************************************************************/
		/// <summary>
		/// Event code to be executed. Events are allowed to safely
		/// reschedule themselves with the EventScheduler during invocations
		/// </summary>
		/********************************************************************/
		public abstract void DoEvent();
	}
}

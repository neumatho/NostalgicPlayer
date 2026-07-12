/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Library.Sound.Timer.Events
{
	/// <summary>
	/// Event for player events
	/// </summary>
	internal class PlayerEvent : ITimedEvent
	{
		private readonly SoundBase soundBase;
		private readonly EventArgs ev;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerEvent(SoundBase soundBase, EventArgs e)
		{
			this.soundBase = soundBase;
			ev = e;
		}



		/********************************************************************/
		/// <summary>
		/// Do whatever this event want to do
		/// </summary>
		/********************************************************************/
		public void Execute(int differenceTime)
		{
		}
	}
}

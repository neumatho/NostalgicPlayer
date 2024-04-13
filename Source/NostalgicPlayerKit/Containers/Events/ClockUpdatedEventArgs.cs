/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Events
{
	/// <summary>
	/// </summary>
	public delegate void ClockUpdatedEventHandler(object sender, ClockUpdatedEventArgs e);

	/// <summary>
	/// Event class holding needed information when sending a clock update event
	/// </summary>
	public class ClockUpdatedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ClockUpdatedEventArgs(TimeSpan time)
		{
			Time = time;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the current time
		/// </summary>
		/********************************************************************/
		public TimeSpan Time
		{
			get;
		}
	}
}

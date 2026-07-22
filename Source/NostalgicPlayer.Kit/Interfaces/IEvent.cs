/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Player provides events
	/// </summary>
	public interface IEvent
	{
		/// <summary>
		/// Return all events that needs to be triggered from the player
		/// </summary>
		EventArgs[] GetTriggeredEvents();
	}
}

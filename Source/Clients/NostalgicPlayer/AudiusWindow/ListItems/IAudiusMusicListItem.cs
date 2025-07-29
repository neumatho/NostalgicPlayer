﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	/// <summary>
	/// All the different controls that can be added as a list item,
	/// should implement this interface
	/// </summary>
	public interface IAudiusMusicListItem : IAudiusListItem
	{
		/// <summary>
		/// Event called when to play tracks
		/// </summary>
		public event TrackEventHandler PlayTracks;

		/// <summary>
		/// Event called when to add tracks
		/// </summary>
		public event TrackEventHandler AddTracks;
	}
}

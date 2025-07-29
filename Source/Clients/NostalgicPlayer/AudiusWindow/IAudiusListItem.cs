﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// All the different controls that can be added as a list item,
	/// should implement this interface
	/// </summary>
	public interface IAudiusListItem
	{
		/// <summary>
		/// Return the control itself
		/// </summary>
		Control Control { get; }

		/// <summary>
		/// Will initialize the control
		/// </summary>
		void Initialize(AudiusListItem listItem);

		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		void RefreshItem(PictureDownloader pictureDownloader);

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

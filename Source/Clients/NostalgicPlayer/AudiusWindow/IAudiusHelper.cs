/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.External.Audius.Models.Playlists;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Different helper methods for Audius
	/// </summary>
	public interface IAudiusHelper
	{
		/// <summary>
		/// Show error message
		/// </summary>
		void ShowErrorMessage(Exception ex, AudiusListControl audiusListControl, IAudiusWindowApi audiusWindowApi);

		/// <summary>
		/// Get tracks for all playlists
		/// </summary>
		List<AudiusListItem> FillPlaylistsWithTracks(PlaylistModel[] playlists, CancellationToken cancellationToken);
	}
}

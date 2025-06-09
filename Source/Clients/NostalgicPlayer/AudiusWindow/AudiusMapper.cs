/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Different mappers for Audius
	/// </summary>
	public static class AudiusMapper
	{
		/********************************************************************/
		/// <summary>
		/// Map from a track to item
		/// </summary>
		/********************************************************************/
		public static AudiusListItem MapTrackToItem(TrackModel track, int position)
		{
			return new AudiusListItem(
				position,
				track.Title,
				track.User.Name,
				track.Duration.HasValue ? TimeSpan.FromSeconds(track.Duration.Value) : TimeSpan.Zero,
				track.RepostCount,
				track.FavoriteCount,
				track.PlayCount,
				track.Artwork?._150x150
			);
		}
	}
}

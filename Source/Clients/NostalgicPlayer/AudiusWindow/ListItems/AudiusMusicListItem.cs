/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	/// <summary>
	/// This class is used for each item in an Audius list
	/// </summary>
	public class AudiusMusicListItem : AudiusListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusMusicListItem(int position, string itemId, string title, string artist, TimeSpan duration, int reposts, int favorites, int plays, string imageUrl) : base(position, itemId, imageUrl)
		{
			Title = title;
			Artist = artist;
			Duration = duration;
			Reposts = reposts;
			Favorites = favorites;
			Plays = plays;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the title of the item
		/// </summary>
		/********************************************************************/
		public string Title
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the artist of the item
		/// </summary>
		/********************************************************************/
		public string Artist
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the total duration of the item
		/// </summary>
		/********************************************************************/
		public TimeSpan Duration
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of reposts for the item
		/// </summary>
		/********************************************************************/
		public int Reposts
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of favorites for the item
		/// </summary>
		/********************************************************************/
		public int Favorites
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of plays for the item
		/// </summary>
		/********************************************************************/
		public int Plays
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a list of tracks. Only used for playlists
		/// </summary>
		/********************************************************************/
		public AudiusMusicListItem[] Tracks
		{
			get; init;
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// This class is used for each item in an Audius list
	/// </summary>
	public class AudiusListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListItem(int position, string itemId, string title, string artist, TimeSpan duration, int reposts, int favorites, int plays, string coverUrl)
		{
			Position = position;
			ItemId = itemId;
			Title = title;
			Artist = artist;
			Duration = duration;
			Reposts = reposts;
			Favorites = favorites;
			Plays = plays;
			CoverUrl = coverUrl;
		}



		/********************************************************************/
		/// <summary>
		/// Return the position of the item
		/// </summary>
		/********************************************************************/
		public int Position
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the ID of the item
		/// </summary>
		/********************************************************************/
		public string ItemId
		{
			get;
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
		/// Returns the URL to the cover image
		/// </summary>
		/********************************************************************/
		public string CoverUrl
		{
			get;
		}
	}
}

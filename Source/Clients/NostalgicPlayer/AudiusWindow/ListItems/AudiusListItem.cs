/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
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
		public AudiusListItem(int position, string itemId, string imageUrl)
		{
			Position = position;
			ItemId = itemId;
			ImageUrl = imageUrl;
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
		/// Returns the URL to the image
		/// </summary>
		/********************************************************************/
		public string ImageUrl
		{
			get;
		}
	}
}

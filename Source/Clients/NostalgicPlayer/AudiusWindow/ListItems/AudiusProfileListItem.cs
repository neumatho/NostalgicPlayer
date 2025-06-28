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
	public class AudiusProfileListItem : AudiusListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusProfileListItem(int position, string itemId, string name, string handle, string imageUrl) : base(position, itemId, imageUrl)
		{
			Name = name;
			Handle = handle;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the user
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the handle of the user
		/// </summary>
		/********************************************************************/
		public string Handle
		{
			get;
		}
	}
}

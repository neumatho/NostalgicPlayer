/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Audius.Models.Users;

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
		public AudiusProfileListItem(int position, UserModel user) : base(position, user.Id, user.ProfilePicture?._150x150)
		{
			User = user;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the whole user model
		/// </summary>
		/********************************************************************/
		public UserModel User
		{
			get;
		}
	}
}

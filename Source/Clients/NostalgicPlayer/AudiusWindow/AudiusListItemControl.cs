/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.GuiKit.Extensions;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Render a single item in an Audius list, such as a track or playlist
	/// </summary>
	public partial class AudiusListItemControl : UserControl
	{
		private readonly AudiusListItem item;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListItemControl(AudiusListItem item)
		{
			InitializeComponent();

			this.item = item;
			SetupControls();
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item is refreshed with all missing data
		/// </summary>
		/********************************************************************/
		public void RefreshItem()
		{

		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Setup all the controls with the values from the item
		/// </summary>
		/********************************************************************/
		private void SetupControls()
		{
			positionLabel.Text = item.Position.ToString();
			titleLabel.Text = item.Title;
			artistLabel.Text = item.Artist;
			durationLabel.Text = item.Duration.ToFormattedString();
			repostsLabel.Text = string.Format(Resources.IDS_AUDIUS_ITEM_REPOSTS, item.Reposts.ToBeautifiedString());
			favoritesLabel.Text = item.Favorites.ToBeautifiedString();
			playsLabel.Text = string.Format(Resources.IDS_AUDIUS_ITEM_PLAYS, item.Plays.ToBeautifiedString());
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Audius.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Playlists;
using Polycode.NostalgicPlayer.External.Download;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the profile Playlists tab
	/// </summary>
	public partial class ProfilePlaylistsPageControl : UserControl, IAudiusPage
	{
		private IAudiusWindowApi audiusWindowApi;
		private IAudiusHelper audiusHelper;
		private IAudiusClientFactory clientFactory;

		private TaskHelper taskHelper;

		private string userId;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ProfilePlaylistsPageControl()
		{
			InitializeComponent();
		}

		#region IAudiusPage implementation
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(IAudiusWindowApi audiusWindow, IPictureDownloader downloader, string id)
		{
			audiusWindowApi = audiusWindow;
			audiusHelper = DependencyInjection.Container.GetInstance<IAudiusHelper>();
			clientFactory = DependencyInjection.Container.GetInstance<IAudiusClientFactory>();

			userId = id;

			audiusListControl.Initialize(downloader);

			taskHelper = new TaskHelper();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page with new data
		/// </summary>
		/********************************************************************/
		public void RefreshPage()
		{
			using (new SleepCursor())
			{
				CleanupPage();
				audiusListControl.SetLoading(true);

				GetPlaylists();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup used resources
		/// </summary>
		/********************************************************************/
		public void CleanupPage()
		{
			using (new SleepCursor())
			{
				taskHelper.CancelTask();

				audiusListControl.ClearItems();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Retrieve the user playlists from Audius
		/// </summary>
		/********************************************************************/
		private void GetPlaylists()
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				IUserClient userClient = clientFactory.GetUserClient();
				PlaylistModel[] playlists = userClient.GetPlaylists(userId, cancellationToken);

				List<AudiusListItem> items = audiusHelper.FillPlaylistsWithTracks(playlists, cancellationToken);

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => audiusHelper.ShowErrorMessage(ex, audiusListControl, audiusWindowApi));
		}
		#endregion
	}
}

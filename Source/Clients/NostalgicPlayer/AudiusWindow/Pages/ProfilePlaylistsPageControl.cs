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
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Audius.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Playlists;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the profile Playlists tab
	/// </summary>
	public partial class ProfilePlaylistsPageControl : UserControl, IAudiusPage
	{
		private IMainWindowApi mainWindowApi;
		private IAudiusWindowApi audiusWindowApi;

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
		public void Initialize(IMainWindowApi mainWindow, IAudiusWindowApi audiusWindow, PictureDownloader downloader, string id)
		{
			mainWindowApi = mainWindow;
			audiusWindowApi = audiusWindow;

			userId = id;

			audiusListControl.Initialize(mainWindow, downloader);

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
				AudiusApi audiusApi = new AudiusApi();

				IUserClient userClient = audiusApi.GetUserClient();
				PlaylistModel[] playlists = userClient.GetPlaylists(userId, cancellationToken);

				List<AudiusListItem> items = AudiusHelper.FillPlaylistsWithTracks(playlists, cancellationToken);

				Invoke(() =>
				{
					using (new SleepCursor())
					{
						audiusListControl.SetLoading(false);
						audiusListControl.SetItems(items);
					}
				});

				return Task.CompletedTask;
			}, (ex) => AudiusHelper.ShowErrorMessage(ex, audiusListControl, mainWindowApi, audiusWindowApi));
		}
		#endregion
	}
}

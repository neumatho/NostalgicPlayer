/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the profile Tracks tab
	/// </summary>
	public partial class ProfileTracksPageControl : UserControl, IAudiusPage
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
		public ProfileTracksPageControl()
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

				GetTracks();
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
		/// Retrieve the user tracks from Audius
		/// </summary>
		/********************************************************************/
		private void GetTracks()
		{
			taskHelper.RunTask((cancellationToken) =>
			{
				AudiusApi audiusApi = new AudiusApi();

				IUserClient userClient = audiusApi.GetUserClient();
				TrackModel[] tracks = userClient.GetTracks(userId, cancellationToken);

				List<AudiusListItem> items = tracks
					.Select((x, i) => AudiusMapper.MapTrackToItem(x, i + 1))
					.Cast<AudiusListItem>()
					.ToList();

				cancellationToken.ThrowIfCancellationRequested();

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

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.External;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Audius.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Tracks;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the profile Tracks tab
	/// </summary>
	public partial class ProfileTracksPageControl : UserControl, IDependencyInjectionControl, IAudiusPage
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
		public ProfileTracksPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IAudiusHelper audiusHelper, IAudiusClientFactory audiusClientFactory)
		{
			this.audiusHelper = audiusHelper;
			clientFactory = audiusClientFactory;
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
				IUserClient userClient = clientFactory.GetUserClient();
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
			}, (ex) => audiusHelper.ShowErrorMessage(ex, audiusListControl, audiusWindowApi));
		}
		#endregion
	}
}

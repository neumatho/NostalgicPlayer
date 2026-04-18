/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Windows.Media;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Logic.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Manages Windows System Media Transport Controls integration
	/// </summary>
	public class SystemMediaTransportControlsService : ISystemMediaTransportControlsService
	{
		private SystemMediaTransportControls smtc;

		/// <summary>
		/// Raised when the user presses the play button in system media controls
		/// </summary>
		public event EventHandler PlayRequested;

		/// <summary>
		/// Raised when the user presses the pause button in system media controls
		/// </summary>
		public event EventHandler PauseRequested;

		/// <summary>
		/// Raised when the user presses the stop button in system media controls
		/// </summary>
		public event EventHandler StopRequested;

		/// <summary>
		/// Raised when the user presses the next track button in system media controls
		/// </summary>
		public event EventHandler NextRequested;

		/// <summary>
		/// Raised when the user presses the previous track button in system media controls
		/// </summary>
		public event EventHandler PreviousRequested;

		/********************************************************************/
		/// <summary>
		/// Initialize the SMTC with the given window handle
		/// </summary>
		/********************************************************************/
		public void Initialize(IntPtr windowHandle)
		{
			try
			{
				// Get SMTC for Win32 window using interop
				smtc = SystemMediaTransportControlsInterop.GetForWindow(windowHandle);

				smtc.IsEnabled = true;
				smtc.IsPlayEnabled = true;
				smtc.IsPauseEnabled = true;
				smtc.IsStopEnabled = true;
				smtc.IsNextEnabled = true;
				smtc.IsPreviousEnabled = true;

				smtc.ButtonPressed += Smtc_ButtonPressed;
			}
			catch (Exception)
			{
				// SMTC may not be available on all platforms
				smtc = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			if (smtc != null)
			{
				smtc.ButtonPressed -= Smtc_ButtonPressed;
				smtc.IsEnabled = false;
				smtc = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle SMTC button presses
		/// </summary>
		/********************************************************************/
		private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
		{
			switch (args.Button)
			{
				case SystemMediaTransportControlsButton.Play:
				case SystemMediaTransportControlsButton.Pause:
				{
					if (smtc.PlaybackStatus == MediaPlaybackStatus.Playing)
						PauseRequested?.Invoke(this, EventArgs.Empty);
					else
						PlayRequested?.Invoke(this, EventArgs.Empty);

					break;
				}

				case SystemMediaTransportControlsButton.Stop:
				{
					StopRequested?.Invoke(this, EventArgs.Empty);
					break;
				}

				case SystemMediaTransportControlsButton.Next:
				{
					NextRequested?.Invoke(this, EventArgs.Empty);
					break;
				}

				case SystemMediaTransportControlsButton.Previous:
				{
					PreviousRequested?.Invoke(this, EventArgs.Empty);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update playback status
		/// </summary>
		/********************************************************************/
		public void UpdatePlaybackStatus(MediaPlaybackStatus status)
		{
			if (smtc != null)
				smtc.PlaybackStatus = status;
		}



		/********************************************************************/
		/// <summary>
		/// Update the media information displayed in system controls
		/// </summary>
		/********************************************************************/
		public void UpdateMetadata(ModuleListListItem listItem, ModuleInfoStatic staticInfo, int currentSong, int maxSongs)
		{
			if (smtc == null)
				return;

			try
			{
				SystemMediaTransportControlsDisplayUpdater updater = smtc.DisplayUpdater;
				updater.Type = MediaPlaybackType.Music;
				updater.AppMediaId = Resources.IDS_SMTC_APP_MEDIA_ID;

				// Get title
				string title = staticInfo?.Title;
				if (string.IsNullOrEmpty(title) && listItem != null)
					title = listItem.ListItem.DisplayName;

				// Get artist
				string artist = staticInfo?.Author;

				// Get album (use player name)
				string album = staticInfo?.PlayerAgentInfo?.AgentName;

				// Update properties
				string displayTitle = title ?? Resources.IDS_SMTC_UNKNOWN_TITLE;
				if (maxSongs > 1)
					displayTitle = string.Format(Resources.IDS_SMTC_SONG_FORMAT, displayTitle, currentSong, maxSongs);

				updater.MusicProperties.Title = displayTitle;
				updater.MusicProperties.Artist = artist ?? string.Empty;
				updater.MusicProperties.AlbumTitle = album ?? string.Empty;

				// Add track info if multiple songs
				if (maxSongs > 1)
					updater.MusicProperties.AlbumTrackCount = (uint)maxSongs;

				updater.Update();
			}
			catch (Exception)
			{
				// Ignore errors updating metadata
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear the media information
		/// </summary>
		/********************************************************************/
		public void ClearMetadata()
		{
			if (smtc == null)
				return;

			try
			{
				smtc.DisplayUpdater.ClearAll();
				smtc.DisplayUpdater.Update();
			}
			catch (Exception)
			{
				// Ignore errors
			}
		}
	}
}

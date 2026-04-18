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
	/// Interface for Windows System Media Transport Controls integration
	/// </summary>
	public interface ISystemMediaTransportControlsService : IDisposable
	{
		/// <summary>
		/// Raised when the user presses the play button in system media controls
		/// </summary>
		event EventHandler PlayRequested;

		/// <summary>
		/// Raised when the user presses the pause button in system media controls
		/// </summary>
		event EventHandler PauseRequested;

		/// <summary>
		/// Raised when the user presses the stop button in system media controls
		/// </summary>
		event EventHandler StopRequested;

		/// <summary>
		/// Raised when the user presses the next track button in system media controls
		/// </summary>
		event EventHandler NextRequested;

		/// <summary>
		/// Raised when the user presses the previous track button in system media controls
		/// </summary>
		event EventHandler PreviousRequested;

		/// <summary>
		/// Initialize the SMTC with the given window handle
		/// </summary>
		void Initialize(IntPtr windowHandle);

		/// <summary>
		/// Update playback status
		/// </summary>
		void UpdatePlaybackStatus(MediaPlaybackStatus status);

		/// <summary>
		/// Update the media information displayed in system controls
		/// </summary>
		void UpdateMetadata(ModuleListListItem listItem, ModuleInfoStatic staticInfo, int currentSong, int maxSongs);

		/// <summary>
		/// Clear the media information
		/// </summary>
		void ClearMetadata();
	}
}

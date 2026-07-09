/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	/// <summary>
	/// IClientPlayerControl part of the adapter — delegates everything to MainWindowForm.
	/// </summary>
	public partial class MainWindowApiAdapter : IClientPlayerControl
	{
		#region Playback
		/// <summary>True while a module is currently playing</summary>
		public bool IsPlaying => mainWindowForm.IsPlaying;

		/// <summary>True if playback is paused</summary>
		public bool IsPaused => mainWindowForm.IsPaused;

		/// <summary>Start (or resume) playback</summary>
		public void PlayModule() => mainWindowForm.PlayModule();

		/// <summary>Pause playback</summary>
		public void PausePlaying() => mainWindowForm.PausePlaying();

		/// <summary>Resume playback after a pause</summary>
		public void ResumePlaying() => mainWindowForm.ResumePlaying();

		// StopModule() ist bereits in MainWindowApiAdapter.cs (IMainWindowApi-Teil) implementiert.

		/// <summary>Restart the current song from the beginning</summary>
		public void RestartSong() => mainWindowForm.RestartSong();
		#endregion

		#region Sub-songs
		/// <summary>Number of sub-songs in the current module</summary>
		public int SubSongCount => mainWindowForm.SubSongCount;

		/// <summary>Current sub-song number (1-based)</summary>
		public int SubSongCurrent => mainWindowForm.SubSongCurrent;

		/// <summary>Switch to the next sub-song</summary>
		public void NextSubSong() => mainWindowForm.NextSubSong();

		/// <summary>Switch to the previous sub-song</summary>
		public void PreviousSubSong() => mainWindowForm.PreviousSubSong();
		#endregion

		#region Module list
		/// <summary>Index of the currently playing module in the list (0-based; -1 if none)</summary>
		public int ModuleIndex => mainWindowForm.ModuleIndex;

		/// <summary>Number of modules in the playlist</summary>
		public int ModuleCount => mainWindowForm.ModuleCount;

		/// <summary>Switch to the next module in the list</summary>
		public void NextModule() => mainWindowForm.NextModule();

		/// <summary>Switch to the previous module in the list</summary>
		public void PreviousModule() => mainWindowForm.PreviousModule();
		#endregion

		#region Time / position
		/// <summary>Elapsed time of the current song</summary>
		public TimeSpan ElapsedTime => mainWindowForm.ElapsedTime;

		/// <summary>Total time of the current song</summary>
		public TimeSpan TotalTime => mainWindowForm.TotalTime;

		/// <summary>Current snapshot (seek) position</summary>
		public int SnapshotPosition => mainWindowForm.SnapshotPosition;

		/// <summary>Number of snapshots available for seeking</summary>
		public int SnapshotCount => mainWindowForm.SnapshotCount;

		/// <summary>Set the snapshot (seek) position</summary>
		public void SetSnapshotPosition(int position) => mainWindowForm.SetSnapshotPosition(position);
		#endregion

		#region Mixer
		/// <summary>Enable/disable a range of mixer channels</summary>
		public void EnableChannels(bool enabled, int startChannel, int stopChannel = -1)
			=> mainWindowForm.EnableChannels(enabled, startChannel, stopChannel);
		#endregion
	}
}

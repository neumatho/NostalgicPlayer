/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Gui.Interfaces
{
	/// <summary>
	/// Control surface that the NostalgicPlayer client exposes to plugins (e.g.
	/// PatternViewer) for playback control, navigation and time/position info.
	/// Implemented by the host client (WinForms, Console, ...).
	/// </summary>
	public interface IClientPlayerControl
	{
		#region Playback
		/// <summary>True while a module is currently playing</summary>
		bool IsPlaying { get; }

		/// <summary>True if playback is paused</summary>
		bool IsPaused { get; }

		/// <summary>Start (or resume) playback</summary>
		void PlayModule();

		/// <summary>Pause playback</summary>
		void PausePlaying();

		/// <summary>Resume playback after a pause</summary>
		void ResumePlaying();

		/// <summary>Stop playback and free the current module</summary>
		void StopModule();

		/// <summary>Restart the current song from the beginning</summary>
		void RestartSong();
		#endregion

		#region Sub-songs
		/// <summary>Number of sub-songs in the current module</summary>
		int SubSongCount { get; }

		/// <summary>Current sub-song number (1-based)</summary>
		int SubSongCurrent { get; }

		/// <summary>Switch to the next sub-song</summary>
		void NextSubSong();

		/// <summary>Switch to the previous sub-song</summary>
		void PreviousSubSong();
		#endregion

		#region Module list
		/// <summary>Index of the currently playing module in the list (0-based; -1 if none)</summary>
		int ModuleIndex { get; }

		/// <summary>Number of modules in the playlist</summary>
		int ModuleCount { get; }

		/// <summary>Switch to the next module in the list</summary>
		void NextModule();

		/// <summary>Switch to the previous module in the list</summary>
		void PreviousModule();
		#endregion

		#region Time / position
		/// <summary>Elapsed time of the current song</summary>
		TimeSpan ElapsedTime { get; }

		/// <summary>Total time of the current song</summary>
		TimeSpan TotalTime { get; }

		/// <summary>Current snapshot (seek) position</summary>
		int SnapshotPosition { get; }

		/// <summary>Number of snapshots available for seeking</summary>
		int SnapshotCount { get; }

		/// <summary>Set the snapshot (seek) position</summary>
		void SetSnapshotPosition(int position);
		#endregion

		#region Mixer
		/// <summary>Enable/disable a range of mixer channels</summary>
		void EnableChannels(bool enabled, int startChannel, int stopChannel = -1);
		#endregion
	}
}

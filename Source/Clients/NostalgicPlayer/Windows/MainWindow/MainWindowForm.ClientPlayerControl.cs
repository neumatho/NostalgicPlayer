/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	/// <summary>
	/// Implementation of the IClientPlayerControl surface used by plugins
	/// (e.g. PatternViewer) to control playback / navigation / time / mixer.
	/// </summary>
	public partial class MainWindowForm
	{
		#region Playback
		/// <summary>True while a module is currently playing</summary>
		public bool IsPlaying => playItem != null;

		/// <summary>True if playback is paused</summary>
		public bool IsPaused => pauseCheckButton.Checked;

		/// <summary>Start (or resume) playback</summary>
		public void PlayModule()
		{
			if (playItem == null)
			{
				// No module playing, try to start one
				if (moduleListControl.Items.Count > 0)
					LoadAndPlayModule(0);
			}
			else if (pauseCheckButton.Checked)
			{
				// Resume from pause
				pauseCheckButton.Checked = false;
			}
		}

		/// <summary>Pause playback</summary>
		public void PausePlaying()
		{
			if ((playItem != null) && !pauseCheckButton.Checked)
				pauseCheckButton.Checked = true;
		}

		/// <summary>Resume playback after a pause</summary>
		public void ResumePlaying()
		{
			if ((playItem != null) && pauseCheckButton.Checked)
				pauseCheckButton.Checked = false;
		}

		/// <summary>Restart the current song from the beginning</summary>
		public void RestartSong()
		{
			if (playItem != null)
				StartSong(moduleHandler.PlayingModuleInformation.CurrentSong);
		}
		#endregion

		#region Sub-songs
		/// <summary>Number of sub-songs in the current module</summary>
		public int SubSongCount => playItem != null ? moduleHandler.StaticModuleInformation.MaxSongNumber : 0;

		/// <summary>Current sub-song number (1-based)</summary>
		public int SubSongCurrent => playItem != null ? moduleHandler.PlayingModuleInformation.CurrentSong + 1 : 0;

		/// <summary>Switch to the next sub-song</summary>
		public void NextSubSong()
		{
			if (playItem != null)
			{
				int curSong = moduleHandler.PlayingModuleInformation.CurrentSong;
				int maxSong = moduleHandler.StaticModuleInformation.MaxSongNumber;

				if (curSong < maxSong - 1)
					StartSong(curSong + 1);
			}
		}

		/// <summary>Switch to the previous sub-song</summary>
		public void PreviousSubSong()
		{
			if (playItem != null)
			{
				int curSong = moduleHandler.PlayingModuleInformation.CurrentSong;

				if (curSong > 0)
					StartSong(curSong - 1);
			}
		}
		#endregion

		#region Module list
		/// <summary>Index of the currently playing module in the list (0-based; -1 if none)</summary>
		public int ModuleIndex => playItem != null ? moduleListControl.Items.IndexOf(playItem) : -1;

		/// <summary>Number of modules in the playlist</summary>
		public int ModuleCount => moduleListControl.Items.Count;

		/// <summary>Switch to the next module in the list</summary>
		public void NextModule()
		{
			if (moduleListControl.Items.Count > 0)
			{
				int currentIndex = playItem != null ? moduleListControl.Items.IndexOf(playItem) : -1;
				int nextIndex = currentIndex + 1;

				if (nextIndex < moduleListControl.Items.Count)
				{
					StopAndFreeModule();
					LoadAndPlayModule(nextIndex);
				}
			}
		}

		/// <summary>Switch to the previous module in the list</summary>
		public void PreviousModule()
		{
			if (moduleListControl.Items.Count > 0)
			{
				int currentIndex = playItem != null ? moduleListControl.Items.IndexOf(playItem) : -1;
				int prevIndex = currentIndex - 1;

				if (prevIndex >= 0)
				{
					StopAndFreeModule();
					LoadAndPlayModule(prevIndex);
				}
			}
		}
		#endregion

		#region Time / position
		/// <summary>Elapsed time of the current song</summary>
		public TimeSpan ElapsedTime => timeOccurred;

		/// <summary>Total time of the current song</summary>
		public TimeSpan TotalTime => playItem != null ? moduleHandler.PlayingModuleInformation.SongTotalTime : TimeSpan.Zero;

		/// <summary>Current snapshot (seek) position</summary>
		public int SnapshotPosition => playItem != null ? moduleHandler.PlayingModuleInformation.SongPosition : 0;

		/// <summary>Number of snapshots available for seeking</summary>
		public int SnapshotCount => playItem != null ? moduleHandler.PlayingModuleInformation.SongLength : 0;

		/// <summary>Set the snapshot (seek) position</summary>
		public void SetSnapshotPosition(int position)
		{
			if (playItem != null)
				SetPosition(position);
		}
		#endregion

		#region Mixer
		/// <summary>Enable/disable a range of mixer channels</summary>
		public void EnableChannels(bool enabled, int startChannel, int stopChannel = -1)
		{
			moduleHandler.EnableChannels(enabled, startChannel, stopChannel);
			moduleHandler.ApplyCurrentMixerSettings();

			// Update the settings window if it's open
			if (stopChannel == -1)
				stopChannel = startChannel;

			for (int ch = startChannel; ch <= stopChannel; ch++)
			{
				settingsWindow?.UpdateMixerChannel(ch, enabled);
			}
		}
		#endregion
	}
}

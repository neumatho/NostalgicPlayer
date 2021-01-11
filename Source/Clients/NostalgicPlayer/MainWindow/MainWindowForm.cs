/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This is the main window
	/// </summary>
	public partial class MainWindowForm : WindowFormBase
	{
		private Manager agentManager;
		private ModuleHandler moduleHandler;

		private Settings userSettings;
		private PathSettings pathSettings;
		private SoundSettings soundSettings;

		private MainWindowSettings mainWindowSettings;

		private OpenFileDialog moduleFileDialog;

		// Timer variables
		private MainWindowSettings.TimeFormat timeFormat;
		private DateTime timeStart;
		private TimeSpan timeOccurred;

		// Module variables
		private ModuleListItem playItem;
		private TimeSpan songTotalTime;
		//XXprivate int subSongMultiply;

		// Information variables
		private int prevSongPosition;

		// List times
		private TimeSpan listTime;
		private TimeSpan selectedTime;

		// Window/control status variables
		private bool allowPosSliderUpdate;

		// Misc.
		private List<int> randomList;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainWindowForm()
		{
			InitializeComponent();

			// Initialize member variables
			playItem = null;
			allowPosSliderUpdate = true;
			randomList = new List<int>();

			if (!DesignMode)
			{
				// Initialize and load settings
				InitSettings();

				// Load agents
				LoadAgents();

				// Initialize module handler
				InitModuleHandler();

				// Initialize main window
				InitializeControls();

				// Create other windows
				CreateWindows();
			}

			SetupHandlers();
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		public void ShowErrorMessage(string message, ModuleListItem listItem)
		{
			CustomMessageBox dialog = new CustomMessageBox(message, Properties.Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error);
			dialog.AddButton(Properties.Resources.IDS_BUT_SKIP, '1');
			dialog.AddButton(Properties.Resources.IDS_BUT_SKIPREMOVE, '2');
			dialog.AddButton(Properties.Resources.IDS_BUT_STOP, '3');
			dialog.ShowDialog();
			char response = dialog.GetButtonResult();

			switch (response)
			{
				// Skip
				case '1':
				{
					// Get the index of the module that couldn't be loaded + 1
					int index = moduleListBox.Items.IndexOf(listItem) + 1;

					// Get the number of items in the list
					int count = moduleListBox.Items.Count;

					// Deselect the playing flag
					ChangePlayItem(null);

					// Do there exist a "next" module
					if (index < count)
					{
						// Yes, load it
						LoadAndPlayModule((ModuleListItem)moduleListBox.Items[index]);
					}
					else
					{
						// No
						if (count != 1)
						{
							// Load the first module, but only if it's valid
							// or haven't been loaded before
							ModuleListItem item = (ModuleListItem)moduleListBox.Items[0];
							if (!item.HaveTime || (item.HaveTime && item.Time.TotalMilliseconds != 0))
								LoadAndPlayModule(item);
						}
					}
					break;
				}

				// Skip and remove
				case '2':
				{
					// Get the index of the module that couldn't be loaded
					int index = moduleListBox.Items.IndexOf(listItem);

					// Get the number of items in the list - 1
					int count = moduleListBox.Items.Count - 1;

					// Deselect the playing flag
					ChangePlayItem(null);

					// Remove the module from the list
					moduleListBox.Items.RemoveAt(index);

					// Do there exist a "next" module
					if (index < count)
					{
						// Yes, load it
						LoadAndPlayModule((ModuleListItem)moduleListBox.Items[index]);
					}
					else
					{
						// No
						if (count >= 1)
						{
							// Load the first module
							LoadAndPlayModule((ModuleListItem)moduleListBox.Items[0]);
						}
					}
					break;
				}

				// Stop playing
				case '3':
				{
					// Deselect the playing flag
					ChangePlayItem(null);
					break;
				}
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Setup event handlers
		/// </summary>
		/********************************************************************/
		private void SetupHandlers()
		{
			// Form
			FormClosed += MainWindowForm_FormClosed;

			// Module information
			infoGroup.Panel.Click += InfoGroup_Click;
			infoLabel.Click += InfoGroup_Click;

			// Module list
			moduleListBox.SelectedIndexChanged += ModuleListBox_SelectedIndexChanged;
			moduleListBox.ListBox.MouseDoubleClick += ModuleListBox_MouseDoubleClick;

			// Volume
			muteCheckButton.CheckedChanged += MuteCheckButton_CheckedChanged;
			masterVolumeTrackBar.ValueChanged += MasterVolumeTrackBar_ValueChanged;

			// Position slider
			positionTrackBar.MouseDown += PositionTrackBar_MouseDown;
			positionTrackBar.MouseUp += PositionTrackBar_MouseUp;
			positionTrackBar.Click += PositionTrackBar_Click;
			positionTrackBar.ValueChanged += PositionTrackBar_ValueChanged;

			// Tape deck
			playButton.Click += PlayButton_Click;
			pauseCheckButton.CheckedChanged += PauseCheckButton_CheckedChanged;
			ejectButton.Click += EjectButton_Click;

			rewindButton.Click += RewindButton_Click;
			fastForwardButton.Click += FastForwardButton_Click;

			previousModuleButton.Click += PreviousModuleButton_Click;
			nextModuleButton.Click += NextModuleButton_Click;

			// Module handler
			moduleHandler.PositionChanged += ModuleHandler_PositionChanged;
			moduleHandler.EndReached += ModuleHandler_EndReached;
		}

		#region Form events
		/********************************************************************/
		/// <summary>
		/// Is called when the main window is closed
		/// </summary>
		/********************************************************************/

		private void MainWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Stop any playing modules
			StopAndFreeModule();

			// Stop the module handler
			CleanupModuleHandler();

			// Close all windows
			CloseWindows();

			// Save and cleanup the settings
			CleanupSettings();
		}
		#endregion

		#region Module information events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the module information
		/// </summary>
		/********************************************************************/
		private void InfoGroup_Click(object sender, EventArgs e)
		{
			// Switch between the time formats
			if (timeFormat == MainWindowSettings.TimeFormat.Elapsed)
				timeFormat = MainWindowSettings.TimeFormat.Remaining;
			else
				timeFormat = MainWindowSettings.TimeFormat.Elapsed;

			// Show it to the user
			PrintInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Is called every second to update the module information
		/// </summary>
		/********************************************************************/
		private void ClockTimer_Tick(object sender, EventArgs e)
		{
			// Do only print the information if the module is playing
			if ((playItem != null) && moduleHandler.IsPlaying)
			{
				// Calculate time offset
				timeOccurred = DateTime.Now - timeStart;

				// Print the module information
				PrintInfo();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed position
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_PositionChanged(object sender, EventArgs e)
		{
			BeginInvoke(new Action(() =>
			{
				int newPos = moduleHandler.PlayingModuleInformation.SongPosition;

				// Change the time to the position time
				if (newPos < prevSongPosition)
					SetPositionTime(newPos);

				prevSongPosition = newPos;

				// Print the information
				PrintInfo();
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player has reached the end
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_EndReached(object sender, EventArgs e)
		{
			BeginInvoke(new Action(() =>
			{
				// Check to see if there is module loop on
				if (!loopCheckButton.Checked)
				{
					// Get the number of modules in the list
					int count = moduleListBox.Items.Count;

					// Get the index of the current playing module
					int curPlay = moduleListBox.Items.IndexOf(playItem);

					// The next module to load
					int newPlay = curPlay + 1;

					// Test to see if we is at the end of the list
					if (newPlay == count)
						newPlay = 0;

					// Free the module
					StopAndFreeModule();

					// Load the module
					LoadAndPlayModule((ModuleListItem)moduleListBox.Items[newPlay]);
				}
			}));
		}
		#endregion

		#region Module list events
		/********************************************************************/
		/// <summary>
		/// The user changed the selection of the modules
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Update the time and count controls
			UpdateTimes();
			UpdateListCount();

			// Update the list controls
			UpdateListControls();
		}



		/********************************************************************/
		/// <summary>
		/// The user double-clicked in the module list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// Find out, if an item has been clicked and which one
			int index = moduleListBox.IndexFromPoint(e.Location);
			if (index != -1)
			{
				// Stop playing any modules
				StopAndFreeModule();

				// Load and play the selected module
				LoadAndPlayModule((ModuleListItem)moduleListBox.Items[index]);
			}
		}
		#endregion

		#region Volume events
		/********************************************************************/
		/// <summary>
		/// The user has changed the mute status
		/// </summary>
		/********************************************************************/
		private void MuteCheckButton_CheckedChanged(object sender, EventArgs e)
		{
			moduleHandler.SetMuteStatus(muteCheckButton.Checked);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the master volume has changed
		/// </summary>
		/********************************************************************/
		private void MasterVolumeTrackBar_ValueChanged(object sender, EventArgs e)
		{
			moduleHandler.SetVolume(masterVolumeTrackBar.Value);
		}
		#endregion

		#region Position slider events
		/********************************************************************/
		/// <summary>
		/// The user hold down the mouse button on the slider
		/// </summary>
		/********************************************************************/

		private void PositionTrackBar_MouseDown(object sender, MouseEventArgs e)
		{
			// Prevent slider updates when printing information
			allowPosSliderUpdate = false;
		}



		/********************************************************************/
		/// <summary>
		/// The user released the mouse button on the slider
		/// </summary>
		/********************************************************************/

		private void PositionTrackBar_MouseUp(object sender, MouseEventArgs e)
		{
			allowPosSliderUpdate = true;
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the track bar
		/// </summary>
		/********************************************************************/
		private void PositionTrackBar_Click(object sender, EventArgs e)
		{
			// Calculate the song position
			int songLength = moduleHandler.PlayingModuleInformation.SongLength;
			int val = positionTrackBar.Value;

			int newSongPos = val == 100 ? songLength - 1 : val * songLength / 100;

			// Set the new song position
			SetPosition(newSongPos);
		}



		/********************************************************************/
		/// <summary>
		/// The value has changed
		/// </summary>
		/********************************************************************/
		private void PositionTrackBar_ValueChanged(object sender, EventArgs e)
		{
			// Only do this, if the user change the position and not the player
			if (!allowPosSliderUpdate)
			{
				// Calculate the song position
				int songLength = moduleHandler.PlayingModuleInformation.SongLength;
				int val = positionTrackBar.Value;

				int newSongPos = val == 100 ? songLength - 1 : val * songLength / 100;

				// Set the new song position
				SetPosition(newSongPos);
			}
		}
		#endregion

		#region Tape deck events
		/********************************************************************/
		/// <summary>
		/// The user clicked on the play button
		/// </summary>
		/********************************************************************/
		private void PlayButton_Click(object sender, EventArgs e)
		{
			if (ShowModuleFileDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
					// Stop playing any modules
					StopAndFreeModule();

					// Clear the module list
					EmptyList();

					// Add all the files in the module list
					AddFilesToList(moduleFileDialog.FileNames);

					// Start to load and play the first module
					LoadAndPlayModule((ModuleListItem)moduleListBox.Items[0]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the pause button
		/// </summary>
		/********************************************************************/
		private void PauseCheckButton_CheckedChanged(object sender, EventArgs e)
		{
			if (pauseCheckButton.Checked)
			{
				moduleHandler.PausePlaying();

				// Stop the timers
				StopTimers();
			}
			else
			{
				// Start to play again
				moduleHandler.ResumePlaying();

				// We add the time to the start time, so there won't be added
				// a lot of seconds to the played time
				timeStart += (DateTime.Now - timeStart - timeOccurred);

				// Start the timers again
				StartTimers(false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the eject button
		/// </summary>
		/********************************************************************/
		private void EjectButton_Click(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				// Stop and free the playing module
				StopAndFreeModule();
				moduleHandler.FreeAllModules();
			}
			else
			{
				// Clear the module list
				EmptyList();
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the rewind button
		/// </summary>
		/********************************************************************/
		private void RewindButton_Click(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				int songPos = moduleHandler.PlayingModuleInformation.SongPosition;

				// Only load the previous module if song position is 0 and
				// module loop isn't on
				if (songPos == 0)
				{
					if (!loopCheckButton.Checked)
						previousModuleButton.PerformClick();

					songPos = 1;
				}

				// Change the position
				SetPosition(songPos - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the fast forward button
		/// </summary>
		/********************************************************************/
		private void FastForwardButton_Click(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				int songLength = moduleHandler.PlayingModuleInformation.SongLength;
				int songPos = moduleHandler.PlayingModuleInformation.SongPosition;

				// Only load the next module if song position is the last and
				// module loop isn't on
				if (songPos == (songLength - 1))
				{
					if (!loopCheckButton.Checked)
						nextModuleButton.PerformClick();

					songPos = -1;
				}

				// Change the position
				SetPosition(songPos + 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the previous module button
		/// </summary>
		/********************************************************************/
		private void PreviousModuleButton_Click(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				// Did the song played for more than 2 seconds?
				if (timeOccurred.TotalSeconds > 2)
				{
					// Yes, start the module over again
					int currentSong = moduleHandler.PlayingModuleInformation.CurrentSong;
					StartSong(currentSong);
				}
				else
				{
					// Load previous module
					int newIndex = moduleListBox.Items.IndexOf(playItem) - 1;
					int count = moduleListBox.Items.Count;

					if (newIndex < 0)
					{
						newIndex = count - 1;
						if (newIndex == 0)
						{
							// Start over anyway, since this is the only module in the list
							int currentSong = moduleHandler.PlayingModuleInformation.CurrentSong;
							StartSong(currentSong);
							return;
						}
					}

					// Stop playing the module
					StopAndFreeModule();

					// Load and play the new module
					LoadAndPlayModule((ModuleListItem)moduleListBox.Items[newIndex]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the next module button
		/// </summary>
		/********************************************************************/
		private void NextModuleButton_Click(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				// Load next module
				int newIndex = moduleListBox.Items.IndexOf(playItem) + 1;
				int count = moduleListBox.Items.Count;

				if (newIndex == count)
					newIndex = 0;

				// Stop playing the module
				StopAndFreeModule();

				// Load and play the new module
				LoadAndPlayModule((ModuleListItem)moduleListBox.Items[newIndex]);
			}
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will read the settings
		/// </summary>
		/********************************************************************/
		private void InitSettings()
		{
			// Create instances of the settings
			userSettings = new Settings("Settings");
			userSettings.LoadSettings();

			// Setup setting wrappers
			pathSettings = new PathSettings(userSettings);
			soundSettings = new SoundSettings(userSettings);

			LoadWindowSettings("MainWindow");
			mainWindowSettings = new MainWindowSettings(allWindowSettings);

			// Set the time format
			timeFormat = mainWindowSettings.Time;

			// Set the master volume
			masterVolumeTrackBar.Value = mainWindowSettings.MasterVolume;
		}



		/********************************************************************/
		/// <summary>
		/// Will save and destroy the settings
		/// </summary>
		/********************************************************************/
		private void CleanupSettings()
		{
			userSettings.SaveSettings();
			userSettings.Dispose();
			userSettings = null;

			soundSettings = null;
			pathSettings = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will setup the controls
		/// </summary>
		/********************************************************************/
		private void InitializeControls()
		{
			// Set the window title
			ShowNormalTitle();

			// Create the file menu
			ToolStripMenuItem fileMenuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_FILE);

			ToolStripMenuItem menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_FILE_OPEN);
			menuItem.ShortcutKeys = Keys.Alt | Keys.O;
			menuItem.Click += PlayButton_Click;
			fileMenuItem.DropDownItems.Add(menuItem);

			fileMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_FILE_EXIT);
			menuItem.ShortcutKeys = Keys.Alt | Keys.F4;
			fileMenuItem.DropDownItems.Add(menuItem);

			menuStrip.Items.Add(fileMenuItem);

			// Create the window menu
			ToolStripMenuItem windowMenuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_WINDOW);

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_WINDOW_SETTINGS);
			menuItem.Enabled = false;
			windowMenuItem.DropDownItems.Add(menuItem);

			windowMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_WINDOW_PLAYERS);
			menuItem.Visible = false;
			windowMenuItem.DropDownItems.Add(menuItem);

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_WINDOW_AGENTS);
			menuItem.Visible = false;
			windowMenuItem.DropDownItems.Add(menuItem);

			menuStrip.Items.Add(windowMenuItem);

			// Create the help menu
			ToolStripMenuItem helpMenuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_HELP);

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_HELP_HELP);
			menuItem.ShortcutKeys = Keys.Alt | Keys.H;
			menuItem.Enabled = false;
			helpMenuItem.DropDownItems.Add(menuItem);

			helpMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Properties.Resources.IDS_MENU_HELP_ABOUT);
			menuItem.Enabled = false;
			helpMenuItem.DropDownItems.Add(menuItem);

			menuStrip.Items.Add(helpMenuItem);

			// Set tooltip on all controls
			toolTip.SetToolTip(infoGroup.Panel, Properties.Resources.IDS_TIP_MAIN_INFO);
			toolTip.SetToolTip(infoLabel, Properties.Resources.IDS_TIP_MAIN_INFO);
			toolTip.SetToolTip(moduleInfoButton, Properties.Resources.IDS_TIP_MAIN_MODULEINFO);

			toolTip.SetToolTip(muteCheckButton, Properties.Resources.IDS_TIP_MAIN_MUTE);
			toolTip.SetToolTip(masterVolumeTrackBar, Properties.Resources.IDS_TIP_MAIN_VOLUME);

			toolTip.SetToolTip(addModuleButton, Properties.Resources.IDS_TIP_MAIN_ADD);
			toolTip.SetToolTip(removeModuleButton, Properties.Resources.IDS_TIP_MAIN_REMOVE);
			toolTip.SetToolTip(swapModulesButton, Properties.Resources.IDS_TIP_MAIN_SWAP);
			toolTip.SetToolTip(sortModulesButton, Properties.Resources.IDS_TIP_MAIN_SORT);
			toolTip.SetToolTip(moveModulesUpButton, Properties.Resources.IDS_TIP_MAIN_UP);
			toolTip.SetToolTip(moveModulesDownButton, Properties.Resources.IDS_TIP_MAIN_DOWN);
			toolTip.SetToolTip(listButton, Properties.Resources.IDS_TIP_MAIN_SELECT);
			toolTip.SetToolTip(diskButton, Properties.Resources.IDS_TIP_MAIN_DISK);

			toolTip.SetToolTip(timeLabel, Properties.Resources.IDS_TIP_MAIN_TIME);
			toolTip.SetToolTip(totalLabel, Properties.Resources.IDS_TIP_MAIN_TOTAL);

			toolTip.SetToolTip(positionTrackBar, Properties.Resources.IDS_TIP_MAIN_POSITIONSLIDER);

			toolTip.SetToolTip(previousModuleButton, Properties.Resources.IDS_TIP_MAIN_PREVMOD);
			toolTip.SetToolTip(previousSongButton, Properties.Resources.IDS_TIP_MAIN_PREVSONG);
			toolTip.SetToolTip(rewindButton, Properties.Resources.IDS_TIP_MAIN_REWIND);
			toolTip.SetToolTip(playButton, Properties.Resources.IDS_TIP_MAIN_PLAY);
			toolTip.SetToolTip(fastForwardButton, Properties.Resources.IDS_TIP_MAIN_FORWARD);
			toolTip.SetToolTip(nextSongButton, Properties.Resources.IDS_TIP_MAIN_NEXTSONG);
			toolTip.SetToolTip(nextModuleButton, Properties.Resources.IDS_TIP_MAIN_NEXTMOD);
			toolTip.SetToolTip(ejectButton, Properties.Resources.IDS_TIP_MAIN_EJECT);
			toolTip.SetToolTip(pauseCheckButton, Properties.Resources.IDS_TIP_MAIN_PAUSE);

			toolTip.SetToolTip(loopCheckButton, Properties.Resources.IDS_TIP_MAIN_MODULELOOP);
			toolTip.SetToolTip(showSamplesButton, Properties.Resources.IDS_TIP_MAIN_SAMP);

			// Update the tape deck buttons
			UpdateTapeDeck();

			// Print the module information
			PrintInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Will create and open other windows
		/// </summary>
		/********************************************************************/
		private void CreateWindows()//XX
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will save windows specific settings and close all the windows
		/// used
		/// </summary>
		/********************************************************************/
		private void CloseWindows()
		{
			// Destroy the module file dialog
			if (moduleFileDialog != null)
			{
				moduleFileDialog.Dispose();
				moduleFileDialog = null;
			}

			// Remember settings from main window
			mainWindowSettings.Time = timeFormat;
			mainWindowSettings.MasterVolume = masterVolumeTrackBar.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Will add all the given files to the module list
		/// </summary>
		/********************************************************************/
		private void AddFilesToList(string[] files, int startIndex = -1)
		{
			using (new SleepCursor())
			{
				List<FileInformation> itemList = new List<FileInformation>();

				foreach (string file in files)
				{
					//XX TODO: When adding drag'n'drop support, check for folders here
					AppendFile(file, itemList);
				}

				// Add the items to the list
				moduleListBox.BeginUpdate();

				try
				{
					if (startIndex == -1)
						moduleListBox.Items.AddRange(itemList.Select(fi => new ModuleListItem(new SingleFileListItem(fi.FullPath))).ToArray());
					else
					{
						for (int i = itemList.Count - 1; i >= 0; i--)
							moduleListBox.Items.Insert(startIndex, new ModuleListItem(new SingleFileListItem(itemList[i].FullPath)));
					}
				}
				finally
				{
					moduleListBox.EndUpdate();
				}

				// Update the controls
				UpdateListCount();
				UpdateTimes();
				UpdateTapeDeck();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the sub-songs variables
		/// </summary>
		/********************************************************************/
		private void InitSubSongs()
		{
			//XXsubSongMultiply = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize window controls
		/// </summary>
		/********************************************************************/
		private void InitControls()
		{
			prevSongPosition = -1;

			// Start the timers
			if (playItem != null)
				StartTimers(true);

			// Make sure the pause button isn't pressed
			pauseCheckButton.Checked = false;

			// Set the position slider on the first position
			positionTrackBar.Value = 0;

			// Update the tape deck buttons
			UpdateTapeDeck();

			// Get module information
			ModuleInfoFloating playingModuleInfo = moduleHandler.PlayingModuleInformation;

			// Get the total time of the playing song
			songTotalTime = playingModuleInfo.TotalTime;

			// Change the time to the position time
			SetPositionTime(playingModuleInfo.SongPosition);

			// Print the module information
			PrintInfo();

			// Update the list item
			SetTimeOnItem(playItem, songTotalTime);

			if (playItem != null)
			{
				// Check to see if the playing item can be seen
				int itemIndex = moduleListBox.Items.IndexOf(playItem);
				Rectangle listRect = moduleListBox.Bounds;
				Rectangle itemRect = moduleListBox.GetItemRectangle(itemIndex);
				itemRect.X += listRect.X;
				itemRect.Y += listRect.Y;

				if (!listRect.Contains(itemRect))
				{
					// Make sure the item can be seen
					moduleListBox.TopIndex = itemIndex;
				}

				// Add the new index to the random list
				randomList.Add(itemIndex);

				// Do we need to remove any previous items
				if (randomList.Count > (moduleListBox.Items.Count / 3))
					randomList.RemoveAt(0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Change the play item
		/// </summary>
		/********************************************************************/
		private void ChangePlayItem(ModuleListItem listItem)
		{
			moduleListBox.BeginUpdate();

			try
			{
				// First deselect any previous item
				if (playItem != null)
				{
					playItem.IsPlaying = false;
					moduleListBox.Invalidate(moduleListBox.GetItemRectangle(moduleListBox.Items.IndexOf(playItem)));
				}

				// Remember the item
				playItem = listItem;

				if (playItem != null)
				{
					playItem.IsPlaying = true;
					moduleListBox.Invalidate(moduleListBox.GetItemRectangle(moduleListBox.Items.IndexOf(playItem)));
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure to refresh all the windows
		/// </summary>
		/********************************************************************/
		private void RefreshWindows()//XX
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will set the window title to normal
		/// </summary>
		/********************************************************************/
		private void ShowNormalTitle()
		{
			Text = Properties.Resources.IDS_MAIN_TITLE;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the time on the item given
		/// </summary>
		/********************************************************************/
		private void SetTimeOnItem(ModuleListItem listItem, TimeSpan time)
		{
			if (listItem != null)
			{
				// Get the previous item time
				TimeSpan prevTime = listItem.Time;

				// Change the time on the item
				listItem.Time = time;

				// And update it in the list
				moduleListBox.BeginUpdate();

				try
				{
					moduleListBox.Invalidate(moduleListBox.GetItemRectangle(moduleListBox.Items.IndexOf(listItem)));
				}
				finally
				{
					moduleListBox.EndUpdate();
				}

				// Now calculate the new list time
				listTime = listTime - prevTime + time;

				// And show it
				UpdateTimes();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load all available agents
		/// </summary>
		/********************************************************************/
		private void LoadAgents()
		{
			agentManager = new Manager();
			agentManager.LoadAllAgents();
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the module handler and start it
		/// </summary>
		/********************************************************************/
		private void InitModuleHandler()
		{
			// Force the creation of the handle, because it is needed if
			// an error occur before the main window is opened
			CreateHandle();

			moduleHandler = new ModuleHandler();
			moduleHandler.Initialize(this, agentManager, soundSettings, masterVolumeTrackBar.Value);
		}



		/********************************************************************/
		/// <summary>
		/// Will shutdown the module handler again
		/// </summary>
		/********************************************************************/
		private void CleanupModuleHandler()
		{
			moduleHandler?.Shutdown();
			moduleHandler = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will append the given file to the list given
		/// </summary>
		/********************************************************************/
		private void AppendFile(string fileName, List<FileInformation> list)
		{
			//XX TODO: Add archive support here
			list.Add(new FileInformation(fileName));
		}



		/********************************************************************/
		/// <summary>
		/// Will update all the tape deck buttons
		/// </summary>
		/********************************************************************/
		private void UpdateTapeDeck()
		{
			// Get the number of items in the list
			int count = moduleListBox.Items.Count;

			// Get playing flag
			bool isPlaying = moduleHandler.IsPlaying;

			// If the module list is empty, disable the eject button
			ejectButton.Enabled = count > 0;

			// If no module is playing, disable the pause button
			pauseCheckButton.Enabled = isPlaying;

			// If only one item is in the list or none is playing, disable
			// the previous and next module buttons
			if (isPlaying && (count > 1))
			{
				previousModuleButton.Enabled = true;
				nextModuleButton.Enabled = true;
			}
			else
			{
				previousModuleButton.Enabled = false;
				nextModuleButton.Enabled = false;
			}

			ModuleInfoStatic staticModuleInfo = moduleHandler.StaticModuleInformation;
			ModuleInfoFloating playingModuleInfo = moduleHandler.PlayingModuleInformation;

			// If playing sub-song 1, disable the previous song button
			int curSong = playingModuleInfo.CurrentSong;
			previousSongButton.Enabled = curSong > 0;

			// If playing the last sub-song, disable the next song button
			nextSongButton.Enabled = curSong < staticModuleInfo.MaxSongNumber - 1;

			// If no module is playing or the player doesn't support position change,
			// disable the rewind and forward buttons + the position slider
			if (isPlaying && staticModuleInfo.CanChangePosition)
			{
				rewindButton.Enabled = true;
				fastForwardButton.Enabled = true;
				positionTrackBar.Enabled = true;
			}
			else
			{
				rewindButton.Enabled = false;
				fastForwardButton.Enabled = false;
				positionTrackBar.Enabled = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the list times
		/// </summary>
		/********************************************************************/
		private void UpdateTimes()
		{
			// Calculate the total time for all the selected items
			selectedTime = new TimeSpan(0);

			foreach (ModuleListItem listItem in moduleListBox.SelectedItems)
			{
				// Add the time to the total
				selectedTime += listItem.Time;
			}

			// Build the selected time string
			string selStr, listStr;

			TimeSpan tempTime = new TimeSpan((((long)selectedTime.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
			if ((int)tempTime.TotalHours > 0)
				selStr = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT);
			else
				selStr = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT_SMALL);

			// And build the list time string
			tempTime = new TimeSpan((((long)listTime.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
			if ((int)tempTime.TotalHours > 0)
				listStr = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT);
			else
				listStr = tempTime.ToString(Properties.Resources.IDS_TIMEFORMAT_SMALL);

			// And update the label control
			timeLabel.Text = $"{selStr}/{listStr}";
		}



		/********************************************************************/
		/// <summary>
		/// Will update the list count control
		/// </summary>
		/********************************************************************/
		private void UpdateListCount()
		{
			// Get the different numbers
			int selected = moduleListBox.SelectedItems.Count;
			int total = moduleListBox.Items.Count;

			// Update the "number of files" label
			totalLabel.Text = $"{selected}/{total}";
		}



		/********************************************************************/
		/// <summary>
		/// Will update the list controls
		/// </summary>
		/********************************************************************/
		private void UpdateListControls()
		{
			if (moduleListBox.SelectedIndex == -1)
			{
				// No items are selected
				removeModuleButton.Enabled = false;
				swapModulesButton.Enabled = false;
				moveModulesUpButton.Enabled = false;
				moveModulesDownButton.Enabled = false;
			}
			else
			{
				// Some items are selected
				removeModuleButton.Enabled = true;
				moveModulesUpButton.Enabled = true;
				moveModulesDownButton.Enabled = true;

				// Are two and only two items selected?
				if (moduleListBox.SelectedItems.Count == 2)
				{
					// Enable the swap button
					swapModulesButton.Enabled = true;
				}
				else
				{
					// Disable the swap button
					swapModulesButton.Enabled = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Print the current information in the info bar
		/// </summary>
		/********************************************************************/
		private void PrintInfo()
		{
			string posStr, subStr, timeStr;

			if (moduleHandler.IsPlaying)
			{
				ModuleInfoStatic staticModuleInfo = moduleHandler.StaticModuleInformation;
				ModuleInfoFloating playingModuleInfo = moduleHandler.PlayingModuleInformation;

				// Create song position string
				int songLength = playingModuleInfo.SongLength;
				int songPos = playingModuleInfo.SongPosition;

				if (songLength > 0)
				{
					// We got a length, so we can create the position string
					int percent = songPos * 100 / songLength;
					posStr = string.Format(Properties.Resources.IDS_POSITION, songPos, songLength, percent);

					// Set the position slider
					if (allowPosSliderUpdate && (positionTrackBar.Value != percent))
						positionTrackBar.Value = percent;
				}
				else
				{
					// Set the position string to n/a
					posStr = Properties.Resources.IDS_NOPOSITION;

					//XX Never ending update
				}

				// Create sub-song string
				int currentSong = playingModuleInfo.CurrentSong + 1;
				int maxSong = staticModuleInfo.MaxSongNumber;

				if (maxSong == 0)
				{
					subStr = Properties.Resources.IDS_NOSUBSONGS;
					timeStr = timeFormat == MainWindowSettings.TimeFormat.Elapsed ? Properties.Resources.IDS_NOTIME : Properties.Resources.IDS_NONEGATIVETIME;
				}
				else
				{
					subStr = string.Format(Properties.Resources.IDS_SUBSONGS, currentSong, maxSong);

					// Format the time string
					if (timeFormat == MainWindowSettings.TimeFormat.Elapsed)
					{
						timeStr = string.Format("{0} {1}", Properties.Resources.IDS_TIME, timeOccurred.ToString(Properties.Resources.IDS_TIMEFORMAT));
					}
					else
					{
						// Calculate the remaining time
						TimeSpan tempSpan = songTotalTime - timeOccurred;

						// Check to see if we got a negative number
						if (tempSpan.TotalMilliseconds < 0)
							tempSpan = new TimeSpan(0);

						// Format the string
						timeStr = string.Format("{0} -{1}", Properties.Resources.IDS_TIME, tempSpan.ToString(Properties.Resources.IDS_TIMEFORMAT));
					}
				}
			}
			else
			{
				posStr = Properties.Resources.IDS_NOPOSITION;
				subStr = Properties.Resources.IDS_NOSUBSONGS;
				timeStr = timeFormat == MainWindowSettings.TimeFormat.Elapsed ? Properties.Resources.IDS_NOTIME : Properties.Resources.IDS_NONEGATIVETIME;
			}

			infoLabel.Text = $"{posStr} {subStr} {timeStr}";
		}



		/********************************************************************/
		/// <summary>
		/// Will change the time depending on the position given
		/// </summary>
		/********************************************************************/
		private void SetPositionTime(int position)
		{
			// Set the new time
			TimeSpan newTime = moduleHandler.GetPositionTime(position);
			if (newTime.TotalMilliseconds != -1)
			{
				timeStart -= (newTime - timeOccurred);
				timeOccurred = newTime;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show the file dialog where the user can select the modules
		/// to put in the module list
		/// </summary>
		/********************************************************************/
		private DialogResult ShowModuleFileDialog()
		{
			// Create the dialog if not already created
			if (moduleFileDialog == null)
			{
				moduleFileDialog = new OpenFileDialog();
				moduleFileDialog.InitialDirectory = pathSettings.Modules;
				moduleFileDialog.Multiselect = true;
			}

			return moduleFileDialog.ShowDialog();
		}



		/********************************************************************/
		/// <summary>
		/// Will load and play the given module
		/// </summary>
		/********************************************************************/
		private void LoadAndPlayModule(ModuleListItem listItem)
		{
			bool ok = moduleHandler.LoadAndPlayModule(listItem);
			if (ok)
			{
				// Initialize other stuff in the window
				InitSubSongs();
			}

			// Mark the item in the list
			ChangePlayItem(ok ? listItem : null);

			// Initialize controls
			InitControls();

			// And refresh other windows
			RefreshWindows();
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the current playing module and free it
		/// </summary>
		/********************************************************************/
		private void StopAndFreeModule()
		{
			// Stop playing the module
			moduleHandler.StopAndFreeModule();

			// Stop the timers
			StopTimers();

			// Deselect the playing item
			ChangePlayItem(null);

			// Reset the position slider
			positionTrackBar.Value = 0;

			// Update the window controls
			InitControls();

			// Set the window title back to normal
			ShowNormalTitle();

			// Update the other windows
			RefreshWindows();
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the song given
		/// </summary>
		/********************************************************************/
		private void StartSong(int newSong)
		{
			// First stop any timers
			StopTimers();

			// Now tell the module handler to switch song
			moduleHandler.StartSong(newSong);

			// Initialize all the controls
			InitControls();
		}



		/********************************************************************/
		/// <summary>
		/// Will change the module position
		/// </summary>
		/********************************************************************/
		private void SetPosition(int newPosition)
		{
			if (playItem != null)
			{
				if (newPosition != moduleHandler.PlayingModuleInformation.SongPosition)
				{
					// Change the time to the position time
					SetPositionTime(newPosition);

					// Change the position
					moduleHandler.SetSongPosition(newPosition);
					prevSongPosition = newPosition;

					// Show it to the user
					PrintInfo();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will remove all the items from the module list
		/// </summary>
		/********************************************************************/
		private void EmptyList()
		{
			// Clear the module list
			moduleListBox.Items.Clear();

			// Clear the time variables
			listTime = new TimeSpan(0);
			selectedTime = new TimeSpan(0);

			// And update the window
			UpdateTimes();

			// Update the "number of files" label
			totalLabel.Text = "0/0";

			// Update the tape deck
			UpdateTapeDeck();

			// Clear the random list
			randomList.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Will start all the timers
		/// </summary>
		/********************************************************************/
		private void StartTimers(bool resetTimes)
		{
			// Start the playing timer
			if (resetTimes)
			{
				timeStart = DateTime.Now;
				timeOccurred = new TimeSpan(0);
			}

			clockTimer.Start();

			//XX Start the never ending timer if needed
		}



		/********************************************************************/
		/// <summary>
		/// Will stop all the timers
		/// </summary>
		/********************************************************************/
		private void StopTimers()
		{
			clockTimer.Stop();

			//XX Never ending timer
		}
		#endregion
	}
}

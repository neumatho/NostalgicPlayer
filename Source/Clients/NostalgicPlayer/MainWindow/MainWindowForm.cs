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
using Krypton.Toolkit;
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
		private readonly Random rnd;
		private readonly List<int> randomList;

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

			rnd = new Random();
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
				SetupControls();

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
						LoadAndPlayModule(index);
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
						LoadAndPlayModule(index);
					}
					else
					{
						// No
						if (count >= 1)
						{
							// Load the first module
							LoadAndPlayModule(0);
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
			moduleListBox.KeyPress += ModuleListBox_KeyPress;

			// Volume
			muteCheckButton.CheckedChanged += MuteCheckButton_CheckedChanged;
			masterVolumeTrackBar.ValueChanged += MasterVolumeTrackBar_ValueChanged;

			// List controls
			addModuleButton.Click += AddModuleButton_Click;
			removeModuleButton.Click += RemoveModuleButton_Click;
			swapModulesButton.Click += SwapModulesButton_Click;
			sortModulesButton.Click += SortModulesButton_Click;
			moveModulesUpButton.Click += MoveModulesUpButton_Click;
			moveModulesDownButton.Click += MoveModulesDownButton_Click;
			listButton.Click += ListButton_Click;

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

		#region Keyboard shortcuts
		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the main window
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Check for different keyboard shortcuts
			Keys modifiers = keyData & Keys.Modifiers;
			Keys key = keyData & Keys.KeyCode;

			if (modifiers == Keys.None)
			{
				// Keys without any modifiers
				switch (key)
				{
					// F12 - Play a random module
					case Keys.F12:
					{
						int total = moduleListBox.Items.Count;

						// Do only continue if we have more than one
						// module in the list
						if (total > 1)
						{
							// Find a random module until we found
							// one that is not the playing one
							int playingIndex = playItem == null ? -1 : moduleListBox.Items.IndexOf(playItem);
							int index;

							do
							{
								index = rnd.Next(total);
							}
							while ((index == playingIndex) || randomList.Contains(index));

							// Free any playing module
							StopAndFreeModule();

							// Now load the module
							LoadAndPlayModule(index);
						}

						return true;
					}

					// Delete - Eject
					case Keys.Delete:
					{
						ejectButton.PerformClick();
						return true;
					}

					// Insert - Play
					case Keys.Insert:
					{
						playButton.PerformClick();
						return true;
					}

					// Space - Pause
					case Keys.Space:
					{
						pauseCheckButton.PerformClick();
						return true;
					}

					// Left arrow - Rewind
					case Keys.Left:
					{
						rewindButton.PerformClick();
						return true;
					}

					// Right arrow - Fast forward
					case Keys.Right:
					{
						fastForwardButton.PerformClick();
						return true;
					}

					// Backspace - Module loop
					case Keys.Back:
					{
						loopCheckButton.PerformClick();
						return true;
					}
				}
			}
			else if (modifiers == Keys.Shift)
			{
				// Keys with shift pressed down
				switch (key)
				{
					// Space - Mute
					case Keys.Space:
					{
						muteCheckButton.PerformClick();
						return true;
					}

					// Left arrow - Load previous module
					case Keys.Left:
					{
						if (moduleListBox.Items.Count > 1)
							previousModuleButton.PerformClick();

						return true;
					}

					// Right arrow - Fast forward
					case Keys.Right:
					{
						if (moduleListBox.Items.Count > 1)
							nextModuleButton.PerformClick();

						return true;
					}
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion

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
					LoadAndPlayModule(newPlay);
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
				LoadAndPlayModule(index);
			}
		}



		/********************************************************************/
		/// <summary>
		/// User clicked on a key in the module list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Enter - Load the selected module
			if (e.KeyChar == '\r')
			{
				if (moduleListBox.SelectedItem != null)
				{
					// Stop playing any modules
					StopAndFreeModule();

					// Load and play the selected module
					LoadAndPlayModule((ModuleListItem)moduleListBox.SelectedItem);
				}
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

		#region List controls events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the add module button
		/// </summary>
		/********************************************************************/
		private void AddModuleButton_Click(object sender, EventArgs e)
		{
			if (ShowModuleFileDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
//XX				int jumpNumber;

					// Get the selected item
					int selected = moduleListBox.SelectedIndex;
					if (selected < 0)
					{
						selected = -1;
	//XX					jumpNumber = moduleListBox.Items.Count;
					}
	//XX				else
	//XX					jumpNumber = selected;

					// Add all the files in the module list
					AddFilesToList(moduleFileDialog.FileNames, selected);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the remove module button
		/// </summary>
		/********************************************************************/
		private void RemoveModuleButton_Click(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				RemoveSelectedItems();

				// Update the controls
				UpdateListCount();
				UpdateTimes();
				UpdateTapeDeck();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the swap modules button
		/// </summary>
		/********************************************************************/
		private void SwapModulesButton_Click(object sender, EventArgs e)
		{
			int index1 = moduleListBox.SelectedIndices[0];
			int index2 = moduleListBox.SelectedIndices[1];

			moduleListBox.BeginUpdate();

			try
			{
				// Swap the items
				ModuleListItem item1 = (ModuleListItem)moduleListBox.Items[index1];
				ModuleListItem item2 = (ModuleListItem)moduleListBox.Items[index2];

				moduleListBox.Items.RemoveAt(index1);
				moduleListBox.Items.Insert(index1, item2);

				moduleListBox.Items.RemoveAt(index2);
				moduleListBox.Items.Insert(index2, item1);

				// Keep the selection
				moduleListBox.SetSelected(index1, true);
				moduleListBox.SetSelected(index2, true);
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the sort modules button
		/// </summary>
		/********************************************************************/
		private void SortModulesButton_Click(object sender, EventArgs e)
		{
			// Show the menu
			sortContextMenu.Show(sender);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the move up button
		/// </summary>
		/********************************************************************/
		private void MoveModulesUpButton_Click(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				// Get the keyboard modifiers
				Keys modifiers = ModifierKeys;

				// Move the items
				if ((modifiers & Keys.Shift) != 0)
					MoveSelectedItemsToTop();
				else
					MoveSelectedItemsUp();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the move down button
		/// </summary>
		/********************************************************************/
		private void MoveModulesDownButton_Click(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				// Get the keyboard modifiers
				Keys modifiers = ModifierKeys;

				// Move the items
				if ((modifiers & Keys.Shift) != 0)
					MoveSelectedItemsToBottom();
				else
					MoveSelectedItemsDown();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the list button
		/// </summary>
		/********************************************************************/
		private void ListButton_Click(object sender, EventArgs e)
		{
			// Show the menu
			listContextMenu.Show(sender);
		}

		#region Sort menu events
		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the A-Z menu item
		/// </summary>
		/********************************************************************/
		private void SortMenu_AZ(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListBox.BeginUpdate();

				try
				{
					ModuleListItem[] newList = moduleListBox.Items.Cast<ModuleListItem>().OrderBy(i => i.ShortText).ToArray();

					moduleListBox.Items.Clear();
					moduleListBox.Items.AddRange(newList);
				}
				finally
				{
					moduleListBox.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the Z-A menu item
		/// </summary>
		/********************************************************************/
		private void SortMenu_ZA(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListBox.BeginUpdate();

				try
				{
					ModuleListItem[] newList = moduleListBox.Items.Cast<ModuleListItem>().OrderByDescending(i => i.ShortText).ToArray();

					moduleListBox.Items.Clear();
					moduleListBox.Items.AddRange(newList);
				}
				finally
				{
					moduleListBox.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the shuffle menu item
		/// </summary>
		/********************************************************************/
		private void SortMenu_Shuffle(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				ShuffleList();

				// If no module is playing, load the first one
				if (playItem == null)
					LoadAndPlayModule(0);
			}
		}
		#endregion

		#region Select menu events
		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the all menu item
		/// </summary>
		/********************************************************************/
		private void SelectMenu_All(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListBox.BeginUpdate();

				try
				{
					for (int i = moduleListBox.Items.Count - 1; i >= 0; i--)
						moduleListBox.SetSelected(i, true);
				}
				finally
				{
					moduleListBox.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the none menu item
		/// </summary>
		/********************************************************************/
		private void SelectMenu_None(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListBox.BeginUpdate();

				try
				{
					for (int i = moduleListBox.Items.Count - 1; i >= 0; i--)
						moduleListBox.SetSelected(i, false);
				}
				finally
				{
					moduleListBox.EndUpdate();
				}
			}
		}
		#endregion

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
					LoadAndPlayModule(0);
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
					LoadAndPlayModule(newIndex);
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
				LoadAndPlayModule(newIndex);
			}
		}
		#endregion

		#endregion

		#region Private methods

		/********************************************************************/
		/// <summary>
		/// Initialize the sub-songs variables
		/// </summary>
		/********************************************************************/
		private void InitSubSongs()
		{
			//XXsubSongMultiply = 0;
		}

		#region Settings
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
		#endregion

		#region Window
		/********************************************************************/
		/// <summary>
		/// Will setup the controls
		/// </summary>
		/********************************************************************/
		private void SetupControls()
		{
			// Set the window title
			ShowNormalTitle();

			// Create the menu
			CreateMainMenu();

			// Create button menus
			CreateSortMenu();
			CreateListMenu();

			// Set tooltip on all controls
			SetTooltips();

			// Update the list controls
			UpdateListControls();

			// Update the tape deck buttons
			UpdateTapeDeck();

			// Print the module information
			PrintInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Creates the main menu
		/// </summary>
		/********************************************************************/
		private void CreateMainMenu()
		{
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
		}



		/********************************************************************/
		/// <summary>
		/// Create the sort button context menu
		/// </summary>
		/********************************************************************/
		private void CreateSortMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Properties.Resources.IDS_SORTMENU_SORT_AZ);
			item.Image = Properties.Resources.IDB_AZ;
			item.Click += SortMenu_AZ;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Properties.Resources.IDS_SORTMENU_SORT_ZA);
			item.Image = Properties.Resources.IDB_ZA;
			item.Click += SortMenu_ZA;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Properties.Resources.IDS_SORTMENU_SHUFFLE);
			item.Image = Properties.Resources.IDB_SHUFFLE;
			item.Click += SortMenu_Shuffle;
			menuItems.Items.Add(item);

			sortContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Create the list button context menu
		/// </summary>
		/********************************************************************/
		private void CreateListMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Properties.Resources.IDS_SELECTMENU_ALL);
			item.Click += SelectMenu_All;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Properties.Resources.IDS_SELECTMENU_NONE);
			item.Click += SelectMenu_None;
			menuItems.Items.Add(item);

			listContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Set tooltip on all the controls
		/// </summary>
		/********************************************************************/
		private void SetTooltips()
		{
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
		#endregion

		#region Control updating
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
		/// Will subtract the time from the item given from the list time
		/// </summary>
		/********************************************************************/
		private void RemoveItemTimeFromList(ModuleListItem listItem)
		{
			// Subtract the item time
			if (listItem.HaveTime)
				listTime -= listItem.Time;
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
		#endregion

		#region Module handling
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
		/// Will load and play the given module
		/// </summary>
		/********************************************************************/
		private void LoadAndPlayModule(int index)
		{
			LoadAndPlayModule((ModuleListItem)moduleListBox.Items[index]);
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
		#endregion

		#region File handling
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
		#endregion

		#region Timers
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

		#region Module list methods
		/********************************************************************/
		/// <summary>
		/// Will add all the given files to the module list
		/// </summary>
		/********************************************************************/
		private void AddFilesToList(string[] files, int startIndex = -1)
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
		/// Will remove all the selected items from the list
		/// </summary>
		/********************************************************************/
		private void RemoveSelectedItems()
		{
			moduleListBox.BeginUpdate();

			try
			{
				// Remove all the selected module items
				foreach (int index in moduleListBox.SelectedIndices.Cast<int>().Reverse())	// Take the items in reverse order, which is done via a copy of the selected items
				{
					ModuleListItem listItem = (ModuleListItem)moduleListBox.Items[index];

					// If the item is the one that is playing, stop it
					if (listItem.IsPlaying)
					{
						playItem = null;

						StopAndFreeModule();
						moduleHandler.FreeAllModules();
					}

					// Subtract the item time from the list
					RemoveItemTimeFromList(listItem);

					moduleListBox.Items.Remove(listItem);
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items one index up
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsUp()
		{
			moduleListBox.BeginUpdate();

			try
			{
				int previousSelected = -1;
				bool previousMoved = false;

				foreach (int selected in moduleListBox.SelectedIndices)
				{
					if ((selected > 0) && (((selected - 1) != previousSelected) || previousMoved))
					{
						MoveItem(selected, selected - 1);
						previousMoved = true;
					}
					else
						previousMoved = false;

					previousSelected = selected;
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items one index up
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsDown()
		{
			moduleListBox.BeginUpdate();

			try
			{
				int previousSelected = -1;
				bool previousMoved = false;
				int listCount = moduleListBox.Items.Count;

				foreach (int selected in moduleListBox.SelectedIndices.Cast<int>().Reverse())	// Take the items in reverse order, which is done via a copy of the selected items
				{
					if (((selected + 1) < listCount) && (((selected + 1) != previousSelected) || previousMoved))
					{
						MoveItem(selected, selected + 1);
						previousMoved = true;
					}
					else
						previousMoved = false;

					previousSelected = selected;
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items to the top of the list
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsToTop()
		{
			moduleListBox.BeginUpdate();

			try
			{
				// Move all the items
				int index = 0;

				foreach (int selected in moduleListBox.SelectedIndices)
				{
					MoveItem(selected, index);
					index++;
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items to the bottom of the list
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsToBottom()
		{
			moduleListBox.BeginUpdate();

			try
			{
				// Move all the items
				int listCount = moduleListBox.Items.Count;
				int index = 0;

				foreach (int selected in moduleListBox.SelectedIndices.Cast<int>().Reverse())	// Take the items in reverse order, which is done via a copy of the selected items
				{
					MoveItem(selected, listCount - index - 1);
					index++;
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will shuffle the entire list
		/// </summary>
		/********************************************************************/
		private void ShuffleList()
		{
			moduleListBox.BeginUpdate();

			try
			{
				// Get the number of items
				int total = moduleListBox.Items.Count;

				// Do we have enough items in the list?
				if (total > 1)
				{
					// Create a new resulting list
					List<ModuleListItem> newList = new List<ModuleListItem>();

					// Make a copy of all the items in the list
					List<ModuleListItem> tempList = new List<ModuleListItem>(moduleListBox.Items.Cast<ModuleListItem>());

					// Well, if a module is playing, we want to
					// place that module in the top of the list
					if (playItem != null)
					{
						// Find the item index
						int index = moduleListBox.Items.IndexOf(playItem);

						// Move the item to the new list
						newList.Add(tempList[index]);
						tempList.RemoveAt(index);

						// One item less to shuffle
						total--;
					}

					// Ok, now it's time to shuffle
					for (; total > 0; total--)
					{
						// Find a random item and add it to the new list +
						// remove it from the old one
						int index = rnd.Next(total);

						// Move the item to the new list
						newList.Add(tempList[index]);
						tempList.RemoveAt(index);
					}

					// Copy the new list into the list control
					moduleListBox.Items.Clear();
					moduleListBox.Items.AddRange(newList.ToArray());
				}
			}
			finally
			{
				moduleListBox.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move a item in the list
		/// </summary>
		/********************************************************************/
		private void MoveItem(int from, int to)
		{
			ModuleListItem listItem = (ModuleListItem)moduleListBox.Items[from];
			moduleListBox.Items.RemoveAt(from);
			moduleListBox.Items.Insert(to, listItem);

			// Keep selection
			moduleListBox.SetSelected(to, true);
		}
		#endregion

		#endregion
	}
}

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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AboutWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AgentWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow;
using Polycode.NostalgicPlayer.GuiKit.Controls;
using Polycode.NostalgicPlayer.Kit.Containers;
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
		private enum FileDropType
		{
			ClearAndAdd,
			Append,
			Insert
		}

		private class AgentEntry
		{
			public Guid TypeId;
			public bool Enabled;
		}

		private Manager agentManager;
		private ModuleHandler moduleHandler;

		private Settings userSettings;
		private PathSettings pathSettings;
		private SoundSettings soundSettings;

		private MainWindowSettings mainWindowSettings;

		private OpenFileDialog moduleFileDialog;
		private OpenFileDialog loadListFileDialog;
		private SaveFileDialog saveListFileDialog;

		// Menus
		private ToolStripSeparator agentSettingsSeparatorMenuItem;
		private ToolStripMenuItem agentSettingsMenuItem;
		private ToolStripMenuItem agentShowMenuItem;

		// Timer variables
		private MainWindowSettings.TimeFormat timeFormat;
		private DateTime timeStart;
		private TimeSpan timeOccurred;

		// Module variables
		private ModuleListItem playItem;
		private TimeSpan songTotalTime;
		private int subSongMultiply;

		// Information variables
		private int prevSongPosition;

		// List times
		private TimeSpan listTime;
		private TimeSpan selectedTime;

		// Window/control status variables
		private bool allowPosSliderUpdate;

		// Drag'n'drop variables
		private int indexOfItemUnderMouseToDrop = -2;
		private Rectangle dragBoxFromMouseDown;
		private bool drawLine;
		private FileDropType dropType;

		private bool restoreSelection = false;
		private int[] savedSelection = new int[0];

		// Misc.
		private readonly Random rnd;
		private readonly List<int> randomList;

		// Other windows
		private AboutWindowForm aboutWindow = null;
		private SettingsWindowForm settingsWindow = null;
		private ModuleInfoWindowForm moduleInfoWindow = null;
		private SampleInfoWindowForm sampleInfoWindow = null;

		private readonly Dictionary<Guid, AgentSettingsWindowForm> openAgentSettings;
		private readonly Dictionary<Guid, AgentDisplayWindowForm> openAgentDisplays;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainWindowForm()
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			positionTrackBar.BackStyle = PaletteBackStyle.SeparatorLowProfile;

			// Enable drag'n'drop
			moduleListBox.ListBox.AllowDrop = true;

			// Disable escape key closing
			disableEscapeKey = true;

			// Initialize member variables
			playItem = null;
			allowPosSliderUpdate = true;

			rnd = new Random();
			randomList = new List<int>();

			openAgentSettings = new Dictionary<Guid, AgentSettingsWindowForm>();
			openAgentDisplays = new Dictionary<Guid, AgentDisplayWindowForm>();

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
		/// Add a single agent to the menu if needed
		/// </summary>
		/********************************************************************/
		public void AddAgentToMenu(AgentInfo agentInfo)
		{
			if (agentInfo.HasSettings)
			{
				// Create the menu item
				ToolStripMenuItem newMenuItem = new ToolStripMenuItem(agentInfo.TypeName);
				newMenuItem.Tag = agentInfo;
				newMenuItem.Click += Menu_Window_AgentSettings_Click;

				// Find the right place in the menu to insert the agent
				int insertPos = 0;

				foreach (ToolStripMenuItem menuItem in agentSettingsMenuItem.DropDownItems)
				{
					if (agentInfo.TypeName.CompareTo(menuItem.Name) > 0)
						break;

					insertPos++;
				}

				agentSettingsMenuItem.DropDownItems.Insert(insertPos, newMenuItem);
			}

			if (agentInfo.HasDisplay)
			{
				// Create the menu item
				ToolStripMenuItem newMenuItem = new ToolStripMenuItem(agentInfo.TypeName);
				newMenuItem.Tag = agentInfo;
				newMenuItem.Click += Menu_Window_AgentDisplay_Click;

				// Find the right place in the menu to insert the agent
				int insertPos = 0;

				foreach (ToolStripMenuItem menuItem in agentShowMenuItem.DropDownItems)
				{
					if (agentInfo.TypeName.CompareTo(menuItem.Name) > 0)
						break;

					insertPos++;
				}

				agentShowMenuItem.DropDownItems.Insert(insertPos, newMenuItem);
			}

			// Hide/show different menu items
			agentSettingsMenuItem.Visible = agentSettingsMenuItem.DropDownItems.Count > 0;
			agentShowMenuItem.Visible = agentShowMenuItem.DropDownItems.Count > 0;
			agentSettingsSeparatorMenuItem.Visible = agentSettingsMenuItem.DropDownItems.Count > 0 || agentShowMenuItem.DropDownItems.Count > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Remove a single agent from the menu if needed
		/// </summary>
		/********************************************************************/
		public void RemoveAgentFromMenu(AgentInfo agentInfo)
		{
			if (agentInfo.HasSettings)
			{
				// Find the agent menu
				for (int i = agentSettingsMenuItem.DropDownItems.Count - 1; i >= 0; i--)
				{
					if (((AgentInfo)agentSettingsMenuItem.DropDownItems[i].Tag).TypeId == agentInfo.TypeId)
					{
						// Found it, so remove it
						agentSettingsMenuItem.DropDownItems.RemoveAt(i);
						break;
					}
				}
			}

			if (agentInfo.HasDisplay)
			{
				// Find the agent menu
				for (int i = agentShowMenuItem.DropDownItems.Count - 1; i >= 0; i--)
				{
					if (((AgentInfo)agentShowMenuItem.DropDownItems[i].Tag).TypeId == agentInfo.TypeId)
					{
						// Found it, so remove it
						agentShowMenuItem.DropDownItems.RemoveAt(i);
						break;
					}
				}
			}

			// Hide/show different menu items
			agentSettingsMenuItem.Visible = agentSettingsMenuItem.DropDownItems.Count > 0;
			agentShowMenuItem.Visible = agentShowMenuItem.DropDownItems.Count > 0;
			agentSettingsSeparatorMenuItem.Visible = agentSettingsMenuItem.DropDownItems.Count > 0 || agentShowMenuItem.DropDownItems.Count > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		public void ShowSimpleErrorMessage(string message)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(message, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error))
			{
				dialog.AddButton(Resources.IDS_BUT_OK, 'O');
				dialog.ShowDialog();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		public void ShowErrorMessage(string message, ModuleListItem listItem)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(message, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error))
			{
				dialog.AddButton(Resources.IDS_BUT_SKIP, 'S');
				dialog.AddButton(Resources.IDS_BUT_SKIPREMOVE, 'r');
				dialog.AddButton(Resources.IDS_BUT_STOP, 'p');
				dialog.ShowDialog();
				char response = dialog.GetButtonResult();

				switch (response)
				{
					// Skip
					case 'S':
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
					case 'r':
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
					case 'p':
					{
						// Deselect the playing flag
						ChangePlayItem(null);
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return some information about the current playing file
		/// </summary>
		/********************************************************************/
		public MultiFileInfo GetFileInfo()
		{
			return playItem == null ? null : ListItemConverter.Convert(playItem);
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the current playing module and free it
		/// </summary>
		/********************************************************************/
		public void StopModule()
		{
			StopAndFreeModule();
		}

		#region Agent window handling
		/********************************************************************/
		/// <summary>
		/// Will open/activate a settings window for the given agent
		/// </summary>
		/********************************************************************/
		public void OpenAgentSettingsWindow(AgentInfo agentInfo)
		{
			lock (openAgentSettings)
			{
				if (openAgentSettings.TryGetValue(agentInfo.TypeId, out AgentSettingsWindowForm agentSettingsWindow))
				{
					if (agentSettingsWindow.WindowState == FormWindowState.Minimized)
						agentSettingsWindow.WindowState = FormWindowState.Normal;

					agentSettingsWindow.Activate();
				}
				else
				{
					agentSettingsWindow = new AgentSettingsWindowForm(agentInfo);
					agentSettingsWindow.Disposed += (o, args) => { openAgentSettings.Remove(agentInfo.TypeId); };
					agentSettingsWindow.Show();

					openAgentSettings[agentInfo.TypeId] = agentSettingsWindow;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will close a settings window for the given agent
		/// </summary>
		/********************************************************************/
		public void CloseAgentSettingsWindow(AgentInfo agentInfo)
		{
			lock (openAgentSettings)
			{
				if (openAgentSettings.TryGetValue(agentInfo.TypeId, out AgentSettingsWindowForm agentSettingsWindow))
				{
					agentSettingsWindow.Close();

					openAgentSettings.Remove(agentInfo.TypeId);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will open/activate a display window for the given agent
		/// </summary>
		/********************************************************************/
		public void OpenAgentDisplayWindow(AgentInfo agentInfo)
		{
			lock (openAgentDisplays)
			{
				if (openAgentDisplays.TryGetValue(agentInfo.TypeId, out AgentDisplayWindowForm agentDisplayWindow))
				{
					if (agentDisplayWindow.WindowState == FormWindowState.Minimized)
						agentDisplayWindow.WindowState = FormWindowState.Normal;

					agentDisplayWindow.Activate();
				}
				else
				{
					agentDisplayWindow = new AgentDisplayWindowForm(agentManager, agentInfo, moduleHandler);
					agentDisplayWindow.Disposed += (o, args) => { openAgentDisplays.Remove(agentInfo.TypeId); };
					agentDisplayWindow.Show();

					openAgentDisplays[agentInfo.TypeId] = agentDisplayWindow;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will close a display window for the given agent
		/// </summary>
		/********************************************************************/
		public void CloseAgentDisplayWindow(AgentInfo agentInfo)
		{
			lock (openAgentDisplays)
			{
				if (openAgentDisplays.TryGetValue(agentInfo.TypeId, out AgentDisplayWindowForm agentDisplayWindow))
				{
					agentDisplayWindow.Close();

					openAgentDisplays.Remove(agentInfo.TypeId);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get all opened visuals
		/// </summary>
		/********************************************************************/
		public IEnumerable<AgentDisplayWindowForm> GetAllOpenedVisuals()
		{
			lock (openAgentDisplays)
			{
				foreach (AgentDisplayWindowForm agentDisplayWindow in openAgentDisplays.Values)
					yield return  agentDisplayWindow;
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Setup event handlers
		/// </summary>
		/********************************************************************/
		private void SetupHandlers()
		{
			// Form
			Load += MainWindowForm_Load;
			Resize += MainWindowForm_Resize;
			FormClosed += MainWindowForm_FormClosed;

			// Module information
			infoGroup.Panel.Click += InfoGroup_Click;
			infoLabel.Click += InfoGroup_Click;
			clockTimer.Tick += ClockTimer_Tick;

			moduleInfoButton.Click += ModuleInfoButton_Click;

			// Module list
			moduleListBox.SelectedIndexChanged += ModuleListBox_SelectedIndexChanged;
			moduleListBox.ListBox.MouseDoubleClick += ModuleListBox_MouseDoubleClick;
			moduleListBox.KeyPress += ModuleListBox_KeyPress;

			moduleListBox.ListBox.MouseDown += ModuleListBox_MouseDown;
			moduleListBox.ListBox.MouseUp += ModuleListBox_MouseUp;
			moduleListBox.ListBox.MouseMove += ModuleListBox_MouseMove;
			moduleListBox.ListBox.DragEnter += ModuleListBox_DragEnter;
			moduleListBox.ListBox.DragOver += ModuleListBox_DragOver;
			moduleListBox.ListBox.DragLeave += ModuleListBox_DragLeave;
			moduleListBox.ListBox.DragDrop += ModuleListBox_DragDrop;
			scrollTimer.Tick += ScrollTimer_Tick;

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
			diskButton.Click += DiskButton_Click;

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

			previousSongButton.Click += PreviousSongButton_Click;
			nextSongButton.Click += NextSongButton_Click;

			previousModuleButton.Click += PreviousModuleButton_Click;
			nextModuleButton.Click += NextModuleButton_Click;

			// Loop/sample group
			sampleInfoButton.Click += SampleInfoButton_Click;

			// Module handler
			moduleHandler.PositionChanged += ModuleHandler_PositionChanged;
			moduleHandler.EndReached += ModuleHandler_EndReached;
			moduleHandler.ModuleInfoChanged += ModuleHandler_ModuleInfoChanged;
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

					// Sub-song 1
					case Keys.D1:
					case Keys.NumPad1:
					{
						SwitchSubSong(0);
						return true;
					}

					// Sub-song 2
					case Keys.D2:
					case Keys.NumPad2:
					{
						SwitchSubSong(1);
						return true;
					}

					// Sub-song 3
					case Keys.D3:
					case Keys.NumPad3:
					{
						SwitchSubSong(2);
						return true;
					}

					// Sub-song 4
					case Keys.D4:
					case Keys.NumPad4:
					{
						SwitchSubSong(3);
						return true;
					}

					// Sub-song 5
					case Keys.D5:
					case Keys.NumPad5:
					{
						SwitchSubSong(4);
						return true;
					}

					// Sub-song 6
					case Keys.D6:
					case Keys.NumPad6:
					{
						SwitchSubSong(5);
						return true;
					}

					// Sub-song 7
					case Keys.D7:
					case Keys.NumPad7:
					{
						SwitchSubSong(6);
						return true;
					}

					// Sub-song 8
					case Keys.D8:
					case Keys.NumPad8:
					{
						SwitchSubSong(7);
						return true;
					}

					// Sub-song 9
					case Keys.D9:
					case Keys.NumPad9:
					{
						SwitchSubSong(8);
						return true;
					}

					// Sub-song 10
					case Keys.D0:
					case Keys.NumPad0:
					{
						SwitchSubSong(9);
						return true;
					}

					// Add 10 to the sub-song selector
					case Keys.Oemplus:
					case Keys.Add:
					{
						if (playItem != null)
						{
							int maxSong = moduleHandler.StaticModuleInformation.MaxSongNumber;

							// Check for out of bounds
							if ((subSongMultiply + 10) < maxSong)
								subSongMultiply += 10;
						}

						return true;
					}

					// Subtract 10 from the sub-song selector
					case Keys.OemMinus:
					case Keys.Subtract:
					{
						if (playItem != null)
						{
							// Check for out of bounds
							if (subSongMultiply != 0)
								subSongMultiply -= 10;
						}

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
		/// Is called when the form has been loaded
		/// </summary>
		/********************************************************************/
		private void MainWindowForm_Load(object sender, EventArgs e)
		{
			RetrieveStartupFiles();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the window is resize. Used to detect if minimized
		/// </summary>
		/********************************************************************/
		private void MainWindowForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				if (IsAboutWindowOpen())
					aboutWindow.Close();

				if (IsModuleInfoWindowOpen())
				{
					moduleInfoWindow.UpdateWindowSettings();
					moduleInfoWindow.WindowState = FormWindowState.Minimized;
				}

				if (IsSampleInfoWindowOpen())
				{
					sampleInfoWindow.UpdateWindowSettings();
					sampleInfoWindow.WindowState = FormWindowState.Minimized;
				}

				if (IsSettingsWindowOpen())
				{
					settingsWindow.UpdateWindowSettings();
					settingsWindow.WindowState = FormWindowState.Minimized;
				}

				// Minimize all agent settings windows
				lock (openAgentSettings)
				{
					foreach (AgentSettingsWindowForm agentSettingsWindow in openAgentSettings.Values)
					{
						agentSettingsWindow.UpdateWindowSettings();
						agentSettingsWindow.WindowState = FormWindowState.Minimized;
					}
				}

				// Minimize all agent display windows
				lock (openAgentDisplays)
				{
					foreach (AgentDisplayWindowForm agentDisplayWindow in openAgentDisplays.Values)
					{
						agentDisplayWindow.UpdateWindowSettings();
						agentDisplayWindow.WindowState = FormWindowState.Minimized;
					}
				}
			}
			else if (WindowState == FormWindowState.Normal)
			{
				if (IsModuleInfoWindowOpen())
					moduleInfoWindow.WindowState = FormWindowState.Normal;

				if (IsSampleInfoWindowOpen())
					sampleInfoWindow.WindowState = FormWindowState.Normal;

				if (IsSettingsWindowOpen())
					settingsWindow.WindowState = FormWindowState.Normal;

				// Show all agent settings windows
				lock (openAgentSettings)
				{
					foreach (AgentSettingsWindowForm agentSettingsWindow in openAgentSettings.Values)
						agentSettingsWindow.WindowState = FormWindowState.Normal;
				}

				// Show all agent display windows
				lock (openAgentDisplays)
				{
					foreach (AgentDisplayWindowForm agentDisplayWindow in openAgentDisplays.Values)
						agentDisplayWindow.WindowState = FormWindowState.Normal;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/

		private void MainWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Stop any playing modules
			StopAndFreeModule();

			// Close all windows
			CloseWindows();

			// Stop the module handler
			CleanupModuleHandler();

			// Save and cleanup the settings
			CleanupSettings();
		}
		#endregion

		#region Menu events

		#region Window menu
		/********************************************************************/
		/// <summary>
		/// User selected the settings menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Window_Settings_Click(object sender, EventArgs e)
		{
			if (IsSettingsWindowOpen())
				settingsWindow.Activate();
			else
			{
				settingsWindow = new SettingsWindowForm(agentManager, moduleHandler, this, userSettings);
				settingsWindow.Disposed += (o, args) => { settingsWindow = null; };
				settingsWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// User selected one of the agent settings menu items
		/// </summary>
		/********************************************************************/
		private void Menu_Window_AgentSettings_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = (AgentInfo)((ToolStripMenuItem)sender).Tag;
			OpenAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// User selected one of the agent display menu items
		/// </summary>
		/********************************************************************/
		private void Menu_Window_AgentDisplay_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = (AgentInfo)((ToolStripMenuItem)sender).Tag;
			OpenAgentDisplayWindow(agentInfo);
		}
		#endregion

		#region Help menu
		/********************************************************************/
		/// <summary>
		/// User selected the help menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Help_Help_Click(object sender, EventArgs e)
		{
			using (HelpWindowForm dialog = new HelpWindowForm())
			{
				dialog.ShowDialog();
			}
		}



		/********************************************************************/
		/// <summary>
		/// User selected the about menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Help_About_Click(object sender, EventArgs e)
		{
			if (IsAboutWindowOpen())
				aboutWindow.Activate();
			else
			{
				aboutWindow = new AboutWindowForm(agentManager);
				aboutWindow.Disposed += (o, args) => { aboutWindow = null; };
				aboutWindow.Show();
			}
		}
		#endregion

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
		/// Is called when the user clicks on the module information button
		/// </summary>
		/********************************************************************/
		private void ModuleInfoButton_Click(object sender, EventArgs e)
		{
			if (IsModuleInfoWindowOpen())
			{
				if (moduleInfoWindow.WindowState == FormWindowState.Minimized)
					moduleInfoWindow.WindowState = FormWindowState.Normal;

				moduleInfoWindow.Activate();
			}
			else
			{
				moduleInfoWindow = new ModuleInfoWindowForm(moduleHandler, this);
				moduleInfoWindow.Disposed += (o, args) => { moduleInfoWindow = null; };
				moduleInfoWindow.Show();
			}
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

					// Free the module
					StopAndFreeModule();

					// The next module to load
					int newPlay = curPlay + 1;

					// Test to see if we is at the end of the list
					if (newPlay < count)
					{
						// Load the module
						LoadAndPlayModule(newPlay);
					}
				}
			}));
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			BeginInvoke(new Action(() =>
			{
				if (IsModuleInfoWindowOpen())
					moduleInfoWindow.UpdateWindow(e.Line, e.Value);
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

			if (!restoreSelection)
				SaveSelection();
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

		#region Drag'n'drop
		/********************************************************************/
		/// <summary>
		/// User clicked in the module list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_MouseDown(object sender, MouseEventArgs e)
		{
			// If no items is selected, then do not start drag'n'drop
			if (moduleListBox.SelectedIndex != -1)
			{
				int clickedItemIndex = moduleListBox.IndexFromPoint(e.Location);
				if ((clickedItemIndex >= 0) && ((e.Button & MouseButtons.Left) != 0) && (moduleListBox.GetSelected(clickedItemIndex) /*|| (ModifierKeys == Keys.Shift)*/))	// Shift test is removed, because there is some issue that the selection collection isn't updated when selecting items with shift and begin to drag without release the mouse button first
				{
					RestoreSelection(clickedItemIndex);

					// Remember the point where the mouse down occurred. The DragSize
					// indicates the size that the mouse can move before a drag event
					// should be started
					Size dragSize = SystemInformation.DragSize;

					// Create a rectangle using DragSize, with the mouse position
					// being at the center of the rectangle
					dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
				}
				else
					dragBoxFromMouseDown = Rectangle.Empty;
			}
			else
			{
				// Reset the rectangle if no item is selected
				dragBoxFromMouseDown = Rectangle.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// User released the mouse in the module list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_MouseUp(object sender, MouseEventArgs e)
		{
			dragBoxFromMouseDown = Rectangle.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// User moves the mouse around in the module list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_MouseMove(object sender, MouseEventArgs e)
		{
			// If the mouse moves outside the rectangle, start the drag
			if ((dragBoxFromMouseDown != Rectangle.Empty) && !dragBoxFromMouseDown.Contains(e.X, e.Y))
			{
				// Start the dragging, where custom data is all the selected items.
				// Also make a copy of the collection
				moduleListBox.DoDragDrop(this, DragDropEffects.Move);

				// Stop drag functionality by clearing the rectangle
				dragBoxFromMouseDown = Rectangle.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse enters the list control and a dragging
		/// begins
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_DragEnter(object sender, DragEventArgs e)
		{
			indexOfItemUnderMouseToDrop = -2;

			// Start the timer
			scrollTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse leaves the list control
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_DragLeave(object sender, EventArgs e)
		{
			DrawLine(true);

			// Stop the timer again
			scrollTimer.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Should set if it is valid to drag into the list box
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(GetType()))
			{
				// Drag started from our own list, so it is ok
				e.Effect = DragDropEffects.Move;
				drawLine = true;
			}
			else if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Either a file or directory is dropped from File Explorer
				if ((ModifierKeys & Keys.Control) != 0)
				{
					if ((ModifierKeys & Keys.Shift) != 0)
					{
						e.Effect = DragDropEffects.Copy;		// Append to list
						dropType = FileDropType.Append;
						drawLine = false;
					}
					else
					{
						e.Effect = DragDropEffects.Move;		// Insert into position in list
						dropType = FileDropType.Insert;
						drawLine = true;
					}
				}
				else
				{
					e.Effect = DragDropEffects.Move;			// Clear list and add files
					dropType = FileDropType.ClearAndAdd;
					drawLine = false;
				}
			}
			else
			{
				// Unknown type, so it is not allowed
				e.Effect = DragDropEffects.None;
			}

			if (e.Effect != DragDropEffects.None)
			{
				// Remember the index where the drop will occur
				Point clientPoint = moduleListBox.PointToClient(new Point(e.X, e.Y));
				int index = moduleListBox.IndexFromPoint(clientPoint);

				// Hotfix, because of a bug in IndexFromPoint()
				if (index == 65535)
					index = -1;

				// Because Krypton ListBox control uses OwnerDrawVariable when
				// drawing the control, the above IndexFromPoint will always
				// return the last item, even if the mouse is over the empty area.
				// https://stackoverflow.com/questions/48387671/ownerdrawvariable-listbox-selects-last-item-when-clicking-on-control-below-items
				//
				// We try to make a work-around for this
				if ((index == moduleListBox.Items.Count - 1) && (index != -1))      // It would be -1, if the list is empty
				{
					// Get the rectangle for the last item
					Rectangle lastItemRect = moduleListBox.GetItemRectangle(moduleListBox.Items.Count - 1);

					// Is the mouse inside this rect?
					if (!lastItemRect.Contains(clientPoint))
					{
						// No, then the mouse is over the empty area
						index = -1;
					}
				}

				// If the drop point has changed, redraw the line
				if (index != indexOfItemUnderMouseToDrop)
				{
					// First erase the old line
					DrawLine(true);

					indexOfItemUnderMouseToDrop = index;

					// Then draw the new one
					DrawLine(false);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the drop ends
		/// </summary>
		/********************************************************************/
		private void ModuleListBox_DragDrop(object sender, DragEventArgs e)
		{
			// Stop the timer again
			scrollTimer.Stop();

			using (new SleepCursor())
			{
				if (e.Data.GetDataPresent(GetType()))
				{
					// Moving list items around
					//
					// Get the selected items and order them in reverse
					var selectedItems = moduleListBox.SelectedIndices.Cast<int>().OrderByDescending(i => i);

					if (indexOfItemUnderMouseToDrop == -1)
					{
						int insertAt = moduleListBox.Items.Count;

						foreach (int index in selectedItems)
						{
							ModuleListItem listItem = (ModuleListItem)moduleListBox.Items[index];

							moduleListBox.Items.Insert(insertAt--, listItem);
							moduleListBox.Items.RemoveAt(index);
						}
					}
					else
					{
						int insertAt = indexOfItemUnderMouseToDrop;

						foreach (int index in selectedItems)
						{
							ModuleListItem listItem = (ModuleListItem)moduleListBox.Items[index];

							moduleListBox.Items.Insert(insertAt, listItem);

							if (index < insertAt)
								moduleListBox.Items.RemoveAt(index);
							else
								moduleListBox.Items.RemoveAt(index + 1);
						}
					}
				}
				else if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					// File or directory dragged from File Explorer
					string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

					switch (dropType)
					{
						case FileDropType.Append:
						{
							AddFilesToList(files, checkForList: true);
							break;
						}

						case FileDropType.ClearAndAdd:
						{
							StopAndFreeModule();
							EmptyList();

							AddFilesToList(files, checkForList: true);

							LoadAndPlayModule(0);
							break;
						}

						case FileDropType.Insert:
						{
							AddFilesToList(files, indexOfItemUnderMouseToDrop, true);
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called by an interval to check if a scroll is needed
		/// </summary>
		/********************************************************************/
		private void ScrollTimer_Tick(object sender, EventArgs e)
		{
			if (moduleListBox.Items.Count > 0)
			{
				int y = moduleListBox.PointToClient(MousePosition).Y;

				if ((moduleListBox.Height - y) <= 10)
				{
					int itemsThatCanBeSeen = moduleListBox.Height / moduleListBox.GetItemHeight(0);
					if (moduleListBox.TopIndex + itemsThatCanBeSeen < moduleListBox.Items.Count)
					{
						DrawLine(true);
						moduleListBox.TopIndex++;
					}
				}
				else if (y <= 10)
				{
					if (moduleListBox.TopIndex > 0)
					{
						DrawLine(true);
						moduleListBox.TopIndex--;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw a line at the place where the drop will happen
		/// </summary>
		/********************************************************************/
		private void DrawLine(bool erase)
		{
			if (erase || drawLine)
			{
				using (Graphics g = moduleListBox.ListBox.CreateGraphics())
				{
					// Find the position where to draw the line
					int pos;

					int count = moduleListBox.Items.Count;

					int indexCheck;

					// If no items in the list or it is the first position to insert,
					// just draw the line at top of the control
					int indexFromTop = indexOfItemUnderMouseToDrop - moduleListBox.TopIndex;

					if ((count == 0) || (indexFromTop == 0))
					{
						pos = 0;
						indexCheck = -1;
					}
					else
					{
						int height = moduleListBox.GetItemHeight(0);

						// Do we point at any item?
						if (indexOfItemUnderMouseToDrop < 0)
						{
							pos = (count - moduleListBox.TopIndex) * height - 1;
							indexCheck = count - 1;
						}
						else
						{
							pos = indexFromTop * height - 1;
							indexCheck = indexOfItemUnderMouseToDrop - 1;
						}
					}

					if (erase && moduleListBox.SelectedIndices.Contains(indexCheck))
					{
						Rectangle itemRect = moduleListBox.GetItemRectangle(indexCheck);

						// This is hardcoded to draw how Krypton list box draw a selected item
						using (SolidBrush corner1 = new SolidBrush(Color.FromArgb(251, 249, 244)))
						{
							using (SolidBrush corner2 = new SolidBrush(Color.FromArgb(215, 193, 136)))
							{
								using (SolidBrush corner3 = new SolidBrush(Color.FromArgb(208, 181, 113)))
								{
									using (SolidBrush corner4 = new SolidBrush(Color.FromArgb(247, 243, 232)))
									{
										using (Pen line = new Pen(Color.FromArgb(194, 160, 73)))
										{
											g.FillRectangle(corner1, itemRect.Left, pos, 1, 1);
											g.FillRectangle(corner2, itemRect.Left + 1, pos, 1, 1);
											g.DrawLine(line, itemRect.Left + 2, pos, itemRect.Right - 3, pos);
											g.FillRectangle(corner3, itemRect.Right - 2, pos, 1, 1);
											g.FillRectangle(corner4, itemRect.Right - 1, pos, 1, 1);
										}
									}
								}
							}
						}
					}
					else
					{
						// Draw the line
						Rectangle rect = indexCheck < 0 ? moduleListBox.ClientRectangle : moduleListBox.GetItemRectangle(indexCheck);

						using (Pen pen = new Pen(erase ? Color.White : Color.CornflowerBlue))
						{
							g.DrawLine(pen, rect.Left, pos, rect.Right, pos);
						}
					}
				}
			}
		}


		// This saving and restoring stuff of the selected items, is made
		// so the list box behaves like the list view, when the user selects
		// the item. See https://www.codeproject.com/Articles/36412/Drag-and-Drop-ListBox

		/********************************************************************/
		/// <summary>
		/// Will save the current selected items
		/// </summary>
		/********************************************************************/
		private void SaveSelection()
		{
			savedSelection = moduleListBox.SelectedIndices.Cast<int>().ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore the saved selection back again
		/// </summary>
		/********************************************************************/
		private void RestoreSelection(int clickedItemIndex)
		{
			if ((ModifierKeys == Keys.None) && savedSelection.Contains(clickedItemIndex))
			{
				restoreSelection = true;

				foreach (int index in savedSelection)
					moduleListBox.SetSelected(index, true);

				// Select the clicked item again to make it the current item
				moduleListBox.SetSelected(clickedItemIndex, true);

				restoreSelection = false;
			}
		}
		#endregion

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



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the disk button
		/// </summary>
		/********************************************************************/
		private void DiskButton_Click(object sender, EventArgs e)
		{
			// Show the menu
			diskContextMenu.Show(sender);
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

		#region Disk menu events
		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the load list menu item
		/// </summary>
		/********************************************************************/
		private void DiskMenu_LoadList(object sender, EventArgs e)
		{
			if (ShowLoadListFileDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
					// Stop and free any modules
					StopAndFreeModule();

					// Clear the module list
					EmptyList();

					// Load the file into the module list
					LoadModuleList(loadListFileDialog.FileName);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the append list menu item
		/// </summary>
		/********************************************************************/
		private void DiskMenu_AppendList(object sender, EventArgs e)
		{
			if (ShowLoadListFileDialog() == DialogResult.OK)
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

					// Load the file into the module list
					LoadModuleList(loadListFileDialog.FileName, selected);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the save list menu item
		/// </summary>
		/********************************************************************/
		private void DiskMenu_SaveList(object sender, EventArgs e)
		{
			if (ShowSaveListFileDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
					SaveModuleList(saveListFileDialog.FileName);
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
			allowPosSliderUpdate = false;
			SetPosition(newSongPos);
			allowPosSliderUpdate = true;
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
		/// The user clicked on the previous song button
		/// </summary>
		/********************************************************************/
		private void PreviousSongButton_Click(object sender, EventArgs e)
		{
			// Get current playing song
			int curSong = moduleHandler.PlayingModuleInformation.CurrentSong;

			// Start new song
			StartSong(curSong - 1);
		}



		/********************************************************************/
		/// <summary>
		/// The user clicked on the next song button
		/// </summary>
		/********************************************************************/
		private void NextSongButton_Click(object sender, EventArgs e)
		{
			// Get current playing song
			int curSong = moduleHandler.PlayingModuleInformation.CurrentSong;

			// Start new song
			StartSong(curSong + 1);
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

		#region Loop / sample events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the sample information button
		/// </summary>
		/********************************************************************/
		private void SampleInfoButton_Click(object sender, EventArgs e)
		{
			if (IsSampleInfoWindowOpen())
			{
				if (sampleInfoWindow.WindowState == FormWindowState.Minimized)
					sampleInfoWindow.WindowState = FormWindowState.Normal;

				sampleInfoWindow.Activate();
			}
			else
			{
				sampleInfoWindow = new SampleInfoWindowForm(moduleHandler);
				sampleInfoWindow.Disposed += (o, args) => { sampleInfoWindow = null; };
				sampleInfoWindow.Show();
			}
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check to see if NostalgicPlayer has started normally or not
		/// </summary>
		/********************************************************************/
		private bool StartedNormally()//XX
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the sub-songs variables
		/// </summary>
		/********************************************************************/
		private void InitSubSongs()
		{
			subSongMultiply = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will switch to another sub-song
		/// </summary>
		/********************************************************************/
		private void SwitchSubSong(int song)
		{
			if (playItem != null)
			{
				song += subSongMultiply;

				// Get the maximum song number available
				int maxSong = moduleHandler.StaticModuleInformation.MaxSongNumber;

				// Can we play the sub-song selected?
				if (song < maxSong)
				{
					// Yes, play the new sub-song
					StartSong(song);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will check to see if any files needed to be added to the module
		/// list when starting up
		/// </summary>
		/********************************************************************/
		private void RetrieveStartupFiles()//XX
		{
			// First check if we started normally or not.
			// A non-normal start is when the user clicked on a module
			// in the File Explorer when NostalgicPlayer is not running
			if (StartedNormally())
			{
				// Call Invoke, so the main window is showing before
				// start to scan
				BeginInvoke(new Action(() =>
				{
					using (new SleepCursor())
					{
						// Get the start scan path
						string path = pathSettings.StartScan;

						// Add the files in the module list if there is any path
						if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
						{
							// Add all the files in the directory
							List<ModuleListItem> itemList = new List<ModuleListItem>();
							AddDirectoryToList(path, itemList, false);

							if (itemList.Count > 0)
							{
								// Add the items to the list
								moduleListBox.BeginUpdate();

								try
								{
									moduleListBox.Items.AddRange(itemList.ToArray());
								}
								finally
								{
									moduleListBox.EndUpdate();
								}

								// Update the window
								UpdateListCount();
								UpdateTimes();
								UpdateTapeDeck();

								// Load and play the first module
								LoadAndPlayModule(0);
							}
						}
					}
				}));
			}
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
			SetNormalTitle();

			// Create the menu
			CreateMainMenu();

			// Create button menus
			CreateSortMenu();
			CreateListMenu();
			CreateDiskMenu();

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
			ToolStripMenuItem fileMenuItem = new ToolStripMenuItem(Resources.IDS_MENU_FILE);

			ToolStripMenuItem menuItem = new ToolStripMenuItem(Resources.IDS_MENU_FILE_OPEN);
			menuItem.ShortcutKeys = Keys.Alt | Keys.O;
			menuItem.Click += PlayButton_Click;
			fileMenuItem.DropDownItems.Add(menuItem);

			fileMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_FILE_EXIT);
			menuItem.ShortcutKeys = Keys.Alt | Keys.F4;
			fileMenuItem.DropDownItems.Add(menuItem);

			menuStrip.Items.Add(fileMenuItem);

			// Create the window menu
			ToolStripMenuItem windowMenuItem = new ToolStripMenuItem(Resources.IDS_MENU_WINDOW);

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_WINDOW_SETTINGS);
			menuItem.Click += Menu_Window_Settings_Click;
			windowMenuItem.DropDownItems.Add(menuItem);

			agentSettingsSeparatorMenuItem = new ToolStripSeparator();
			agentSettingsSeparatorMenuItem.Visible = false;
			windowMenuItem.DropDownItems.Add(agentSettingsSeparatorMenuItem);

			agentShowMenuItem = new ToolStripMenuItem(Resources.IDS_MENU_WINDOW_AGENT_SHOW);
			agentShowMenuItem.Visible = false;
			windowMenuItem.DropDownItems.Add(agentShowMenuItem);

			agentSettingsMenuItem = new ToolStripMenuItem(Resources.IDS_MENU_WINDOW_AGENT_SETTINGS);
			agentSettingsMenuItem.Visible = false;
			windowMenuItem.DropDownItems.Add(agentSettingsMenuItem);

			menuStrip.Items.Add(windowMenuItem);

			// Create the help menu
			ToolStripMenuItem helpMenuItem = new ToolStripMenuItem(Resources.IDS_MENU_HELP);

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_HELP_HELP);
			menuItem.ShortcutKeys = Keys.Alt | Keys.H;
			menuItem.Click += Menu_Help_Help_Click;
			helpMenuItem.DropDownItems.Add(menuItem);

			helpMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_HELP_ABOUT);
			menuItem.Click += Menu_Help_About_Click;
			helpMenuItem.DropDownItems.Add(menuItem);

			menuStrip.Items.Add(helpMenuItem);

			// Add all agent windows to the menu which have settings or show windows
			foreach (AgentInfo agentInfo in agentManager.GetAllAgents())
				AddAgentToMenu(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Create the sort button context menu
		/// </summary>
		/********************************************************************/
		private void CreateSortMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_SORTMENU_SORT_AZ);
			item.Image = Resources.IDB_AZ;
			item.Click += SortMenu_AZ;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_SORTMENU_SORT_ZA);
			item.Image = Resources.IDB_ZA;
			item.Click += SortMenu_ZA;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_SORTMENU_SHUFFLE);
			item.Image = Resources.IDB_SHUFFLE;
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

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_SELECTMENU_ALL);
			item.Click += SelectMenu_All;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_SELECTMENU_NONE);
			item.Click += SelectMenu_None;
			menuItems.Items.Add(item);

			listContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Create the disk button context menu
		/// </summary>
		/********************************************************************/
		private void CreateDiskMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_DISKMENU_LOAD);
			item.Image = Resources.IDB_LOAD;
			item.Click += DiskMenu_LoadList;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_DISKMENU_APPEND);
			item.Click += DiskMenu_AppendList;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_DISKMENU_SAVE);
			item.Image = Resources.IDB_SAVE;
			item.Click += DiskMenu_SaveList;
			menuItems.Items.Add(item);

			diskContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Set tooltip on all the controls
		/// </summary>
		/********************************************************************/
		private void SetTooltips()
		{
			toolTip.SetToolTip(infoGroup.Panel, Resources.IDS_TIP_MAIN_INFO);
			toolTip.SetToolTip(infoLabel, Resources.IDS_TIP_MAIN_INFO);
			toolTip.SetToolTip(moduleInfoButton, Resources.IDS_TIP_MAIN_MODULEINFO);

			toolTip.SetToolTip(muteCheckButton, Resources.IDS_TIP_MAIN_MUTE);
			toolTip.SetToolTip(masterVolumeTrackBar, Resources.IDS_TIP_MAIN_VOLUME);

			toolTip.SetToolTip(addModuleButton, Resources.IDS_TIP_MAIN_ADD);
			toolTip.SetToolTip(removeModuleButton, Resources.IDS_TIP_MAIN_REMOVE);
			toolTip.SetToolTip(swapModulesButton, Resources.IDS_TIP_MAIN_SWAP);
			toolTip.SetToolTip(sortModulesButton, Resources.IDS_TIP_MAIN_SORT);
			toolTip.SetToolTip(moveModulesUpButton, Resources.IDS_TIP_MAIN_UP);
			toolTip.SetToolTip(moveModulesDownButton, Resources.IDS_TIP_MAIN_DOWN);
			toolTip.SetToolTip(listButton, Resources.IDS_TIP_MAIN_SELECT);
			toolTip.SetToolTip(diskButton, Resources.IDS_TIP_MAIN_DISK);

			toolTip.SetToolTip(timeLabel, Resources.IDS_TIP_MAIN_TIME);
			toolTip.SetToolTip(totalLabel, Resources.IDS_TIP_MAIN_TOTAL);

			toolTip.SetToolTip(positionTrackBar, Resources.IDS_TIP_MAIN_POSITIONSLIDER);

			toolTip.SetToolTip(previousModuleButton, Resources.IDS_TIP_MAIN_PREVMOD);
			toolTip.SetToolTip(previousSongButton, Resources.IDS_TIP_MAIN_PREVSONG);
			toolTip.SetToolTip(rewindButton, Resources.IDS_TIP_MAIN_REWIND);
			toolTip.SetToolTip(playButton, Resources.IDS_TIP_MAIN_PLAY);
			toolTip.SetToolTip(fastForwardButton, Resources.IDS_TIP_MAIN_FORWARD);
			toolTip.SetToolTip(nextSongButton, Resources.IDS_TIP_MAIN_NEXTSONG);
			toolTip.SetToolTip(nextModuleButton, Resources.IDS_TIP_MAIN_NEXTMOD);
			toolTip.SetToolTip(ejectButton, Resources.IDS_TIP_MAIN_EJECT);
			toolTip.SetToolTip(pauseCheckButton, Resources.IDS_TIP_MAIN_PAUSE);

			toolTip.SetToolTip(loopCheckButton, Resources.IDS_TIP_MAIN_MODULELOOP);
			toolTip.SetToolTip(sampleInfoButton, Resources.IDS_TIP_MAIN_SAMP);
		}



		/********************************************************************/
		/// <summary>
		/// Will create and open other windows
		/// </summary>
		/********************************************************************/
		private void CreateWindows()
		{
			if (mainWindowSettings.OpenModuleInformationWindow)
			{
				moduleInfoWindow = new ModuleInfoWindowForm(moduleHandler, this);
				moduleInfoWindow.Show();
			}

			if (mainWindowSettings.OpenSampleInformationWindow)
			{
				sampleInfoWindow = new SampleInfoWindowForm(moduleHandler);
				sampleInfoWindow.Show();
			}

			foreach (Guid typeId in mainWindowSettings.OpenAgentWindows)
			{
				AgentInfo agentInfo = agentManager.GetAllAgents().FirstOrDefault(a => a.TypeId == typeId);
				if (agentInfo != null)
					OpenAgentDisplayWindow(agentInfo);
			}
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

			// Destroy the load list file dialog
			if (loadListFileDialog != null)
			{
				loadListFileDialog.Dispose();
				loadListFileDialog = null;
			}

			// Destroy the save list file dialog
			if (saveListFileDialog != null)
			{
				saveListFileDialog.Dispose();
				saveListFileDialog = null;
			}

			// Remember settings from main window
			mainWindowSettings.Time = timeFormat;
			mainWindowSettings.MasterVolume = masterVolumeTrackBar.Value;

			// Close agent settings windows
			lock (openAgentSettings)
			{
				foreach (AgentSettingsWindowForm agentSettingsWindow in openAgentSettings.Values)
					agentSettingsWindow.Close();

				openAgentSettings.Clear();
			}

			// Close agent display windows
			lock (openAgentDisplays)
			{
				mainWindowSettings.OpenAgentWindows = openAgentDisplays.Keys.ToArray();

				foreach (AgentDisplayWindowForm agentDisplayWindow in openAgentDisplays.Values)
					agentDisplayWindow.Close();

				openAgentDisplays.Clear();
			}

			// Close the about window
			if (IsAboutWindowOpen())
				aboutWindow.Close();

			aboutWindow = null;

			// Close the settings window
			if (IsSettingsWindowOpen())
				settingsWindow.Close();

			settingsWindow = null;

			// Close the module information window
			bool openAgain = IsModuleInfoWindowOpen();
			if (openAgain)
				moduleInfoWindow.Close();

			moduleInfoWindow = null;
			mainWindowSettings.OpenModuleInformationWindow = openAgain;

			// Close the sample information window
			openAgain = IsSampleInfoWindowOpen();
			if (openAgain)
				sampleInfoWindow.Close();

			sampleInfoWindow = null;
			mainWindowSettings.OpenSampleInformationWindow = openAgain;
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure to refresh all the windows
		/// </summary>
		/********************************************************************/
		private void RefreshWindows()
		{
			if (IsModuleInfoWindowOpen())
				moduleInfoWindow.RefreshWindow();

			if (IsSampleInfoWindowOpen())
				sampleInfoWindow.RefreshWindow();

			if (IsSettingsWindowOpen())
				settingsWindow.RefreshWindow();
		}



		/********************************************************************/
		/// <summary>
		/// Check if the about window is open
		/// </summary>
		/********************************************************************/
		private bool IsAboutWindowOpen()
		{
			return (aboutWindow != null) && !aboutWindow.IsDisposed;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the settings window is open
		/// </summary>
		/********************************************************************/
		private bool IsSettingsWindowOpen()
		{
			return (settingsWindow != null) && !settingsWindow.IsDisposed;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the module information window is open
		/// </summary>
		/********************************************************************/
		private bool IsModuleInfoWindowOpen()
		{
			return (moduleInfoWindow != null) && !moduleInfoWindow.IsDisposed;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the sample information window is open
		/// </summary>
		/********************************************************************/
		private bool IsSampleInfoWindowOpen()
		{
			return (sampleInfoWindow != null) && !sampleInfoWindow.IsDisposed;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the window title to normal
		/// </summary>
		/********************************************************************/
		private void SetNormalTitle()
		{
			Text = Resources.IDS_MAIN_TITLE;
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
				if (itemRect == Rectangle.Empty)
				{
					// Sometimes the rectangle returned is empty.
					// If that's the case, scroll to the playing item
					moduleListBox.TopIndex = itemIndex;
				}
				else
				{
					itemRect.X += listRect.X;
					itemRect.Y += listRect.Y;

					if (!listRect.Contains(itemRect))
					{
						// Make sure the item can be seen
						moduleListBox.TopIndex = itemIndex;
					}
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
			bool isLoaded = moduleHandler.IsModuleLoaded;

			// If the module list is empty, disable the eject button
			ejectButton.Enabled = count > 0;

			// If no module is loaded, disable the pause button
			pauseCheckButton.Enabled = isLoaded;

			// If only one item is in the list or none is loaded, disable
			// the previous and next module buttons
			if (isLoaded && (count > 1))
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

			// If no module is loaded or the player doesn't support position change,
			// disable the rewind and forward buttons + the position slider
			if (isLoaded && staticModuleInfo.CanChangePosition)
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
				selStr = tempTime.ToString(Resources.IDS_TIMEFORMAT);
			else
				selStr = tempTime.ToString(Resources.IDS_TIMEFORMAT_SMALL);

			// And build the list time string
			tempTime = new TimeSpan((((long)listTime.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
			if ((int)tempTime.TotalHours > 0)
				listStr = tempTime.ToString(Resources.IDS_TIMEFORMAT);
			else
				listStr = tempTime.ToString(Resources.IDS_TIMEFORMAT_SMALL);

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

			if (moduleHandler.IsModuleLoaded)
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
					posStr = string.Format(Resources.IDS_POSITION, songPos, songLength, percent);

					// Set the position slider
					if (allowPosSliderUpdate && (positionTrackBar.Value != percent))
						positionTrackBar.Value = percent;
				}
				else
				{
					// Set the position string to n/a
					posStr = Resources.IDS_NOPOSITION;

					//XX Never ending update
				}

				// Create sub-song string
				int currentSong = playingModuleInfo.CurrentSong + 1;
				int maxSong = staticModuleInfo.MaxSongNumber;

				if (maxSong == 0)
				{
					subStr = Resources.IDS_NOSUBSONGS;
					timeStr = timeFormat == MainWindowSettings.TimeFormat.Elapsed ? Resources.IDS_NOTIME : Resources.IDS_NONEGATIVETIME;
				}
				else
				{
					subStr = string.Format(Resources.IDS_SUBSONGS, currentSong, maxSong);

					// Format the time string
					if (timeFormat == MainWindowSettings.TimeFormat.Elapsed)
					{
						timeStr = string.Format("{0} {1}", Resources.IDS_TIME, timeOccurred.ToString(Resources.IDS_TIMEFORMAT));
					}
					else
					{
						// Calculate the remaining time
						TimeSpan tempSpan = songTotalTime - timeOccurred;

						// Check to see if we got a negative number
						if (tempSpan.TotalMilliseconds < 0)
							tempSpan = new TimeSpan(0);

						// Format the string
						timeStr = string.Format("{0} -{1}", Resources.IDS_TIME, tempSpan.ToString(Resources.IDS_TIMEFORMAT));
					}
				}
			}
			else
			{
				posStr = Resources.IDS_NOPOSITION;
				subStr = Resources.IDS_NOSUBSONGS;
				timeStr = timeFormat == MainWindowSettings.TimeFormat.Elapsed ? Resources.IDS_NOTIME : Resources.IDS_NONEGATIVETIME;
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

			// Fix agent settings
			FixAgentSettings(Manager.AgentType.Players);
			FixAgentSettings(Manager.AgentType.Output);

			// And finally, save the settings to disk
			userSettings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Fix agent settings (removed non-exiting ones and add unknown)
		/// plus tell the manager about which agents that are disabled
		/// </summary>
		/********************************************************************/
		private void FixAgentSettings(Manager.AgentType agentType)
		{
			string section = agentType + " Agents";

			// Build lookup list
			Dictionary<Guid, AgentInfo> allAgents = agentManager.GetAllAgents(agentType).ToDictionary(agentInfo => agentInfo.TypeId, agentInfo => agentInfo);

			// First comes the delete loop. It will scan the section and see
			// if the agent has been loaded and if it hasn't, the entry will
			// be removed from the section
			List<Guid> toRemove = new List<Guid>();
			foreach (AgentEntry agentEntry in ReadAgentEntry(section))
			{
				// Do we have the entry in the collection
				if (!allAgents.ContainsKey(agentEntry.TypeId))
				{
					// It doesn't, so remove the entry
					toRemove.Add(agentEntry.TypeId);
				}
			}

			// Remove all entries marked to be so
			foreach (Guid typeId in toRemove)
				userSettings.RemoveEntry(section, typeId.ToString("D"));

			// Now take all the loaded agents and add those which does not
			// exists in the settings file
			//
			// This dictionary holds how many types that are enabled for each agent
			Dictionary<Guid, int> enableCount = allAgents.Values.Select(a => a.AgentId).Distinct().ToDictionary(id => id, id => 0);

			foreach (AgentInfo agentInfo in allAgents.Values)
			{
				// Does the agent already exists in the settings file?
				bool enabled = false;

				bool found = false;
				foreach (AgentEntry agentEntry in ReadAgentEntry(section))
				{
					if (agentInfo.TypeId == agentEntry.TypeId)
					{
						enabled = agentEntry.Enabled;

						found = true;
						break;
					}
				}

				if (!found)
				{
					// The agent isn't in the settings file
					enabled = true;

					// Add a new entry in the settings file
					userSettings.SetBoolEntry(section, agentInfo.TypeId.ToString("D"), true);
				}

				if (enabled)
					enableCount[agentInfo.AgentId] = ++enableCount[agentInfo.AgentId];
			}

			// Flush all agents that is disabled
			foreach (Guid agentId in enableCount.Where(pair => pair.Value == 0).Select(pair => pair.Key))
				agentManager.UnloadAgent(agentId);
		}



		/********************************************************************/
		/// <summary>
		/// Will read and parse a single agent entry in the section given
		/// </summary>
		/********************************************************************/
		private IEnumerable<AgentEntry> ReadAgentEntry(string section)
		{
			for (int i = 0; ; i++)
			{
				Guid typeId = Guid.Empty;
				bool enabled = false;

				try
				{
					// Try to read the entry
					string value = userSettings.GetStringEntry(section, i, out string id);
					if (string.IsNullOrEmpty(value))
						break;

					// Parse the entry value
					if (!bool.TryParse(value, out enabled))
						enabled = true;

					typeId = Guid.Parse(id);
				}
				catch (Exception)
				{
					// Ignore exception
					;
				}

				if (typeId != Guid.Empty)
					yield return new AgentEntry { TypeId = typeId, Enabled = enabled };
			}
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

				// Mark the item in the list
				ChangePlayItem(listItem);

				// Initialize controls
				InitControls();

				// And refresh other windows
				RefreshWindows();
			}
			else
			{
				// Free loaded module
				StopAndFreeModule();
			}
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
			SetNormalTitle();

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
		private void AddSingleFileToList(string fileName, List<ModuleListItem> list, bool checkForList)
		{
			IMultiFileLoader loader = null;

			if (checkForList)
			{
				using (FileStream fs = File.OpenRead(fileName))
				{
					loader = ListFactory.Create(fs);
					if (loader != null)
					{
						foreach (MultiFileInfo info in loader.LoadList(Path.GetDirectoryName(fileName), fs))
							list.Add(ListItemConverter.Convert(info));
					}
				}
			}

			//XX TODO: Add archive support here

			if (loader == null)
			{
				// Just a plain file
				list.Add(new ModuleListItem(new SingleFileListItem(fileName)));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will append all the files inside the given directory to the list
		/// given
		/// </summary>
		/********************************************************************/
		private void AddDirectoryToList(string directory, List<ModuleListItem> list, bool checkForList)
		{
			// First go through all the files
			foreach (string fileName in Directory.EnumerateFiles(directory))
				AddSingleFileToList(fileName, list, checkForList);

			// Now go through all the directories
			foreach (string directoryName in Directory.EnumerateDirectories(directory))
				AddDirectoryToList(directoryName, list, checkForList);
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

			DialogResult result = moduleFileDialog.ShowDialog();
			if (result == DialogResult.OK)
				moduleFileDialog.InitialDirectory = null;	// Clear so it won't start in the initial directory again, but in current directory next time it is opened

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will show the file dialog where the user can select where to
		/// load the module list
		/// </summary>
		/********************************************************************/
		private DialogResult ShowLoadListFileDialog()
		{
			// Create the dialog if not already created
			if (loadListFileDialog == null)
			{
				loadListFileDialog = new OpenFileDialog();
				loadListFileDialog.InitialDirectory = pathSettings.ModuleList;
				loadListFileDialog.DefaultExt = "npml";
				loadListFileDialog.Filter = "List files|*.npml";
			}

			DialogResult result = loadListFileDialog.ShowDialog();
			if (result == DialogResult.OK)
				loadListFileDialog.InitialDirectory = null;	// Clear so it won't start in the initial directory again, but in current directory next time it is opened

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will show the file dialog where the user can select where to
		/// save the module list
		/// </summary>
		/********************************************************************/
		private DialogResult ShowSaveListFileDialog()
		{
			// Create the dialog if not already created
			if (saveListFileDialog == null)
			{
				saveListFileDialog = new SaveFileDialog();
				saveListFileDialog.InitialDirectory = pathSettings.ModuleList;
				saveListFileDialog.DefaultExt = "npml";
				saveListFileDialog.Filter = "List files|*.npml";
			}

			DialogResult result = saveListFileDialog.ShowDialog();
			if (result == DialogResult.OK)
				saveListFileDialog.InitialDirectory = null;	// Clear so it won't start in the initial directory again, but in current directory next time it is opened

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will load a module list into the list at the index given
		/// </summary>
		/********************************************************************/
		private void LoadModuleList(string fileName, int index = -1)
		{
			try
			{
				using (FileStream fs = File.OpenRead(fileName))
				{
					IMultiFileLoader loader = ListFactory.Create(fs);
					if (loader == null)
						ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_UNKNOWN_LIST_FORMAT, fileName));
					else
					{
						List<ModuleListItem> tempList = new List<ModuleListItem>();

						foreach (MultiFileInfo info in loader.LoadList(Path.GetDirectoryName(fileName), fs))
							tempList.Add(ListItemConverter.Convert(info));

						moduleListBox.BeginUpdate();

						try
						{
							if (index == -1)
								moduleListBox.Items.AddRange(tempList.ToArray());
							else
							{
								for (int i = tempList.Count - 1; i >= 0; i--)
									moduleListBox.Items.Insert(index, tempList[i]);

							}
						}
						finally
						{
							moduleListBox.EndUpdate();
						}
					}
				}
			}
			catch(Exception ex)
			{
				// Show error
				ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_LOAD_LIST, ex.Message));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will save the whole module list to an NPML file
		/// </summary>
		/********************************************************************/
		private void SaveModuleList(string fileName)
		{
			try
			{
				NpmlList.SaveList(fileName, GetModuleList());
			}
			catch(Exception ex)
			{
				// Show error
				ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_SAVE_LIST, ex.Message));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will return an enumerator returning all the module list items
		/// </summary>
		/********************************************************************/
		private IEnumerable<MultiFileInfo> GetModuleList()
		{
			foreach (ModuleListItem listItem in moduleListBox.Items)
				yield return ListItemConverter.Convert(listItem);
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
		private void AddFilesToList(string[] files, int startIndex = -1, bool checkForList = false)
		{
			List<ModuleListItem> itemList = new List<ModuleListItem>();

			foreach (string file in files)
			{
				if (Directory.Exists(file))
				{
					// It's a directory, so add all files inside it
					AddDirectoryToList(file, itemList, checkForList);
				}
				else
				{
					// It's a file
					AddSingleFileToList(file, itemList, checkForList);
				}
			}

			// Add the items to the list
			moduleListBox.BeginUpdate();

			try
			{
				if (startIndex == -1)
					moduleListBox.Items.AddRange(itemList.ToArray());
				else
				{
					for (int i = itemList.Count - 1; i >= 0; i--)
						moduleListBox.Items.Insert(startIndex, itemList[i]);
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
				// Remember which item to select, after removing is done
				int indexToSelect = moduleListBox.SelectedIndices[moduleListBox.SelectedIndices.Count - 1] - moduleListBox.SelectedIndices.Count + 1;

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

				if (moduleListBox.Items.Count > 0)
				{
					if (indexToSelect >= moduleListBox.Items.Count)
						indexToSelect = moduleListBox.Items.Count - 1;

					moduleListBox.SelectedIndex = indexToSelect;
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

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Krypton.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AboutWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AgentWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.FavoriteSongSystemWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles;
using Polycode.NostalgicPlayer.Client.GuiPlayer.NewVersionWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.OpenUrlWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Gui.Controls;
using Polycode.NostalgicPlayer.Kit.Gui.Extensions;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Interfaces;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This is the main window
	/// </summary>
	public partial class MainWindowForm : WindowFormBase, IMainWindowApi, IExtraChannels
	{
		private class AgentEntry
		{
			public Guid TypeId { get; set; }
			public bool Enabled { get; set; }
		}

		private Manager agentManager;
		private ModuleHandler moduleHandler;

		// Settings
		private ISettings userSettings;
		private OptionSettings optionSettings;
		private ModuleSettings moduleSettings;
		private PathSettings pathSettings;
		private SoundSettings soundSettings;

		private MainWindowSettings mainWindowSettings;

		// File dialogs
		private OpenFileDialog moduleFileDialog;
		private FolderBrowserDialog moduleDirectoryDialog;
		private OpenFileDialog loadListFileDialog;
		private SaveFileDialog saveListFileDialog;

		// Menus
		private ToolStripSeparator agentSettingsSeparatorMenuItem;
		private ToolStripMenuItem agentSettingsMenuItem;
		private ToolStripMenuItem agentShowMenuItem;

		// Context menus
		private KryptonContextMenuItem setSubSongMenuItem;
		private KryptonContextMenuItem clearSubSongMenuItem;

		// Timer variables
		private MainWindowSettings.TimeFormat timeFormat;
		private TimeSpan timeOccurred;
		private TimeSpan neverEndingTimeout;
		private bool neverEndingStarted;

		// Module variables
		private ModuleListItem playItem;
		private int subSongMultiply;
		private int startedSubSong;

		// List times
		private TimeSpan listTime;
		private TimeSpan selectedTime;

		// Window/control status variables
		private bool allowPosSliderUpdate;

		// Different helper classes
		private ModuleDatabase database;
		private FileScanner fileScanner;

		// Play samples from sample info window info
		private int playSamplesChannelNumber;
		private bool playSamples;

		// Misc.
		private readonly List<int> randomList;

		private long lastAddedTimeFromExplorer = 0;
		private readonly Lock processingEndReached = new Lock();

		// Other windows
		private HelpWindowForm helpWindow = null;
		private AboutWindowForm aboutWindow = null;
		private SettingsWindowForm settingsWindow = null;
		private ModuleInfoWindowForm moduleInfoWindow = null;
		private FavoriteSongSystemForm favoriteSongSystemWindow = null;
		private SampleInfoWindowForm sampleInfoWindow = null;
		private AudiusWindowForm audiusWindow = null;
		private SearchPopupForm searchPopupForm = null;

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

			// Disable escape key closing
			disableEscapeKey = true;

			// Initialize member variables
			playItem = null;
			allowPosSliderUpdate = true;

			randomList = new List<int>();

			openAgentSettings = new Dictionary<Guid, AgentSettingsWindowForm>();
			openAgentDisplays = new Dictionary<Guid, AgentDisplayWindowForm>();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form by loading agents, initialize controls etc.
		/// </summary>
		/********************************************************************/
		public void InitializeForm(Manager.LoadAgentProgress loadProgress)
		{
			// Initialize and load settings
			InitSettings();

			// Load agents
			LoadAgents(loadProgress);

			// Initialize module handler
			InitModuleHandler();

			// Initialize main window
			SetupControls();

			// Initialize helper classes
			database = new ModuleDatabase();

			if ((new DateTime(optionSettings.LastCleanupTime).AddDays(7) < DateTime.Now))
			{
				database.StartCleanup(() =>
				{
					BeginInvoke(() =>
					{
						if (IsFavoriteSongSystemWindowOpen())
							favoriteSongSystemWindow.RefreshWindow();
					});
				});

				optionSettings.LastCleanupTime = DateTime.Now.Ticks;
			}

			fileScanner = new FileScanner(moduleListControl, optionSettings, agentManager, database, this);
			fileScanner.Start();

			SetupHandlers();
		}



		/********************************************************************/
		/// <summary>
		/// Is call if a second instance of the application is started. This
		/// method is called from the first instance and will get the
		/// arguments from the second instance
		/// </summary>
		/********************************************************************/
		public void StartupHandler(string[] arguments)
		{
			if (arguments.Length > 0)
				AddFilesFromStartupHandler(arguments);
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "howtouse.html";
		#endregion

		#region IMainWindowApi implementation
		/********************************************************************/
		/// <summary>
		/// Return the form of the main window
		/// </summary>
		/********************************************************************/
		public Form Form => this;



		/********************************************************************/
		/// <summary>
		/// Return the extra channel implementation
		/// </summary>
		/********************************************************************/
		public IExtraChannels ExtraChannelsImplementation => this;



		/********************************************************************/
		/// <summary>
		/// Open the help window if not already open
		/// </summary>
		/********************************************************************/
		public void OpenHelpWindow(string newUrl)
		{
			if (IsHelpWindowOpen())
				helpWindow.Activate();
			else
			{
				helpWindow = new HelpWindowForm(this, optionSettings);
				helpWindow.Disposed += (o, args) => { helpWindow = null; };
				helpWindow.Show();
			}

			helpWindow.Navigate(newUrl);
		}



		/********************************************************************/
		/// <summary>
		/// Will show a question to the user
		/// </summary>
		/********************************************************************/
		public bool ShowQuestion(string question)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(question, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Question))
			{
				dialog.AddButton(Resources.IDS_BUT_YES, 'y');
				dialog.AddButton(Resources.IDS_BUT_NO, 'n');
				dialog.ShowDialog(this);

				return dialog.GetButtonResult('n') == 'y';
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		public void ShowSimpleErrorMessage(string message)
		{
			ShowSimpleErrorMessage(this, message);
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		public void ShowSimpleErrorMessage(IWin32Window owner, string message)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(message, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error))
			{
				dialog.AddButton(Resources.IDS_BUT_OK, 'O');
				dialog.ShowDialog(owner);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		public void ShowErrorMessage(string message, ModuleListItem listItem)
		{
			char response;

			switch (moduleSettings.ModuleError)
			{
				case ModuleSettings.ModuleErrorAction.ShowError:
				{
					using (CustomMessageBox dialog = new CustomMessageBox(message, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error))
					{
						dialog.AddButton(Resources.IDS_BUT_SKIP, 'S');
						dialog.AddButton(Resources.IDS_BUT_SKIPREMOVE, 'r');
						dialog.AddButton(Resources.IDS_BUT_STOP, 'p');
						dialog.ShowDialog(this);
						response = dialog.GetButtonResult('p');
					}
					break;
				}

				case ModuleSettings.ModuleErrorAction.SkipFile:
				default:
				{
					response = 'S';
					break;
				}

				case ModuleSettings.ModuleErrorAction.SkipFileAndRemoveFromList:
				{
					response = 'r';
					break;
				}

				case ModuleSettings.ModuleErrorAction.StopPlaying:
				{
					response = 'p';
					break;
				}
			}

			StopAndFreeModule();

			switch (response)
			{
				// Skip
				case 'S':
				{
					// Get the index of the module that couldn't be loaded
					int index = moduleListControl.Items.IndexOf(listItem);
					if (index != -1)
					{
						index++;

						// Get the number of items in the list
						int count = moduleListControl.Items.Count;

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
							if ((count > 1) && (moduleSettings.ModuleListEnd == ModuleSettings.ModuleListEndAction.JumpToStart))
							{
								// Load the first module, but only if it's valid
								// or haven't been loaded before
								ModuleListItem item = moduleListControl.Items[0];
								if (!item.HaveTime || (item.HaveTime && item.Duration.TotalMilliseconds != 0))
									LoadAndPlayModule(item);
							}
						}
					}
					break;
				}

				// Skip and remove
				case 'r':
				{
					// Get the index of the module that couldn't be loaded
					int index = moduleListControl.Items.IndexOf(listItem);
					if (index != -1)
					{
						// Get the number of items in the list - 1
						int count = moduleListControl.Items.Count - 1;

						// Deselect the playing flag
						ChangePlayItem(null);

						// Remove the module from the list
						moduleListControl.Items.RemoveAt(index);

						// Update the window
						UpdateControls();

						// Do there exist a "next" module
						if (index < count)
						{
							// Yes, load it
							LoadAndPlayModule(index);
						}
						else
						{
							// No
							if ((count > 1) && (moduleSettings.ModuleListEnd == ModuleSettings.ModuleListEndAction.JumpToStart))
							{
								// Load the first module, but only if it's valid
								// or haven't been loaded before
								ModuleListItem item = moduleListControl.Items[0];
								if (!item.HaveTime || (item.HaveTime && item.Duration.TotalMilliseconds != 0))
									LoadAndPlayModule(item);
							}
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



		/********************************************************************/
		/// <summary>
		/// Will add all the given files to the module list
		/// </summary>
		/********************************************************************/
		public void AddFilesToModuleList(string[] files, int startIndex = -1, bool checkForList = false)
		{
			AddFilesToList(files, startIndex, checkForList);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the given module list items to the module list
		/// </summary>
		/********************************************************************/
		public void AddItemsToModuleList(ModuleListItem[] items, bool clearAndPlay)
		{
			AddItemsToList(items, clearAndPlay);
		}



		/********************************************************************/
		/// <summary>
		/// Will replace the given item with the new list of items
		/// </summary>
		/********************************************************************/
		public void ReplaceItemInModuleList(ModuleListItem listItem, List<ModuleListItem> newItems)
		{
			if ((listItem != null) && (newItems.Count > 0))
			{
				// Find index of the item
				int index = moduleListControl.Items.IndexOf(listItem);
				if (index != -1)
				{
					moduleListControl.BeginUpdate();

					try
					{
						moduleListControl.Items.RemoveAt(index);
						moduleListControl.Items.InsertRange(index, newItems);
					}
					finally
					{
						moduleListControl.EndUpdate();
					}

					UpdateListCount();
					UpdateListControls();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will remove all the items in the given list from the module list
		/// </summary>
		/********************************************************************/
		public void RemoveItemsFromModuleList(List<ModuleListItem> items)
		{
			moduleListControl.BeginUpdate();

			try
			{
				foreach (ModuleListItem moduleListItem in items)
					moduleListControl.Items.Remove(moduleListItem);
			}
			finally
			{
				moduleListControl.EndUpdate();
			}

			UpdateListCount();
			UpdateListControls();
		}



		/********************************************************************/
		/// <summary>
		/// Will update all the items in the given list on the module list
		/// </summary>
		/********************************************************************/
		public void UpdateModuleList(List<ModuleListItemUpdateInfo> items)
		{
			moduleListControl.BeginUpdate();

			try
			{
				foreach (ModuleListItemUpdateInfo updateInfo in items)
				{
					if (moduleListControl.Items.Contains(updateInfo.ListItem))
					{
						if (updateInfo.Duration.HasValue)
						{
							TimeSpan prevTime = updateInfo.ListItem.Duration;

							// Change the time on the item
							updateInfo.ListItem.Duration = updateInfo.Duration.Value;

							// Now calculate the new list time
							listTime = listTime - prevTime + updateInfo.Duration.Value;
						}
					}
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}

			UpdateTimes();
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
					if (agentInfo.TypeName.CompareTo(menuItem.Text) < 0)
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
					if (agentInfo.TypeName.CompareTo(menuItem.Text) < 0)
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
					agentSettingsWindow = new AgentSettingsWindowForm(agentManager, agentInfo, this, optionSettings);
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
					agentDisplayWindow = new AgentDisplayWindowForm(agentManager, agentInfo, moduleHandler, this, optionSettings);
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
		/// Enable/disable user interface settings
		/// </summary>
		/********************************************************************/
		public void EnableUserInterfaceSettings()
		{
			using (new SleepCursor())
			{
				toolTip.Active = optionSettings.ToolTips;

				SetTitle();
				moduleListControl.EnableListNumber(optionSettings.ShowListNumber);
				moduleListControl.EnableFullPath(optionSettings.ShowFullPath);

				if (optionSettings.UseDatabase)
				{
					toolTip.SetToolTip(favoritesButton, Resources.IDS_TIP_MAIN_FAVORITES);
					favoritesButton.Enabled = true;
				}
				else
				{
					toolTip.SetToolTip(favoritesButton, Resources.IDS_TIP_MAIN_FAVORITES_DISABLED);
					favoritesButton.Enabled = false;

					if (IsFavoriteSongSystemWindowOpen())
						favoriteSongSystemWindow.Close();
				}
			}
		}
		#endregion

		#region IExtraChannels implementation
		/********************************************************************/
		/// <summary>
		/// Do any needed initialization
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			playSamplesChannelNumber = 0;
			playSamples = false;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Play the extra channels
		/// </summary>
		/********************************************************************/
		public bool PlayChannels(IChannel[] channels)
		{
			// Get the sample to play
			if (IsSampleInfoWindowOpen())
			{
				PlaySampleInfo playInfo = sampleInfoWindow.GetNextSampleFromQueue();
				if (playInfo != null)
				{
					if (playInfo.Note < 0)
					{
						// Mute all the channels
						for (int i = 0; i < SampleInfoWindowForm.PolyphonyChannels; i++)
							channels[i].Mute();

						playSamples = false;
					}
					else
					{
						int playChannel;

						// Find the next channel to play in
						if (sampleInfoWindow.IsPolyphonyEnabled)
						{
							playChannel = playSamplesChannelNumber;

							playSamplesChannelNumber++;
							if (playSamplesChannelNumber == SampleInfoWindowForm.PolyphonyChannels)
								playSamplesChannelNumber = 0;
						}
						else
						{
							playChannel = 0;

							// Mute the other channels
							for (int i = 1; i < SampleInfoWindowForm.PolyphonyChannels; i++)
								channels[i].Mute();
						}

						SampleInfo sampleInfo = playInfo.SampleInfo;
						IChannel channel = channels[playChannel];

						// Check the item to see if it's a legal sample
						if (((sampleInfo.Sample != null) || (sampleInfo.MultiOctaveSamples != null)) && (sampleInfo.Length > 0))
						{
							Array sample = sampleInfo.Sample;
							uint offset = sampleInfo.SampleOffset;
							uint length = sampleInfo.Length;
							uint loopStart = sampleInfo.LoopStart;
							uint loopLength = sampleInfo.LoopLength;

							// Find the frequency to play with
							int note = playInfo.Note;

							if ((sampleInfo.Flags & SampleInfo.SampleFlag.MultiOctave) != 0)
							{
								int octave = note / 12;
								if (octave > 7)
									octave = 7;

								SampleInfo.MultiOctaveInfo octaveInfo = sampleInfo.MultiOctaveSamples[octave];
								note += octaveInfo.NoteAdd;

								sample = octaveInfo.Sample;
								offset = octaveInfo.SampleOffset;

								length = octaveInfo.Length;
								loopStart = octaveInfo.LoopStart;
								loopLength = octaveInfo.LoopLength;
							}

							// Calculate the frequency
							double frequency = sampleInfo.MiddleC;

							if (note < 48)
							{
								for (int i = note; i < 48; i++)
									frequency /= 1.059463094359;
							}
							else
							{
								for (int i = 48; i < note; i++)
									frequency *= 1.059463094359;
							}

							// Find the play flag
							PlaySampleFlag playFlag = PlaySampleFlag.None;

							if ((sampleInfo.Flags & SampleInfo.SampleFlag._16Bit) != 0)
								playFlag |= PlaySampleFlag._16Bit;

							if ((sampleInfo.Flags & SampleInfo.SampleFlag.Stereo) != 0)
								playFlag |= PlaySampleFlag.Stereo;

							// Play it
							channel.PlaySample(-1, sample, offset, length - offset, playFlag);

							channel.SetFrequency((uint)frequency);

							ushort vol = sampleInfo.Volume;
							channel.SetVolume(vol == 0 ? (ushort)256 : vol);

							int pan = sampleInfo.Panning;
							channel.SetPanning(pan == -1 ? (ushort)ChannelPanningType.Center : (ushort)pan);

							if ((sampleInfo.Flags & SampleInfo.SampleFlag.Loop) != 0)
								channel.SetLoop(loopStart, loopLength, (sampleInfo.Flags & SampleInfo.SampleFlag.PingPong) != 0 ? ChannelLoopType.PingPong : ChannelLoopType.Normal);
						}
						else
						{
							// Mute the channel
							channel.Mute();
						}

						playSamples = true;
					}
				}
			}

			return playSamples;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of extra channels
		/// </summary>
		/********************************************************************/
		public int ExtraChannels => SampleInfoWindowForm.PolyphonyChannels;
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
			Shown += MainWindowForm_Shown;
			Resize += MainWindowForm_Resize;
			FormClosed += MainWindowForm_FormClosed;

			// Module information
			infoGroup.Panel.Click += InfoGroup_Click;
			infoLabel.Click += InfoGroup_Click;

			equalizerButton.Click += EqualizerButton_Click;
			moduleInfoButton.Click += ModuleInfoButton_Click;

			// Module list
			moduleListControl.SelectedIndexChanged += ModuleListControl_SelectedIndexChanged;
			moduleListControl.MouseDoubleClick += ModuleListControl_MouseDoubleClick;
			moduleListControl.KeyPress += ModuleListControl_KeyPress;
			moduleListControl.DragDrop += ModuleListControl_DragDrop;

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

			// Functions group
			favoritesButton.Click += FavoritesButton_Click;
			sampleInfoButton.Click += SampleInfoButton_Click;

			// Never ending
			neverEndingTimer.Tick += NeverEndingTimer_Tick;

			// Module handler
			moduleHandler.ClockUpdated += ModuleHandler_ClockUpdated;
			moduleHandler.PositionChanged += ModuleHandler_PositionChanged;
			moduleHandler.SubSongChanged += ModuleHandler_SubSongChanged;
			moduleHandler.EndReached += ModuleHandler_EndReached;
			moduleHandler.ModuleInfoChanged += ModuleHandler_ModuleInfoChanged;
			moduleHandler.PlayerFailed += ModuleHandler_PlayerFailed;
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
						int total = moduleListControl.Items.Count;

						// Do only continue if we have more than one
						// module in the list
						if (total > 1)
						{
							// Find a random module until we found
							// one that is not the playing one
							int playingIndex = playItem == null ? -1 : moduleListControl.Items.IndexOf(playItem);
							int index;

							do
							{
								index = RandomGenerator.GetRandomNumber(total);
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
						if (moduleListControl.Items.Count > 1)
							previousModuleButton.PerformClick();

						return true;
					}

					// Right arrow - Fast forward
					case Keys.Right:
					{
						if (moduleListControl.Items.Count > 1)
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
		/// Is called when the form is shown for the first time
		/// </summary>
		/********************************************************************/
		private void MainWindowForm_Shown(object sender, EventArgs e)
		{
			// Create other windows
			CreateWindows();

			// Check if this is a new version of the application
			string versionFile = Path.Combine(Settings.SettingsDirectory, "CurrentVersion.txt");

			string currentVersion = Env.CurrentVersion;

			if (File.Exists(versionFile))
			{
				string previousVersion = File.ReadAllText(versionFile);
				if (previousVersion != currentVersion)
				{
					using (NewVersionWindowForm dialog = new NewVersionWindowForm(previousVersion, currentVersion))
					{
						dialog.ShowDialog(this);
					}

					File.WriteAllText(versionFile, currentVersion);
				}
			}
			else
				File.WriteAllText(versionFile, currentVersion);

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

				// Minimize all windows
				foreach (WindowFormBase window in GetAllOpenedWindows())
				{
					window.UpdateWindowSettings();
					window.WindowState = FormWindowState.Minimized;
				}
			}
			else if (WindowState == FormWindowState.Normal)
			{
				// Open all windows
				foreach (WindowFormBase window in GetAllOpenedWindows())
					window.WindowState = FormWindowState.Normal;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void MainWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			using (new SleepCursor())
			{
				// Remember list/module properties
				int rememberSelected, rememberPosition, rememberSong;

				if (moduleHandler.IsModuleLoaded)
				{
					rememberSelected = moduleListControl.Items.IndexOf(playItem);
					rememberPosition = moduleHandler.PlayingModuleInformation.SongPosition;
					rememberSong = moduleHandler.PlayingModuleInformation.CurrentSong;
				}
				else
				{
					rememberSelected = -1;
					rememberPosition = -1;
					rememberSong = -1;
				}

				// Stop any playing modules
				StopAndFreeModule();
				moduleHandler.FreeAllModules();

				// Close all windows
				CloseWindows();

				// Stop file scanner
				fileScanner.Stop();
				fileScanner = null;

				// Stop the module handler
				CleanupModuleHandler();

				// Remember the module list
				RememberModuleList(rememberSelected, rememberPosition, rememberSong);

				// Close down the database
				CloseDatabase();

				// Save and cleanup the settings
				CleanupSettings();
			}
		}
		#endregion

		#region Menu events

		#region File menu
		/********************************************************************/
		/// <summary>
		/// User selected the settings menu item
		/// </summary>
		/********************************************************************/
		private void Menu_File_OpenUrl_Click(object sender, EventArgs e)
		{
			using (OpenUrlWindowForm dialog = new OpenUrlWindowForm())
			{
				DialogResult result = dialog.ShowDialog(this);
				if ((result != DialogResult.Cancel) && (!string.IsNullOrEmpty(dialog.GetUrl())))
				{
					using (new SleepCursor())
					{
						ModuleListItem listItem = new ModuleListItem(new UrlModuleListItem(dialog.GetName(), dialog.GetUrl()));
						AddItemsToList([ listItem ], result == DialogResult.OK);
					}
				}
			}
		}
		#endregion

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
				settingsWindow = new SettingsWindowForm(agentManager, moduleHandler, this, optionSettings, userSettings);
				settingsWindow.Disposed += (o, args) => { settingsWindow = null; };
				settingsWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// User selected the Audius menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Window_Audius_Click(object sender, EventArgs e)
		{
			if (IsAudiusWindowOpen())
			{
				if (audiusWindow.WindowState == FormWindowState.Minimized)
					audiusWindow.WindowState = FormWindowState.Normal;

				audiusWindow.Activate();
			}
			else
			{
				audiusWindow = new AudiusWindowForm(this, optionSettings);
				audiusWindow.Disposed += (o, args) => { audiusWindow = null; };
				audiusWindow.Show();
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
			OpenHelpWindow("index.html");
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
				aboutWindow = new AboutWindowForm(agentManager, this, optionSettings);
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
				moduleInfoWindow = new ModuleInfoWindowForm(moduleHandler, this, optionSettings, moduleSettings);
				moduleInfoWindow.Disposed += (o, args) => { moduleInfoWindow = null; };
				moduleInfoWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time the clock is updated
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_ClockUpdated(object sender, ClockUpdatedEventArgs e)
		{
			BeginInvoke(() =>
			{
				if (playItem != null)
				{
					// Set the time offset
					timeOccurred = e.Time;

					// Print the module information
					PrintInfo();
				}
			});
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed position
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_PositionChanged(object sender, EventArgs e)
		{
			HandlePositionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed sub-song
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_SubSongChanged(object sender, SubSongChangedEventArgs e)
		{
			HandleSubSongChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player has reached the end
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_EndReached(object sender, EventArgs e)
		{
			HandleEndReached();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			BeginInvoke(() =>
			{
				if (IsModuleInfoWindowOpen())
					moduleInfoWindow.UpdateWindow(e.Line, e.Value);
			});
		}



		/********************************************************************/
		/// <summary>
		/// Is called if the player fails while playing
		/// </summary>
		/********************************************************************/
		private void ModuleHandler_PlayerFailed(object sender, PlayerFailedEventArgs e)
		{
			BeginInvoke(() =>
			{
				ModuleListItem listItem = playItem;
				string playerName = moduleHandler.StaticModuleInformation.PlayerAgentInfo.AgentName;

				StopAndFreeModule();
				moduleHandler.CloseOutputAgent();

				ShowErrorMessage(string.Format(Resources.IDS_ERR_PLAYER_FAILED, playerName, e.ErrorMessage), listItem);
			});
		}
		#endregion

		#region Module list events
		/********************************************************************/
		/// <summary>
		/// The user changed the selection of the modules
		/// </summary>
		/********************************************************************/
		private void ModuleListControl_SelectedIndexChanged(object sender, EventArgs e)
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
		private void ModuleListControl_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// Find out, if an item has been clicked and which one
			int index = moduleListControl.IndexFromPoint(e.Location);
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
		private void ModuleListControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Enter - Load the selected module
			if (e.KeyChar == '\r')
			{
				if (moduleListControl.SelectedItem != null)
				{
					// Stop playing any modules
					StopAndFreeModule();

					// Load and play the selected module
					LoadAndPlayModule(moduleListControl.SelectedItem);
				}
			}
			// Open search popup for alphanumeric characters and wildcards
			else if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '*' || e.KeyChar == '?')
			{
				OpenSearchPopup(e.KeyChar.ToString());
				e.Handled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the drop ends
		/// </summary>
		/********************************************************************/
		private void ModuleListControl_DragDrop(object sender, DragEventArgs e)
		{
			using (new SleepCursor())
			{
				ModuleListControl.DragDropInformation dropInfo = moduleListControl.GetLatestDragAndDropInformation(e);

				switch (dropInfo.Type)
				{
					case ModuleListControl.DragDropType.List:
					{
						// Free any extra loaded modules
						moduleHandler.FreeExtraModules();
						break;
					}

					case ModuleListControl.DragDropType.File:
					{
						int jumpNumber = -1;

						// File or directory dragged from File Explorer
						string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

						switch (dropInfo.DropType)
						{
							case ModuleListControl.FileDropType.Append:
							{
								jumpNumber = moduleListControl.Items.Count;

								AddFilesToList(files, checkForList: true);
								break;
							}

							case ModuleListControl.FileDropType.ClearAndAdd:
							{
								StopAndFreeModule();
								EmptyList();

								AddFilesToList(files, checkForList: true);

								LoadAndPlayModule(0);
								break;
							}

							case ModuleListControl.FileDropType.Insert:
							{
								jumpNumber = dropInfo.IndexOfItemUnderMouseToDrop == -1 ? 0 : dropInfo.IndexOfItemUnderMouseToDrop;
								AddFilesToList(files, dropInfo.IndexOfItemUnderMouseToDrop, true);

								// Free any extra loaded modules
								moduleHandler.FreeExtraModules();
								break;
							}
						}

						if ((jumpNumber != -1) && optionSettings.AddJump)
						{
							// Stop playing any modules and load the first added one
							StopAndFreeModule();
							LoadAndPlayModule(jumpNumber);
						}
						break;
					}
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
			// Show the menu
			addContextMenu.Show(sender);
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
				UpdateControls();

				// Free any extra loaded modules
				moduleHandler.FreeExtraModules();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the swap modules button
		/// </summary>
		/********************************************************************/
		private void SwapModulesButton_Click(object sender, EventArgs e)
		{
			int index1 = moduleListControl.SelectedIndexes[0];
			int index2 = moduleListControl.SelectedIndexes[1];

			moduleListControl.BeginUpdate();

			try
			{
				// Swap the items
				ModuleListItem item1 = moduleListControl.Items[index1];
				ModuleListItem item2 = moduleListControl.Items[index2];

				moduleListControl.Items.RemoveAt(index1);
				moduleListControl.Items.Insert(index1, item2);

				moduleListControl.Items.RemoveAt(index2);
				moduleListControl.Items.Insert(index2, item1);

				// Keep the selection
				moduleListControl.SetSelected(index1, true);
				moduleListControl.SetSelected(index2, true);
			}
			finally
			{
				moduleListControl.EndUpdate();
			}

			// Free any extra loaded modules
			moduleHandler.FreeExtraModules();
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

				// Free any extra loaded modules
				moduleHandler.FreeExtraModules();
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

				// Free any extra loaded modules
				moduleHandler.FreeExtraModules();
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

		#region Add menu events
		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the add files menu item
		/// </summary>
		/********************************************************************/
		private void AddMenu_Files(object sender, EventArgs e)
		{
			if (ShowModuleFileDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
					int jumpNumber;

					// Get the selected item
					int selected = moduleListControl.SelectedIndex;
					if (selected < 0)
					{
						selected = -1;
						jumpNumber = moduleListControl.Items.Count;
					}
					else
						jumpNumber = selected;

					// Add all the files in the module list
					AddFilesToList(moduleFileDialog.FileNames, selected);

					// Free any extra loaded modules
					moduleHandler.FreeExtraModules();

					// Should we load the first added module?
					if (optionSettings.AddJump)
					{
						// Stop playing any modules and load the first added one
						StopAndFreeModule();
						LoadAndPlayModule(jumpNumber);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the add directory menu item
		/// </summary>
		/********************************************************************/
		private void AddMenu_Directory(object sender, EventArgs e)
		{
			if (ShowModuleDirectoryDialog() == DialogResult.OK)
			{
				using (new SleepCursor())
				{
					int jumpNumber;

					// Get the selected item
					int selected = moduleListControl.SelectedIndex;
					if (selected < 0)
					{
						selected = -1;
						jumpNumber = moduleListControl.Items.Count;
					}
					else
						jumpNumber = selected;

					// Add all the files in the module list
					AddFilesToList(new[] { moduleDirectoryDialog.SelectedPath }, selected);

					// Free any extra loaded modules
					moduleHandler.FreeExtraModules();

					// Should we load the first added module?
					if (optionSettings.AddJump)
					{
						// Stop playing any modules and load the first added one
						StopAndFreeModule();
						LoadAndPlayModule(jumpNumber);
					}
				}
			}
		}
		#endregion

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
				moduleListControl.BeginUpdate();

				try
				{
					ModuleListItem[] newList = moduleListControl.Items.OrderBy(i => i).ToArray();

					moduleListControl.Items.Clear();
					moduleListControl.Items.AddRange(newList);
				}
				finally
				{
					moduleListControl.EndUpdate();
				}

				// Free any extra loaded modules
				moduleHandler.FreeExtraModules();
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
				moduleListControl.BeginUpdate();

				try
				{
					ModuleListItem[] newList = moduleListControl.Items.OrderByDescending(i => i).ToArray();

					moduleListControl.Items.Clear();
					moduleListControl.Items.AddRange(newList);
				}
				finally
				{
					moduleListControl.EndUpdate();
				}

				// Free any extra loaded modules
				moduleHandler.FreeExtraModules();
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
				else
					moduleHandler.FreeExtraModules();
			}
		}
		#endregion

		#region List menu events
		/********************************************************************/
		/// <summary>
		/// Is called right before the list context menu is opened
		/// </summary>
		/********************************************************************/
		private void ListContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Get playing flag
			bool isLoaded = moduleHandler.IsModuleLoaded;

			// Enable if a module is playing
			setSubSongMenuItem.Enabled = isLoaded;

			// Enable if a default sub-song has been set on the selected item
			clearSubSongMenuItem.Enabled = moduleListControl.SelectedItems.Any(x => x.DefaultSubSong.HasValue);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the all menu item
		/// </summary>
		/********************************************************************/
		private void ListMenu_SelectAll(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListControl.BeginUpdate();

				try
				{
					moduleListControl.SetSelectedOnAllItems(true);
				}
				finally
				{
					moduleListControl.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the none menu item
		/// </summary>
		/********************************************************************/
		private void ListMenu_SelectNone(object sender, EventArgs e)
		{
			using (new SleepCursor())
			{
				moduleListControl.BeginUpdate();

				try
				{
					moduleListControl.SetSelectedOnAllItems(false);
				}
				finally
				{
					moduleListControl.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the set default sub-song menu
		/// item
		/// </summary>
		/********************************************************************/
		private void ListMenu_SetDefaultSubSong(object sender, EventArgs e)
		{
			if (playItem != null)
			{
				moduleListControl.BeginUpdate();

				try
				{
					playItem.DefaultSubSong = moduleHandler.PlayingModuleInformation.CurrentSong;
				}
				finally
				{
					moduleListControl.EndUpdate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user selects the clear default sub-song menu
		/// item
		/// </summary>
		/********************************************************************/
		private void ListMenu_ClearDefaultSubSong(object sender, EventArgs e)
		{
			moduleListControl.BeginUpdate();

			try
			{
				foreach (ModuleListItem selectedItem in moduleListControl.SelectedItems)
					selectedItem.DefaultSubSong = null;
			}
			finally
			{
				moduleListControl.EndUpdate();
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
					int jumpNumber;

					// Get the selected item
					int selected = moduleListControl.SelectedIndex;
					if (selected < 0)
					{
						selected = -1;
						jumpNumber = moduleListControl.Items.Count;
					}
					else
						jumpNumber = selected;

					// Load the file into the module list
					LoadModuleList(loadListFileDialog.FileName, selected);

					// Free any extra loaded modules
					moduleHandler.FreeExtraModules();

					// Should we load the first added module?
					if (optionSettings.AddJump)
					{
						// Stop playing any modules and load the first added one
						StopAndFreeModule();
						LoadAndPlayModule(jumpNumber);
					}
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
					int newIndex = moduleListControl.Items.IndexOf(playItem) - 1;
					int count = moduleListControl.Items.Count;

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
				int newIndex = moduleListControl.Items.IndexOf(playItem) + 1;
				int count = moduleListControl.Items.Count;

				if (newIndex == count)
				{
					// We are, now check what we have to do
					ModuleSettings.ModuleListEndAction listEnd = moduleSettings.ModuleListEnd;

					if (listEnd == ModuleSettings.ModuleListEndAction.Eject)
					{
						// Eject the module
						StopAndFreeModule();
						newIndex = -1;
					}
					else
					{
						if ((count == 1) || (listEnd == ModuleSettings.ModuleListEndAction.Loop))
							newIndex = -1;
						else
							newIndex = 0;
					}
				}

				if (newIndex >= 0)
				{
					// Stop playing the module
					StopAndFreeModule();

					// Load and play the new module
					LoadAndPlayModule(newIndex);
				}
			}
		}
		#endregion

		#region Functions events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the favorite song system button
		/// </summary>
		/********************************************************************/
		private void FavoritesButton_Click(object sender, EventArgs e)
		{
			if (IsFavoriteSongSystemWindowOpen())
			{
				if (favoriteSongSystemWindow.WindowState == FormWindowState.Minimized)
					favoriteSongSystemWindow.WindowState = FormWindowState.Normal;

				favoriteSongSystemWindow.Activate();
			}
			else
			{
				favoriteSongSystemWindow = new FavoriteSongSystemForm(this, database, optionSettings);
				favoriteSongSystemWindow.Disposed += (o, args) => { favoriteSongSystemWindow = null; };
				favoriteSongSystemWindow.Show();
			}
		}



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
				sampleInfoWindow = new SampleInfoWindowForm(agentManager, moduleHandler, this, optionSettings);
				sampleInfoWindow.Disposed += (o, args) => { sampleInfoWindow = null; };
				sampleInfoWindow.Show();
			}
		}
		#endregion

		#region Never ending events
		/********************************************************************/
		/// <summary>
		/// Is called when the never ending timeout has been reached
		/// </summary>
		/********************************************************************/
		private void NeverEndingTimer_Tick(object sender, EventArgs e)
		{
			HandleEndReached();
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
			subSongMultiply = 0;
			startedSubSong = moduleHandler.PlayingModuleInformation.CurrentSong;
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
		private void RetrieveStartupFiles()
		{
			using (new SleepCursor())
			{
				bool loadList = false;

				// Get the start scan path
				string path = pathSettings.StartScan;

				if (optionSettings.RememberList)
				{
					string fileName = Path.Combine(Settings.SettingsDirectory, "___RememberList.npml");

					if (File.Exists(fileName))
					{
						loadList = true;

						// Load the list
						LoadModuleList(fileName);

						// Get the remember information
						RememberListSettings infoSettings = new RememberListSettings();

						// Load the module if anyone was selected
						if (infoSettings.ListPosition != -1)
						{
							// Load module into memory
							LoadAndPlayModule(infoSettings.ListPosition, infoSettings.SubSong, infoSettings.ModulePosition);
						}
					}
				}

				// If we haven't loaded any remembered module list, then check
				// to see if the user have a "start path"
				if (!loadList)
				{
					// Add the files in the module list if there is any path
					if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
					{
						// Add all the files in the directory
						List<ModuleListItem> itemList = new List<ModuleListItem>();

						string[] listExtensions = ListFactory.GetExtensions();
						string[] archiveExtensions = new ArchiveDetector(agentManager).GetExtensions();

						AddDirectoryToList(path, itemList, listExtensions, archiveExtensions, false);

						if (itemList.Count > 0)
						{
							// Add the items to the list
							moduleListControl.BeginUpdate();

							try
							{
								moduleListControl.Items.AddRange(itemList);
							}
							finally
							{
								moduleListControl.EndUpdate();
							}

							// Update the window
							UpdateControls();

							// Load and play the first module
							LoadAndPlayModule(0);
						}
					}
				}

				StartupHandler(Environment.GetCommandLineArgs().Skip(1).ToArray());
			}

			// Tell the file scanner to scan the new items
			fileScanner.ScanItems(moduleListControl.Items.Take(moduleListControl.Items.Count));
		}



		/********************************************************************/
		/// <summary>
		/// Add given files from Explorer to the list
		/// </summary>
		/********************************************************************/
		private void AddFilesFromStartupHandler(string[] arguments)
		{
			BeginInvoke(() =>
			{
				// Because Explorer calls the application for each file selected
				// when multiple files are opened, we have a timeout for 1 second
				// before we consider it as a new bunch
				bool continuing = (DateTime.Now.Ticks - lastAddedTimeFromExplorer) < TimeSpan.TicksPerSecond;

				using (new SleepCursor())
				{
					// Check the "add to list" option
					if (!optionSettings.AddToList && !continuing)
					{
						// Stop playing module if any
						StopAndFreeModule();

						// Clear the module list
						EmptyList();
					}

					// Get the previous number of items in the list
					int listCount = moduleListControl.Items.Count;

					// Add all the files to the module list
					AddFilesToList(arguments, checkForList: true);

					// Check if the added module should be loaded
					if ((listCount == 0) || (optionSettings.AddJump && !continuing))
						LoadAndPlayModule(listCount);
				}

				lastAddedTimeFromExplorer = DateTime.Now.Ticks;
			});
		}



		/********************************************************************/
		/// <summary>
		/// Close down the database handler
		/// </summary>
		/********************************************************************/
		private void CloseDatabase()
		{
			if (optionSettings.UseDatabase)
				database.SaveDatabase();
			else
				database.DeleteDatabase();

			database.CloseDown();
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
			userSettings = DependencyInjection.GetDefaultProvider().GetService<ISettings>();
			userSettings.LoadSettings("Settings");
			FixSettings();

			// Setup setting wrappers
			optionSettings = new OptionSettings(userSettings);
			moduleSettings = new ModuleSettings(userSettings);
			pathSettings = new PathSettings(userSettings);
			soundSettings = new SoundSettings(userSettings);

			LoadWindowSettings("MainWindow");
			mainWindowSettings = new MainWindowSettings(allWindowSettings);

			// Set the time format
			timeFormat = mainWindowSettings.Time;

			// Set the master volume
			masterVolumeTrackBar.Value = mainWindowSettings.MasterVolume;

			// Set the mute status
			muteCheckButton.Checked = mainWindowSettings.Mute;

			// Set the loop song status
			loopCheckButton.Checked = mainWindowSettings.LoopSong;

			// Initialize base class
			SetOptions(this, optionSettings);
		}



		/********************************************************************/
		/// <summary>
		/// Will fix the settings if needed
		/// </summary>
		/********************************************************************/
		private void FixSettings()
		{
			int version = userSettings.GetIntEntry("General", "Version", 1);

			if (version == 1)
			{
				ConvertSettingsToVersion2();
				version++;
			}

			if (version == 2)
			{
				ConvertSettingsToVersion3();
				version++;
			}

			userSettings.SetIntEntry("General", "Version", version);
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the settings from version 1 to version 2
		/// </summary>
		/********************************************************************/
		private void ConvertSettingsToVersion2()
		{
			// Move some of the options to the modules section
			bool boolValue = userSettings.GetBoolEntry("Options", "DoubleBuffering", false);
			userSettings.SetBoolEntry("Modules", "DoubleBuffering", boolValue);

			int intValue = userSettings.GetIntEntry("Options", "DoubleBufferingEarlyLoad", 2);
			userSettings.SetIntEntry("Modules", "DoubleBufferingEarlyLoad", intValue);

			ModuleSettings.ModuleErrorAction enum1Value = userSettings.GetEnumEntry("Options", "ModuleError", ModuleSettings.ModuleErrorAction.ShowError);
			userSettings.SetEnumEntry("Modules", "ModuleError", enum1Value);

			boolValue = userSettings.GetBoolEntry("Options", "NeverEnding", false);
			userSettings.SetBoolEntry("Modules", "NeverEnding", boolValue);

			intValue = userSettings.GetIntEntry("Options", "NeverEndingTimeout", 180);
			userSettings.SetIntEntry("Modules", "NeverEndingTimeout", intValue);

			ModuleSettings.ModuleListEndAction enum2Value = userSettings.GetEnumEntry("Options", "ModuleListEnd", ModuleSettings.ModuleListEndAction.JumpToStart);
			userSettings.SetEnumEntry("Modules", "ModuleListEnd", enum2Value);

			userSettings.RemoveEntry("Options", "DoubleBuffering");
			userSettings.RemoveEntry("Options", "DoubleBufferingEarlyLoad");
			userSettings.RemoveEntry("Options", "ModuleError");
			userSettings.RemoveEntry("Options", "NeverEnding");
			userSettings.RemoveEntry("Options", "NeverEndingTimeout");
			userSettings.RemoveEntry("Options", "ModuleListEnd");
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the settings from version 2 to version 3
		/// </summary>
		/********************************************************************/
		private void ConvertSettingsToVersion3()
		{
			bool boolValue = userSettings.GetBoolEntry("Sound", "Surround");
			userSettings.SetEnumEntry("Sound", "SurroundMode", boolValue ? SurroundMode.DolbyProLogic : SurroundMode.None);

			userSettings.RemoveEntry("Sound", "Surround");
		}



		/********************************************************************/
		/// <summary>
		/// Will save and destroy the settings
		/// </summary>
		/********************************************************************/
		private void CleanupSettings()
		{
			userSettings.SaveSettings();
			userSettings = null;

			soundSettings = null;
			pathSettings = null;
			moduleSettings = null;
			optionSettings = null;
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
			// Create the menu
			CreateMainMenu();

			// Create button menus
			CreateAddContextMenu();
			CreateSortContextMenu();
			CreateListContextMenu();
			CreateDiskContextMenu();

			// Set tooltip on all controls
			SetTooltips();

			// Update the list controls
			UpdateListControls();

			// Update the tape deck buttons
			UpdateTapeDeck();

			// Print the module information
			PrintInfo();

			// Enable/disable user interface stuff
			EnableUserInterfaceSettings();
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

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_FILE_OPEN_URL);
			menuItem.ShortcutKeys = Keys.Shift | Keys.Alt | Keys.O;
			menuItem.Click += Menu_File_OpenUrl_Click;
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

			windowMenuItem.DropDownItems.Add(new ToolStripSeparator());

			menuItem = new ToolStripMenuItem(Resources.IDS_MENU_WINDOW_AUDIUS);
			menuItem.Click += Menu_Window_Audius_Click;
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
			{
				if (agentInfo.Enabled)
					AddAgentToMenu(agentInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create the add button context menu
		/// </summary>
		/********************************************************************/
		private void CreateAddContextMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_ADD_FILES);
			item.Image = Resources.IDB_FILE;
			item.Click += AddMenu_Files;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_ADD_DIRECTORY);
			item.Image = Resources.IDB_DIRECTORY;
			item.Click += AddMenu_Directory;
			menuItems.Items.Add(item);

			addContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Create the sort button context menu
		/// </summary>
		/********************************************************************/
		private void CreateSortContextMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_SORT_SORT_AZ);
			item.Image = Resources.IDB_AZ;
			item.Click += SortMenu_AZ;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_SORT_SORT_ZA);
			item.Image = Resources.IDB_ZA;
			item.Click += SortMenu_ZA;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_SORT_SHUFFLE);
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
		private void CreateListContextMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_LIST_SELECT_ALL);
			item.Click += ListMenu_SelectAll;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_LIST_SELECT_NONE);
			item.Click += ListMenu_SelectNone;
			menuItems.Items.Add(item);

			KryptonContextMenuSeparator separatorItem = new KryptonContextMenuSeparator();
			menuItems.Items.Add(separatorItem);

			setSubSongMenuItem = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_LIST_SET_SUBSONG);
			setSubSongMenuItem.Image = Resources.IDB_SET_SUBSONG;
			setSubSongMenuItem.Click += ListMenu_SetDefaultSubSong;
			menuItems.Items.Add(setSubSongMenuItem);

			clearSubSongMenuItem = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_LIST_CLEAR_SUBSONG);
			clearSubSongMenuItem.Image = Resources.IDB_CLEAR_SUBSONG;
			clearSubSongMenuItem.Click += ListMenu_ClearDefaultSubSong;
			menuItems.Items.Add(clearSubSongMenuItem);

			listContextMenu.Items.Add(menuItems);
		}



		/********************************************************************/
		/// <summary>
		/// Create the disk button context menu
		/// </summary>
		/********************************************************************/
		private void CreateDiskContextMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();

			KryptonContextMenuItem item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_DISK_LOAD);
			item.Image = Resources.IDB_LOAD;
			item.Click += DiskMenu_LoadList;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_DISK_APPEND);
			item.Click += DiskMenu_AppendList;
			menuItems.Items.Add(item);

			item = new KryptonContextMenuItem(Resources.IDS_CONTEXTMENU_DISK_SAVE);
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
			toolTip.SetToolTip(listButton, Resources.IDS_TIP_MAIN_LIST);
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
				moduleInfoWindow = new ModuleInfoWindowForm(moduleHandler, this, optionSettings, moduleSettings);
				moduleInfoWindow.Show();
			}

			if (mainWindowSettings.OpenFavoriteSongSystemWindow && optionSettings.UseDatabase)
			{
				favoriteSongSystemWindow = new FavoriteSongSystemForm(this, database, optionSettings);
				favoriteSongSystemWindow.Show();
			}

			if (mainWindowSettings.OpenSampleInformationWindow)
			{
				sampleInfoWindow = new SampleInfoWindowForm(agentManager, moduleHandler, this, optionSettings);
				sampleInfoWindow.Show();
			}

			if (mainWindowSettings.OpenAudiusWindow)
			{
				audiusWindow = new AudiusWindowForm(this, optionSettings);
				audiusWindow.Show();
			}

			OpenEqualizerWindow();

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

			// Destroy the module directory dialog
			if (moduleDirectoryDialog != null)
			{
				moduleDirectoryDialog.Dispose();
				moduleDirectoryDialog = null;
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
			mainWindowSettings.Mute = muteCheckButton.Checked;
			mainWindowSettings.LoopSong = loopCheckButton.Checked;

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

			// Close the help window
			if (IsHelpWindowOpen())
				helpWindow.Close();

			helpWindow = null;

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

			// Close the favorite song system window
			openAgain = IsFavoriteSongSystemWindowOpen();
			if (openAgain)
				favoriteSongSystemWindow.Close();

			favoriteSongSystemWindow = null;
			mainWindowSettings.OpenFavoriteSongSystemWindow = openAgain;

			// Close the sample information window
			openAgain = IsSampleInfoWindowOpen();
			if (openAgain)
				sampleInfoWindow.Close();

			sampleInfoWindow = null;
			mainWindowSettings.OpenSampleInformationWindow = openAgain;

			// Close the Audius window
			openAgain = IsAudiusWindowOpen();
			if (openAgain)
				audiusWindow.Close();

			audiusWindow = null;
			mainWindowSettings.OpenAudiusWindow = openAgain;

			CloseEqualizerWindow();
		}



		/********************************************************************/
		/// <summary>
		/// Get all opened windows
		/// </summary>
		/********************************************************************/
		private IEnumerable<WindowFormBase> GetAllOpenedWindows()
		{
			if (IsHelpWindowOpen())
				yield return helpWindow;

			if (IsModuleInfoWindowOpen())
				yield return moduleInfoWindow;

			if (IsFavoriteSongSystemWindowOpen())
				yield return favoriteSongSystemWindow;

			if (IsSampleInfoWindowOpen())
				yield return sampleInfoWindow;

			if (IsAudiusWindowOpen())
				yield return audiusWindow;

			foreach (WindowFormBase window in EnumerateEqualizerWindow())
				yield return window;

			if (IsSettingsWindowOpen())
				yield return settingsWindow;

			// All agent settings windows
			lock (openAgentSettings)
			{
				foreach (AgentSettingsWindowForm agentSettingsWindow in openAgentSettings.Values)
					yield return agentSettingsWindow;
			}

			// All agent display windows
			lock (openAgentDisplays)
			{
				foreach (AgentDisplayWindowForm agentDisplayWindow in openAgentDisplays.Values)
					yield return agentDisplayWindow;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure to refresh all the windows
		/// </summary>
		/********************************************************************/
		private void RefreshWindows(bool onLoad)
		{
			if (IsModuleInfoWindowOpen())
				moduleInfoWindow.RefreshWindow(onLoad);

			if (IsSampleInfoWindowOpen())
				sampleInfoWindow.RefreshWindow();

			if (onLoad && IsFavoriteSongSystemWindowOpen())
				favoriteSongSystemWindow.RefreshWindow();

			if (IsSettingsWindowOpen())
				settingsWindow.RefreshWindow();
		}



		/********************************************************************/
		/// <summary>
		/// Check if the help window is open
		/// </summary>
		/********************************************************************/
		private bool IsHelpWindowOpen()
		{
			return (helpWindow != null) && !helpWindow.IsDisposed;
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
		/// Check if the favorite song system window is open
		/// </summary>
		/********************************************************************/
		private bool IsFavoriteSongSystemWindowOpen()
		{
			return (favoriteSongSystemWindow != null) && !favoriteSongSystemWindow.IsDisposed;
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
		/// Check if the Audius window is open
		/// </summary>
		/********************************************************************/
		private bool IsAudiusWindowOpen()
		{
			return (audiusWindow != null) && !audiusWindow.IsDisposed;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the window title
		/// </summary>
		/********************************************************************/
		private void SetTitle()
		{
			string title = Resources.IDS_MAIN_TITLE;

			// Show we change the title
			if (optionSettings.ShowNameInTitle && moduleHandler.IsModuleLoaded)
			{
				string moduleTitle = moduleHandler.StaticModuleInformation.Title;

				if (string.IsNullOrEmpty(moduleTitle))
				{
					MultiFileInfo fileInfo = GetFileInfo();
					if (fileInfo != null)
						moduleTitle = fileInfo.DisplayName;
				}

				if (!string.IsNullOrEmpty(moduleTitle))
					title += " / " + moduleTitle;
			}

			Text = title;
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

			// Print the module information
			PrintInfo();

			// Change the window title
			SetTitle();

			// Update the list item
			SetTimeOnItem(playItem, playingModuleInfo.SongTotalTime);

			if (playItem != null)
			{
				// Check to see if the playing item can be seen
				int itemIndex = moduleListControl.Items.IndexOf(playItem);

				// Make sure the item can be seen
				if (!moduleListControl.IsItemVisible(itemIndex))
					moduleListControl.TopIndex = itemIndex;

				// Add the new index to the random list
				randomList.Add(itemIndex);

				// Do we need to remove any previous items
				if (randomList.Count > (moduleListControl.Items.Count / 3))
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
			moduleListControl.BeginUpdate();

			try
			{
				// First deselect any previous item
				if (playItem != null)
				{
					playItem.IsPlaying = false;
					moduleListControl.InvalidateItem(moduleListControl.Items.IndexOf(playItem));
				}

				// Remember the item
				playItem = listItem;

				if (playItem != null)
				{
					playItem.IsPlaying = true;
					moduleListControl.InvalidateItem(moduleListControl.Items.IndexOf(playItem));
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
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
				listTime -= listItem.Duration;
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
				TimeSpan prevTime = listItem.Duration;

				// Change the time on the item
				listItem.Duration = time;

				// Find index of the item
				int index = moduleListControl.Items.IndexOf(listItem);
				if (index != -1)
				{
					// And update it in the list
					moduleListControl.InvalidateItem(index);

					// Now calculate the new list time
					listTime = listTime - prevTime + time;

					// And show it
					UpdateTimes();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update all user controls
		/// </summary>
		/********************************************************************/
		private void UpdateControls()
		{
			UpdateListCount();
			UpdateTimes();
			UpdateTapeDeck();
		}



		/********************************************************************/
		/// <summary>
		/// Will update all the tape deck buttons
		/// </summary>
		/********************************************************************/
		private void UpdateTapeDeck()
		{
			// Get the number of items in the list
			int count = moduleListControl.Items.Count;

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

			foreach (ModuleListItem listItem in moduleListControl.SelectedItems)
			{
				// Add the time to the total
				selectedTime += listItem.Duration;
			}

			// Build the selected time string and list time string
			string selStr = selectedTime.ToFormattedString();
			string listStr = listTime.ToFormattedString();

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
			int selected = moduleListControl.SelectedItems.Count;
			int total = moduleListControl.Items.Count;

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
			if (moduleListControl.SelectedIndex == -1)
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
				if (moduleListControl.SelectedItems.Count == 2)
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
					if (percent > 100)
						percent = 100;

					posStr = string.Format(Resources.IDS_POSITION, songPos, songLength, percent);

					// Set the position slider
					if (allowPosSliderUpdate && (positionTrackBar.Value != percent))
						positionTrackBar.Value = percent;
				}
				else
				{
					// Set the position string to n/a
					posStr = Resources.IDS_NOPOSITION;

					// If never ending timeout is on, we need to update the position slider
					if (neverEndingStarted)
					{
						// Calculate the percent
						int percent = Math.Min((int)(timeOccurred.TotalMilliseconds * 100 / neverEndingTimeout.TotalMilliseconds), 100);

						// Set the position slider
						if (allowPosSliderUpdate && (positionTrackBar.Value != percent))
							positionTrackBar.Value = percent;
					}
				}

				// Create sub-song string
				int currentSong = playingModuleInfo.CurrentSong + 1;
				int maxSong = staticModuleInfo.MaxSongNumber;

				if (maxSong == 0)
					subStr = Resources.IDS_NOSUBSONGS;
				else
					subStr = string.Format(Resources.IDS_SUBSONGS, currentSong, maxSong);

				// Format the time string
				if (timeFormat == MainWindowSettings.TimeFormat.Elapsed)
					timeStr = string.Format("{0} {1}", Resources.IDS_TIME, timeOccurred.ToFormattedString(true));
				else
				{
					// Calculate the remaining time
					TimeSpan tempSpan = playingModuleInfo.SongTotalTime - timeOccurred;

					// Check to see if we got a negative number
					if (tempSpan.TotalMilliseconds < 0)
						tempSpan = new TimeSpan(0);

					// Format the string
					timeStr = string.Format("{0} -{1}", Resources.IDS_TIME, tempSpan.ToFormattedString(true));
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
		#endregion

		#region Module handling
		/********************************************************************/
		/// <summary>
		/// Will load all available agents
		/// </summary>
		/********************************************************************/
		private void LoadAgents(Manager.LoadAgentProgress loadProgress)
		{
			agentManager = new Manager();
			agentManager.LoadAllAgents(loadProgress);

			// Fix agent settings
			userSettings.RemoveSection("Players Agents");       // Not used anymore

			// Build a list that holds which agents that still have some active types
			Dictionary<Guid, bool> agentsEnableStatus = new Dictionary<Guid, bool>();

			FixAgentSettings("Formats", agentsEnableStatus, agentManager.GetAllAgents(Manager.AgentType.Players).Union(agentManager.GetAllAgents(Manager.AgentType.ModuleConverters)));
			FixAgentSettings("Output", agentsEnableStatus, agentManager.GetAllAgents(Manager.AgentType.Output));
			FixAgentSettings("Visuals", agentsEnableStatus, agentManager.GetAllAgents(Manager.AgentType.Visuals));
			FixAgentSettings("Decrunchers", agentsEnableStatus, agentManager.GetAllAgents(Manager.AgentType.FileDecrunchers).Union(agentManager.GetAllAgents(Manager.AgentType.ArchiveDecrunchers)));

			// Flush all agents that is disabled
			foreach (Guid agentId in agentsEnableStatus.Where(pair => pair.Value == false).Select(pair => pair.Key))
				agentManager.UnloadAgent(agentId);

			// And finally, save the settings to disk
			userSettings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Fix agent settings (removed non-exiting ones and add unknown)
		/// plus tell the manager about which agents that are disabled
		/// </summary>
		/********************************************************************/
		private void FixAgentSettings(string prefix, Dictionary<Guid, bool> agentsEnableStatus, IEnumerable<AgentInfo> agents)
		{
			string section = prefix + " Agents";

			// Build lookup list
			Dictionary<Guid, AgentInfo> allAgents = agents.ToDictionary(agentInfo => agentInfo.TypeId, agentInfo => agentInfo);

			// Append the agents to the enable status dictionary, if not already there
			foreach (AgentInfo agentInfo in allAgents.Values)
				agentsEnableStatus.TryAdd(agentInfo.AgentId, false);

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
			// exist in the settings file
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
						agentInfo.Enabled = enabled;

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
					agentsEnableStatus[agentInfo.AgentId] = true;
			}
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
			moduleHandler.SetMuteStatus(muteCheckButton.Checked);
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
		private void LoadAndPlayModule(int index, int subSong = -1, int startPos = -1)
		{
			if (index < moduleListControl.Items.Count)
				LoadAndPlayModule(moduleListControl.Items[index], subSong, startPos);
		}



		/********************************************************************/
		/// <summary>
		/// Will load and play the given module
		/// </summary>
		/********************************************************************/
		private void LoadAndPlayModule(ModuleListItem listItem, int subSong = -1, int startPos = -1)
		{
			using (new SleepCursor())
			{
				bool ok = moduleHandler.LoadAndPlayModule(listItem, subSong, startPos);
				if (ok)
				{
					// Initialize other stuff in the window
					InitSubSongs();

					// Mark the item in the list
					ChangePlayItem(listItem);

					// Initialize controls
					InitControls();

					// Update database
					UpdateDatabase(listItem);

					// And refresh other windows
					RefreshWindows(true);
				}
				else
				{
					// Free loaded module
					StopAndFreeModule();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will play the module already loaded
		/// </summary>
		/********************************************************************/
		private void PlayNextModule(int index)
		{
			ModuleListItem listItem = moduleListControl.Items[index];

			if (moduleHandler.PlayModule(listItem))
			{
				// Initialize other stuff in the window
				InitSubSongs();

				// Mark the item in the list
				ChangePlayItem(listItem);

				// Initialize controls
				InitControls();

				// Update database
				UpdateDatabase(listItem);

				// And refresh other windows
				RefreshWindows(true);
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
			SetTitle();

			// Update the other windows
			RefreshWindows(false);
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
			if (moduleHandler.StartSong(playItem, newSong))
			{
				startedSubSong = newSong;

				// Initialize all the controls
				InitControls();

				// Refresh all windows
				RefreshWindows(false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update the database
		/// </summary>
		/********************************************************************/
		private void UpdateDatabase(ModuleListItem listItem)
		{
			// Update database if enabled
			if (optionSettings.UseDatabase)
			{
				if (listItem.ListItem is IStreamModuleListItem)
					return;

				ModuleDatabaseInfo moduleDatabaseInfo = database.RetrieveInformation(listItem.ListItem.Source);
				if (moduleDatabaseInfo != null)
					moduleDatabaseInfo = new ModuleDatabaseInfo(moduleHandler.PlayingModuleInformation.SongTotalTime, moduleDatabaseInfo.ListenCount + 1, DateTime.Now);
				else
					moduleDatabaseInfo = new ModuleDatabaseInfo(moduleHandler.PlayingModuleInformation.SongTotalTime, 1, DateTime.Now);

				database.StoreInformation(listItem.ListItem.Source, moduleDatabaseInfo);
			}
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
					// Change the position
					moduleHandler.SetSongPosition(newPosition);

					// Show it to the user
					PrintInfo();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle position changed event
		/// </summary>
		/********************************************************************/
		private void HandlePositionChanged()
		{
			BeginInvoke(() =>
			{
				// Print the information
				PrintInfo();

				// If module loop is off and double buffering is enabled,
				// check if it's time to load the next module in the list,
				// except if "next sub-song" option is enabled and not playing
				// the last sub-song
				if (!loopCheckButton.Checked && moduleSettings.DoubleBuffering && ((moduleSettings.ModuleEnd != ModuleSettings.ModuleEndAction.NextSubSong) || ((startedSubSong + 1) == moduleHandler.StaticModuleInformation.MaxSongNumber)))
				{
					// Is the file already loaded?
					if (!moduleHandler.IsDoubleBufferingModuleLoaded && (playItem != null))
					{
						// Everything is enabled, check if it's time to load
						int songLength = moduleHandler.PlayingModuleInformation.SongLength;
						int earlyLoad = moduleSettings.DoubleBufferingEarlyLoad;
						ModuleSettings.ModuleListEndAction listEnd = moduleSettings.ModuleListEnd;
						int newPos = moduleHandler.PlayingModuleInformation.SongPosition;

						if (newPos >= (songLength - earlyLoad))
						{
							// Check to see if we have to load the module
							int curPlay = moduleListControl.Items.IndexOf(playItem);
							int count = moduleListControl.Items.Count;

							// Are we at the end of the list?
							bool load = true;
							int newPlay = 0;

							if ((curPlay + 1) == count)
							{
								if ((count == 1) || (listEnd != ModuleSettings.ModuleListEndAction.JumpToStart))
									load = false;
							}
							else
							{
								// Just go to the next module
								newPlay = curPlay + 1;
							}

							// Load next module
							if (load)
							{
								using (new SleepCursor())
								{
									// If output agent has changed, do not load the module
									if (moduleHandler.OutputAgentInfo.Enabled && (moduleHandler.OutputAgentInfo.TypeId == soundSettings.OutputAgent))
										moduleHandler.LoadAndInitModule(moduleListControl.Items[newPlay], showError: false);
								}
							}
						}
					}
				}
			});
		}



		/********************************************************************/
		/// <summary>
		/// Handle sub-song changed event
		/// </summary>
		/********************************************************************/
		private void HandleSubSongChanged()
		{
			BeginInvoke(() =>
			{
				SetTimeOnItem(playItem, moduleHandler.PlayingModuleInformation.SongTotalTime);
				PrintInfo();
				UpdateTapeDeck();
			});
		}



		/********************************************************************/
		/// <summary>
		/// Handle end reached event
		/// </summary>
		/********************************************************************/
		private void HandleEndReached()
		{
			BeginInvoke((ModuleListItem itemToEnd) =>
			{
				lock (processingEndReached)
				{
					if ((playItem != null) && (playItem == itemToEnd))
					{
						// Check to see if there is module loop on
						if (loopCheckButton.Checked)
						{
							// It is, so update the position as well, so it starts over
							HandlePositionChanged();
						}
						else
						{
							if (moduleSettings.ModuleEnd == ModuleSettings.ModuleEndAction.NextSubSong)
							{
								if ((startedSubSong + 1) < moduleHandler.StaticModuleInformation.MaxSongNumber)
								{
									startedSubSong++;

									StartSong(startedSubSong);
									return;
								}
							}

							bool loadNext = true;

							// Get the number of modules in the list
							int count = moduleListControl.Items.Count;

							// Get the index of the current playing module
							int curPlay = moduleListControl.Items.IndexOf(playItem);

							// The next module to load
							int newPlay = curPlay + 1;

							// Test to see if we are at the end of the list
							if (newPlay == count)
							{
								// We are, now check what we have to do
								ModuleSettings.ModuleListEndAction listEnd = moduleSettings.ModuleListEnd;

								if (listEnd == ModuleSettings.ModuleListEndAction.Eject)
								{
									// Eject the module
									StopAndFreeModule();
									loadNext = false;
								}
								else
								{
									if ((count == 1) || (listEnd == ModuleSettings.ModuleListEndAction.Loop))
										loadNext = false;
									else
										newPlay = 0;
								}
							}

							// Should we load the next module?
							if (loadNext)
							{
								if (moduleSettings.DoubleBuffering && moduleHandler.IsDoubleBufferingModuleLoaded)
								{
									// Double buffering is on and the next module has already been loaded, so just
									// start to play it
									PlayNextModule(newPlay);
								}
								else
								{
									// Free the module
									StopAndFreeModule();

									// Load the module
									LoadAndPlayModule(newPlay);
								}
							}
						}
					}
				}
			}, playItem);
		}
		#endregion

		#region File handling
		/********************************************************************/
		/// <summary>
		/// Will append the given file to the list given
		/// </summary>
		/********************************************************************/
		private void AddSingleFileToList(string fileName, List<ModuleListItem> list, string[] listExtensions, string[] archiveExtensions, bool checkForList)
		{
			try
			{
				IMultiFileLoader loader = null;

				string extension = Path.GetExtension(fileName);
				if (!string.IsNullOrEmpty(extension))
					extension = extension.Substring(1).ToLower();

				if (extension == "lnk")
				{
					fileName = FindFileNameFromShortcut(fileName);

					extension = Path.GetExtension(fileName);
					if (!string.IsNullOrEmpty(extension))
						extension = extension.Substring(1).ToLower();
				}

				if (checkForList)
				{
					if (listExtensions.Contains(extension))
					{
						using (FileStream fs = File.OpenRead(fileName))
						{
							loader = ListFactory.Create(fs, extension);
							if (loader != null)
							{
								foreach (MultiFileInfo info in loader.LoadList(Path.GetDirectoryName(fileName), fs, extension))
									list.Add(ListItemConverter.Convert(info));
							}
						}
					}
				}

				if (loader == null)
				{
					if (archiveExtensions.Contains(extension))
					{
						// Check if the file is an archive
						ArchiveDetector detector = new ArchiveDetector(agentManager);

						bool isArchive = detector.IsArchive(fileName);
						if (isArchive)
						{
							foreach (string archiveFileName in detector.GetEntries(fileName))
								list.Add(new ModuleListItem(new ArchiveFileModuleListItem(archiveFileName)));
						}
						else
						{
							// Just a plain file
							list.Add(new ModuleListItem(new SingleFileModuleListItem(fileName)));
						}
					}
					else
					{
						// Just a plain file
						list.Add(new ModuleListItem(new SingleFileModuleListItem(fileName)));
					}
				}
			}
			catch (Exception ex)
			{
				// Show error
				ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_ADD_ITEMS, ex.Message));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will append all the files inside the given directory to the list
		/// given
		/// </summary>
		/********************************************************************/
		private void AddDirectoryToList(string directory, List<ModuleListItem> list, string[] listExtensions, string[] archiveExtensions, bool checkForList)
		{
			// First go through all the files
			foreach (string fileName in Directory.EnumerateFiles(directory))
				AddSingleFileToList(fileName, list, listExtensions, archiveExtensions, checkForList);

			// Now go through all the directories
			foreach (string directoryName in Directory.EnumerateDirectories(directory))
				AddDirectoryToList(directoryName, list, listExtensions, archiveExtensions, checkForList);
		}



		/********************************************************************/
		/// <summary>
		/// Will try to get the file name from a shortcut
		/// </summary>
		/********************************************************************/
		private string FindFileNameFromShortcut(string fileName)
		{
			string pathOnly = Path.GetDirectoryName(fileName);
			string fileNameOnly = Path.GetFileName(fileName);

			Shell32.Shell shell = new Shell32.Shell();
			Shell32.Folder folder = shell.NameSpace(pathOnly);
			Shell32.FolderItem folderItem = folder.ParseName(fileNameOnly);

			if ((folderItem == null) || !folderItem.IsLink)
				return fileName;

			return ((Shell32.ShellLinkObject)folderItem.GetLink).Path;
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
				moduleFileDialog.InitialDirectory = null;   // Clear so it won't start in the initial directory again, but in current directory next time it is opened

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will show the directory dialog where the user can select a
		/// directory to put in the module list
		/// </summary>
		/********************************************************************/
		private DialogResult ShowModuleDirectoryDialog()
		{
			// Create the dialog if not already created
			if (moduleDirectoryDialog == null)
			{
				moduleDirectoryDialog = new FolderBrowserDialog();
				moduleDirectoryDialog.InitialDirectory = pathSettings.Modules;
			}

			DialogResult result = moduleDirectoryDialog.ShowDialog();
			if (result == DialogResult.OK)
				moduleDirectoryDialog.InitialDirectory = null;   // Clear so it won't start in the initial directory again, but in current directory next time it is opened

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
				loadListFileDialog.InitialDirectory = null; // Clear so it won't start in the initial directory again, but in current directory next time it is opened

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
				saveListFileDialog.InitialDirectory = null; // Clear so it won't start in the initial directory again, but in current directory next time it is opened

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
				string extension = Path.GetExtension(fileName);
				if (!string.IsNullOrEmpty(extension))
					extension = extension.Substring(1).ToLower();

				using (FileStream fs = File.OpenRead(fileName))
				{
					IMultiFileLoader loader = ListFactory.Create(fs, extension);
					if (loader == null)
						ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_UNKNOWN_LIST_FORMAT, fileName));
					else
					{
						List<ModuleListItem> tempList = new List<ModuleListItem>();

						foreach (MultiFileInfo info in loader.LoadList(Path.GetDirectoryName(fileName), fs, extension))
							tempList.Add(ListItemConverter.Convert(info));

						int currentCount = moduleListControl.Items.Count;

						moduleListControl.BeginUpdate();

						try
						{
							if (index == -1)
								moduleListControl.Items.AddRange(tempList);
							else
							{
								for (int i = tempList.Count - 1; i >= 0; i--)
									moduleListControl.Items.Insert(index, tempList[i]);
							}
						}
						finally
						{
							moduleListControl.EndUpdate();
						}

						// Update the controls
						UpdateControls();

						// Tell the file scanner to scan the new items
						fileScanner.ScanItems(moduleListControl.Items.Skip(index == -1 ? currentCount : index).Take(tempList.Count));
					}
				}
			}
			catch (Exception ex)
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
			catch (Exception ex)
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
			foreach (ModuleListItem listItem in moduleListControl.Items)
				yield return ListItemConverter.Convert(listItem);
		}



		/********************************************************************/
		/// <summary>
		/// Open the search popup
		/// </summary>
		/********************************************************************/
		private void OpenSearchPopup(string initialText)
		{
			if (searchPopupForm == null || searchPopupForm.IsDisposed)
			{
				searchPopupForm = new SearchPopupForm();
				searchPopupForm.ItemSelected += SearchPopup_ItemSelected;
				searchPopupForm.SearchTextChanged += SearchPopup_SearchTextChanged;
			}

			// Apply list number setting
			searchPopupForm.EnableListNumber(optionSettings.ShowListNumber);

			// Set initial search text
			searchPopupForm.SetInitialText(initialText);

			// Position and size the popup to match the module list
			System.Drawing.Point location = moduleListControl.PointToScreen(new System.Drawing.Point(0, 0));
			System.Drawing.Size size = moduleListControl.Size;
			searchPopupForm.ShowAt(location, size);

			// Perform initial filter
			FilterModules();
		}



		/********************************************************************/
		/// <summary>
		/// Filter modules based on search text
		/// </summary>
		/********************************************************************/
		private void FilterModules()
		{
			if (searchPopupForm == null || searchPopupForm.IsDisposed)
				return;

			string searchText = searchPopupForm.SearchText;
			if (string.IsNullOrEmpty(searchText))
			{
				searchPopupForm.UpdateResults(new ModuleListItem[0]);
				return;
			}

			// Check if user entered wildcards
			bool hasWildcards = searchText.Contains('*') || searchText.Contains('?');
			string pattern;

			if (hasWildcards)
			{
				// Convert wildcard pattern to regex
				pattern = "^" + System.Text.RegularExpressions.Regex.Escape(searchText).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			}
			else
			{
				// Auto-add wildcards: *text*
				pattern = System.Text.RegularExpressions.Regex.Escape(searchText);
			}

			// Filter module list
			List<ModuleListItem> filteredModules = new List<ModuleListItem>();
			System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			foreach (ModuleListItem item in moduleListControl.Items)
			{
				if (hasWildcards)
				{
					if (regex.IsMatch(item.ListItem.DisplayName))
						filteredModules.Add(item);
				}
				else
				{
					if (item.ListItem.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
						filteredModules.Add(item);
				}
			}

			searchPopupForm.UpdateResults(filteredModules.ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Is called when user selects an item in search popup
		/// </summary>
		/********************************************************************/
		private void SearchPopup_ItemSelected(object sender, ModuleListItem selectedItem)
		{
			if (selectedItem == null)
				return;

			// Find the selected module in the main list
			int index = moduleListControl.Items.IndexOf(selectedItem);
			if (index >= 0)
			{
				// Found the module, play it
				StopAndFreeModule();
				LoadAndPlayModule(index);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchPopup_SearchTextChanged(object sender, EventArgs e)
		{
			FilterModules();
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
				timeOccurred = new TimeSpan(0);

				if ((moduleHandler.PlayingModuleInformation.DurationInfo == null) && moduleSettings.NeverEnding)
				{
					neverEndingTimeout = new TimeSpan(moduleSettings.NeverEndingTimeout * TimeSpan.TicksPerSecond);
					neverEndingStarted = true;
				}
				else
					neverEndingStarted = false;
			}

			// Start the never ending timer if needed
			if (neverEndingStarted)
			{
				neverEndingTimer.Interval = (int)(neverEndingTimeout - timeOccurred).TotalMilliseconds;
				neverEndingTimer.Start();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will stop all the timers
		/// </summary>
		/********************************************************************/
		private void StopTimers()
		{
			neverEndingTimer.Stop();
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

			string[] listExtensions = ListFactory.GetExtensions();
			string[] archiveExtensions = new ArchiveDetector(agentManager).GetExtensions();

			foreach (string file in files)
			{
				if (Directory.Exists(file))
				{
					// It's a directory, so add all files inside it
					AddDirectoryToList(file, itemList, listExtensions, archiveExtensions, checkForList);
				}
				else
				{
					// It's a file
					AddSingleFileToList(file, itemList, listExtensions, archiveExtensions, checkForList);
				}
			}

			int currentCount = moduleListControl.Items.Count;

			// Add the items to the list
			moduleListControl.BeginUpdate();

			try
			{
				if (startIndex == -1)
					moduleListControl.Items.AddRange(itemList);
				else
				{
					for (int i = itemList.Count - 1; i >= 0; i--)
						moduleListControl.Items.Insert(startIndex, itemList[i]);
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}

			// Update the controls
			UpdateControls();

			// Tell the file scanner to scan the new items
			fileScanner.ScanItems(moduleListControl.Items.Skip(startIndex == -1 ? currentCount : startIndex).Take(itemList.Count));
		}



		/********************************************************************/
		/// <summary>
		/// Will add the given module list items to the module list
		/// </summary>
		/********************************************************************/
		private void AddItemsToList(ModuleListItem[] items, bool clearAndPlay)
		{
			moduleListControl.BeginUpdate();

			try
			{
				if (clearAndPlay)
				{
					// Free any playing module
					StopAndFreeModule();

					EmptyList();
				}

				moduleListControl.Items.AddRange(items);

				// Add time from each item
				listTime += new TimeSpan(items.Where(x => x.HaveTime).Select(x => x.Duration).Sum(x => x.Ticks));

				if (clearAndPlay)
				{
					// Load the module
					LoadAndPlayModule(0);
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}

			// Update the controls
			UpdateControls();
		}



		/********************************************************************/
		/// <summary>
		/// Will remove all the items from the module list
		/// </summary>
		/********************************************************************/
		private void EmptyList()
		{
			// Clear the module list
			moduleListControl.Items.Clear();

			// Tell scanner to stop
			fileScanner.ClearQueue();

			// Clear the time variables
			listTime = TimeSpan.Zero;
			selectedTime = TimeSpan.Zero;

			// And update the window
			UpdateControls();

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
			moduleListControl.BeginUpdate();

			try
			{
				// Safe check
				if (moduleListControl.SelectedIndexes.Count == 0)
					return;

				// Remember which item to select, after removing is done
				int indexToSelect = moduleListControl.SelectedIndexes[moduleListControl.SelectedIndexes.Count - 1] - moduleListControl.SelectedIndexes.Count + 1;

				// Remove all the selected module items
				foreach (int index in moduleListControl.SelectedIndexes.Reverse())  // Take the items in reverse order, which is done via a copy of the selected items
				{
					ModuleListItem listItem = moduleListControl.Items[index];

					// If the item is the one that is playing, stop it
					if (listItem.IsPlaying)
					{
						playItem = null;

						StopAndFreeModule();
						moduleHandler.FreeAllModules();
					}

					// Subtract the item time from the list
					RemoveItemTimeFromList(listItem);

					moduleListControl.Items.Remove(listItem);
				}

				if (moduleListControl.Items.Count > 0)
				{
					if (indexToSelect >= moduleListControl.Items.Count)
						indexToSelect = moduleListControl.Items.Count - 1;

					moduleListControl.SelectedIndex = indexToSelect;
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items one index up
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsUp()
		{
			moduleListControl.BeginUpdate();

			try
			{
				int previousSelected = -1;
				bool previousMoved = false;

				foreach (int selected in moduleListControl.SelectedIndexes)
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
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items one index up
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsDown()
		{
			moduleListControl.BeginUpdate();

			try
			{
				int previousSelected = -1;
				bool previousMoved = false;
				int listCount = moduleListControl.Items.Count;

				foreach (int selected in moduleListControl.SelectedIndexes.Reverse())   // Take the items in reverse order, which is done via a copy of the selected items
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
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items to the top of the list
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsToTop()
		{
			moduleListControl.BeginUpdate();

			try
			{
				// Move all the items
				int index = 0;

				foreach (int selected in moduleListControl.SelectedIndexes)
				{
					MoveItem(selected, index);
					index++;
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move all the selected items to the bottom of the list
		/// </summary>
		/********************************************************************/
		private void MoveSelectedItemsToBottom()
		{
			moduleListControl.BeginUpdate();

			try
			{
				// Move all the items
				int listCount = moduleListControl.Items.Count;
				int index = 0;

				foreach (int selected in moduleListControl.SelectedIndexes.Reverse())   // Take the items in reverse order, which is done via a copy of the selected items
				{
					MoveItem(selected, listCount - index - 1);
					index++;
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will shuffle the entire list
		/// </summary>
		/********************************************************************/
		private void ShuffleList()
		{
			moduleListControl.BeginUpdate();

			try
			{
				// Get the number of items
				int total = moduleListControl.Items.Count;

				// Do we have enough items in the list?
				if (total > 1)
				{
					// Create a new resulting list
					List<ModuleListItem> newList = new List<ModuleListItem>();

					// Make a copy of all the items in the list
					List<ModuleListItem> tempList = new List<ModuleListItem>(moduleListControl.Items);

					// Well, if a module is playing, we want to
					// place that module in the top of the list
					if (playItem != null)
					{
						// Find the item index
						int index = moduleListControl.Items.IndexOf(playItem);

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
						int index = RandomGenerator.GetRandomNumber(total);

						// Move the item to the new list
						newList.Add(tempList[index]);
						tempList.RemoveAt(index);
					}

					// Copy the new list into the list control
					moduleListControl.Items.Clear();
					moduleListControl.Items.AddRange(newList);
				}
			}
			finally
			{
				moduleListControl.EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will move a item in the list
		/// </summary>
		/********************************************************************/
		private void MoveItem(int from, int to)
		{
			ModuleListItem listItem = moduleListControl.Items[from];
			moduleListControl.Items.RemoveAt(from);
			moduleListControl.Items.Insert(to, listItem);

			// Keep selection
			moduleListControl.SetSelected(to, true);
		}



		/********************************************************************/
		/// <summary>
		/// Will save the whole module list as a remember list
		/// </summary>
		/********************************************************************/
		private void RememberModuleList(int selected, int position, int song)
		{
			try
			{
				string fileName = Path.Combine(Settings.SettingsDirectory, "___RememberList.npml");
				RememberListSettings infoSettings = new RememberListSettings();

				// Delete the file if it exists
				if (File.Exists(fileName))
					File.Delete(fileName);

				infoSettings.DeleteSettings();

				// Check if we should write the module list
				if (optionSettings.RememberList && (moduleListControl.Items.Count > 0))
				{
					// Save the module list
					SaveModuleList(fileName);

					if (optionSettings.RememberListPosition)
					{
						infoSettings.ListPosition = selected;

						if (optionSettings.RememberModulePosition)
						{
							infoSettings.ModulePosition = position;
							infoSettings.SubSong = song;
						}

						infoSettings.SaveSettings();
					}
				}
			}
			catch (Exception)
			{
				// Ignore any kind of exception
			}
		}
		#endregion

		#endregion

		#region Equalizer
		private EqualizerWindowForm equalizerWindow = null;

		/********************************************************************/
		/// <summary>
		/// User selected the Equalizer menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Window_Equalizer_Click(object sender, EventArgs e)
		{
			if (IsEqualizerWindowOpen())
			{
				if (equalizerWindow.WindowState == FormWindowState.Minimized)
					equalizerWindow.WindowState = FormWindowState.Normal;

				equalizerWindow.Activate();
			}
			else
			{
				equalizerWindow = new EqualizerWindowForm(moduleHandler, this, optionSettings, soundSettings);
				equalizerWindow.Disposed += (o, args) => { equalizerWindow = null; };
				equalizerWindow.Show();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Event handler for the equalizer button click
		/// </summary>
		/********************************************************************/
		private void EqualizerButton_Click(object sender, EventArgs e)
		{
			// Call the same method as the menu item
			Menu_Window_Equalizer_Click(sender, e);
		}

		/********************************************************************/
		/// <summary>
		/// Open the Equalizer window if configured to
		/// </summary>
		/********************************************************************/
		private void OpenEqualizerWindow()
		{
			if (mainWindowSettings.OpenEqualizerWindow)
			{
				equalizerWindow = new EqualizerWindowForm(moduleHandler, this, optionSettings, soundSettings);
				equalizerWindow.Show();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Close the Equalizer window
		/// </summary>
		/********************************************************************/
		private void CloseEqualizerWindow()
		{
			bool openAgain = IsEqualizerWindowOpen();
			if (openAgain)
				equalizerWindow.Close();

			equalizerWindow = null;
			mainWindowSettings.OpenEqualizerWindow = openAgain;
		}

		/********************************************************************/
		/// <summary>
		/// Enumerate Equalizer window if open
		/// </summary>
		/********************************************************************/
		private IEnumerable<Form> EnumerateEqualizerWindow()
		{
			if (IsEqualizerWindowOpen())
				yield return equalizerWindow;
		}

		/********************************************************************/
		/// <summary>
		/// Check to see if Equalizer window is open
		/// </summary>
		/********************************************************************/
		private bool IsEqualizerWindowOpen()
		{
			return (equalizerWindow != null) && !equalizerWindow.IsDisposed;
		}
		#endregion
	}
}

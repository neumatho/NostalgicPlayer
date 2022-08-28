/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Components;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow
{
	/// <summary>
	/// This shows the module information
	/// </summary>
	public partial class ModuleInfoWindowForm : WindowFormBase
	{
		private const int FirstCustomLine = 9;

		private ModuleHandler moduleHandler;
		private MainWindowForm mainWindow;

		private bool doNotUpdateAutoSelection;

		private readonly ModuleInfoWindowSettings settings;

		private const int Page_ModuleInfo = 0;
		private const int Page_Comments = 1;
		private const int Page_Lyrics = 2;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoWindowForm(ModuleHandler moduleHandler, MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.moduleHandler = moduleHandler;
			this.mainWindow = mainWindow;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("ModuleInfoWindow");
				settings = new ModuleInfoWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_MODULE_INFO_TITLE;

				// Set the tab titles
				navigator.Pages[Page_ModuleInfo].Text = Resources.IDS_MODULE_INFO_TAB_INFO;
				navigator.Pages[Page_Comments].Text = Resources.IDS_MODULE_INFO_TAB_COMMENT;
				navigator.Pages[Page_Lyrics].Text = Resources.IDS_MODULE_INFO_TAB_LYRICS;

				// Add the columns to the controls
				moduleInfoInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_MODULE_INFO_COLUMN_DESCRIPTION,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						Width = settings.Column1Width
					});

				moduleInfoInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_MODULE_INFO_COLUMN_VALUE,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						Width = settings.Column2Width
					});

				// Make sure that the content is up-to date
				RefreshWindow();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the window and add all the items again
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			if (moduleHandler != null)
			{
				// Remove all the items
				moduleInfoInfoDataGridView.Rows.Clear();
				moduleInfoCommentReadOnlyRichTextBox.Clear();
				moduleInfoLyricsReadOnlyRichTextBox.Clear();

				// Add the items
				doNotUpdateAutoSelection = true;

				AddItems();

				doNotUpdateAutoSelection = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will be called every time a new value has changed
		/// </summary>
		/********************************************************************/
		public void UpdateWindow(int line, string newValue)
		{
			if (moduleHandler != null)
			{
				// Check to see if there are any module loaded at the moment
				if (moduleHandler.IsModuleLoaded)
				{
					if ((FirstCustomLine + line) < moduleInfoInfoDataGridView.RowCount)
					{
						moduleInfoInfoDataGridView.Rows[FirstCustomLine + line].Cells[1].Value = newValue;
						moduleInfoInfoDataGridView.InvalidateRow(FirstCustomLine + line);
					}
				}
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void ModuleInfoWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindow != null)		// Main window is null, if the window has already been closed (because Owner has been set)
			{
				// Save the settings
				settings.Column1Width = moduleInfoInfoDataGridView.Columns[0].Width;
				settings.Column2Width = moduleInfoInfoDataGridView.Columns[1].Width;

				// Cleanup
				mainWindow = null;
				moduleHandler = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Navigator_SelectedPageChanged(object sender, EventArgs e)
		{
			if (!doNotUpdateAutoSelection)
				settings.AutoSelectTab = navigator.SelectedIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks in a cell in the data grid
		/// </summary>
		/********************************************************************/
		private void ModuleInfoInfoDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!Env.IsWindows10S)
			{
				// Check if the file name has been clicked
				if ((e.RowIndex == 7) && (e.ColumnIndex == 1))
				{
					string fileName = moduleInfoInfoDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
					if (ArchivePath.IsArchivePath(fileName))
						fileName = ArchivePath.GetArchiveName(fileName);

					// Start File Explorer and select the file
					Process.Start("explorer.exe", $"/select,\"{fileName}\"");
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will add all the items in the list
		/// </summary>
		/********************************************************************/
		private void AddItems()
		{
			// Check to see if there are any module loaded at the moment
			MultiFileInfo fileInfo;

			if (moduleHandler.IsPlaying && ((fileInfo = mainWindow.GetFileInfo()) != null))
			{
				// Module in memory, add items
				ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;
				ModuleInfoFloating floatingInfo = moduleHandler.PlayingModuleInformation;

				string val = staticInfo.ModuleName;
				if (string.IsNullOrEmpty(val))
					val = Path.GetFileName(ArchivePath.IsArchivePath(fileInfo.FileName) ? ArchivePath.GetEntryName(fileInfo.FileName) : fileInfo.FileName);

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULENAME, val);

				val = staticInfo.Author;
				if (string.IsNullOrEmpty(val))
					val = Resources.IDS_MODULE_INFO_UNKNOWN;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, val);

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, staticInfo.ModuleFormat);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, staticInfo.PlayerName);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, staticInfo.Channels);

				if (floatingInfo.DurationInfo == null)
					val = Resources.IDS_MODULE_INFO_UNKNOWN;
				else
				{
					TimeSpan time = new TimeSpan((((long)floatingInfo.DurationInfo.TotalTime.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
					if ((int)time.TotalHours > 0)
						val = time.ToString(Resources.IDS_TIMEFORMAT);
					else
						val = time.ToString(Resources.IDS_TIMEFORMAT_SMALL);
				}

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, val);

				val = $"{staticInfo.ModuleSize:n0}";
				if (staticInfo.CrunchedSize != 0)
					val += string.Format(Resources.IDS_MODULE_INFO_ITEM_PACKEDSIZE, staticInfo.CrunchedSize == -1 ? Resources.IDS_MODULE_INFO_UNKNOWN.ToLower() : staticInfo.CrunchedSize.ToString("N0"));

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, val);

				if (Env.IsWindows10S)
					moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.FileName);
				else
				{
					int row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.FileName);
					moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewLinkCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, TrackVisitedState = false };
				}

				// Add player specific items
				if (floatingInfo.ModuleInformation != null)
				{
					// Add an empty line
					DataGridViewRow emptyRow = new DataGridViewRow();
					emptyRow.Cells.AddRange(new DataGridViewCell[]
					{
						new KryptonDataGridViewTextBoxCell { Value = string.Empty, ToolTipText = string.Empty },
						new KryptonDataGridViewTextBoxCell { Value = string.Empty, ToolTipText = string.Empty }
					});

					moduleInfoInfoDataGridView.Rows.Add(emptyRow);

					foreach (string info in floatingInfo.ModuleInformation)
					{
						string[] splittedInfo = info.Split('\t');
						moduleInfoInfoDataGridView.Rows.Add(splittedInfo[0], splittedInfo[1]);
					}
				}

				// Add comment
				if (staticInfo.Comment.Length > 0)
				{
					navigator.Pages[Page_Comments].Visible = true;

					if (settings.AutoSelectTab == Page_Comments)
						navigator.SelectedIndex = Page_Comments;

					// Switch font
					moduleInfoCommentReadOnlyRichTextBox.SetFont(staticInfo.CommentFont ?? FontPalette.GetMonospaceFont());

					// Set text
					moduleInfoCommentReadOnlyRichTextBox.Lines = staticInfo.Comment;
				}
				else
					navigator.Pages[Page_Comments].Visible = false;

				// Add lyrics
				if (staticInfo.Lyrics.Length > 0)
				{
					navigator.Pages[Page_Lyrics].Visible = true;

					if (settings.AutoSelectTab == Page_Lyrics)
						navigator.SelectedIndex = Page_Lyrics;

					// Switch font
					moduleInfoLyricsReadOnlyRichTextBox.SetFont(staticInfo.LyricsFont ?? FontPalette.GetMonospaceFont());

					// Set text
					moduleInfoLyricsReadOnlyRichTextBox.Lines = staticInfo.Lyrics;
				}
				else
					navigator.Pages[Page_Lyrics].Visible = false;
			}
			else
			{
				// No module in memory
				string na = Resources.IDS_MODULE_INFO_ITEM_NA;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULENAME, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, na);

				navigator.Pages[Page_Comments].Visible = false;
				navigator.Pages[Page_Lyrics].Visible = false;
			}

			// Resize the rows, so the lines are compacted
			moduleInfoInfoDataGridView.AutoResizeRows();
		}
		#endregion
	}
}

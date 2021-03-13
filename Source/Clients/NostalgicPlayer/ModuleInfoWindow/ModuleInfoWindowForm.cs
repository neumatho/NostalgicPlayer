/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
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

		private readonly ModuleInfoWindowSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoWindowForm(ModuleHandler moduleHandler, MainWindowForm mainWindow)
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			moduleInfoDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = PaletteDrawBorders.BottomRight;
			moduleInfoDataGridView.StateCommon.DataCell.Border.DrawBorders = PaletteDrawBorders.None;

			// Remember the arguments
			this.moduleHandler = moduleHandler;
			this.mainWindow = mainWindow;

			if (!DesignMode)
			{
				// Load window settings
				LoadWindowSettings("ModuleInfoWindow");
				settings = new ModuleInfoWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_MODULE_INFO_TITLE;

				// Add the columns to the control
				moduleInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_MODULE_INFO_COLUMN_DESCRIPTION,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						Width = settings.Column1Width
					});

				moduleInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
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
			// Remove all the items
			moduleInfoDataGridView.Rows.Clear();

			// Add the items
			AddItems();
		}



		/********************************************************************/
		/// <summary>
		/// Will be called every time a new value has changed
		/// </summary>
		/********************************************************************/
		public void UpdateWindow(int line, string newValue)
		{
			// Check to see if there are any module loaded at the moment
			if (moduleHandler.IsModuleLoaded)
			{
				if ((FirstCustomLine + line) < moduleInfoDataGridView.RowCount)
				{
					moduleInfoDataGridView.Rows[FirstCustomLine + line].Cells[1].Value = newValue;
					moduleInfoDataGridView.InvalidateRow(FirstCustomLine + line);
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
			// Save the settings
			settings.Column1Width = moduleInfoDataGridView.Columns[0].Width;
			settings.Column2Width = moduleInfoDataGridView.Columns[1].Width;

			// Cleanup
			mainWindow = null;
			moduleHandler = null;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks in a cell in the data grid
		/// </summary>
		/********************************************************************/
		private void ModuleInfoDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!Env.IsWindows10S)
			{
				// Check if the file name has been clicked
				if ((e.RowIndex == 7) && (e.ColumnIndex == 1))
				{
					// Start File Explorer and select the file
					Process.Start("explorer.exe", $"/select,\"{moduleInfoDataGridView[e.ColumnIndex, e.RowIndex].Value}\"");
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
			if (moduleHandler.IsModuleLoaded)
			{
				// Module in memory, add items
				ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;
				ModuleInfoFloating floatingInfo = moduleHandler.PlayingModuleInformation;
				MultiFileInfo fileInfo = mainWindow.GetFileInfo();

				string val = staticInfo.ModuleName;
				if (string.IsNullOrEmpty(val))
					val = Path.GetFileName(fileInfo.FileName);

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULENAME, val);

				val = staticInfo.Author;
				if (string.IsNullOrEmpty(val))
					val = Resources.IDS_MODULE_INFO_UNKNOWN;

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, val);

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, staticInfo.ModuleFormat);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, staticInfo.PlayerName);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, staticInfo.Channels);

				TimeSpan time = floatingInfo.TotalTime;
				if (time.TotalMilliseconds == 0)
					val = Resources.IDS_MODULE_INFO_UNKNOWN;
				else
				{
					time = new TimeSpan((((long)time.TotalMilliseconds + 500) / 1000 * 1000) * TimeSpan.TicksPerMillisecond);
					if ((int)time.TotalHours > 0)
						val = time.ToString(Resources.IDS_TIMEFORMAT);
					else
						val = time.ToString(Resources.IDS_TIMEFORMAT_SMALL);
				}

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, val);

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, $"{staticInfo.ModuleSize:n0}");

				if (Env.IsWindows10S)
					moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.FileName);
				else
				{
					int row = moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.FileName);
					moduleInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewLinkCell { Value = moduleInfoDataGridView.Rows[row].Cells[1].Value, TrackVisitedState = false };
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

					moduleInfoDataGridView.Rows.Add(emptyRow);

					foreach (string info in floatingInfo.ModuleInformation)
					{
						string[] splittedInfo = info.Split('\t');
						moduleInfoDataGridView.Rows.Add(splittedInfo[0], splittedInfo[1]);
					}
				}
			}
			else
			{
				// No module in memory
				string na = Resources.IDS_MODULE_INFO_ITEM_NA;

				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULENAME, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, na);
				moduleInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, na);
			}

			// Resize the rows, so the lines are compacted
			moduleInfoDataGridView.AutoResizeRows();
		}
		#endregion
	}
}

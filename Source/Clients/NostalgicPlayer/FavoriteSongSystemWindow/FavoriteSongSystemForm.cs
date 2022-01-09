/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Controls;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.FavoriteSongSystemWindow
{
	/// <summary>
	/// This shows the favorite song system
	/// </summary>
	public partial class FavoriteSongSystemForm : WindowFormBase
	{
		private MainWindowForm mainWindow;
		private ModuleDatabase database;

		private readonly FavoriteSongSystemWindowSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FavoriteSongSystemForm(MainWindowForm mainWindow, ModuleDatabase database, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.mainWindow = mainWindow;
			this.database = database;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("FavoriteSongSystemWindow");
				settings = new FavoriteSongSystemWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_FAVORITE_TITLE;

				// Add the columns to the grid
				favoriteDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
				{
					Name = "#",
					Resizable = DataGridViewTriState.True,
					SortMode = DataGridViewColumnSortMode.NotSortable,
					DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
					Width = settings.Column1Width,
					DisplayIndex = settings.Column1Pos
				});

				favoriteDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
				{
					Name = Resources.IDS_FAVORITE_COLUMN_NAME,
					Resizable = DataGridViewTriState.True,
					SortMode = DataGridViewColumnSortMode.NotSortable,
					Width = settings.Column2Width,
					DisplayIndex = settings.Column2Pos
				});

				favoriteDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
				{
					Name = Resources.IDS_FAVORITE_COLUMN_COUNT,
					Resizable = DataGridViewTriState.True,
					SortMode = DataGridViewColumnSortMode.NotSortable,
					DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
					Width = settings.Column3Width,
					DisplayIndex = settings.Column3Pos
				});

				// Add items to the combo controls
				showComboBox.Items.AddRange(new object[]
				{
					Resources.IDS_FAVORITE_SHOW_TOP10,
					Resources.IDS_FAVORITE_SHOW_TOP50,
					Resources.IDS_FAVORITE_SHOW_TOP100,
					Resources.IDS_FAVORITE_SHOW_TOPX,
					Resources.IDS_FAVORITE_SHOW_BOTTOM10,
					Resources.IDS_FAVORITE_SHOW_BOTTOM50,
					Resources.IDS_FAVORITE_SHOW_BOTTOM100,
					Resources.IDS_FAVORITE_SHOW_BOTTOMX
				});

				showComboBox.SelectedIndex = (int)settings.Show;
				otherNumberTextBox.Text = settings.ShowOther.ToString();

				// Set tool tip
				toolTip.SetToolTip(addButton, Resources.IDS_TIP_FAVORITE_ADD);
				toolTip.SetToolTip(removeButton, Resources.IDS_TIP_FAVORITE_REMOVE);
				toolTip.SetToolTip(resetButton, Resources.IDS_TIP_FAVORITE_RESET);

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
			// Remove all the items from the list
			favoriteDataGridView.Rows.Clear();

			// Define sorting method
			var sortMethod1 = (KeyValuePair<string, ModuleDatabaseInfo> p) => p.Value.ListenCount;
			var sortMethod2 = (KeyValuePair<string, ModuleDatabaseInfo> p) => p.Value.LastLoaded;

			// Now add the items
			IEnumerable<KeyValuePair<string, ModuleDatabaseInfo>> list = database.RetrieveAllInformation().Where(p => p.Value.ListenCount > 0);

			switch ((FavoriteSongSystemWindowSettings.WhatToShow)showComboBox.SelectedIndex)
			{
				case FavoriteSongSystemWindowSettings.WhatToShow.Top10:
				{
					list = list.OrderByDescending(sortMethod1).ThenByDescending(sortMethod2).Take(10);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.Top50:
				{
					list = list.OrderByDescending(sortMethod1).ThenByDescending(sortMethod2).Take(50);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.Top100:
				{
					list = list.OrderByDescending(sortMethod1).ThenByDescending(sortMethod2).Take(100);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.TopX:
				{
					list = list.OrderByDescending(sortMethod1).ThenByDescending(sortMethod2).Take(int.Parse(otherNumberTextBox.Text));
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.Bottom10:
				{
					list = list.OrderBy(sortMethod1).ThenBy(sortMethod2).Take(10);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.Bottom50:
				{
					list = list.OrderBy(sortMethod1).ThenBy(sortMethod2).Take(50);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.Bottom100:
				{
					list = list.OrderBy(sortMethod1).ThenBy(sortMethod2).Take(100);
					break;
				}

				case FavoriteSongSystemWindowSettings.WhatToShow.BottomX:
				{
					list = list.OrderBy(sortMethod1).ThenBy(sortMethod2).Take(int.Parse(otherNumberTextBox.Text));
					break;
				}
			}

			int pos = 1;

			foreach (KeyValuePair<string, ModuleDatabaseInfo> pair in list)
			{
				DataGridViewRow row = new DataGridViewRow();

				string fileName = Path.GetFileName(pair.Key);
				if (ArchivePath.IsArchivePath(fileName))
					fileName = ArchivePath.GetEntryName(fileName);

				row.Cells.AddRange(new DataGridViewCell[]
				{
					new KryptonDataGridViewTextBoxCell { Value = pos },
					new KryptonDataGridViewTextBoxCell { Value = fileName },
					new KryptonDataGridViewTextBoxCell { Value = pair.Value.ListenCount }
				});

				row.Tag = pair.Key;

				favoriteDataGridView.Rows.Add(row);

				pos++;
			}

			// Resize the rows, so the lines are compacted
			favoriteDataGridView.AutoResizeRows();
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void FavoriteSongSystemWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindow != null)		// Main window is null, if the window has already been closed (because Owner has been set)
			{
				// Save the settings
				settings.Column1Width = favoriteDataGridView.Columns[0].Width;
				settings.Column1Pos = favoriteDataGridView.Columns[0].DisplayIndex;

				settings.Column2Width = favoriteDataGridView.Columns[1].Width;
				settings.Column2Pos = favoriteDataGridView.Columns[1].DisplayIndex;

				settings.Column3Width = favoriteDataGridView.Columns[2].Width;
				settings.Column3Pos = favoriteDataGridView.Columns[2].DisplayIndex;

				// Cleanup
				database = null;
				mainWindow = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when an item is selected in the list
		/// </summary>
		/********************************************************************/
		private void FavoriteDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			if (favoriteDataGridView.SelectedRows.Count > 0)
			{
				addButton.Enabled = true;
				removeButton.Enabled = true;
			}
			else
			{
				addButton.Enabled = false;
				removeButton.Enabled = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when an item is double clicked in the list
		/// </summary>
		/********************************************************************/
		private void FavoriteDataGridView_DoubleClick(object sender, EventArgs e)
		{
			if (favoriteDataGridView.SelectedRows.Count > 0)
				mainWindow.AddFilesToList(new[] { favoriteDataGridView.SelectedRows[0].Tag as string });
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a save format has changed
		/// </summary>
		/********************************************************************/
		private void ShowComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			FavoriteSongSystemWindowSettings.WhatToShow toShow = (FavoriteSongSystemWindowSettings.WhatToShow)showComboBox.SelectedIndex;

			otherNumberTextBox.Enabled = (toShow == FavoriteSongSystemWindowSettings.WhatToShow.TopX) || (toShow == FavoriteSongSystemWindowSettings.WhatToShow.BottomX);
			settings.Show = toShow;

			RefreshWindow();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the other number has changed
		/// </summary>
		/********************************************************************/
		private void OtherNumberTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(otherNumberTextBox.Text))
			{
				settings.ShowOther = int.Parse(otherNumberTextBox.Text);
				RefreshWindow();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the add button is clicked
		/// </summary>
		/********************************************************************/
		private void AddButton_Click(object sender, EventArgs e)
		{
			DataGridViewSelectedRowCollection selectedRows = favoriteDataGridView.SelectedRows;
			int count = selectedRows.Count;

			if (count > 0)
			{
				using (new SleepCursor())
				{
					string[] files = new string[count];

					for (int i = 0; i < count; i++)
						files[i] = selectedRows[i].Tag as string;

					mainWindow.AddFilesToList(files);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the remove button is clicked
		/// </summary>
		/********************************************************************/
		private void RemoveButton_Click(object sender, EventArgs e)
		{
			DataGridViewSelectedRowCollection selectedRows = favoriteDataGridView.SelectedRows;
			int count = selectedRows.Count;

			if (count > 0)
			{
				using (new SleepCursor())
				{
					foreach (DataGridViewRow row in selectedRows)
					{
						string fullPath = row.Tag as string;

						ModuleDatabaseInfo moduleDatabaseInfo = database.RetrieveInformation(fullPath);
						if (moduleDatabaseInfo != null)
							database.StoreInformation(fullPath, new ModuleDatabaseInfo(moduleDatabaseInfo.Duration, 0, DateTime.MinValue));
					}

					RefreshWindow();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the reset button is clicked
		/// </summary>
		/********************************************************************/
		private void ResetButton_Click(object sender, EventArgs e)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(Resources.IDS_FAVORITE_RESET, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Question))
			{
				dialog.AddButton(Resources.IDS_BUT_YES, 'Y');
				dialog.AddButton(Resources.IDS_BUT_NO, 'N');
				dialog.ShowDialog();

				if (dialog.GetButtonResult() == 'Y')
				{
					using (new SleepCursor())
					{
						database.RunAction((_, moduleDatabaseInfo) =>
						{
							if (moduleDatabaseInfo.ListenCount > 0)
								return new ModuleDatabaseInfo(moduleDatabaseInfo.Duration, 0, DateTime.MinValue);

							return null;
						});

						RefreshWindow();
					}
				}
			}
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Gui.Extensions;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Logic.Playlists;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class InfoPageControl : UserControl
	{
		private IMainWindowApi mainWindowApi;

		private int firstCustomLine;
		private bool showingFileName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InfoPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		/// </summary>
		/********************************************************************/
		public void InitControl(IMainWindowApi mainWindowApi, ModuleInfoWindowSettings settings)
		{
			// Remember the arguments
			this.mainWindowApi = mainWindowApi;

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
		}



		/********************************************************************/
		/// <summary>
		/// Save settings
		/// </summary>
		/********************************************************************/
		public void SaveSettings(ModuleInfoWindowSettings settings)
		{
			settings.Column1Width = moduleInfoInfoDataGridView.Columns[0].Width;
			settings.Column2Width = moduleInfoInfoDataGridView.Columns[1].Width;
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the control with new data
		/// </summary>
		/********************************************************************/
		public void RefreshControl(bool isPlaying, ModuleInfoStatic staticInfo, ModuleInfoFloating floatingInfo)
		{
			moduleInfoInfoDataGridView.Rows.Clear();

			// Check to see if there are any module loaded at the moment
			PlaylistFileInfo fileInfo;

			if (isPlaying && ((fileInfo = mainWindowApi.GetFileInfo()) != null))
			{
				firstCustomLine = 7;

				// Module in memory, add items
				string val = staticInfo.Title;
				if (string.IsNullOrEmpty(val))
					val = fileInfo.DisplayName;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TITLE, val);

				val = staticInfo.Author;
				if (string.IsNullOrEmpty(val))
					val = Resources.IDS_MODULE_INFO_UNKNOWN;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, val);

				int row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, staticInfo.Format);
				moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewTextBoxCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, ToolTipText = string.Join("\r\n", staticInfo.FormatDescription.SplitIntoLines(moduleInfoInfoDataGridView.Handle, 400, moduleInfoInfoDataGridView.Font)) };

				row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, staticInfo.PlayerName);
				moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewTextBoxCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, ToolTipText = string.Join("\r\n", staticInfo.PlayerDescription.SplitIntoLines(moduleInfoInfoDataGridView.Handle, 400, moduleInfoInfoDataGridView.Font)) };

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, staticInfo.Channels);

				if (floatingInfo.DurationInfo == null)
					val = Resources.IDS_MODULE_INFO_UNKNOWN;
				else
					val = floatingInfo.DurationInfo.TotalTime.ToFormattedString();

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, val);

				showingFileName = false;

				if (fileInfo.Type != PlaylistFileInfo.FileType.Audius)
				{
					if (fileInfo.Type == PlaylistFileInfo.FileType.Url)
					{
						moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_URL, fileInfo.Source);
						firstCustomLine++;
					}
					else
					{
						val = string.Format(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE_VALUE, staticInfo.ModuleSize.ToBeautifiedString());
						moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, val);
						firstCustomLine++;

						if (staticInfo.DecruncherAlgorithms != null)
						{
							val = staticInfo.CrunchedSize == -1 ? Resources.IDS_MODULE_INFO_UNKNOWN : string.Format(Resources.IDS_MODULE_INFO_ITEM_PACKEDSIZE_VALUE, staticInfo.CrunchedSize);
							val += string.Format(" / {0}", string.Join(" \u2b95 ", staticInfo.DecruncherAlgorithms));

							moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_PACKEDSIZE, val);
							firstCustomLine++;
						}

						if (Env.IsWindows10S)
							moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.Source);
						else
						{
							row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.Source);
							moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewLinkCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, TrackVisitedState = false };
						}

						firstCustomLine++;

						showingFileName = true;
					}
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
			}
			else
			{
				// No module in memory
				string na = Resources.IDS_MODULE_INFO_ITEM_NA;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TITLE, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, na);
			}

			// Resize the rows, so the lines are compacted
			moduleInfoInfoDataGridView.AutoResizeRows();
		}



		/********************************************************************/
		/// <summary>
		/// Will be called every time a new value has changed
		/// </summary>
		/********************************************************************/
		public void UpdateControl(int line, string newValue)
		{
			if (line < 0)
			{
				if (line == ModuleInfoChanged.ModuleNameChanged)
				{
					moduleInfoInfoDataGridView.Rows[0].Cells[1].Value = newValue;
					moduleInfoInfoDataGridView.InvalidateRow(0);
				}
				else if (line == ModuleInfoChanged.AuthorChanged)
				{
					moduleInfoInfoDataGridView.Rows[1].Cells[1].Value = newValue;
					moduleInfoInfoDataGridView.InvalidateRow(1);
				}
			}
			else if ((firstCustomLine + line) < moduleInfoInfoDataGridView.RowCount)
			{
				moduleInfoInfoDataGridView.Rows[firstCustomLine + line].Cells[1].Value = newValue;
				moduleInfoInfoDataGridView.InvalidateRow(firstCustomLine + line);
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks in a cell in the data grid
		/// </summary>
		/********************************************************************/
		private void ModuleInfoInfoDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!Env.IsWindows10S && showingFileName)
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
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Images;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Holds the list control used in the different pages
	/// </summary>
	public partial class ModLibraryListControl : UserControl, IDependencyInjectionControl
	{
		private INostalgicImageBank imageBank;

		private bool flatViewEnabled;
		private FlatViewSortOrder sortOrder;

		private DataGridViewColumn imagesColumn;
		private DataGridViewColumn nameColumn;
		private DataGridViewColumn pathColumn;
		private DataGridViewColumn sizeColumn;

		private string currentPath;
		private List<TreeNode> items = [];
		private string searchBuffer = string.Empty;

		private Bitmap combinedFileFavoriteBitmap = null;
		private Bitmap combinedDirectoryFavoriteBitmap = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryListControl()
		{
			InitializeComponent();

			Disposed += ListControl_Disposed;

			// Move the status label into the list, so its transparent
			// background shows the list background instead of ours
			moduleListView.AddOverlayControl(statusLabel);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(INostalgicImageBank imageBank)
		{
			this.imageBank = imageBank;
		}

		#region Public properties
		/********************************************************************/
		/// <summary>
		/// Hold the flat view sort order
		/// </summary>
		/********************************************************************/
		public FlatViewSortOrder SortOrder => sortOrder;
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(ModLibraryWindowSettings settings, bool isOnline)
		{
			flatViewEnabled = isOnline ? settings.OnlineFlatViewEnabled : settings.OfflineFlatViewEnabled;
			sortOrder = isOnline ? settings.OnlineFlatViewSortOrder : settings.OfflineFlatViewSortOrder;

			// Add the columns to the grid
			imagesColumn = new DataGridViewImageColumn
			{
				Name = string.Empty,
				Resizable = DataGridViewTriState.False,
				SortMode = DataGridViewColumnSortMode.NotSortable,
				DefaultCellStyle = new DataGridViewCellStyle { NullValue = null, Alignment = DataGridViewContentAlignment.MiddleLeft },
				Width = 30
			};
			moduleListView.Columns.Add(imagesColumn);

			nameColumn = new DataGridViewTextBoxColumn
			{
				Name = Resources.IDS_MODLIBRARY_COLUMN_NAME,
				Resizable = DataGridViewTriState.True,
				SortMode = DataGridViewColumnSortMode.Programmatic,
				Width = isOnline ? settings.OnlineColumnWidthName : settings.OfflineColumnWidthName
			};
			moduleListView.Columns.Add(nameColumn);

			pathColumn = new DataGridViewTextBoxColumn
			{
				Name = Resources.IDS_MODLIBRARY_COLUMN_PATH,
				Resizable = DataGridViewTriState.True,
				SortMode = DataGridViewColumnSortMode.Programmatic,
				Width = isOnline ? settings.OnlineColumnWidthPath : settings.OfflineColumnWidthPath,
				Visible = flatViewEnabled
			};
			moduleListView.Columns.Add(pathColumn);

			sizeColumn = new DataGridViewTextBoxColumn
			{
				Name = Resources.IDS_MODLIBRARY_COLUMN_SIZE,
				Resizable = DataGridViewTriState.True,
				SortMode = DataGridViewColumnSortMode.NotSortable,
				DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
				Width = isOnline ? settings.OnlineColumnWidthSize : settings.OfflineColumnWidthSize
			};
			moduleListView.Columns.Add(sizeColumn);

			UpdateSortGlyphs();
		}



		/********************************************************************/
		/// <summary>
		/// Store settings
		/// </summary>
		/********************************************************************/
		public void StoreSettings(ModLibraryWindowSettings settings, bool isOnline)
		{
			if (isOnline)
			{
				settings.OnlineFlatViewSortOrder = sortOrder;
				settings.OnlineColumnWidthName = nameColumn.Width;
				settings.OnlineColumnWidthPath = pathColumn.Width;
				settings.OnlineColumnWidthSize = sizeColumn.Width;
			}
			else
			{
				settings.OfflineFlatViewSortOrder = sortOrder;
				settings.OfflineColumnWidthName = nameColumn.Width;
				settings.OfflineColumnWidthPath = pathColumn.Width;
				settings.OfflineColumnWidthSize = sizeColumn.Width;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the items to show in the list
		/// </summary>
		/********************************************************************/
		public void SetItems(string currentPath, List<TreeNode> entries)
		{
			this.currentPath = currentPath;

			// Reset the row count first, so no values are retrieved with
			// invalid indexes while the list is changed
			moduleListView.RowCount = 0;

			items = entries;
			moduleListView.RowCount = items.Count;

			ResetSearch();
		}



		/********************************************************************/
		/// <summary>
		/// Return the items in the list
		/// </summary>
		/********************************************************************/
		public List<TreeNode> GetItems()
		{
			return items;
		}



		/********************************************************************/
		/// <summary>
		/// Return the selected items in the same order as shown in the
		/// list
		/// </summary>
		/********************************************************************/
		public List<TreeNode> GetSelectedItems()
		{
			return moduleListView.SelectedRows
				.Cast<DataGridViewRow>()
				.Select(row => row.Index)
				.Where(index => index < items.Count)
				.Order()
				.Select(index => items[index])
				.ToList();
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the list
		/// </summary>
		/********************************************************************/
		public void ClearItems()
		{
			moduleListView.RowCount = 0;
			items = [];

			ResetSearch();
		}



		/********************************************************************/
		/// <summary>
		/// Select the item at the given index and make sure it is visible
		/// </summary>
		/********************************************************************/
		public void SelectItem(int index, bool setFocus = true)
		{
			// Check against the row count too, since the rows are hidden
			// while loading
			if ((index < 0) || (index >= items.Count) || (index >= moduleListView.RowCount))
				return;

			if (!moduleListView.Rows[index].Displayed)
				moduleListView.FirstDisplayedScrollingRowIndex = index;

			// Move the current cell too, so keyboard navigation continues
			// from the selected item and not from the previous current row
			moduleListView.ClearSelection();
			moduleListView.CurrentCell = moduleListView[imagesColumn.Index, index];
			moduleListView.Rows[index].Selected = true;

			if (setFocus)
				moduleListView.Focus();
		}



		/********************************************************************/
		/// <summary>
		/// Set the flat view state
		/// </summary>
		/********************************************************************/
		public void SetFlatView(bool enabled)
		{
			flatViewEnabled = enabled;
			pathColumn.Visible = enabled;

			UpdateSortGlyphs();
		}



		/********************************************************************/
		/// <summary>
		/// Set load state
		/// </summary>
		/********************************************************************/
		public void SetLoading(bool loading, string loadingText = null)
		{
			statusLabel.Visible = loading;

			if (loading)
			{
				// Hide the rows while loading, so the status text is not
				// drawn on top of them. The items are kept, so they can be
				// shown again if the loading is aborted
				moduleListView.RowCount = 0;
				ResetSearch();

				statusLabel.Text = loadingText ?? string.Empty;
				CenterStatusLabel();
			}
			else if (moduleListView.RowCount != items.Count)
				moduleListView.RowCount = items.Count;
		}
		#endregion

		#region Public events
		/********************************************************************/
		/// <summary>
		/// Fires when an item in the list is activated, either by
		/// double-clicking it or pressing Enter
		/// </summary>
		/********************************************************************/
		[Browsable(true)]
		[Category("Action")]
		[Description("Fires when an item in the list is activated, either by double-clicking it or pressing Enter.")]
		public event ItemActivateEventHandler ItemActivate;



		/********************************************************************/
		/// <summary>
		/// Fires when the user wants to go to the parent directory
		/// </summary>
		/********************************************************************/
		[Browsable(true)]
		[Category("Action")]
		[Description("Fires when the user wants to go to the parent directory.")]
		public event EventHandler NavigateUp;



		/********************************************************************/
		/// <summary>
		/// Fires when the user changes the flat view sort order by clicking
		/// on a column header. The owner should then reload the items
		/// </summary>
		/********************************************************************/
		[Browsable(true)]
		[Category("Action")]
		[Description("Fires when the user changes the flat view sort order by clicking on a column header.")]
		public event EventHandler SortOrderChanged;



		/********************************************************************/
		/// <summary>
		/// Fires when the user right-clicks on an item in the list
		/// </summary>
		/********************************************************************/
		[Browsable(true)]
		[Category("Action")]
		[Description("Fires when the user right-clicks on an item in the list.")]
		public event OpenContextMenuEventHandler OpenContextMenu;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ListControl_Disposed(object sender, EventArgs e)
		{
			combinedFileFavoriteBitmap?.Dispose();
			combinedDirectoryFavoriteBitmap?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the list needs the value for a cell
		/// </summary>
		/********************************************************************/
		private void ModuleListView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if ((e.RowIndex < 0) || (e.RowIndex >= items.Count))
				return;

			TreeNode entry = items[e.RowIndex];

			if (e.ColumnIndex == imagesColumn.Index)
				e.Value = GetRowImage(entry);
			else if (e.ColumnIndex == nameColumn.Index)
				e.Value = entry.Name;
			else if (e.ColumnIndex == pathColumn.Index)
				e.Value = GetPathText(entry);
			else if (e.ColumnIndex == sizeColumn.Index)
			{
				if (entry.IsDirectory)
				{
					int fileCount = CountFilesInTree(entry);
					e.Value = fileCount > 0 ? string.Format(Resources.IDS_MODLIBRARY_FILE_COUNT, fileCount) : string.Empty;
				}
				else
					e.Value = FormatFileSize(entry.Size);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the list needs the tool tip for a cell
		/// </summary>
		/********************************************************************/
		private void ModuleListView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
		{
			if ((e.RowIndex < 0) || (e.RowIndex >= items.Count))
				return;

			// Tell what the icon means, like the sample list in the
			// sample information window does
			if (e.ColumnIndex == imagesColumn.Index)
				e.ToolTipText = items[e.RowIndex].IsDirectory ? Resources.IDS_MODLIBRARY_TOOLTIP_FOLDER : Resources.IDS_MODLIBRARY_TOOLTIP_MODULE;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a column header is clicked. The list does not sort
		/// itself, it just tells the owner that the sort order has changed
		/// </summary>
		/********************************************************************/
		private void ModuleListView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			// Only allow sorting in flat view mode
			if (!flatViewEnabled || (e.Button != MouseButtons.Left))
				return;

			FlatViewSortOrder newSortOrder;

			if (e.ColumnIndex == nameColumn.Index)
				newSortOrder = FlatViewSortOrder.NameThenPath;
			else if (e.ColumnIndex == pathColumn.Index)
				newSortOrder = FlatViewSortOrder.PathThenName;
			else
				return;

			if (newSortOrder == sortOrder)
				return;

			sortOrder = newSortOrder;
			UpdateSortGlyphs();

			SortOrderChanged?.Invoke(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a list item is double-clicked
		/// </summary>
		/********************************************************************/
		private void ModuleListView_DoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			// Only react when an actual row is double-clicked, not the
			// column headers or the empty area below the rows
			DataGridView.HitTestInfo hitTest = moduleListView.HitTest(e.X, e.Y);
			if (hitTest.Type != DataGridViewHitTestType.Cell)
				return;

			ActivateItem(hitTest.RowIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the list is clicked
		/// </summary>
		/********************************************************************/
		private void ModuleListView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
				return;

			// Only react when an actual row is clicked, not the
			// column headers or the empty area below the rows
			DataGridView.HitTestInfo hitTest = moduleListView.HitTest(e.X, e.Y);
			if (hitTest.Type != DataGridViewHitTestType.Cell)
				return;

			int index = hitTest.RowIndex;
			if ((index < 0) || (index >= items.Count))
				return;

			// Select the clicked item, so it is clear which item the menu is for
			if (!moduleListView.Rows[index].Selected)
				SelectItem(index);

			Point location = PointToClient(moduleListView.PointToScreen(e.Location));
			OpenContextMenu?.Invoke(this, new OpenContextMenuEventArgs(items[index], location));
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed down in the list view
		/// </summary>
		/********************************************************************/
		private void ModuleListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				// Suppress the key, so the list does not move to the next row
				e.Handled = true;
				e.SuppressKeyPress = true;

				if (moduleListView.SelectedRows.Count > 0)
					ActivateItem(moduleListView.SelectedRows[0].Index);
			}
			else if (e.KeyCode == Keys.Back)
			{
				e.Handled = true;

				NavigateUp?.Invoke(this, EventArgs.Empty);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the list view
		/// </summary>
		/********************************************************************/
		private void ModuleListView_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Control characters like Enter and Backspace are handled in KeyDown
			if (char.IsControl(e.KeyChar))
				return;

			e.Handled = true;

			// Add to search buffer and restart the timeout
			searchBuffer += e.KeyChar;
			searchTimer.Stop();
			searchTimer.Start();

			// Select the first matching item. If nothing matches, the
			// current selection is kept
			int index = items.FindIndex(item => item.Name.StartsWith(searchBuffer, StringComparison.CurrentCultureIgnoreCase));
			if (index >= 0)
				SelectItem(index);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when keyboard quick search timer expires (1 second)
		/// </summary>
		/********************************************************************/
		private void SearchTimer_Tick(object sender, EventArgs e)
		{
			ResetSearch();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Show the sort glyph on the column the list is sorted by. Glyphs
		/// are only shown in flat view, since that is the only view that
		/// can be sorted
		/// </summary>
		/********************************************************************/
		private void UpdateSortGlyphs()
		{
			nameColumn.HeaderCell.SortGlyphDirection = flatViewEnabled && (sortOrder == FlatViewSortOrder.NameThenPath) ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.None;
			pathColumn.HeaderCell.SortGlyphDirection = flatViewEnabled && (sortOrder == FlatViewSortOrder.PathThenName) ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.None;
		}



		/********************************************************************/
		/// <summary>
		/// Return the image to use for the current row
		/// </summary>
		/********************************************************************/
		private Bitmap GetRowImage(TreeNode entry)
		{
			if (entry.IsFavorite)
			{
				if (entry.IsDirectory)
				{
					if (combinedDirectoryFavoriteBitmap == null)
						combinedDirectoryFavoriteBitmap = CreateCombinedBitmap(imageBank.ModuleLibrary.Directory);

					return combinedDirectoryFavoriteBitmap;
				}

				if (combinedFileFavoriteBitmap == null)
					combinedFileFavoriteBitmap = CreateCombinedBitmap(imageBank.ModuleLibrary.File);

				return combinedFileFavoriteBitmap;
			}

			return entry.IsDirectory ? imageBank.ModuleLibrary.Directory : imageBank.ModuleLibrary.File;
		}



		/********************************************************************/
		/// <summary>
		/// Return the text to show in the path column
		/// </summary>
		/********************************************************************/
		private string GetPathText(TreeNode entry)
		{
			if (!flatViewEnabled || entry.IsDirectory)
				return string.Empty;

			// Extract path from FullPath (remove filename)
			int lastSlash = entry.FullPath.LastIndexOf('/');
			string pathOnly = lastSlash >= 0 ? entry.FullPath.Substring(0, lastSlash) : string.Empty;

			// Remove current path prefix to show only relative path
			if (!string.IsNullOrEmpty(currentPath) && pathOnly.StartsWith(currentPath))
			{
				pathOnly = pathOnly.Substring(currentPath.Length);
				if (pathOnly.StartsWith("/"))
					pathOnly = pathOnly.Substring(1);
			}

			return pathOnly;
		}



		/********************************************************************/
		/// <summary>
		/// Create combined bitmap
		/// </summary>
		/********************************************************************/
		private Bitmap CreateCombinedBitmap(Bitmap typeBitmap)
		{
			Bitmap favoriteBitmap = imageBank.ModuleLibrary.Favorite;

			int padding = 2;
			int width = typeBitmap.Width + padding + favoriteBitmap.Width;
			int height = Math.Max(typeBitmap.Height, favoriteBitmap.Height);

			// Create new bitmap and draw the two into it
			Bitmap bitmap = new Bitmap(width, height);

			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.DrawImage(typeBitmap, new Rectangle(new Point(0, 0), typeBitmap.Size), new Rectangle(new Point(0, 0), typeBitmap.Size), GraphicsUnit.Pixel);
				g.DrawImage(favoriteBitmap, new Rectangle(new Point(typeBitmap.Width + padding, 0), favoriteBitmap.Size), new Rectangle(new Point(0, 0), favoriteBitmap.Size), GraphicsUnit.Pixel);
			}

			return bitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Tell the parent that the item at the given index is activated
		/// </summary>
		/********************************************************************/
		private void ActivateItem(int index)
		{
			if ((index < 0) || (index >= items.Count))
				return;

			ItemActivate?.Invoke(this, new ItemActivateEventArgs(items[index], index));
		}



		/********************************************************************/
		/// <summary>
		/// Clear the keyboard quick search buffer
		/// </summary>
		/********************************************************************/
		private void ResetSearch()
		{
			searchBuffer = string.Empty;
			searchTimer.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Center the loading label
		/// </summary>
		/********************************************************************/
		private void CenterStatusLabel()
		{
			statusLabel.PerformLayout();

			Size listSize = statusLabel.Parent.ClientSize;
			statusLabel.Location = new Point((listSize.Width - statusLabel.Width) / 2, (listSize.Height - statusLabel.Height) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// Format file size in KB, MB, GB like Windows Explorer
		/// </summary>
		/********************************************************************/
		private string FormatFileSize(long bytes)
		{
			if (bytes < 1024)
				return $"{bytes} bytes";

			if (bytes < 1024 * 1024)
				return $"{Math.Ceiling(bytes / 1024.0):0} KB";

			if (bytes < 1024 * 1024 * 1024)
				return $"{Math.Round(bytes / (1024.0 * 1024.0), 1):0.0} MB";

			return $"{Math.Round(bytes / (1024.0 * 1024.0 * 1024.0), 2):0.00} GB";
		}



		/********************************************************************/
		/// <summary>
		/// Count files recursively in a tree node
		/// </summary>
		/********************************************************************/
		private int CountFilesInTree(TreeNode node)
		{
			int count = 0;

			foreach (TreeNode child in node.Children)
			{
				if (child.IsDirectory)
					count += CountFilesInTree(child);
				else
					count++;
			}

			return count;
		}
		#endregion
	}
}

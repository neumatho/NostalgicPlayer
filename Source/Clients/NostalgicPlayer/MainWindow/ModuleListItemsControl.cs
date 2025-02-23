/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Controls;
using Polycode.NostalgicPlayer.GuiKit.Components;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This control draws the list items list
	/// </summary>
	public partial class ModuleListItemsControl : UserControl
	{
		private readonly Color textColor = Color.FromArgb(30, 57, 91);
		private readonly Color defaultSubSongColor = Color.FromArgb(159, 81, 255);

		private readonly Color selectedBackgroundColor1 = Color.FromArgb(255, 225, 112);
		private readonly Color selectedBackgroundColor2 = Color.FromArgb(255, 216, 108);
		private readonly Color selectedBackgroundColor3 = Color.FromArgb(255, 237, 123);

		private readonly Color dragDropLineColor = Color.CornflowerBlue;

		/// <summary></summary>
		public const int ItemHeight = 13 + 3;

		private ModuleListControl owner;
		private VScrollBar vScrollBar;

		private bool listNumberEnabled;
		private bool showFullPathEnabled;

		private int lastItemSelected;
		private int deltaToLastSelected;

		private int lastToolTipItem;

		private ModuleListControl.ItemCollection collection;
		private readonly SortedDictionary<int, ModuleListItem> selectedItems;

		private IReadOnlyList<ModuleListItem> publicSelectedItems;
		private IReadOnlyList<int> publicSelectedIndexes;

		// Drag'n'drop variables
		private int indexOfItemUnderMouseToDrop = -2;
		private Rectangle dragBoxFromMouseDown;
		private bool drawLine;
		private ModuleListControl.FileDropType dropType;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListItemsControl()
		{
			InitializeComponent();

			listNumberEnabled = false;
			showFullPathEnabled = false;

			lastItemSelected = -1;
			deltaToLastSelected = 0;

			lastToolTipItem = -1;

			selectedItems = new SortedDictionary<int, ModuleListItem>();
			publicSelectedItems = null;
			publicSelectedIndexes = null;
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Return the item collection
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ModuleListControl.ItemCollection Items => collection;
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Set the owner of this control
		/// </summary>
		/********************************************************************/
		public void SetControls(ModuleListControl moduleListControl, VScrollBar scrollBar)
		{
			owner = moduleListControl;
			vScrollBar = scrollBar;

			collection = new ModuleListControl.ItemCollection(moduleListControl, this);
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the selected items
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IReadOnlyList<ModuleListItem> SelectedItems
		{
			get
			{
				if (publicSelectedItems == null)
					publicSelectedItems = new ReadOnlyCollection<ModuleListItem>(selectedItems.Values.ToList());

				return publicSelectedItems;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the selected indexes collection
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IReadOnlyList<int> SelectedIndexes
		{
			get
			{
				if (publicSelectedIndexes == null)
					publicSelectedIndexes = new ReadOnlyCollection<int>(selectedItems.Keys.ToList());

				return publicSelectedIndexes;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return any selected index if at least one item is selected or -1
		/// if none is selected
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectedIndex
		{
			get
			{
				if (selectedItems.Count == 0)
					return -1;

				return selectedItems.Keys.First();
			}

			set
			{
				ClearSelectionLists(-1);

				if (value >= 0)
					selectedItems[value] = collection[value];

				ItemSelectionChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return any selected item if at least one item is selected or null
		/// if none is selected
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ModuleListItem SelectedItem
		{
			get
			{
				if (selectedItems.Count == 0)
					return null;

				return selectedItems.Values.First();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear public selection lists if needed
		/// </summary>
		/********************************************************************/
		public void ClearSelectionLists(int index)
		{
			if (index == -1)
			{
				publicSelectedItems = null;
				publicSelectedIndexes = null;

				selectedItems.Clear();

				lastItemSelected = -1;
				deltaToLastSelected = 0;
			}
			else if (selectedItems.ContainsKey(index))
			{
				publicSelectedItems = null;
				publicSelectedIndexes = null;

				selectedItems.Remove(index);

				if (index == lastItemSelected)
				{
					lastItemSelected = -1;
					deltaToLastSelected = 0;
				}
				else if (((index >= (lastItemSelected + deltaToLastSelected)) && (index <= lastItemSelected)) || ((index <= (lastItemSelected + deltaToLastSelected)) && (index >= lastItemSelected)))
				{
					deltaToLastSelected--;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Either select or deselect the item at the given index
		/// </summary>
		/********************************************************************/
		public void SetSelected(int index, bool select)
		{
			if (select)
				selectedItems[index] = collection[index];
			else
				selectedItems.Remove(index);

			ItemSelectionChanged();

			if (select)
				MakeItemVisible(index);
		}



		/********************************************************************/
		/// <summary>
		/// Either select or deselect all the items
		/// </summary>
		/********************************************************************/
		public void SetSelectedOnAllItems(bool select)
		{
			if (select)
			{
				for (int i = collection.Count - 1; i >= 0; i--)
					selectedItems[i] = collection[i];
			}
			else
				selectedItems.Clear();

			ItemSelectionChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate which item index is under the given point
		/// </summary>
		/********************************************************************/
		public int IndexFromPoint(Point point)
		{
			if ((collection == null) || (collection.Count == 0))
				return -1;

			int index = (point.Y / ItemHeight) + vScrollBar.Value;
			if (index >= collection.Count)
				return -1;

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			listNumberEnabled = enable;

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable full path viewing
		/// </summary>
		/********************************************************************/
		public void EnableFullPath(bool enable)
		{
			showFullPathEnabled = enable;

			lastToolTipItem = -1;
			toolTip.SetToolTip(this, string.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the last drag'n'drop that has been made
		/// </summary>
		/********************************************************************/
		public ModuleListControl.DragDropInformation GetLatestDragAndDropInformation(DragEventArgs e)
		{
			return new ModuleListControl.DragDropInformation
			{
				Type = e.Data.GetDataPresent(GetType()) ? ModuleListControl.DragDropType.List : e.Data.GetDataPresent(DataFormats.FileDrop) ? ModuleListControl.DragDropType.File : ModuleListControl.DragDropType.Unknown,
				IndexOfItemUnderMouseToDrop = indexOfItemUnderMouseToDrop,
				DropType = dropType
			};
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Paint the whole control
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawBackground(e.Graphics);
			DrawItems(e.Graphics);

			base.OnPaint(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the list control
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Check for different keyboard shortcuts
			Keys modifiers = keyData & Keys.Modifiers;
			Keys key = keyData & Keys.KeyCode;

			switch (key)
			{
				case Keys.Up:
				{
					if (lastItemSelected == -1)
						SelectFirstVisibleItem();
					else
					{
						if (modifiers == Keys.Shift)
							UpdateSelectedItemsUp(1);
						else
						{
							if (lastItemSelected > 0)
							{
								lastItemSelected--;

								selectedItems.Clear();
								selectedItems[lastItemSelected] = collection[lastItemSelected];

								MakeItemVisible(lastItemSelected);
								deltaToLastSelected = 0;
							}
						}

						ItemSelectionChanged();
					}

					return true;
				}

				case Keys.Down:
				{
					if (lastItemSelected == -1)
						SelectLastVisibleItem();
					else
					{
						if (modifiers == Keys.Shift)
							UpdateSelectedItemsDown(1);
						else
						{
							if (lastItemSelected < (collection.Count - 1))
							{
								lastItemSelected++;

								selectedItems.Clear();
								selectedItems[lastItemSelected] = collection[lastItemSelected];

								MakeItemVisible(lastItemSelected);
								deltaToLastSelected = 0;
							}
						}

						ItemSelectionChanged();
					}

					return true;
				}

				case Keys.PageUp:
				{
					if (lastItemSelected == -1)
						SelectFirstVisibleItem();
					else
					{
						int itemsVisible = CalculateNumberOfVisibleItems();
						int lastSelected = lastItemSelected + deltaToLastSelected;
						int topIndex = Math.Max(lastSelected - itemsVisible + 1, 0);

						if (modifiers == Keys.Shift)
							UpdateSelectedItemsUp(lastSelected - topIndex);
						else
						{
							if (lastItemSelected > 0)
							{
								lastItemSelected = topIndex;

								selectedItems.Clear();
								selectedItems[lastItemSelected] = collection[lastItemSelected];

								vScrollBar.Value = topIndex;
								deltaToLastSelected = 0;
							}
						}

						ItemSelectionChanged();
					}

					return true;
				}

				case Keys.PageDown:
				{
					if (lastItemSelected == -1)
						SelectLastVisibleItem();
					else
					{
						int itemsVisible = CalculateNumberOfVisibleItems();
						int lastSelected = lastItemSelected + deltaToLastSelected;
						int topIndex = Math.Min(lastSelected, Math.Max(collection.Count - itemsVisible, 0));

						if (modifiers == Keys.Shift)
							UpdateSelectedItemsDown(itemsVisible - 1);
						else
						{
							if (lastItemSelected < (collection.Count - 1))
							{
								lastItemSelected = Math.Min(topIndex + itemsVisible - 1, collection.Count - 1);

								selectedItems.Clear();
								selectedItems[lastItemSelected] = collection[lastItemSelected];

								vScrollBar.Value = topIndex;
								deltaToLastSelected = 0;
							}
						}

						ItemSelectionChanged();
					}

					return true;
				}

				case Keys.Home:
				{
					if (collection.Count > 0)
					{
						selectedItems.Clear();

						if (modifiers == Keys.Shift)
						{
							for (int i = 0; i <= lastItemSelected; i++)
								selectedItems[i] = collection[i];

							deltaToLastSelected = 0 - lastItemSelected;
						}
						else
						{
							selectedItems[0] = collection[0];

							lastItemSelected = 0;
							deltaToLastSelected = 0;
						}

						vScrollBar.Value = 0;

						ItemSelectionChanged();
					}

					return true;
				}

				case Keys.End:
				{
					if (collection.Count > 0)
					{
						selectedItems.Clear();

						int lastIndex = collection.Count - 1;

						if (modifiers == Keys.Shift)
						{
							for (int i = lastItemSelected; i <= lastIndex; i++)
								selectedItems[i] = collection[i];

							deltaToLastSelected = lastIndex - lastItemSelected;
						}
						else
						{
							selectedItems[lastIndex] = collection[lastIndex];

							lastItemSelected = lastIndex;
							deltaToLastSelected = 0;
						}

						vScrollBar.Value = Math.Max(lastIndex - CalculateNumberOfVisibleItems() + 1, 0);

						ItemSelectionChanged();
					}

					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse button is pressed
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				bool selectionChanged = false;

				int index = IndexFromPoint(e.Location);
				if (index != -1)
				{
					switch (ModifierKeys)
					{
						case Keys.Control:
						{
							if (!selectedItems.ContainsKey(index))
								selectedItems[index] = collection[index];
							else
								selectedItems.Remove(index);

							lastItemSelected = index;
							selectionChanged = true;
							break;
						}

						case Keys.Shift:
						{
							if (lastItemSelected == -1)
							{
								selectedItems[index] = collection[index];

								lastItemSelected = index;
							}
							else
							{
								selectedItems.Clear();

								int from = Math.Min(lastItemSelected, index);
								int to = Math.Max(lastItemSelected, index);

								for (int i = from; i <= to; i++)
									selectedItems[i] = collection[i];
							}

							selectionChanged = true;
							break;
						}

						default:
						{
							if (!selectedItems.ContainsKey(index))
							{
								selectedItems.Clear();
								selectedItems[index] = collection[index];

								lastItemSelected = index;
								selectionChanged = true;
							}
							break;
						}
					}

					// Remember the point where the mouse down occurred. The DragSize
					// indicates the size that the mouse can move before a drag event
					// should be started
					Size dragSize = SystemInformation.DragSize;

					// Create a rectangle using DragSize, with the mouse position
					// being at the center of the rectangle
					dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
				}
				else
				{
					selectedItems.Clear();
					lastItemSelected = -1;
					selectionChanged = true;

					dragBoxFromMouseDown = Rectangle.Empty;
				}

				if (selectionChanged)
				{
					deltaToLastSelected = 0;

					ItemSelectionChanged();
				}
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse button is released
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int previousCount = selectedItems.Count;

				int index = IndexFromPoint(e.Location);
				if (index != -1)
				{
					switch (ModifierKeys)
					{
						case Keys.Control:
						{
							if ((index != lastItemSelected) && selectedItems.ContainsKey(index))
							{
								selectedItems.Remove(index);

								lastItemSelected = index;
							}
							break;
						}

						case Keys.Shift:
							break;

						default:
						{
							if (selectedItems.ContainsKey(index))
							{
								selectedItems.Clear();
								selectedItems[index] = collection[index];

								lastItemSelected = index;
							}
							break;
						}
					}
				}
				else
				{
					selectedItems.Clear();
					lastItemSelected = -1;
				}

				if (selectedItems.Count != previousCount)
				{
					deltaToLastSelected = 0;

					ItemSelectionChanged();
				}
			}

			dragBoxFromMouseDown = Rectangle.Empty;

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse move around in the list control
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			// If the mouse moves outside the rectangle, start the drag
			if ((dragBoxFromMouseDown != Rectangle.Empty) && !dragBoxFromMouseDown.Contains(e.X, e.Y))
			{
				// Start the dragging, where custom data is all the selected items.
				// Also make a copy of the collection
				DoDragDrop(this, DragDropEffects.Move);

				// Stop drag functionality by clearing the rectangle
				dragBoxFromMouseDown = Rectangle.Empty;
			}
			else
			{
				if (showFullPathEnabled)
				{
					int index = IndexFromPoint(e.Location);
					if (index != lastToolTipItem)
					{
						lastToolTipItem = index;

						if (index != -1)
							toolTip.SetToolTip(this, collection[index].ListItem.FullPath);
						else
							toolTip.SetToolTip(this, string.Empty);
					}
				}
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse move outside the list control
		/// </summary>
		/********************************************************************/
		protected override void OnMouseLeave(EventArgs e)
		{
			toolTip.SetToolTip(this, string.Empty);

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse enters the list control and a dragging
		/// begins
		/// </summary>
		/********************************************************************/
		protected override void OnDragEnter(DragEventArgs e)
		{
			indexOfItemUnderMouseToDrop = -2;

			// Start the timer
			scrollTimer.Start();

			base.OnDragEnter(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the mouse leaves the list control
		/// </summary>
		/********************************************************************/
		protected override void OnDragLeave(EventArgs e)
		{
			DrawLine(true);

			// Stop the timer again
			scrollTimer.Stop();

			base.OnDragLeave(e);
		}



		/********************************************************************/
		/// <summary>
		/// Should set if it is valid to drag into the list box
		/// </summary>
		/********************************************************************/
		protected override void OnDragOver(DragEventArgs e)
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
						e.Effect = DragDropEffects.Copy;        // Append to list
						dropType = ModuleListControl.FileDropType.Append;
						drawLine = false;
					}
					else
					{
						e.Effect = DragDropEffects.Move;        // Insert into position in list
						dropType = ModuleListControl.FileDropType.Insert;
						drawLine = true;
					}
				}
				else
				{
					e.Effect = DragDropEffects.Move;            // Clear list and add files
					dropType = ModuleListControl.FileDropType.ClearAndAdd;
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
				Point clientPoint = PointToClient(new Point(e.X, e.Y));
				int index = IndexFromPoint(clientPoint);

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

			base.OnDragOver(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the drop ends
		/// </summary>
		/********************************************************************/
		protected override void OnDragDrop(DragEventArgs e)
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
					var reversedSelectedItems = selectedItems.OrderByDescending(i => i.Key);

					if (indexOfItemUnderMouseToDrop == -1)
					{
						int insertAt = collection.Count;

						foreach (KeyValuePair<int, ModuleListItem> pair in reversedSelectedItems)
						{
							collection.Insert(insertAt--, pair.Value);
							collection.RemoveAt(pair.Key);
						}
					}
					else
					{
						int insertAt = indexOfItemUnderMouseToDrop;
						int insertExtra = 0;
						int removeExtra = 0;

						foreach (KeyValuePair<int, ModuleListItem> pair in reversedSelectedItems)
						{
							collection.Insert(insertAt + insertExtra, pair.Value);

							if (pair.Key < insertAt)
							{
								collection.RemoveAt(pair.Key);
								insertExtra--;
							}
							else
							{
								collection.RemoveAt(pair.Key + removeExtra + 1);
								removeExtra++;
							}
						}
					}

					ClearSelectionLists(-1);
					ItemSelectionChanged();
				}
			}

			base.OnDragDrop(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called by an interval to check if a scroll is needed
		/// </summary>
		/********************************************************************/
		private void ScrollTimer_Tick(object sender, EventArgs e)
		{
			if (collection.Count > 0)
			{
				int y = PointToClient(MousePosition).Y;

				if ((Height - y) <= 10)
				{
					int itemsThatCanBeSeen = CalculateNumberOfVisibleItems();
					if (owner.TopIndex + itemsThatCanBeSeen < collection.Count)
					{
						DrawLine(true);
						owner.TopIndex++;
					}
				}
				else if (y <= 10)
				{
					if (owner.TopIndex > 0)
					{
						DrawLine(true);
						owner.TopIndex--;
					}
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return brush to use when drawing selected item background
		/// </summary>
		/********************************************************************/
		private Brush GetSelectedItemBackgroundBrush()
		{
			LinearGradientBrush selectedItemBackgroundBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, ItemHeight), selectedBackgroundColor1, selectedBackgroundColor3);

			ColorBlend blend = new ColorBlend
			{
				Colors = [ selectedBackgroundColor1, selectedBackgroundColor2, selectedBackgroundColor2, selectedBackgroundColor3 ],
				Positions = [ 0.0f, 0.1f, 0.7f, 1.0f ]
			};

			selectedItemBackgroundBrush.InterpolationColors = blend;

			return selectedItemBackgroundBrush;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g)
		{
			g.FillRectangle(Brushes.White, 0, 0, Width, Height);
		}



		/********************************************************************/
		/// <summary>
		/// Draw all the items
		/// </summary>
		/********************************************************************/
		private void DrawItems(Graphics g)
		{
			if ((collection != null) && (collection.Count > 0))
			{
				using (Font font = FontPalette.GetRegularFont())
				{
					using (Font boldFont = new Font(font, FontStyle.Bold))
					{
						using (Brush selectedItemBackgroundBrush = GetSelectedItemBackgroundBrush())
						{
							int count = collection.Count;
							int height = Height;

							for (int i = vScrollBar.Value, y = 0; i < count; i++)
							{
								DrawSingleItem(g, font, boldFont, selectedItemBackgroundBrush, y, i);

								y += ItemHeight;
								if (y >= height)
									break;
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw a single item
		/// </summary>
		/********************************************************************/
		private void DrawSingleItem(Graphics g, Font font, Font boldFont, Brush selectedItemBackgroundBrush, int y, int itemIndex)
		{
			ModuleListItem item = collection[itemIndex];

			DrawItemBackground(g, selectedItemBackgroundBrush, y, itemIndex);
			DrawItemPlayingStatus(g, y, item);
			int timeWidth = DrawItemDuration(g, font, y, item);
			DrawItemName(g, font, boldFont, itemIndex, y, timeWidth, item);
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the background of a single item
		/// </summary>
		/********************************************************************/
		private void DrawItemBackground(Graphics g, Brush selectedItemBackgroundBrush, int y, int itemIndex)
		{
			if (selectedItems.ContainsKey(itemIndex))
				g.FillRectangle(selectedItemBackgroundBrush, 0, y, Width, ItemHeight);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the current item is the playing one and if so, draw the
		/// image telling so
		/// </summary>
		/********************************************************************/
		private void DrawItemPlayingStatus(Graphics g, int y, ModuleListItem item)
		{
			if (item.IsPlaying)
			{
				Image image = Resources.IDB_PLAYING_ITEM;
				g.DrawImage(image, 2, y + 4, image.Width, image.Height);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the duration of the item if any
		/// </summary>
		/********************************************************************/
		private int DrawItemDuration(Graphics g, Font font, int y, ModuleListItem item)
		{
			if (!item.HaveTime)
				return 0;

			string moduleTime = item.Duration.ToString((int)item.Duration.TotalHours > 0 ? Resources.IDS_TIMEFORMAT : Resources.IDS_TIMEFORMAT_SMALL);

			Size size = TextRenderer.MeasureText(g, moduleTime, font);
			TextRenderer.DrawText(g, moduleTime, font, new Point(Width - size.Width, y + 1), textColor, TextFormatFlags.NoPrefix);

			return size.Width;
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the name of the item
		/// </summary>
		/********************************************************************/
		private void DrawItemName(Graphics g, Font font, Font boldFont, int index, int y, int timeWidth, ModuleListItem item)
		{
			int maxWidth = Width - 12 - timeWidth;
			string defaultSubSong = string.Empty;

			if (item.DefaultSubSong.HasValue)
			{
				defaultSubSong = string.Format(Resources.IDS_DEFAULT_SUBSONG, item.DefaultSubSong.Value + 1);
				int width = TextRenderer.MeasureText(g, defaultSubSong, boldFont).Width;

				maxWidth -= width;
			}

			string name = listNumberEnabled ? $"{index + 1}. {item.ListItem.DisplayName}" : item.ListItem.DisplayName;
			string showName = name;
			int nameWidth = TextRenderer.MeasureText(g, showName, font).Width;

			while (nameWidth > maxWidth)
			{
				name = name.Substring(0, name.Length - 1);
				showName = name + "…";

				nameWidth = TextRenderer.MeasureText(g, showName, font).Width;
			}

			TextRenderer.DrawText(g, showName, font, new Point(10, y + 1), textColor, TextFormatFlags.NoPrefix);

			if (item.DefaultSubSong.HasValue)
				TextRenderer.DrawText(g, defaultSubSong, boldFont, new Point(10 + nameWidth, y + 1), defaultSubSongColor, TextFormatFlags.NoPrefix);
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
				using (Graphics g = CreateGraphics())
				{
					// Find the position where to draw the line
					int pos;

					int count = collection.Count;

					int indexCheck;

					// If no items are in the list or it is the first position
					// to insert, just draw the line at top of the control
					int indexFromTop = indexOfItemUnderMouseToDrop - owner.TopIndex;

					if ((count == 0) || (indexFromTop == 0))
					{
						pos = 0;
						indexCheck = -1;
					}
					else
					{
						// Do we point at any item?
						if (indexOfItemUnderMouseToDrop < 0)
						{
							pos = (count - owner.TopIndex) * ItemHeight - 1;
							indexCheck = count - 1;
						}
						else
						{
							pos = indexFromTop * ItemHeight - 1;
							indexCheck = indexOfItemUnderMouseToDrop - 1;
						}
					}

					if (erase && selectedItems.ContainsKey(indexCheck))
					{
						using (Font font = FontPalette.GetRegularFont())
						{
							using (Font boldFont = new Font(font, FontStyle.Bold))
							{
								using (Brush selectedItemBackgroundBrush = GetSelectedItemBackgroundBrush())
								{
									DrawSingleItem(g, font, boldFont, selectedItemBackgroundBrush, pos - ItemHeight + 1, indexCheck);
								}
							}
						}
					}
					else
					{
						// Draw the line
						using (Pen pen = new Pen(erase ? Color.White : dragDropLineColor))
						{
							g.DrawLine(pen, 0, pos, Width, pos);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do whatever is needed when the selection collection changes
		/// </summary>
		/********************************************************************/
		private void ItemSelectionChanged()
		{
			publicSelectedItems = null;
			publicSelectedIndexes = null;
			owner.OnSelectedIndexChanged();

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of visible items
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int CalculateNumberOfVisibleItems()
		{
			return Height / ItemHeight;
		}



		/********************************************************************/
		/// <summary>
		/// Will make sure that the item with the given index is visible
		/// </summary>
		/********************************************************************/
		private void MakeItemVisible(int index)
		{
			if (index < vScrollBar.Value)
				vScrollBar.Value = index;
			else
			{
				int itemsVisible = CalculateNumberOfVisibleItems();

				if (index >= (vScrollBar.Value + itemsVisible))
				{
					int newValue = index - itemsVisible + 1;
					if (newValue >= collection.Count)
						newValue = Math.Max(collection.Count - itemsVisible, index);

					vScrollBar.Value = newValue;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will select the first item visible if any
		/// </summary>
		/********************************************************************/
		private void SelectFirstVisibleItem()
		{
			if (collection.Count > 0)
			{
				lastItemSelected = vScrollBar.Value;
				deltaToLastSelected = 0;

				selectedItems.Clear();
				selectedItems[lastItemSelected] = collection[lastItemSelected];

				ItemSelectionChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will select the last item visible if any
		/// </summary>
		/********************************************************************/
		private void SelectLastVisibleItem()
		{
			if (collection.Count > 0)
			{
				int itemsVisible = CalculateNumberOfVisibleItems();

				lastItemSelected = Math.Min(vScrollBar.Value + itemsVisible - 1, collection.Count - 1);
				deltaToLastSelected = 0;

				selectedItems.Clear();
				selectedItems[lastItemSelected] = collection[lastItemSelected];

				ItemSelectionChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the selected items moving the count given items up
		/// </summary>
		/********************************************************************/
		private void UpdateSelectedItemsUp(int count)
		{
			int newSelectedIndex = lastItemSelected + deltaToLastSelected;

			while ((count > 0) && (newSelectedIndex > 0))
			{
				deltaToLastSelected--;

				newSelectedIndex = lastItemSelected + deltaToLastSelected;

				if (deltaToLastSelected >= 0)
					selectedItems.Remove(newSelectedIndex + 1);
				else
					selectedItems[newSelectedIndex] = collection[newSelectedIndex];

				count--;
			}

			MakeItemVisible(newSelectedIndex);
		}



		/********************************************************************/
		/// <summary>
		/// Will update the selected items moving the count given items down
		/// </summary>
		/********************************************************************/
		private void UpdateSelectedItemsDown(int count)
		{
			int collectionCount = collection.Count - 1;
			int newSelectedIndex = lastItemSelected + deltaToLastSelected;

			while ((count > 0) && (newSelectedIndex < collectionCount))
			{
				deltaToLastSelected++;

				newSelectedIndex = lastItemSelected + deltaToLastSelected;

				if (deltaToLastSelected <= 0)
					selectedItems.Remove(newSelectedIndex - 1);
				else
					selectedItems[newSelectedIndex] = collection[newSelectedIndex];

				count--;
			}

			MakeItemVisible(newSelectedIndex);
		}
		#endregion
	}
}

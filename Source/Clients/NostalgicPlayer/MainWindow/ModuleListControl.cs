/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Native;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This control replaces a normal ListBox control to speed up things
	/// with large lists
	/// </summary>
	public partial class ModuleListControl : UserControl
	{
		#region ItemCollection class
		/// <summary>
		/// Holds all the items to show in the control
		/// </summary>
		public class ItemCollection : IList<ModuleListItem>
		{
			private readonly ModuleListControl owner;
			private readonly ModuleListItemsControl listItemsControl;

			private readonly List<ModuleListItem> collection;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public ItemCollection(ModuleListControl moduleListControl, ModuleListItemsControl moduleListItemsControl)
			{
				owner = moduleListControl;
				listItemsControl = moduleListItemsControl;

				collection = new List<ModuleListItem>();
			}

			#region Properties
			/********************************************************************/
			/// <summary>
			/// Return the number of items in the collection
			/// </summary>
			/********************************************************************/
			public int Count => collection.Count;



			/********************************************************************/
			/// <summary>
			/// Tells whether this collection is read-only or not
			/// </summary>
			/********************************************************************/
			public bool IsReadOnly => false;
			#endregion

			#region Public methods
			/********************************************************************/
			/// <summary>
			/// Return an enumerator to traverse the collection
			/// </summary>
			/********************************************************************/
			public IEnumerator<ModuleListItem> GetEnumerator()
			{
				return collection.GetEnumerator();
			}



			/********************************************************************/
			/// <summary>
			/// Return an enumerator to traverse the collection
			/// </summary>
			/********************************************************************/
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}



			/********************************************************************/
			/// <summary>
			/// Add a single item to the collection
			/// </summary>
			/********************************************************************/
			public void Add(ModuleListItem item)
			{
				collection.Add(item);

				InvalidateIfNeeded(collection.Count - 1);
			}



			/********************************************************************/
			/// <summary>
			/// Add a bunch of items to the collection
			/// </summary>
			/********************************************************************/
			public void AddRange(IEnumerable<ModuleListItem> items)
			{
				int startIndex = collection.Count;

				collection.AddRange(items);

				InvalidateIfNeeded(startIndex, collection.Count - 1);
			}



			/********************************************************************/
			/// <summary>
			/// Insert a single item to the collection at the specified position
			/// </summary>
			/********************************************************************/
			public void Insert(int index, ModuleListItem item)
			{
				collection.Insert(index, item);

				InvalidateIfNeeded(index);
			}



			/********************************************************************/
			/// <summary>
			/// Insert a bunch of items to the collection at the specified
			/// position
			/// </summary>
			/********************************************************************/
			public void InsertRange(int index, IEnumerable<ModuleListItem> items)
			{
				List<ModuleListItem> list = items.ToList();

				for (int i = list.Count - 1; i >= 0; i--)
					collection.Insert(index, list[i]);

				InvalidateIfNeeded(index, list.Count - 1);
			}



			/********************************************************************/
			/// <summary>
			/// Remove the item from the collection at the index given
			/// </summary>
			/********************************************************************/
			public void RemoveAt(int index)
			{
				collection.RemoveAt(index);

				listItemsControl.ClearSelectionLists(index);
				InvalidateIfNeeded(index);
			}



			/********************************************************************/
			/// <summary>
			/// Will try to find the given item and remove it from the collection
			/// </summary>
			/********************************************************************/
			public bool Remove(ModuleListItem item)
			{
				int index = collection.IndexOf(item);
				if (index != -1)
				{
					RemoveAt(index);
					return true;
				}

				return false;
			}



			/********************************************************************/
			/// <summary>
			/// Remove all items from the collection
			/// </summary>
			/********************************************************************/
			public void Clear()
			{
				collection.Clear();

				listItemsControl.ClearSelectionLists(-1);
				owner.FixScrollbar();
				listItemsControl.Invalidate();
			}



			/********************************************************************/
			/// <summary>
			/// Checks to see if the given item is stored in the collection
			/// </summary>
			/********************************************************************/
			public bool Contains(ModuleListItem item)
			{
				return collection.Contains(item);
			}



			/********************************************************************/
			/// <summary>
			/// Try to find the given item and return the index in the collection
			/// </summary>
			/********************************************************************/
			public int IndexOf(ModuleListItem item)
			{
				return collection.IndexOf(item);
			}



			/********************************************************************/
			/// <summary>
			/// Copy the collection into the given array
			/// </summary>
			/********************************************************************/
			public void CopyTo(ModuleListItem[] array, int arrayIndex)
			{
				collection.CopyTo(array, arrayIndex);
			}



			/********************************************************************/
			/// <summary>
			/// Return or set the item at the given index from the collection
			/// </summary>
			/********************************************************************/
			public ModuleListItem this[int index]
			{
				get => collection[index];

				set
				{
					collection[index] = value;

					InvalidateIfNeeded(index);
				}
			}
			#endregion

			#region Private methods
			/********************************************************************/
			/// <summary>
			/// Invalidate the list control, if the given index is visible
			/// </summary>
			/********************************************************************/
			private void InvalidateIfNeeded(int index)
			{
				owner.FixScrollbar();

				if (owner.IsItemVisible(index))
					listItemsControl.Invalidate();
			}



			/********************************************************************/
			/// <summary>
			/// Invalidate the list control, if any items in the given range of
			/// indexes are visible
			/// </summary>
			/********************************************************************/
			private void InvalidateIfNeeded(int startIndex, int endIndex)
			{
				owner.FixScrollbar();

				for (int i = startIndex; i <= endIndex; i++)
				{
					if (owner.IsItemVisible(i))
					{
						listItemsControl.Invalidate();
						break;
					}
				}
			}
			#endregion
		}
		#endregion

		/// <summary>
		/// Tells what kind of drag'n'drop that has been made
		/// </summary>
		public enum DragDropType
		{
			/// <summary>
			/// Not a known type
			/// </summary>
			Unknown,

			/// <summary>
			/// Inside the list
			/// </summary>
			List,

			/// <summary>
			/// File drop
			/// </summary>
			File
		}

		/// <summary>
		/// What kind of file drop has been made
		/// </summary>
		public enum FileDropType
		{
			/// <summary></summary>
			ClearAndAdd,
			/// <summary></summary>
			Append,
			/// <summary></summary>
			Insert
		}

		/// <summary>
		/// Holds information about the last drag'n'drop
		/// </summary>
		public struct DragDropInformation
		{
			/// <summary></summary>
			public DragDropType Type;
			/// <summary></summary>
			public int IndexOfItemUnderMouseToDrop;
			/// <summary></summary>
			public FileDropType DropType;
		}

		private int updateCount;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListControl()
		{
			InitializeComponent();

			moduleListItemsControl.SetControls(this, moduleListScrollBar);

			updateCount = 0;
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Return the item collection
		/// </summary>
		/********************************************************************/
		public ItemCollection Items => moduleListItemsControl.Items;



		/********************************************************************/
		/// <summary>
		/// Return the selected items collection
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<ModuleListItem> SelectedItems => moduleListItemsControl.SelectedItems;



		/********************************************************************/
		/// <summary>
		/// Return the selected indexes collection
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<int> SelectedIndexes => moduleListItemsControl.SelectedIndexes;



		/********************************************************************/
		/// <summary>
		/// Return any selected index if at least one item is selected or -1
		/// if none is selected
		/// </summary>
		/********************************************************************/
		public int SelectedIndex
		{
			get => moduleListItemsControl.SelectedIndex;
			set => moduleListItemsControl.SelectedIndex = value;
		}


		/********************************************************************/
		/// <summary>
		/// Return any selected item if at least one item is selected or null
		/// if none is selected
		/// </summary>
		/********************************************************************/
		public ModuleListItem SelectedItem => moduleListItemsControl.SelectedItem;



		/********************************************************************/
		/// <summary>
		/// Hold the item index which is shown in the top of the list
		/// </summary>
		/********************************************************************/
		public int TopIndex
		{
			get => moduleListScrollBar.Value;

			set
			{
				if (value < 0)
					value = 0;
				else
				{
					int maxValue = moduleListScrollBar.Maximum - moduleListScrollBar.LargeChange + 1;

					if (value > maxValue)
						value = maxValue;
				}

				moduleListScrollBar.Value = value;
			}
		}
		#endregion

		#region Events
		/********************************************************************/
		/// <summary>
		/// Event called when the selected items changes
		/// </summary>
		/********************************************************************/
		public event EventHandler SelectedIndexChanged;
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will stop updating the control until EndUpdate() is called
		/// </summary>
		/********************************************************************/
		public void BeginUpdate()
		{
			if (!IsHandleCreated)
				return;

			if (updateCount == 0)
				User32.SendMessageW(Handle, WM.SETREDRAW, new IntPtr(0), new IntPtr(0));

			updateCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Will start updating the control again
		/// </summary>
		/********************************************************************/
		public bool EndUpdate()
		{
			if (updateCount > 0)
			{
				updateCount--;

				if (updateCount == 0)
				{
					User32.SendMessageW(Handle, WM.SETREDRAW, new IntPtr(1), new IntPtr(0));
					Invalidate(true);
				}

				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will invalidate a single item
		/// </summary>
		/********************************************************************/
		public void InvalidateItem(int index)
		{
			if (index >= 0)
			{
				if (IsItemVisible(index))
					moduleListItemsControl.Invalidate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Either select or deselect the item at the given index
		/// </summary>
		/********************************************************************/
		public void SetSelected(int index, bool select)
		{
			moduleListItemsControl.SetSelected(index, select);
		}



		/********************************************************************/
		/// <summary>
		/// Either select or deselect all the items
		/// </summary>
		/********************************************************************/
		public void SetSelectedOnAllItems(bool select)
		{
			moduleListItemsControl.SetSelectedOnAllItems(select);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate which item index is under the given point
		/// </summary>
		/********************************************************************/
		public int IndexFromPoint(Point point)
		{
			return moduleListItemsControl.IndexFromPoint(point);
		}



		/********************************************************************/
		/// <summary>
		/// Find out, if the item with the given index is visible or not
		/// </summary>
		/********************************************************************/
		public bool IsItemVisible(int index)
		{
			int itemsVisible = Height / ModuleListItemsControl.ItemHeight;

			if ((index >= moduleListScrollBar.Value) && (index < (moduleListScrollBar.Value + itemsVisible)))
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			moduleListItemsControl.EnableListNumber(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Return information about the last drag'n'drop that has been made
		/// </summary>
		/********************************************************************/
		public DragDropInformation GetLatestDragAndDropInformation(DragEventArgs e)
		{
			return moduleListItemsControl.GetLatestDragAndDropInformation(e);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Is called when the control resizes
		/// </summary>
		/********************************************************************/
		protected override void OnResize(EventArgs e)
		{
			FixScrollbar();

			base.OnResize(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a mouse wheel message arrives
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// Navigate the mouse wheel message to the scrollbar control
			User32.SendMessageW(moduleListScrollBar.Handle, WM.MOUSEWHEEL, (e.Delta * SystemInformation.MouseWheelScrollLines) << 16, IntPtr.Zero);

			base.OnMouseWheel(e);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			moduleListItemsControl.Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ListItemControl_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			OnMouseDoubleClick(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ListItemControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			OnKeyPress(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ListItemControl_DragDrop(object sender, DragEventArgs e)
		{
			OnDragDrop(e);
		}
		#endregion

		#region Internal methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void OnSelectedIndexChanged()
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged(this, EventArgs.Empty);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate scrollbar values
		/// </summary>
		/********************************************************************/
		private void FixScrollbar()
		{
			if (Items != null)
			{
				int itemsVisible = Height / ModuleListItemsControl.ItemHeight;

				moduleListScrollBar.Maximum = Math.Max(Items.Count - 1 - 1, 0);
				moduleListScrollBar.LargeChange = Math.Max(itemsVisible - 1, 1);
				moduleListScrollBar.Enabled = itemsVisible < Items.Count;

				int maxValue = moduleListScrollBar.Maximum - moduleListScrollBar.LargeChange + 1;
				if (moduleListScrollBar.Value > maxValue)
					moduleListScrollBar.Value = maxValue;
			}
		}
		#endregion
	}
}

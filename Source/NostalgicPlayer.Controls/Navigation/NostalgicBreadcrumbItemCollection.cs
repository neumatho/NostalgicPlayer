/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Polycode.NostalgicPlayer.Controls.Navigation
{
	/// <summary>
	/// Holds all the items shown in a NostalgicBreadcrumb control
	/// </summary>
	public class NostalgicBreadcrumbItemCollection : Collection<NostalgicBreadcrumbItem>
	{
		private readonly NostalgicBreadcrumb owner;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal NostalgicBreadcrumbItemCollection(NostalgicBreadcrumb owner)
		{
			this.owner = owner;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new item with the given text and add it to the end
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumbItem Add(string text)
		{
			NostalgicBreadcrumbItem item = new NostalgicBreadcrumbItem(text);
			Add(item);

			return item;
		}



		/********************************************************************/
		/// <summary>
		/// Add all the given items to the end. The control is only updated
		/// once when all items has been added
		/// </summary>
		/********************************************************************/
		public void AddRange(IEnumerable<NostalgicBreadcrumbItem> items)
		{
			ArgumentNullException.ThrowIfNull(items);

			foreach (NostalgicBreadcrumbItem item in items)
			{
				ArgumentNullException.ThrowIfNull(item);

				item.Owner = owner;
				Items.Add(item);
			}

			owner.ItemsChanged();
		}



		/********************************************************************/
		/// <summary>
		/// Remove all items after the given index, so the item at the index
		/// becomes the last one. The control is only updated once when all
		/// items has been removed
		/// </summary>
		/********************************************************************/
		public void RemoveAfter(int index)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

			if (index == (Count - 1))
				return;

			for (int i = (Count - 1); i > index; i--)
			{
				Items[i].Owner = null;
				Items.RemoveAt(i);
			}

			owner.ItemsChanged();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void InsertItem(int index, NostalgicBreadcrumbItem item)
		{
			ArgumentNullException.ThrowIfNull(item);

			item.Owner = owner;
			base.InsertItem(index, item);

			owner.ItemsChanged();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void SetItem(int index, NostalgicBreadcrumbItem item)
		{
			ArgumentNullException.ThrowIfNull(item);

			this[index].Owner = null;

			item.Owner = owner;
			base.SetItem(index, item);

			owner.ItemsChanged();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void RemoveItem(int index)
		{
			this[index].Owner = null;
			base.RemoveItem(index);

			owner.ItemsChanged();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void ClearItems()
		{
			foreach (NostalgicBreadcrumbItem item in Items)
				item.Owner = null;

			base.ClearItems();

			owner.ItemsChanged();
		}
		#endregion
	}
}

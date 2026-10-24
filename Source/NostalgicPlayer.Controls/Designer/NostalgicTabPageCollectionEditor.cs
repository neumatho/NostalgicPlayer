/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel.Design;
using Polycode.NostalgicPlayer.Controls.Containers;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// Collection editor for the NostalgicTab.Pages property. It creates
	/// NostalgicTabPage instances and works directly on the typed Pages
	/// collection, which adds/removes the pages in the tab control's
	/// Controls collection and nothing else
	/// </summary>
	internal class NostalgicTabPageCollectionEditor : CollectionEditor
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTabPageCollectionEditor(Type type) : base(type)
		{
		}



		/********************************************************************/
		/// <summary>
		/// The collection holds NostalgicTabPage items
		/// </summary>
		/********************************************************************/
		protected override Type CreateCollectionItemType()
		{
			return typeof(NostalgicTabPage);
		}



		/********************************************************************/
		/// <summary>
		/// The "Add" button only creates NostalgicTabPage items
		/// </summary>
		/********************************************************************/
		protected override Type[] CreateNewItemTypes()
		{
			return [ typeof(NostalgicTabPage) ];
		}



		/********************************************************************/
		/// <summary>
		/// Read the current pages. The Pages collection is a typed wrapper
		/// that does not implement the non-generic IList the base editor
		/// expects, so we provide the items ourselves
		/// </summary>
		/********************************************************************/
		protected override object[] GetItems(object editValue)
		{
			if (editValue is NostalgicTab.NostalgicTabPageCollection pages)
			{
				object[] items = new object[pages.Count];

				for (int i = 0; i < pages.Count; i++)
					items[i] = pages[i];

				return items;
			}

			return base.GetItems(editValue);
		}



		/********************************************************************/
		/// <summary>
		/// Write the pages back. Going through the typed wrapper means the
		/// pages end up only in the control's Controls collection. Pages the
		/// user removed are destroyed separately by the collection form
		/// </summary>
		/********************************************************************/
		protected override object SetItems(object editValue, object[] value)
		{
			if (editValue is NostalgicTab.NostalgicTabPageCollection pages)
			{
				pages.Clear();

				foreach (object item in value)
				{
					if (item is NostalgicTabPage page)
						pages.Add(page);
				}

				return editValue;
			}

			return base.SetItems(editValue, value);
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Shows a list of Audius items, such as tracks, playlists, etc.
	/// </summary>
	public partial class AudiusListControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListControl()
		{
			InitializeComponent();
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Initialize the list with new items
		/// </summary>
		/********************************************************************/
		public void SetItems(IEnumerable<AudiusListItem> items)
		{
			// First remove and dispose all existing items
			foreach (Control ctrl in flowLayoutPanel.Controls)
				ctrl.Dispose();

			flowLayoutPanel.Controls.Clear();

			// Then add the new items
			foreach (AudiusListItem item in items)
			{
				AudiusListItemControl ctrl = new AudiusListItemControl(item);
				ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;

				flowLayoutPanel.Controls.Add(ctrl);
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the flow layout resizes
		/// </summary>
		/********************************************************************/
		private void FlowLayout_Resize(object sender, EventArgs e)
		{
			foreach (Control ctrl in flowLayoutPanel.Controls)
				ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the flow layout scrolls
		/// </summary>
		/********************************************************************/
		private void FlowLayout_Scroll(object sender, ScrollEventArgs e)
		{
			UpdateVisibleItems();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update visible items if needed
		/// </summary>
		/********************************************************************/
		private void UpdateVisibleItems()
		{
			int i;

			int startYOfVisibleArea = Math.Abs(flowLayoutPanel.AutoScrollPosition.Y);
			int endYOfVisibleArea = startYOfVisibleArea + flowLayoutPanel.Height;

			int itemYPosition = 0;

			// First skip all items that are not visible (has been scrolled up)
			for (i = 0; i < flowLayoutPanel.Controls.Count; i++)
			{
				AudiusListItemControl item = (AudiusListItemControl)flowLayoutPanel.Controls[i];
				int itemHeight = item.Height + item.Margin.Vertical;

				// Stop loop if item is visible
				if (IsItemVisible(startYOfVisibleArea, endYOfVisibleArea, itemYPosition, itemYPosition + itemHeight))
					break;

				itemYPosition += itemHeight;
			}

			// Update all visible items
			for (; i < flowLayoutPanel.Controls.Count; i++)
			{
				AudiusListItemControl item = (AudiusListItemControl)flowLayoutPanel.Controls[i];
				int itemHeight = item.Height + item.Margin.Vertical;

				// If the item is not visible, then stop the loop
				if (!IsItemVisible(startYOfVisibleArea, endYOfVisibleArea, itemYPosition, itemYPosition + itemHeight))
					break;

				item.RefreshItem();

				itemYPosition += itemHeight;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will find out if an item is visible in the current view
		/// </summary>
		/********************************************************************/
		private bool IsItemVisible(int visibleStart, int visibleEnd, int itemStart, int itemEnd)
		{
			return (itemStart < visibleEnd) && (itemEnd > visibleStart);
		}
		#endregion
	}
}

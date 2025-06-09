/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Audius;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Shows a list of Audius items, such as tracks, playlists, etc.
	/// </summary>
	public partial class AudiusListControl : UserControl
	{
		private readonly PictureDownloader pictureDownloader;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListControl()
		{
			InitializeComponent();

			Disposed += AudiusListControl_Disposed;

			pictureDownloader = new PictureDownloader();
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Set load state
		/// </summary>
		/********************************************************************/
		public void SetLoading(bool loading)
		{
			loadingLabel.Visible = loading;

			if (loading)
				CenterLoadingLabel();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the list with new items
		/// </summary>
		/********************************************************************/
		public void SetItems(IEnumerable<AudiusListItem> items)
		{
			flowLayoutPanel.SuspendLayout();

			try
			{
				// First remove and dispose all existing items
				ClearItems();

				// Then add the new items
				foreach (AudiusListItem item in items)
				{
					AudiusListItemControl ctrl = new AudiusListItemControl(item);
					ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;

					flowLayoutPanel.Controls.Add(ctrl);
				}

				// Make sure the already visible items are updated
				UpdateVisibleItems();
			}
			finally
			{
				flowLayoutPanel.ResumeLayout();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the list
		/// </summary>
		/********************************************************************/
		public void ClearItems()
		{
			flowLayoutPanel.SuspendLayout();

			try
			{
				foreach (AudiusListItemControl ctrl in flowLayoutPanel.Controls)
					ctrl.Dispose();

				flowLayoutPanel.Controls.Clear();
			}
			finally
			{
				flowLayoutPanel.ResumeLayout();
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AudiusListControl_Disposed(object sender, EventArgs e)
		{
			// Items are already disposed at this point, so no need to dispose them again

			pictureDownloader.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the flow layout resizes
		/// </summary>
		/********************************************************************/
		private void FlowLayout_Resize(object sender, EventArgs e)
		{
			flowLayoutPanel.SuspendLayout();

			try
			{
				foreach (AudiusListItemControl ctrl in flowLayoutPanel.Controls)
					ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;
			}
			finally
			{
				flowLayoutPanel.ResumeLayout();
			}

			if (loadingLabel.Visible)
				CenterLoadingLabel();
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
		/// Center the loading label
		/// </summary>
		/********************************************************************/
		private void CenterLoadingLabel()
		{
			loadingLabel.Location = new Point((ClientSize.Width - loadingLabel.Width) / 2, (ClientSize.Height - loadingLabel.Height) / 2);
		}



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

				item.RefreshItem(pictureDownloader);

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

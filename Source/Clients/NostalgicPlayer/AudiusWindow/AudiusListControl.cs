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
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Shows a list of Audius items, such as tracks, playlists, etc.
	/// </summary>
	public partial class AudiusListControl : UserControl
	{
		private IMainWindowApi mainWindowApi;

		private PictureDownloader pictureDownloader;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListControl()
		{
			InitializeComponent();

			Disposed += AudiusListControl_Disposed;
			flowLayoutPanel.MouseWheel += FlowLayout_MouseWheel;
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(IMainWindowApi mainWindow)
		{
			mainWindowApi = mainWindow;

			pictureDownloader = new PictureDownloader();
		}



		/********************************************************************/
		/// <summary>
		/// Set load state
		/// </summary>
		/********************************************************************/
		public void SetLoading(bool loading)
		{
			statusLabel.Visible = loading;

			if (loading)
			{
				statusLabel.Text = Resources.IDS_AUDIUS_LOADING;
				CenterStatusLabel();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the list with new items
		/// </summary>
		/********************************************************************/
		public void SetItems(List<AudiusListItem> items)
		{
			flowLayoutPanel.SuspendLayout();

			try
			{
				// First remove and dispose all existing items
				ClearItems();

				if (items.Count == 0)
				{
					statusLabel.Visible = true;
					statusLabel.Text = Resources.IDS_AUDIUS_NO_ITEMS;
					CenterStatusLabel();
				}
				else
				{
					// Then add the new items
					foreach (AudiusListItem item in items)
					{
						AudiusListItemControl ctrl = new AudiusListItemControl(item, mainWindowApi);
						ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;

						flowLayoutPanel.Controls.Add(ctrl);
					}

					// Make sure the already visible items are updated
					UpdateVisibleItems();
				}
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
				while (flowLayoutPanel.Controls.Count > 0)
				{
					// When disposing the control, it is automatically removed from the collection
					flowLayoutPanel.Controls[0].Dispose();
				}

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

			if (statusLabel.Visible)
				CenterStatusLabel();

			UpdateVisibleItems();
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



		/********************************************************************/
		/// <summary>
		/// Is called when the flow layout scrolls via mouse wheel
		/// </summary>
		/********************************************************************/
		private void FlowLayout_MouseWheel(object sender, MouseEventArgs e)
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
		private void CenterStatusLabel()
		{
			statusLabel.PerformLayout();
			statusLabel.Location = new Point((ClientSize.Width - statusLabel.Width) / 2, (ClientSize.Height - statusLabel.Height) / 2);
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

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.External.Download;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
{
	/// <summary>
	/// Shows a list of Audius items, such as tracks, playlists, etc.
	/// </summary>
	public partial class AudiusListControl : UserControl
	{
		private IMainWindowApi mainWindowApi;
		private IPictureDownloader pictureDownloader;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusListControl()
		{
			InitializeComponent();

			flowLayoutPanel.MouseWheel += FlowLayout_MouseWheel;
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(IPictureDownloader downloader)
		{
			mainWindowApi = DependencyInjection.Container.GetInstance<IMainWindowApi>();
			pictureDownloader = downloader;
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
				statusLabel.Text = loadingText ?? Resources.IDS_AUDIUS_LOADING;
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
						IAudiusListItem listItem;

						if (item is AudiusMusicListItem musicItem)
						{
							if (musicItem.Tracks != null)
								listItem = new AudiusPlaylistListItemControl();
							else
								listItem = new AudiusMusicListItemControl();

							((IAudiusMusicListItem)listItem).PlayTracks += MusicListItem_PlayTracks;
							((IAudiusMusicListItem)listItem).AddTracks += MusicListItem_AddTracks;
						}
						else if (item is AudiusProfileListItem profileItem)
						{
							listItem = new AudiusProfileListItemControl();

							((IAudiusProfileListItem)listItem).ShowProfile += ProfileListItem_ShowInfo;
						}
						else
							continue;

						listItem.Initialize(item);

						flowLayoutPanel.Controls.Add(listItem.Control);
					}
				}
			}
			finally
			{
				flowLayoutPanel.ResumeLayout();
			}

			// This need to be done after the layout is resumed, otherwise it will not work correctly
			if (flowLayoutPanel.Controls.Count > 0)
			{
				SetItemWidths();

				// Make sure the already visible items are updated
				UpdateVisibleItems();
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



		/********************************************************************/
		/// <summary>
		/// Event called when to show user information
		/// </summary>
		/********************************************************************/
		public event ProfileEventHandler ShowProfile;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the flow layout resizes
		/// </summary>
		/********************************************************************/
		private void FlowLayout_Resize(object sender, EventArgs e)
		{
			SetItemWidths();

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



		/********************************************************************/
		/// <summary>
		/// Is called when to clear the list and start playing the given
		/// track
		/// </summary>
		/********************************************************************/
		private void MusicListItem_PlayTracks(object sender, TrackEventArgs e)
		{
			ModuleListItem[] listItems = e.Items.Select(CreateModuleListItem).ToArray();

			mainWindowApi.AddItemsToModuleList(listItems, true);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to add the given track to the list
		/// </summary>
		/********************************************************************/
		private void MusicListItem_AddTracks(object sender, TrackEventArgs e)
		{
			ModuleListItem[] listItems = e.Items.Select(CreateModuleListItem).ToArray();

			mainWindowApi.AddItemsToModuleList(listItems, false);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when to show the profile information
		/// </summary>
		/********************************************************************/
		private void ProfileListItem_ShowInfo(object sender, ProfileEventArgs e)
		{
			// Just call the next event handler
			if (ShowProfile != null)
				ShowProfile(sender, e);
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
		/// Set width of all items
		/// </summary>
		/********************************************************************/
		private void SetItemWidths()
		{
			flowLayoutPanel.SuspendLayout();

			try
			{
				foreach (Control ctrl in flowLayoutPanel.Controls)
					ctrl.Width = flowLayoutPanel.ClientSize.Width - ctrl.Margin.Horizontal;
			}
			finally
			{
				flowLayoutPanel.ResumeLayout();
			}
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
				IAudiusListItem item = (IAudiusListItem)flowLayoutPanel.Controls[i];
				int itemHeight = item.Control.Height + item.Control.Margin.Vertical;

				// Stop loop if item is visible
				if (IsItemVisible(startYOfVisibleArea, endYOfVisibleArea, itemYPosition, itemYPosition + itemHeight))
					break;

				itemYPosition += itemHeight;
			}

			// Update all visible items
			for (; i < flowLayoutPanel.Controls.Count; i++)
			{
				IAudiusListItem item = (IAudiusListItem)flowLayoutPanel.Controls[i];
				int itemHeight = item.Control.Height + item.Control.Margin.Vertical;

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



		/********************************************************************/
		/// <summary>
		/// Will create a module item from the given Audius item
		/// </summary>
		/********************************************************************/
		private ModuleListItem CreateModuleListItem(AudiusMusicListItem item)
		{
			return new ModuleListItem(new AudiusModuleListItem(item.Title, item.ItemId))
			{
				Duration = item.Duration
			};
		}
		#endregion
	}
}

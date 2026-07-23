/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows
{
	/// <summary>
	/// Defines an Explorer command and how its top-level menu item is shown
	/// </summary>
	internal readonly struct ExplorerCommandDefinition
	{
		public Guid ClassId { get; }
		public bool RenderTopLevelIcon { get; }

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ExplorerCommandDefinition(Guid classId, bool renderTopLevelIcon)
		{
			ClassId = classId;
			RenderTopLevelIcon = renderTopLevelIcon;
		}
	}

	/// <summary>
	/// Renders and invokes an Explorer command in a ToolStrip menu
	/// </summary>
	internal sealed class ExplorerCommandToolStripMenu : IDisposable
	{
		private readonly ExplorerCommandDefinition definition;
		private readonly Action<Exception> invocationErrorHandler;

		private ExplorerCommandMenu explorerCommandMenu;

		public KryptonContextMenuItem MenuItem { get; }

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ExplorerCommandToolStripMenu(ExplorerCommandDefinition definition, Action<Exception> invocationErrorHandler)
		{
			this.definition = definition;
			this.invocationErrorHandler = invocationErrorHandler;

			MenuItem = new KryptonContextMenuItem
			{
				Visible = false
			};

			MenuItem.Click += MenuItem_Click;
		}



		/********************************************************************/
		/// <summary>
		/// Rebuild the menu for the given selected files
		/// </summary>
		/********************************************************************/
		public void Update(IntPtr ownerWindow, IReadOnlyList<string> fileNames)
		{
			Clear();

			try
			{
				explorerCommandMenu = ExplorerCommandMenu.TryCreate(definition.ClassId, ownerWindow, fileNames);
				ExplorerCommandMenu.Entry root = explorerCommandMenu?.Root;

				if ((root == null) || !root.Visible || string.IsNullOrEmpty(root.Text))
				{
					Clear();
					return;
				}

				MenuItem.Text = root.Text;
				MenuItem.Enabled = root.Enabled;
				MenuItem.Checked = root.Checked;

				if (definition.RenderTopLevelIcon)
					MenuItem.Image = root.TakeImage();

				AddMenuItems(MenuItem, root.Children);

				MenuItem.Visible = true;
			}
			catch
			{
				Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Hide the menu and release the current Explorer command
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			MenuItem.Visible = false;

			while (MenuItem.Items.Count > 0)
			{
				KryptonContextMenuItemBase item = MenuItem.Items[0];
				MenuItem.Items.RemoveAt(0);
				item.Dispose();
			}

			MenuItem.Image?.Dispose();
			MenuItem.Image = null;

			explorerCommandMenu?.Dispose();
			explorerCommandMenu = null;
		}



		/********************************************************************/
		/// <summary>
		/// Release the menu and current Explorer command
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			Clear();
			MenuItem.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Invoke a top-level command that has no subcommands
		/// </summary>
		/********************************************************************/
		private void MenuItem_Click(object sender, EventArgs e)
		{
			ExplorerCommandMenu.Entry root = explorerCommandMenu?.Root;
			if ((root != null) && (root.Children.Count == 0))
				Invoke(root);
		}



		/********************************************************************/
		/// <summary>
		/// Add Explorer command entries to a ToolStrip menu
		/// </summary>
		/********************************************************************/
		private void AddMenuItems(KryptonContextMenuItem menuItems, IReadOnlyList<ExplorerCommandMenu.Entry> commands)
		{
			KryptonContextMenuItems submenuCollection = new KryptonContextMenuItems();

			foreach (ExplorerCommandMenu.Entry command in commands)
			{
				if (!command.Visible)
					continue;

				if (command.SeparatorBefore)
					AddSeparator(submenuCollection);

				if (command.IsSeparator)
					AddSeparator(submenuCollection);
				else
				{
					KryptonContextMenuItem menuItem = new KryptonContextMenuItem(command.Text)
					{
						Image = command.TakeImage(),
						Enabled = command.Enabled,
						Checked = command.Checked
					};
					menuItem.Click += (sender, e) => Invoke(command);

					submenuCollection.Items.Add(menuItem);
				}

				if (command.SeparatorAfter)
					AddSeparator(submenuCollection);
			}

			if (submenuCollection.Items.Count > 0)
				menuItems.Items.Add(submenuCollection);
		}



		/********************************************************************/
		/// <summary>
		/// Add a separator unless the menu already ends with one
		/// </summary>
		/********************************************************************/
		private static void AddSeparator(KryptonContextMenuItems menuItems)
		{
			if ((menuItems.Items.Count > 0) && (menuItems.Items[menuItems.Items.Count - 1] is not KryptonContextMenuSeparator))
				menuItems.Items.Add(new KryptonContextMenuSeparator());
		}



		/********************************************************************/
		/// <summary>
		/// Invoke an Explorer command and report any error
		/// </summary>
		/********************************************************************/
		private void Invoke(ExplorerCommandMenu.Entry command)
		{
			try
			{
				command.Invoke();
			}
			catch (Exception ex)
			{
				invocationErrorHandler?.Invoke(ex);
			}
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// This part of the main window holds everything related to Module Library window
	/// </summary>
	public partial class MainWindowForm
	{
		private ModLibraryWindowForm modLibraryWindow = null;

		/********************************************************************/
		/// <summary>
		/// User selected the Module Library menu item
		/// </summary>
		/********************************************************************/
		private void Menu_Window_ModLibrary_Click(object sender, EventArgs e)
		{
			if (IsModLibraryWindowOpen())
			{
				if (modLibraryWindow.WindowState == FormWindowState.Minimized)
					modLibraryWindow.WindowState = FormWindowState.Normal;

				modLibraryWindow.Activate();
			}
			else
			{
				modLibraryWindow = new ModLibraryWindowForm(this, optionSettings);
				modLibraryWindow.Disposed += (o, args) => { modLibraryWindow = null; };
				modLibraryWindow.Show();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find a module in the module list by file path
		/// </summary>
		/********************************************************************/
		public ModuleListItem FindModuleInList(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return null;

			foreach (ModuleListItem item in moduleListControl.Items)
			{
				if (string.Equals(item.ListItem.Source, filePath, StringComparison.OrdinalIgnoreCase))
					return item;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Select and play the given module list item
		/// </summary>
		/********************************************************************/
		public void SelectAndPlayModule(ModuleListItem listItem)
		{
			if (listItem == null)
				return;

			int index = moduleListControl.Items.IndexOf(listItem);
			if (index != -1)
			{
				moduleListControl.SelectedIndex = index;
				moduleListControl.Focus();
				LoadAndPlayModule(listItem);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add files to module list from ModLibrary with play immediately option
		/// </summary>
		/********************************************************************/
		public void AddFilesToModuleList(string[] files, bool playImmediately)
		{
			AddFilesToList(files, -1, false);

			if (playImmediately && (files.Length > 0))
			{
				// Find and play the first added module
				ModuleListItem item = FindModuleInList(files[0]);
				if (item != null)
					SelectAndPlayModule(item);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if Module Library window is open
		/// </summary>
		/********************************************************************/
		private bool IsModLibraryWindowOpen()
		{
			return (modLibraryWindow != null) && !modLibraryWindow.IsDisposed;
		}
	}
}

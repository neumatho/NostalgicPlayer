/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Library.Interfaces;
using Polycode.NostalgicPlayer.Logic.Playlists;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Adapter that delegates IMainWindowApi calls to the MainWindowForm
	/// </summary>
	public class MainWindowApiAdapter : IMainWindowApi
	{
		private MainWindowForm mainWindowForm;

		/********************************************************************/
		/// <summary>
		/// Initialize the adapter with the main window form
		/// </summary>
		/********************************************************************/
		public void Initialize(MainWindowForm mainWindowForm)
		{
			this.mainWindowForm = mainWindowForm;
		}

		#region IMainWindowApi implementation
		/********************************************************************/
		/// <summary>
		/// Return the form of the main window
		/// </summary>
		/********************************************************************/
		public Form Form => mainWindowForm;



		/********************************************************************/
		/// <summary>
		/// Return the extra channel implementation
		/// </summary>
		/********************************************************************/
		public IExtraChannels ExtraChannelsImplementation => mainWindowForm;



		/********************************************************************/
		/// <summary>
		/// Open the help window if not already open
		/// </summary>
		/********************************************************************/
		public void OpenHelpWindow(string newUrl)
		{
			mainWindowForm.OpenHelpWindow(newUrl);
		}



		/********************************************************************/
		/// <summary>
		/// Will show a question to the user
		/// </summary>
		/********************************************************************/
		public bool ShowQuestion(string question)
		{
			return mainWindowForm.ShowQuestion(question);
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		public void ShowSimpleErrorMessage(string message)
		{
			mainWindowForm.ShowSimpleErrorMessage(message);
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		public void ShowSimpleErrorMessage(IWin32Window owner, string message)
		{
			mainWindowForm.ShowSimpleErrorMessage(owner, message);
		}



		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		/********************************************************************/
		public void ShowErrorMessage(string message, ModuleListItem listItem)
		{
			mainWindowForm.ShowErrorMessage(message, listItem);
		}



		/********************************************************************/
		/// <summary>
		/// Will add all the given files to the module list
		/// </summary>
		/********************************************************************/
		public void AddFilesToModuleList(string[] files, int startIndex = -1, bool checkForList = false)
		{
			mainWindowForm.AddFilesToModuleList(files, startIndex, checkForList);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the given module list items to the module list
		/// </summary>
		/********************************************************************/
		public void AddItemsToModuleList(ModuleListItem[] items, bool clearAndPlay)
		{
			mainWindowForm.AddItemsToModuleList(items, clearAndPlay);
		}



		/********************************************************************/
		/// <summary>
		/// Will replace the given item with the new list of items
		/// </summary>
		/********************************************************************/
		public void ReplaceItemInModuleList(ModuleListItem listItem, List<ModuleListItem> newItems)
		{
			mainWindowForm.ReplaceItemInModuleList(listItem, newItems);
		}



		/********************************************************************/
		/// <summary>
		/// Will remove all the items in the given list from the module list
		/// </summary>
		/********************************************************************/
		public void RemoveItemsFromModuleList(List<ModuleListItem> items)
		{
			mainWindowForm.RemoveItemsFromModuleList(items);
		}



		/********************************************************************/
		/// <summary>
		/// Will update all the items in the given list on the module list
		/// </summary>
		/********************************************************************/
		public void UpdateModuleList(List<ModuleListItemUpdateInfo> items)
		{
			mainWindowForm.UpdateModuleList(items);
		}



		/********************************************************************/
		/// <summary>
		/// Return some information about the current playing file
		/// </summary>
		/********************************************************************/
		public PlaylistFileInfo GetFileInfo()
		{
			return mainWindowForm.GetFileInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the current playing module and free it
		/// </summary>
		/********************************************************************/
		public void StopModule()
		{
			mainWindowForm.StopModule();
		}



		/********************************************************************/
		/// <summary>
		/// Add a single agent to the menu if needed
		/// </summary>
		/********************************************************************/
		public void AddAgentToMenu(AgentInfo agentInfo)
		{
			mainWindowForm.AddAgentToMenu(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Remove a single agent from the menu if needed
		/// </summary>
		/********************************************************************/
		public void RemoveAgentFromMenu(AgentInfo agentInfo)
		{
			mainWindowForm.RemoveAgentFromMenu(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will open/activate a settings window for the given agent
		/// </summary>
		/********************************************************************/
		public void OpenAgentSettingsWindow(AgentInfo agentInfo)
		{
			mainWindowForm.OpenAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will close a settings window for the given agent
		/// </summary>
		/********************************************************************/
		public void CloseAgentSettingsWindow(AgentInfo agentInfo)
		{
			mainWindowForm.CloseAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will open/activate a display window for the given agent
		/// </summary>
		/********************************************************************/
		public void OpenAgentDisplayWindow(AgentInfo agentInfo)
		{
			mainWindowForm.OpenAgentDisplayWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will close a display window for the given agent
		/// </summary>
		/********************************************************************/
		public void CloseAgentDisplayWindow(AgentInfo agentInfo)
		{
			mainWindowForm.CloseAgentDisplayWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable user interface settings
		/// </summary>
		/********************************************************************/
		public void EnableUserInterfaceSettings()
		{
			mainWindowForm.EnableUserInterfaceSettings();
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Defines the public API to the main window
	/// </summary>
	public interface IMainWindowApi
	{
		/// <summary>
		/// Return the form of the main window
		/// </summary>
		Form Form { get; }

		/// <summary>
		/// Return the extra channel implementation
		/// </summary>
		IExtraChannels ExtraChannelsImplementation { get; }

		/// <summary>
		/// Open the help window if not already open
		/// </summary>
		void OpenHelpWindow(string newUrl);

		/// <summary>
		/// Will show a question to the user
		/// </summary>
		bool ShowQuestion(string question);

		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		void ShowSimpleErrorMessage(string message);

		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		void ShowSimpleErrorMessage(IWin32Window owner, string message);

		/// <summary>
		/// Will show an error message to the user with options
		/// </summary>
		void ShowErrorMessage(string message, ModuleListItem listItem);

		/// <summary>
		/// Will add all the given files to the module list
		/// </summary>
		void AddFilesToModuleList(string[] files, int startIndex = -1, bool checkForList = false);

		/// <summary>
		/// Will add the given module list items to the module list
		/// </summary>
		void AddItemsToModuleList(ModuleListItem[] items, bool clearAndPlay);

		/// <summary>
		/// Will replace the given item with the new list of items
		/// </summary>
		void ReplaceItemInModuleList(ModuleListItem listItem, List<ModuleListItem> newItems);

		/// <summary>
		/// Will remove all the items in the given list from the module list
		/// </summary>
		void RemoveItemsFromModuleList(List<ModuleListItem> items);

		/// <summary>
		/// Will update all the items in the given list on the module list
		/// </summary>
		void UpdateModuleList(List<ModuleListItemUpdateInfo> items);

		/// <summary>
		/// Return some information about the current playing file
		/// </summary>
		MultiFileInfo GetFileInfo();

		/// <summary>
		/// Will stop the current playing module and free it
		/// </summary>
		void StopModule();

		/// <summary>
		/// Add a single agent to the menu if needed
		/// </summary>
		void AddAgentToMenu(AgentInfo agentInfo);

		/// <summary>
		/// Remove a single agent from the menu if needed
		/// </summary>
		void RemoveAgentFromMenu(AgentInfo agentInfo);

		/// <summary>
		/// Will open/activate a settings window for the given agent
		/// </summary>
		void OpenAgentSettingsWindow(AgentInfo agentInfo);

		/// <summary>
		/// Will close a settings window for the given agent
		/// </summary>
		void CloseAgentSettingsWindow(AgentInfo agentInfo);

		/// <summary>
		/// Will open/activate a display window for the given agent
		/// </summary>
		void OpenAgentDisplayWindow(AgentInfo agentInfo);

		/// <summary>
		/// Will close a display window for the given agent
		/// </summary>
		void CloseAgentDisplayWindow(AgentInfo agentInfo);

		/// <summary>
		/// Enable/disable user interface settings
		/// </summary>
		void EnableUserInterfaceSettings();
	}
}

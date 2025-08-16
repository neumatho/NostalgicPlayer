/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// All module items need to implement this interface
	/// </summary>
	public interface IModuleListItem
	{
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Return the source to the item, e.g. a file path or a URL
		/// </summary>
		string Source { get; }

		/// <summary>
		/// Create the loader to use to load this item
		/// </summary>
		LoaderBase CreateLoader(Manager agentManager);
	}
}

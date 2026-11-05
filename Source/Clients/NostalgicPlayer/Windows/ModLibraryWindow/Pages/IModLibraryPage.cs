/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages
{
	/// <summary>
	/// All ModLibrary pages must implement this interface
	/// </summary>
	public interface IModLibraryPage
	{
		/// <summary>
		/// Initialize the page
		/// </summary>
		void Initialize(IModLibraryWindowApi modLibraryWindow, ModLibraryWindowSettings settings);

		/// <summary>
		/// Cleanup used resources
		/// </summary>
		void Cleanup(ModLibraryWindowSettings settings);

		/// <summary>
		/// Will load the page with data
		/// </summary>
		void LoadPage(string breadcrumbPath, string searchText, SearchMode searchMode);

		/// <summary>
		/// Will reload the page with data
		/// </summary>
		void ReloadPage();

		/// <summary>
		/// Do a new search and update the list
		/// </summary>
		void Search(string searchText, SearchMode searchMode, bool force = false);

		/// <summary>
		/// Set the play immediately state
		/// </summary>
		void SetPlayImmediately(bool enabled);
	}
}

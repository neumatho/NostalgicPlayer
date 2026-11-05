/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Manages favorites for the ModLibrary
	/// </summary>
	public interface IModLibraryFavorites
	{
		/// <summary>
		/// Load favorites from file
		/// </summary>
		void Load();

		/// <summary>
		/// Save favorites to file (only if changed)
		/// </summary>
		void Save();

		/// <summary>
		/// Check if a path is a favorite
		/// </summary>
		bool IsFavorite(string fullPath);

		/// <summary>
		/// Toggle favorite status for a path
		/// </summary>
		bool Toggle(string fullPath);
	}
}

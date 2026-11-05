/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Different helper methods for ModLibrary
	/// </summary>
	public interface IModLibraryHelper
	{
		/// <summary>
		/// Set new directories
		/// </summary>
		void SetDirectories(string cacheDir, string modulesDir);

		/// <summary>
		/// Return the current cache directory
		/// </summary>
		string CacheDirectory { get; }

		/// <summary>
		/// Return the current modules directory
		/// </summary>
		string ModulesDirectory { get; }

		/// <summary>
		/// Check if a file should be included (filters out smpl.* files)
		/// </summary>
		bool ShouldIncludeFile(string nameWithPath);

		/// <summary>
		/// Show error message
		/// </summary>
		void ShowErrorMessage(Exception ex, ModLibraryListControl listControl, IModLibraryWindowApi modLibraryWindowApi);
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Different helper methods for ModLibrary
	/// </summary>
	internal class ModLibraryHelper : IModLibraryHelper
	{
		private readonly IMainWindowApi mainWindowApi;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryHelper(IMainWindowApi mainWindowApi)
		{
			this.mainWindowApi = mainWindowApi;
		}



		/********************************************************************/
		/// <summary>
		/// Set new directories
		/// </summary>
		/********************************************************************/
		public void SetDirectories(string cacheDir, string modulesDir)
		{
			CacheDirectory = cacheDir;
			ModulesDirectory = modulesDir;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current cache directory
		/// </summary>
		/********************************************************************/
		public string CacheDirectory
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current modules directory
		/// </summary>
		/********************************************************************/
		public string ModulesDirectory
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Check if a file should be included (filters out smpl.* files)
		/// </summary>
		/********************************************************************/
		public bool ShouldIncludeFile(string nameWithPath)
		{
			// Skip sample files (smpl.*)
			string fileName = nameWithPath.Substring(nameWithPath.LastIndexOf('/') + 1);
			return !fileName.StartsWith("smpl.", StringComparison.OrdinalIgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Show error message
		/// </summary>
		/********************************************************************/
		public void ShowErrorMessage(Exception ex, ModLibraryListControl listControl, IModLibraryWindowApi modLibraryWindowApi)
		{
			string message = ex is TimeoutException ? Resources.IDS_MODLIBRARY_ERR_TIMEOUT : ex.Message;

			modLibraryWindowApi.Form.BeginInvoke(() =>
			{
				listControl.SetLoading(false);
				listControl.ClearItems();

				mainWindowApi.ShowSimpleErrorMessage(message, modLibraryWindowApi.Form);
			});
		}
	}
}

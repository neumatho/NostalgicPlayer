/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.IO;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows
{
	/// <summary>
	/// Helper class for Windows shell operations
	/// </summary>
	internal static class WindowsShellHelper
	{
		/********************************************************************/
		/// <summary>
		/// Open File Explorer and select the given file
		/// </summary>
		/********************************************************************/
		public static void ShowInFileExplorer(string fileName)
		{
			Process.Start("explorer.exe", $"/select,\"{fileName}\"");
		}



		/********************************************************************/
		/// <summary>
		/// Show the Windows properties dialog for the given file
		/// </summary>
		/********************************************************************/
		public static void ShowProperties(string fileName)
		{
			string directoryName = Path.GetDirectoryName(fileName);
			string fileNameOnly = Path.GetFileName(fileName);

			Shell32.Shell shell = new Shell32.Shell();
			Shell32.Folder folder = shell.NameSpace(directoryName);
			Shell32.FolderItem folderItem = folder?.ParseName(fileNameOnly);

			if (folderItem == null)
				throw new FileNotFoundException(null, fileName);

			folderItem.InvokeVerb("properties");
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// This class holds a list item pointing to a file inside an archive
	/// </summary>
	public class ArchiveFileModuleListItem : IModuleListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveFileModuleListItem(string fullArchivePath)
		{
			Source = fullArchivePath;
		}

		#region IModuleListItem implementation
		/********************************************************************/
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		/********************************************************************/
		public string DisplayName => Path.GetFileName(ArchivePath.GetEntryName(Source));



		/********************************************************************/
		/// <summary>
		/// Return the full path to the file
		/// </summary>
		/********************************************************************/
		public string Source
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Create the loader to use to load this item
		/// </summary>
		/********************************************************************/
		public LoaderBase CreateLoader(Manager agentManager)
		{
			return new Loader(agentManager);
		}
		#endregion
	}
}

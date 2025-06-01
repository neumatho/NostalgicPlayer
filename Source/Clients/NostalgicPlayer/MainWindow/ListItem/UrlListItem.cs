/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// This class holds a list item pointing to an URL
	/// </summary>
	public class UrlListItem : IStreamListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public UrlListItem(string displayName, string url)
		{
			DisplayName = displayName;
			Source = url;
		}

		#region IModuleListItem implementation
		/********************************************************************/
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		/********************************************************************/
		public string DisplayName
		{
			get;
		}



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
			return new StreamLoader(agentManager);
		}
		#endregion
	}
}

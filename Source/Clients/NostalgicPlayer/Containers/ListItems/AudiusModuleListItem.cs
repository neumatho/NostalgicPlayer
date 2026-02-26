/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.ListItems
{
	/// <summary>
	/// This class holds a list item pointing to an Audius track
	/// </summary>
	public class AudiusModuleListItem : IStreamModuleListItem
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusModuleListItem(string displayName, string trackId)
		{
			DisplayName = displayName;
			Source = trackId;
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
		public LoaderBase CreateLoader()
		{
			return new AudiusLoader();
		}
		#endregion
	}
}

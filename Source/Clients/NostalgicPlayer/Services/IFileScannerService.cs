/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Class that scans added files to find extra information
	/// </summary>
	public interface IFileScannerService
	{
		/// <summary>
		/// Will tell the scanner to scan the given range of items
		/// </summary>
		void ScanItems(IEnumerable<ModuleListItem> items);

		/// <summary>
		/// Will clear the current query and stop ongoing scanning
		/// </summary>
		void ClearQueue();
	}
}

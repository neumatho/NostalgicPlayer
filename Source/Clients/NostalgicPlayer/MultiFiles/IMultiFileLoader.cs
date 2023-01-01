/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles
{
	/// <summary>
	/// This interface is implemented by classes that can handle
	/// different kind of files that contains other files, e.g.
	/// module lists and archives
	/// </summary>
	public interface IMultiFileLoader
	{
		/// <summary>
		/// Will load a list from the given file
		/// </summary>
		IEnumerable<MultiFileInfo> LoadList(string directory, Stream stream);
	}
}

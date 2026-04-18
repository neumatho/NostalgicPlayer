/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Loaders.FileLoaders
{
	/// <summary>
	/// Factory to create file loaders
	/// </summary>
	public interface IFileLoaderFactory
	{
		/// <summary>
		/// Will parse the file name path and return the loader to use
		/// </summary>
		ILoader GetFileLoader(string fileName);
	}
}

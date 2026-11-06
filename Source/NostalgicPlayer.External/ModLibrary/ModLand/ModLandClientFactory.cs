/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.ModLibrary.Interfaces;
using Polycode.NostalgicPlayer.External.ModLibrary.ModLand.Clients;

namespace Polycode.NostalgicPlayer.External.ModLibrary.ModLand
{
	/// <summary>
	/// Creates the different clients
	/// </summary>
	internal class ModLandClientFactory : IModLandClientFactory
	{
		/********************************************************************/
		/// <summary>
		/// Return a database downloader client
		/// </summary>
		/********************************************************************/
		public IDatabaseDownloaderClient GetDatabaseDownloaderClient()
		{
			return new DatabaseDownloaderClient();
		}



		/********************************************************************/
		/// <summary>
		/// Return a module downloader client
		/// </summary>
		/********************************************************************/
		public IModuleDownloaderClient GetModuleDownloaderClient()
		{
			return new ModuleDownloaderClient();
		}
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.External.Audius;
using Polycode.NostalgicPlayer.External.Download;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Players;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Loader
{
	/// <summary>
	/// Special loader factory for Audius loader
	/// </summary>
	public class AudiusLoaderFactory : IAudiusLoaderFactory, IDisposable
	{
		private readonly IAgentManager agentManager;
		private readonly IPlayerFactory playerFactory;
		private readonly IAudiusClientFactory clientFactory;
		private readonly IPictureDownloader pictureDownloader;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusLoaderFactory(IAgentManager agentManager, IPlayerFactory playerFactory, IAudiusClientFactory audiusClientFactory, IPictureDownloaderFactory pictureDownloaderFactory)
		{
			this.agentManager = agentManager;
			this.playerFactory = playerFactory;
			clientFactory = audiusClientFactory;

			pictureDownloader = pictureDownloaderFactory.Create();
			pictureDownloader.SetMaxNumberInCache(5);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose the picture downloader
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			pictureDownloader.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Create specific loader for Audius
		/// </summary>
		/********************************************************************/
		public AudiusLoader CreateAudiusLoader()
		{
			return new AudiusLoader(agentManager, playerFactory, clientFactory, pictureDownloader);
		}
	}
}

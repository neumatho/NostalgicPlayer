/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Audius;
using Polycode.NostalgicPlayer.Audius.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	/// <summary>
	/// Loader class that helps start an Audius streaming
	/// </summary>
	public class AudiusLoader : StreamLoader
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudiusLoader(Manager agentManager) : base(agentManager)
		{
		}

		#region LoaderBase overrides
		/********************************************************************/
		/// <summary>
		/// Will try to find a player that understand the source and then
		/// load it into memory or prepare it
		/// </summary>
		/********************************************************************/
		public override bool Load(string source, out string errorMessage)
		{
			AudiusApi audiusApi = new AudiusApi();

			ITrackClient trackClient = audiusApi.GetTrackClient();
			Uri trackUrl = trackClient.GetStreamingUrl(source);

			return base.Load(trackUrl.AbsoluteUri, out errorMessage);
		}
		#endregion
	}
}

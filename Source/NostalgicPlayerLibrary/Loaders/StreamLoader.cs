/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
{
	/// <summary>
	/// Loader class that helps start a streaming
	/// </summary>
	public class StreamLoader : LoaderBase
	{
		private readonly Manager agentManager;

		private Uri uri;
		private IStreamerAgent streamerAgent;

		private string format;
		private string formatDescription;
		private string playerName;
		private string playerDescription;

		private HttpClient httpClient;
		private HttpResponseMessage responseMessage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StreamLoader(Manager agentManager)
		{
			this.agentManager = agentManager;

			Player = null;
			PlayerAgentInfo = null;
			streamerAgent = null;
			uri = null;
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
			errorMessage = string.Empty;

			uri = new Uri(source);

			httpClient = new HttpClient();

			responseMessage = Task.Run(() => httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)).Result;
			if (responseMessage.StatusCode != HttpStatusCode.OK)
			{
				errorMessage = string.Format(Resources.IDS_ERR_GET_STREAM_HEADERS, uri.AbsoluteUri, (int)responseMessage.StatusCode);
				Unload();

				return false;
			}

			var mimeType = responseMessage.Content.Headers.ContentType?.MediaType;

			bool result = FindStreamer(mimeType, out errorMessage);
			if (!result)
			{
				// Set error message if not already set
				if (string.IsNullOrEmpty(errorMessage))
					errorMessage = string.Format(Resources.IDS_ERR_UNKNOWN_STREAM, mimeType, uri.AbsoluteUri);

				Unload();

				return false;
			}

			PrepareStream();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will unload the started stream and free it from memory
		/// </summary>
		/********************************************************************/
		public override void Unload()
		{
			Stream?.Dispose();
			Stream = null;

			responseMessage?.Dispose();
			responseMessage = null;

			httpClient?.Dispose();
			httpClient = null;

			PlayerAgentInfo = null;
			streamerAgent = null;

			Player = null;

			uri = null;

			format = string.Empty;
			formatDescription = string.Empty;
			playerName = string.Empty;
			playerDescription = string.Empty;
		}
		#endregion

		#region LoaderInfoBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the source (file name or url) of the module loaded
		/// </summary>
		/********************************************************************/
		public override string Source => uri.AbsoluteUri;



		/********************************************************************/
		/// <summary>
		/// Return the agent player instance
		/// </summary>
		/********************************************************************/
		internal override IAgentWorker WorkerAgent => streamerAgent;



		/********************************************************************/
		/// <summary>
		/// Return the format loaded
		/// </summary>
		/********************************************************************/
		internal override string Format => format;



		/********************************************************************/
		/// <summary>
		/// Return the format description
		/// </summary>
		/********************************************************************/
		internal override string FormatDescription => formatDescription;



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		internal override string PlayerName => playerName;



		/********************************************************************/
		/// <summary>
		/// Return the description of the player
		/// </summary>
		/********************************************************************/
		internal override string PlayerDescription => playerDescription;



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		internal override long ModuleSize => 0;



		/********************************************************************/
		/// <summary>
		/// Return the size of the module crunched. Is zero if not crunched.
		/// If -1, it means the crunched length is unknown
		/// </summary>
		/********************************************************************/
		internal override long CrunchedSize => 0;



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		internal override string[] DecruncherAlgorithms => null;
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the stream to use when streaming
		/// </summary>
		/********************************************************************/
		internal StreamingStream Stream
		{
			get; private set;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to find a streamer that understand the mimetype given
		/// </summary>
		/********************************************************************/
		private bool FindStreamer(string mimeType, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				if (!string.IsNullOrEmpty(mimeType))
				{
					// Create a list with all the streamers
					List<(IStreamerAgent streamer, AgentInfo agentInfo)> agents = new List<(IStreamerAgent, AgentInfo)>();

					foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.Streamers))
					{
						// Is the streamer enabled?
						if (agentInfo.Enabled)
						{
							// Create an instance of the streamer
							if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IStreamerAgent streamer)
								agents.Add((streamer, agentInfo));
						}
					}

					foreach ((IStreamerAgent streamer, AgentInfo agentInfo) agent in agents)
					{
						AgentInfo agentInfo = agent.agentInfo;
						IStreamerAgent streamer = null;

						// Check the mime type
						if (agent.streamer.PlayableMimeTypes.Any(x => x.Equals(mimeType, StringComparison.CurrentCultureIgnoreCase)))
							streamer = agent.streamer;

						if (streamer != null)
						{
							// We found the right streamer
							PlayerAgentInfo = agentInfo;
							streamerAgent = streamer;

							return true;
						}
					}

					// No streamer was found
				}
			}
			catch(Exception ex)
			{
				// Build an error message
				errorMessage = string.Format(Resources.IDS_ERR_STREAM, uri.AbsoluteUri, ex.HResult.ToString("X8"), ex.Message);
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare the stream
		/// </summary>
		/********************************************************************/
		private void PrepareStream()
		{
			playerName = PlayerAgentInfo.AgentName;
			playerDescription = PlayerAgentInfo.AgentDescription;

			if (string.IsNullOrEmpty(PlayerAgentInfo.TypeName))
			{
				format = PlayerAgentInfo.AgentName;
				formatDescription = PlayerAgentInfo.AgentDescription;
			}
			else
			{
				format = PlayerAgentInfo.TypeName;
				formatDescription = PlayerAgentInfo.TypeDescription;
			}

			Stream = new StreamingStream(responseMessage.Content.ReadAsStream(), false);

			Player = new StreamingPlayer(agentManager);
		}
		#endregion
	}
}

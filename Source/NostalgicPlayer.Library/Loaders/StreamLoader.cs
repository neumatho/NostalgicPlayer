/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Players;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Loader class that helps start a streaming
	/// </summary>
	public class StreamLoader : LoaderBase
	{
		private const int InitialTimeout = 10000;
		private const int MaxNumberOfRedirects = 10;

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

			httpClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = false
			});

			for (int i = 0; i < MaxNumberOfRedirects; i++)
			{
				responseMessage = Task.Run(() =>
				{
					using (CancellationTokenSource cancellationToken = new CancellationTokenSource(InitialTimeout))
					{
						return httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token);
					}
				}).Result;

				if (responseMessage.StatusCode is HttpStatusCode.OK or HttpStatusCode.PartialContent)
					break;

				// If we get a redirect, we will follow it but only once
				if ((responseMessage.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
				    && responseMessage.Headers.Location != null)
				{
					uri = responseMessage.Headers.Location.IsAbsoluteUri ? responseMessage.Headers.Location : new Uri(uri, responseMessage.Headers.Location);

					responseMessage.Dispose();
					responseMessage = null;
				}
				else
				{
					errorMessage = string.Format(Resources.IDS_ERR_GET_STREAM_HEADERS, uri.AbsoluteUri, (int)responseMessage.StatusCode);
					Unload();

					return false;
				}
			}

			if (responseMessage == null)
			{
				errorMessage = string.Format(Resources.IDS_ERR_TOO_MANY_REDIRECTS, uri.AbsoluteUri);
				Unload();

				return false;
			}

			string mimeType = responseMessage.Content.Headers.ContentType?.MediaType;

			Stream = new StreamingStream(responseMessage.Content.ReadAsStream(), GetSeeker());

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



		/********************************************************************/
		/// <summary>
		/// Return a seeking implementation of the stream supports it
		/// </summary>
		/********************************************************************/
		protected virtual IStreamSeek GetSeeker()
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Close the connection and open a new connection, setting the
		/// stream position to the given position
		/// </summary>
		/********************************************************************/
		protected Stream ReconnectToPosition(long newPosition)
		{
			responseMessage?.Dispose();
			responseMessage = null;

			responseMessage = Task.Run(() =>
			{
				using (CancellationTokenSource cancellationToken = new CancellationTokenSource(InitialTimeout))
				{
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
					request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(newPosition, null);

					return httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token);
				}
			}).Result;

			if (responseMessage.StatusCode is HttpStatusCode.OK or HttpStatusCode.PartialContent)
				return responseMessage.Content.ReadAsStream();

			return null;
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

					Stream.RecordMode = true;

					foreach ((IStreamerAgent streamer, AgentInfo agentInfo) agent in agents)
					{
						AgentInfo agentInfo = agent.agentInfo;

						// Check the mime type
						if (agent.streamer.PlayableMimeTypes.Any(x => x.Equals(mimeType, StringComparison.CurrentCultureIgnoreCase)))
						{
							if (agent.streamer.Identify(Stream) == AgentResult.Ok)
							{
								// We found the right streamer
								PlayerAgentInfo = agentInfo;
								streamerAgent = agent.streamer;

								Stream.Rewind();
								Stream.RecordMode = false;

								return true;
							}
						}

						Stream.Flush();
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

			Player = new StreamingPlayer(agentManager);
		}
		#endregion
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Agent;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Helper class to decrunch a file using single file decrunchers
	/// </summary>
	internal class SingleFileDecruncher
	{
		private readonly Manager manager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleFileDecruncher(Manager agentManager)
		{
			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to decrunch the file if needed. Will also check if the
		/// file has been crunched multiple times
		/// </summary>
		/********************************************************************/
		public Stream DecrunchFileMultipleLevels(Stream stream)
		{
			List<string> decruncherAlgorithms = new List<string>();

			for (;;)
			{
				DecruncherStream decruncherStream = DecrunchFile(stream, decruncherAlgorithms);
				if (decruncherStream == null)
				{
					DecruncherAlgorithms = decruncherAlgorithms.Count > 0 ? decruncherAlgorithms.ToArray() : null;

					// Make sure that the stream is at the beginning
					stream.Seek(0, SeekOrigin.Begin);

					return stream;
				}

				if (decruncherStream.CanSeek)
					stream = decruncherStream;
				else
					stream = new SeekableStream(decruncherStream, false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		public string[] DecruncherAlgorithms
		{
			get; private set;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to decrunch the file if needed
		/// </summary>
		/********************************************************************/
		private DecruncherStream DecrunchFile(Stream crunchedDataStream, List<string> decruncherAlgorithms)
		{
			HashSet<Guid> agentsToSkip = new HashSet<Guid>();

			foreach (AgentInfo info in manager.GetAllAgents(Manager.AgentType.FileDecrunchers))
			{
				if (agentsToSkip.Contains(info.AgentId))
					continue;

				AgentInfo agentInfo = info;

				// Do the decruncher implement multiple format identify method?
				if (agentInfo.Agent is IAgentMultipleFormatIdentify multipleFormatIdentify)
				{
					// Since this is a multi format agent, we don't want to call the agent for
					// each format and therefore we store the ID in this list
					agentsToSkip.Add(agentInfo.AgentId);

					IdentifyFormatInfo identifyFormatInfo = multipleFormatIdentify.IdentifyFormat(crunchedDataStream);
					if (identifyFormatInfo != null)
					{
						agentInfo = manager.GetAgent(Manager.AgentType.FileDecrunchers, identifyFormatInfo.TypeId);
						if (agentInfo.Enabled)
						{
							IFileDecruncherAgent decruncher = identifyFormatInfo.Worker as IFileDecruncherAgent;
							if (decruncher != null)
							{
								decruncherAlgorithms.Add(agentInfo.TypeName);

								return decruncher.OpenStream(crunchedDataStream);
							}
						}
					}
				}
				else
				{
					// Create an instance of the decruncher
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IFileDecruncherAgent decruncher)
					{
						// Check the file
						AgentResult agentResult = decruncher.Identify(crunchedDataStream);
						if (agentResult == AgentResult.Ok)
						{
							decruncherAlgorithms.Add(agentInfo.TypeName);

							return decruncher.OpenStream(crunchedDataStream);
						}

						if (agentResult != AgentResult.Unknown)
						{
							// Some error occurred
							throw new DecruncherException(agentInfo.TypeName, "Identify() returned an error");
						}
					}
				}
			}

			return null;
		}
		#endregion
	}
}

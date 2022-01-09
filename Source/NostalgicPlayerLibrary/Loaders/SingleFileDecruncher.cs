/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
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
			for (;;)
			{
				DecruncherStream decruncherStream = DecrunchFile(stream);
				if (decruncherStream == null)
				{
					// Make sure that the stream is at the beginning
					stream.Seek(0, SeekOrigin.Begin);

					return stream;
				}

				if (decruncherStream.CanSeek)
					stream = decruncherStream;
				else
					stream = new SeekableStream(decruncherStream);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to decrunch the file if needed
		/// </summary>
		/********************************************************************/
		private DecruncherStream DecrunchFile(Stream crunchedDataStream)
		{
			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.FileDecrunchers))
			{
				// Is the decruncher enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the decruncher
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IFileDecruncherAgent decruncher)
					{
						// Check the file
						AgentResult agentResult = decruncher.Identify(crunchedDataStream);
						if (agentResult == AgentResult.Ok)
							return decruncher.OpenStream(crunchedDataStream);

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

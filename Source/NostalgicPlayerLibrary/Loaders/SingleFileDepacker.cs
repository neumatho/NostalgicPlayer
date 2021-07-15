/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
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
	/// Helper class to depack a file using single file depackers
	/// </summary>
	public class SingleFileDepacker
	{
		private readonly Manager manager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleFileDepacker(Manager agentManager)
		{
			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to depack the file if needed. Will also check if the
		/// file has been packed multiple times
		/// </summary>
		/********************************************************************/
		public Stream DepackFileMultipleLevels(Stream stream)
		{
			for (;;)
			{
				DepackerStream depackerStream = DepackFile(stream);
				if (depackerStream == null)
				{
					// Make sure that the stream is at the beginning
					stream.Seek(0, SeekOrigin.Begin);

					return stream;
				}

				if (depackerStream.CanSeek)
					stream = depackerStream;
				else
					stream = new SeekableStream(depackerStream);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to depack the file if needed
		/// </summary>
		/********************************************************************/
		private DepackerStream DepackFile(Stream packedDataStream)
		{
			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.FileDecrunchers))
			{
				// Is the depacker enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the depacker
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IFileDecruncherAgent decruncher)
					{
						// Check the file
						AgentResult agentResult = decruncher.Identify(packedDataStream);
						if (agentResult == AgentResult.Ok)
							return decruncher.OpenStream(packedDataStream);

						if (agentResult != AgentResult.Unknown)
						{
							// Some error occurred
							throw new DepackerException(agentInfo.TypeName, "Identify() returned an error");
						}
					}
				}
			}

			return null;
		}
		#endregion
	}
}

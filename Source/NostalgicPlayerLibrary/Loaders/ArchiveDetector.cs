/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
{
	/// <summary>
	/// Helper class to detect archive files and extract entries
	/// </summary>
	public class ArchiveDetector
	{
		/// <summary></summary>
		protected Manager manager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveDetector(Manager agentManager)
		{
			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Return all available extensions for archive files
		/// </summary>
		/********************************************************************/
		public string[] GetExtensions()
		{
			return GetAllArchiveAgents().SelectMany(x => x.archiveDecruncher.FileExtensions).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will check if the given file is an archive
		/// </summary>
		/********************************************************************/
		public bool IsArchive(string fullPath)
		{
			try
			{
				// Try to open the file
				using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
				{
					IArchiveDecruncherAgent decruncherAgent = FindArchiveAgent(fs, null, out Stream newStream);
					newStream.Dispose();

					return decruncherAgent != null;
				}
			}
			catch(Exception)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will return a collection with all the entries in the given
		/// archive file
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetEntries(string fullPath)
		{
			// Try to open the file
			using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
			{
				foreach (string entry in GetAllEntries(fullPath, fs))
					yield return entry;
			}
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Will try to find the archive agent that can be used on the
		/// given stream and then open the archive and return a new stream
		/// </summary>
		/********************************************************************/
		protected IArchive OpenArchive(string archiveFileName, Stream stream, out Stream newStream, out List<string> decruncherAlgorithms)
		{
			decruncherAlgorithms = new List<string>();

			IArchiveDecruncherAgent archiveDecruncher = FindArchiveAgent(stream, decruncherAlgorithms, out newStream);
			if (archiveDecruncher == null)
				throw new Exception(Resources.IDS_LOADERR_NO_ARCHIVE_DECRUNCHER);

			// Seek back to the beginning of the stream
			newStream.Seek(0, SeekOrigin.Begin);

			return archiveDecruncher.OpenArchive(archiveFileName, newStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return all enabled archive agents
		/// </summary>
		/********************************************************************/
		private IEnumerable<(AgentInfo agentInfo, IArchiveDecruncherAgent archiveDecruncher)> GetAllArchiveAgents()
		{
			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.ArchiveDecrunchers))
			{
				// Is the decruncher enabled?
				if (agentInfo.Enabled)
				{
					// Create an instance of the decruncher
					if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IArchiveDecruncherAgent archiveDecruncher)
						yield return (agentInfo, archiveDecruncher);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find the archive agent that can be used on the
		/// given stream
		/// </summary>
		/********************************************************************/
		private IArchiveDecruncherAgent FindArchiveAgent(Stream stream, List<string> decruncherAlgorithms, out Stream newStream)
		{
			SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);

			// The archive file itself can be crunched, e.g. Bzip2, so decrunch it
			newStream = decruncher.DecrunchFileMultipleLevels(stream);

			if ((decruncherAlgorithms != null) && (decruncher.DecruncherAlgorithms != null))
				decruncherAlgorithms.AddRange(decruncher.DecruncherAlgorithms);

			foreach ((AgentInfo agentInfo, IArchiveDecruncherAgent archiveDecruncher) in GetAllArchiveAgents())
			{
				// Check the file
				AgentResult agentResult = archiveDecruncher.Identify(newStream);
				if (agentResult == AgentResult.Ok)
				{
					decruncherAlgorithms?.Add(agentInfo.TypeName);

					return archiveDecruncher;
				}

				if (agentResult != AgentResult.Unknown)
				{
					// Some error occurred
					throw new DecruncherException(agentInfo.TypeName, "Identify() returned an error");
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find the archive agent that can be used on the
		/// given stream and then open the archive and return a new stream
		/// </summary>
		/********************************************************************/
		private IEnumerable<string> GetAllEntries(string fullPath, Stream archiveStream)
		{
			string archiveFileName = ArchivePath.IsArchivePath(fullPath) ? ArchivePath.GetEntryName(fullPath) : Path.GetFileName(fullPath);
			IArchive archive = OpenArchive(archiveFileName, archiveStream, out Stream newStream, out _);

			try
			{
				foreach (string entry in archive.GetEntries())
				{
					Stream entryStream = archive.OpenEntry(entry);

					try
					{
						if (!entryStream.CanSeek)
							entryStream = new SeekableStream(entryStream, false);

						IArchiveDecruncherAgent decruncherAgent = FindArchiveAgent(entryStream, null, out Stream newEntryStream);
						if (decruncherAgent != null)
						{
							try
							{
								foreach (string resultEntry in GetAllEntries($"{fullPath}|{entry}", newEntryStream))
									yield return resultEntry;
							}
							finally
							{
								newEntryStream.Dispose();
							}
						}
						else
							yield return $"{fullPath}|{entry}";
					}
					finally
					{
						entryStream.Dispose();
					}
				}
			}
			finally
			{
				newStream.Dispose();
			}
		}
		#endregion
	}
}

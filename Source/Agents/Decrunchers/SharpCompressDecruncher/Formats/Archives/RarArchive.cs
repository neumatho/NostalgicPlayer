/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers.Rar;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Archives
{
	/// <summary>
	/// Handle a Rar archive
	/// </summary>
	internal class RarArchive : IArchive
	{
		private readonly string agentName;
		private readonly SharpCompress.Archives.Rar.RarArchive archive;

		private Stream archiveStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public RarArchive(string agentName, Stream archiveStream)
		{
			this.agentName = agentName;

			archive = SharpCompress.Archives.Rar.RarArchive.Open(archiveStream);
			this.archiveStream = archiveStream;
		}

		#region IArchive implementation
		/********************************************************************/
		/// <summary>
		/// Open the entry with the name given
		/// </summary>
		/********************************************************************/
		public ArchiveStream OpenEntry(string entryName)
		{
			if (archive.IsSolid)
			{
				// Solid Rar archives is only supported trough the reader, so use that
				archiveStream.Seek(0, SeekOrigin.Begin);
				RarReader reader = RarReader.Open(archiveStream);

				while (reader.MoveToNextEntry())
				{
					if (!reader.Entry.IsDirectory && (reader.Entry.Key.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
						return new ArchiveEntryStream(reader.Entry, reader.OpenEntryStream());
				}

				throw new DecruncherException(agentName, string.Format(Resources.IDS_SCOM_ERR_ENTRY_NOT_FOUND, entryName));
			}

			RarArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Key.Equals(entryName, StringComparison.OrdinalIgnoreCase));
			if (entry == null)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_SCOM_ERR_ENTRY_NOT_FOUND, entryName));

			return new ArchiveEntryStream(entry, entry.OpenEntryStream());
		}



		/********************************************************************/
		/// <summary>
		/// Return all entries
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetEntries()
		{
			foreach (RarArchiveEntry entry in archive.Entries.Where(e => !e.IsDirectory))
				yield return entry.Key;
		}
		#endregion
	}
}

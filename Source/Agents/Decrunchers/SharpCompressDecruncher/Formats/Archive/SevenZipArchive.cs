﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using SharpCompress.Archives.SevenZip;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Archive
{
	/// <summary>
	/// Handle a 7-Zip archive
	/// </summary>
	internal class SevenZipArchive : IArchive
	{
		private readonly string agentName;
		private readonly SharpCompress.Archives.SevenZip.SevenZipArchive archive;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SevenZipArchive(string agentName, Stream archiveStream)
		{
			this.agentName = agentName;

			archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(archiveStream);
		}

		#region IArchive implementation
		/********************************************************************/
		/// <summary>
		/// Open the entry with the name given
		/// </summary>
		/********************************************************************/
		public ArchiveStream OpenEntry(string entryName)
		{
			entryName = entryName.Replace('\\', '/');

			SevenZipArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Key.Equals(entryName, StringComparison.OrdinalIgnoreCase));
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
			foreach (SevenZipArchiveEntry entry in archive.Entries.Where(e => !e.IsDirectory))
				yield return entry.Key;
		}
		#endregion
	}
}

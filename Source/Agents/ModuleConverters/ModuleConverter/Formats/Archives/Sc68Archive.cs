/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats.Streams;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Utility;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats.Archives
{
	/// <summary>
	/// Handle the SC68 archive
	/// </summary>
	internal class Sc68Archive : IArchive
	{
		private readonly string agentName;
		private readonly Stream stream;

		private readonly List<Sc68Entry> entries;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sc68Archive(string agentName, string archiveFileName, Stream archiveStream)
		{
			this.agentName = agentName;
			stream = archiveStream;

			// Read all the entries
			entries = new List<Sc68Entry>();
			ReadEntries(Path.GetFileNameWithoutExtension(archiveFileName));
		}

		#region IArchive implementation
		/********************************************************************/
		/// <summary>
		/// Open the entry with the name given
		/// </summary>
		/********************************************************************/
		public ArchiveStream OpenEntry(string entryName)
		{
			Sc68Entry entry = entries.FirstOrDefault(e => e.EntryName.Equals(entryName, StringComparison.OrdinalIgnoreCase));
			if (entry == null)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_ERR_ENTRY_NOT_FOUND, entryName));

			return new Sc68Stream(entry, stream);
		}



		/********************************************************************/
		/// <summary>
		/// Return all entries
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetEntries()
		{
			foreach (Sc68Entry entry in entries)
				yield return entry.EntryName;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read all the entries into a list
		/// </summary>
		/********************************************************************/
		private void ReadEntries(string archiveFileName)
		{
			List<Sc68DataBlockInfo> dataBlocks = Sc68Helper.FindAllModules(stream, out string _);

			for (int i = 0; i < dataBlocks.Count; i++)
			{
				Sc68DataBlockInfo dataBlockInfo = dataBlocks[i];

				Sc68Entry entry = new Sc68Entry
				{
					DataBlockInfo = dataBlockInfo,
					EntryName = $"{archiveFileName}_{i + 1}"
				};

				entries.Add(entry);
			}
		}
		#endregion
	}
}

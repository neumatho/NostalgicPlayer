/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archive
{
	/// <summary>
	/// Handle a Lha archive
	/// </summary>
	internal class LhaArchive : IArchive
	{
		public class FileEntry
		{
			public string FileName;
			public int CrunchedSize;
			public int DecrunchedSize;
			public long Position;
			public int Method;
			public ushort? Crc;
		}

		private static readonly Dictionary<string, int> methods = new Dictionary<string, int>()
		{
			{ Constants.LzHuff0_Method, Constants.LzHuff0_Method_Num },
			{ Constants.LzHuff1_Method, Constants.LzHuff1_Method_Num },
			{ Constants.LzHuff2_Method, Constants.LzHuff2_Method_Num },
			{ Constants.LzHuff3_Method, Constants.LzHuff3_Method_Num },
			{ Constants.LzHuff4_Method, Constants.LzHuff4_Method_Num },
			{ Constants.LzHuff5_Method, Constants.LzHuff5_Method_Num },
			{ Constants.LzHuff6_Method, Constants.LzHuff6_Method_Num },
			{ Constants.LzHuff7_Method, Constants.LzHuff7_Method_Num },
			{ Constants.Larc_Method, Constants.Larc_Method_Num },
			{ Constants.Larc5_Method, Constants.Larc5_Method_Num },
			{ Constants.Larc4_Method, Constants.Larc4_Method_Num },
			{ Constants.LzhDirs_Method, Constants.LzhDirs_Method_Num }
		};

		private readonly string agentName;
		private readonly Stream stream;

		private readonly List<FileEntry> entries;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LhaArchive(string agentName, Stream archiveStream)
		{
			this.agentName = agentName;
			stream = archiveStream;

			// Read all the entries
			entries = new List<FileEntry>();
			ReadEntries();
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

			FileEntry entry = entries.FirstOrDefault(e => e.FileName.Equals(entryName, StringComparison.OrdinalIgnoreCase));
			if (entry == null)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_ENTRY_NOT_FOUND, entryName));

			return new LhaStream(agentName, entry, stream);
		}



		/********************************************************************/
		/// <summary>
		/// Return all entries
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetEntries()
		{
			foreach (FileEntry fileEntry in entries)
				yield return fileEntry.FileName;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read all the entries into a list
		/// </summary>
		/********************************************************************/
		private void ReadEntries()
		{
			LhaCore lha = new LhaCore(agentName, stream);

			// Make sure we are at the start of the file
			stream.Seek(0, SeekOrigin.Begin);

			// Get all the headers
			while (lha.GetHeader(out LzHeader hdr))
			{
				// Find compression method
				int method = methods[Encoding.ASCII.GetString(hdr.Method)];

				// Don't add directory entries
				if (((hdr.UnixMode & Constants.Unix_File_TypeMask) == Constants.Unix_File_Regular) && (method != Constants.LzhDirs_Method_Num))
				{
					// Create the file entry
					FileEntry fileEntry = new FileEntry
					{
						FileName = hdr.DecodedName,
						CrunchedSize = hdr.PackedSize,
						DecrunchedSize = hdr.OriginalSize,
						Position = stream.Position,
						Method = method,
						Crc = hdr.HasCrc ? hdr.Crc : null
					};

					entries.Add(fileEntry);
				}

				// Skip crunched data
				stream.Seek(hdr.PackedSize, SeekOrigin.Current);
			}
		}
		#endregion
	}
}

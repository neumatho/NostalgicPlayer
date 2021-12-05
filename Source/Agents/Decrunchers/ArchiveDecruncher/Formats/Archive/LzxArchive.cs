/******************************************************************************/
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
using System.IO.Hashing;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archive
{
	/// <summary>
	/// Handle a Lzx archive
	/// </summary>
	internal class LzxArchive : IArchive
	{
		public class SkipInfo
		{
			public int DecrunchedSize;
			public byte[] DataCrc;
		}

		public class FileEntry
		{
			public string FileName;
			public int CrunchedSize;
			public int DecrunchedSize;
			public bool Merged;
			public byte PackMode;
			public long Position;
			public SkipInfo[] DecrunchedBytesToSkip;
			public byte[] DataCrc;
		}

		private readonly string agentName;
		private readonly Stream stream;

		private readonly List<FileEntry> entries;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LzxArchive(string agentName, Stream archiveStream)
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

			return new LzxStream(agentName, entry, stream);
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
			// Seek to right after the info header
			stream.Seek(10, SeekOrigin.Begin);

			byte[] crc = new byte[4];
			byte[] archiveHeader = new byte[31];
			byte[] headerFileName = new byte[256];
			byte[] headerComment = new byte[256];

			Encoding encoder = EncoderCollection.Amiga;

			Crc32 crc32 = new Crc32();

			List<FileEntry> entriesToFix = new List<FileEntry>();
			List<SkipInfo> skipInfo = new List<SkipInfo>();

			for (;;)
			{
				int actual = stream.Read(archiveHeader, 0, 31);
				if (actual == 0)
					break;

				if (actual != 31)
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Archive_Header"));

				// Reset crc calculation
				crc32.Reset();

				crc[0] = archiveHeader[26];
				crc[1] = archiveHeader[27];
				crc[2] = archiveHeader[28];
				crc[3] = archiveHeader[29];

				// Must set the field to 0 before calculating the crc
				archiveHeader[26] = 0;
				archiveHeader[27] = 0;
				archiveHeader[28] = 0;
				archiveHeader[29] = 0;

				crc32.Append(archiveHeader);

				// Get the file name
				int temp = archiveHeader[30];	// File name length
				actual = stream.Read(headerFileName, 0, temp);
				if (actual != temp)
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Header_FileName"));

				headerFileName[temp] = 0;
				crc32.Append(headerFileName.AsSpan(0, temp));

				// Get the comment
				temp = archiveHeader[14];		// Comment length
				actual = stream.Read(headerComment, 0, temp);
				if (actual != temp)
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Header_Comment"));

				headerComment[temp] = 0;
				crc32.Append(headerComment.AsSpan(0, temp));

				byte[] sum = crc32.GetCurrentHash();
				if ((sum[0] != crc[0]) || (sum[1] != crc[1]) || (sum[2] != crc[2]) || (sum[3] != crc[3]))
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Archive_Header"));

				// Get the size of unpacked data
				int decrunchedSize = (archiveHeader[5] << 24) | (archiveHeader[4] << 16) | (archiveHeader[3] << 8) | archiveHeader[2];

				// Get the size of packed data
				int crunchedSize = (archiveHeader[9] << 24) | (archiveHeader[8] << 16) | (archiveHeader[7] << 8) | archiveHeader[6];

				// Create the file entry
				FileEntry fileEntry = new FileEntry
				{
					FileName = encoder.GetString(headerFileName),
					CrunchedSize = crunchedSize,
					DecrunchedSize = decrunchedSize,
					PackMode = archiveHeader[11],
					Position = stream.Position,
					DataCrc = archiveHeader.AsSpan(22, 4).ToArray()
				};

				if (skipInfo.Count > 0)
					fileEntry.DecrunchedBytesToSkip = skipInfo.ToArray();

				// Add entry to list
				entries.Add(fileEntry);

				if (crunchedSize == 0)
				{
					// The entry is a merged entry, so mark it as one that need to be fixed later
					fileEntry.Merged = true;

					entriesToFix.Add(fileEntry);

					skipInfo.Add(new SkipInfo
					{
						DecrunchedSize = decrunchedSize,
						DataCrc = fileEntry.DataCrc
					});
				}
				else
				{
					// Now got a bunch of packed data. Fix any previous entries that is merged
					// into this block
					if (entriesToFix.Count > 0)
					{
						fileEntry.Merged = true;

						foreach (FileEntry fe in entriesToFix)
						{
							fe.Position = stream.Position;
							fe.CrunchedSize = crunchedSize;
						}
					}
					else
						fileEntry.Merged = false;

					entriesToFix.Clear();
					skipInfo.Clear();

					// Seek past the packed data
					stream.Seek(crunchedSize, SeekOrigin.Current);
				}
			}
		}
		#endregion
	}
}

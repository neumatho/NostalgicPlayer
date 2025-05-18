/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archives
{
	/// <summary>
	/// Handle an ArcFS archive
	/// </summary>
	internal class ArcFsArchive : IArchive
	{
		private const int HeaderSize = 96;
		private const int EntrySize = 36;

		private const int MaxOutput = 512 << 20;

		/// <summary></summary>
		public enum EntryMethod
		{
			/// <summary></summary>
			EndOfDirectory = 0x00,
			/// <summary></summary>
			ObjectHasBeenDeleted = 0x01,
			/// <summary></summary>
			Stored = 0x82,
			/// <summary></summary>
			Packed = 0x83,
			/// <summary></summary>
			Crunched = 0x84,
			/// <summary></summary>
			Compressed = 0xff
		}

		private class ArcFsHeader
		{
			public uint EntriesLength;
			public uint DataOffset;
			public uint MinReadVersion;
			public uint MinWriteVersion;
			public uint FormatVersion;
		}

		/// <summary></summary>
		public class ArcFsEntry
		{
			public EntryMethod Method;
			public string FileName;
			public int DecrunchedSize;
			public int CrunchedSize;

			public ushort Crc16;
			public byte CrunchingBits;
			public bool IsDirectory;
			public uint ValueOffset;
			public int CrunchedDataOffset;
		}

		private readonly string agentName;
		private readonly Stream stream;
		private readonly Encoding encoder;

		private readonly List<ArcFsEntry> entries;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArcFsArchive(string agentName, Stream archiveStream)
		{
			this.agentName = agentName;
			stream = archiveStream;
			encoder = EncoderCollection.Acorn;

			// Read all the entries
			entries = new List<ArcFsEntry>();
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
			ArcFsEntry entry = entries.FirstOrDefault(e => e.FileName.Equals(entryName, StringComparison.OrdinalIgnoreCase));
			if (entry == null)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_ENTRY_NOT_FOUND, entryName));

			return new ArcFsStream(agentName, entry, stream);
		}



		/********************************************************************/
		/// <summary>
		/// Return all entries
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetEntries()
		{
			foreach (ArcFsEntry entry in entries)
				yield return entry.FileName;
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
			ArcFsHeader header = ReadHeader();
			if (header == null)
				throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_INVALID_HEADER);

			string path = string.Empty;

			for (int i = 0; i < header.EntriesLength; i += EntrySize)
			{
				ArcFsEntry entry = ReadEntry();
				if (entry == null)
					throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_INVALID_HEADER);

				if (entry.Method == EntryMethod.ObjectHasBeenDeleted)
					continue;

				if (entry.Method == EntryMethod.EndOfDirectory)
				{
					path = Path.GetDirectoryName(path);
					path ??= string.Empty;
					continue;
				}

				if (entry.IsDirectory)
				{
					path = Path.Combine(path, entry.FileName);
					continue;
				}

				if (entry.Method == EntryMethod.Stored)
					entry.CrunchedSize = entry.DecrunchedSize;

				// Ignore junk offset/size
				if (entry.ValueOffset >= (stream.Length - header.DataOffset))
					continue;

				uint offset = header.DataOffset + entry.ValueOffset;
				if (entry.CrunchedSize > (stream.Length - offset))
					continue;

				// Ignore sizes over the allowed limit
				if (entry.DecrunchedSize > MaxOutput)
					continue;

				entry.FileName = Path.Combine(path, entry.FileName);
				entry.CrunchedDataOffset = (int)offset;

				entries.Add(entry);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read and check the archive header
		/// </summary>
		/********************************************************************/
		private ArcFsHeader ReadHeader()
		{
			ArcFsHeader header = new ArcFsHeader();

			stream.Seek(8, SeekOrigin.Begin);

			using (ReaderStream rs = new ReaderStream(stream, true))
			{
				header.EntriesLength = rs.Read_L_UINT32();
				header.DataOffset = rs.Read_L_UINT32();
				header.MinReadVersion = rs.Read_L_UINT32();
				header.MinWriteVersion = rs.Read_L_UINT32();
				header.FormatVersion = rs.Read_L_UINT32();

				if (rs.EndOfStream)
					return null;

				if ((header.EntriesLength % EntrySize) != 0)
					return null;

				if ((header.DataOffset < HeaderSize) || ((header.DataOffset - HeaderSize) < header.EntriesLength))
					return null;

				// These seem to be the highest versions that exists
				if ((header.MinReadVersion > 260) || (header.MinWriteVersion > 260) || (header.FormatVersion > 0x0a))
					return null;

				if (header.DataOffset > stream.Length)
					return null;

				rs.Seek(HeaderSize - 8 - (5 * 4), SeekOrigin.Current);
			}

			return header;
		}



		/********************************************************************/
		/// <summary>
		/// Read a single entry
		/// </summary>
		/********************************************************************/
		private ArcFsEntry ReadEntry()
		{
			ArcFsEntry entry = new ArcFsEntry();

			using (ReaderStream rs = new ReaderStream(stream, true))
			{
				entry.Method = (EntryMethod)(rs.Read_UINT8());

				entry.FileName = rs.ReadString(encoder, 11);
				entry.DecrunchedSize = rs.Read_L_INT32();

				rs.Seek(9, SeekOrigin.Current);

				entry.CrunchingBits = rs.Read_UINT8();
				entry.Crc16 = rs.Read_L_UINT16();
				entry.CrunchedSize = rs.Read_L_INT32();
				entry.ValueOffset = rs.Read_L_UINT32();
				entry.IsDirectory = entry.ValueOffset >> 31 != 0;
				entry.ValueOffset &= 0x7fffffff;

				if (rs.EndOfStream)
					return null;
			}

			return entry;
		}
		#endregion
	}
}

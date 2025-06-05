/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archives;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams
{
	/// <summary>
	/// Handle decrunching of a single entry
	/// </summary>
	internal class LhaStream : ArchiveStream
	{
		private readonly string agentName;
		private readonly LhaArchive.FileEntry entry;

		private readonly LhaCore lha;

		private int decrunchedBytesLeft;
		private bool stored;			// Indicate if the data are just stored or compressed

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LhaStream(string agentName, LhaArchive.FileEntry entry, ReaderStream archiveStream) : base(archiveStream, true)
		{
			this.agentName = agentName;
			this.entry = entry;

			// Seek to the right position
			wrapperStream.Seek(entry.Position, SeekOrigin.Begin);

			lha = new LhaCore(agentName, archiveStream);

			// Initialize status variables
			stored = false;
			decrunchedBytesLeft = entry.DecrunchedSize;

			ushort dicBit = 0;

			switch (entry.Method)
			{
				case Constants.Larc_Method_Num:
				{
					dicBit = 11;
					break;
				}

				case Constants.LzHuff1_Method_Num:
				case Constants.LzHuff4_Method_Num:
				case Constants.Larc5_Method_Num:
				{
					dicBit = 12;
					break;
				}

				case Constants.LzHuff6_Method_Num:
				case Constants.LzHuff7_Method_Num:
				{
					dicBit = (ushort)((entry.Method - Constants.LzHuff6_Method_Num) + 15);
					break;
				}

				case Constants.LzHuff0_Method_Num:
				case Constants.Larc4_Method_Num:
				{
					stored = true;
					break;
				}

				default:
				{
					dicBit = 13;
					break;
				}
			}

			if (!stored)
				lha.InitializeDecoder(dicBit, entry.DecrunchedSize, entry.CrunchedSize, entry.Method);
		}

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		public override int GetDecrunchedLength()
		{
			return entry.DecrunchedSize;
		}
		#endregion

		#region ArchiveStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the crunched data
		/// </summary>
		/********************************************************************/
		public override int GetCrunchedLength()
		{
			return entry.CrunchedSize;
		}
		#endregion

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (decrunchedBytesLeft == 0)
			{
				EndOfStream = true;
				return 0;
			}

			int totalRead = 0;
			int countLeft = count;

			while (countLeft > 0)
			{
				int copied;

				// Time to read some more data
				if (stored)
				{
					copied = wrapperStream.Read(buffer, offset, Math.Min(countLeft, decrunchedBytesLeft));
					lha.crc.CalcCrc(buffer, offset, (uint)copied);
				}
				else
					copied = lha.Decode(countLeft, buffer, offset);

				if (copied == 0)
					break;

				countLeft -= copied;
				totalRead += copied;
				offset += copied;
				decrunchedBytesLeft -= copied;
			}

			if (decrunchedBytesLeft == 0)
			{
				if (entry.Crc.HasValue)
				{
					if (lha.crc.Crc != entry.Crc.Value)
						throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Entry_Data"));
				}

				EndOfStream = true;
			}
			else
				EndOfStream = false;

			return totalRead;
		}
		#endregion
	}
}

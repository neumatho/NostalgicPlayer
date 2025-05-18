/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archives;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams
{
	/// <summary>
	/// Handle decrunching of a single entry
	/// </summary>
	internal class ArcFsStream : ArchiveStream
	{
		private readonly string agentName;
		private readonly ArcFsArchive.ArcFsEntry entry;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArcFsStream(string agentName, ArcFsArchive.ArcFsEntry entry, Stream archiveStream) : base(new MemoryStream(), false)
		{
			this.agentName = agentName;
			this.entry = entry;

			DecrunchData(archiveStream);
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
			return (int)entry.CrunchedSize;
		}
		#endregion

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => wrapperStream.Position;

			set => wrapperStream.Position = value;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			return wrapperStream.Seek(offset, origin);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		private void DecrunchData(Stream archiveStream)
		{
			byte[] crunchedData = new byte[entry.CrunchedSize];
			byte[] decrunchedData = new byte[entry.DecrunchedSize];

			archiveStream.Seek(entry.CrunchedDataOffset, SeekOrigin.Begin);
			archiveStream.ReadExactly(crunchedData, 0, entry.CrunchedSize);

			string err = Arc.Arc.Unpack(decrunchedData, decrunchedData.Length, crunchedData, crunchedData.Length, (int)entry.Method, entry.CrunchingBits);
			if (err != null)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_DECRUNCH_FAILED, err));

			// ArcFS CRC may sometimes just be 0, in which case, ignore it
			if (entry.Crc16 != 0)
			{
				Crc16 crc = new Crc16();
				crc.CalcCrc(decrunchedData, 0, (uint)decrunchedData.Length);

				if (crc.Crc != entry.Crc16)
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Entry data"));
			}

			wrapperStream.Write(decrunchedData);
			wrapperStream.Seek(0, SeekOrigin.Begin);
		}
		#endregion
	}
}

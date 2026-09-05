/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal class FileDataStdStreamSeekable : FileDataSeekableBuffered
	{
		private readonly Stream stream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDataStdStreamSeekable(Stream s) : base(FileDataStdStream.GetLength(s))
		{
			stream = s;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override Stream GetStream()
		{
			return stream;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override byte_span InternalReadBuffered(size_t pos, byte_span dst)
		{
			size_t currentPos = (size_t)stream.Position;

			if (pos != currentPos)
				stream.Seek((long)pos, SeekOrigin.Begin);

			size_t bytesToRead = dst.Size();
			size_t bytesRead = 0;

			while (bytesToRead > 0)
			{
				c_int bytesChunkToRead = c_int.CreateSaturating(bytesToRead);
				c_int bytesChunkRead = stream.Read((dst.Data() + bytesRead).AsSpan(bytesChunkToRead));

				bytesRead += (size_t)bytesChunkRead;
				bytesToRead -= (size_t)bytesChunkRead;

				// Unlike std::istream::read, Stream.Read may return fewer
				// bytes than asked for without being at the end, so only a
				// read of nothing means that the end was reached
				if (bytesChunkRead == 0)
					break;
			}

			return dst.First(bytesRead);
		}
	}
}

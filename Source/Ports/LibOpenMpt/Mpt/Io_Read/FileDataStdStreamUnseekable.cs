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
	internal class FileDataStdStreamUnseekable : FileDataUnseekable
	{
		private readonly Stream stream;
		private bool eof;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDataStdStreamUnseekable(Stream s)
		{
			stream = s;
			eof = false;
		}

		#region Overrides
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
		protected override bool InternalEof()
		{
			return eof;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override byte_span InternalReadUnseekable(byte_span dst)
		{
			size_t bytesToRead = dst.Size();
			size_t bytesRead = 0;

			while (bytesToRead > 0)
			{
				c_int bytesChunkToRead = c_int.CreateSaturating(bytesToRead);
				c_int bytesChunkRead = stream.Read(dst.Data().AsSpan((c_int)bytesRead, bytesChunkToRead));

				// Unlike std::istream::read, Stream.Read may return fewer
				// bytes than asked for without being at the end, so only a
				// read of nothing means that the end was reached
				if (bytesChunkRead == 0)
				{
					eof = true;
					break;
				}

				bytesRead += (size_t)bytesChunkRead;
				bytesToRead -= (size_t)bytesChunkRead;
			}

			return dst.First(bytesRead);
		}
		#endregion
	}
}

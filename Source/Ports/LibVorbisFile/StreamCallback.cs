/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibVorbisFile
{
	/// <summary>
	/// Callback methods which uses a stream
	/// </summary>
	internal static class StreamCallback
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Read(CPointer<byte> ptr, size_t size, size_t nmemb, object datasource)
		{
			// Check for empty read
			int toRead = (int)(size * nmemb);
			if (toRead <= 0)
				return 0;

			Stream stream = (Stream)datasource;

			int ret = stream.Read(ptr.Buffer, ptr.Offset, toRead);

			return (size_t)ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Seek(object datasource, ogg_int64_t offset, SeekOrigin whence)
		{
			Stream stream = (Stream)datasource;
			if (!stream.CanSeek)
				return -1;

			// Translate the seek to an absolute one
			long pos;

			if (whence == SeekOrigin.Current)
				pos = stream.Position;
			else if (whence == SeekOrigin.End)
				pos = stream.Length;
			else if (whence == SeekOrigin.Begin)
				pos = 0;
			else
				return -1;

			// Check for errors or overflow
			if ((pos < 0) || (offset < -pos) || (offset > (long.MaxValue - pos)))
				return -1;

			pos += offset;

			stream.Seek(pos, SeekOrigin.Begin);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Close(object datasource)
		{
			Stream stream = (Stream)datasource;
			stream.Close();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long Tell(object datasource)
		{
			Stream stream = (Stream)datasource;

			return (c_long)stream.Position;
		}
	}
}

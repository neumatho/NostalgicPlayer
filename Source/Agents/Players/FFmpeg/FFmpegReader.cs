/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.FFmpeg
{
	/// <summary>
	/// Holds the needed reader methods needed by ffmpeg
	/// </summary>
	internal static class FFmpegReader
	{
		/********************************************************************/
		/// <summary>
		/// Read from the stream
		/// </summary>
		/********************************************************************/
		public static int ReadFromStream(object opaque, CPointer<byte> buf, int buf_Size)
		{
			Stream stream = (Stream)opaque;

			int read = stream.Read(buf.AsSpan(buf_Size));

			return read == 0 ? Error.EOF : read;
		}



		/********************************************************************/
		/// <summary>
		/// Seek around in the stream
		/// </summary>
		/********************************************************************/
		public static long SeekInStream(object opaque, long offset, AvSeek whence)
		{
			Stream stream = (Stream)opaque;

			switch (whence)
			{
				case AvSeek.Size:
					return stream.Length;

				case AvSeek.Set:
				{
					stream.Seek(offset, SeekOrigin.Begin);
					break;
				}

				case AvSeek.Cur:
				{
					stream.Seek(offset, SeekOrigin.Current);
					break;
				}

				case AvSeek.End:
				{
					stream.Seek(offset, SeekOrigin.End);
					break;
				}
			}

			return 0;
		}
	}
}

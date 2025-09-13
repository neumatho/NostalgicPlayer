/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Stream helper methods
	/// </summary>
	public static class StreamHelper
	{
		/********************************************************************/
		/// <summary>
		/// Copy length bytes from one stream to another. If getting out of
		/// data from the source stream, fill up the rest with zeros
		/// </summary>
		/********************************************************************/
		public static void CopyDataForceLength(Stream source, Stream destination, int length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);

				if (len < 1024)
					Array.Clear(buf, len, 1024 - len);

				destination.Write(buf, 0, 1024);

				length -= 1024;
			}

			if (length > 0)
			{
				int len = source.Read(buf, 0, length);
				if (len < length)
					Array.Clear(buf, len, length - len);

				destination.Write(buf, 0, length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Copy length bytes from one stream to another. If getting out of
		/// data from the source stream, the copying is stopped
		/// </summary>
		/********************************************************************/
		public static void CopyData(Stream source, Stream destination, int length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);
				if (len == 0)
					return;

				destination.Write(buf, 0, len);

				length -= len;
			}

			while (length > 0)
			{
				int len = source.Read(buf, 0, length);
				if (len == 0)
					return;

				destination.Write(buf, 0, len);

				length -= len;
			}
		}
	}
}

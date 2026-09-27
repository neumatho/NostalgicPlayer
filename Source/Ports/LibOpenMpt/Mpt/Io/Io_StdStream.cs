/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io
{
	/// <summary>
	/// 
	/// </summary>
	internal class FileOperationsStdStream
	{
		private readonly Stream f;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileOperationsStdStream(Stream f_)
		{
			f = f_;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsReadSeekable()
		{
			return f.CanSeek;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte_span2 ReadRawImpl(byte_span2 data)
		{
			size_t bytesToRead = data.Size();
			size_t bytesRead = 0;

			while (bytesToRead > 0)
			{
				c_int bytesChunkRead = f.Read(data.Slice((c_int)bytesRead, (c_int)bytesToRead));
				if (bytesChunkRead == 0)
					break;

				bytesRead += (size_t)bytesChunkRead;
				bytesToRead -= (size_t)bytesChunkRead;
			}

			return data.First(bytesRead);
		}
	}
}

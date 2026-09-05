/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal class FileDataMemory : IFileData
	{
		private readonly CPointer<c_byte> streamData;	// Pointer to memory-mapped file
		private readonly size_t streamLength;			// Size of memory-mapped file in bytes

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDataMemory()
		{
			streamData = null;
			streamLength = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDataMemory(byte_span data)
		{
			streamData = data.Data();
			streamLength = data.Size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override Stream GetStream()
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IsValid()//XX 47
		{
			return streamData.IsNotNull;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool HasPinnedView()//XX 55
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override CPointer<byte> GetRawData()// 59
		{
			return streamData;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override size_t GetLength()//XX 63
		{
			return streamLength;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span Read(size_t pos, byte_span dst)//XX 67
		{
			if (pos >= streamLength)
				return dst.First(0);

			size_t avail = Math.Min(streamLength - pos, dst.Size());
			CPointer<c_byte> src = streamData + pos;
			CMemory.copy(src, src + avail, dst.Data());

			return dst.First(avail);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span2 Read(size_t pos, byte_span2 dst)
		{
			if (pos >= streamLength)
				return dst.First(0);

			size_t avail = Math.Min(streamLength - pos, dst.Size());
			CPointer<c_byte> src = streamData + pos;
			src.AsSpan(avail).CopyTo(dst);

			return dst.First(avail);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool CanRead(size_t pos, size_t length)//XX 84
		{
			if ((pos == streamLength) && (length == 0))
				return true;

			if (pos >= streamLength)
				return false;

			return length <= (streamLength - pos);
		}
	}
}

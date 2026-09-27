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
	internal class FileDataWindow : IFileData
	{
		private readonly IFileData data;
		private readonly size_t dataOffset;
		private readonly size_t dataLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileDataWindow(IFileData src, size_t off, size_t len)
		{
			data = src;
			dataOffset = off;
			dataLength = len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override Stream GetStream()
		{
			return data.GetStream();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IsValid()
		{
			return data.IsValid();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool HasPinnedView()
		{
			return data.HasPinnedView();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override CPointer<byte> GetRawData()
		{
			return data.GetRawData() + dataOffset;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override size_t GetLength()
		{
			return dataLength;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span Read(size_t pos, byte_span dst)
		{
			if (pos >= dataLength)
				return dst.First(0);

			return data.Read(dataOffset + pos, dst.First(Math.Min(dst.Size(), dataLength - pos)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span2 Read(size_t pos, byte_span2 dst)
		{
			if (pos >= dataLength)
				return dst.First(0);

			return data.Read(dataOffset + pos, dst.First(Math.Min(dst.Size(), dataLength - pos)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool CanRead(size_t pos, size_t length)
		{
			if ((pos == dataLength) && (length == 0))
				return true;

			if (pos >= dataLength)
				return false;

			return length <= (dataLength - pos);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override size_t GetReadableLength(size_t pos, size_t length)
		{
			if (pos >= dataLength)
				return 0;

			return Math.Min(length, dataLength - pos);
		}
	}
}

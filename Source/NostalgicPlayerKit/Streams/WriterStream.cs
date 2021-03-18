/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class wraps another stream and adds some helper methods to write data
	/// </summary>
	public class WriterStream : Stream
	{
		private readonly Stream wrapperStream;

		private readonly byte[] saveBuffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public WriterStream(Stream wrapperStream)
		{
			this.wrapperStream = wrapperStream;

			saveBuffer = new byte[8];
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports reading
		/// </summary>
		/********************************************************************/
		public override bool CanRead
		{
			get
			{
				return wrapperStream.CanRead;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports writing
		/// </summary>
		/********************************************************************/
		public override bool CanWrite
		{
			get
			{
				return wrapperStream.CanWrite;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek
		{
			get
			{
				return wrapperStream.CanSeek;
			}
		}




		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length
		{
			get
			{
				return wrapperStream.Length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get
			{
				return wrapperStream.Position;
			}

			set
			{
				wrapperStream.Position = value;
			}
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



		/********************************************************************/
		/// <summary>
		/// Set new length
		/// </summary>
		/********************************************************************/
		public override void SetLength(long value)
		{
			wrapperStream.SetLength(value);
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			return wrapperStream.Read(buffer, offset, count);
		}



		/********************************************************************/
		/// <summary>
		/// Write data to the stream
		/// </summary>
		/********************************************************************/
		public override void Write(byte[] buffer, int offset, int count)
		{
			wrapperStream.Write(buffer, offset, count);
		}



		/********************************************************************/
		/// <summary>
		/// Flush buffers
		/// </summary>
		/********************************************************************/
		public override void Flush()
		{
			wrapperStream.Flush();
		}
		#endregion

		#region Helper write methods
		/********************************************************************/
		/// <summary>
		/// Write a byte (8 bit integer) to the stream
		/// </summary>
		/********************************************************************/
		public void Write_UINT8(byte data)
		{
			saveBuffer[0] = data;
			Write(saveBuffer, 0, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Write a byte (8 bit integer) to the stream
		/// </summary>
		/********************************************************************/
		public void Write_INT8(sbyte data)
		{
			saveBuffer[0] = (byte)data;

			Write(saveBuffer, 0, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 16 bit integer in little endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_L_UINT16(ushort data)
		{
			saveBuffer[0] = (byte)(data & 0xff);
			saveBuffer[1] = (byte)(data >> 8);

			Write(saveBuffer, 0, 2);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 32 bit integer in little endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_L_UINT32(uint data)
		{
			saveBuffer[0] = (byte)(data & 0xff);
			saveBuffer[1] = (byte)(data >> 8);
			saveBuffer[2] = (byte)(data >> 16);
			saveBuffer[3] = (byte)(data >> 24);

			Write(saveBuffer, 0, 4);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 64 bit integer in little endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_L_UINT64(ulong data)
		{
			saveBuffer[0] = (byte)(data & 0xff);
			saveBuffer[1] = (byte)(data >> 8);
			saveBuffer[2] = (byte)(data >> 16);
			saveBuffer[3] = (byte)(data >> 24);
			saveBuffer[4] = (byte)(data >> 32);
			saveBuffer[5] = (byte)(data >> 40);
			saveBuffer[6] = (byte)(data >> 48);
			saveBuffer[7] = (byte)(data >> 56);

			Write(saveBuffer, 0, 8);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 16 bit integer in big endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_B_UINT16(ushort data)
		{
			saveBuffer[0] = (byte)(data >> 8);
			saveBuffer[1] = (byte)(data & 0xff);

			Write(saveBuffer, 0, 2);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 32 bit integer in big endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_B_UINT32(uint data)
		{
			saveBuffer[0] = (byte)(data >> 24);
			saveBuffer[1] = (byte)(data >> 16);
			saveBuffer[2] = (byte)(data >> 8);
			saveBuffer[3] = (byte)(data & 0xff);

			Write(saveBuffer, 0, 4);
		}



		/********************************************************************/
		/// <summary>
		/// Write a 64 bit integer in big endian format to the stream
		/// </summary>
		/********************************************************************/
		public void Write_B_UINT64(ulong data)
		{
			saveBuffer[0] = (byte)(data >> 56);
			saveBuffer[1] = (byte)(data >> 48);
			saveBuffer[2] = (byte)(data >> 40);
			saveBuffer[3] = (byte)(data >> 32);
			saveBuffer[4] = (byte)(data >> 24);
			saveBuffer[5] = (byte)(data >> 16);
			saveBuffer[6] = (byte)(data >> 8);
			saveBuffer[7] = (byte)(data & 0xff);

			Write(saveBuffer, 0, 8);
		}



		/********************************************************************/
		/// <summary>
		/// Write a string in UTF-8 format
		/// </summary>
		/********************************************************************/
		public void WriteString(string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			Write_B_UINT16((ushort)bytes.Length);

			if (bytes.Length > 0)
				Write(bytes, 0, bytes.Length);
		}
		#endregion
	}
}

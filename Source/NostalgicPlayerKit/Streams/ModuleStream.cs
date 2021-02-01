/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class wraps another stream and adds some helper methods to read the data
	/// </summary>
	public class ModuleStream : Stream
	{
		private readonly Stream wrapperStream;

		private readonly byte[] loadBuffer;
		private readonly bool isLittleEndian;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream)
		{
			this.wrapperStream = wrapperStream;

			loadBuffer = new byte[4];
			isLittleEndian = BitConverter.IsLittleEndian;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			wrapperStream.Dispose();
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
				return false;
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
			int read = wrapperStream.Read(buffer, offset, count);
			if (read < count)
				EndOfStream = true;

			return read;
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public int ReadSigned(sbyte[] buffer, int offset, int count)
		{
			int read = wrapperStream.Read((byte[])(Array)buffer, offset, count);
			if (read < count)
				EndOfStream = true;

			return read;
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

		#region Helper read methods
		/********************************************************************/
		/// <summary>
		/// Indicate if end of stream has been reached
		/// </summary>
		/********************************************************************/
		public bool EndOfStream
		{
			get; private set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Reads a string of the limited byte size from the stream into
		/// the specified buffer
		/// </summary>
		/********************************************************************/
		public void ReadString(byte[] buffer, int maxLen)
		{
			int bytesRead = Read(buffer, 0, maxLen);
			buffer[bytesRead] = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a byte (8 bit integer) from the stream
		/// </summary>
		/********************************************************************/
		public byte Read_UINT8()
		{
			int read = Read(loadBuffer, 0, 1);
			if (read == 1)
				return loadBuffer[0];

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a byte (8 bit integer) from the stream
		/// </summary>
		/********************************************************************/
		public sbyte Read_INT8()
		{
			int read = Read(loadBuffer, 0, 1);
			if (read == 1)
				return (sbyte)loadBuffer[0];

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a 16 bit integer in little endian format from the stream
		/// and return it in the native host format
		/// </summary>
		/********************************************************************/
		public ushort Read_L_UINT16()
		{
			int read = Read(loadBuffer, 0, 2);
			if (read == 2)
			{
				if (!isLittleEndian)
				{
					byte tmp = loadBuffer[0];
					loadBuffer[0] = loadBuffer[1];
					loadBuffer[1] = tmp;
				}

				return BitConverter.ToUInt16(loadBuffer, 0);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a 32 bit integer in little endian format from the stream
		/// and return it in the native host format
		/// </summary>
		/********************************************************************/
		public uint Read_L_UINT32()
		{
			int read = Read(loadBuffer, 0, 4);
			if (read == 4)
			{
				if (!isLittleEndian)
				{
					byte tmp1 = loadBuffer[0];
					byte tmp2 = loadBuffer[1];
					loadBuffer[0] = loadBuffer[3];
					loadBuffer[1] = loadBuffer[2];
					loadBuffer[2] = tmp2;
					loadBuffer[3] = tmp1;
				}

				return BitConverter.ToUInt32(loadBuffer, 0);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads an array of 16 bit integers in little endian format from
		/// the stream and convert the integers to the native host format
		/// </summary>
		/********************************************************************/
		public void ReadArray_L_UINT16s(ushort[] buffer, int count)
		{
			for (int i = 0; i < count; i++)
				buffer[i] = Read_L_UINT16();
		}



		/********************************************************************/
		/// <summary>
		/// Reads an array of 32 bit integers in little endian format from
		/// the stream and convert the integers to the native host format
		/// </summary>
		/********************************************************************/
		public void ReadArray_L_UINT32s(uint[] buffer, int count)
		{
			for (int i = 0; i < count; i++)
				buffer[i] = Read_L_UINT32();
		}



		/********************************************************************/
		/// <summary>
		/// Reads a 16 bit integer in big endian format from the stream
		/// and return it in the native host format
		/// </summary>
		/********************************************************************/
		public ushort Read_B_UINT16()
		{
			int read = Read(loadBuffer, 0, 2);
			if (read == 2)
			{
				if (isLittleEndian)
				{
					byte tmp = loadBuffer[0];
					loadBuffer[0] = loadBuffer[1];
					loadBuffer[1] = tmp;
				}

				return BitConverter.ToUInt16(loadBuffer, 0);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a 32 bit integer in big endian format from the stream
		/// and return it in the native host format
		/// </summary>
		/********************************************************************/
		public uint Read_B_UINT32()
		{
			int read = Read(loadBuffer, 0, 4);
			if (read == 4)
			{
				if (isLittleEndian)
				{
					byte tmp1 = loadBuffer[0];
					byte tmp2 = loadBuffer[1];
					loadBuffer[0] = loadBuffer[3];
					loadBuffer[1] = loadBuffer[2];
					loadBuffer[2] = tmp2;
					loadBuffer[3] = tmp1;
				}

				return BitConverter.ToUInt32(loadBuffer, 0);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads an array of 16 bit integers in big endian format from
		/// the stream and convert the integers to the native host format
		/// </summary>
		/********************************************************************/
		public void ReadArray_B_UINT16s(ushort[] buffer, int count)
		{
			for (int i = 0; i < count; i++)
				buffer[i] = Read_B_UINT16();
		}



		/********************************************************************/
		/// <summary>
		/// Reads an array of 32 bit integers in big endian format from
		/// the stream and convert the integers to the native host format
		/// </summary>
		/********************************************************************/
		public void ReadArray_B_UINT32s(uint[] buffer, int count)
		{
			for (int i = 0; i < count; i++)
				buffer[i] = Read_B_UINT32();
		}
		#endregion
	}
}

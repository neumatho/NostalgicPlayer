/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Streams
{
	/// <summary>
	/// Will read the decompress music data
	/// </summary>
	internal class Mo3Stream : ReaderStream
	{
		private readonly uint suggestedReserveSize;
		private readonly uint targetSize;
		private uint totalRemain;

		private ushort data = 0;
		private int strLen = 0;		// Length of repeated string
		private int strOffset = 0;	// Offset of repeated string

		private readonly List<byte> streamCache;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mo3Stream(ReaderStream wrapperStream, uint targetSize, uint suggestedReserveSize) : base(wrapperStream, true)
		{
			this.suggestedReserveSize = suggestedReserveSize;
			this.targetSize = targetSize;
			totalRemain = targetSize;

			streamCache = new List<byte>();
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => false;



		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => targetSize;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => streamCache.Count;
			set => throw new NotSupportedException("Seek not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (EndOfStream)
				return 0;

			ReaderStream readerStream = (ReaderStream)wrapperStream;

			uint remain = Math.Min(totalRemain, (uint)count);
			int read = 0;

			if (streamCache.Count == 0)
			{
				// First byte is always read verbatim
				streamCache.Capacity = (int)Math.Min(targetSize, suggestedReserveSize);
				streamCache.Add(readerStream.Read_UINT8());

				totalRemain--;
				remain--;
				read++;
			}

			int sLen = strLen;
			if (sLen != 0)
			{
				// Previous string copy is still in progress
				uint copyLen = Math.Min((uint)sLen, remain);
				totalRemain -= copyLen;
				remain -= copyLen;
				sLen -= (int)copyLen;
				read += (int)copyLen;

				streamCache.AddRange(Enumerable.Repeat<byte>(0, (int)copyLen));
				int src = (int)(streamCache.Count - copyLen + strOffset);
				int dst = (int)(streamCache.Count - copyLen);

				do
				{
					copyLen--;
					streamCache[dst++] = streamCache[src++];
				}
				while (copyLen > 0);
			}

			ushort dat = data;
			int carry = 0;	// x86 carry (used to propagate the most significant bit from one byte to another)

			// Shift control bits until it is empty:
			// a 0 bit means literal : the next data byte is copied
			// a 1 means compressed data
			// then the next 2 bits determines what is the LZ ptr
			// ('00' same as previous, else stored in stream)
			bool ReadCtrlBit()
			{
				dat <<= 1;
				carry = dat > 0xff ? 1 : 0;
				dat &= 0xff;

				if (dat == 0)
				{
					byte nextByte = readerStream.Read_UINT8();
					if (readerStream.EndOfStream)
						return true;

					dat = nextByte;
					dat <<= 1;
					dat += 1;
					carry = dat > 0xff ? 1 : 0;
					dat &= 0xff;
				}

				return false;
			}

			// Length coded within control stream:
			// most significant bit is 1
			// then the first bit of each bits pair (noted n1),
			// until second bit is 0 (noted n0)
			void DecodeCtrlBits()
			{
				sLen++;

				do
				{
					if (ReadCtrlBit())
						break;

					sLen = (sLen << 1) + carry;

					if (ReadCtrlBit())
						break;
				}
				while (carry != 0);
			}

			while (remain != 0)
			{
				if (ReadCtrlBit())
					break;

				if (carry == 0)
				{
					// A 0 ctrl bit means 'copy', not compressed byte
					streamCache.Add(readerStream.Read_UINT8());
					read++;

					if (readerStream.EndOfStream)
						break;

					totalRemain--;
					remain--;
				}
				else
				{
					// A 1 ctrl bit means compressed bytes are following
					byte lengthAdjust = 0;	// Length adjustment
					DecodeCtrlBits();		// Read length, and if strLen > 3 (coded using more than 1 bits pair) also part of the offset value
					sLen -= 3;

					if (sLen < 0)
					{
						// Reuse same previous relative LZ ptr (strOffset is not re-computed)
						sLen++;
					}
					else
					{
						// LZ ptr in ctrl stream
						byte b = readerStream.Read_UINT8();

						if (readerStream.EndOfStream)
							break;

						strOffset = (sLen << 8) | b;	// Read less significant offset byte from stream
						sLen = 0;
						strOffset = ~strOffset;

						if (strOffset < -1280)
							lengthAdjust++;

						lengthAdjust++;		// Length is always at least 1

						if (strOffset < -32000)
							lengthAdjust++;
					}

					if ((strOffset >= 0) || (-streamCache.Count > strOffset))
						break;

					// Read the next 2 bits as part of strLen
					if (ReadCtrlBit())
						break;

					sLen = (sLen << 1) + carry;

					if (ReadCtrlBit())
						break;

					sLen = (sLen << 1) + carry;

					if (sLen == 0)
					{
						// Length does not fit in 2 bits
						DecodeCtrlBits();	// Decode length: 1 is the most significant bit,
						sLen += 2;			// then first bit of each bits pairs (noted n1), until n0
					}

					sLen += lengthAdjust;	// Length adjustment

					if ((sLen <= 0) || (totalRemain < (uint)sLen))
						break;

					// Copy previous string
					// Need to do this in two steps (allocate, then copy) as source and destination
					// may overlap (e.g. strOffset = -1, strLen = 2 repeats last character twice)
					uint copyLen = Math.Min((uint)sLen, remain);
					totalRemain -= copyLen;
					remain -= copyLen;
					sLen -= (int)copyLen;
					read += (int)copyLen;

					streamCache.AddRange(Enumerable.Repeat<byte>(0, (int)copyLen));
					int src = (int)(streamCache.Count - copyLen + strOffset);
					int dst = (int)(streamCache.Count - copyLen);

					do
					{
						copyLen--;
						streamCache[dst++] = streamCache[src++];
					}
					while (copyLen > 0);
				}
			}

			data = dat;
			strLen = sLen;

			// Premature EOF or corrupted stream?
			EndOfStream = (remain != 0) || (read < count);

			// Copy read data into the buffer
			streamCache.CopyTo(streamCache.Count - read, buffer, 0, read);

			return read;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Check if the decrunching was successfully
		/// </summary>
		/********************************************************************/
		public bool UnpackedSuccessfully => (totalRemain == 0) && !EndOfStream;
	}
}

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
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams
{
	/// <summary>
	/// Base class to all XPK streams
	/// </summary>
	internal abstract class XpkStream : DecruncherStream
	{
		protected readonly string agentName;

		private readonly uint decrunchedSize;
		private readonly uint rawSize;

		private readonly uint headerSize;
		private readonly bool longHeaders;

		private uint currentOffset = 0;
		private uint destOffset = 0;
		private bool isLast = false;

		private byte[] rawData = null;
		private int bufferIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected XpkStream(string agentName, Stream wrapperStream) : base(wrapperStream, false)
		{
			this.agentName = agentName;

			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				readerStream.Seek(4, SeekOrigin.Begin);
				decrunchedSize = readerStream.Read_B_UINT32();

				// Skip type
				readerStream.Seek(4, SeekOrigin.Current);

				rawSize = readerStream.Read_B_UINT32();

				if ((rawSize == 0) || (decrunchedSize == 0))
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				if ((rawSize > 0x1000000) || (decrunchedSize > 0x1000000))
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				readerStream.Seek(32, SeekOrigin.Begin);
				byte flags = readerStream.Read_UINT8();
				longHeaders = (flags & 1) != 0;

				if ((flags & 2) != 0)		// Needs password. We do not support that
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_NO_ENCRYPTION);

				if ((flags & 4) != 0)
				{
					readerStream.Seek(36, SeekOrigin.Begin);
					headerSize = (uint)38 + readerStream.Read_B_UINT16();
				}
				else
					headerSize = 36;

				if (decrunchedSize + 8 > readerStream.Length)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
			}
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int taken = 0;

			while ((count > 0) && !isLast)
			{
				if (rawData == null)
				{
					using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
					{
						void ReadDualValue(uint offsetShort, uint offsetLong, out uint value)
						{
							if (longHeaders)
							{
								readerStream.Seek(currentOffset + offsetLong, SeekOrigin.Begin);
								value = readerStream.Read_B_UINT32();
							}
							else
							{
								readerStream.Seek(currentOffset + offsetShort, SeekOrigin.Begin);
								value = readerStream.Read_B_UINT16();
							}
						}

						bool HeaderChecksum(byte[] buf, uint offs, uint len)
						{
							if ((len == 0) || (offs + len > buf.Length))
								return false;

							byte tmp = 0;
							for (uint i = 0; i < len; i++)
								tmp ^= buf[offs + i];

							return tmp == 0;
						}

						bool ChunkChecksum(byte[] buf, uint offs, uint len, ushort checkValue)
						{
							if ((len == 0) || (offs + len > buf.Length))
								return false;

							byte[] tmp = { 0, 0 };
							for (uint i = 0; i < len; i++)
								tmp[i & 1] ^= buf[offs + i];

							return (tmp[0] == (checkValue >> 8)) && (tmp[1] == (checkValue & 0xff));
						}

						uint checkHeaderLen = longHeaders ? (uint)12 : 8;

						if (currentOffset == 0)
						{
							// Return first
							currentOffset = headerSize;
						}
						else
						{
							ReadDualValue(4, 4, out uint tmp);
							tmp = (uint)((tmp + 3) & ~3);

							if (tmp + currentOffset + checkHeaderLen > decrunchedSize)
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							currentOffset += checkHeaderLen + tmp;
						}

						ReadDualValue(4, 4, out uint decrunchedChunkSize);
						ReadDualValue(6, 8, out uint rawChunkSize);

						byte[] hdr = new byte[checkHeaderLen];
						readerStream.Seek(currentOffset, SeekOrigin.Begin);
						readerStream.Read(hdr, 0, (int)checkHeaderLen);

						if (!HeaderChecksum(hdr, 0, (uint)hdr.Length))
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_WRONG_HEADER_CHECKSUM);

						byte[] chunk = new byte[decrunchedChunkSize];
						readerStream.Seek(currentOffset + checkHeaderLen, SeekOrigin.Begin);
						readerStream.Read(chunk, 0, (int)decrunchedChunkSize);

						ushort hdrCheck = (ushort)((hdr[2] << 8) | hdr[3]);
						if ((chunk.Length != 0) && !ChunkChecksum(chunk, 0, (uint)chunk.Length, hdrCheck))
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_WRONG_CHUNK_CHECKSUM);

						byte type = hdr[0];
						Decompress(chunk, rawChunkSize, type);
						bufferIndex = 0;

						if (type == 15)
							isLast = true;
					}
				}

				if (!isLast)
				{
					// Copy decrunched data into the read buffer
					int todo = Math.Min(count, rawData.Length - bufferIndex);
					Array.Copy(rawData, bufferIndex, buffer, offset, todo);

					count -= todo;
					taken += todo;
					bufferIndex += todo;
					offset += todo;

					if (bufferIndex == rawData.Length)
						rawData = null;
				}
			}

			return taken;
		}
		#endregion

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		protected override int GetDecrunchedLength()
		{
			return (int)rawSize;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected abstract void DecompressImpl(byte[] chunk, byte[] rawData);

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		private void Decompress(byte[] chunk, uint rawChunkSize, byte chunkType)
		{
			if (destOffset + rawChunkSize > rawSize)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			if (rawChunkSize == 0)
				return;

			switch (chunkType)
			{
				// Raw chunk (not crunched)
				case 0:
				{
					if (rawChunkSize != chunk.Length)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					rawData = chunk;
					break;
				}

				// Crunched
				case 1:
				{
					rawData = new byte[rawChunkSize];
					DecompressImpl(chunk, rawData);
					break;
				}

				// Last chunk
				case 15:
					break;

				default:
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_UNKNOWN_CHUNK_TYPE);
			}

			destOffset += rawChunkSize;
		}
		#endregion
	}
}

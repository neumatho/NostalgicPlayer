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

namespace Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats.Streams
{
	/// <summary>
	/// This stream read data packed with XPK-SQSH
	/// </summary>
	internal class Xpk_SqshStream : DepackerStream
	{
		private const int SafetySize = 1024;

		private readonly string agentName;

		private readonly int depackedLength;

		private byte[] depackedData;
		private int bufferIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_SqshStream(string agentName, Stream wrapperStream) : base(wrapperStream)
		{
			this.agentName = agentName;

			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				readerStream.Seek(12, SeekOrigin.Begin);
				depackedLength = (int)readerStream.Read_B_UINT32();
			}

			wrapperStream.Seek(36, SeekOrigin.Begin);
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

			while (count > 0)
			{
				if (depackedData == null)
				{
					byte[] header = new byte[8];
					int bytesRead = wrapperStream.Read(header, 0, 8);
					if (bytesRead == 0)
						return taken;

					if (bytesRead != 8)
						throw new DepackerException(agentName, Resources.IDS_FILEDECR_ERR_CORRUPT_DATA);

					byte type = header[0];				// Type of chunk
					byte hchk = header[1];				// Chunk header checksum

					// Chunk data checksum
					ushort chk = (ushort)(header[2] << 8);
					chk |= header[3];

					// Packed length
					ushort cp = (ushort)(header[4] << 8);
					cp |= header[5];

					// Unpacked length
					ushort cu = (ushort)(header[6] << 8);
					cu |= header[7];

					// Check header checksum
					hchk ^= type;
					hchk ^= (byte)(chk ^ (chk >> 8));
					hchk ^= (byte)(cp ^ (cp >> 8));
					hchk ^= (byte)(cu ^ (cu >> 8));

					if (hchk != 0)
						throw new DepackerException(agentName, Resources.IDS_FILEDECR_ERR_WRONG_HEADER_CHECKSUM);

					// Make packed length size long aligned
					cp = (ushort)((cp + 3) & 0xfffc);

					// Allocate buffer to hold the packed data and read the block
					byte[] packedData = new byte[cp + SafetySize];
					bytesRead = wrapperStream.Read(packedData, 0, cp);
					if (bytesRead != cp)
						throw new DepackerException(agentName, Resources.IDS_FILEDECR_ERR_CORRUPT_DATA);

					// Check chunk data checksum
					uint l = 0;
					for (int lp = cp; lp != 0; )
					{
						lp -= 4;
						l ^= (uint)((packedData[lp] << 24) | (packedData[lp + 1] << 16) | (packedData[lp + 2] << 8) | packedData[lp + 3]);
					}

					chk ^= (ushort)(l & 0xffff);
					chk ^= (ushort)(l >> 16);

					if (chk != 0)
						throw new DepackerException(agentName, Resources.IDS_FILEDECR_ERR_WRONG_CHUNK_DATA_CHECKSUM);

					// Allocate buffer for the depacked data
					depackedData = new byte[cu + SafetySize];
					bufferIndex = 0;

					// Check the type
					switch (type)
					{
						// Raw chunk (unpacked)
						case 0:
						{
							Array.Copy(packedData, 0, depackedData, 0, cu);
							break;
						}

						// Packed
						case 1:
						{
							UnSqsh(packedData, 0, depackedData, 0);
							break;
						}

						// Unknown
						default:
							throw new DepackerException(agentName, Resources.IDS_FILEDECR_ERR_UNKNOWN_CHUNK_TYPE);
					}
				}

				// Copy depacked data into the read buffer
				int todo = Math.Min(count, depackedData.Length - SafetySize - bufferIndex);
				Array.Copy(depackedData, bufferIndex, buffer, offset, todo);

				count -= todo;
				taken += todo;
				bufferIndex += todo;

				if (bufferIndex == (depackedData.Length - SafetySize))
					depackedData = null;
			}

			return taken;
		}
		#endregion

		#region DepackerStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		/********************************************************************/
		protected override int GetDepackedLength()
		{
			return depackedLength;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will unpack a single chunk
		/// </summary>
		/********************************************************************/
		private void UnSqsh(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
		{
			int a4, a6;
			int d0, d1, d2, d3, d4, d5, d6, a2, a5;
			byte[] a3 =
				{
					2, 3, 4, 5, 6, 7, 8, 0, 3, 2, 4, 5, 6, 7, 8, 0, 4, 3, 5, 2, 6, 7, 8, 0, 5, 4,
					6, 2, 3, 7, 8, 0, 6, 5, 7, 2, 3, 4, 8, 0, 7, 6, 8, 2, 3, 4, 5, 0, 8, 7, 6, 2, 3, 4, 5, 0
				};

			a6 = destinationIndex;
			a6 += source[sourceIndex++] << 8;
			a6 += source[sourceIndex++];
			d0 = d1 = d2 = d3 = a2 = 0;

			d3 = source[sourceIndex++];
			destination[destinationIndex++] = (byte)d3;

			l6c6:
			if (d1 >= 8)
				goto l6dc;

			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l75a;

			d0 += 1;
			d5 = 0;
			d6 = 8;
			goto l734;

			l6dc:
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l726;

			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) == 0)
				goto l75a;

			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l6f6;

			d6 = 2;
			goto l708;

			l6f6:
			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) == 0)
				goto l706;

			d6 = Bfextu(source, sourceIndex, d0, 3);
			d0 += 3;
			goto l70a;

			l706:
			d6 = 3;
			l708:
			d0 += 1;
			l70a:
			d6 = a3[(8 * a2) + d6 - 17];
			if (d6 != 8)
				goto l730;

			l718:
			if (d2 < 20)
				goto l722;

			d5 = 1;
			goto l732;

			l722:
			d5 = 0;
			goto l734;

			l726:
			d0 += 1;
			d6 = 8;

			if (d6 == a2)
				goto l718;

			d6 = a2;

			l730:
			d5 = 4;
			l732:
			d2 += 8;
			l734:
			d4 = Bfexts(source, sourceIndex, d0, d6);

			d0 += d6;
			d3 -= d4;
			destination[destinationIndex++] = (byte)d3;

			d5--;
			if (d5 != -1)
				goto l734;

			if (d1 == 31)
				goto l74a;

			d1 += 1;

			l74a:
			a2 = d6;
			l74c:
			d6 = d2;
			d6 >>= 3;
			d2 -= d6;

			if (destinationIndex < a6)
				goto l6c6;

			destinationIndex = a6;
			return;

			l75a:
			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l766;

			d4 = 2;
			goto l79e;

			l766:
			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l772;

			d4 = 4;
			goto l79e;

			l772:
			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l77e;

			d4 = 6;
			goto l79e;

			l77e:
			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l792;

			d0 += 1;
			d6 = Bfextu(source, sourceIndex, d0, 3);
			d0 += 3;
			d6 += 8;
			goto l7a8;

			l792:
			d0 += 1;
			d6 = Bfextu(source, sourceIndex, d0, 5);
			d0 += 5;
			d4 = 16;
			goto l7a6;

			l79e:
			d0 += 1;
			d6 = Bfextu(source, sourceIndex, d0, 1);
			d0 += 1;

			l7a6:
			d6 += d4;
			l7a8:
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l7c4;

			d0 += 1;
			if (Bfextu(source, sourceIndex, d0, 1) != 0)
				goto l7bc;

			d5 = 8;
			a5 = 0;
			goto l7ca;

			l7bc:
			d5 = 14;
			a5 = -0x1100;
			goto l7ca;

			l7c4:
			d5 = 12;
			a5 = -0x100;

			l7ca:
			d0 += 1;
			d4 = Bfextu(source, sourceIndex, d0, d5);
			d0 += d5;
			d6 -= 3;

			if (d6 < 0)
				goto l7e0;

			if (d6 == 0)
				goto l7da;

			d1 -= 1;

			l7da:
			d1 -= 1;
			if (d1 >= 0)
				goto l7e0;

			d1 = 0;

			l7e0:
			d6 += 2;
			a4 = -1 + destinationIndex + a5 - d4;

			l7ex:
			destination[destinationIndex++] = destination[a4++];
			d6--;
			if (d6 != -1)
				goto l7ex;

			d3 = destination[--a4];
			goto l74c;
		}



		/********************************************************************/
		/// <summary>
		/// Emulate the 68020 bfextu command
		/// </summary>
		/********************************************************************/
		private int Bfextu(byte[] source, int sourceIndex, int bo, int bc)
		{
			sourceIndex += bo / 8;

			int r = source[sourceIndex++];
			r <<= 8;
			r |= source[sourceIndex++];
			r <<= 8;
			r |= source[sourceIndex];
			r <<= bo % 8;
			r &= 0xffffff;
			r >>= 24 - bc;

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Emulate the 68020 bfexts command
		/// </summary>
		/********************************************************************/
		private int Bfexts(byte[] source, int sourceIndex, int bo, int bc)
		{
			sourceIndex += bo / 8;

			int r = source[sourceIndex++];
			r <<= 8;
			r |= source[sourceIndex++];
			r <<= 8;
			r |= source[sourceIndex];
			r <<= (bo % 8) + 8;
			r >>= 32 - bc;

			return r;
		}
		#endregion
	}
}

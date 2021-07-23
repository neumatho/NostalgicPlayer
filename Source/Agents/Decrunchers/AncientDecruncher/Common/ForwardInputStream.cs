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

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Read from a buffer forward
	/// </summary>
	internal class ForwardInputStream : IInputStream
	{
		private readonly string agentName;
		private readonly Stream wrapperStream;

		private uint currentOffset;
		private readonly uint endOffset;
		private readonly bool allowOverrun;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ForwardInputStream(string agentName, Stream inputStream, uint startOffset, uint endOffset, bool allowOverrun = false)
		{
			this.agentName = agentName;
			wrapperStream = inputStream;

			currentOffset = startOffset;
			this.endOffset = endOffset;
			this.allowOverrun = allowOverrun;

			if ((currentOffset > this.endOffset) || (currentOffset > wrapperStream.Length) || (this.endOffset > wrapperStream.Length))
				throw new ArgumentException();
		}

		#region IInputStream implementation
		/********************************************************************/
		/// <summary>
		/// Read a single byte
		/// </summary>
		/********************************************************************/
		public byte ReadByte()
		{
			if (currentOffset >= endOffset)
			{
				if (allowOverrun)
				{
					currentOffset++;
					return 0;
				}

				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_OUT_OF_DATA);
			}

			wrapperStream.Seek(currentOffset, SeekOrigin.Begin);
			currentOffset++;

			byte[] ret = new byte[1];
			wrapperStream.Read(ret, 0, 1);

			return ret[0];
		}



		/********************************************************************/
		/// <summary>
		/// Consume the given number of bytes
		/// </summary>
		/********************************************************************/
		public byte[] Consume(uint bytes, byte[] buffer)
		{
			if (currentOffset + bytes > endOffset)
			{
				if (allowOverrun && (buffer != null))
				{
					wrapperStream.Seek(currentOffset, SeekOrigin.Begin);

					for (uint i = 0; i < bytes; i++)
					{
						if (currentOffset < endOffset)
							wrapperStream.Read(buffer, (int)i, 1);
						else
							buffer[i] = 0;

						currentOffset++;
					}

					return buffer;
				}

				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_OUT_OF_DATA);
			}

			byte[] ret = new byte[bytes];
			wrapperStream.Seek(currentOffset, SeekOrigin.Begin);
			wrapperStream.Read(ret, 0, (int)bytes);

			currentOffset += bytes;

			return ret;
		}
		#endregion
	}
}

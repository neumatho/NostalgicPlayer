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
	/// Read from a buffer backwards
	/// </summary>
	internal class BackwardInputStream : IInputStream
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
		public BackwardInputStream(string agentName, Stream inputStream, uint startOffset, uint endOffset, bool allowOverrun = false)
		{
			this.agentName = agentName;
			wrapperStream = inputStream;

			currentOffset = endOffset;
			this.endOffset = startOffset;
			this.allowOverrun = allowOverrun;

			if ((currentOffset < this.endOffset) || (currentOffset > wrapperStream.Length) || (this.endOffset > wrapperStream.Length))
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
			if (currentOffset <= endOffset)
			{
				if (allowOverrun)
				{
					--currentOffset;
					return 0;
				}

				throw new DepackerException(agentName, Resources.IDS_ANC_ERR_OUT_OF_DATA);
			}

			--currentOffset;
			wrapperStream.Seek(currentOffset, SeekOrigin.Begin);

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
			if (currentOffset < endOffset + bytes)
			{
				if (allowOverrun && (buffer != null))
				{
					for (uint i = bytes; i != 0; i--)
					{
						if (currentOffset > endOffset)
						{
							wrapperStream.Seek(currentOffset - 1, SeekOrigin.Begin);
							wrapperStream.Read(buffer, (int)i - 1, 1);
						}
						else
							buffer[i - 1] = 0;

						--currentOffset;
					}

					return buffer;
				}

				throw new DepackerException(agentName, Resources.IDS_ANC_ERR_OUT_OF_DATA);
			}

			currentOffset -= bytes;

			byte[] ret = new byte[bytes];
			wrapperStream.Seek(currentOffset, SeekOrigin.Begin);
			wrapperStream.Read(ret, 0, (int)bytes);

			return ret;
		}
		#endregion
	}
}

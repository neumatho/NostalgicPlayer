/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Write to a buffer forward
	/// </summary>
	internal class ForwardOutputStream : IOutputStream
	{
		private readonly string agentName;
		private readonly byte[] buf;

		private readonly uint startOffset;
		private uint currentOffset;
		private readonly uint endOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ForwardOutputStream(string agentName, byte[] buffer, uint startOffset, uint endOffset)
		{
			this.agentName = agentName;
			buf = buffer;

			this.startOffset = startOffset;
			currentOffset = startOffset;
			this.endOffset = endOffset;

			if ((this.startOffset > this.endOffset) || (currentOffset > buffer.Length) || (this.endOffset > buffer.Length))
				throw new ArgumentException();
		}

		#region IOutputStream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		/********************************************************************/
		public bool Eof => currentOffset == endOffset;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public uint GetOffset()
		{
			return currentOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single byte to the output
		/// </summary>
		/********************************************************************/
		public void WriteByte(uint value)
		{
			if (currentOffset >= endOffset)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			buf[currentOffset++] = (byte)value;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public byte Copy(uint distance, uint count)
		{
			if ((distance == 0) || (startOffset + distance > currentOffset) || (currentOffset + count > endOffset))
				throw new ArgumentException();

			byte ret = 0;

			for (uint i = 0; i < count; i++, currentOffset++)
				ret = buf[currentOffset] = buf[currentOffset - distance];

			return ret;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public byte Copy(uint distance, uint count, List<byte[]> prevBuffer)
		{
			if ((distance == 0) || (OverflowCheck.Sum(currentOffset, count) > endOffset))
				throw new ArgumentException();

			uint prevCount = 0;
			byte ret = 0;

			if (OverflowCheck.Sum(startOffset, distance) > currentOffset)
			{
				uint prevSize = (uint)prevBuffer.Sum(b => b.Length);
				if ((startOffset + distance) > (currentOffset + prevSize))
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				uint prevDist = startOffset + distance - currentOffset;
				prevCount = Math.Min(count, prevDist);

				byte[] prev = null;
				int len = 0;

				int bufIndex;
				for (bufIndex = 0; bufIndex < prevBuffer.Count; bufIndex++)
				{
					byte[] b = prevBuffer[bufIndex];
					len += b.Length;

					if ((prevSize - prevDist) < len)
					{
						prev = b;
						break;
					}
				}

				if (prev == null)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				uint total = prevCount;
				uint index = (uint)(prevSize - prevDist - (len - prev.Length));

				for (;;)
				{
					uint todo = Math.Min(total, (uint)(prev.Length - index));

					for (uint i = 0; i < todo; i++, currentOffset++)
						ret = buf[currentOffset] = prev[index + i];

					total -= todo;
					if (total == 0)
						break;

					if (bufIndex == prevBuffer.Count)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					prev = prevBuffer[++bufIndex];
					index = 0;
				}
			}

			for (uint i = prevCount; i < count; i++, currentOffset++)
				ret = buf[currentOffset] = buf[currentOffset - distance];

			return ret;
		}
	}
}

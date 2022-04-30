/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Write to a buffer backwards
	/// </summary>
	internal class BackwardOutputStream : IOutputStream
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
		public BackwardOutputStream(string agentName, byte[] buffer, uint startOffset, uint endOffset)
		{
			this.agentName = agentName;
			buf = buffer;

			this.startOffset = startOffset;
			currentOffset = endOffset;
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
		public bool Eof => currentOffset == startOffset;



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
			if (currentOffset <= startOffset)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			buf[--currentOffset] = (byte)value;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		/********************************************************************/
		public byte Copy(uint distance, uint count)
		{
			if ((distance == 0) || (OverflowCheck.Sum(startOffset, count) > currentOffset) || (OverflowCheck.Sum(currentOffset, distance) > endOffset))
				throw new ArgumentException();

			byte ret = 0;

			for (uint i = 0; i < count; i++, --currentOffset)
				ret = buf[currentOffset - 1] = buf[currentOffset + distance - 1];

			return ret;
		}
		#endregion
	}
}

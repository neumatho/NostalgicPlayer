/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod
{
	/// <summary>
	/// Helper class to read and write UniMod files
	/// </summary>
	public class MUniTrk
	{
		private byte[] track;						// The whole track
		private int rowStart;						// Offset to the row
		private int rowEnd;							// End offset of a row (exclusive)
		private int rowPc;							// Current unimod(tm) program counter

		private byte lastByte;						// For UniSkipOpcode()

		/********************************************************************/
		/// <summary>
		/// Finds the address of the row number 'row' in the UniMod(tm)
		/// stream 't'. Returns -1 if the row can't be found
		/// </summary>
		/********************************************************************/
		public int UniFindRow(byte[] t, ushort row)
		{
			int offset = -1;

			if (t != null)
			{
				offset = 0;

				while (true)
				{
					byte c = t[offset];				// Get rep/len byte
					if (c == 0)
						return -1;					// Zero? -> end of track..

					byte l = (byte)((c >> 5) + 1);	// Extract repeat value
					if (l > row)
						break;						// Reached wanted row? -> return pointer

					row -= l;						// Haven't reached row yet.. update row
					offset += c & 0x1f;				// Point to next row
				}
			}

			return offset;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the internal variables to point to the specific row
		/// </summary>
		/********************************************************************/
		public void UniSetRow(byte[] t, int offset)
		{
			track = t;
			rowStart = offset;
			rowPc = rowStart;
			rowEnd = offset != -1 ? rowStart + (track[rowPc++] & 0x1f) : offset;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the next byte in the stream
		/// </summary>
		/********************************************************************/
		public byte UniGetByte()
		{
			return (lastByte = (rowPc < rowEnd) ? track[rowPc++] : (byte)0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the next word in the stream
		/// </summary>
		/********************************************************************/
		public ushort UniGetWord()
		{
			return (ushort)((UniGetByte() << 8) | UniGetByte());
		}



		/********************************************************************/
		/// <summary>
		/// Skips all the bytes to the opcode
		/// </summary>
		/********************************************************************/
		public void UniSkipOpcode()
		{
			if (lastByte < (byte)Command.UniLast)
			{
				ushort t = SharedLookupTables.UniOperands[lastByte];

				while (t-- != 0)
					UniGetByte();
			}
		}
	}
}

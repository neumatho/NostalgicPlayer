/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod
{
	/// <summary>
	/// Helper class to read and write UniMod files
	///
	/// Sparse description of the internal module format
	/// ------------------------------------------------
	///
	/// A UNITRK stream is an array of bytes representing a single track of a pattern.
	/// It's made up of 'repeat/length' bytes, opcodes and operands (sort of a assembly
	/// language):
	///
	/// rrrlllll
	/// [REP/LEN][OPCODE][OPERAND][OPCODE][OPERAND] [REP/LEN][OPCODE][OPERAND]..
	/// ^                                         ^ ^
	/// |-------ROWS 0 - 0+REP of a track---------| |-------ROWS xx - xx+REP of a track...
	///
	/// The rep/len byte contains the number of bytes in the current row, _including_
	/// the length byte itself (So the LENGTH byte of row 0 in the previous example
	/// would have a value of 5). This makes it easy to search through a stream for a
	/// particular row. A track is concluded by a 0-value length byte.
	///
	/// The upper 3 bits of the rep/len byte contain the number of times -1 this row
	/// is repeated for this track. (so a value of 7 means this row is repeated 8 times)
	///
	/// Opcodes can range from 1 to 255 but currently only opcodes 1 to 62 are being
	/// used. Each opcode can have a different number of operands. You can find the
	/// number of operands to a particular opcode by using the opcode as an index into
	/// the 'unioperands' table.
	/// </summary>
	public class MUniTrk
	{
		private const int BufPage = 128;			// Smallest uni buffer size

		private byte[] track;						// The whole track
		private int rowStart;						// Offset to the row
		private int rowEnd;							// End offset of a row (exclusive)
		private int rowPc;							// Current UniMod(tm) program counter

		private byte lastByte;						// For UniSkipOpcode()

		private byte[] uniBuf;						// Pointer to the temporary UniTrk buffer
		private ushort uniMax;						// Maximum number of bytes to be written to this buffer

		private ushort uniPc;						// Index in the buffer where next opcode will be written
		private ushort uniTt;						// Holds index of the rep/len byte of a row
		private ushort lastP;						// Holds index to the previous row (needed for compressing)

		/********************************************************************/
		/// <summary>
		/// Finds the address of the row number 'row' in the UniMod(tm)
		/// stream 't'. Returns -1 if the row can't be found
		/// </summary>
		/********************************************************************/
		public int UniFindRow(byte[] t, ushort row)
		{
			int offset = -1;

			if ((t != null) && (t.Length > 0))
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



		/********************************************************************/
		/// <summary>
		/// Initialize the uni stream
		/// </summary>
		/********************************************************************/
		public bool UniInit()
		{
			uniMax = BufPage;

			uniBuf = new byte[uniMax];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Frees the unitrk stream
		/// </summary>
		/********************************************************************/
		public void UniCleanup()
		{
			uniBuf = null;
		}



		/********************************************************************/
		/// <summary>
		/// Resets the index-pointers to create a new track
		/// </summary>
		/********************************************************************/
		public void UniReset()
		{
			uniTt = 0;			// Reset index to rep/len byte
			uniPc = 1;			// First opcode will be written to index 1
			lastP = 0;			// No previous row yet
			uniBuf[0] = 0;		// Clear rep/len byte
		}



		/********************************************************************/
		/// <summary>
		/// Appends one byte of data to the current row of a track
		/// </summary>
		/********************************************************************/
		public void UniWriteByte(byte data)
		{
			if (UniExpand(1))
			{
				// Write byte to current position and update
				uniBuf[uniPc++] = data;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Appends a word to the current row of a track
		/// </summary>
		/********************************************************************/
		public void UniWriteWord(ushort data)
		{
			if (UniExpand(2))
			{
				// Write byte to current position and update
				uniBuf[uniPc++] = (byte)(data >> 8);
				uniBuf[uniPc++] = (byte)(data & 0xff);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Closes the current row of a unitrk stream (updates the rep/len
		/// byte) and sets pointers to start of new row
		/// </summary>
		/********************************************************************/
		public void UniNewLine()
		{
			ushort n = (ushort)((uniBuf[lastP] >> 5) + 1);			// Repeat of previous row
			ushort l = (ushort)(uniBuf[lastP] & 0x1f);				// Length of previous row

			ushort len = (ushort)(uniPc - uniTt);

			// Now, check if the previous and the current row are identical..
			// When they are, just increase the repeat field of the previous row
			if ((n < 8) && (len == l) && Helpers.ArrayCompare(uniBuf, lastP + 1, uniBuf, uniTt + 1, len - 1))
			{
				uniBuf[lastP] += 0x20;
				uniPc = (ushort)(uniTt + 1);
			}
			else
			{
				if (UniExpand(len))
				{
					// Current and previous row aren't equal... update the pointers
					uniBuf[uniTt] = (byte)len;
					lastP = uniTt;
					uniTt = uniPc++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Determines the length (in rows) of a unitrk stream
		/// </summary>
		/********************************************************************/
		public ushort UniTrkLen(byte[] t)
		{
			ushort len = 0;
			int offset = 0;
			byte c;

			while ((c = (byte)(t[offset] & 0x1f)) != 0)
			{
				len += c;
				offset += c;
			}

			len++;

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// Terminates the current unitrk stream and return a pointer to a
		/// copy of the stream
		/// </summary>
		/********************************************************************/
		public byte[] UniDup()
		{
			if (!UniExpand(uniPc - uniTt))
				return null;

			uniBuf[uniTt] = 0;

			byte[] d = new byte[uniPc];

			Array.Copy(uniBuf, d, uniPc);

			return d;
		}



		/********************************************************************/
		/// <summary>
		/// Appends an effect opcode to the unitrk stream
		/// </summary>
		/********************************************************************/
		public void UniEffect(Command eff, ushort dat)
		{
			if ((eff == 0) || (eff >= Command.UniLast))
				return;

			UniWriteByte((byte)eff);

			if (SharedLookupTables.UniOperands[(byte)eff] == 2)
				UniWriteWord(dat);
			else
				UniWriteByte((byte)dat);
		}



		/********************************************************************/
		/// <summary>
		/// Appends UNI_PTEFFECTX opcode to the unitrk stream
		/// </summary>
		/********************************************************************/
		public void UniPtEffect(byte eff, byte dat, ModuleFlag flags)
		{
			if ((eff != 0) || (dat != 0) || ((flags & ModuleFlag.ArpMem) != 0))
				UniEffect(Command.UniPtEffect0 + eff, dat);
		}



		/********************************************************************/
		/// <summary>
		/// Appends new note to the unitrk stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UniNote(ushort note)
		{
			UniEffect(Command.UniNote, note);
		}



		/********************************************************************/
		/// <summary>
		/// Appends new instrument to the unitrk stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UniInstrument(ushort ins)
		{
			UniEffect(Command.UniInstrument, ins);
		}



		/********************************************************************/
		/// <summary>
		/// Appends UNI_VOLEFFECT + effect/dat to unistream
		/// </summary>
		/********************************************************************/
		public void UniVolEffect(VolEffect eff, byte dat)
		{
			// Don't write empty effect
			if ((eff != VolEffect.None) || (dat != 0))
			{
				UniWriteByte((byte)Command.UniVolEffects);
				UniWriteByte((byte)eff);
				UniWriteByte(dat);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Expands the buffer with the number of bytes wanted
		/// </summary>
		/********************************************************************/
		private bool UniExpand(int wanted)
		{
			if ((uniPc + wanted) >= uniMax)
			{
				// Expand the buffer by BUFPAGE bytes
				byte[] newBuf = new byte[uniMax + BufPage];

				// Copy the old data to the new block
				Array.Copy(uniBuf, newBuf, uniMax);

				uniBuf = newBuf;
				uniMax += BufPage;
			}

			return true;
		}
		#endregion
	}
}

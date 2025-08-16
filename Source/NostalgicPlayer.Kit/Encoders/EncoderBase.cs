/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.Encoders
{
	/// <summary>
	/// Base class for all encoders
	/// </summary>
	public abstract class EncoderBase : Encoding
	{
		/********************************************************************/
		/// <summary>
		/// Single byte encoding
		/// </summary>
		/********************************************************************/
		public override bool IsSingleByte => true;



		/********************************************************************/
		/// <summary>
		/// Return the number of bytes needed to encode the given characters
		/// </summary>
		/********************************************************************/
		public override int GetByteCount(char[] chars, int index, int count)
		{
			ValidateArguments(chars, index, count);

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Encode the given characters into the given buffer
		/// </summary>
		/********************************************************************/
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			ValidateArguments(chars, charIndex, charCount);
			ValidateArguments(bytes, byteIndex, 0);

			if ((charCount + byteIndex) < bytes.Length)
				throw new ArgumentException("Resulting byte array is too short");

			byte[][] highByteIndexTable = GetHighByteIndexTable();

			for (; charIndex < charCount; charIndex++, byteIndex++)
				bytes[byteIndex] = GetByteFromMultiTable(chars[charIndex], highByteIndexTable);

			return charCount;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of characters needed to decode the given bytes
		/// </summary>
		/********************************************************************/
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			ValidateArguments(bytes, index, count);

			int needed = 0;
			while ((count > 0) && (bytes[index] != 0x00))
			{
				index++;
				count--;
				needed++;
			}

			return needed;
		}



		/********************************************************************/
		/// <summary>
		/// Decode the given bytes into the given buffer
		/// </summary>
		/********************************************************************/
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			ValidateArguments(bytes, byteIndex, byteCount);
			ValidateArguments(chars, charIndex, 0);

			ushort[] lookupTable = GetLookupTable();

			int taken = 0;
			for (; taken < byteCount; byteIndex++, charIndex++, taken++)
			{
				// Stop with null terminator
				if (bytes[byteIndex] == 0x00)
					break;

				chars[charIndex] = (char)lookupTable[bytes[byteIndex]];
			}

			return taken;
		}



		/********************************************************************/
		/// <summary>
		/// Return the max number of bytes needed
		/// </summary>
		/********************************************************************/
		public override int GetMaxByteCount(int charCount)
		{
			return charCount;
		}



		/********************************************************************/
		/// <summary>
		/// Return the max number of characters needed
		/// </summary>
		/********************************************************************/
		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}



		/********************************************************************/
		/// <summary>
		/// Validates the arguments
		/// </summary>
		/********************************************************************/
		protected void ValidateArguments(Array array, int index, int count)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (index < 0)
				throw new ArgumentException("Negative index", nameof(index));

			if (count < 0)
				throw new ArgumentException("Negative index", nameof(count));

			if ((index + count) > array.Length)
				throw new ArgumentException("Index + count bigger than character array");
		}



		/********************************************************************/
		/// <summary>
		/// Lookup in the given high byte table to find the lower byte table
		/// to use. Then look up into this one to find the byte to use
		/// </summary>
		/********************************************************************/
		private byte GetByteFromMultiTable(char chr, byte[][] indexTable)
		{
			// Find the lower byte table
			byte[] lowerTable = indexTable[chr >> 8];
			if (lowerTable == null)
				return 0x3f;			// Return '?' as an unknown character

			return lowerTable[chr & 0xff];
		}



		/********************************************************************/
		/// <summary>
		/// Return the lookup table
		/// </summary>
		/********************************************************************/
		protected abstract ushort[] GetLookupTable();



		/********************************************************************/
		/// <summary>
		/// Return the high byte index table
		/// </summary>
		/********************************************************************/
		protected abstract byte[][] GetHighByteIndexTable();
	}
}

/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	/// 
	/// </summary>
	internal static class MptStringBuffer
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static StdString ReadStringBuffer(ReadWriteMode mode, CPointer<uint8> srcBuffer, size_t srcSize)
		{
			StdString dest = new StdString();
			CPointer<uint8> src = srcBuffer;

			if ((mode == ReadWriteMode.NullTerminated) || (mode == ReadWriteMode.SpacePaddedNull))
			{
				// We assume that the last character of the source buffer is null
				if (srcSize > 0)
					srcSize--;
			}

			if ((mode == ReadWriteMode.NullTerminated) || (mode == ReadWriteMode.MaybeNullTerminated))
			{
				// Copy null-terminated string, stopping at null
				dest.assign(src, Algorithm.find<CPointer<uint8>, uint8>(src, src + srcSize, 0x00));
			}
			else if ((mode == ReadWriteMode.SpacePadded) || (mode == ReadWriteMode.SpacePaddedNull))
			{
				// Copy string over
				dest.assign(src, src + srcSize);

				// Convert null characters to space
				Algorithm.transform(dest.begin(), dest.end(), dest.begin(), (uint8 c) => c != 0x00 ? c : (uint8)' ');

				// Trim trailing spaces
				dest = MptString.Trim_Right(dest, new StdString([ (uint8)' ' ]));
			}

			return dest;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static void WriteStringBuffer(ReadWriteMode mode, CPointer<uint8> destBuffer, size_t destSize, CPointer<uint8> srcBuffer, size_t srcSize)
		{
			size_t maxSize = Math.Min(destSize, srcSize);
			CPointer<uint8> dst = destBuffer;
			CPointer<uint8> src = srcBuffer;

			// First, copy over null-terminated string
			size_t pos = maxSize;

			while (pos > 0)
			{
				if ((dst[0] = src[0]) == 0x00)
					break;

				pos--;
				dst++;
				src++;
			}

			if ((mode == ReadWriteMode.NullTerminated) || (mode == ReadWriteMode.MaybeNullTerminated))
			{
				// Fill rest of string with nulls
				Algorithm.fill<CPointer<uint8>, uint8>(dst, dst + destSize - maxSize + pos, 0x00);
			}
			else if ((mode == ReadWriteMode.SpacePadded) || (mode == ReadWriteMode.SpacePaddedNull))
			{
				// Fill the rest of the destination string with spaces
				Algorithm.fill<CPointer<uint8>, uint8>(dst, dst + destSize - maxSize + pos, (uint8)' ');
			}

			if ((mode == ReadWriteMode.NullTerminated) || (mode == ReadWriteMode.SpacePaddedNull))
			{
				// Make sure that destination is really null-terminated
				MptString.SetNullTerminator(destBuffer, destSize);
			}
		}
	}
}

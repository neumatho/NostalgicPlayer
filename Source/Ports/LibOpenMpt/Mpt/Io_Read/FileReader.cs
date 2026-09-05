/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Utility = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal static class FileReader
	{
		/********************************************************************/
		/// <summary>
		/// Read a "T" object from the stream.
		/// If not enough bytes can be read, false is returned.
		/// If successful, the file cursor is advanced by the size of "T"
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Read<T>(FileCursor f, ref T target) where T : unmanaged//XX 238
		{
			byte_span2 dst = Memory.As_Raw_Memory(ref target);

			if (dst.Size() != f.GetRaw(dst).Size())
				return false;

			f.Skip(dst.Size());

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, T[] destArray) where T : unmanaged//XX 253
		{
			if (!f.CanRead((size_t)(Marshal.SizeOf<T>() * destArray.Length)))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, array<T> destArray) where T : unmanaged//XX 267
		{
			if (!f.CanRead((size_t)Marshal.SizeOf<T>() * destArray.size()))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, CPointer<T> destArray) where T : unmanaged
		{
			if (!f.CanRead((size_t)(Marshal.SizeOf<T>() * destArray.Length)))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static array<T> ReadArray<T>(FileCursor f, size_t destSize) where T : unmanaged//XX 291
		{
			array<T> destArray = new array<T>(destSize);

			ReadArray<T>(f, destArray);

			return destArray;
		}



		/********************************************************************/
		/// <summary>
		/// Read unsigned 32-Bit integer in little-endian format.
		/// If successful, the file cursor is advanced by the size of the
		/// integer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32 ReadUInt32LE(FileCursor f)//XX 374
		{
			uint32le target = new uint32le();

			if (!Read(f, ref target))
				return 0;

			return target;
		}



		/********************************************************************/
		/// <summary>
		/// Compare a magic string with the current stream position.
		/// Returns true if they are identical and advances the file cursor
		/// by the length of the "magic" string.
		/// Returns false if the string could not be found. The file cursor
		/// is not advanced in this case
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadMagic(FileCursor f, string magic)//XX 601
		{
			size_t magicLength = (size_t)magic.Length;
			byte[] buffer = new byte[magicLength];

			if (f.GetRaw(buffer).Size() != magicLength)
				return false;

			if (CMemory.memcmp(buffer, magic, magicLength) != 0)
				return false;

			f.Skip(magicLength);

			return true;
		}
	}
}
